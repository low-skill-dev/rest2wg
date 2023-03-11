using System.Security.Cryptography;

byte[] bytes = RandomNumberGenerator.GetBytes(512 / 8);
byte[] keyHash = SHA512.HashData(bytes);

Console.WriteLine("Key base64        (for client):");
Console.WriteLine(Convert.ToBase64String(bytes));
Console.WriteLine("Key hash base64   (for server):");
Console.WriteLine(Convert.ToBase64String(keyHash));