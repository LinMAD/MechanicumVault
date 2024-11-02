using MechanicumVault.App.Client.Common.Mode;

namespace MechanicumVault.App.Client
{
	public class Program
	{
		static void Main(string[] args)
		{
			var client = new Common.Client(args);
			client.Run();
		}
	}
}