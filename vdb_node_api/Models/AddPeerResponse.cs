namespace vdb_node_api.Models
{
	public class AddPeerResponse
	{
		public string PeerPublicKey { get; set; }
		public string AllowedIps { get; set; }

		public string? InterfacePublicKey { get; set; }

		public AddPeerResponse(string peerPublicKey, string allowedIps, string? interfacePublicKey=null)
		{
			this.PeerPublicKey = peerPublicKey;
			this.AllowedIps = allowedIps;
			this.InterfacePublicKey = interfacePublicKey;
		}
	}
}
