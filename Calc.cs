using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace Avalo;

/// <summary>Расчёт, FSM, номер документа, SHA256 — вся бизнес-логика в одном месте.</summary>
public static class Calc
{
    // Формула: (минут / 45) * 1500 руб.; тренер +1000 руб.; скидка всадника в процентах.
    public static decimal ItemCost(int minutes, bool trainer, decimal discountRate)
    {
        if (minutes <= 0) throw new ArgumentOutOfRangeException(nameof(minutes));
        decimal sum = (minutes / 45m) * 1500m + (trainer ? 1000m : 0m);
        return Math.Round(sum * (1 - discountRate / 100m), 2);
    }

    public static decimal OrderTotal(IEnumerable<OrderItem> items, decimal discountRate)
    {
        decimal total = 0m;
        foreach (OrderItem i in items)
            total += ItemCost(i.DurationMinutes, i.HasTrainer, discountRate);
        return Math.Round(total, 2);
    }

    // FSM: разрешённые переходы.
    private static readonly Dictionary<string, string[]> Next = new()
    {
        [Statuses.Booked] = new[] { Statuses.Saddled, Statuses.Cancelled },
        [Statuses.Saddled] = new[] { Statuses.Training, Statuses.Cancelled },
        [Statuses.Training] = new[] { Statuses.Completed, Statuses.Cancelled },
        [Statuses.Completed] = Array.Empty<string>(),
        [Statuses.Cancelled] = Array.Empty<string>()
    };

    public static bool CanGo(string from, string to) =>
        Next.TryGetValue(from, out string[]? list) && Array.IndexOf(list, to) >= 0;

    public static bool IsEditable(string status) => status == Statuses.Booked;

    public static string DocNum(int year, int index) => $"ПР23-КСК-{year}-{index:D5}";

    public static string Sha256(string text)
    {
        byte[] hash = SHA256.HashData(Encoding.UTF8.GetBytes(text));
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    public static string PasswordHash(string password) => Sha256(password);

    // Подпись для экспорта: [Номер]+[Дата]+[Контрагент]+[Сумма]+[КоличествоПозиций]
    public static string DocHash(Order order) => Sha256(
        $"{order.OrderNum}|{order.OrderDate:yyyy-MM-dd HH:mm:ss}|{order.RiderName}|{order.TotalPrice:F2}|{order.Items.Count}");
}
