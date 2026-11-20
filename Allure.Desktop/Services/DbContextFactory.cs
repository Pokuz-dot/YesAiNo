using Allure.Core.Data;
using Microsoft.EntityFrameworkCore;

namespace Allure.Desktop.Services;

/// <summary>Фабрика контекстов EF Core (Pomelo MySQL) на основе строки подключения.</summary>
public sealed class DbContextFactory
{
    private readonly string _connectionString;

    public DbContextFactory(string connectionString)
    {
        _connectionString = connectionString
            ?? throw new ArgumentNullException(nameof(connectionString));
    }

    public AppDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseMySql(_connectionString, ServerVersion.AutoDetect(_connectionString))
            .Options;
        return new AppDbContext(options);
    }
}
