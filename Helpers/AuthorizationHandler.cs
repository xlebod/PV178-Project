using System.Security.Cryptography;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using SettleDown.Models;

namespace SettleDown.Helpers;

public static class AuthorizationHandler
{
    public static string CreateSaltedHashPassword(string password, byte[] salt)
    {
        if (salt.Length != 128 / 8)
            throw new InvalidOperationException("Must provide a 128 bit salt!");
        
        return Convert.ToBase64String(KeyDerivation.Pbkdf2(
            password: password,
            salt: salt,
            prf: KeyDerivationPrf.HMACSHA256,
            iterationCount: 100000,
            numBytesRequested: 256 / 8));
        
    }

    public static byte[] Create128BitSalt()
    {
        var salt = new byte[128 / 8];
        using var rngCsp = new RNGCryptoServiceProvider();
        rngCsp.GetNonZeroBytes(salt);

        return salt;
    }



    public static bool AreCredentialsValid(SettleDownCredential credential, string password)
    {
        return CreateSaltedHashPassword(password, credential.Salt) == credential.Password;
    }
}