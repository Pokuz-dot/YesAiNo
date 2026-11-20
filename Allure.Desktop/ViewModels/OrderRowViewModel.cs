using Allure.Core.Models;
using Avalonia.Media;
using System;

namespace Allure.Desktop.ViewModels;

/// <summary>Строка списка заказов (DataGrid) с цветовой индикацией статуса.</summary>
public sealed class OrderRowViewModel
{
    public int OrderId { get; }
    public string OrderNum { get; }
    public DateTime OrderDate { get; }
    public string OrderDateText => OrderDate.ToString("yyyy-MM-dd HH:mm");
    public string RiderName { get; }
    public decimal TotalPrice { get; }
    public string TotalPriceText => TotalPrice.ToString("F2");
    public string Status { get; }
    public IBrush StatusBrush { get; }

    public OrderRowViewModel(RidingOrder order, string riderName)
    {
        ArgumentNullException.ThrowIfNull(order);
        OrderId = order.Id;
        OrderNum = order.OrderNum;
        OrderDate = order.OrderDate;
        RiderName = riderName;
        TotalPrice = order.TotalPrice;
        Status = order.Status;
        StatusBrush = Status switch
        {
            OrderStatuses.Booked => new SolidColorBrush(Color.Parse("#FFF3E0")),
            OrderStatuses.HorseSaddled => new SolidColorBrush(Color.Parse("#E3F2FD")),
            OrderStatuses.Training => new SolidColorBrush(Color.Parse("#E8F5E9")),
            OrderStatuses.Completed => new SolidColorBrush(Color.Parse("#F5F5F5")),
            OrderStatuses.CancelledVet => new SolidColorBrush(Color.Parse("#FFEBEE")),
            _ => Brushes.White
        };
    }
}
