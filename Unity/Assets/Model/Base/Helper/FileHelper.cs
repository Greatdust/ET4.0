using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ETModel
{
    /// <summary>
	/// 文件工具类
	/// </summary>
	public static class FileHelper
	{
        /// <summary>
		/// 获得文件夹路径下所有的文件 包括子文件夹
		/// </summary>
		/// <param name="files">Files.</param>
		/// <param name="dir">文件夹目录.</param>
		public static void GetAllFiles(List<string> files, string dir)
		{
			string[] fls = Directory.GetFiles(dir);
			foreach (string fl in fls)
			{
				files.Add(fl);
			}

			string[] subDirs = Directory.GetDirectories(dir);
			foreach (string subDir in subDirs)
			{
				GetAllFiles(files, subDir);
			}
		}

        /// <summary>
        /// 清空文件夹
        /// </summary>
        /// <param name="dir">Dir.</param>
        public static void CleanDirectory(string dir)
		{
			foreach (string subdir in Directory.GetDirectories(dir))
			{
				Directory.Delete(subdir, true);		
			}

			foreach (string subFile in Directory.GetFiles(dir))
			{
				File.Delete(subFile);
			}
		}

        /// <summary>
        /// 复制文件夹
        /// </summary>
        /// <param name="srcDir">Source dir.</param>
        /// <param name="tgtDir">Tgt dir.</param>
        public static void CopyDirectory(string srcDir, string tgtDir)
		{
			DirectoryInfo source = new DirectoryInfo(srcDir);
			DirectoryInfo target = new DirectoryInfo(tgtDir);
	
			if (target.FullName.StartsWith(source.FullName, StringComparison.CurrentCultureIgnoreCase))
			{
				throw new Exception("父目录不能拷贝到子目录！");
			}
	
			if (!source.Exists)
			{
				return;
			}
	
			if (!target.Exists)
			{
				target.Create();
			}
	
			FileInfo[] files = source.GetFiles();
	
			for (int i = 0; i < files.Length; i++)
			{
				File.Copy(files[i].FullName, Path.Combine(target.FullName, files[i].Name), true);
			}
	
			DirectoryInfo[] dirs = source.GetDirectories();
	
			for (int j = 0; j < dirs.Length; j++)
			{
				CopyDirectory(dirs[j].FullName, Path.Combine(target.FullName, dirs[j].Name));
			}
		}
	}
}
