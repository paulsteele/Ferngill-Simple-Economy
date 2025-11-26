using System.IO;

namespace fse.core.services;

public interface IFileService
{
	string? ReadAllText(string path);
}

public class FileService : IFileService
{
	public string? ReadAllText(string path) => !File.Exists(path) ? null : File.ReadAllText(path);
}