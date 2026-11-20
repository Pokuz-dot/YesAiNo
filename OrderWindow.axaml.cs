using Avalonia.Controls;
using Avalonia.Interactivity;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Avalo;

/// <summary>Окно заказа: заголовок + позиции, итог пересчитывается на лету.</summary>
public partial class OrderWindow : Window
{
    private readonly Order _order;
    private readonly List<Horse> _horses;
    private readonly List<Rider> _riders;

    public OrderWindow(Order? existing, int userId, List<Horse> horses, List<Rider> riders)
    {
        InitializeComponent();
        _horses = horses;
        _riders = riders;

        if (existing is not null)
        {
            _order = existing;
        }
        else
        {
            int year = DateTime.Now.Year;
            _order = new Order
            {
                OrderNum = Calc.DocNum(year, Db.NextOrderIndex(year)),
                UserId = userId,
                OrderDate = DateTime.Now,
                Status = Statuses.Booked
            };
        }

        CmbRider.ItemsSource = _riders;
        CmbHorse.ItemsSource = _horses;
        if (_riders.Count > 0) CmbRider.SelectedIndex = 0;
        if (_horses.Count > 0) CmbHorse.SelectedIndex = 0;
        DatePicker.SelectedDate = DateTime.Today;

        RefreshHeader();
        RefreshItems();

        BtnAddItem.Click += (_, _) => AddItem();
        BtnRemoveItem.Click += (_, _) =>
        {
            if (ItemsGrid.SelectedItem is OrderItem item)
            {
                _order.Items.Remove(item);
                RefreshItems();
            }
        };
        BtnSave.Click += (_, _) => Save();
    }

    private bool Editable => Calc.IsEditable(_order.Status);

    private void RefreshHeader()
    {
        TxtNum.Text = _order.OrderNum;
        TxtDate.Text = _order.OrderDate.ToString("yyyy-MM-dd HH:mm");
        TxtStatus.Text = _order.Status;
        var rider = _riders.FirstOrDefault(r => r.Id == _order.ClubRiderId);
        if (rider is not null) CmbRider.SelectedItem = rider;
        Recalc();
    }

    private void RefreshItems()
    {
        ItemsGrid.ItemsSource = null;
        ItemsGrid.ItemsSource = _order.Items;
        Recalc();
    }

    private decimal Discount()
    {
        if (CmbRider.SelectedItem is Rider rider) return rider.DiscountRate;
        return _riders.FirstOrDefault(r => r.Id == _order.ClubRiderId)?.DiscountRate ?? 0m;
    }

    private void Recalc()
    {
        decimal discount = Discount();
        foreach (OrderItem item in _order.Items)
            item.ItemCost = Calc.ItemCost(item.DurationMinutes, item.HasTrainer, discount);
        _order.TotalPrice = Calc.OrderTotal(_order.Items, discount);
        TxtTotal.Text = $"{_order.TotalPrice:F2} руб.";
        ItemsGrid.ItemsSource = null;
        ItemsGrid.ItemsSource = _order.Items;
    }

    private void AddItem()
    {
        if (!Editable)
        {
            ErrorText.Text = "Заказ закрыт для редактирования.";
            return;
        }
        if (CmbHorse.SelectedItem is not Horse horse)
        {
            ErrorText.Text = "Выберите лошадь.";
            return;
        }
        if (!int.TryParse(TxtMinutes.Text, out int minutes) || minutes <= 0)
        {
            ErrorText.Text = "Длительность — положительное число минут.";
            return;
        }

        DateTime date = DatePicker.SelectedDate?.Date ?? DateTime.Today;
        if (!TimeSpan.TryParse(TxtTime.Text, out TimeSpan time))
        {
            ErrorText.Text = "Время укажите в формате ЧЧ:ММ (например, 10:00).";
            return;
        }
        _order.Items.Add(new OrderItem
        {
            HorseId = horse.Id,
            HorseName = horse.Nickname,
            SessionDateTime = date + time,
            DurationMinutes = minutes,
            HasTrainer = ChkTrainer.IsChecked == true
        });
        ErrorText.Text = "";
        RefreshItems();
    }

    private void Save()
    {
        ErrorText.Text = "";
        if (!Editable)
        {
            ErrorText.Text = "Заказ закрыт для редактирования.";
            return;
        }
        if (CmbRider.SelectedItem is not Rider rider)
        {
            ErrorText.Text = "Выберите всадника.";
            return;
        }

        _order.ClubRiderId = rider.Id;
        _order.RiderName = rider.FullName;

        try
        {
            foreach (OrderItem item in _order.Items)
            {
                Horse horse = _horses.First(h => h.Id == item.HorseId);
                if (!horse.IsReadyForTraining)
                    throw new InvalidOperationException($"Лошадь «{horse.Nickname}» не допущена к тренировкам.");
                int busy = Db.HorseMinutesOnDay(item.HorseId, item.SessionDateTime, _order.Id);
                if (busy + item.DurationMinutes > 180)
                    throw new InvalidOperationException($"Лошадь «{horse.Nickname}» превысит лимит 3 часов в сутки.");
            }

            Recalc();
            Db.SaveOrder(_order);
            Close();
        }
        catch (InvalidOperationException ex)
        {
            ErrorText.Text = ex.Message;
        }
        catch (Exception ex)
        {
            ErrorText.Text = "Ошибка сохранения: " + ex.Message;
        }
    }

    // ---------- Смена статуса по FSM ----------

    private async void ChangeStatus(string newStatus)
    {
        ErrorText.Text = "";
        if (_order.Id == 0)
        {
            ErrorText.Text = "Сначала сохраните заказ.";
            return;
        }
        if (!Calc.CanGo(_order.Status, newStatus))
        {
            ErrorText.Text = $"Переход «{_order.Status}» → «{newStatus}» запрещён.";
            return;
        }
        if (newStatus == Statuses.Saddled && _order.Items.Count == 0)
        {
            ErrorText.Text = "Нельзя перевести пустой заказ в «Лошадь оседлана».";
            return;
        }

        string? reason = null;
        if (newStatus == Statuses.Cancelled)
        {
            var dialog = new CancelReasonWindow();
            bool ok = await dialog.ShowDialog<bool>(this);
            if (!ok) return;
            reason = dialog.Reason;
        }

        try
        {
            Db.UpdateStatus(_order.Id, newStatus, reason);
            _order.Status = newStatus;
            _order.CancelReason = reason;
            RefreshHeader();
        }
        catch (Exception ex)
        {
            ErrorText.Text = "Ошибка смены статуса: " + ex.Message;
        }
    }

    private void OnStatusSaddled(object? sender, RoutedEventArgs e) => ChangeStatus(Statuses.Saddled);
    private void OnStatusTraining(object? sender, RoutedEventArgs e) => ChangeStatus(Statuses.Training);
    private void OnStatusCompleted(object? sender, RoutedEventArgs e) => ChangeStatus(Statuses.Completed);
    private void OnStatusCancelled(object? sender, RoutedEventArgs e) => ChangeStatus(Statuses.Cancelled);
}
