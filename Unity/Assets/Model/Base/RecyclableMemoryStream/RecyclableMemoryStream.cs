// The MIT License (MIT)
// 
// Copyright (c) 2015-2016 Microsoft
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

namespace Microsoft.IO
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Threading;

    // <summary>
    /// MemoryStream实现，用于处理池和管理使用可能很大的内存流
        ///缓冲区。
        /// </ summary>
        /// <备注>
        ///此类与RecylableMemoryStreamManager协同工作以提供MemoryStream
        ///对象调用者，同时避免这些特定问题：
        /// 1. LOH分配 - 由于所有大型缓冲区都是合并的，因此它们永远不会产生Gen2 GC
        /// 2.内存浪费 - 当标准内存流用完时，标准内存流的大小会翻倍。这个
        ///导致持续的内存增长，因为每个流接近允许的最大大小。
        /// 3.内存复制 - 每次MemoryStream增长时，所有字节都被复制到新的缓冲区中。
        ///此实现仅在调用GetBuffer时复制字节。
        /// 4.内存碎片 - 通过使用同类缓冲区大小，它可以确保内存块
        ///可以很容易地重复使用。
        ///
        ///流在一系列统一大小的块之上实现。随着流的长度增长，
        ///从内存管理器中检索其他块。这些块是汇集的，而不是流
        ///对象本身。
        ///
        ///这个实现中最大的问题是调用GetBuffer（）时。这需要一个
        ///连续缓冲区。如果仅使用单个块，则返回该块。如果多个块
        ///正在使用中，我们从内存管理器中检索更大的缓冲区。这些大缓冲区也汇集在一起​​，
        ///按大小拆分 - 它们是块大小的倍数（默认为1 MB）。
        ///
        ///一旦将大缓冲区分配给流，块就不会再次用于此流。所有操作都在
        ///大缓冲区。根据需要，可以使用池中较大的缓冲区替换大缓冲区。所有块和大缓冲区
        ///在流中维护，直到流被释放（除非在流管理器中启用了AggressiveBufferReturn）。
        ///
        /// </备注>
    public sealed class RecyclableMemoryStream : MemoryStream
    {
        private const long MaxStreamLength = Int32.MaxValue;

        private static readonly byte[] emptyArray = new byte[0];

        /// <summary>
        /// All of these blocks must be the same size
        /// </summary>
        private readonly List<byte[]> blocks = new List<byte[]>(1);

        /// <summary>
        /// This buffer exists so that WriteByte can forward all of its calls to Write
        /// without creating a new byte[] buffer on every call.
        /// </summary>
        private readonly byte[] byteBuffer = new byte[1];

        private readonly Guid id;

        private readonly RecyclableMemoryStreamManager memoryManager;

        private readonly string tag;

        /// <summary>
        /// This list is used to store buffers once they're replaced by something larger.
        /// This is for the cases where you have users of this class that may hold onto the buffers longer
        /// than they should and you want to prevent race conditions which could corrupt the data.
        /// </summary>
        private List<byte[]> dirtyBuffers;

        // long to allow Interlocked.Read (for .NET Standard 1.4 compat)
        private long disposedState;

        /// <summary>
        /// This is only set by GetBuffer() if the necessary buffer is larger than a single block size, or on
        /// construction if the caller immediately requests a single large buffer.
        /// </summary>
        /// <remarks>If this field is non-null, it contains the concatenation of the bytes found in the individual
        /// blocks. Once it is created, this (or a larger) largeBuffer will be used for the life of the stream.
        /// </remarks>
        private byte[] largeBuffer;

        /// <summary>
        /// Unique identifier for this stream across it's entire lifetime
        /// </summary>
        /// <exception cref="ObjectDisposedException">Object has been disposed</exception>
        internal Guid Id
        {
            get
            {
                this.CheckDisposed();
                return this.id;
            }
        }

        /// <summary>
        /// A temporary identifier for the current usage of this stream.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Object has been disposed</exception>
        internal string Tag
        {
            get
            {
                this.CheckDisposed();
                return this.tag;
            }
        }

        /// <summary>
        /// Gets the memory manager being used by this stream.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Object has been disposed</exception>
        internal RecyclableMemoryStreamManager MemoryManager
        {
            get
            {
                this.CheckDisposed();
                return this.memoryManager;
            }
        }

        /// <summary>
        /// Callstack of the constructor. It is only set if MemoryManager.GenerateCallStacks is true,
        /// which should only be in debugging situations.
        /// </summary>
        internal string AllocationStack { get; }

        /// <summary>
        /// Callstack of the Dispose call. It is only set if MemoryManager.GenerateCallStacks is true,
        /// which should only be in debugging situations.
        /// </summary>
        internal string DisposeStack { get; private set; }

        #region Constructors
        /// <summary>
        /// Allocate a new RecyclableMemoryStream object.
        /// </summary>
        /// <param name="memoryManager">The memory manager</param>
        public RecyclableMemoryStream(RecyclableMemoryStreamManager memoryManager)
            : this(memoryManager, null, 0, null) { }

        /// <summary>
        /// Allocate a new RecyclableMemoryStream object
        /// </summary>
        /// <param name="memoryManager">The memory manager</param>
        /// <param name="tag">A string identifying this stream for logging and debugging purposes</param>
        public RecyclableMemoryStream(RecyclableMemoryStreamManager memoryManager, string tag)
            : this(memoryManager, tag, 0, null) { }

        /// <summary>
        /// Allocate a new RecyclableMemoryStream object
        /// </summary>
        /// <param name="memoryManager">The memory manager</param>
        /// <param name="tag">A string identifying this stream for logging and debugging purposes</param>
        /// <param name="requestedSize">The initial requested size to prevent future allocations</param>
        public RecyclableMemoryStream(RecyclableMemoryStreamManager memoryManager, string tag, int requestedSize)
            : this(memoryManager, tag, requestedSize, null) { }

        /// <summary>
        /// Allocate a new RecyclableMemoryStream object
        /// </summary>
        /// <param name="memoryManager">The memory manager</param>
        /// <param name="tag">A string identifying this stream for logging and debugging purposes</param>
        /// <param name="requestedSize">The initial requested size to prevent future allocations</param>
        /// <param name="initialLargeBuffer">An initial buffer to use. This buffer will be owned by the stream and returned to the memory manager upon Dispose.</param>
        internal RecyclableMemoryStream(RecyclableMemoryStreamManager memoryManager, string tag, int requestedSize,
                                        byte[] initialLargeBuffer)
            : base(emptyArray)
        {
            this.memoryManager = memoryManager;
            this.id = Guid.NewGuid();
            this.tag = tag;

            if (requestedSize < memoryManager.BlockSize)
            {
                requestedSize = memoryManager.BlockSize;
            }

            if (initialLargeBuffer == null)
            {
                this.EnsureCapacity(requestedSize);
            }
            else
            {
                this.largeBuffer = initialLargeBuffer;
            }

            if (this.memoryManager.GenerateCallStacks)
            {
                this.AllocationStack = Environment.StackTrace;
            }

            RecyclableMemoryStreamManager.Events.Writer.MemoryStreamCreated(this.id, this.tag, requestedSize);
            this.memoryManager.ReportStreamCreated();
        }
        #endregion

        #region Dispose and Finalize
        ~RecyclableMemoryStream()
        {
            this.Dispose(false);
        }

        /// <summary>
        /// Returns the memory used by this stream back to the pool.
        /// </summary>
        /// <param name="disposing">Whether we're disposing (true), or being called by the finalizer (false)</param>
        [SuppressMessage("Microsoft.Usage", "CA1816:CallGCSuppressFinalizeCorrectly",
            Justification = "We have different disposal semantics, so SuppressFinalize is in a different spot.")]
        protected override void Dispose(bool disposing)
        {
            if (Interlocked.CompareExchange(ref this.disposedState, 1, 0) != 0)
            {
                string doubleDisposeStack = null;
                if (this.memoryManager.GenerateCallStacks)
                {
                    doubleDisposeStack = Environment.StackTrace;
                }

                RecyclableMemoryStreamManager.Events.Writer.MemoryStreamDoubleDispose(this.id, this.tag,
                                                                                     this.AllocationStack,
                                                                                     this.DisposeStack,
                                                                                     doubleDisposeStack);
                return;
            }

            RecyclableMemoryStreamManager.Events.Writer.MemoryStreamDisposed(this.id, this.tag);

            if (this.memoryManager.GenerateCallStacks)
            {
                this.DisposeStack = Environment.StackTrace;
            }

            if (disposing)
            {
                this.memoryManager.ReportStreamDisposed();

                GC.SuppressFinalize(this);
            }
            else
            {
                // We're being finalized.

                RecyclableMemoryStreamManager.Events.Writer.MemoryStreamFinalized(this.id, this.tag, this.AllocationStack);

#if !NETSTANDARD1_4
                if (AppDomain.CurrentDomain.IsFinalizingForUnload())
                {
                    // If we're being finalized because of a shutdown, don't go any further.
                    // We have no idea what's already been cleaned up. Triggering events may cause
                    // a crash.
                    base.Dispose(disposing);
                    return;
                }
#endif

                this.memoryManager.ReportStreamFinalized();
            }

            this.memoryManager.ReportStreamLength(this.length);

            if (this.largeBuffer != null)
            {
                this.memoryManager.ReturnLargeBuffer(this.largeBuffer, this.tag);
            }

            if (this.dirtyBuffers != null)
            {
                foreach (var buffer in this.dirtyBuffers)
                {
                    this.memoryManager.ReturnLargeBuffer(buffer, this.tag);
                }
            }

            this.memoryManager.ReturnBlocks(this.blocks, this.tag);
            this.blocks.Clear();

            base.Dispose(disposing);
        }

        /// <summary>
        /// Equivalent to Dispose
        /// </summary>
#if NETSTANDARD1_4
        public void Close()
#else
        public override void Close()
#endif
        {
            this.Dispose(true);
        }
        #endregion

        #region MemoryStream overrides
        /// <summary>
        /// Gets or sets the capacity
        /// </summary>
        /// <remarks>Capacity is always in multiples of the memory manager's block size, unless
        /// the large buffer is in use.  Capacity never decreases during a stream's lifetime. 
        /// Explicitly setting the capacity to a lower value than the current value will have no effect. 
        /// This is because the buffers are all pooled by chunks and there's little reason to 
        /// allow stream truncation.
        /// </remarks>
        /// <exception cref="ObjectDisposedException">Object has been disposed</exception>
        public override int Capacity
        {
            get
            {
                this.CheckDisposed();
                if (this.largeBuffer != null)
                {
                    return this.largeBuffer.Length;
                }

                long size = (long)this.blocks.Count * this.memoryManager.BlockSize;
                return (int)Math.Min(int.MaxValue, size);
            }
            set
            {
                this.CheckDisposed();
                this.EnsureCapacity(value);
            }
        }

        private int length;

        /// <summary>
        /// Gets the number of bytes written to this stream.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Object has been disposed</exception>
        public override long Length
        {
            get
            {
                this.CheckDisposed();
                return this.length;
            }
        }

        private int position;

        /// <summary>
        /// Gets the current position in the stream
        /// </summary>
        /// <exception cref="ObjectDisposedException">Object has been disposed</exception>
        public override long Position
        {
            get
            {
                this.CheckDisposed();
                return this.position;
            }
            set
            {
                this.CheckDisposed();
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException("value", "value must be non-negative");
                }

                if (value > MaxStreamLength)
                {
                    throw new ArgumentOutOfRangeException("value", "value cannot be more than " + MaxStreamLength);
                }

                this.position = (int)value;
            }
        }

        /// <summary>
        /// Whether the stream can currently read
        /// </summary>
        public override bool CanRead => !this.Disposed;

        /// <summary>
        /// Whether the stream can currently seek
        /// </summary>
        public override bool CanSeek => !this.Disposed;

        /// <summary>
        /// Always false
        /// </summary>
        public override bool CanTimeout => false;

        /// <summary>
        /// Whether the stream can currently write
        /// </summary>
        public override bool CanWrite => !this.Disposed;

        /// <summary>
        /// Returns a single buffer containing the contents of the stream.
        /// The buffer may be longer than the stream length.
        /// </summary>
        /// <returns>A byte[] buffer</returns>
        /// <remarks>IMPORTANT: Doing a Write() after calling GetBuffer() invalidates the buffer. The old buffer is held onto
        /// until Dispose is called, but the next time GetBuffer() is called, a new buffer from the pool will be required.</remarks>
        /// <exception cref="ObjectDisposedException">Object has been disposed</exception>
