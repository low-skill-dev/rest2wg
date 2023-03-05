using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace vdb_node_wireguard_manipulator
{
	public class WgFullPeerInfo
	{
		public WgInterfaceInfo? Interface { get; set; }

		public string PublicKey;
		public string? Endpoint;
		public string AllowedIps;
		public string? LatestHandshake;
		public string? Transfer;
		public string? PersistentKeepalive;

		public WgFullPeerInfo(WgInterfaceInfo? @interface, string publicKey, string? endpoint, string allowedIps, string? latestHandshake, string? transfer, string? persistentKeepalive)
		{
			this.Interface = @interface;
			this.PublicKey = publicKey;
			this.Endpoint = endpoint;
			this.AllowedIps = allowedIps;
			this.LatestHandshake = latestHandshake;
			this.Transfer = transfer;
			this.PersistentKeepalive = persistentKeepalive;
		}
	}
}
