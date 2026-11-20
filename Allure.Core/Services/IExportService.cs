using Allure.Core.Models;

namespace Allure.Core.Services;

/// <summary>
/// Экспорт документа строгой отчётности (маршрутный лист тренировки и акт
/// проверки снаряжения с подписью инструктора) в .txt / .html с SHA256-подписью.
/// </summary>
public interface IExportService
{
    string BuildTextDocument(RidingOrder order, string counterpartyName, string instructorName);

    string BuildHtmlDocument(RidingOrder order, string counterpartyName, string instructorName);

    void ExportToFile(string path, RidingOrder order, string counterpartyName, string instructorName);
}
