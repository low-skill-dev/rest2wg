namespace vdb_node_api.Models
{
	public class InterfaceActionRequest
	{
		public string PublicKey { get; set; }

		public InterfaceActionRequest(string publicKey)
		{
			PublicKey = publicKey;
		}
	}
}
