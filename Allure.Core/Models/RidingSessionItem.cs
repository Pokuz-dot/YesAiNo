namespace Allure.Core.Models;

/// <summary>
/// Позиция заказа — одно занятие (таблица riding_session_items, Detail).
/// </summary>
public class RidingSessionItem
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public int HorseId { get; set; }
    public DateTime SessionDateTime { get; set; }
    public int DurationMinutes { get; set; } = 45;
    public bool HasTrainer { get; set; }
    public decimal ItemCost { get; set; }

    public RidingOrder? Order { get; set; }
    public Horse? Horse { get; set; }
}
