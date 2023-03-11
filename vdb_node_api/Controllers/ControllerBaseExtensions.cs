using Microsoft.AspNetCore.Mvc;

namespace vdb_node_api.Controllers;


#pragma warning disable IDE0060
public static class ControllerBaseExtensions
{
	[NonAction]
	public static bool ValidatePubkey(this ControllerBase ctr, string pk)
	{
		return !string.IsNullOrWhiteSpace(pk)
			&& pk.Length < 1024
			&& Convert.TryFromBase64String(pk, new byte[pk.Length], out _);
	}

	[NonAction]
	public static async IAsyncEnumerable<T> IncapsulateEnumerator<T>(this ControllerBase ctr, IAsyncEnumerator<T> enumerator)
	{
		while (await enumerator.MoveNextAsync())
			yield return enumerator.Current;
	}
}
