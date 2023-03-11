using System.Diagnostics;
using vdb_node_api.Models.Runtime;
using vdb_node_wireguard_manipulator;

namespace vdb_node_api.Services;

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

	private DateTime _lastUpdateUtc;
	private int _peersRemovedOnLastUpdate;

	public string? InterfacePubkey { get; private set; }

	public PeersBackgroundService(IpDedicationService ipService, SettingsProviderService settingsProvider, ILogger<PeersBackgroundService> logger)
	{
		_ipService = ipService;
		_settings = settingsProvider.PeersBackgroundServiceSettings;
		_logger = logger;

		if (_settings.HandshakeAgoLimitSeconds <= 0)
			_settings.HandshakeAgoLimitSeconds = int.MaxValue;

		_lastUpdateUtc = DateTime.MinValue;
	}

	public async Task<string> EnsurePeerAdded(string pubkey)
	{
		string ip = _ipService.EnsureDedicatedAddressForPeer(pubkey);
		string result = await WgCommandsExecutor.AddPeer(pubkey, ip);

		return !string.IsNullOrWhiteSpace(result) ? throw new AggregateException(result) : ip;
	}
	public async Task<bool> EnsurePeerRemoved(string pubkey)
	{
		string output = await WgCommandsExecutor.RemovePeer(pubkey);
		/* DeletePeer вернет true, если пир был найден и удален.
		 * Вывод в консоль от WG отсутствует по-умолчанию.
		 */
		return _ipService.DeletePeer(pubkey) && string.IsNullOrWhiteSpace(output);
	}

	public async IAsyncEnumerator<WgShortPeerInfo> GetPeersAndUpdateState()
	{
		_peersRemovedOnLastUpdate = 0;
		var enumer = await WgCommandsExecutor.GetPeersListEnumerator();

		var syncedDictionary = new Dictionary<string, int>();

		while (enumer.MoveNext())
		{
			var peer = enumer.Current;

			if (peer.HandshakeSecondsAgo > _settings.HandshakeAgoLimitSeconds)
			{
				_logger.LogInformation($"Peer {peer.PublicKey} removed because last " +
					$"handshake occured more than {_settings.HandshakeAgoLimitSeconds} seconds ago.");
				_peersRemovedOnLastUpdate++;
				await EnsurePeerRemoved(peer.PublicKey);
			}
			else
			{
				syncedDictionary.Add(peer.PublicKey, _ipService.StringToIndex(peer.AllowedIps));
				yield return peer;
			}
		}

		_ipService.SyncState(syncedDictionary);
		_lastUpdateUtc = DateTime.UtcNow;
		InterfacePubkey = WgCommandsExecutor.LastSeenInterfacePubkey;
		yield break;
	}


	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		Stopwatch sw = new();
		while (!stoppingToken.IsCancellationRequested)
		{
			// Данное условие проверяет, не был ли список обновлен форсированно.
			if ((DateTime.UtcNow - _lastUpdateUtc).TotalSeconds > _settings.PeersRenewIntervalSeconds)
			{
				_logger.LogInformation($"Peers cleanup and sync started.");
				sw.Restart();
				var enumer = GetPeersAndUpdateState();
				while (await enumer.MoveNextAsync()) ;
				sw.Stop();
				_logger.LogInformation($"Peers cleanup completed: {_peersRemovedOnLastUpdate} peers removed. " +
					$"Took {sw.ElapsedMilliseconds / 1000} seconds.");
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
			int delayS = _settings.PeersRenewIntervalSeconds - (int)(DateTime.UtcNow - _lastUpdateUtc).TotalSeconds;
			_logger.LogInformation($"ExecuteAsync is delayed for {delayS} seconds.");
			await Task.Delay(delayS * 1000);
		}
	}
}
