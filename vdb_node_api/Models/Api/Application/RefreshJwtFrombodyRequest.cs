namespace vdb_node_api.Models.Api.Application
{
	public class RefreshJwtFrombodyRequest
	{
		public string RefreshJwt { get; set; }

		public RefreshJwtFrombodyRequest(string refreshJwt)
		{
			this.RefreshJwt = refreshJwt;
		}
	}
}
