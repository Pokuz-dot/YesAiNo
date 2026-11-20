using Allure.Core.Data;
using Allure.Core.Models;

namespace Allure.Core.Services;

/// <summary>
/// Демонстрационные данные (5–10 записей в каждой таблице).
/// Используется, если БД создана через EF (EnsureCreated), а не через
/// DDL-скрипт Database/Yes.sql. Суммы считаются через OrderCalculationService.
/// Логины: bereitor / bereitor123, admin / admin123, manager / manager123.
/// </summary>
public static class DatabaseSeeder
{
    public static void SeedIfEmpty(AppDbContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        if (context.Roles.Any())
            return;

        var auth = new AuthService();
        var calc = new OrderCalculationService();

        var roles = new List<Role>
        {
            new() { Id = 1, Name = "Главный берейтор-зоотехник" },
            new() { Id = 2, Name = "Администратор манежа" }
        };
        context.Roles.AddRange(roles);

        context.Users.AddRange(
            new User { Id = 1, RoleId = 1, Login = "bereitor", PasswordHash = auth.HashPassword("bereitor123"), FullName = "Смирнов Алексей Петрович" },
            new User { Id = 2, RoleId = 2, Login = "admin", PasswordHash = auth.HashPassword("admin123"), FullName = "Кузнецова Мария Ивановна" },
            new User { Id = 3, RoleId = 2, Login = "manager", PasswordHash = auth.HashPassword("manager123"), FullName = "Волков Дмитрий Сергеевич" });

        context.HorseBreeds.AddRange(
            new HorseBreed { Id = 1, BreedName = "Орловский рысак", Temperament = "Спокойный", OriginCountry = "Россия" },
            new HorseBreed { Id = 2, BreedName = "Арабская чистокровная", Temperament = "Горячая", OriginCountry = "ОАЭ" },
            new HorseBreed { Id = 3, BreedName = "Владимирский тяжеловоз", Temperament = "Добродушный", OriginCountry = "Россия" },
            new HorseBreed { Id = 4, BreedName = "Тракененская", Temperament = "Энергичная", OriginCountry = "Россия" });

        context.Horses.AddRange(
            new Horse { Id = 1, BreedId = 1, Nickname = "Гром", PassportChipCode = "985112345678901", BirthYear = 2019, SuitColor = "Вороной", IsReadyForTraining = true },
            new Horse { Id = 2, BreedId = 2, Nickname = "Заря", PassportChipCode = "985112345678902", BirthYear = 2017, SuitColor = "Гнедая", IsReadyForTraining = true },
            new Horse { Id = 3, BreedId = 3, Nickname = "Ветер", PassportChipCode = "985112345678903", BirthYear = 2020, SuitColor = "Рыжий", IsReadyForTraining = true },
            new Horse { Id = 4, BreedId = 4, Nickname = "Молния", PassportChipCode = "985112345678904", BirthYear = 2018, SuitColor = "Тёмно-гнедая", IsReadyForTraining = false },
            new Horse { Id = 5, BreedId = 1, Nickname = "Снежок", PassportChipCode = "985112345678905", BirthYear = 2021, SuitColor = "Серый", IsReadyForTraining = true },
            new Horse { Id = 6, BreedId = 2, Nickname = "Песня", PassportChipCode = "985112345678906", BirthYear = 2016, SuitColor = "Рыжая", IsReadyForTraining = true });

        var riders = new List<ClubRider>
        {
            new() { Id = 1, FullName = "Анна Соколова", Phone = "+7(921)555-01-01", SkillLevel = "Опытный", DiscountRate = 10m },
            new() { Id = 2, FullName = "Игорь Лебедев", Phone = "+7(921)555-01-02", SkillLevel = "Начинающий", DiscountRate = 0m },
            new() { Id = 3, FullName = "Ольга Морозова", Phone = "+7(921)555-01-03", SkillLevel = "Опытный", DiscountRate = 15m },
            new() { Id = 4, FullName = "Павел Орлов", Phone = "+7(921)555-01-04", SkillLevel = "Любитель", DiscountRate = 5m },
            new() { Id = 5, FullName = "Елена Быстрова", Phone = "+7(921)555-01-05", SkillLevel = "Опытный", DiscountRate = 10m }
        };
        context.ClubRiders.AddRange(riders);

        RidingOrder MakeOrder(int id, string num, int userId, int riderId, DateTime date, string status, string? cancelReason = null)
        {
            var order = new RidingOrder
            {
                Id = id, OrderNum = num, UserId = userId, ClubRiderId = riderId,
                OrderDate = date, Status = status, CancelReason = cancelReason
            };
            order.TotalPrice = 0m;
            return order;
        }

        var orders = new List<RidingOrder>
        {
            MakeOrder(1, "ПР23-КСК-2026-00001", 2, 1, new DateTime(2026, 11, 21, 10, 0, 0), OrderStatuses.Booked),
            MakeOrder(2, "ПР23-КСК-2026-00002", 2, 2, new DateTime(2026, 11, 21, 12, 0, 0), OrderStatuses.HorseSaddled),
            MakeOrder(3, "ПР23-КСК-2026-00003", 1, 3, new DateTime(2026, 11, 20, 9, 0, 0), OrderStatuses.Training),
            MakeOrder(4, "ПР23-КСК-2026-00004", 2, 4, new DateTime(2026, 11, 19, 11, 0, 0), OrderStatuses.Completed),
            MakeOrder(5, "ПР23-КСК-2026-00005", 1, 5, new DateTime(2026, 11, 18, 15, 0, 0), OrderStatuses.CancelledVet,
                "Лошадь заболела, требуется ветеринарный осмотр"),
            MakeOrder(6, "ПР23-КСК-2026-00006", 2, 1, new DateTime(2026, 11, 22, 10, 0, 0), OrderStatuses.Booked)
        };

        void AddItem(RidingOrder order, int horseId, DateTime when, int minutes, bool trainer)
        {
            decimal discount = riders.First(r => r.Id == order.ClubRiderId).DiscountRate;
            order.Items.Add(new RidingSessionItem
            {
                OrderId = order.Id, HorseId = horseId, SessionDateTime = when,
                DurationMinutes = minutes, HasTrainer = trainer,
                ItemCost = calc.CalculateItemCost(minutes, trainer, discount)
            });
        }

        AddItem(orders[0], 1, new DateTime(2026, 11, 21, 10, 0, 0), 45, true);
        AddItem(orders[0], 2, new DateTime(2026, 11, 21, 11, 0, 0), 90, false);
        AddItem(orders[1], 3, new DateTime(2026, 11, 21, 12, 0, 0), 45, false);
        AddItem(orders[2], 1, new DateTime(2026, 11, 20, 9, 0, 0), 45, true);
        AddItem(orders[3], 5, new DateTime(2026, 11, 19, 11, 0, 0), 90, true);
        AddItem(orders[4], 4, new DateTime(2026, 11, 18, 15, 0, 0), 45, false);
        AddItem(orders[5], 6, new DateTime(2026, 11, 22, 10, 0, 0), 45, true);
        AddItem(orders[5], 1, new DateTime(2026, 11, 22, 11, 0, 0), 45, false);

        foreach (RidingOrder order in orders)
        {
            decimal discount = riders.First(r => r.Id == order.ClubRiderId).DiscountRate;
            order.TotalPrice = calc.CalculateOrderTotal(order.Items, discount);
        }

        context.RidingOrders.AddRange(orders);
        context.SaveChanges();
    }
}
