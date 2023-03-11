namespace vdb_node_wireguard_manipulator;

public class WgInterfaceInfo
{
	public string? Name { get; set; }
	public string PublicKey { get; set; }

	public WgInterfaceInfo(string publicKey)
	{
		PublicKey = publicKey;
	}
}
