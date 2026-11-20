namespace Allure.Core.Models;

/// <summary>
/// Учётная запись сотрудника (таблица users).
/// Пароль хранится только в виде SHA256-хеша (password_hash).
/// </summary>
public class User
{
    public int Id { get; set; }
    public int RoleId { get; set; }
    public string Login { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;

    public Role? Role { get; set; }
    public ICollection<RidingOrder> Orders { get; set; } = new List<RidingOrder>();
}
