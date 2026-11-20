using Allure.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace Allure.Core.Data;

/// <summary>
/// Контекст данных EF Core (Pomelo MySQL). Имена таблиц и столбцов
/// соответствуют DDL-скрипту Database/Yes.sql (phpMyAdmin).
/// </summary>
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Role> Roles => Set<Role>();
    public DbSet<User> Users => Set<User>();
    public DbSet<HorseBreed> HorseBreeds => Set<HorseBreed>();
    public DbSet<Horse> Horses => Set<Horse>();
    public DbSet<ClubRider> ClubRiders => Set<ClubRider>();
    public DbSet<RidingOrder> RidingOrders => Set<RidingOrder>();
    public DbSet<RidingSessionItem> RidingSessionItems => Set<RidingSessionItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Role>(e =>
        {
            e.ToTable("roles");
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).HasMaxLength(100).IsRequired();
        });

        modelBuilder.Entity<User>(e =>
        {
            e.ToTable("users");
            e.HasKey(x => x.Id);
            e.Property(x => x.Login).HasMaxLength(50).IsRequired();
            e.HasIndex(x => x.Login).IsUnique();
            e.Property(x => x.PasswordHash).HasMaxLength(255).IsRequired();
            e.Property(x => x.FullName).HasMaxLength(150).IsRequired();
            e.HasOne(x => x.Role).WithMany(r => r.Users)
                .HasForeignKey(x => x.RoleId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<HorseBreed>(e =>
        {
            e.ToTable("horse_breeds");
            e.HasKey(x => x.Id);
            e.Property(x => x.BreedName).HasMaxLength(100).IsRequired();
            e.Property(x => x.Temperament).HasMaxLength(100);
            e.Property(x => x.OriginCountry).HasMaxLength(100);
        });

        modelBuilder.Entity<Horse>(e =>
        {
            e.ToTable("horses");
            e.HasKey(x => x.Id);
            e.Property(x => x.Nickname).HasMaxLength(100).IsRequired();
            e.Property(x => x.PassportChipCode).HasMaxLength(50).IsRequired();
            e.HasIndex(x => x.PassportChipCode).IsUnique();
            e.Property(x => x.SuitColor).HasMaxLength(50);
            e.HasOne(x => x.Breed).WithMany(b => b.Horses)
                .HasForeignKey(x => x.BreedId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ClubRider>(e =>
        {
            e.ToTable("club_riders");
            e.HasKey(x => x.Id);
            e.Property(x => x.FullName).HasMaxLength(150).IsRequired();
            e.Property(x => x.Phone).HasMaxLength(20).IsRequired();
            e.Property(x => x.SkillLevel).HasMaxLength(50);
            e.Property(x => x.DiscountRate).HasPrecision(5, 2);
        });

        modelBuilder.Entity<RidingOrder>(e =>
        {
            e.ToTable("riding_orders");
            e.HasKey(x => x.Id);
            e.Property(x => x.OrderNum).HasMaxLength(30).IsRequired();
            e.HasIndex(x => x.OrderNum).IsUnique();
            e.Property(x => x.TotalPrice).HasPrecision(10, 2);
            e.Property(x => x.Status).HasMaxLength(30).IsRequired();
            e.Property(x => x.CancelReason).HasMaxLength(255);
            e.HasOne(x => x.User).WithMany(u => u.Orders)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.ClubRider).WithMany(r => r.Orders)
                .HasForeignKey(x => x.ClubRiderId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<RidingSessionItem>(e =>
        {
            e.ToTable("riding_session_items");
            e.HasKey(x => x.Id);
            e.Property(x => x.ItemCost).HasPrecision(10, 2);
            e.HasOne(x => x.Order).WithMany(o => o.Items)
                .HasForeignKey(x => x.OrderId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Horse).WithMany(h => h.SessionItems)
                .HasForeignKey(x => x.HorseId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
