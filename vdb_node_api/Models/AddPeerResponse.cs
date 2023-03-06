namespace vdb_node_api.Models
{
	public class AddPeerResponse
	{
		public string PublicKey { get; set; }
		public string AllowedIps { get; set; }

		public AddPeerResponse(string publicKey ,string allowedIps)
		{
			this.PublicKey = publicKey;
			this.AllowedIps = allowedIps;
		}
	}
}
