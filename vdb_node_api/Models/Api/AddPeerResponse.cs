namespace vdb_node_api.Models.Api;

public class AddPeerResponse
{
    public string AllowedIps { get; init; }
    public string InterfacePublicKey { get; init; }

    public AddPeerResponse(string allowedIps, string interfacePublicKey)
    {
        AllowedIps = allowedIps;
        InterfacePublicKey = interfacePublicKey;
    }
}
