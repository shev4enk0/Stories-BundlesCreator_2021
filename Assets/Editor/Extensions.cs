using System.IO;
using System.Collections.Generic;

public static partial class Extensions
{
	public static string CombinePathsWith(this string first, params string[] others)
	{
		var path = first;
		foreach (string section in others)
			path = Path.Combine(path, section);


		first = path;

		return path;
	}

	public static List<FileInfo> GetFilesRecursive(this DirectoryInfo directory, string filePattern = "*")
	{
		var fileList = new List<FileInfo>();
		fileList.AddRange(directory.GetFiles(filePattern));

		foreach (var dir in directory.GetDirectories())
			fileList.AddRange(dir.GetFilesRecursive(filePattern));

		return fileList;
	}
}
