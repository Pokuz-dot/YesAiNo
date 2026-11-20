using Allure.Core.Models;
using Allure.Core.Services;
using Allure.Desktop.Services;
using Allure.Desktop.ViewModels;
using Avalonia.Controls;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace Allure.Desktop.Views;

public partial class MainWindow : Window
{
    private readonly MainViewModel _viewModel;

    public MainWindow(MainViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
        DataContext = _viewModel;

        _viewModel.LogoutRequested += OnLogout;
        _viewModel.OpenOrderRequested += OpenOrder;
        _viewModel.NewOrderRequested += NewOrder;
        _viewModel.ExportRequested += ExportOrder;
        _viewModel.DeleteRequested += DeleteOrder;

        OrdersGrid.Sorting += OnOrdersSorting;
        OrdersGrid.DoubleTapped += (_, _) => _viewModel.OpenOrderCommand.Execute(null);

        _viewModel.Refresh();
    }

    private void OnOrdersSorting(object? sender, DataGridColumnEventArgs e)
    {
        string? column = e.Column.Header?.ToString() switch
        {
            "Номер" => nameof(OrderRowViewModel.OrderNum),
            "Дата" => nameof(OrderRowViewModel.OrderDate),
            "Всадник" => nameof(OrderRowViewModel.RiderName),
            "Сумма, руб." => nameof(OrderRowViewModel.TotalPrice),
            "Статус" => nameof(OrderRowViewModel.Status),
            _ => null
        };
        if (column is not null)
            _viewModel.SortBy(column);
        e.Handled = true;
    }

    private void OnLogout()
    {
        var loginWindow = new LoginWindow(App.Services.GetRequiredService<LoginViewModel>());
        loginWindow.Show();
        Close();
    }

    private async void OpenOrder(int orderId)
    {
        var orderViewModel = App.Services.GetRequiredService<OrderViewModel>();
        orderViewModel.LoadForEdit(orderId);
        var window = new OrderWindow(orderViewModel);
        await window.ShowDialog(this);
        _viewModel.Refresh();
    }

    private async void NewOrder()
    {
        var orderViewModel = App.Services.GetRequiredService<OrderViewModel>();
        orderViewModel.LoadForNew();
        var window = new OrderWindow(orderViewModel);
        await window.ShowDialog(this);
        _viewModel.Refresh();
    }

    private async void ExportOrder(int orderId)
    {
        var factory = App.Services.GetRequiredService<DbContextFactory>();
        var exportService = App.Services.GetRequiredService<IExportService>();
        var session = App.Services.GetRequiredService<SessionService>();

        using var context = factory.CreateDbContext();
        RidingOrder? order = context.RidingOrders
            .Include(o => o.Items).ThenInclude(i => i.Horse)
            .Include(o => o.ClubRider)
            .FirstOrDefault(o => o.Id == orderId);
        if (order is null)
            return;

        var dialog = new SaveFileDialog
        {
            Title = "Экспорт маршрутного листа",
            SuggestedFileName = $"{order.OrderNum}.txt",
            FileTypeChoices = new[]
            {
                new FileDialogFilter { Name = "Текстовый документ", Extensions = new[] { "txt" } },
                new FileDialogFilter { Name = "HTML документ", Extensions = new[] { "html" } }
            }
        };

        string? path = await dialog.Show(this);
        if (string.IsNullOrWhiteSpace(path))
            return;

        string instructor = session.CurrentUser?.FullName ?? "Инструктор";
        exportService.ExportToFile(path, order,
            order.ClubRider?.FullName ?? string.Empty, instructor);
    }

    private async Task<bool> DeleteOrder(int orderId)
    {
        var factory = App.Services.GetRequiredService<DbContextFactory>();
        using var context = factory.CreateDbContext();
        RidingOrder? order = context.RidingOrders.FirstOrDefault(o => o.Id == orderId);
        if (order is null)
            return false;

        var confirm = new ConfirmWindow($"Удалить заказ {order.OrderNum}?");
        bool confirmed = await confirm.ShowDialog<bool>(this);
        if (!confirmed)
            return false;

        context.RidingOrders.Remove(order);
        context.SaveChanges();
        return true;
    }
}
