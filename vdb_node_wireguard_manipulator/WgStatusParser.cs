using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("vdb_node_wireguard_manipulator.tests")]
[assembly:InternalsVisibleTo("vdb_node_api.tests")]
namespace vdb_node_wireguard_manipulator;


/* TODO: Create function for parsing peers from the StreamReader,
 * which is used in ProcessInfo.StandardOutput.
 */
internal static class WgStatusParser
{
	private static string GetValueFromLine(string text, int LineStartIndex)
	{
		int currentPos = LineStartIndex;
		currentPos = FindNextColon(text, currentPos) + 1;
		currentPos = FindNextNonWhitespace(text, currentPos);
		var valueBefore = FindNextRN(text, currentPos);
		var value = valueBefore > 0 ?
			Substring(text, currentPos, valueBefore - 1) : text.Substring(currentPos);
		return value;
	}
	private static string Substring(string s, int startIndex, int endIndex)
	{
		return s.Substring(startIndex, endIndex - startIndex + 1);
	}
	private static int FindNext(string text, int start, Func<char, bool> predicate)
	{
		for (int i = start; i < text.Length; i++)
		{
			if (predicate(text[i])) return i;
		}
		return -1;
	}
	private static int FindNextNonWhitespace(string text, int start)
	{
		return FindNext(text, start, x => !char.IsWhiteSpace(x));
	}
	private static int FindNextWhitespace(string text, int start)
	{
		return FindNext(text, start, char.IsWhiteSpace);
	}
	private static int FindNextNewLine(string text, int start)
	{
		return FindNext(text, start, x => x == '\n');
	}
	private static int FindNextRN(string text, int start)
	{
		return FindNext(text, start, x => x == '\r' || x == '\n');
	}
	private static int FindNextColon(string text, int start)
	{
		return FindNext(text, start, x => x == ':');
	}
	private static bool StartsWith(string text, string search, int start = 0)
	{
		if (search.Length > text.Length - start) return false;
		for (int i = 0; i < search.Length; i++)
		{
			if (search[i] != text[start + i]) return false;
		}
		return true;
	}

	private const string InterfaceStartString = @"interface";
	private const string InterfacePublicKeyString = @"public key";
	private const string PeerStartString = @"peer";
	private const string PeerEndpointString = @"endpoint";
	private const string PeerAllowedIpsString = @"allowed ips";
	private const string PeerLatestHandshakeString = @"latest handshake";
	private const string PeerTransferStringString = @"transfer";
	private const string PeerPersistentKeepaliveString = @"persistent keepalive";

	public static List<WgFullPeerInfo> ParsePeersFromWgShow(string output, out List<WgInterfaceInfo> interfaces)
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
		*/

		string value;
		List<WgFullPeerInfo> result = new();
		List<WgInterfaceInfo> foundInterfaces = new();
		WgInterfaceInfo interfaceInfo = null!;
		WgFullPeerInfo peerInfo = null!;

		for (int currentPos = 0; currentPos < output.Length; currentPos++)
		{
			currentPos = FindNextNonWhitespace(output, currentPos);
			if (currentPos == -1) break;

#if DEBUG
			char currChar; string leftStringStart;
			try
			{
				currChar = output[currentPos];
				leftStringStart = output.Substring(currentPos, Math.Min(256, output.Length - currentPos));
			}
			catch { }
#endif

			if (StartsWith(output, InterfaceStartString, currentPos)) // interface:
			{
				currentPos += InterfaceStartString.Length;
				if (interfaceInfo is not null) foundInterfaces.Add(interfaceInfo);
				interfaceInfo = new(null!, null!);

				value = GetValueFromLine(output, currentPos);
				interfaceInfo.Name = value;
				currentPos += value.Length + 1;
				continue;
			}
			if (StartsWith(output, InterfacePublicKeyString, currentPos))
			{
				currentPos += InterfacePublicKeyString.Length;

				value = GetValueFromLine(output, currentPos);
				interfaceInfo.PublicKey = value;
				currentPos += value.Length + 1;
				continue;
			}
			if (StartsWith(output, PeerStartString, currentPos))
			{
				currentPos += PeerStartString.Length;
				if (peerInfo is not null) result.Add(peerInfo);
				peerInfo = new(interfaceInfo, null!, null, null!, null, null, null);

				value = GetValueFromLine(output, currentPos);
				peerInfo.PublicKey = value;
				currentPos += value.Length + 1;
				continue;
			}
			if (StartsWith(output, PeerEndpointString, currentPos))
			{
				currentPos += PeerEndpointString.Length;
				value = GetValueFromLine(output, currentPos);
				peerInfo.Endpoint = value;
				currentPos += value.Length + 1;
				continue;
			}
			if (StartsWith(output, PeerAllowedIpsString, currentPos))
			{
				currentPos += PeerAllowedIpsString.Length;
				value = GetValueFromLine(output, currentPos);
				peerInfo.AllowedIps = value;
				currentPos += value.Length + 1;
				continue;
			}
			if (StartsWith(output, PeerLatestHandshakeString, currentPos))
			{
				currentPos += PeerLatestHandshakeString.Length;
				value = GetValueFromLine(output, currentPos);
				peerInfo.LatestHandshake = value;
				currentPos += value.Length + 1;
				continue;
			}
			if (StartsWith(output, PeerTransferStringString, currentPos))
			{
				currentPos += PeerTransferStringString.Length;
				value = GetValueFromLine(output, currentPos);
				peerInfo.Transfer = value;
				currentPos += value.Length + 1;
				continue;
			}
			if (StartsWith(output, PeerPersistentKeepaliveString, currentPos))
			{
				currentPos += PeerPersistentKeepaliveString.Length;
				value = GetValueFromLine(output, currentPos);
				peerInfo.PersistentKeepalive = value;
				currentPos += value.Length + 1;
				continue;
			}

			currentPos = FindNextNewLine(output, currentPos + 1);
		}

