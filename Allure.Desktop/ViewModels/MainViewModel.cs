using Allure.Core.Models;
using Allure.Core.Services;
using Allure.Desktop.Services;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace Allure.Desktop.ViewModels;

/// <summary>
/// Главное окно: переключатель «Клуб / Ипподром», список заказов с поиском,
/// фильтрацией по статусам и сортировкой, таймер академического часа (45 мин).
/// </summary>
public partial class MainViewModel : ObservableObject
{
    private readonly DbContextFactory _dbFactory;
    private readonly SessionService _session;
    private readonly TrainingTimerService _timer = new();
    private readonly DispatcherTimer _uiTimer;

    private readonly System.Collections.Generic.List<OrderRowViewModel> _allOrders = new();
    private string _sortColumn = nameof(OrderRowViewModel.OrderDate);
    private bool _sortAscending = true;

    [ObservableProperty]
    private string _currentUserText = string.Empty;

    [ObservableProperty]
    private string _currentUserRole = string.Empty;

    [ObservableProperty]
    private bool _isClubMode = true;

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private string _statusFilter = "Все";

    [ObservableProperty]
    private OrderRowViewModel? _selectedOrder;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private string _timerText = "45:00";

    [ObservableProperty]
    private string _hippodromeStats = string.Empty;

    public ObservableCollection<OrderRowViewModel> Orders { get; } = new();
    public ObservableCollection<HorseRowViewModel> Horses { get; } = new();
    public ObservableCollection<RiderRowViewModel> Riders { get; } = new();
    public ObservableCollection<ScheduleRowViewModel> Schedule { get; } = new();
    public System.Collections.Generic.List<string> Statuses { get; } =
        new() { "Все" }.Concat(OrderStatuses.All).ToList();

    public bool IsHippodromeMode => !IsClubMode;

    public event Action? LogoutRequested;
    public event Action<int>? OpenOrderRequested;
    public event Action? NewOrderRequested;
    public event Action<int>? ExportRequested;
    public event Func<int, System.Threading.Tasks.Task<bool>>? DeleteRequested;

    public MainViewModel(DbContextFactory dbFactory, SessionService session)
    {
        _dbFactory = dbFactory ?? throw new ArgumentNullException(nameof(dbFactory));
        _session = session ?? throw new ArgumentNullException(nameof(session));

        if (_session.CurrentUser is not null)
        {
            CurrentUserText = _session.CurrentUser.FullName;
            CurrentUserRole = _session.CurrentUser.Role?.Name ?? string.Empty;
        }

        _timer.Finished += (_, _) =>
        {
            TimerText = "00:00 — смена окончена!";
            try { Console.Beep(880, 600); } catch { /* звуковой сигнал доступен на Windows */ }
        };

        _uiTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        _uiTimer.Tick += (_, _) =>
        {
            _timer.Tick(TimeSpan.FromSeconds(1));
            TimerText = _timer.FormatRemaining();
        };
    }

    // ---------- Переключатель Клуб / Ипподром ----------

    [RelayCommand]
    private void ShowClub()
    {
        IsClubMode = true;
        OnPropertyChanged(nameof(IsHippodromeMode));
    }

    [RelayCommand]
    private void ShowHippodrome()
    {
        IsClubMode = false;
        OnPropertyChanged(nameof(IsHippodromeMode));
    }

    // ---------- Загрузка данных ----------

    [RelayCommand]
    public void Refresh()
    {
        ErrorMessage = string.Empty;
        try
        {
            using var context = _dbFactory.CreateDbContext();
            DatabaseSeeder.SeedIfEmpty(context);

            var orders = context.RidingOrders
                .Include(o => o.ClubRider)
                .Include(o => o.Items).ThenInclude(i => i.Horse)
                .OrderByDescending(o => o.OrderDate)
                .ToList();

            _allOrders.Clear();
            _allOrders.AddRange(orders.Select(o =>
                new OrderRowViewModel(o, o.ClubRider?.FullName ?? string.Empty)));

            Horses.Clear();
            foreach (HorseRowViewModel row in context.Horses.Include(h => h.Breed)
                         .OrderBy(h => h.Nickname).Select(h => new HorseRowViewModel(h)))
                Horses.Add(row);

            Riders.Clear();
            foreach (RiderRowViewModel row in context.ClubRiders
                         .OrderBy(r => r.FullName).Select(r => new RiderRowViewModel(r)))
                Riders.Add(row);

            Schedule.Clear();
            foreach (RidingOrder order in orders)
            {
                foreach (RidingSessionItem item in order.Items.OrderBy(i => i.SessionDateTime))
                {
                    Schedule.Add(new ScheduleRowViewModel(order.OrderNum, item.SessionDateTime,
                        item.Horse?.Nickname ?? $"Лошадь #{item.HorseId}",
                        order.ClubRider?.FullName ?? string.Empty,
                        item.DurationMinutes, item.HasTrainer));
                }
            }

            int active = orders.Count(o => o.Status == OrderStatuses.Training);
            decimal revenue = orders.Where(o => o.Status == OrderStatuses.Completed).Sum(o => o.TotalPrice);
            HippodromeStats = $"Активных тренировок сейчас: {active}. " +
                              $"Выручка по завершённым заказам: {revenue:F2} руб. " +
                              $"Всего занятий в расписании: {Schedule.Count}.";
        }
        catch (Exception ex)
        {
            ErrorMessage = "Ошибка загрузки данных. Проверьте MySQL и строку подключения. " + ex.Message;
            return;
        }

        ApplyFilter();
    }

