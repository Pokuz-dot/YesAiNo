using System.Text.RegularExpressions;

namespace Allure.Core.Services;

/// <summary>
/// Маска номера документа: ПР23-КСК-{Год}-{Номер:D5},
/// где КСК — шифр предметной области (конноспортивный клуб).
/// </summary>
public sealed partial class DocumentNumberGenerator : IDocumentNumberGenerator
{
    public const string Prefix = "ПР23-КСК";

    [GeneratedRegex(@"^ПР23-КСК-\d{4}-\d{5}$")]
    private static partial Regex DocumentNumberRegex();

    public string Generate(int year, int index)
    {
        if (index <= 0)
            throw new ArgumentOutOfRangeException(nameof(index), "Индекс документа должен быть положительным.");
        return $"{Prefix}-{year}-{index:D5}";
    }

    public bool IsValid(string? documentNumber) =>
        !string.IsNullOrWhiteSpace(documentNumber) && DocumentNumberRegex().IsMatch(documentNumber.Trim());

    public int NextIndex(IEnumerable<string> existingNumbers, int year)
    {
        ArgumentNullException.ThrowIfNull(existingNumbers);
        int max = 0;
        foreach (string number in existingNumbers)
        {
            if (string.IsNullOrWhiteSpace(number))
                continue;
            string[] parts = number.Trim().Split('-');
            // Формат: ПР23-КСК-2026-00042
            if (parts.Length == 4 && int.TryParse(parts[2], out int y) && y == year
                && int.TryParse(parts[3], out int idx) && idx > max)
            {
                max = idx;
            }
        }
        return max + 1;
    }
}
