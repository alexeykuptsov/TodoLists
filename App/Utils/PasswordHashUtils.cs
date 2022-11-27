using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;
using System.Text;

namespace TodoLists.App.Utils;

[ExcludeFromCodeCoverage]
public static class PasswordHashUtils
{
    public static (byte[] PasswordHash, byte[] PasswordSalt) CreatePasswordHash(string password)
    {
        using var hmac = new HMACSHA512();
        return (PasswordHash: hmac.ComputeHash(Encoding.UTF8.GetBytes(password)), PasswordSalt: hmac.Key);
    }

    public static bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt)
    {
        using var hmac = new HMACSHA512(passwordSalt);
        var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
        return computedHash.SequenceEqual(passwordHash);
    }
}