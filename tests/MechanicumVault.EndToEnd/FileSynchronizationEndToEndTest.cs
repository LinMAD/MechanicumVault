using MechanicumVault.EndToEnd.Handlers;

namespace MechanicumVault.EndToEnd;

public class FileSynchronizationEndToEndTest
{
	private const int DefaultTimeout = 3000;
	const string ClientSourceDirectory = "Resources\\SourceDirectory";
	const string ServerDestinationDirectory = "Resources\\DestinationDirectory";
	const string TestFileName = "e2e.txt";
	const string TestFileContent = "Lorem Ipsum is simply dummy text of the printing and typesetting industry.";

	[Fact]
	public void ConnectClientToServer_ConnectServer()
	{
		var integration = new IntegrationProcessHandler(ClientSourceDirectory, ServerDestinationDirectory);
		
		bool serverCompletedWithinTimeout = false;
		bool clientCompletedWithinTimeout = false;
		
		Task serverTask = Task.Run(() =>
		{
			integration.StartServer(new CancellationToken());
			Assert.False(serverCompletedWithinTimeout, "Server started before timout of task");
		});
		Task clientTask = Task.Run(() =>
		{
			integration.StartClient(new CancellationToken());
			Assert.False(clientCompletedWithinTimeout, "Client must be connected to Server before timout of task");
		});

		Thread.Sleep(500);
		serverCompletedWithinTimeout = serverTask.Wait(millisecondsTimeout: DefaultTimeout);
		clientCompletedWithinTimeout = clientTask.Wait(millisecondsTimeout: DefaultTimeout);
	}

	[Fact]
	public async Task ConnectClientToServer_SynchronizeNewFile()
	{
		var cancellationTokenSource = new CancellationTokenSource();
		var integration = new IntegrationProcessHandler(ClientSourceDirectory, ServerDestinationDirectory);

		var clientPathToCreateFile = Path.Combine(ClientSourceDirectory, TestFileName);
		var serverPathToSyncFile = Path.Combine(ServerDestinationDirectory, TestFileName);
		
		_ = integration.StartServerWithClient(cancellationTokenSource.Token);
		
		Thread.Sleep(1000);
		File.Create(clientPathToCreateFile).Close();	
		Thread.Sleep(1000);
		Assert.True(Directory.Exists(ClientSourceDirectory));
		Assert.True(File.Exists(clientPathToCreateFile));
		
		Thread.Sleep(1000);
		Assert.True(Directory.Exists(ServerDestinationDirectory));
		Assert.True(File.Exists(serverPathToSyncFile));

		// Stop session and check if sync done correctly => new file was created
		cancellationTokenSource.Cancel();
		Thread.Sleep(1000);

		File.Delete(clientPathToCreateFile);
		File.Delete(serverPathToSyncFile);
		
		Assert.False(File.Exists(clientPathToCreateFile), "Expected on client side that file will be deleted");
		Assert.False(File.Exists(serverPathToSyncFile), "Expected on server side that file will be deleted");
	}

	[Fact]
	public void ConnectClientToServer_SynchronizeNewFileAndDelete()
	{
		var cancellationTokenSource = new CancellationTokenSource();
		var integration = new IntegrationProcessHandler(ClientSourceDirectory, ServerDestinationDirectory);

		var clientPathToCreateFile = Path.Combine(ClientSourceDirectory, TestFileName);
		var serverPathToSyncFile = Path.Combine(ServerDestinationDirectory, TestFileName);
		
		_ = integration.StartServerWithClient(cancellationTokenSource.Token);

		Thread.Sleep(1000);
		File.Create(clientPathToCreateFile).Close();
		
		Thread.Sleep(2000);
		Assert.True(Directory.Exists(ServerDestinationDirectory));
		Assert.True(File.Exists(serverPathToSyncFile));
		
		Thread.Sleep(1000);
		File.Delete(clientPathToCreateFile);
		Thread.Sleep(3000);
		
		// Stop session and check if sync done correctly => new file was created and deleted
		cancellationTokenSource.Cancel();
		
		Assert.False(File.Exists(clientPathToCreateFile), "Expected on client side that file will be deleted");
		Assert.False(File.Exists(serverPathToSyncFile), "Expected on server side that file will be deleted");
	}
}