using Allure.Core.Models;
using System;

namespace Allure.Desktop.ViewModels;

/// <summary>Строка таблицы всадников клуба.</summary>
public sealed class RiderRowViewModel
{
    public int Id { get; }
    public string FullName { get; }
    public string Phone { get; }
    public string SkillLevel { get; }
    public decimal DiscountRate { get; }
    public string DiscountText => $"{DiscountRate:F2}%";

    public RiderRowViewModel(ClubRider rider)
    {
        ArgumentNullException.ThrowIfNull(rider);
        Id = rider.Id;
        FullName = rider.FullName;
        Phone = rider.Phone;
        SkillLevel = rider.SkillLevel ?? string.Empty;
        DiscountRate = rider.DiscountRate;
    }
}
