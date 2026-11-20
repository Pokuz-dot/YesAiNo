using System;
using System.Collections.Generic;
using MySqlConnector;

namespace Avalo;

/// <summary>Простой доступ к MySQL через ADO.NET (MySqlConnector).</summary>
public static class Db
{
    // Строка подключения — поменяйте пароль, если нужен.
    private const string Conn =
        "server=localhost;database=Yes;uid=root;pwd=;charset=utf8mb4";

    private static MySqlConnection Open()
    {
        var conn = new MySqlConnection(Conn);
        conn.Open();
        return conn;
    }

    public static DbUser? FindUser(string login)
    {
        using var conn = Open();
        using var cmd = new MySqlCommand(
            "SELECT u.id, u.login, u.password_hash, u.full_name, r.name " +
            "FROM users u JOIN roles r ON r.id = u.role_id WHERE u.login = @login", conn);
        cmd.Parameters.AddWithValue("@login", login);
        using var reader = cmd.ExecuteReader();
        if (!reader.Read()) return null;
        return new DbUser
        {
            Id = reader.GetInt32(0),
            Login = reader.GetString(1),
            PasswordHash = reader.GetString(2),
            FullName = reader.GetString(3),
            RoleName = reader.GetString(4)
        };
    }

    public static List<Horse> LoadHorses()
    {
        var list = new List<Horse>();
        using var conn = Open();
        using var cmd = new MySqlCommand(
            "SELECT h.id, h.nickname, b.breed_name, h.birth_year, h.suit_color, h.is_ready_for_training " +
            "FROM horses h JOIN horse_breeds b ON b.id = h.breed_id ORDER BY h.nickname", conn);
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            list.Add(new Horse
            {
                Id = reader.GetInt32(0),
                Nickname = reader.GetString(1),
                BreedName = reader.GetString(2),
                BirthYear = reader.GetInt32(3),
                SuitColor = reader.IsDBNull(4) ? "" : reader.GetString(4),
                IsReadyForTraining = reader.GetBoolean(5)
            });
        }
        return list;
    }

    public static List<Rider> LoadRiders()
    {
        var list = new List<Rider>();
        using var conn = Open();
        using var cmd = new MySqlCommand(
            "SELECT id, full_name, phone, skill_level, discount_rate FROM club_riders ORDER BY full_name", conn);
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            list.Add(new Rider
            {
                Id = reader.GetInt32(0),
                FullName = reader.GetString(1),
                Phone = reader.GetString(2),
                SkillLevel = reader.IsDBNull(3) ? "" : reader.GetString(3),
                DiscountRate = reader.GetDecimal(4)
            });
        }
        return list;
    }

    public static List<Order> LoadOrders()
    {
        var orders = new List<Order>();
        using var conn = Open();

        using (var cmd = new MySqlCommand(
            "SELECT o.id, o.order_num, o.user_id, o.club_rider_id, o.order_date, o.total_price, o.status, o.cancel_reason, r.full_name " +
            "FROM riding_orders o JOIN club_riders r ON r.id = o.club_rider_id ORDER BY o.order_date DESC", conn))
        using (var reader = cmd.ExecuteReader())
        {
            while (reader.Read())
            {
                orders.Add(new Order
                {
                    Id = reader.GetInt32(0),
                    OrderNum = reader.GetString(1),
                    UserId = reader.GetInt32(2),
                    ClubRiderId = reader.GetInt32(3),
                    OrderDate = reader.GetDateTime(4),
                    TotalPrice = reader.GetDecimal(5),
                    Status = reader.GetString(6),
                    CancelReason = reader.IsDBNull(7) ? null : reader.GetString(7),
                    RiderName = reader.GetString(8)
                });
            }
        }

        foreach (Order order in orders)
        {
            using var cmd = new MySqlCommand(
                "SELECT i.id, i.horse_id, h.nickname, i.session_datetime, i.duration_minutes, i.has_trainer, i.item_cost " +
                "FROM riding_session_items i JOIN horses h ON h.id = i.horse_id " +
                "WHERE i.order_id = @id ORDER BY i.session_datetime", conn);
            cmd.Parameters.AddWithValue("@id", order.Id);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                order.Items.Add(new OrderItem
                {
                    Id = reader.GetInt32(0),
                    HorseId = reader.GetInt32(1),
                    HorseName = reader.GetString(2),
                    SessionDateTime = reader.GetDateTime(3),
                    DurationMinutes = reader.GetInt32(4),
                    HasTrainer = reader.GetBoolean(5),
                    ItemCost = reader.GetDecimal(6)
                });
            }
        }
        return orders;
    }

    public static int NextOrderIndex(int year)
    {
        using var conn = Open();
        using var cmd = new MySqlCommand("SELECT order_num FROM riding_orders", conn);
        using var reader = cmd.ExecuteReader();
        int max = 0;
        while (reader.Read())
        {
            string[] parts = reader.GetString(0).Split('-');
            if (parts.Length == 4 && int.TryParse(parts[2], out int y) && y == year
                && int.TryParse(parts[3], out int idx) && idx > max)
                max = idx;
        }
        return max + 1;
    }

    /// <summary>Сохраняет заказ с позициями (транзакция). Возвращает id заказа.</summary>
    public static int SaveOrder(Order order)
    {
        using var conn = Open();
        using var tx = conn.BeginTransaction();
        int id = order.Id;

        if (id == 0)
        {
            using var cmd = new MySqlCommand(
                "INSERT INTO riding_orders (order_num, user_id, club_rider_id, order_date, total_price, status, cancel_reason) " +
                "VALUES (@num, @user, @rider, @date, @total, @status, @reason)", conn, tx);
            cmd.Parameters.AddWithValue("@num", order.OrderNum);
            cmd.Parameters.AddWithValue("@user", order.UserId);
            cmd.Parameters.AddWithValue("@rider", order.ClubRiderId);
            cmd.Parameters.AddWithValue("@date", order.OrderDate);
            cmd.Parameters.AddWithValue("@total", order.TotalPrice);
            cmd.Parameters.AddWithValue("@status", order.Status);
            cmd.Parameters.AddWithValue("@reason", (object?)order.CancelReason ?? DBNull.Value);
            cmd.ExecuteNonQuery();
            id = (int)cmd.LastInsertedId;
            order.Id = id;
        }
        else
        {
            using (var cmd = new MySqlCommand(
                "UPDATE riding_orders SET club_rider_id = @rider, order_date = @date, total_price = @total, status = @status, cancel_reason = @reason WHERE id = @id",
                conn, tx))
            {
                cmd.Parameters.AddWithValue("@rider", order.ClubRiderId);
                cmd.Parameters.AddWithValue("@date", order.OrderDate);
                cmd.Parameters.AddWithValue("@total", order.TotalPrice);
                cmd.Parameters.AddWithValue("@status", order.Status);
                cmd.Parameters.AddWithValue("@reason", (object?)order.CancelReason ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@id", id);
                cmd.ExecuteNonQuery();
            }
            using (var cmd = new MySqlCommand("DELETE FROM riding_session_items WHERE order_id = @id", conn, tx))
            {
                cmd.Parameters.AddWithValue("@id", id);
                cmd.ExecuteNonQuery();
            }
        }

        foreach (OrderItem item in order.Items)
        {
            using var cmd = new MySqlCommand(
                "INSERT INTO riding_session_items (order_id, horse_id, session_datetime, duration_minutes, has_trainer, item_cost) " +
                "VALUES (@order, @horse, @when, @minutes, @trainer, @cost)", conn, tx);
            cmd.Parameters.AddWithValue("@order", id);
            cmd.Parameters.AddWithValue("@horse", item.HorseId);
            cmd.Parameters.AddWithValue("@when", item.SessionDateTime);
            cmd.Parameters.AddWithValue("@minutes", item.DurationMinutes);
            cmd.Parameters.AddWithValue("@trainer", item.HasTrainer);
            cmd.Parameters.AddWithValue("@cost", item.ItemCost);
            cmd.ExecuteNonQuery();
        }

        tx.Commit();
        return id;
    }

    public static void UpdateStatus(int orderId, string status, string? reason)
    {
        using var conn = Open();
        using var cmd = new MySqlCommand(
            "UPDATE riding_orders SET status = @status, cancel_reason = @reason WHERE id = @id", conn);
        cmd.Parameters.AddWithValue("@status", status);
        cmd.Parameters.AddWithValue("@reason", (object?)reason ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@id", orderId);
        cmd.ExecuteNonQuery();
    }

    public static void DeleteOrder(int orderId)
    {
        using var conn = Open();
        using var cmd = new MySqlCommand("DELETE FROM riding_orders WHERE id = @id", conn);
        cmd.Parameters.AddWithValue("@id", orderId);
        cmd.ExecuteNonQuery();
    }

    /// <summary>Сколько минут лошадь уже занята в этот день (контроль 3 часов).</summary>
    public static int HorseMinutesOnDay(int horseId, DateTime day, int excludeOrderId)
    {
        using var conn = Open();
        using var cmd = new MySqlCommand(
            "SELECT COALESCE(SUM(duration_minutes), 0) FROM riding_session_items " +
            "WHERE horse_id = @horse AND DATE(session_datetime) = DATE(@day) AND order_id <> @order", conn);
        cmd.Parameters.AddWithValue("@horse", horseId);
        cmd.Parameters.AddWithValue("@day", day);
        cmd.Parameters.AddWithValue("@order", excludeOrderId);
        return Convert.ToInt32(cmd.ExecuteScalar());
    }
}
