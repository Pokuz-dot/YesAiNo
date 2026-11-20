using Allure.Core.Services;
using Xunit;

namespace Allure.Tests;

/// <summary>Тесты маски номера документа и SHA256-подписи.</summary>
public sealed class IntegrityAndNumberTests
{
    [Fact]
    public void DocumentNumber_Generate_UsesMaskPR23KSK()
    {
        var generator = new DocumentNumberGenerator();
        Assert.Equal("ПР23-КСК-2026-00042", generator.Generate(2026, 42));
        Assert.True(generator.IsValid("ПР23-КСК-2026-00042"));
        Assert.False(generator.IsValid("2026-42"));
    }

    [Fact]
    public void DocumentNumber_NextIndex_FindsMaxForYear()
    {
        var generator = new DocumentNumberGenerator();
        var existing = new[] { "ПР23-КСК-2026-00001", "ПР23-КСК-2026-00007", "ПР23-КСК-2025-00099" };
        Assert.Equal(8, generator.NextIndex(existing, 2026));
    }

    [Fact]
    public void Integrity_ComputeHash_IsDeterministic64HexChars()
    {
        var service = new IntegrityService();
        var date = new DateTime(2026, 11, 21, 10, 0, 0);
        string first = service.ComputeHash("ПР23-КСК-2026-00001", date, "Анна Соколова", 4950m, 2);
        string second = service.ComputeHash("ПР23-КСК-2026-00001", date, "Анна Соколова", 4950m, 2);
        Assert.Equal(first, second);
        Assert.Equal(64, first.Length);
    }

    [Fact]
    public void Integrity_ComputeHash_ChangesWhenTotalChanges()
    {
        var service = new IntegrityService();
        var date = new DateTime(2026, 11, 21, 10, 0, 0);
        string original = service.ComputeHash("ПР23-КСК-2026-00001", date, "Анна Соколова", 4950m, 2);
        string tampered = service.ComputeHash("ПР23-КСК-2026-00001", date, "Анна Соколова", 9999m, 2);
        Assert.NotEqual(original, tampered);
    }
}
