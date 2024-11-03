namespace MechanicumVault.EndToEnd.Generators;

public class PortGenerator
{
	public static int Generate(int minPort, int maxPort)
	{
		Random random = new Random();
		return random.Next(minPort, maxPort + 1);
	}
}