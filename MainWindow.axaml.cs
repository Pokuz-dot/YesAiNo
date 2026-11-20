using Avalonia.Controls;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Avalo;

public partial class MainWindow : Window
{
    private DbUser? _currentUser;
    private List<Order> _allOrders = new();
    private List<Horse> _horses = new();
    private List<Rider> _riders = new();

    // Блокировка входа: 3 неверных ввода -> 10 секунд.
    private int _failedAttempts;
    private DateTime? _lockoutUntil;
    private DispatcherTimer? _lockoutTimer;

    // Таймер академического часа (45 минут).
    private TimeSpan _timerLeft = TimeSpan.FromMinutes(45);
    private bool _timerRunning;
    private DispatcherTimer? _timer;

    public MainWindow()
    {
        InitializeComponent();

        BtnClub.Click += (_, _) => ShowClub();
        BtnHippodrome.Click += (_, _) => ShowHippodrome();
        BtnLogin.Click += (_, _) => TryLogin();
        ChkShowPassword.Click += (_, _) =>
            TxtPassword.PasswordChar = ChkShowPassword.IsChecked == true ? '\0' : '•';
        BtnLogout.Click += (_, _) => Logout();
        BtnRefresh.Click += (_, _) => LoadData();
        BtnNewOrder.Click += (_, _) => OpenOrderWindow(null);
        BtnOpenOrder.Click += (_, _) => OpenOrderWindow(SelectedOrder());
        BtnDelete.Click += (_, _) => DeleteSelected();
        BtnExport.Click += (_, _) => ExportSelected();
        TxtSearch.TextChanged += (_, _) => ApplyFilter();
        CmbStatus.SelectionChanged += (_, _) => ApplyFilter();
        OrdersGrid.DoubleTapped += (_, _) => OpenOrderWindow(SelectedOrder());

        CmbStatus.ItemsSource = new[] { "Все" }.Concat(Statuses.All).ToList();
        CmbStatus.SelectedIndex = 0;

        SetupTimer();
    }

    // ---------- Вход ----------

    private void TryLogin()
    {
        if (_lockoutUntil is not null && DateTime.Now < _lockoutUntil.Value)
            return;

        string login = TxtLogin.Text?.Trim() ?? "";
        string password = TxtPassword.Text ?? "";
        if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(password))
        {
            LoginError.Text = "Введите логин и пароль.";
            return;
        }

        DbUser? user;
        try
        {
            user = Db.FindUser(login);
        }
        catch (Exception ex)
        {
            LoginError.Text = "Нет соединения с БД. Импортируйте Database/Yes.sql в phpMyAdmin. " + ex.Message;
            return;
        }

        if (user is null || user.PasswordHash != Calc.PasswordHash(password))
        {
            _failedAttempts++;
            TxtPassword.Text = "";
            if (_failedAttempts >= 3)
            {
                _lockoutUntil = DateTime.Now.AddSeconds(10);
                StartLockoutCountdown();
                LoginError.Text = "Неверный логин или пароль. Интерфейс заблокирован.";
            }
            else
            {
                LoginError.Text = $"Неверный логин или пароль. Осталось попыток: {3 - _failedAttempts}.";
            }
            return;
        }

