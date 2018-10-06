using System.Collections.Generic;

namespace ETModel
{
    /// <summary>
    /// 操作码 助手
    /// </summary>
    public static class OpcodeHelper
	{
        /// <summary>
        /// 需要调试日志消息集。
        /// </summary>
        private static readonly HashSet<ushort> needDebugLogMessageSet = new HashSet<ushort> { 1 };

        /// <summary>
        /// 确定指定的操作码如果是需要调试日志消息。
        /// </summary>
        /// <returns><c>true</c>果指定的操作码是需要调试日志消息；否则， <c>false</c>.</returns>
        /// <param name="opcode">Opcode.</param>
        public static bool IsNeedDebugLogMessage(ushort opcode)
		{
			//return true;
			if (opcode > 1000)
			{
				return true;
			}

			if (needDebugLogMessageSet.Contains(opcode))
			{
				return true;
			}

			return false;
		}
        /// <summary>
        /// 如果是客户端修补程序的信息，确定指定的操作码。
        /// </summary>
        /// <returns><c>true</c> if is client hotfix message the specified opcode; otherwise, <c>false</c>.</returns>
        /// <param name="opcode">Opcode.</param>
        public static bool IsClientHotfixMessage(ushort opcode)
		{
			return opcode > 10000;
		}
	}
}