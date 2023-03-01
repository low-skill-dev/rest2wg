using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vdb_node_wireguard_manipulator
{
	public class CommandsExecutor
	{
		public string WgSericeName = "wg0";

		public CommandsExecutor()
		{

		}

		private async Task<string> RunCommandWithBash(string command)
		{
			var psi = new ProcessStartInfo();
			psi.FileName = "/bin/sh";
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
			return await RunCommandWithBash(command);
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
