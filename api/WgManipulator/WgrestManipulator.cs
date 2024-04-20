using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using WgManipulator.Models;

namespace WgManipulator;

public class WgrestManipulator : IWgManipulator
{
	private const string _defaultServerAddress = "http://127.0.0.1:8000/";
	private const string _defaultDeviceName = "wgrest";

	private const string _defaultDeviceUrl = $"{_defaultServerAddress}v1/devices/{_defaultDeviceName}/";
	private const string _defaultPeersUrl = $"{_defaultServerAddress}v1/devices/{_defaultDeviceName}/peers/";

	private readonly string _deviceUrl;
	private readonly string _peersUrl;
	private WgDevice? _lastRetrievedDevice;

	private static readonly HttpClient _client = new();

	public WgrestManipulator(
		string deviceUrl = _defaultDeviceUrl,
		string peersUrl = _defaultPeersUrl)
	{
		_deviceUrl = deviceUrl;
		_peersUrl = peersUrl;
	}

	// v1.GET("/devices/:name/peers/", wc.ListDevicePeers)
	public async Task<List<WgPeer>> GetPeers()
	{
		return (await _client.GetFromJsonAsync<List<WgrestPeer>>(_peersUrl))!
			.Select(x => x.ToWgPeer()).ToList();
	}

	// v1.POST("/devices/:name/peers/", wc.CreateDevicePeer)
	public async Task<int> AddPeers(IEnumerable<WgAddPeerRequest> peers)
	{
		var cnt = 0;

		foreach(var r in peers)
			if((await _client.PostAsJsonAsync(_peersUrl,
				WgrestAddPeerRequest.FromWgAddPeerRequest(r)))
				.IsSuccessStatusCode) cnt++;

		return cnt;
	}

	// v1.DELETE("/devices/:name/peers/:urlSafePubKey/", wc.DeleteDevicePeer)
	public async Task<List<WgPeer>> DeletePeers(IEnumerable<string> pubkeys)
	{
		var ret = new List<WgPeer>(pubkeys.TryGetNonEnumeratedCount(out var c) ? c : 1);

		foreach(var pk in pubkeys)
		{
			var urlSafePk = pk.Replace('+', '-').Replace('/', '_');
			var url = $"{_peersUrl}{urlSafePk}/";

			var peerInfo = await _client.GetFromJsonAsync<WgrestPeer>(url);
			if(peerInfo is null) continue;

			if((await _client.DeleteAsync(url)).IsSuccessStatusCode)
				ret.Add(peerInfo.ToWgPeer());
		}

		return ret;
	}

	// v1.GET("/devices/:name/", wc.GetDevice)
	public async Task<WgDevice> GetInterfaceInfo()
	{
		_lastRetrievedDevice ??= (await _client.GetFromJsonAsync<WgrestDevice>(_deviceUrl))!.ToWgDevice();
		return _lastRetrievedDevice;
	}
}