using Allure.Core.Models;

namespace Allure.Core.Services;

/// <summary>
/// Строгий FSM: Забронировано -> Лошадь оседлана -> Тренировка в манеже ->
/// Завершена успешно / Отмена по ветпоказаниям.
/// </summary>
public sealed class OrderStatusService : IOrderStatusService
{
    public const int MinCancelReasonLength = 10;

    private static readonly Dictionary<string, string[]> AllowedTransitions = new()
    {
        [OrderStatuses.Booked] = new[] { OrderStatuses.HorseSaddled, OrderStatuses.CancelledVet },
        [OrderStatuses.HorseSaddled] = new[] { OrderStatuses.Training, OrderStatuses.CancelledVet },
        [OrderStatuses.Training] = new[] { OrderStatuses.Completed, OrderStatuses.CancelledVet },
        [OrderStatuses.Completed] = Array.Empty<string>(),
        [OrderStatuses.CancelledVet] = Array.Empty<string>()
    };

    public bool CanTransition(string fromStatus, string toStatus) =>
        AllowedTransitions.TryGetValue(fromStatus, out var next) && next.Contains(toStatus);

    public bool IsEditable(string status) => status == OrderStatuses.Booked;

    public void EnsureEditable(RidingOrder order)
    {
        ArgumentNullException.ThrowIfNull(order);
        if (!IsEditable(order.Status))
            throw new InvalidOperationException(
                $"Заказ {order.OrderNum} в статусе «{order.Status}» закрыт для редактирования.");
    }

    public void Transition(RidingOrder order, string newStatus, string? cancelReason = null, int itemCount = 0)
    {
        ArgumentNullException.ThrowIfNull(order);

        if (!CanTransition(order.Status, newStatus))
            throw new InvalidOperationException(
                $"Переход заказа {order.OrderNum} из статуса «{order.Status}» в «{newStatus}» запрещён FSM.");

        if (newStatus == OrderStatuses.HorseSaddled && itemCount == 0)
            throw new InvalidOperationException(
                "Нельзя перевести пустой заказ (0 позиций) в статус «Лошадь оседлана».");

        if (newStatus == OrderStatuses.CancelledVet)
        {
            if (string.IsNullOrWhiteSpace(cancelReason) || cancelReason.Trim().Length < MinCancelReasonLength)
                throw new InvalidOperationException(
                    $"Отмена заказа требует причину длиной не менее {MinCancelReasonLength} символов.");
            order.CancelReason = cancelReason.Trim();
        }

        order.Status = newStatus;
    }

    public void EnsureHorseReady(Horse horse)
    {
        ArgumentNullException.ThrowIfNull(horse);
        if (!horse.IsReadyForTraining)
            throw new InvalidOperationException(
                $"Лошадь «{horse.Nickname}» не допущена к тренировкам (is_ready_for_training = false).");
    }
}
