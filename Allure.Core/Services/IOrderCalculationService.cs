using Allure.Core.Models;

namespace Allure.Core.Services;

/// <summary>
/// Сервис расчёта стоимости (ПК 1.1, ПК 1.2). Не зависит от UI.
/// Формула: (duration_minutes / 45) * 1500 руб.; +1000 руб. за тренера;
/// скидка discount_rate (%) для опытных всадников.
/// </summary>
public interface IOrderCalculationService
{
    decimal CalculateItemCost(int durationMinutes, bool hasTrainer, decimal discountRate);

    decimal CalculateOrderTotal(IEnumerable<RidingSessionItem> items, decimal discountRate);

    /// <summary>Контроль переутомления: лошадь — не более 3 часов в сутки.</summary>
    bool IsHorseOverloaded(int alreadyBookedMinutesToday, int newDurationMinutes);
}
