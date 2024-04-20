using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace WgManipulator.Models;
internal class WgrestAddPeerRequest
{
	[JsonPropertyName("public_key")]
	public string Pubkey { get; set; }

	[JsonPropertyName("allowed_ips")]
	public string[] AllowedIps { get; set; }

	public static WgrestAddPeerRequest FromWgAddPeerRequest(WgAddPeerRequest r)
	{
		return new()
		{
			Pubkey = r.Pubkey,
			AllowedIps = [r.AllowedIp],
		};
	}
}
