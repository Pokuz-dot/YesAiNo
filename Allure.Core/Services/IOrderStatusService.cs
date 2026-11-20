using Allure.Core.Models;

namespace Allure.Core.Services;

/// <summary>
/// Сервис конечного автомата состояний заказа (FSM).
/// Любой недопустимый переход бросает InvalidOperationException.
/// </summary>
public interface IOrderStatusService
{
    bool CanTransition(string fromStatus, string toStatus);

    /// <summary>Редактирование разрешено только в статусе «Забронировано».</summary>
    bool IsEditable(string status);

    /// <summary>Проверяет, что заказ можно редактировать, иначе бросает исключение.</summary>
    void EnsureEditable(RidingOrder order);

    /// <summary>
    /// Переводит заказ в новый статус с проверками:
    /// пустой заказ нельзя перевести в «Лошадь оседлана»;
    /// отмена требует причину длиной не менее 10 символов.
    /// </summary>
    void Transition(RidingOrder order, string newStatus, string? cancelReason = null, int itemCount = 0);

    /// <summary>Назначение лошади на тренировку запрещено при is_ready_for_training == false.</summary>
    void EnsureHorseReady(Horse horse);
}
