using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;

namespace ETModel
{
    /// <summary>
    /// 文件版本配置信息
    /// </summary>
	public class FileVersionInfo
	{
		public string File;
		public string MD5;
		public long Size;
	}

    /// <summary>
    /// 版本配置文件
    /// </summary>
	public class VersionConfig : Object
	{
        /// <summary>
        /// 版本号
        /// </summary>
		public int Version;
        /// <summary>
        /// 总大小
        /// </summary>
        public long TotalSize;
		/// <summary>
        /// 文件信息表
        /// </summary>
		[BsonIgnore]
		public Dictionary<string, FileVersionInfo> FileInfoDict = new Dictionary<string, FileVersionInfo>();

        /// <summary>
        /// 初始化完成之后 遍历字典得到文件总大小
        /// </summary>
		public override void EndInit()
		{
			base.EndInit();

			foreach (FileVersionInfo fileVersionInfo in this.FileInfoDict.Values)
			{
				this.TotalSize += fileVersionInfo.Size;
			}
		}
	}
}