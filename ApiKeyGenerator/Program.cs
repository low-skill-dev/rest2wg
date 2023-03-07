using System.Security.Cryptography;

var bytes = RandomNumberGenerator.GetBytes(512 / 8);
var keyHash = SHA512.HashData(bytes);

Console.WriteLine("Key base64        (for client):");
Console.WriteLine(Convert.ToBase64String(bytes));
Console.WriteLine("Key hash base64   (for server):");
Console.WriteLine(Convert.ToBase64String(keyHash));