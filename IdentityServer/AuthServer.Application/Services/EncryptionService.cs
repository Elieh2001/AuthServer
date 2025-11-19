using System.Security.Cryptography;
using System.Text;
using AuthServer.Application.Interfaces;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Configuration;

namespace AuthServer.Application.Services;

public class EncryptionService : IEncryptionService
{
    #region Members

    private readonly IDataProtector _protector;
    private readonly string _masterKey;

    #endregion

    #region Constructors

    public EncryptionService(IDataProtectionProvider provider, IConfiguration configuration)
    {
        _protector = provider.CreateProtector("AuthServer.Encryption");
        _masterKey = configuration["EncryptionSettings:MasterKey"];
    }

    #endregion

    #region Public Methods

    public string Encrypt(string plainText)
    {
        if (string.IsNullOrEmpty(plainText))
            return plainText;

        try
        {
            return _protector.Protect(plainText);
        }
        catch
        {
            // Fallback to AES if DataProtection fails
            return EncryptAES(plainText);
        }
    }

    public string Decrypt(string cipherText)
    {
        if (string.IsNullOrEmpty(cipherText))
            return cipherText;

        try
        {
            return _protector.Unprotect(cipherText);
        }
        catch
        {
            // Fallback to AES if DataProtection fails
            return DecryptAES(cipherText);
        }
    }

    private string EncryptAES(string plainText)
    {
        using (var aes = Aes.Create())
        {
            var key = Encoding.UTF8.GetBytes(_masterKey.PadRight(32).Substring(0, 32));
            aes.Key = key;
            aes.GenerateIV();

            var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
            var plainBytes = Encoding.UTF8.GetBytes(plainText);
            var cipherBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);

            var result = new byte[aes.IV.Length + cipherBytes.Length];
            Buffer.BlockCopy(aes.IV, 0, result, 0, aes.IV.Length);
            Buffer.BlockCopy(cipherBytes, 0, result, aes.IV.Length, cipherBytes.Length);

            return Convert.ToBase64String(result);
        }
    }

    #endregion

    #region Private Methods

    private string DecryptAES(string cipherText)
    {
        var fullCipher = Convert.FromBase64String(cipherText);

        using (var aes = Aes.Create())
        {
            var key = Encoding.UTF8.GetBytes(_masterKey.PadRight(32).Substring(0, 32));
            aes.Key = key;

            var iv = new byte[aes.IV.Length];
            var cipher = new byte[fullCipher.Length - iv.Length];

            Buffer.BlockCopy(fullCipher, 0, iv, 0, iv.Length);
            Buffer.BlockCopy(fullCipher, iv.Length, cipher, 0, cipher.Length);

            aes.IV = iv;

            var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
            var plainBytes = decryptor.TransformFinalBlock(cipher, 0, cipher.Length);

            return Encoding.UTF8.GetString(plainBytes);
        }
    }

    #endregion
}