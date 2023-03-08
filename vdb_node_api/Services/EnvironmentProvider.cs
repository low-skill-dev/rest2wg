namespace vdb_node_api.Services
{
	public class EnvironmentProvider
	{
		const string ENV_ALLOW_NOAUTH = "REST2WG_ALLOW_NOAUTH";
		const string ENV_AUTH_KEYHASH = "REST2WG_AUTH_KEYHASH_BASE64";

		public bool ALLOW_NOAUTH { get; init; }
		public byte[]? AUTH_KEYHASH { get; init; } = null;

		public EnvironmentProvider(ILogger<EnvironmentProvider>? logger)
		{
			this.ALLOW_NOAUTH = Environment.GetEnvironmentVariable(ENV_ALLOW_NOAUTH)
				?.Equals("true", StringComparison.InvariantCultureIgnoreCase) ?? false;
			logger?.LogInformation($"{nameof(ALLOW_NOAUTH)}={ALLOW_NOAUTH}.");

			var key = Environment.GetEnvironmentVariable(ENV_AUTH_KEYHASH);
			if (key is not null) {
				var buf = new byte[512];
				if(Convert.TryFromBase64String(key,buf, out var bytesNum)) {
					if (bytesNum>0 && bytesNum <= 512)
					{
						AUTH_KEYHASH = new byte[bytesNum];
						buf.CopyTo(AUTH_KEYHASH, 0);
					}
				}
			}
			var showedKey = AUTH_KEYHASH is null ? "null" : "(hidden)";
			logger?.LogInformation($"{nameof(AUTH_KEYHASH)}={showedKey}.");
		}
	}
}
