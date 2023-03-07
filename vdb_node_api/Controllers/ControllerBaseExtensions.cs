using Microsoft.AspNetCore.Mvc;

namespace vdb_node_api.Controllers;

public static class ControllerBaseExtensions
{
	[NonAction]
	public static bool ValidatePubkey(this ControllerBase ctr, string pk)
	{
		return !string.IsNullOrWhiteSpace(pk)
			&& pk.Length < 1024
			&& Convert.TryFromBase64String(pk, new byte[pk.Length], out _);
	}
}
