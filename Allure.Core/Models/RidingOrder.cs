namespace Allure.Core.Models;

/// <summary>
/// Заголовок заказа на тренировку (таблица riding_orders, Master).
/// Номер документа по маске ПР23-КСК-{Год}-{Номер:D5}.
/// </summary>
public class RidingOrder
{
    public int Id { get; set; }
    public string OrderNum { get; set; } = string.Empty;
    public int UserId { get; set; }
    public int ClubRiderId { get; set; }
    public DateTime OrderDate { get; set; } = DateTime.Now;
    public decimal TotalPrice { get; set; }
    public string Status { get; set; } = OrderStatuses.Booked;
    public string? CancelReason { get; set; }

    public User? User { get; set; }
    public ClubRider? ClubRider { get; set; }
    public ICollection<RidingSessionItem> Items { get; set; } = new List<RidingSessionItem>();
}
