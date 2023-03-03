using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace vdb_node_wireguard_manipulator
{
	public class CommandsExecutor
	{
		public string WgSericeName = "wg0";

		public struct WireguardPeerInfo
		{
			public struct WireguardInterfaceInfo
			{
				public string Name;
				public string PublicKey;
			}

			public WireguardInterfaceInfo? InterfaceInfo;

			public string PublicKey;
			public string? Endpoint;
			public string AllowedIps;
			public string? LatestHandshake;
			public string? Transfer;
			public string? PersistentKeepalive;
			//public IPAddress EndpointIp => new IPAddress(
			//	this.Endpoint.Split(new char[] { '.', ':' },4)
			//	.Select(byte.Parse).ToArray());

			public static List<WireguardPeerInfo> FromFullOutput(string fullOutput)
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

				var lines = fullOutput.Split(Environment.NewLine,StringSplitOptions.RemoveEmptyEntries);

				var interfaceNameIter = lines.SkipWhile(x => !x.TrimStart().StartsWith("interface"));
				var interfaceName = interfaceNameIter.First().Split(':').Last().Trim();
				var interfacePkIter = interfaceNameIter.SkipWhile(x => !x.TrimStart().StartsWith("public key"));
				var interfacePk = interfacePkIter.First().Split(":").Last().Trim();

				var interfaceInfo = new WireguardInterfaceInfo { Name = interfaceName, PublicKey = interfacePk };

				var peersBlock = interfacePkIter.SkipWhile(x => !x.TrimStart().StartsWith("peer"));

				List<List<string>> peersLines = new(fullOutput.Length/52/2); // approx.
				int prevLen = 0;
				while (true)
				{
					peersBlock = peersBlock.Skip(prevLen);
					var peer = peersBlock.Take(1).TakeWhile(x => !x.TrimStart().StartsWith("peer")).ToList();
					if (peer.Count == 0) break;
					peersLines.Add(peer);
					prevLen = peer.Count;
				}

				var parsed = peersLines.Select(FromPeerOutput).ToList();
				parsed.ForEach(x=> x.InterfaceInfo = interfaceInfo);

				return parsed;
			}

			public static WireguardPeerInfo FromPeerOutput(IEnumerable<string> peerOutputLines)
			{
				WireguardPeerInfo result = new() { InterfaceInfo = null };

				result.PublicKey = FindAndExtractValue("peer",peerOutputLines)!;
				result.AllowedIps = FindAndExtractValue("allowed ips", peerOutputLines)!;
				result.Endpoint = FindAndExtractValue("endpoint", peerOutputLines);
				result.LatestHandshake = FindAndExtractValue("latest handshake", peerOutputLines);
				result.Transfer = FindAndExtractValue("transfer", peerOutputLines);
				result.PersistentKeepalive = FindAndExtractValue("persistent keepalive", peerOutputLines);

				return result;
			}

			private static string? FindAndExtractValue(string name, IEnumerable<string> lines)
			{
				return lines.FirstOrDefault(x => x.TrimStart().StartsWith(name))?.Split(':').Last().Trim() ?? null;
			}


		}

		public CommandsExecutor()
		{

		}

		private async Task<string> RunCommand(string command, bool runWithShell=false)
		{
			var psi = new ProcessStartInfo();
			if(runWithShell) psi.FileName = "/bin/sh";

			psi.Arguments = command;
			psi.RedirectStandardOutput = true;
			psi.UseShellExecute = false;
			psi.CreateNoWindow = true;

			var process = Process.Start(psi);

			if (process is null)
			{
				throw new AggregateException("Unable to perform the command");
			}

			await process.WaitForExitAsync();

			var output = process.StandardOutput.ReadToEnd();

			return output;
		}

		private string GetAddPeerCommand(string pubKey, string allowedIps)
		{
			return $"wg set wg0 peer \"{pubKey}\" allowed-ips {allowedIps}";
		}
		private string GetRemovePeerCommand(string pubKey)
		{
			return $"wg set wg0 peer \"{pubKey}\" remove";
		}


		public async Task<string> ExecuteArbitraryCommand(string command)
		{
			return await RunCommand(command);
		}

		public async Task<string> AddPeer(string pubKey, string allowedIps)
		{
			return await ExecuteArbitraryCommand(GetAddPeerCommand(pubKey, allowedIps));
		}

		public async Task<string> RemovePeer(string pubKey)
		{
			return await ExecuteArbitraryCommand(GetRemovePeerCommand(pubKey));
		}
	}
}
