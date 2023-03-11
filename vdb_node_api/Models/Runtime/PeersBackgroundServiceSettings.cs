namespace vdb_node_api.Models.Runtime;

public class PeersBackgroundServiceSettings
{
	public int PeersRenewIntervalSeconds { get; set; } = 3600;
	public int HandshakeAgoLimitSeconds { get; set; } = 600;

	public PeersBackgroundServiceSettings(int peersRenewIntervalSeconds, int handshakeAgoLimitSeconds)
	{
		PeersRenewIntervalSeconds = peersRenewIntervalSeconds;
		HandshakeAgoLimitSeconds = handshakeAgoLimitSeconds;
	}
}
