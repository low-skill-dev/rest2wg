namespace vdb_node_api.Models
{
	public class PeerActionRequest
	{
		public string PublicKey { get; set; }

		public PeerActionRequest(string publicKey)
		{
			PublicKey = publicKey;
		}
	}
}
