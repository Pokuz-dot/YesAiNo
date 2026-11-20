using Allure.Core.Models;
using Allure.Core.Services;
using Xunit;

namespace Allure.Tests;

/// <summary>Тесты 3–4 по спецификации ПК 1.4: FSM и InvalidOperationException.</summary>
public sealed class OrderStatusServiceTests
{
    private readonly OrderStatusService _service = new();

    private static RidingOrder NewOrder(string status) => new()
    {
        OrderNum = "ПР23-КСК-2026-00001",
        Status = status
    };

    [Fact]
    public void Transition_AllowedChain_Succeeds()
    {
        // Тест 3: Забронировано -> Лошадь оседлана -> Тренировка в манеже -> Завершена успешно.
        RidingOrder order = NewOrder(OrderStatuses.Booked);
        _service.Transition(order, OrderStatuses.HorseSaddled, itemCount: 2);
        _service.Transition(order, OrderStatuses.Training, itemCount: 2);
        _service.Transition(order, OrderStatuses.Completed, itemCount: 2);
        Assert.Equal(OrderStatuses.Completed, order.Status);
    }

    [Fact]
    public void Transition_ForbiddenJump_ThrowsInvalidOperationException()
    {
        // Тест 4: Забронировано -> Завершена успешно запрещён FSM.
        RidingOrder order = NewOrder(OrderStatuses.Booked);
        Assert.Throws<InvalidOperationException>(() =>
            _service.Transition(order, OrderStatuses.Completed, itemCount: 1));
    }

    [Fact]
    public void Transition_EmptyOrderToSaddled_ThrowsInvalidOperationException()
    {
        RidingOrder order = NewOrder(OrderStatuses.Booked);
        Assert.Throws<InvalidOperationException>(() =>
            _service.Transition(order, OrderStatuses.HorseSaddled, itemCount: 0));
    }

    [Fact]
    public void Transition_CancelWithoutReason_ThrowsInvalidOperationException()
    {
        RidingOrder order = NewOrder(OrderStatuses.Booked);
        Assert.Throws<InvalidOperationException>(() =>
            _service.Transition(order, OrderStatuses.CancelledVet, cancelReason: "коротко", itemCount: 1));
    }

    [Fact]
    public void EnsureEditable_ClosedOrder_ThrowsInvalidOperationException()
    {
        RidingOrder order = NewOrder(OrderStatuses.Completed);
        Assert.Throws<InvalidOperationException>(() => _service.EnsureEditable(order));
    }

    [Fact]
    public void EnsureHorseReady_NotReadyHorse_ThrowsInvalidOperationException()
    {
        var horse = new Horse { Nickname = "Молния", IsReadyForTraining = false };
        Assert.Throws<InvalidOperationException>(() => _service.EnsureHorseReady(horse));
    }
}
