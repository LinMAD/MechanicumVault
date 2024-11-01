namespace MechanicumVault.Core.Exceptions;

/// <summary>
/// Generic exception for incorrect configurations.
/// </summary>
/// <param name="message">Error message</param>
public class InvalidConfiguration(string message) : MechanicumVaultException(message)
{
	// TODO Add collection of error message related to configuration later 
}
