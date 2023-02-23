namespace vdb_node_api.Services
{
	/* Данный Singleton/Background-сервис служит для выполнения
	 * периодических действий. Прежде всего, он должен удалять
	 * хоты, которые уже не используются, но, по какой-то причине,
	 * не были удалены по команде главного сервера. Либо, а таковое 
	 * архитектурное решение может быть принято в будущем, главный 
	 * сервер в принципе может не заниматься удалением устаревших 
	 * соединений.
	 */
	public sealed class PeersBackgroundService:BackgroundService
	{
		private readonly IpDedicationService _ipService;
		public PeersBackgroundService(IpDedicationService ipService)
		{
			_ipService = ipService;
		}

		protected override Task ExecuteAsync(CancellationToken stoppingToken)
		{
			// detect unused peers
			// delete them from config
			// delete them from IpDedicationService
			// ensure IpDedicationService got no unused addresses
			throw new NotImplementedException();
		}
	}
}
