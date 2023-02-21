namespace vdb_node_api.Models.Api.Application
{
	public class LoginRequest
	{
		public string ApiKey { get; set; }

		public LoginRequest(string apiKey)
		{
			ApiKey = apiKey;
		}
	}
}
