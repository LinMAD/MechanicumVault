namespace MechanicumVault.Core.Exceptions;

/// <summary>
/// Exception that indicates run time execution issues.
/// </summary>
/// <param name="message"></param>
public class RuntimeException : MechanicumVaultException
{
	public RuntimeException(string? message) : base(message)
	{
	}

	public RuntimeException(string? message, Exception? innerException) : base(message, innerException)
	{
	}
}
