namespace Models;

public class WgAddPeerResponse
{
    public string AllowedIp { get; init; }
    public string InterfacePubkey { get; init; }

    public WgAddPeerResponse(string allowedIp, string interfacePubkey)
    {
		AllowedIp = allowedIp;
		InterfacePubkey = interfacePubkey;
    }
}
