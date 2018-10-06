using System.IO;
using System.Security.Cryptography;

namespace ETModel
{
	public static class MD5Helper
	{
        /// <summary>
		/// 获取文件MD5值
		/// </summary>
		/// <returns>The M d5.</returns>
		/// <param name="filePath">File path.</param>
		public static string FileMD5(string filePath)
		{
			byte[] retVal;
            using (FileStream file = new FileStream(filePath, FileMode.Open))
			{
				MD5 md5 = new MD5CryptoServiceProvider();
				retVal = md5.ComputeHash(file);
			}
			return retVal.ToHex("x2");
		}
	}
}
