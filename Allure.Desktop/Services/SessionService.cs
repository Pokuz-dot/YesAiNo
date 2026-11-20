using Allure.Core.Models;

namespace Allure.Desktop.Services;

/// <summary>Текущая сессия авторизованного сотрудника.</summary>
public sealed class SessionService
{
    public User? CurrentUser { get; set; }

    public void SignIn(User user) => CurrentUser = user ?? throw new ArgumentNullException(nameof(user));

    public void SignOut() => CurrentUser = null;
}
