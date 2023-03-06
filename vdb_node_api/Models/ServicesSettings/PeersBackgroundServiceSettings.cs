namespace vdb_node_api.Models.ServicesSettings
{
	public class PeersBackgroundServiceSettings
	{
		public int PeersRenewIntervalSeconds { get; set; }
		public int HandshakeAgoLimitSeconds { get; set; }

		public PeersBackgroundServiceSettings(int peersRenewIntervalSeconds, int handshakeAgoLimitSeconds)
		{
			this.PeersRenewIntervalSeconds = peersRenewIntervalSeconds;
			this.HandshakeAgoLimitSeconds = handshakeAgoLimitSeconds;
		}
	}
}
