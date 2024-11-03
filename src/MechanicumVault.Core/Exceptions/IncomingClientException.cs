namespace MechanicumVault.Core.Exceptions;

public class IncomingClientException : MechanicumVaultException
{
	public IncomingClientException(string? message) : base(message)
	{
	}

	public IncomingClientException(string? message, Exception? innerException) : base(message, innerException)
	{
	}
}