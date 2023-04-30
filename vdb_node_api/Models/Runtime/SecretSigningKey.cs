namespace vdb_node_api.Models.Runtime;

public class SecretSigningKey
{
	public string KeyBase64 { get; init; }

	public SecretSigningKey(string keyBase64)
	{
		KeyBase64 = keyBase64;
	}
}
