using System.Collections.ObjectModel;

namespace vdb_node_api.Services;

/* Данный Singleton-сервис служит для нумерации IP-адресов клиентов.
 * Кажому серверу предлагается адресовать до нескольких млн клиентов,
 * путем итерации по трём байтам следующего рода
 * 10.6.0.0		->	+1			-> 10.6.0.1
 * 10.6.0.1		->	+255		-> 10.6.1.0
 * 10.6.1.0		->	+7933050	-> 10.128.255.255
 * Согласно RFC 1918 (https://datatracker.ietf.org/doc/html/rfc1918#section-3),
 * существуют и другие пространства приватных адресов, однако 
 * 10.6.0.0 -> 10.128.255.255 является общепринятым и достаточным.
 * 
 * Данный сервис должен заниматься хранением публичного ключа и
 * адреса, выделенного для него. Хранение осуществляется в формате
 * словаря <string pubKeyBase64, int dedicatedAddress>, где 
 * int dedicatedAddress служит для хранения IP-адреса в виде 4 байтов.
 * 
 * Максимальное количество клиентов предлагается ограничить 
 * пространством 10.10.255.255, а равно 10*255*255 = 650к клиентов.
 * 
 * Длинна ключа клиента WG составляет 256 бит, а равно 32 байта.
 * Длинна IP-адреса состаавляет 4 байта. Маска - константа, /32.
 * Поскольку Base64 представляет строку длинной равной 4/3 символов
 * от количества байт в изначальной последовательности, то для
 * хранения в таком виде потребуется 4/3*32*650*1000/1024/1024 Мб
 * памяти, что составляет менее 27 мб и является приемлемым.
 * Для хранения адресов потребуется 4*4*650*1000/1024/1024 Мб
 * памяти, что составляет менее 10 мб и является приемлимым.
 * 
 * Одним из вариантов является не хранение адресов, а их формирование
 * на основании индекса в массиве хранимых публичных ключей.
 * Заметим, что List<T> в С# представляет инкапуляцию Array<T>
 * и имеет скорость случайного доступа равную О(1). Однако 
 * такой вариант плох проверкой ключа на существование, ибо
 * 650к - большое число элементов. В данном случае предлагается
 * использовать словарь.
 * 
 * Словарь элементов представляет собой публичный ключ и адрес,
 * выделенный для него. Поскольку ключи словаря представляют
 * объект типа HashSet, то проверка уникальности ключа является
 * высокопроизводительной и позволяет легко избегать выделения 
 * нескольких адресов одному ключу.
 * 
 * Предполагается, что данный сервис должен гарантировать полный
 * контроль выделенных адресов wireguard и на этапе разработки
 * не видится препятствий к этому.
 * 
 * Unit testing private methods in C#
 * https://stackoverflow.com/a/15607491
 */

public sealed class IpDedicationService
{
	private void Swap<T>(ref T v1, ref T v2) { var t = v1; v1 = v2; v2 = t; }

	private const byte FirstIpByteStart = 10;
	private const byte SecondIpByteStart = 6;

	private const int MaxClients = 248 * 255 * 255; // can be safely increased up to (255-6)*255*255
	private const int NetworkMask = 32;

	private Dictionary<string, int> _dedicatedAddresses;
	private readonly HashSet<int> _usedAddresses;
	
	public int IpsAvailable => MaxClients - _usedAddresses.Count;
	public ReadOnlyDictionary<string, int> DedicatedAddresses => _dedicatedAddresses.AsReadOnly();

	public IpDedicationService()
	{
		_dedicatedAddresses = new();
		_usedAddresses = new();
	}

	public int StringToIndex(string address) // tested
	{
		int slashIndex = address.LastIndexOf('/');
		string actualAddress = slashIndex == -1 ?
			address : address.Substring(0, slashIndex);

		return BytesToIndex(actualAddress.Split('.').Select(byte.Parse).ToArray());
	}
	private int BytesToIndex(byte[] bytes) // tested
	{
		bytes[0] -= FirstIpByteStart;
		bytes[1] -= SecondIpByteStart;
		Swap(ref bytes[3], ref bytes[0]);
		Swap(ref bytes[2], ref bytes[1]);
		return BitConverter.ToInt32(bytes);
	}

	private byte[] IndexToBytes(int index) // tested
	{
		byte[] address = BitConverter.GetBytes(index);
		address[3] += FirstIpByteStart; // BitConverter return bytes right to left!
		address[2] += SecondIpByteStart; // BitConverter return bytes right to left!

		return address;
	}
	private string IndexToString(int index) // tested
	{
		byte[] address = IndexToBytes(index);

		// concat methods performance compared https://imgur.com/a/5dGE8xE
		// BitConverter return bytes from smaller to bigger! (right to left)
		return $"{address[3]}.{address[2]}.{address[1]}.{address[0]}/{NetworkMask}";
	}

	/// <returns>
	/// address dedicated for the pubKey in the format 
	/// '10<b>.</b>0<b>.</b>0<b>.</b>0<b>/</b>32'
	/// </returns>
	public string EnsureDedicatedAddressForPeer(string pubKey) // tested
	{
		if (_dedicatedAddresses.TryGetValue(pubKey, out int address))
		{
			return IndexToString(address);
		}
		else
		{
			if (_dedicatedAddresses.Count >= MaxClients)
			{
				throw new IndexOutOfRangeException("Max number of clients reached.");
			}

			// firstly, try to use count as new address
			int addr = _dedicatedAddresses.Count;
			if (_usedAddresses.Contains(addr))
			{
				for (addr = 0; addr < MaxClients; addr++)
				{
					if (!_usedAddresses.Contains(addr))
					{
						AddPeer(pubKey, addr);
						return IndexToString(addr);
					}
				}
			}

			AddPeer(pubKey, addr);
			return IndexToString(addr);
		}
	}


	/// <returns>
	/// true if the pubKey was successfully added, false otherwise.
	/// </returns>
	private bool AddPeer(string pubKey, int addressIndex) // tested
	{
		_dedicatedAddresses.Add(pubKey, addressIndex);
		return _usedAddresses.Add(addressIndex);
	}

	/// <returns>
	/// true if the pubKey was successfully found and removed, false otherwise.
	/// </returns>
	public bool DeletePeer(string pubKey) // tested
	{
		if (!_dedicatedAddresses.TryGetValue(pubKey, out int address))
		{
			return false;
		}
		else
		{
			_usedAddresses.Remove(address);
			return _dedicatedAddresses.Remove(pubKey);
		}
	}

	/* Данный метод синхронизрует хранимые адреса с актуальным списком,
	 * полученным от 'wg show wg0'. Данный метод следует исполнять периодически,
	 * он обеспечивает корреакцию пиров, который не были отключены должным образом.
	 * Если не выполнять данный метод, то со временем IpDedicationService забьётся
	 * адресами, которые уже недействительны. Данный метод должен получать от 
	 * PeerBackgroundService готовую коллекцию, на основании которой перезаписывать
	 * собственные.
	 */
	public void SyncState(Dictionary<string, int> keyToIpActulState)
	{
		_dedicatedAddresses = keyToIpActulState;
		_usedAddresses.Clear();
		foreach (int ip in _dedicatedAddresses.Values)
			_usedAddresses.Add(ip);
	}
}
