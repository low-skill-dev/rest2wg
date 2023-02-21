namespace vdb_node_api.Models.Runtime
{
	public class ApplicationAccountServiceSettings
	{
		public int KeyLengthBytes { get; set; } = 64; // 2^(64*8) variants
		public int MaxNumberOfApplicationAccounts { get; set; } = 32768; // like for the future?

		public bool GenerateRandomNameForAccounts { get; set; } = true;

		public double AccessNotBeforeFromUtcNowSeconds { get; set; } = 0;
		public double AccessNotAfterFromUtcNowSeconds { get; set; } = 600;	
		
		public double RefreshNotBeforeFromUtcNowSeconds { get; set; } = 60;
		public double RefreshNotAfterFromUtcNowSeconds { get; set; } = TimeSpan.FromDays(60).TotalSeconds;

		public double AccountsDeletionIntervalSeconds { get; set; } = TimeSpan.FromMinutes(1).TotalSeconds;
	}
}
