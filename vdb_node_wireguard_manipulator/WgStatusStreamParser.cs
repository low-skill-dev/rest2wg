namespace vdb_node_wireguard_manipulator;

internal class WgStatusStreamParser : WgStatusParser
{
	public static string? LastSeenInterfacePubkey = null;
	public static IEnumerator<WgShortPeerInfo> ParsePeersFromStreamShortly(StreamReader output)
	{
		int currentPos;
		string value;
		WgShortPeerInfo peerInfo = null!;

		while (!output.EndOfStream)
		{
			string? line = output.ReadLine();
			if (line is null) break;
			currentPos = FindNextNonWhitespace(line, 0);
			if (currentPos == -1) continue;

			if (StartsWith(line, InterfacePublicKeyString, currentPos))
			{
				currentPos += InterfacePublicKeyString.Length;

				value = GetValueFromLine(line, currentPos);
				LastSeenInterfacePubkey = value;
				continue;
			}
			if (StartsWith(line, PeerStartString, currentPos))
			{
				currentPos += PeerStartString.Length;
				if (peerInfo is not null) yield return peerInfo;
				peerInfo = new(null!, null!, int.MaxValue);

				value = GetValueFromLine(line, currentPos);
				peerInfo.PublicKey = value;
				continue;
			}
			if (StartsWith(line, PeerAllowedIpsString, currentPos))
			{
				currentPos += PeerAllowedIpsString.Length;
				value = GetValueFromLine(line, currentPos);
				peerInfo.AllowedIps = value;
				continue;
			}
			if (StartsWith(line, PeerLatestHandshakeString, currentPos))
			{
				currentPos += PeerLatestHandshakeString.Length;
				value = GetValueFromLine(line, currentPos);
				peerInfo.HandshakeSecondsAgo = HandshakeToSecond(value);
				continue;
			}
		}

		if (peerInfo is not null)
			yield return peerInfo;

		yield break;
	}
}

