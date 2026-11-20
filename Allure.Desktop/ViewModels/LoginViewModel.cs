using Allure.Core.Models;
using Allure.Core.Services;
using Allure.Desktop.Services;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.EntityFrameworkCore;
using CommunityToolkit.Mvvm.Input;
using System;

namespace Allure.Desktop.ViewModels;

/// <summary>
/// Авторизация: маска пароля с переключателем видимости,
/// блокировка интерфейса на 10 секунд после 3 неверных вводов.
/// </summary>
public partial class LoginViewModel : ObservableObject
{
    private readonly DbContextFactory _dbFactory;
    private readonly IAuthService _auth;
    private readonly SessionService _session;

    private int _failedAttempts;
    private DateTime? _lockoutUntil;
    private DispatcherTimer? _lockoutTimer;

    [ObservableProperty]
    private string _login = string.Empty;

    [ObservableProperty]
    private string _password = string.Empty;

    [ObservableProperty]
    private char _passwordChar = '•';

    [ObservableProperty]
    private bool _showPassword;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private bool _isLocked;

    [ObservableProperty]
    private string _lockoutText = string.Empty;

    public event Action<User>? LoginSucceeded;

    public LoginViewModel(DbContextFactory dbFactory, IAuthService auth, SessionService session)
    {
        _dbFactory = dbFactory ?? throw new ArgumentNullException(nameof(dbFactory));
        _auth = auth ?? throw new ArgumentNullException(nameof(auth));
        _session = session ?? throw new ArgumentNullException(nameof(session));
    }

    partial void OnShowPasswordChanged(bool value)
    {
        PasswordChar = value ? '\0' : '•';
    }

    [RelayCommand]
    private void Login()
    {
        if (IsLocked)
            return;

        if (string.IsNullOrWhiteSpace(Login))
        {
            ErrorMessage = "Введите логин.";
            return;
        }
        if (string.IsNullOrWhiteSpace(Password))
        {
            ErrorMessage = "Введите пароль.";
            return;
        }

        User? user;
        try
        {
            using var context = _dbFactory.CreateDbContext();
            user = context.Users.Include(u => u.Role)
                .FirstOrDefault(u => u.Login == Login.Trim());
        }
        catch (Exception ex)
        {
            ErrorMessage = "Нет соединения с БД. Импортируйте Database/Yes.sql в phpMyAdmin. " + ex.Message;
            return;
        }

        if (user is null || !_auth.VerifyPassword(Password, user.PasswordHash))
        {
            (_failedAttempts, _lockoutUntil) = _auth.RegisterFailedAttempt(_failedAttempts, DateTime.Now);
            if (_auth.IsLockedOut(_failedAttempts, _lockoutUntil, DateTime.Now))
            {
                StartLockoutCountdown();
                ErrorMessage = "Неверный логин или пароль. Интерфейс заблокирован.";
            }
            else
            {
                ErrorMessage = $"Неверный логин или пароль. Осталось попыток: {AuthService.MaxFailedAttempts - _failedAttempts}.";
            }
            Password = string.Empty;
            return;
        }

        _failedAttempts = 0;
        _lockoutUntil = null;
        ErrorMessage = string.Empty;
        _session.SignIn(user);
        LoginSucceeded?.Invoke(user);
    }

    private void StartLockoutCountdown()
    {
        IsLocked = true;
        _lockoutTimer?.Stop();
        _lockoutTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        _lockoutTimer.Tick += (_, _) =>
        {
            if (_lockoutUntil is null || DateTime.Now >= _lockoutUntil.Value)
            {
                _lockoutTimer?.Stop();
                IsLocked = false;
                LockoutText = string.Empty;
                _failedAttempts = 0;
                _lockoutUntil = null;
                ErrorMessage = "Блокировка снята. Попробуйте снова.";
                return;
            }
            int secondsLeft = (int)Math.Ceiling((_lockoutUntil.Value - DateTime.Now).TotalSeconds);
            LockoutText = $"Повторите через {secondsLeft} сек.";
        };
        _lockoutTimer.Start();
        LockoutText = "Повторите через 10 сек.";
    }
}
