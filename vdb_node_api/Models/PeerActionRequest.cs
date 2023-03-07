namespace vdb_node_api.Models
{
	public class PeerActionRequest
	{
		public string PublicKey { get; set; }

		public PeerActionRequest(string publicKey)
		{
			PublicKey = publicKey;
		}

		public AddPeerResponse CreateAddResponse(string allowedIps, string? interfacePublicKey=null)
		{
			return new(this.PublicKey, allowedIps, interfacePublicKey);
		}
	}
}
