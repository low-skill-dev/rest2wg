using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace vdb_node_wireguard_manipulator.tests
{
	public class WireguardInterfaceInfoTests
	{
		[Fact]
		public void CanParseInterface()
		{
			var output = "interface: wg0\r\n  public key: Kq0LygX5ESfSpIDQO0k4dGSCnOAIZlJDPFacBeOBMCE=\r\n  private key: (hidden)\r\n  listening port: 51820\r\n\r\npeer: zO2jr59gppRHrPlaVshrm/AH8YUzVGToGDvoeS8j4CI=\r\n  endpoint: 31.173.84.131:21260\r\n  allowed ips: 10.8.0.101/32\r\n  latest handshake: 9 hours, 18 minutes, 25 seconds ago\r\n  transfer: 20.19 MiB received, 683.31 MiB sent\r\n  persistent keepalive: every 25 seconds\r\n\r\npeer: 3Sg1yyugDmxoYFnEu+1i280QZpgD8tAHHHXUZPWDSk4=\r\n  endpoint: 5.188.98.224:60796\r\n  allowed ips: 10.8.0.115/32\r\n  latest handshake: 1 day, 7 hours, 7 minutes, 11 seconds ago\r\n  transfer: 116.43 MiB received, 2.32 GiB sent\r\n  persistent keepalive: every 25 seconds\r\n\r\npeer: MTy1q7mVlyuUkCnvz0ZrXPCTSzhbjNMbhzfJU/gSsF8=\r\n  endpoint: 31.173.82.117:25566\r\n  allowed ips: 10.8.0.100/32\r\n  latest handshake: 1 day, 13 hours, 13 minutes, 53 seconds ago\r\n  transfer: 13.19 MiB received, 176.12 MiB sent\r\n  persistent keepalive: every 25 seconds\r\n\r\npeer: V1evhjZQhhygsdrtriAb5AzuUuE8SkQNHJ4YAYdxGQs=\r\n  allowed ips: 10.0.0.0/32\r\n  persistent keepalive: every 25 seconds\r\n\r\npeer: HTqjO7TgQ1Mke2PKPtC2XGrAAK1INyH6j9ke7cn8cQU=\r\n  allowed ips: 10.1.1.1/32\r\n  persistent keepalive: every 25 seconds\r\n\r\npeer: LwkXubXSXLOzQPK0a6PQp1DWz08lsfk+Oyp7s1056H8=\r\n  allowed ips: 10.6.6.6/32\r\n  persistent keepalive: every 25 seconds\r\n\r\npeer: 9c0dwlfFnTPuVon4au2l3mx94jme2czT4CSkd8ZbODM=\r\n  allowed ips: 10.64.64.64/32\r\n  persistent keepalive: every 25 seconds\r\n\r\npeer: /T3Yzw1oFJYYLDsC2bVG1UE2q2fuUPppSI+O3tr18Ek=\r\n  allowed ips: 10.255.255.255/32\r\n  persistent keepalive: every 25 seconds\r\n\r\npeer: rLb4WX7XDfOIs69hO1CGUrQuUqn42NT7OFAnttnupGA=\r\n  allowed ips: 10.8.0.102/32\r\n  persistent keepalive: every 25 seconds\r\n\r\npeer: hSDPy+JWh+IYRK72AThfvET+N8B8/i7SBWvTq1m8Y1I=\r\n  allowed ips: 10.8.0.110/32\r\n  persistent keepalive: every 25 seconds";
			var lines = output.Split(Environment.NewLine);
			var result = WireguardInterfaceInfo.ParseFromWgOutput(lines);

			Assert.Equal("wg0", result.Name);
			Assert.Equal("Kq0LygX5ESfSpIDQO0k4dGSCnOAIZlJDPFacBeOBMCE=", result.PublicKey);
		}
	}
}
