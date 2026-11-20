using Allure.Core.Models;
using Allure.Core.Services;
using Xunit;

namespace Allure.Tests;

/// <summary>Тесты 1–2 по спецификации ПК 1.4: расчёт стоимости и скидок.</summary>
public sealed class OrderCalculationServiceTests
{
    private readonly OrderCalculationService _service = new();

    [Fact]
    public void CalculateItemCost_StandardParameters_Returns1500()
    {
        // Тест 1: стандарт — 45 мин, без тренера, без скидки.
        decimal cost = _service.CalculateItemCost(45, hasTrainer: false, discountRate: 0m);
        Assert.Equal(1500m, cost);
    }

    [Fact]
    public void CalculateItemCost_WithTrainerAndDiscount_AppliesSurchargeAndDiscount()
    {
        // Тест 2: 90 мин = 2 акад. часа (3000) + тренер (1000) = 4000; скидка 10% -> 3600.
        decimal cost = _service.CalculateItemCost(90, hasTrainer: true, discountRate: 10m);
        Assert.Equal(3600m, cost);
    }

    [Fact]
    public void CalculateOrderTotal_SumsItemsWithDiscount()
    {
        var items = new[]
        {
            new RidingSessionItem { DurationMinutes = 45, HasTrainer = true },   // 2500 -> 2250
            new RidingSessionItem { DurationMinutes = 90, HasTrainer = false }   // 3000 -> 2700
        };
        Assert.Equal(4950m, _service.CalculateOrderTotal(items, discountRate: 10m));
    }

    [Fact]
    public void CalculateItemCost_NegativeDuration_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => _service.CalculateItemCost(-45, false, 0m));
    }

    [Fact]
    public void IsHorseOverloaded_MoreThanThreeHours_ReturnsTrue()
    {
        Assert.True(_service.IsHorseOverloaded(alreadyBookedMinutesToday: 150, newDurationMinutes: 45));
        Assert.False(_service.IsHorseOverloaded(alreadyBookedMinutesToday: 135, newDurationMinutes: 45));
    }
}
