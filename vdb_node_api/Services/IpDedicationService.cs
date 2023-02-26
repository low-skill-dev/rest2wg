using System;

namespace vdb_node_api.Services
{
	/* Данный Singleton-сервис служит для
	 * нумерации IP-адресов клиентов.
	 * Кажому серверу предлагается выделить до 65к клиентов,
	 * путем нумерации следующего рода
	 * 10.6.1.1		->	+1      -> 10.6.1.2
	 * 10.6.1.2		->	+253    -> 10.6.1.255
	 * 10.6.1.255	->	+1      -> 10.6.2.1	
	 * 10.6.2.1		->	+64770	-> 10.6.255.255
	 * Таким образом, суммарный объем составляет >255*255 = >65к адресов,
	 * нумеруемых в пространстве 10.6.1.1/32 -> 10.6.255.255/32.
	 * 
	 * Дополнительно закладывается алгоритмическая возможность нумерации
	 * пространства по второму байту, следующим образом
	 * 10.6.255.255	->	+1		-> 10.7.1.1
	 * Таким образом, обспечивается возможность нумерации 
	 * 255*255*(255-6) = 16191225 адресов.
	 * 
	 * Данный сервис должен заниматься хранением публичного ключа и
	 * адреса, выделенного для него. Хранение осуществляется в формате
	 * (string pubKeyBase64, byte[] dedicatedAddress).
	 * Длинна ключа клиента WG составляет 256 бит, а равно 32 байта.
	 * Длинна IP-адреса состаавляет 4 байта. Маска - константа, /32.
	 * Поскольку Base64 представляет строку длинной равной 4/3 символов
	 * от количества байт в изначальной последовательности, то для
	 * хранения в таком виде потребуется 4/3*32*65025/1024/1024 Мб
	 * памяти, что составляет менее 3 мб и является приемлемым.
	 * Для хранения адресов потребуется 4*4*65025/1024/1024 Мб
	 * памяти, что составляет менее 1 мб и является приемлимым.
	 * 
	 * Одним из вариантов является не хранение адресов, а их формирование
	 * на основании индекса в массиве хранимых публичных ключей.
	 * Заметим, что List<T> в С# представляет инкапуляцию Array<T>
	 * и имеет скорость случайного доступа равную О(1). Однако 
	 * такой вариант плох проверкой ключа на существование, ибо
	 * 65к - большое число элементов. В данном случае предлагается
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
		// Константы никак не влияют на производительность
		private const int NetworkMask = 32;
		private const int MaxClients = 255 * 255;

		private const byte FirstByteStart = 10;
		private const byte SecondByteStart = 1;
		private const byte ThirdByteStart = 1;
		private const byte FourthByteStart = 1;

		private  const byte FirstByteCanTake = 256 - FirstByteStart;
		private const byte SecondByteCanTake = 256 - SecondByteStart;
		private const byte ThirdByteCanTake = 256 - ThirdByteStart;
		private const byte FourthByteCanTake = 256 - FourthByteStart;

		private const int LastByteCanTake = FourthByteCanTake;
		private const int LastTwoBytesCanTake = FourthByteCanTake * ThirdByteCanTake;
		private const int LastThreeBytesCanTake = LastTwoBytesCanTake * SecondByteCanTake;


		private Dictionary<string, byte[]> _dedicatedAddresses { get; set; }
		private HashSet<byte[]> _usedAddresses { get; set; }

		public IpDedicationService()
		{
			_dedicatedAddresses = new();
			_usedAddresses = new();
		}

		private string BytesToAddress(byte[] address)
		{
			if (address.Length != 4)
			{
				throw new ArgumentException("IP adress must contain 4 bytes.");
			}

			// concat methods performance compared https://imgur.com/a/5dGE8xE
			return $"{address[0]}.{address[1]}.{address[2]}.{address[3]}/{NetworkMask}";
		}
		private byte[] AddressToBytes(string address)
		{
			var slashIndex = address.LastIndexOf('/');
			var actualAddress = slashIndex == -1 ?
				address : address.Substring(0, slashIndex);

			return actualAddress.Split('.').Select(byte.Parse).ToArray();
		}

		// be shure not to exceed the limit!
		private byte[] IndexToAddress(int index)
		{
			byte second = (byte)(SecondByteStart + index / LastTwoBytesCanTake);
			byte third = (byte)(ThirdByteStart + (index% LastTwoBytesCanTake)/LastByteCanTake);
			byte fourth = (byte)(FourthByteStart + (index%LastTwoBytesCanTake%LastByteCanTake));

			return new byte[] { FirstByteStart, second, third, fourth };
		}

		/// <returns>address dedicated for the pubKey in the format '10.8.*.*/32'</returns>
		public string EnsureDedicatedAddressForPeer(string pubKey)
		{
			if (_dedicatedAddresses.TryGetValue(pubKey, out var addressBytes))
			{
				return BytesToAddress(addressBytes);
			}
			else
			{
				if (this._dedicatedAddresses.Count >= MaxClients)
				{
					throw new IndexOutOfRangeException("Max number of clients reached.");
				}

				// firstly, try to use count as new address
				var address = IndexToAddress(_dedicatedAddresses.Count);
				if (_usedAddresses.Contains(address))
				{
					byte[] tryedAddress = new byte[4] { 10, 8, default, default };
					for (int i = 0; i < MaxClients; i++)
					{
						byte first = (byte)(1 + i / 255); // max(index) = 255*255 = 65025
						byte second = (byte)(1 + i % 255);

						tryedAddress[2] = second;
						tryedAddress[3] = first;

						if (!_usedAddresses.Contains(tryedAddress))
						{
							address = tryedAddress;

							_dedicatedAddresses.Add(pubKey, address);
							_usedAddresses.Add(address);
							return BytesToAddress(address);
						}
					}

					throw new IndexOutOfRangeException(
						"Unbale to find unused address for the client.");
				}

				_dedicatedAddresses.Add(pubKey, address);
				_usedAddresses.Add(address);
				return BytesToAddress(address);
			}
		}

		/// <returns>true if the pubKey was successfully found and removed, false otherwise.</returns>
		private bool RemovePeer(string pubKey)
		{
			if (!_dedicatedAddresses.TryGetValue(pubKey, out var address))
			{
				return false;
			}
			else
			{
				_usedAddresses.Remove(address);
				return _dedicatedAddresses.Remove(pubKey);
			}
		}
	}
}
