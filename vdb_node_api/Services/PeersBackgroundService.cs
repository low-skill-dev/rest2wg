using System.Diagnostics;
using System.Runtime;
using vdb_node_api.Models.ServicesSettings;
using vdb_node_wireguard_manipulator;

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
	public sealed class PeersBackgroundService : BackgroundService
	{
		private readonly PeersBackgroundServiceSettings _settings;
		private readonly IpDedicationService _ipService;
		private readonly ILogger<PeersBackgroundService> _logger;
		private Dictionary<string, WgFullPeerInfo> _peers;
		private Dictionary<string, WgInterfaceInfo> _interfaces;
		private DateTime _lastUpdateUtc;
		public PeersBackgroundService(IpDedicationService ipService, SettingsProviderService settingsProvider, ILogger<PeersBackgroundService> logger)
		{
			_ipService = ipService;
			_settings = settingsProvider.PeersBackgroundServiceSettings;
			_logger = logger;

			_peers = new();
			_interfaces = new();
			_lastUpdateUtc = DateTime.MinValue;
		}


		public async Task<IEnumerable<WgFullPeerInfo>> GetPeers(bool forceUpdate = false)
		{
			if (forceUpdate) await UpdatePeersListAndSyncConf();

			return _peers.Values;
		}
		public async Task<IEnumerable<WgInterfaceInfo>> GetInterfaces(bool forceUpdate = false)
		{
			if (forceUpdate) await UpdatePeersListAndSyncConf();

			return _interfaces.Values;
		}

		public async Task<WgFullPeerInfo?> GetPeerInfo(string pubkey,bool forceUpdate = false)
		{
			if (forceUpdate) await UpdatePeersListAndSyncConf();

			return _peers.GetValueOrDefault(pubkey);
		}
		public async Task<WgInterfaceInfo?> GetInterfaceInfo(string pubkey, bool forceUpdate = false)
		{
			if (forceUpdate) await UpdatePeersListAndSyncConf();

			return _interfaces.GetValueOrDefault(pubkey);
		}

		public async Task DeletePeer(string pubkey)
		{
			_ipService.DeletePeer(pubkey);
			_peers.Remove(pubkey);
			var result = await CommandsExecutor.RemovePeer(pubkey);

			if (!string.IsNullOrWhiteSpace(result))
			{
				throw new AggregateException(result);
			}
		}
		/// <returns>AllowedIps string for the added peer</returns>
		public async Task<string> AddPeer(string pubkey)
		{
			var ip = _ipService.EnsureDedicatedAddressForPeer(pubkey);
			var result= await CommandsExecutor.AddPeer(pubkey, ip);

			if (!string.IsNullOrWhiteSpace(result))
			{
				throw new AggregateException(result);
			}

			return ip;
		}

		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			Stopwatch sw = new();
			while (!stoppingToken.IsCancellationRequested)
			{
				// Данное условие проверяет, не был ли список обновлен форсированно.
				if ((DateTime.UtcNow - _lastUpdateUtc).TotalSeconds > _settings.PeersRenewIntervalSeconds)
				{
					try
					{
						_logger.LogInformation($"Beginning update peers and sync conf.");
						sw.Restart();
						await UpdatePeersListAndSyncConf();
						sw.Stop();
						_logger.LogInformation($"Completed update peers and sync conf. " +
							$"Took {sw.ElapsedMilliseconds/1000} seconds.");
					}
					catch (Exception ex)
					{
						_logger.LogError($"Unable to delete outdated peers and sync conf: {ex.Message}.");
					}
				}

				/* Данное мат. выражение задает вычисляет, сколько мс назад
				 * было проведено обновление и задаёт следующее обновление не 
				 * спустя интервал, а спустя интервал минус данное значение.
				 * 
				 * Это предотваращает ситуцию, когда, например, пир был
				 * обновлен вручную (интервал-1) секунд назад, при этом условие
				 * выше будет ложно, а значит реальное обновление будет выполнено
				 * де-факто спустя целых 2 интервала, вместо одного.
				 */

				var delayS = _settings.PeersRenewIntervalSeconds - (int)(DateTime.UtcNow - _lastUpdateUtc).TotalSeconds;

				_logger.LogInformation($"ExecuteAsync is delayed for {delayS} seconds.");
				await Task.Delay(delayS*1000);
			}
		}

		private async Task UpdatePeersListAndSyncConf()
		{
			_logger.LogInformation($"Beginning peers list update and conf sync. " +
				$"Currently storing: {_peers.Count} peers.");

			// Сначала получаем актуальный список пиров
			var parsed = await CommandsExecutor.GetPeersList();
			if (parsed.interfaces.Count > 1)
			{
				var message = "There are multiple interfaces was detected. " +
					"This is currently not supported. Ensure that there is no mistake " +
					"in the wireguard configuration.";

				_logger.LogCritical(message);
				throw new AggregateException(message);
			}
			_peers = parsed.peers.ToDictionary(x => x.PublicKey);
			_interfaces = parsed.interfaces.ToDictionary(x => x.PublicKey);
			// На основании этого списка удаляем все пиры, у которых истек срок жизни
			await DeleteInactivePeers();
			// Теперь синхронизирум полученную конфигурацию с IpDedicationService
			_ipService.SyncState(_peers);

			_logger.LogInformation($"Updating and sync complete. " +
				$"Currently storing: {_peers.Count} peers.");

			_lastUpdateUtc = DateTime.UtcNow;
		}

		private async Task DeleteInactivePeers()
		{
			foreach(var peer in _peers.Values)
			{
				if (peer.HandshakeTotalSeconds > _settings.HandshakeAgoLimitSeconds)
				{
					await CommandsExecutor.RemovePeer(peer.PublicKey);
					_ipService.DeletePeer(peer.PublicKey);
					_peers.Remove(peer.PublicKey);
				}
			}
		}
	}
}
