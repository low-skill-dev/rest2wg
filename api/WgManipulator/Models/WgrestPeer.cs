using Models;
using System.Text.Json.Serialization;

namespace WgManipulator.Models;

internal class WgrestPeer : WgrestAddPeerRequest
{
	[JsonPropertyName("last_handshake_time")]
	public DateTime LastHandshake { get; set; }

	[JsonPropertyName("receive_bytes")]
	public long ReceivedBytes { get; set; }

	[JsonPropertyName("transmit_bytes")]
	public long TransmittedBytes { get; set; }

	public WgPeer ToWgPeer()
	{
		return new()
		{
			Pubkey = this.Pubkey,
			AllowedIp = this.AllowedIps[0],
			LastHandshake = this.LastHandshake,
			ReceivedBytes = this.ReceivedBytes,
			TransmittedBytes = this.TransmittedBytes,
		};
	}
}