    private void ApplyFilter()
    {
        Orders.Clear();
        IEnumerable<OrderRowViewModel> query = _allOrders;

        if (StatusFilter != "Все")
            query = query.Where(o => o.Status == StatusFilter);

        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            string needle = SearchText.Trim().ToLowerInvariant();
            query = query.Where(o => o.OrderNum.ToLowerInvariant().Contains(needle)
                                     || o.RiderName.ToLowerInvariant().Contains(needle));
        }

        query = Sort(query);
        foreach (OrderRowViewModel row in query)
            Orders.Add(row);
    }

    private IEnumerable<OrderRowViewModel> Sort(IEnumerable<OrderRowViewModel> source) =>
        (_sortColumn, _sortAscending) switch
        {
            (nameof(OrderRowViewModel.OrderNum), true) => source.OrderBy(o => o.OrderNum),
            (nameof(OrderRowViewModel.OrderNum), false) => source.OrderByDescending(o => o.OrderNum),
            (nameof(OrderRowViewModel.RiderName), true) => source.OrderBy(o => o.RiderName),
            (nameof(OrderRowViewModel.RiderName), false) => source.OrderByDescending(o => o.RiderName),
            (nameof(OrderRowViewModel.TotalPrice), true) => source.OrderBy(o => o.TotalPrice),
            (nameof(OrderRowViewModel.TotalPrice), false) => source.OrderByDescending(o => o.TotalPrice),
            (nameof(OrderRowViewModel.Status), true) => source.OrderBy(o => o.Status),
            (nameof(OrderRowViewModel.Status), false) => source.OrderByDescending(o => o.Status),
            (_, true) => source.OrderBy(o => o.OrderDate),
            _ => source.OrderByDescending(o => o.OrderDate)
        };

    public void SortBy(string column)
    {
        if (_sortColumn == column)
            _sortAscending = !_sortAscending;
        else
        {
            _sortColumn = column;
            _sortAscending = true;
        }
        ApplyFilter();
    }

    partial void OnSearchTextChanged(string value) => ApplyFilter();

    partial void OnStatusFilterChanged(string value) => ApplyFilter();

    // ---------- Команды над заказами ----------

    [RelayCommand]
    private void OpenOrder()
    {
        if (SelectedOrder is not null)
            OpenOrderRequested?.Invoke(SelectedOrder.OrderId);
    }

    [RelayCommand]
    private void NewOrder() => NewOrderRequested?.Invoke();

    [RelayCommand]
    private void ExportOrder()
    {
        if (SelectedOrder is not null)
            ExportRequested?.Invoke(SelectedOrder.OrderId);
    }

    [RelayCommand]
    private async System.Threading.Tasks.Task DeleteOrder()
    {
        if (SelectedOrder is null)
            return;
        int orderId = SelectedOrder.OrderId;
        bool deleted = DeleteRequested is not null
            && await DeleteRequested.Invoke(orderId);
        if (deleted)
            Refresh();
    }

    [RelayCommand]
    private void Logout()
    {
        _session.SignOut();
        LogoutRequested?.Invoke();
    }

    // ---------- Таймер академического часа (45 мин) ----------

    [RelayCommand]
    private void StartTimer()
    {
        _timer.Start();
        _uiTimer.Start();
    }

    [RelayCommand]
    private void PauseTimer()
    {
        _timer.Pause();
        _uiTimer.Stop();
    }

    [RelayCommand]
    private void ResetTimer()
    {
        _timer.Reset();
        _uiTimer.Stop();
        TimerText = _timer.FormatRemaining();
    }
}
