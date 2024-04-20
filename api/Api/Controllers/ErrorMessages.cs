namespace Api.Controllers;

public static class ErrorMessages
{
	public const string WireguardPublicKeyFormatInvalid =
		@"Wireguard public key must be exact 256-bits long base64-encoded array.";
	public const string WireguardPublicKeyAlreadyExists =
		@"Such wireguard public key is already present on the server. Consider generating another one.";
	public const string DevicesLimitReached =
		@"Devices limit reached.";
	public const string InternalErrorAddingNewPeer =
		@"Internal wireguard error occurred while adding new peer.";
	public const string InternalErrorDeletingPeer =
		@"Internal wireguard error occurred while deleting peer.";
	public const string PeerPubkeyNotFound =
		@"Passed wireguard public key was not found as a peer.";
}

