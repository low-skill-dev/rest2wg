namespace vdb_node_api.Models.Api.Master
{
	public class AppRegistrationRequest: AMasterRequest
	{
		public string? NewAppName { get; set; }

		public AppRegistrationRequest(string masterKey, string appName)
		{
			MasterKey = masterKey;
			NewAppName = appName;
		}
	}
}
