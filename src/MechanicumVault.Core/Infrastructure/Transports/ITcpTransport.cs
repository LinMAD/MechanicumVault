namespace MechanicumVault.Core.Infrastructure.Transports;

/// <summary>
/// Interface for Tcp data transportations for Adapter and Port implementation.
/// </summary>
public interface ITcpTransport
{
	/// <summary>
	/// Must establish connection between P2P.
	/// </summary>
	public void Connect();

	/// <summary>
	/// Must handle connection shutdown.
	/// </summary>
	public void Close();
}
