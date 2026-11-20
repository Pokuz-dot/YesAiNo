using System;
using System.Collections.Generic;

namespace Avalo;

/// <summary>Простые классы под таблицы БД (для DataGrid).</summary>
public class Horse
{
    public int Id { get; set; }
    public string Nickname { get; set; } = "";
    public string BreedName { get; set; } = "";
    public int BirthYear { get; set; }
    public string SuitColor { get; set; } = "";
    public bool IsReadyForTraining { get; set; } = true;
    public string ReadyText => IsReadyForTraining ? "Допущена" : "Не допущена";
}

public class Rider
{
    public int Id { get; set; }
    public string FullName { get; set; } = "";
    public string Phone { get; set; } = "";
    public string SkillLevel { get; set; } = "";
    public decimal DiscountRate { get; set; }
    public string DiscountText => $"{DiscountRate:F2}%";
}

public class Order
{
    public int Id { get; set; }
    public string OrderNum { get; set; } = "";
    public int UserId { get; set; }
    public int ClubRiderId { get; set; }
    public DateTime OrderDate { get; set; } = DateTime.Now;
    public decimal TotalPrice { get; set; }
    public string Status { get; set; } = Statuses.Booked;
    public string? CancelReason { get; set; }

    public string RiderName { get; set; } = "";
    public List<OrderItem> Items { get; set; } = new();

    public string OrderDateText => OrderDate.ToString("yyyy-MM-dd HH:mm");
    public string TotalPriceText => TotalPrice.ToString("F2");
}

public class OrderItem
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public int HorseId { get; set; }
    public string HorseName { get; set; } = "";
    public DateTime SessionDateTime { get; set; }
    public int DurationMinutes { get; set; } = 45;
    public bool HasTrainer { get; set; }
    public decimal ItemCost { get; set; }

    public string SessionText => SessionDateTime.ToString("yyyy-MM-dd HH:mm");
    public string CostText => ItemCost.ToString("F2");
    public string TrainerText => HasTrainer ? "Да" : "Нет";
}

public class DbUser
{
    public int Id { get; set; }
    public string Login { get; set; } = "";
    public string PasswordHash { get; set; } = "";
    public string FullName { get; set; } = "";
    public string RoleName { get; set; } = "";
}

/// <summary>Статусы FSM заказа.</summary>
public static class Statuses
{
    public const string Booked = "Забронировано";
    public const string Saddled = "Лошадь оседлана";
    public const string Training = "Тренировка в манеже";
    public const string Completed = "Завершена успешно";
    public const string Cancelled = "Отмена по ветпоказаниям";

    public static readonly string[] All = { Booked, Saddled, Training, Completed, Cancelled };
}
