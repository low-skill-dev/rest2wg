namespace vdb_node_api.Models
{
	public class AddPeerResponse
	{
		public string AllowedIps { get; set; }

		public AddPeerResponse(string allowedIps)
		{
			this.AllowedIps = allowedIps;
		}
	}
}
