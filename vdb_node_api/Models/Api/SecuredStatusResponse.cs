namespace vdb_node_api.Models.Api;

public class SecuredStatusResponse
{
	public string AuthKeyHmacSha512Base64 { get; init; }

	public SecuredStatusResponse(string authKeyHmacSha512Base64)
	{
		this.AuthKeyHmacSha512Base64 = authKeyHmacSha512Base64;
	}
}
