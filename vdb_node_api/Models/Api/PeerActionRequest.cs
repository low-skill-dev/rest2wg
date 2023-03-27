namespace vdb_node_api.Models.Api;

public class PeerActionRequest
{
    public string PublicKey { get; init; }

    public PeerActionRequest(string publicKey)
    {
        PublicKey = publicKey;
    }

    public AddPeerResponse CreateResponse(string allowedIps, string interfacePublicKey)
    {
        return new(allowedIps, interfacePublicKey);
    }
}
