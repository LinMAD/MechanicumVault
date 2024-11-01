using MechanicumVault.Core.Configurations;
using Microsoft.Extensions.Configuration;

namespace MechanicumVault.Core.UnitTests.Configurations;

public class ServerConfigurationUnitTest
{
	[SetUp]
	public void Setup()
	{
	}

	[Test]
	public void ServerConfiguration_WithoutJsonFile()
	{
		var cfg = new ConfigurationBuilder()
			.AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
			.Build();
		
		var serverCfg = cfg.GetSection("Server").Get<ServerConfiguration>();
		
		Assert.Null(serverCfg, "Expected that no configuration file was found.");
	}
	
	[Test]
	public void ServerConfiguration_WithJsonFile()
	{
		var cfg = new ConfigurationBuilder()
			.AddJsonFile("Resources/appsettings.json", optional: true, reloadOnChange: false)
			.Build();
		
		var serverCfg = cfg.GetSection("Server").Get<ServerConfiguration>();
		
		Assert.NotNull(serverCfg);
		Assert.That(serverCfg.Ip, Is.EqualTo("127.0.0.1"));
		Assert.That(serverCfg.Port, Is.EqualTo(52000));
	}
}