namespace vdb_node_api.Models.Api.Application
{
	public class RenewApiKeyResponse
	{
		public string NewApiKey { get; set; }

		public RenewApiKeyResponse(string newApiKey)
		{
			NewApiKey = newApiKey;
		}
	}
}
