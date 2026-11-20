using Allure.Core.Services;
using Xunit;

namespace Allure.Tests;

/// <summary>Тесты аутентификации (SHA256, блокировка) и таймера 45 минут.</summary>
public sealed class AuthAndTimerTests
{
    [Fact]
    public void Auth_HashAndVerify_RoundTrip()
    {
        var auth = new AuthService();
        string hash = auth.HashPassword("admin123");
        Assert.True(auth.VerifyPassword("admin123", hash));
        Assert.False(auth.VerifyPassword("wrong-password", hash));
    }

    [Fact]
    public void Auth_ThreeFailedAttempts_LocksOutFor10Seconds()
    {
        var auth = new AuthService();
        var now = new DateTime(2026, 11, 20, 12, 0, 0);
        int attempts = 0;
        DateTime? until = null;
        for (int i = 0; i < 3; i++)
            (attempts, until) = auth.RegisterFailedAttempt(attempts, now);

        Assert.True(auth.IsLockedOut(attempts, until, now));
        Assert.Equal(now.AddSeconds(10), until);
        Assert.False(auth.IsLockedOut(attempts, until, now.AddSeconds(11)));
    }

    [Fact]
    public void Timer_AcademicHour_Is45Minutes()
    {
        var timer = new TrainingTimerService();
        Assert.Equal(TimeSpan.FromMinutes(45), TrainingTimerService.AcademicHour);
        Assert.Equal("45:00", timer.FormatRemaining());
    }

    [Fact]
    public void Timer_TickToZero_FiresFinished()
    {
        var timer = new TrainingTimerService();
        bool finished = false;
        timer.Finished += (_, _) => finished = true;
        timer.Start();
        timer.Tick(TimeSpan.FromMinutes(45));
        Assert.True(finished);
        Assert.Equal(TimeSpan.Zero, timer.Remaining);
        Assert.False(timer.IsRunning);
    }
}
