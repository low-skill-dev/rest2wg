using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vdb_node_wireguard_manipulator
{
	public class WireguardInterfaceInfo
	{
		public string Name;
		public string PublicKey;

		public WireguardInterfaceInfo(string name, string publicKey)
		{
			this.Name = name;
			this.PublicKey = publicKey;
		}

		public static WireguardInterfaceInfo ParseFromWgOutput(IEnumerable<string> lines)
		{
			/*
				interface: wg0
				public key: Kq0LygX5ESfSpIDQO0k4dGSCnOAIZlJDPFacBeOBMCE=
				private key: (hidden)
				listening port: 51820
			*/

			var interfaceNameIter = lines.SkipWhile(x => !x.TrimStart().StartsWith("interface"));
			var interfaceName = interfaceNameIter.First().Split(':').Last().Trim();

			var interfacePkIter = interfaceNameIter.SkipWhile(x => !x.TrimStart().StartsWith("public key"));
			var interfacePk = interfacePkIter.First().Split(':').Last().Trim();

			return new WireguardInterfaceInfo(interfaceName, interfacePk);
		}
	}
}
