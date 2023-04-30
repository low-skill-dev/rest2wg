namespace vdb_node_api.Controllers;

public static class ErrorMessages
{
	public const string RefreshJwtIsInvalid =
		@"Refresh JWT format is invalid.";
	public const string RefreshJwtIsNotFound =
		@"Refresh JWT is not found on the server.";
	public const string RefreshJwtUserNotFound =
		@"Refresh JWT is valid but the user it was issued to is not found on the server.";
	public const string RefreshJwtIsExpectedInCookiesXorBody =
		@"Refresh JWT must be provided in cookies XOR request body.";
	public const string RefreshJwtIsExpectedInCookies =
		@"Refresh JWT must be provided in cookies strictly.";

	public const string AccessJwtUserNotFound =
		@"Access JWT is valid but the user it was issued to is not found on the server.";

	public const string WireguardPublicKeyFormatInvalid =
		@"Wireguard public key must be exact 256-bits long base64-encoded array.";
	public const string WireguardPublicKeyAlreadyExists =
		@"Such wireguard public key is already present on the server. Consider generating another one.";
	public const string DevicesLimitReached =
		@"Devices limit reached.";
}

