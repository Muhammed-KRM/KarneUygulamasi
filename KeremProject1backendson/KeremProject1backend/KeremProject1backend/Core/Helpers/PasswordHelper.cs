using System.Security.Cryptography;
using System.Text;

namespace KeremProject1backend.Core.Helpers;

public static class PasswordHelper
{
    public static void CreateHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
    {
        if (string.IsNullOrWhiteSpace(password))
            throw new ArgumentException("Password cannot be empty", nameof(password));

        using var hmac = new HMACSHA512();
        passwordSalt = hmac.Key;
        passwordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
    }

    public static bool VerifyHash(string password, byte[] storedHash, byte[] storedSalt)
    {
        if (string.IsNullOrWhiteSpace(password))
            throw new ArgumentException("Password cannot be empty", nameof(password));

        if (storedHash.Length != 64)
            throw new ArgumentException("Invalid length of password hash (64 bytes expected)", nameof(storedHash));

        if (storedSalt.Length != 128)
            throw new ArgumentException("Invalid length of password salt (128 bytes expected)", nameof(storedSalt));

        using var hmac = new HMACSHA512(storedSalt);
        var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));

        for (int i = 0; i < computedHash.Length; i++)
        {
            if (computedHash[i] != storedHash[i])
                return false;
        }

        return true;
    }
}
