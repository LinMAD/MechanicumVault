using MechanicumVault.App.Client.Common.Mode;

namespace MechanicumVault.App.Client.Common.Configurations;

public class ApplicationConfiguration
{
	public required ClientProviderMode SourceMode { get; set; }
	public required string SourceDirectory { get; set; } = string.Empty;
}