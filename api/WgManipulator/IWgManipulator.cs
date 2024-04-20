using Models;

namespace WgManipulator;

public interface IWgManipulator
{
	/// <returns>
	/// All peers.
	/// </returns>
	public Task<List<WgPeer>> GetPeers();

	/// <returns>
	/// Successfully added peers count.
	/// </returns>
	public Task<int> AddPeers(IEnumerable<WgAddPeerRequest> peers);

	/// <returns>
	/// Successfully deleted peers.
	/// </returns>
	public Task<List<WgPeer>> DeletePeers(IEnumerable<string> pubkeys);

	/// <returns>
	/// Base64-encoded wg interface public key.
	/// </returns>
	public Task<WgDevice> GetInterfaceInfo();
}