		if (peerInfo is not null)
		{
			if (result.Count == 0)
			{
				result.Add(peerInfo);
			}
			else if (result[result.Count - 1].PublicKey != peerInfo.PublicKey)
			{
				result.Add(peerInfo);
			}
		}
		if (interfaceInfo is not null)
		{
			if (foundInterfaces.Count == 0)
			{
				foundInterfaces.Add(interfaceInfo);
			}
			else if (foundInterfaces[foundInterfaces.Count - 1].PublicKey != interfaceInfo.PublicKey)
			{
				foundInterfaces.Add(interfaceInfo);
			}
		}

		interfaces = foundInterfaces;
		return result;
	}

	[Obsolete($"For unknown reason, this method goes much slower than not 'Shortly', " +
		$"so just use " + nameof(ParsePeersFromWgShow) + "(output).Select(" +
		nameof(WgShortPeerInfo) + "." + nameof(WgShortPeerInfo.FromFullInfo) + "). " +
		"This method is planned to be investigated so not removed.")]
	public static List<WgShortPeerInfo> ParsePeersFromWgShowShortly(string output)
	{
		string value;
		List<WgShortPeerInfo> result = new();
		WgShortPeerInfo peerInfo = null!;

		for (int currentPos = 0; currentPos < output.Length; currentPos++)
		{
			currentPos = FindNextNonWhitespace(output, currentPos);
			if (currentPos == -1) break;

#if DEBUG
			char currChar; string leftStringStart;
			try
			{
				currChar = output[currentPos];
				leftStringStart = output.Substring(currentPos, Math.Min(256, output.Length - currentPos));
			}
			catch { }
#endif

			if(StartsWith(output, PeerStartString, currentPos))
			{
				currentPos += PeerStartString.Length;
				if (peerInfo is not null) result.Add(peerInfo);
				peerInfo = new(GetValueFromLine(output, currentPos),int.MaxValue);

				currentPos += peerInfo.PublicKey.Length + 1;
			}
			else if(StartsWith(output, PeerLatestHandshakeString, currentPos))
			{
				currentPos += PeerLatestHandshakeString.Length;
				value = GetValueFromLine(output, currentPos);
				peerInfo.HandshakeSecondsAgo = HandshakeToSecond(value);
				currentPos += value.Length + 1;
			}
			else
			{
				// just skipping
				currentPos = FindNextRN(output, currentPos);
				if (currentPos == -1) break;
			}
		}

		if (peerInfo is not null)
		{
			if (result.Count == 0)
			{
				result.Add(peerInfo);
			}
			else if (result[result.Count - 1].PublicKey != peerInfo.PublicKey)
			{
				result.Add(peerInfo);
			}
		}

		return result;
	}

	internal static int HandshakeToSecond(string latestHanshake)
	{
		int resultSecs = 0;

		for (int i = 0; i < latestHanshake.Length; i++)
		{
			int currValueStart = FindNext((string)latestHanshake, i, char.IsDigit);
			if (currValueStart == -1) break;
			int afterValueSpace = FindNext((string)latestHanshake, currValueStart, char.IsWhiteSpace);

			var currValue = int.Parse(latestHanshake.Substring(currValueStart, afterValueSpace - currValueStart));

			var signatureStart = FindNext((string)latestHanshake, afterValueSpace + 1, char.IsLetter);
			var afterSignatureSpace = FindNext((string)latestHanshake, signatureStart, char.IsWhiteSpace);

			var currSig = latestHanshake.Substring(signatureStart, afterSignatureSpace - signatureStart);

			const int sec = 1;
			const int min = 60 * sec;
			const int hou = 60 * min;
			const int day = 24 * hou;

			if (currSig.StartsWith("day"))
				resultSecs += currValue * day;
			else if (currSig.StartsWith("hour"))
				resultSecs += currValue * hou;
			else if (currSig.StartsWith("minute"))
				resultSecs += currValue * min;
			else if (currSig.StartsWith("second"))
				resultSecs += currValue * sec;

			i = afterSignatureSpace;
		}

		return resultSecs;
	}

}

