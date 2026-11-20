namespace Allure.Core.Models;

/// <summary>
/// Роль пользователя (таблица roles).
/// 1 — Главный берейтор-зоотехник, 2 — Администратор манежа.
/// </summary>
public class Role
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;

    public ICollection<User> Users { get; set; } = new List<User>();
}
