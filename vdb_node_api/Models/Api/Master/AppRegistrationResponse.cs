namespace vdb_node_api.Models.Api.Master
{
	public class AppRegistrationResponse
	{
		public string GeneratedAppKeyBase64Url {get;set;}

		public AppRegistrationResponse(string generatedAppKeyBase64Url)
		{
			this.GeneratedAppKeyBase64Url = generatedAppKeyBase64Url;
		}
	}
}
