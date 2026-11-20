using Allure.Core.Models;
using Avalonia.Data.Converters;
using Avalonia.Media;
using System;
using System.Globalization;

namespace Allure.Desktop.Converters;

/// <summary>Конвертер статуса FSM в цвет для индикации критических состояний.</summary>
public sealed class StatusToBrushConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        string? status = value as string;
        return status switch
        {
            OrderStatuses.Booked => new SolidColorBrush(Color.Parse("#EF6C00")),
            OrderStatuses.HorseSaddled => new SolidColorBrush(Color.Parse("#1565C0")),
            OrderStatuses.Training => new SolidColorBrush(Color.Parse("#2E7D32")),
            OrderStatuses.Completed => new SolidColorBrush(Color.Parse("#616161")),
            OrderStatuses.CancelledVet => new SolidColorBrush(Color.Parse("#C62828")),
            _ => new SolidColorBrush(Color.Parse("#3E2723"))
        };
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
