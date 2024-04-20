namespace Models;

#pragma warning disable CS8618

public class WgAddPeerRequest
{
	public string Pubkey { get; set; }
	public string AllowedIp { get; set; }
}
