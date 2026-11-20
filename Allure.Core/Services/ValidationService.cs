using System.Text.RegularExpressions;

namespace Allure.Core.Services;

/// <summary>Валидация полей ввода: телефоны, номера документов, суммы.</summary>
public sealed partial class ValidationService
{
    /// <summary>Формат телефона: +7(XXX)XXX-XX-XX.</summary>
    [GeneratedRegex(@"^\+7\(\d{3}\)\d{3}-\d{2}-\d{2}$")]
    private static partial Regex PhoneRegex();

    public bool IsValidPhone(string? phone) =>
        !string.IsNullOrWhiteSpace(phone) && PhoneRegex().IsMatch(phone.Trim());

    public bool IsValidDocumentNumber(string? documentNumber, IDocumentNumberGenerator generator) =>
        generator.IsValid(documentNumber);

    /// <summary>Запрет отрицательных сумм.</summary>
    public bool IsValidMoney(decimal amount) => amount >= 0;

    public bool IsValidDuration(int durationMinutes) => durationMinutes > 0;
}
