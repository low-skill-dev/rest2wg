namespace vdb_node_api.Models.Runtime
{
	public class MasterAccountServiceSettings
	{
		public int KeyLengthBytes { get; set; } = 128;
		public int MaxNumberOfMasterAccounts { get; set; } = 1024;
		public int MaxNumberOfDynamicMasterAccounts { get; set; } = 1024;

		public TimeSpan MaxLifeSpanOfDynamicMasterAccount { get; set; } = TimeSpan.MaxValue; //long.MaxValue
		public TimeSpan MaxLifeSpanAfterLastAccessOfDynamicMasterAccount { get; set; } = TimeSpan.FromDays(1);

		public bool IgnoreKeyDuplicates { get; set; } = false;

		public TimeSpan DynamicAccountsCollectionCompressionMinInterval { get; set; } = TimeSpan.FromMinutes(1);
	}
}
