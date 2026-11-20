using Allure.Core.Models;
using Allure.Core.Services;
using Allure.Desktop.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;

namespace Allure.Desktop.ViewModels;

/// <summary>Позиция заказа (Detail) в Master-Detail форме.</summary>
public partial class SessionItemViewModel : ObservableObject
{
    [ObservableProperty]
    private int _horseId;

    [ObservableProperty]
    private DateTime _sessionDateTime = DateTime.Now;

    [ObservableProperty]
    private int _durationMinutes = 45;

    [ObservableProperty]
    private bool _hasTrainer;

    [ObservableProperty]
    private decimal _itemCost;

    public string HorseName { get; set; } = string.Empty;
    public string SessionText => SessionDateTime.ToString("yyyy-MM-dd HH:mm");
    public string CostText => ItemCost.ToString("F2");

    partial void OnSessionDateTimeChanged(DateTime value) => OnPropertyChanged(nameof(SessionText));

    partial void OnItemCostChanged(decimal value) => OnPropertyChanged(nameof(CostText));
}

/// <summary>
/// Master-Detail: заголовок заказа + спецификация позиций.
/// Итог пересчитывается на лету при добавлении/удалении/изменении позиций.
/// Сохранение — транзакционно, с проверками FSM и контроля переутомления.
/// </summary>
public partial class OrderViewModel : ObservableObject
{
    private readonly DbContextFactory _dbFactory;
    private readonly IOrderCalculationService _calc;
    private readonly IOrderStatusService _status;
    private readonly IDocumentNumberGenerator _numbers;
    private readonly SessionService _session;

    [ObservableProperty]
    private int _orderId;

    [ObservableProperty]
    private string _orderNum = string.Empty;

    [ObservableProperty]
    private DateTime _orderDate = DateTime.Now;

    [ObservableProperty]
    private string _status = OrderStatuses.Booked;

    [ObservableProperty]
    private int _riderId;

    [ObservableProperty]
    private decimal _totalPrice;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private SessionItemViewModel? _selectedItem;

    public ObservableCollection<SessionItemViewModel> Items { get; } = new();
    public ObservableCollection<ClubRider> Riders { get; } = new();
    public ObservableCollection<Horse> Horses { get; } = new();
    public System.Collections.Generic.List<string> Statuses { get; } = OrderStatuses.All.ToList();

    public bool IsEditable => _status.IsEditable(Status);
    public bool IsNew => OrderId == 0;

    /// <summary>Заполняется окном: показать диалог причины отмены, вернуть причину или null.</summary>
    public Func<System.Threading.Tasks.Task<string?>>? RequestCancelReason { get; set; }

    public event Action? Saved;

    public OrderViewModel(DbContextFactory dbFactory, IOrderCalculationService calc,
        IOrderStatusService status, IDocumentNumberGenerator numbers, SessionService session)
    {
        _dbFactory = dbFactory ?? throw new ArgumentNullException(nameof(dbFactory));
        _calc = calc ?? throw new ArgumentNullException(nameof(calc));
        _status = status ?? throw new ArgumentNullException(nameof(status));
        _numbers = numbers ?? throw new ArgumentNullException(nameof(numbers));
        _session = session ?? throw new ArgumentNullException(nameof(session));
        Items.CollectionChanged += OnItemsChanged;
    }

