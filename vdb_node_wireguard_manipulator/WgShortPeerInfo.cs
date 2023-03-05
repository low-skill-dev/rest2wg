using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vdb_node_wireguard_manipulator;

public class WgShortPeerInfo
{
	public string PublicKey;
	public int HandshakeSecondsAgo = int.MaxValue;

	public WgShortPeerInfo(string publicKey, int handshakeSecondsAgo)
	{
		this.PublicKey = publicKey;
		this.HandshakeSecondsAgo = handshakeSecondsAgo;
	}

	public static WgShortPeerInfo FromFullInfo(WgFullPeerInfo full)
	 => new(full.PublicKey, full.LatestHandshake is null ?
		 int.MaxValue : WgStatusParser.HandshakeToSecond(full.LatestHandshake));
}

