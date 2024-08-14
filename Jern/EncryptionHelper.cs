using System.Security.Cryptography;
using System.Text;

namespace Jern;

public static class EncryptionHelper
{
    public static bool Error;

    /// <summary>
    /// Encrypts the content of the TextView and saves it to the current file
    /// </summary>
    /// <param name="inputString">From the TextView</param>
    /// <param name="outputFile">The current file</param>
    /// <param name="key">Key for the file</param>
    public static void EncryptFile(string inputString, string outputFile, string key)
    {
        var bytes = Encoding.Unicode.GetBytes(inputString);
        SymmetricAlgorithm crypt = Aes.Create();
        crypt.BlockSize = 128;
        crypt.Key = MD5.HashData(Encoding.Unicode.GetBytes(key));
        crypt.GenerateIV();

        using var memoryStream = new MemoryStream();
        using (var cryptoStream = new CryptoStream(memoryStream, crypt.CreateEncryptor(), CryptoStreamMode.Write))
        {
            cryptoStream.Write(bytes, 0, bytes.Length);
        }

        var iv = crypt.IV;
        var result = Convert.ToBase64String(iv.Concat(memoryStream.ToArray()).ToArray());
        Directory.CreateDirectory(Path.GetDirectoryName(outputFile) ?? throw new InvalidOperationException());
        File.WriteAllText(outputFile, result);
    }

    /// <summary>
    /// Attempts to open a file on disk and decrypt it with the given key
    /// </summary>
    /// <param name="inputFile">Current file</param>
    /// <param name="key">Key for the file</param>
    public static string DecryptFile(string inputFile, string key)
    {
        // Decrypt
        // Make sure that the entries directory exists
        try
        {
            Error = false;
            var combinedBytes = Convert.FromBase64String(File.ReadAllText(inputFile));
            var iv = combinedBytes.Take(16).ToArray();
            var cipherText = combinedBytes.Skip(16).ToArray();

            SymmetricAlgorithm crypt = Aes.Create();
            crypt.BlockSize = 128;
            crypt.Key = MD5.HashData(Encoding.Unicode.GetBytes(key));
            crypt.IV = iv;

            using var memoryStream = new MemoryStream();
            using (var cryptoStream = new CryptoStream(memoryStream, crypt.CreateDecryptor(), CryptoStreamMode.Write))
            {
                cryptoStream.Write(cipherText, 0, cipherText.Length);
            }

            var decryptedBytes = memoryStream.ToArray();
            return Encoding.Unicode.GetString(decryptedBytes);
        }
        catch (CryptographicException)
        {
            Error = true;
            return "Invalid key. (Esc to quit)";
        }
        catch (FormatException)
        {
            Error = true;
            return "Invalid file. (Esc to quit)";
        }
        catch (IOException ex)
        {
            Error = true;
            return ex.Message + "\n(Esc to quit)";
        }
    }
}