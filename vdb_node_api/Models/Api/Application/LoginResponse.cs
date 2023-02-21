namespace vdb_node_api.Models.Api.Application



{
	public class LoginResponse
	{
		public string AccessJwt { get; set; }

		public LoginResponse(string accessJwt)
		{
			this.AccessJwt = accessJwt;
		}
	}
}
