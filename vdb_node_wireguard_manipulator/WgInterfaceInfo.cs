using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vdb_node_wireguard_manipulator
{
	public class WgInterfaceInfo
	{
		public string Name;
		public string PublicKey;

		public WgInterfaceInfo(string name, string publicKey)
		{
			this.Name = name;
			this.PublicKey = publicKey;
		}
	}
}
