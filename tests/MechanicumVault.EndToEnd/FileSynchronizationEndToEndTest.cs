using MechanicumVault.EndToEnd.Handlers;

namespace MechanicumVault.EndToEnd;

[CollectionDefinition("Sequential End to End Collection", DisableParallelization = true)]
public class SequentialCollection { } // This is a stub to force xUnit run by 1 test cases since they use same folders.

[Collection("Sequential End to End Collection")]
public class FileSynchronizationEndToEndTest
{
	private const int DefaultTimeout = 3000;

	const string ClientSourceDirectoryBasePath = "Resources\\SourceDirectory";
	const string ServerDestinationDirectoryBasePath = "Resources\\DestinationDirectory";

	const string TestFileName = "e2e.txt";
	const string TestFileContent = "Lorem Ipsum is simply dummy text of the printing and typesetting industry.";
	const string ClientSubDirectoryName = "TestSubFolder";

	// TODO Take a look on E2E test => looks like there is Race condition for file manipulation in test cases

	public FileSynchronizationEndToEndTest()
	{
		var clientPathToCreateFile = Path.Combine(ClientSourceDirectoryBasePath, TestFileName);
		var serverPathToSyncFile = Path.Combine(ServerDestinationDirectoryBasePath, TestFileName);

		File.Delete(clientPathToCreateFile);
		File.Delete(serverPathToSyncFile);
		{
			var clientSubFolder = Path.Combine(ClientSourceDirectoryBasePath, ClientSubDirectoryName);
			var serverSubFolder = Path.Combine(ServerDestinationDirectoryBasePath, ClientSubDirectoryName);
			if (Directory.Exists(clientSubFolder))
				Directory.Delete(clientSubFolder, true);
			if (Directory.Exists(serverSubFolder))
				Directory.Delete(serverSubFolder, true);
		}
	}

	[Fact]
	public async Task ConnectClientToServer_ConnectServer()
	{
		using var cancellationTokenSource = new CancellationTokenSource();
		var cancellationToken = cancellationTokenSource.Token;

		var integration = new IntegrationProcessHandler(ClientSourceDirectoryBasePath, ServerDestinationDirectoryBasePath);

		bool serverCompletedWithinTimeout = false;
		bool clientCompletedWithinTimeout = false;

		var serverTask = RunServerTaskAsync();
		var clientTask = RunClientTaskAsync();

		serverCompletedWithinTimeout = await Task.WhenAny(
			serverTask,
			Task.Delay(DefaultTimeout, cancellationToken)
		) == serverTask && serverTask is { Result: true };
		clientCompletedWithinTimeout = await Task.WhenAny(
			clientTask,
			Task.Delay(DefaultTimeout, cancellationToken)
		) == clientTask && clientTask is { Result: true };

		if (!serverCompletedWithinTimeout || !clientCompletedWithinTimeout)
		{
			await cancellationTokenSource.CancelAsync();
		}

		Assert.False(serverCompletedWithinTimeout, "Server should not have completed within the specified timeout.");
		Assert.False(clientCompletedWithinTimeout, "Client should not have connected to Server within the specified timeout.");
		return;

		async Task<bool> RunServerTaskAsync()
		{
			try
			{
				await Task.Run(() => integration.StartServer(cancellationToken), cancellationToken);
				return true;
			}
			catch (OperationCanceledException)
			{
				return false;
			}
		}

		async Task<bool> RunClientTaskAsync()
		{
			try
			{
				await Task.Run(() => integration.StartClient(cancellationToken), cancellationToken);
				return true;
			}
			catch (OperationCanceledException)
			{
				return false;
			}
		}
	}

	[Fact]
	public async Task ConnectClientToServer_NewFileAndDelete()
	{
		var cancellationTokenSource = new CancellationTokenSource();
		var integration = new IntegrationProcessHandler(ClientSourceDirectoryBasePath, ServerDestinationDirectoryBasePath);

		var clientPathToCreateFile = Path.Combine(ClientSourceDirectoryBasePath, TestFileName);
		var serverPathToSyncFile = Path.Combine(ServerDestinationDirectoryBasePath, TestFileName);

		_ = integration.StartServerWithClient(cancellationTokenSource.Token);

		Thread.Sleep(1000);
		File.Create(clientPathToCreateFile).Close();
		Assert.True(Directory.Exists(ClientSourceDirectoryBasePath));
		Assert.True(File.Exists(clientPathToCreateFile));

		Thread.Sleep(1000);
		File.Delete(clientPathToCreateFile);
		Thread.Sleep(1000);

		Assert.True(Directory.Exists(ServerDestinationDirectoryBasePath));
		Assert.False(File.Exists(serverPathToSyncFile));

		await cancellationTokenSource.CancelAsync();
	}

	[Fact]
	public async Task ConnectClientToServer_NewFileWithContent()
	{
		var cancellationTokenSource = new CancellationTokenSource();
		var integration = new IntegrationProcessHandler(ClientSourceDirectoryBasePath, ServerDestinationDirectoryBasePath);

		var clientPathToCreateFile = Path.Combine(ClientSourceDirectoryBasePath, TestFileName);
		var serverPathToSyncFile = Path.Combine(ServerDestinationDirectoryBasePath, TestFileName);

		_ = integration.StartServerWithClient(cancellationTokenSource.Token);

		Thread.Sleep(1000);
		File.Create(clientPathToCreateFile).Close();

		await using (StreamWriter writer = new StreamWriter(clientPathToCreateFile, append: true))
		{
			await writer.WriteLineAsync(TestFileContent);
		}
		Thread.Sleep(2000);

		Assert.True(File.Exists(serverPathToSyncFile));

		string fileContent = await File.ReadAllTextAsync(serverPathToSyncFile, cancellationTokenSource.Token);
		Assert.NotEmpty(fileContent);
		Assert.Matches(TestFileContent, fileContent);

		await cancellationTokenSource.CancelAsync();
	}

	[Fact]
	public async Task ConnectClientToServer_FolderWithFile()
	{
		var cancellationTokenSource = new CancellationTokenSource();
		var integration = new IntegrationProcessHandler(ClientSourceDirectoryBasePath, ServerDestinationDirectoryBasePath);

		var clientPathToCreateSubDirectory = Path.Combine(ClientSourceDirectoryBasePath, ClientSubDirectoryName);
		var clientPathToCreateFile = Path.Combine(clientPathToCreateSubDirectory, TestFileName);
		var serverPathToSyncSubDirectory = Path.Combine(ServerDestinationDirectoryBasePath, ClientSubDirectoryName);
		var serverPathToSyncFile = Path.Combine(serverPathToSyncSubDirectory, TestFileName);

		_ = integration.StartServerWithClient(cancellationTokenSource.Token);

		Thread.Sleep(1000);
		Directory.CreateDirectory(clientPathToCreateSubDirectory);
		File.Create(clientPathToCreateFile).Close();
		await using (StreamWriter writer = new StreamWriter(clientPathToCreateFile, append: true))
		{
			await writer.WriteLineAsync(TestFileContent);
		}
		Thread.Sleep(3000);

		Assert.True(Directory.Exists(serverPathToSyncSubDirectory));
		Assert.True(File.Exists(serverPathToSyncFile));

		string fileContent = await File.ReadAllTextAsync(serverPathToSyncFile, cancellationTokenSource.Token);
		Assert.NotEmpty(fileContent);
		Assert.Matches(TestFileContent, fileContent);

		await cancellationTokenSource.CancelAsync();
	}
}