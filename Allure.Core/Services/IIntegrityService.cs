namespace Allure.Core.Services;

/// <summary>
/// Сервис контроля целостности: SHA256 от конкатенации
/// [Номер]+[Дата]+[Контрагент]+[Сумма]+[КоличествоПозиций].
/// </summary>
public interface IIntegrityService
{
    string BuildCanonicalString(string orderNum, DateTime orderDate, string counterparty,
        decimal totalPrice, int itemCount);

    string ComputeHash(string orderNum, DateTime orderDate, string counterparty,
        decimal totalPrice, int itemCount);
}