#if NETSTANDARD1_4
        public byte[] GetBuffer()
#else
        public override byte[] GetBuffer()
#endif
        {
            this.CheckDisposed();

            if (this.largeBuffer != null)
            {
                return this.largeBuffer;
            }

            if (this.blocks.Count == 1)
            {
                return this.blocks[0];
            }

            // Buffer needs to reflect the capacity, not the length, because
            // it's possible that people will manipulate the buffer directly
            // and set the length afterward. Capacity sets the expectation
            // for the size of the buffer.
            var newBuffer = this.memoryManager.GetLargeBuffer(this.Capacity, this.tag);

            // InternalRead will check for existence of largeBuffer, so make sure we
            // don't set it until after we've copied the data.
            this.InternalRead(newBuffer, 0, this.length, 0);
            this.largeBuffer = newBuffer;

            if (this.blocks.Count > 0 && this.memoryManager.AggressiveBufferReturn)
            {
                this.memoryManager.ReturnBlocks(this.blocks, this.tag);
                this.blocks.Clear();
            }

            return this.largeBuffer;
        }

        /// <summary>
        /// Returns a new array with a copy of the buffer's contents. You should almost certainly be using GetBuffer combined with the Length to 
        /// access the bytes in this stream. Calling ToArray will destroy the benefits of pooled buffers, but it is included
        /// for the sake of completeness.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Object has been disposed</exception>
