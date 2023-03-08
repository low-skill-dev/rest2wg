namespace vdb_node_api.Models.ServicesSettings
{
	public class PeersBackgroundServiceSettings
	{
		public int PeersRenewIntervalSeconds { get; set; } = 3600;
		public int HandshakeAgoLimitSeconds { get; set; } = 600;

		public PeersBackgroundServiceSettings(int peersRenewIntervalSeconds, int handshakeAgoLimitSeconds)
		{
			this.PeersRenewIntervalSeconds = peersRenewIntervalSeconds;
			this.HandshakeAgoLimitSeconds = handshakeAgoLimitSeconds;
		}
	}
}
