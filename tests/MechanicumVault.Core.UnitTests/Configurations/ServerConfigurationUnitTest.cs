using MechanicumVault.Core.Configurations;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace MechanicumVault.Core.UnitTests.Configurations;

public class ServerConfigurationUnitTest
{
	[Fact]
	public void ServerConfiguration_WithoutJsonFile()
	{
		var cfg = new ConfigurationBuilder()
			.AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
			.Build();

		Assert.Null(cfg.GetSection("Server").Get<ServerConfiguration>());
	}

	[Fact]
	public void ServerConfiguration_WithJsonFile()
	{
		var cfg = new ConfigurationBuilder()
			.AddJsonFile("Resources/appsettings.json", optional: true, reloadOnChange: false)
			.Build();

		var serverCfg = cfg.GetSection("Server").Get<ServerConfiguration>();

		Assert.NotNull(serverCfg);
		Assert.Equal("127.0.0.1", serverCfg?.Ip);
		Assert.Equal(52000, serverCfg?.Port);
	}
}