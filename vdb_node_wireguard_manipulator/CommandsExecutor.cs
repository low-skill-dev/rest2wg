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
		public CommandsExecutor()
		{

		}

		private async Task<string> RunCommand(string command, bool runWithShell=false)
		{
			var psi = new ProcessStartInfo();
			if(runWithShell) psi.FileName = "/bin/sh";

			psi.Arguments = runWithShell? $"-c \"{command}\"" : command;
			psi.RedirectStandardOutput = true;
			psi.UseShellExecute = false;
			psi.CreateNoWindow = true;

			var process = Process.Start(psi);

			if (process is null)
			{
				throw new AggregateException("Unable to perform the command");
			}

			await process.WaitForExitAsync();

			var output = await process.StandardOutput.ReadToEndAsync();

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
		private string GetWgShowCommand(string wgInterfaceName=null!)
		{
			return wgInterfaceName is null ?
				"wg show" : $"wg show {wgInterfaceName}";
		}

		public async Task<string> AddPeer(string pubKey, string allowedIps)
		{
			return await RunCommand(GetAddPeerCommand(pubKey, allowedIps));
		}
		public async Task<string> RemovePeer(string pubKey)
		{
			return await RunCommand(GetRemovePeerCommand(pubKey));
		}
		public async Task<string> GetPeersListUnparsed()
		{
			return await RunCommand(GetWgShowCommand());
		}
		public async Task<(List<WgFullPeerInfo> peers, List<WgInterfaceInfo> interfaces)> GetPeersList()
		{
			var result =  WgStatusParser.ParsePeersFromWgShow(
				await RunCommand(GetWgShowCommand()), out var ifs);
			return (result, ifs);
		}

		public async Task<List<WgShortPeerInfo>> GetPeersListShortly()
		{
			return WgStatusParser.ParsePeersFromWgShow(
				await RunCommand(GetWgShowCommand()),out _)
				.Select(WgShortPeerInfo.FromFullInfo).ToList();
		}
	}
}
