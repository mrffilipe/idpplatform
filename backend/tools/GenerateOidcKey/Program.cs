using System.Security.Cryptography;

var outputPath = args.Length > 0
    ? args[0]
    : Path.Combine("..", "..", "Kyvo.API", "keys", "oidc-signing.pem");

var directory = Path.GetDirectoryName(outputPath);
if (!string.IsNullOrWhiteSpace(directory))
{
    Directory.CreateDirectory(directory);
}

using var rsa = RSA.Create(2048);
File.WriteAllText(outputPath, rsa.ExportPkcs8PrivateKeyPem());
Console.WriteLine($"Wrote {Path.GetFullPath(outputPath)}");
