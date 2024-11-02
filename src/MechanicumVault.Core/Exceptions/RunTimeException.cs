namespace MechanicumVault.Core.Exceptions;

/// <summary>
/// Exception that indicates run time execution issues.
/// </summary>
/// <param name="message"></param>
public class RunTimeException(string message) : MechanicumVaultException(message);
