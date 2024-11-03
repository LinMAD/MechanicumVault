namespace MechanicumVault.Core.Exceptions;

public class DeserializationException : MechanicumVaultException
{
	public DeserializationException()
	{
	}

	public DeserializationException(string? message) : base(message)
	{
	}

	public DeserializationException(string? message, Exception? innerException) : base(message, innerException)
	{
	}
}