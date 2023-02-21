namespace vdb_node_api.Models.Api.Application



{
	public class LoginResponse
	{
		public string AccessToken { get; set; }
		public string RefreshToken { get; set; }

		public LoginResponse(string accessToken, string refreshToken)
		{
			this.AccessToken = accessToken;
			this.RefreshToken = refreshToken;
		}
	}
}
