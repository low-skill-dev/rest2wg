using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace WgManipulator.Models;

#pragma warning disable CS8618

internal class WgrestDevice
{
	[JsonPropertyName("name")]
	public string Name { get; set; }

	[JsonPropertyName("listen_port")]
	public int ListenPort { get; set; }

	[JsonPropertyName("public_key")]
	public string Pubkey { get; set; }

	[JsonPropertyName("networks")]
	public string[] Networks { get; set; }

	[JsonPropertyName("peers_count")]
	public int PeersCount { get; set; }

	[JsonPropertyName("total_receive_bytes")]
	public long ReceivedBytes { get; set; }

	[JsonPropertyName("total_transmit_bytes")]
	public long TransmittedBytes { get; set; }

	public WgDevice ToWgDevice()
	{
		return new()
		{
			Name = this.Name,
			Port = this.ListenPort,
			Pubkey = this.Pubkey,
			Network = this.Networks[0],
			PeersCount = this.PeersCount,
			ReceivedBytes = this.ReceivedBytes,
			TransmittedBytes = this.TransmittedBytes,
		};
	}
}
