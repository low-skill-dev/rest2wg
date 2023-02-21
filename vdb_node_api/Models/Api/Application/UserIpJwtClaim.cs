using System.Net;

namespace vdb_node_api.Models.Api.Application
{
	public class UserIpJwtClaim
	{
		public const string ValueType = nameof(UserIpJwtClaim);
		public IPAddress Value { get; set; }

		public UserIpJwtClaim(IPAddress address)
		{
			Value = address;
		}
	}
}
