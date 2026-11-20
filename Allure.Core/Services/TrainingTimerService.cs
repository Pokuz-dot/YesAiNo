namespace Allure.Core.Services;

/// <summary>
/// Таймер длительности академического часа верховой езды (45 минут).
/// Чистая логика без UI: UI-сторона тикает через DispatcherTimer и
/// по событию Finished подаёт звуковой сигнал об окончании смены.
/// </summary>
public sealed class TrainingTimerService
{
    public static readonly TimeSpan AcademicHour = TimeSpan.FromMinutes(45);

    public TimeSpan Remaining { get; private set; } = AcademicHour;
    public bool IsRunning { get; private set; }

    public event EventHandler? Finished;

    public void Start() => IsRunning = true;

    public void Pause() => IsRunning = false;

    public void Reset()
    {
        Remaining = AcademicHour;
        IsRunning = false;
    }

    /// <summary>Продвигает таймер на elapsed; при истечении — событие Finished.</summary>
    public void Tick(TimeSpan elapsed)
    {
        if (!IsRunning)
            return;
        Remaining -= elapsed;
        if (Remaining <= TimeSpan.Zero)
        {
            Remaining = TimeSpan.Zero;
            IsRunning = false;
            Finished?.Invoke(this, EventArgs.Empty);
        }
    }

    public string FormatRemaining() =>
        $"{(int)Remaining.TotalMinutes:D2}:{Remaining.Seconds:D2}";
}
