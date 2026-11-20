using Allure.Core.Models;
using Allure.Desktop.ViewModels;
using Avalonia.Controls;
using System;
using System.Linq;

namespace Allure.Desktop.Views;

public partial class OrderWindow : Window
{
    private readonly OrderViewModel _viewModel;

    public OrderWindow(OrderViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
        DataContext = _viewModel;

        DatePicker.SelectedDate = DateTime.Today;
        TimePicker.Clock = new TimeSpan(10, 0, 0);

        _viewModel.RequestCancelReason = AskCancelReason;
        _viewModel.Saved += () => Close();
    }

    private async System.Threading.Tasks.Task<string?> AskCancelReason()
    {
        var dialog = new CancelReasonWindow();
        bool confirmed = await dialog.ShowDialog<bool>(this);
        return confirmed ? dialog.Reason : null;
    }

    private void OnAddItemClicked(object? sender, RoutedEventArgs e)
    {
        if (!_viewModel.IsEditable)
            return;

        int horseId = HorsePicker.SelectedValue is int id ? id : 0;
        if (horseId == 0)
        {
            _viewModel.ErrorMessage = "Выберите лошадь для позиции.";
            return;
        }

        DateTime date = DatePicker.SelectedDate ?? DateTime.Today;
        TimeSpan time = TimePicker.Clock ?? new TimeSpan(10, 0, 0);
        if (!int.TryParse(DurationBox.Text, out int minutes) || minutes <= 0)
        {
            _viewModel.ErrorMessage = "Длительность должна быть положительным числом минут.";
            return;
        }

        _viewModel.Items.Add(new SessionItemViewModel
        {
            HorseId = horseId,
            HorseName = _viewModel.Horses.FirstOrDefault(h => h.Id == horseId)?.Nickname ?? string.Empty,
            SessionDateTime = date.Date + time,
            DurationMinutes = minutes,
            HasTrainer = TrainerBox.IsChecked == true
        });
        _viewModel.Recalculate();
    }
}
