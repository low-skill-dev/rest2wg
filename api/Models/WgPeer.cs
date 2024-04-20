namespace Models;

public class WgPeer : WgAddPeerRequest
{
	public long ReceivedBytes { get; set; }
	public long TransmittedBytes { get; set; }
	public DateTime LastHandshake { get; set; }
}
