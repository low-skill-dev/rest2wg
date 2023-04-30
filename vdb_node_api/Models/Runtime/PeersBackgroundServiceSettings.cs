namespace vdb_node_api.Models.Runtime;

public class PeersBackgroundServiceSettings
{
	public int PeersRenewIntervalSeconds { get; set; } = 0;
	public int HandshakeAgoLimitSeconds { get; set; } = 0;
	public bool SaveConfigOnEveryChange { get; set; } = false;

	public PeersBackgroundServiceSettings(int peersRenewIntervalSeconds, int handshakeAgoLimitSeconds)
	{
		PeersRenewIntervalSeconds = peersRenewIntervalSeconds;
		HandshakeAgoLimitSeconds = handshakeAgoLimitSeconds;
	}
}
