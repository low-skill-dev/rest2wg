namespace vdb_node_api.Models.Api;

public class PeerActionRequest
{
    public string PublicKey { get; set; }

    public PeerActionRequest(string publicKey)
    {
        PublicKey = publicKey;
    }

    public AddPeerResponse CreateResponse(string allowedIps, string? interfacePublicKey = null)
    {
        return new(PublicKey, allowedIps, interfacePublicKey);
    }
}
