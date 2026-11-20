using System.Security.Cryptography;
using System.Text;

namespace Allure.Core.Services;

/// <summary>Хеширование SHA256 (hex) и счётчик неудачных попыток входа.</summary>
public sealed class AuthService : IAuthService
{
    public const int MaxFailedAttempts = 3;
    public static readonly TimeSpan LockoutDuration = TimeSpan.FromSeconds(10);

    public string HashPassword(string password)
    {
        ArgumentNullException.ThrowIfNull(password);
        byte[] hash = SHA256.HashData(Encoding.UTF8.GetBytes(password));
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    public bool VerifyPassword(string password, string passwordHash)
    {
        if (password is null || string.IsNullOrWhiteSpace(passwordHash))
            return false;
        string candidate = HashPassword(password);
        byte[] candidateBytes = Encoding.UTF8.GetBytes(candidate);
        byte[] storedBytes = Encoding.UTF8.GetBytes(passwordHash.Trim().ToLowerInvariant());
        return candidateBytes.Length == storedBytes.Length
               && CryptographicOperations.FixedTimeEquals(candidateBytes, storedBytes);
    }

    public bool IsLockedOut(int failedAttempts, DateTime? lockoutUntil, DateTime now) =>
        failedAttempts >= MaxFailedAttempts && lockoutUntil.HasValue && now < lockoutUntil.Value;

    public (int FailedAttempts, DateTime? LockoutUntil) RegisterFailedAttempt(int currentAttempts, DateTime now)
    {
        int attempts = currentAttempts + 1;
        DateTime? until = attempts >= MaxFailedAttempts ? now.Add(LockoutDuration) : null;
        return (attempts, until);
    }
}
