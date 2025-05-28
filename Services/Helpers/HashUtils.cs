using System.Security.Cryptography;
using System.Text;

public static class HashUtils
{
    /// <summary>
    /// Gera um hash SHA-256 para a string fornecida.
    /// @param input A string que será transformada em hash.
    /// @return O hash SHA-256 em formato hexadecimal.
    /// </summary>
    public static string ComputeSha256Hash(string input)
    {
        using var sha256 = SHA256.Create();
        byte[] bytes = Encoding.UTF8.GetBytes(input);
        byte[] hashBytes = sha256.ComputeHash(bytes);

        StringBuilder builder = new StringBuilder();
        foreach (var b in hashBytes)
        {
            builder.Append(b.ToString("x2"));
        }
        return builder.ToString();
    }
}