namespace Allure.Core.Models;

/// <summary>
/// Статусы жизненного цикла заказа (FSM). Значения совпадают с enum в MySQL.
/// Забронировано -> Лошадь оседлана -> Тренировка в манеже ->
/// Завершена успешно / Отмена по ветпоказаниям.
/// </summary>
public static class OrderStatuses
{
    public const string Booked = "Забронировано";
    public const string HorseSaddled = "Лошадь оседлана";
    public const string Training = "Тренировка в манеже";
    public const string Completed = "Завершена успешно";
    public const string CancelledVet = "Отмена по ветпоказаниям";

    public static readonly string[] All =
    {
        Booked, HorseSaddled, Training, Completed, CancelledVet
    };

    /// <summary>Закрытые статусы: заказ нельзя редактировать.</summary>
    public static bool IsClosed(string status) =>
        status == Completed || status == CancelledVet;
}
