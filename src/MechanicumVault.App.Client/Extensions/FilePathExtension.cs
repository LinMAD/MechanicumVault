using MechanicumVault.App.Client.Common.Configurations;

namespace MechanicumVault.App.Client.Extensions;

public static class FilePathExtension
{

	/// <summary>
	/// Returns path that is relative from root, so to the server will be sent only inner structure.
	/// </summary>
	/// <param name="fullPath">File for synchronization that will be sent to server.</param>
	/// <param name="applicationConfiguration"></param>
	/// <returns></returns>
	/// <exception cref="ArgumentException"></exception>
	public static string GetSourceFolderRelativePath(this string fullPath, ApplicationConfiguration applicationConfiguration)
	{
		var rootAbsolute = Path.GetFullPath(applicationConfiguration.SourceDirectory);
		var fullAbsolute = Path.GetFullPath(fullPath);
		if (!fullAbsolute.StartsWith(rootAbsolute, StringComparison.OrdinalIgnoreCase))
		{
			throw new ArgumentException("The specified path is not under the root folder.");
		}

		return Path.GetRelativePath(rootAbsolute, fullAbsolute);
	}
}