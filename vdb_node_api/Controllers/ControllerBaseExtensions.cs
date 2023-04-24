using Microsoft.AspNetCore.Mvc;

namespace vdb_node_api.Controllers;


#pragma warning disable IDE0060
public static class ControllerBaseExtensions
{
	[NonAction]
	public static bool ValidatePubkey(this ControllerBase ctr, string pk, int strictBytesCount = 256/8)
	{
		return !string.IsNullOrWhiteSpace(pk)
			&& pk.Length <= (strictBytesCount * 4 / 3 + 3)
			&& Convert.TryFromBase64String(pk, new byte[strictBytesCount], out var bytesCount)
			&& (strictBytesCount == -1 || bytesCount == strictBytesCount);
	}

	[NonAction]
	public static bool ValidatePubkeyWithoutDecoding(this ControllerBase ctr, string pk, int strictBytesCount = 256 / 8)
	{
		return !string.IsNullOrWhiteSpace(pk)
			&& pk.Length <= (strictBytesCount * 4 / 3 + 3);
	}

	/// <exception cref="NullReferenceException"/>
	[NonAction]
	public static async IAsyncEnumerable<T> IncapsulateEnumerator<T>(this ControllerBase ctr, IAsyncEnumerator<T> enumerator)
	{
		while(await enumerator.MoveNextAsync())
			yield return enumerator.Current;
	}
}
