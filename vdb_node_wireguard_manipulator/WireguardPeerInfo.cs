using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vdb_node_wireguard_manipulator
{
	public class WireguardPeerInfo
	{
		public WireguardInterfaceInfo? Interface { get; set; }

		public string PublicKey;
		public string? Endpoint;
		public string AllowedIps;
		public string? LatestHandshake;
		public string? Transfer;
		public string? PersistentKeepalive;

		public WireguardPeerInfo(WireguardInterfaceInfo? @interface, string publicKey, string? endpoint, string allowedIps, string? latestHandshake, string? transfer, string? persistentKeepalive)
		{
			this.Interface = @interface;
			this.PublicKey = publicKey;
			this.Endpoint = endpoint;
			this.AllowedIps = allowedIps;
			this.LatestHandshake = latestHandshake;
			this.Transfer = transfer;
			this.PersistentKeepalive = persistentKeepalive;
		}

		public static List<WireguardPeerInfo> ParseFromFullOutput(string fullOutput)
		{
			/*
			interface: wg0
			public key: Kq0LygX5ESfSpIDQO0k4dGSCnOAIZlJDPFacBeOBMCE=
			private key: (hidden)
			listening port: 51820

			peer: zO2jr59gppRHrPlaVshrm/AH8YUzVGToGDvoeS8j4CI=
			  endpoint: 31.173.84.131:21260
			  allowed ips: 10.8.0.101/32
			  latest handshake: 9 hours, 18 minutes, 25 seconds ago
			  transfer: 20.19 MiB received, 683.31 MiB sent
			  persistent keepalive: every 25 seconds

			peer: MTy1q7mVlyuUkCnvz0ZrXPCTSzhbjNMbhzfJU/gSsF8=
			  endpoint: 31.173.82.117:25566
			  allowed ips: 10.8.0.100/32
			  latest handshake: 1 day, 13 hours, 13 minutes, 53 seconds ago
			  transfer: 13.19 MiB received, 176.12 MiB sent
			  persistent keepalive: every 25 seconds

			peer: V1evhjZQhhygsdrtriAb5AzuUuE8SkQNHJ4YAYdxGQs=
			  allowed ips: 10.0.0.0/32
			  persistent keepalive: every 25 seconds

			peer: HTqjO7TgQ1Mke2PKPtC2XGrAAK1INyH6j9ke7cn8cQU=
			  allowed ips: 10.1.1.1/32
			  persistent keepalive: every 25 seconds

			peer: LwkXubXSXLOzQPK0a6PQp1DWz08lsfk+Oyp7s1056H8=
			  allowed ips: 10.6.6.6/32
			  persistent keepalive: every 25 seconds

			peer: 9c0dwlfFnTPuVon4au2l3mx94jme2czT4CSkd8ZbODM=
			  allowed ips: 10.64.64.64/32
			  persistent keepalive: every 25 seconds

			peer: /T3Yzw1oFJYYLDsC2bVG1UE2q2fuUPppSI+O3tr18Ek=
			  allowed ips: 10.255.255.255/32
			  persistent keepalive: every 25 seconds

			peer: rLb4WX7XDfOIs69hO1CGUrQuUqn42NT7OFAnttnupGA=
			  allowed ips: 10.8.0.102/32
			  persistent keepalive: every 25 seconds

			peer: hSDPy+JWh+IYRK72AThfvET+N8B8/i7SBWvTq1m8Y1I=
			  allowed ips: 10.8.0.110/32
			  persistent keepalive: every 25 seconds
			root@disagreeable-spiders:~# 
*/

			var lines = fullOutput.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
			var interfaceInfo = WireguardInterfaceInfo.ParseFromWgOutput(lines);
			var peersBlock = lines.SkipWhile(x => !x.TrimStart().StartsWith("peer"));

			List<List<string>> peersLines = new(fullOutput.Length / 52 / 2); // approx.
			int prevLen = 0;
			while (true)
			{
				peersBlock = peersBlock.Skip(prevLen);
				if (!peersBlock.Any()) break;

				var peer = peersBlock.Skip(1).TakeWhile(x => !x.TrimStart().StartsWith("peer")).ToList();
				peer.Insert(0, peersBlock.First());
				peersLines.Add(peer);
				prevLen = peer.Count;
			}

			var parsed = peersLines.Select(ParseFromWgOutput).ToList();
			for (int i = 0; i < parsed.Count; i++) parsed[i].Interface = interfaceInfo;

			return parsed;
		}

		public static WireguardPeerInfo ParseFromWgOutput(IEnumerable<string> peerOutputLines)
		{
			// not effective, better to iterate, because the output order is knowed.
			var publicKey = FindAndExtractValue("peer", peerOutputLines)!; // must be not null
			var allowedIps = FindAndExtractValue("allowed ips", peerOutputLines)!; // must be not null
			var endpoint = FindAndExtractValue("endpoint", peerOutputLines);
			var latestHandshake = FindAndExtractValue("latest handshake", peerOutputLines);
			var transfer = FindAndExtractValue("transfer", peerOutputLines);
			var persistentKeepalive = FindAndExtractValue("persistent keepalive", peerOutputLines);

			return new(null, publicKey, endpoint, allowedIps, latestHandshake, transfer, persistentKeepalive);
		}

		private static string? FindAndExtractValue(string name, IEnumerable<string> lines)
		{
			return lines.FirstOrDefault(x => x.TrimStart().StartsWith(name))?.Split(':',2).Last().Trim() ?? null;
		}
	}
}
