using System.Runtime.Serialization;

namespace MechanicumVault.Core.Exceptions;

/// <summary>
/// Generic application exception.
/// </summary>
public class MechanicumVaultException : Exception
{
	public MechanicumVaultException()
	{
	}

	protected MechanicumVaultException(SerializationInfo info, StreamingContext context) : base(info, context)
	{
	}

	public MechanicumVaultException(string? message) : base(message)
	{
	}

	public MechanicumVaultException(string? message, Exception? innerException) : base(message, innerException)
	{
	}
}