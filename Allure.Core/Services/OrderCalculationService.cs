using Allure.Core.Models;

namespace Allure.Core.Services;

/// <summary>Реализация расчётной формулы варианта 22 (см. карточку задания).</summary>
public sealed class OrderCalculationService : IOrderCalculationService
{
    /// <summary>Длительность академического часа верховой езды, мин.</summary>
    public const int AcademicHourMinutes = 45;

    /// <summary>Стоимость одного академического часа, руб.</summary>
    public const decimal PricePerAcademicHour = 1500m;

    /// <summary>Надбавка за услуги тренера за занятие, руб.</summary>
    public const decimal TrainerSurcharge = 1000m;

    /// <summary>Максимум тренировочных минут лошади в сутки (3 часа).</summary>
    public const int MaxHorseMinutesPerDay = 180;

    public decimal CalculateItemCost(int durationMinutes, bool hasTrainer, decimal discountRate)
    {
        if (durationMinutes <= 0)
            throw new ArgumentOutOfRangeException(nameof(durationMinutes),
                "Длительность занятия должна быть положительной.");
        if (discountRate < 0 || discountRate > 100)
            throw new ArgumentOutOfRangeException(nameof(discountRate),
                "Скидка должна быть в диапазоне 0–100 (%).");

        decimal baseCost = (durationMinutes / (decimal)AcademicHourMinutes) * PricePerAcademicHour;
        if (hasTrainer)
            baseCost += TrainerSurcharge;

        decimal discount = baseCost * discountRate / 100m;
        return Math.Round(baseCost - discount, 2);
    }

    public decimal CalculateOrderTotal(IEnumerable<RidingSessionItem> items, decimal discountRate)
    {
        ArgumentNullException.ThrowIfNull(items);
        return Math.Round(
            items.Sum(i => CalculateItemCost(i.DurationMinutes, i.HasTrainer, discountRate)), 2);
    }

    public bool IsHorseOverloaded(int alreadyBookedMinutesToday, int newDurationMinutes) =>
        alreadyBookedMinutesToday + newDurationMinutes > MaxHorseMinutesPerDay;
}