    private void OnItemsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.OldItems is not null)
            foreach (SessionItemViewModel item in e.OldItems.OfType<SessionItemViewModel>())
                item.PropertyChanged -= OnItemPropertyChanged;
        if (e.NewItems is not null)
            foreach (SessionItemViewModel item in e.NewItems.OfType<SessionItemViewModel>())
                item.PropertyChanged += OnItemPropertyChanged;
        Recalculate();
    }

    private void OnItemPropertyChanged(object? sender, PropertyChangedEventArgs e) => Recalculate();

    private decimal CurrentDiscount() =>
        Riders.FirstOrDefault(r => r.Id == RiderId)?.DiscountRate ?? 0m;

    /// <summary>Пересчёт итогов документа на лету (Data Binding).</summary>
    public void Recalculate()
    {
        TotalPrice = _calc.CalculateOrderTotal(
            Items.Select(i => new RidingSessionItem
            {
                DurationMinutes = i.DurationMinutes,
                HasTrainer = i.HasTrainer
            }), CurrentDiscount());
        foreach (SessionItemViewModel item in Items)
        {
            item.ItemCost = _calc.CalculateItemCost(item.DurationMinutes, item.HasTrainer, CurrentDiscount());
        }
    }

    public void LoadForEdit(int orderId)
    {
        using var context = _dbFactory.CreateDbContext();
        RidingOrder order = context.RidingOrders
            .Include(o => o.Items).ThenInclude(i => i.Horse)
            .First(o => o.Id == orderId);

        LoadReferenceData(context);
        OrderId = order.Id;
        OrderNum = order.OrderNum;
        OrderDate = order.OrderDate;
        Status = order.Status;
        RiderId = order.ClubRiderId;

        Items.Clear();
        foreach (RidingSessionItem item in order.Items.OrderBy(i => i.SessionDateTime))
        {
            Items.Add(new SessionItemViewModel
            {
                HorseId = item.HorseId,
                HorseName = item.Horse?.Nickname ?? string.Empty,
                SessionDateTime = item.SessionDateTime,
                DurationMinutes = item.DurationMinutes,
                HasTrainer = item.HasTrainer,
                ItemCost = item.ItemCost
            });
        }
        Recalculate();
        OnPropertyChanged(nameof(IsEditable));
        OnPropertyChanged(nameof(IsNew));
    }

    public void LoadForNew()
    {
        using var context = _dbFactory.CreateDbContext();
        LoadReferenceData(context);
        OrderId = 0;
        int year = DateTime.Now.Year;
        var existing = context.RidingOrders.Select(o => o.OrderNum).ToList();
        OrderNum = _numbers.Generate(year, _numbers.NextIndex(existing, year));
        OrderDate = DateTime.Now;
        Status = OrderStatuses.Booked;
        RiderId = Riders.FirstOrDefault()?.Id ?? 0;
        Items.Clear();
        Recalculate();
        OnPropertyChanged(nameof(IsEditable));
        OnPropertyChanged(nameof(IsNew));
    }

    private void LoadReferenceData(Allure.Core.Data.AppDbContext context)
    {
        Riders.Clear();
        foreach (ClubRider rider in context.ClubRiders.OrderBy(r => r.FullName))
            Riders.Add(rider);
        Horses.Clear();
        foreach (Horse horse in context.Horses.OrderBy(h => h.Nickname))
            Horses.Add(horse);
    }

    [RelayCommand]
    private void AddItem()
    {
        if (!IsEditable)
        {
            ErrorMessage = "Заказ закрыт для редактирования.";
            return;
        }
        int horseId = Horses.FirstOrDefault()?.Id ?? 0;
        Items.Add(new SessionItemViewModel
        {
            HorseId = horseId,
            HorseName = Horses.FirstOrDefault(h => h.Id == horseId)?.Nickname ?? string.Empty,
            SessionDateTime = DateTime.Now,
            DurationMinutes = 45,
            HasTrainer = false
        });
        Recalculate();
    }

    [RelayCommand]
    private void RemoveItem()
    {
        if (SelectedItem is not null)
            Items.Remove(SelectedItem);
        Recalculate();
    }

    [RelayCommand]
    private void Save()
    {
        ErrorMessage = string.Empty;
        try
        {
            using var context = _dbFactory.CreateDbContext();
            using var transaction = context.Database.BeginTransaction();

            bool isNew = IsNew;
            RidingOrder order;
            if (isNew)
            {
                order = new RidingOrder
                {
                    OrderNum = OrderNum,
                    UserId = _session.CurrentUser?.Id ?? 1,
                    ClubRiderId = RiderId,
                    OrderDate = OrderDate,
                    Status = Status
                };
                context.RidingOrders.Add(order);
            }
            else
            {
                order = context.RidingOrders.First(o => o.Id == OrderId);
                _status.EnsureEditable(order);
                var oldItems = context.RidingSessionItems
                    .Where(i => i.OrderId == order.Id).ToList();
                context.RidingSessionItems.RemoveRange(oldItems);
            }

            decimal discount = CurrentDiscount();
            var newEntities = new System.Collections.Generic.List<RidingSessionItem>();
            foreach (SessionItemViewModel item in Items)
            {
                if (item.DurationMinutes <= 0)
                    throw new InvalidOperationException("Длительность занятия должна быть положительной.");

                Horse horse = context.Horses.First(h => h.Id == item.HorseId);
                _status.EnsureHorseReady(horse);

                int bookedToday = context.RidingSessionItems
                    .Where(i => i.HorseId == item.HorseId
                                && i.SessionDateTime.Date == item.SessionDateTime.Date
                                && i.OrderId != order.Id)
                    .Sum(i => (int?)i.DurationMinutes) ?? 0;
                if (_calc.IsHorseOverloaded(bookedToday, item.DurationMinutes))
                    throw new InvalidOperationException(
                        $"Лошадь «{horse.Nickname}» превысит лимит 3 часов тренировок в сутки.");

                var entity = new RidingSessionItem
                {
                    HorseId = item.HorseId,
                    SessionDateTime = item.SessionDateTime,
                    DurationMinutes = item.DurationMinutes,
                    HasTrainer = item.HasTrainer,
                    ItemCost = _calc.CalculateItemCost(item.DurationMinutes, item.HasTrainer, discount)
                };
                if (isNew)
                {
                    order.Items.Add(entity); // FK подставится при SaveChanges
                }
                else
                {
                    entity.OrderId = order.Id;
                    context.RidingSessionItems.Add(entity);
                }
                newEntities.Add(entity);
            }

            order.TotalPrice = _calc.CalculateOrderTotal(newEntities, discount);
            context.SaveChanges();
            transaction.Commit();

            OrderId = order.Id;
            TotalPrice = order.TotalPrice;
            OnPropertyChanged(nameof(IsNew));
            Saved?.Invoke();
        }
        catch (InvalidOperationException ex)
        {
            ErrorMessage = ex.Message;
        }
        catch (Exception ex)
        {
            ErrorMessage = "Ошибка сохранения: " + ex.Message;
        }
    }

    [RelayCommand]
    private async System.Threading.Tasks.Task ChangeStatus(string? newStatus)
    {
        if (string.IsNullOrWhiteSpace(newStatus) || newStatus == Status)
            return;
        ErrorMessage = string.Empty;
        try
        {
            string? cancelReason = null;
            if (newStatus == OrderStatuses.CancelledVet)
            {
                cancelReason = RequestCancelReason is not null
                    ? await RequestCancelReason.Invoke()
                    : null;
                if (cancelReason is null)
                    return; // пользователь отменил ввод причины
            }

            using var context = _dbFactory.CreateDbContext();
            RidingOrder order = context.RidingOrders.Include(o => o.Items)
                .First(o => o.Id == OrderId);
            _status.Transition(order, newStatus, cancelReason, order.Items.Count);
            context.SaveChanges();

            Status = order.Status;
            OnPropertyChanged(nameof(IsEditable));
        }
        catch (InvalidOperationException ex)
        {
            ErrorMessage = ex.Message;
        }
        catch (Exception ex)
        {
            ErrorMessage = "Ошибка смены статуса: " + ex.Message;
        }
    }
}
