namespace MechanicumVault.Core.Exceptions;

/// <summary>
/// Generic application exception.
/// </summary>
public class MechanicumVaultException(string message) : Exception(message);