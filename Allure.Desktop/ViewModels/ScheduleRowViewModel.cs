using System;

namespace Allure.Desktop.ViewModels;

/// <summary>Строка расписания ипподрома (позиция заказа).</summary>
public sealed class ScheduleRowViewModel
{
    public string OrderNum { get; }
    public DateTime SessionDateTime { get; }
    public string SessionText => SessionDateTime.ToString("yyyy-MM-dd HH:mm");
    public string HorseName { get; }
    public string RiderName { get; }
    public int DurationMinutes { get; }
    public bool HasTrainer { get; }
    public string TrainerText => HasTrainer ? "Да" : "Нет";

    public ScheduleRowViewModel(string orderNum, DateTime sessionDateTime, string horseName,
        string riderName, int durationMinutes, bool hasTrainer)
    {
        OrderNum = orderNum;
        SessionDateTime = sessionDateTime;
        HorseName = horseName;
        RiderName = riderName;
        DurationMinutes = durationMinutes;
        HasTrainer = hasTrainer;
    }
}
