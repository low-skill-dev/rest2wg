using System.Diagnostics;

namespace vdb_node_wireguard_manipulator;

public static class WgCommandsExecutor
{
	public static string LastSeenInterfacePubkey => WgStatusStreamParser.LastSeenInterfacePubkey;

	private static async Task<string> RunCommand(string command, string fileName = @"wg")
	{
		return await (await RunCommandStream(command, fileName)).ReadToEndAsync();
	}

	private static async Task<StreamReader> RunCommandStream(string command, string fileName = @"wg")
	{
		var psi = new ProcessStartInfo();
		psi.FileName = fileName;

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

		return process.StandardOutput;
	}

	private static string GetAddPeerCommand(string pubKey, string allowedIps)
	{
		return $"set wg0 peer \"{pubKey}\" allowed-ips {allowedIps}";
	}
	private static string GetRemovePeerCommand(string pubKey)
	{
		return $"set wg0 peer \"{pubKey}\" remove";
	}
	private static string GetWgShowCommand(string wgInterfaceName = null!)
	{
		return wgInterfaceName is null ?
			"show" : $"show {wgInterfaceName}";
	}

	public static async Task<string> AddPeer(string pubKey, string allowedIps)
	{
		return await RunCommand(GetAddPeerCommand(pubKey, allowedIps));
	}
	public static async Task<string> RemovePeer(string pubKey)
	{
		return await RunCommand(GetRemovePeerCommand(pubKey));
	}

	public static async Task<IEnumerator<WgShortPeerInfo>> GetPeersListEnumerator()
	{
		return WgStatusStreamParser.ParsePeersFromStreamShortly(
			await RunCommandStream(GetWgShowCommand()));
	}
}