#pragma warning disable CS0809
        [Obsolete("This method has degraded performance vs. GetBuffer and should be avoided.")]
        public override byte[] ToArray()
        {
            this.CheckDisposed();
            var newBuffer = new byte[this.Length];

            this.InternalRead(newBuffer, 0, this.length, 0);
            string stack = this.memoryManager.GenerateCallStacks ? Environment.StackTrace : null;
            RecyclableMemoryStreamManager.Events.Writer.MemoryStreamToArray(this.id, this.tag, stack, 0);
            this.memoryManager.ReportStreamToArray();

            return newBuffer;
        }
#pragma warning restore CS0809

        /// <summary>
        /// Reads from the current position into the provided buffer
        /// </summary>
        /// <param name="buffer">Destination buffer</param>
        /// <param name="offset">Offset into buffer at which to start placing the read bytes.</param>
        /// <param name="count">Number of bytes to read.</param>
        /// <returns>The number of bytes read</returns>
        /// <exception cref="ArgumentNullException">buffer is null</exception>
        /// <exception cref="ArgumentOutOfRangeException">offset or count is less than 0</exception>
        /// <exception cref="ArgumentException">offset subtracted from the buffer length is less than count</exception>
        /// <exception cref="ObjectDisposedException">Object has been disposed</exception>
        public override int Read(byte[] buffer, int offset, int count)
        {
            return this.SafeRead(buffer, offset, count, ref this.position);
        }

        /// <summary>
        /// Reads from the specified position into the provided buffer
        /// </summary>
        /// <param name="buffer">Destination buffer</param>
        /// <param name="offset">Offset into buffer at which to start placing the read bytes.</param>
        /// <param name="count">Number of bytes to read.</param>
        /// <param name="streamPosition">Position in the stream to start reading from</param>
        /// <returns>The number of bytes read</returns>
        /// <exception cref="ArgumentNullException">buffer is null</exception>
        /// <exception cref="ArgumentOutOfRangeException">offset or count is less than 0</exception>
        /// <exception cref="ArgumentException">offset subtracted from the buffer length is less than count</exception>
        /// <exception cref="ObjectDisposedException">Object has been disposed</exception>
        public int SafeRead(byte[] buffer, int offset, int count, ref int streamPosition)
        {
            this.CheckDisposed();
            if (buffer == null)
            {
                throw new ArgumentNullException(nameof(buffer));
            }

            if (offset < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(offset), "offset cannot be negative");
            }

            if (count < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(count), "count cannot be negative");
            }

            if (offset + count > buffer.Length)
            {
                throw new ArgumentException("buffer length must be at least offset + count");
            }

            int amountRead = this.InternalRead(buffer, offset, count, streamPosition);
            streamPosition += amountRead;
            return amountRead;
        }

        /// <summary>
        /// Writes the buffer to the stream
        /// </summary>
        /// <param name="buffer">Source buffer</param>
        /// <param name="offset">Start position</param>
        /// <param name="count">Number of bytes to write</param>
        /// <exception cref="ArgumentNullException">buffer is null</exception>
        /// <exception cref="ArgumentOutOfRangeException">offset or count is negative</exception>
        /// <exception cref="ArgumentException">buffer.Length - offset is not less than count</exception>
        /// <exception cref="ObjectDisposedException">Object has been disposed</exception>
        public override void Write(byte[] buffer, int offset, int count)
        {
            this.CheckDisposed();
            if (buffer == null)
            {
                throw new ArgumentNullException(nameof(buffer));
            }

            if (offset < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(offset), offset,
                                                      "Offset must be in the range of 0 - buffer.Length-1");
            }

            if (count < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(count), count, "count must be non-negative");
            }

            if (count + offset > buffer.Length)
            {
                throw new ArgumentException("count must be greater than buffer.Length - offset");
            }

            int blockSize = this.memoryManager.BlockSize;
            long end = (long)this.position + count;
            // Check for overflow
            if (end > MaxStreamLength)
            {
                throw new IOException("Maximum capacity exceeded");
            }

            long requiredBuffers = (end + blockSize - 1) / blockSize;

            if (requiredBuffers * blockSize > MaxStreamLength)
            {
                throw new IOException("Maximum capacity exceeded");
            }

            this.EnsureCapacity((int)end);

            if (this.largeBuffer == null)
            {
                int bytesRemaining = count;
                int bytesWritten = 0;
                var blockAndOffset = this.GetBlockAndRelativeOffset(this.position);

                while (bytesRemaining > 0)
                {
                    byte[] currentBlock = this.blocks[blockAndOffset.Block];
                    int remainingInBlock = blockSize - blockAndOffset.Offset;
                    int amountToWriteInBlock = Math.Min(remainingInBlock, bytesRemaining);

                    Buffer.BlockCopy(buffer, offset + bytesWritten, currentBlock, blockAndOffset.Offset,
                                     amountToWriteInBlock);

                    bytesRemaining -= amountToWriteInBlock;
                    bytesWritten += amountToWriteInBlock;

                    ++blockAndOffset.Block;
                    blockAndOffset.Offset = 0;
                }
            }
            else
            {
                Buffer.BlockCopy(buffer, offset, this.largeBuffer, this.position, count);
            }
            this.position = (int)end;
            this.length = Math.Max(this.position, this.length);
        }

        /// <summary>
        /// Returns a useful string for debugging. This should not normally be called in actual production code.
        /// </summary>
        public override string ToString()
        {
            return $"Id = {this.Id}, Tag = {this.Tag}, Length = {this.Length:N0} bytes";
        }

        /// <summary>
        /// Writes a single byte to the current position in the stream.
        /// </summary>
        /// <param name="value">byte value to write</param>
        /// <exception cref="ObjectDisposedException">Object has been disposed</exception>
        public override void WriteByte(byte value)
        {
            this.CheckDisposed();
            this.byteBuffer[0] = value;
            this.Write(this.byteBuffer, 0, 1);
        }

        /// <summary>
        /// Reads a single byte from the current position in the stream.
        /// </summary>
        /// <returns>The byte at the current position, or -1 if the position is at the end of the stream.</returns>
        /// <exception cref="ObjectDisposedException">Object has been disposed</exception>
        public override int ReadByte()
        {
            return this.SafeReadByte(ref this.position);
        }

        /// <summary>
        /// Reads a single byte from the specified position in the stream.
        /// </summary>
        /// <param name="streamPosition">The position in the stream to read from</param>
        /// <returns>The byte at the current position, or -1 if the position is at the end of the stream.</returns>
        /// <exception cref="ObjectDisposedException">Object has been disposed</exception>
        public int SafeReadByte(ref int streamPosition)
        {
            this.CheckDisposed();
            if (streamPosition == this.length)
            {
                return -1;
            }
            byte value;
            if (this.largeBuffer == null)
            {
                var blockAndOffset = this.GetBlockAndRelativeOffset(streamPosition);
                value = this.blocks[blockAndOffset.Block][blockAndOffset.Offset];
            }
            else
            {
                value = this.largeBuffer[streamPosition];
            }
            streamPosition++;
            return value;
        }

        /// <summary>
        /// Sets the length of the stream
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">value is negative or larger than MaxStreamLength</exception>
        /// <exception cref="ObjectDisposedException">Object has been disposed</exception>
        public override void SetLength(long value)
        {
            this.CheckDisposed();
            if (value < 0 || value > MaxStreamLength)
            {
                throw new ArgumentOutOfRangeException(nameof(value),
                                                      "value must be non-negative and at most " + MaxStreamLength);
            }

            this.EnsureCapacity((int)value);

            this.length = (int)value;
            if (this.position > value)
            {
                this.position = (int)value;
            }
        }

        /// <summary>
        /// Sets the position to the offset from the seek location
        /// </summary>
        /// <param name="offset">How many bytes to move</param>
        /// <param name="loc">From where</param>
        /// <returns>The new position</returns>
        /// <exception cref="ObjectDisposedException">Object has been disposed</exception>
        /// <exception cref="ArgumentOutOfRangeException">offset is larger than MaxStreamLength</exception>
        /// <exception cref="ArgumentException">Invalid seek origin</exception>
        /// <exception cref="IOException">Attempt to set negative position</exception>
        public override long Seek(long offset, SeekOrigin loc)
        {
            this.CheckDisposed();
            if (offset > MaxStreamLength)
            {
                throw new ArgumentOutOfRangeException(nameof(offset), "offset cannot be larger than " + MaxStreamLength);
            }

            int newPosition;
            switch (loc)
            {
            case SeekOrigin.Begin:
                newPosition = (int)offset;
                break;
            case SeekOrigin.Current:
                newPosition = (int)offset + this.position;
                break;
            case SeekOrigin.End:
                newPosition = (int)offset + this.length;
                break;
            default:
                throw new ArgumentException("Invalid seek origin", nameof(loc));
            }
            if (newPosition < 0)
            {
                throw new IOException("Seek before beginning");
            }
            this.position = newPosition;
            return this.position;
        }

        /// <summary>
        /// Synchronously writes this stream's bytes to the parameter stream.
        /// </summary>
        /// <param name="stream">Destination stream</param>
        /// <remarks>Important: This does a synchronous write, which may not be desired in some situations</remarks>
        public override void WriteTo(Stream stream)
        {
            this.CheckDisposed();
            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            if (this.largeBuffer == null)
            {
                int currentBlock = 0;
                int bytesRemaining = this.length;

                while (bytesRemaining > 0)
                {
                    int amountToCopy = Math.Min(this.blocks[currentBlock].Length, bytesRemaining);
                    stream.Write(this.blocks[currentBlock], 0, amountToCopy);

                    bytesRemaining -= amountToCopy;

                    ++currentBlock;
                }
            }
            else
            {
                stream.Write(this.largeBuffer, 0, this.length);
            }
        }
        #endregion

        #region Helper Methods
        private bool Disposed => Interlocked.Read(ref this.disposedState) != 0;

        private void CheckDisposed()
        {
            if (this.Disposed)
            {
                throw new ObjectDisposedException($"The stream with Id {this.id} and Tag {this.tag} is disposed.");
            }
        }

        private int InternalRead(byte[] buffer, int offset, int count, int fromPosition)
        {
            if (this.length - fromPosition <= 0)
            {
                return 0;
            }

            int amountToCopy;

            if (this.largeBuffer == null)
            {
                var blockAndOffset = this.GetBlockAndRelativeOffset(fromPosition);
                int bytesWritten = 0;
                int bytesRemaining = Math.Min(count, this.length - fromPosition);

                while (bytesRemaining > 0)
                {
                    amountToCopy = Math.Min(this.blocks[blockAndOffset.Block].Length - blockAndOffset.Offset,
                                                bytesRemaining);
                    Buffer.BlockCopy(this.blocks[blockAndOffset.Block], blockAndOffset.Offset, buffer,
                                     bytesWritten + offset, amountToCopy);

                    bytesWritten += amountToCopy;
                    bytesRemaining -= amountToCopy;

                    ++blockAndOffset.Block;
                    blockAndOffset.Offset = 0;
                }
                return bytesWritten;
            }
            amountToCopy = Math.Min(count, this.length - fromPosition);
            Buffer.BlockCopy(this.largeBuffer, fromPosition, buffer, offset, amountToCopy);
            return amountToCopy;
        }

        private struct BlockAndOffset
        {
            public int Block;
            public int Offset;

            public BlockAndOffset(int block, int offset)
            {
                this.Block = block;
                this.Offset = offset;
            }
        }

        private BlockAndOffset GetBlockAndRelativeOffset(int offset)
        {
            var blockSize = this.memoryManager.BlockSize;
            return new BlockAndOffset(offset / blockSize, offset % blockSize);
        }

        private void EnsureCapacity(int newCapacity)
        {
            if (newCapacity > this.memoryManager.MaximumStreamCapacity && this.memoryManager.MaximumStreamCapacity > 0)
            {
                RecyclableMemoryStreamManager.Events.Writer.MemoryStreamOverCapacity(newCapacity,
                                                                                    this.memoryManager
                                                                                        .MaximumStreamCapacity, this.tag,
                                                                                    this.AllocationStack);
                throw new InvalidOperationException("Requested capacity is too large: " + newCapacity + ". Limit is " +
                                                    this.memoryManager.MaximumStreamCapacity);
            }

            if (this.largeBuffer != null)
            {
                if (newCapacity > this.largeBuffer.Length)
                {
                    var newBuffer = this.memoryManager.GetLargeBuffer(newCapacity, this.tag);
                    this.InternalRead(newBuffer, 0, this.length, 0);
                    this.ReleaseLargeBuffer();
                    this.largeBuffer = newBuffer;
                }
            }
            else
            {
                while (this.Capacity < newCapacity)
                {
                    blocks.Add((this.memoryManager.GetBlock()));
                }
            }
        }

        /// <summary>
        /// Release the large buffer (either stores it for eventual release or returns it immediately).
        /// </summary>
        private void ReleaseLargeBuffer()
        {
            if (this.memoryManager.AggressiveBufferReturn)
            {
                this.memoryManager.ReturnLargeBuffer(this.largeBuffer, this.tag);
            }
            else
            {
                if (this.dirtyBuffers == null)
                {
                    // We most likely will only ever need space for one
                    this.dirtyBuffers = new List<byte[]>(1);
                }
                this.dirtyBuffers.Add(this.largeBuffer);
            }

            this.largeBuffer = null;
        }
        #endregion
    }
}