        _failedAttempts = 0;
        _currentUser = user;
        LoginPanel.IsVisible = false;
        TopPanel.IsVisible = true;
        UserText.Text = user.FullName;
        RoleText.Text = user.RoleName;
        ShowClub();
        LoadData();
    }

    private void StartLockoutCountdown()
    {
        TxtLogin.IsEnabled = false;
        TxtPassword.IsEnabled = false;
        BtnLogin.IsEnabled = false;
        _lockoutTimer?.Stop();
        _lockoutTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        _lockoutTimer.Tick += (_, _) =>
        {
            if (_lockoutUntil is null || DateTime.Now >= _lockoutUntil.Value)
            {
                _lockoutTimer?.Stop();
                TxtLogin.IsEnabled = true;
                TxtPassword.IsEnabled = true;
                BtnLogin.IsEnabled = true;
                LockoutText.Text = "";
                _failedAttempts = 0;
                _lockoutUntil = null;
                LoginError.Text = "Блокировка снята. Попробуйте снова.";
                return;
            }
            int left = (int)Math.Ceiling((_lockoutUntil.Value - DateTime.Now).TotalSeconds);
            LockoutText.Text = $"Повторите через {left} сек.";
        };
        _lockoutTimer.Start();
        LockoutText.Text = "Повторите через 10 сек.";
    }

    private void Logout()
    {
        _currentUser = null;
        TopPanel.IsVisible = false;
        ClubPanel.IsVisible = false;
        HippodromePanel.IsVisible = false;
        LoginPanel.IsVisible = true;
        TxtPassword.Text = "";
    }

    // ---------- Переключатель Клуб / Ипподром ----------

    private void ShowClub()
    {
        BtnClub.Classes.Add("active");
        BtnHippodrome.Classes.Remove("active");
        ClubPanel.IsVisible = true;
        HippodromePanel.IsVisible = false;
    }

    private void ShowHippodrome()
    {
        BtnHippodrome.Classes.Add("active");
        BtnClub.Classes.Remove("active");
        ClubPanel.IsVisible = false;
        HippodromePanel.IsVisible = true;
    }

    // ---------- Данные ----------

    private void LoadData()
    {
        ErrorText.Text = "";
        try
        {
            _horses = Db.LoadHorses();
            _riders = Db.LoadRiders();
            _allOrders = Db.LoadOrders();
        }
        catch (Exception ex)
        {
            ErrorText.Text = "Ошибка загрузки данных. Проверьте MySQL. " + ex.Message;
            return;
        }

        HorsesGrid.ItemsSource = _horses;
        RidersGrid.ItemsSource = _riders;
        ApplyFilter();
        UpdateSchedule();
    }

    private void ApplyFilter()
    {
        string search = (TxtSearch.Text ?? "").Trim().ToLowerInvariant();
        string status = CmbStatus.SelectedItem as string ?? "Все";

        var query = _allOrders.AsEnumerable();
        if (status != "Все")
            query = query.Where(o => o.Status == status);
        if (search.Length > 0)
            query = query.Where(o => o.OrderNum.ToLowerInvariant().Contains(search)
                                     || o.RiderName.ToLowerInvariant().Contains(search));
        OrdersGrid.ItemsSource = query.ToList();
    }

    private void UpdateSchedule()
    {
        var rows = new List<object>();
        foreach (Order order in _allOrders)
        {
            foreach (OrderItem item in order.Items)
            {
                rows.Add(new
                {
                    OrderNum = order.OrderNum,
                    SessionText = item.SessionText,
                    HorseName = item.HorseName,
                    RiderName = order.RiderName,
                    DurationMinutes = item.DurationMinutes,
                    TrainerText = item.TrainerText
                });
            }
        }
        ScheduleGrid.ItemsSource = rows;

        int active = _allOrders.Count(o => o.Status == Statuses.Training);
        decimal revenue = _allOrders.Where(o => o.Status == Statuses.Completed).Sum(o => o.TotalPrice);
        HippoStats.Text = $"Активных тренировок сейчас: {active}. " +
                          $"Выручка по завершённым заказам: {revenue:F2} руб. " +
                          $"Всего занятий в расписании: {rows.Count}.";
    }

    private Order? SelectedOrder() => OrdersGrid.SelectedItem as Order;

    // ---------- Заказы ----------

    private async void OpenOrderWindow(Order? order)
    {
        var window = new OrderWindow(order, _currentUser?.Id ?? 1, _horses, _riders);
        await window.ShowDialog(this);
        LoadData();
    }

    private async void DeleteSelected()
    {
        Order? order = SelectedOrder();
        if (order is null) return;

        var confirm = new ConfirmWindow($"Удалить заказ {order.OrderNum}?");
        if (await confirm.ShowDialog<bool>(this))
        {
            Db.DeleteOrder(order.Id);
            LoadData();
        }
    }

    private async void ExportSelected()
    {
        Order? order = SelectedOrder();
        if (order is null) return;

        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel is null) return;

        var file = await topLevel.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = "Экспорт маршрутного листа",
            SuggestedFileName = $"{order.OrderNum}.txt",
            FileTypeChoices = new[]
            {
                new FilePickerFileType("Текстовый документ", new[] { "*.txt" }),
                new FilePickerFileType("HTML документ", new[] { "*.html" })
            }
        });
        if (file is null) return;
        string path = file.Path.LocalPath;

        string instructor = _currentUser?.FullName ?? "Инструктор";
        string content = path.EndsWith(".html", StringComparison.OrdinalIgnoreCase)
            ? BuildHtml(order, instructor)
            : BuildText(order, instructor);
        System.IO.File.WriteAllText(path, content, Encoding.UTF8);
    }

    private string BuildText(Order order, string instructor)
    {
        var sb = new StringBuilder();
        sb.AppendLine("КОННОСПОРТИВНЫЙ КЛУБ И ИППОДРОМ «АЛЛЮР»");
        sb.AppendLine("МАРШРУТНЫЙ ЛИСТ ТРЕНИРОВКИ И АКТ ПРОВЕРКИ СНАРЯЖЕНИЯ");
        sb.AppendLine(new string('=', 55));
        sb.AppendLine($"Номер документа: {order.OrderNum}");
        sb.AppendLine($"Дата: {order.OrderDate:yyyy-MM-dd HH:mm:ss}");
        sb.AppendLine($"Всадник: {order.RiderName}");
        sb.AppendLine($"Статус: {order.Status}");
        sb.AppendLine($"Итого: {order.TotalPrice:F2} руб.");
        sb.AppendLine($"Позиций: {order.Items.Count}");
        sb.AppendLine();
        int n = 1;
        foreach (OrderItem item in order.Items)
        {
            sb.AppendLine($"  {n}. {item.HorseName} — {item.SessionText} — {item.DurationMinutes} мин. — тренер: {item.TrainerText} — {item.ItemCost:F2} руб.");
            n++;
        }
        sb.AppendLine();
        sb.AppendLine($"Подпись инструктора по верховой езде: {instructor} /____________/");
        sb.AppendLine($"SHA256: {Calc.DocHash(order)}");
        return sb.ToString();
    }

    private string BuildHtml(Order order, string instructor)
    {
        string Enc(string s) => System.Net.WebUtility.HtmlEncode(s);
        var sb = new StringBuilder();
        sb.AppendLine("<!DOCTYPE html><html lang=\"ru\"><head><meta charset=\"utf-8\">" +
                      "<title>Маршрутный лист тренировки</title></head><body>");
        sb.AppendLine("<h1>Конноспортивный клуб и ипподром «Аллюр»</h1>");
        sb.AppendLine("<h2>Маршрутный лист тренировки и акт проверки снаряжения</h2>");
        sb.AppendLine($"<p>Номер: {Enc(order.OrderNum)}<br>Дата: {order.OrderDate:yyyy-MM-dd HH:mm:ss}<br>" +
                      $"Всадник: {Enc(order.RiderName)}<br>Статус: {Enc(order.Status)}<br>" +
                      $"Итого: {order.TotalPrice:F2} руб.<br>Позиций: {order.Items.Count}</p>");
        sb.AppendLine("<table border=\"1\"><tr><th>№</th><th>Лошадь</th><th>Дата и время</th><th>Минут</th><th>Тренер</th><th>Сумма</th></tr>");
        int n = 1;
        foreach (OrderItem item in order.Items)
        {
            sb.AppendLine($"<tr><td>{n}</td><td>{Enc(item.HorseName)}</td><td>{item.SessionText}</td>" +
                          $"<td>{item.DurationMinutes}</td><td>{item.TrainerText}</td><td>{item.ItemCost:F2}</td></tr>");
            n++;
        }
        sb.AppendLine("</table>");
        sb.AppendLine($"<p>Подпись инструктора: {Enc(instructor)} /____________/</p>");
        sb.AppendLine($"<p>SHA256: {Calc.DocHash(order)}</p>");
        sb.AppendLine("</body></html>");
        return sb.ToString();
    }

    // ---------- Таймер 45 минут ----------

    private void SetupTimer()
    {
        _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        _timer.Tick += (_, _) =>
        {
            if (!_timerRunning) return;
            _timerLeft -= TimeSpan.FromSeconds(1);
            if (_timerLeft <= TimeSpan.Zero)
            {
                _timerLeft = TimeSpan.Zero;
                _timerRunning = false;
                TimerText.Text = "00:00 — смена окончена!";
                try { Console.Beep(880, 600); } catch { /* звуковой сигнал на Windows */ }
                return;
            }
            TimerText.Text = $"{(int)_timerLeft.TotalMinutes:D2}:{_timerLeft.Seconds:D2}";
        };

        BtnTimerStart.Click += (_, _) => { _timerRunning = true; _timer?.Start(); };
        BtnTimerPause.Click += (_, _) => { _timerRunning = false; _timer?.Stop(); };
        BtnTimerReset.Click += (_, _) =>
        {
            _timerRunning = false;
            _timer?.Stop();
            _timerLeft = TimeSpan.FromMinutes(45);
            TimerText.Text = "45:00";
        };
    }
}
