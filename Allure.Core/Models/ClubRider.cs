namespace Allure.Core.Models;

/// <summary>
/// Всадник клуба (таблица club_riders).
/// DiscountRate — скидка в процентах (0–100), например 10 = 10%.
/// </summary>
public class ClubRider
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? SkillLevel { get; set; }
    public decimal DiscountRate { get; set; }

    public ICollection<RidingOrder> Orders { get; set; } = new List<RidingOrder>();
}
