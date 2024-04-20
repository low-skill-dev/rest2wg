using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Models;

#pragma warning disable CS8618

public class WgDevice
{
	public string Name { get; set; }
	public int Port { get; set; }
	public string Pubkey { get; set; }
	public string Network { get; set; }
	public int PeersCount { get; set; }
	public long ReceivedBytes { get; set; }
	public long TransmittedBytes { get; set; }
}
