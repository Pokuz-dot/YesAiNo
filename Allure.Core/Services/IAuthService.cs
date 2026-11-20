namespace Allure.Core.Services;

/// <summary>
/// Сервис аутентификации: SHA256-хеширование паролей и политика блокировки
/// (3 неверных ввода — блокировка интерфейса на 10 секунд).
/// </summary>
public interface IAuthService
{
    string HashPassword(string password);

    bool VerifyPassword(string password, string passwordHash);

    bool IsLockedOut(int failedAttempts, DateTime? lockoutUntil, DateTime now);

    (int FailedAttempts, DateTime? LockoutUntil) RegisterFailedAttempt(int currentAttempts, DateTime now);
}
