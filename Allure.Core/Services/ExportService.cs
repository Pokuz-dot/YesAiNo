using System.Globalization;
using System.Text;
using Allure.Core.Models;

namespace Allure.Core.Services;

/// <summary>
/// Формирует маршрутный лист тренировки и акт проверки снаряжения,
/// в нижнюю строку печатает цифровую подпись SHA256.
/// </summary>
public sealed class ExportService : IExportService
{
    private readonly IIntegrityService _integrity;

    public ExportService(IIntegrityService integrity)
    {
        _integrity = integrity ?? throw new ArgumentNullException(nameof(integrity));
    }

    public string BuildTextDocument(RidingOrder order, string counterpartyName, string instructorName)
    {
        ArgumentNullException.ThrowIfNull(order);
        string hash = _integrity.ComputeHash(order.OrderNum, order.OrderDate, counterpartyName,
            order.TotalPrice, order.Items.Count);

        var sb = new StringBuilder();
        sb.AppendLine("КОННОСПОРТИВНЫЙ КЛУБ И ИППОДРОМ «АЛЛЮР»");
        sb.AppendLine("МАРШРУТНЫЙ ЛИСТ ТРЕНИРОВКИ И АКТ ПРОВЕРКИ СНАРЯЖЕНИЯ");
        sb.AppendLine(new string('=', 60));
        sb.AppendLine($"Номер документа: {order.OrderNum}");
        sb.AppendLine($"Дата: {order.OrderDate:yyyy-MM-dd HH:mm:ss}");
        sb.AppendLine($"Всадник (контрагент): {counterpartyName}");
        sb.AppendLine($"Статус: {order.Status}");
        sb.AppendLine($"Итого к оплате: {order.TotalPrice:F2} руб.");
        sb.AppendLine($"Количество позиций: {order.Items.Count}");
        sb.AppendLine();
        sb.AppendLine("Позиции:");
        int n = 1;
        foreach (RidingSessionItem item in order.Items)
        {
            string horse = item.Horse?.Nickname ?? $"Лошадь #{item.HorseId}";
            sb.AppendLine($"  {n}. {horse} — {item.SessionDateTime:yyyy-MM-dd HH:mm} — " +
                          $"{item.DurationMinutes} мин. — тренер: {(item.HasTrainer ? "да" : "нет")} — " +
                          $"{item.ItemCost:F2} руб.");
            n++;
        }
        sb.AppendLine();
        sb.AppendLine("Акт проверки снаряжения (седла, шлемы, уздечки): проверено.");
        sb.AppendLine($"Подпись инструктора по верховой езде: {instructorName} /____________/");
        sb.AppendLine();
        sb.AppendLine($"SHA256: {hash}");
        return sb.ToString();
    }

    public string BuildHtmlDocument(RidingOrder order, string counterpartyName, string instructorName)
    {
        ArgumentNullException.ThrowIfNull(order);
        string hash = _integrity.ComputeHash(order.OrderNum, order.OrderDate, counterpartyName,
            order.TotalPrice, order.Items.Count);
        string Escape(string s) => System.Net.WebUtility.HtmlEncode(s);

        var sb = new StringBuilder();
        sb.AppendLine("<!DOCTYPE html><html lang=\"ru\"><head><meta charset=\"utf-8\">" +
                      "<title>Маршрутный лист тренировки</title></head><body>");
        sb.AppendLine("<h1>Конноспортивный клуб и ипподром «Аллюр»</h1>");
        sb.AppendLine("<h2>Маршрутный лист тренировки и акт проверки снаряжения</h2>");
        sb.AppendLine($"<p>Номер документа: {Escape(order.OrderNum)}<br>" +
                      $"Дата: {order.OrderDate:yyyy-MM-dd HH:mm:ss}<br>" +
                      $"Всадник: {Escape(counterpartyName)}<br>" +
                      $"Статус: {Escape(order.Status)}<br>" +
                      $"Итого: {order.TotalPrice.ToString("F2", CultureInfo.InvariantCulture)} руб.<br>" +
                      $"Позиций: {order.Items.Count}</p>");
        sb.AppendLine("<table border=\"1\"><tr><th>№</th><th>Лошадь</th><th>Дата и время</th>" +
                      "<th>Минут</th><th>Тренер</th><th>Сумма</th></tr>");
        int n = 1;
        foreach (RidingSessionItem item in order.Items)
        {
            string horse = item.Horse?.Nickname ?? $"Лошадь #{item.HorseId}";
            sb.AppendLine($"<tr><td>{n}</td><td>{Escape(horse)}</td>" +
                          $"<td>{item.SessionDateTime:yyyy-MM-dd HH:mm}</td><td>{item.DurationMinutes}</td>" +
                          $"<td>{(item.HasTrainer ? "да" : "нет")}</td>" +
                          $"<td>{item.ItemCost.ToString("F2", CultureInfo.InvariantCulture)}</td></tr>");
            n++;
        }
        sb.AppendLine("</table>");
        sb.AppendLine($"<p>Подпись инструктора по верховой езде: {Escape(instructorName)} /____________/</p>");
        sb.AppendLine($"<p>SHA256: {hash}</p>");
        sb.AppendLine("</body></html>");
        return sb.ToString();
    }

    public void ExportToFile(string path, RidingOrder order, string counterpartyName, string instructorName)
    {
        ArgumentNullException.ThrowIfNull(path);
        string content = path.EndsWith(".html", StringComparison.OrdinalIgnoreCase)
            ? BuildHtmlDocument(order, counterpartyName, instructorName)
            : BuildTextDocument(order, counterpartyName, instructorName);
        File.WriteAllText(path, content, Encoding.UTF8);
    }
}
