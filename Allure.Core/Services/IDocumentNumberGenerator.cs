namespace Allure.Core.Services;

/// <summary>Генератор номера документа по маске ПР23-КСК-{Год}-{Номер:D5}.</summary>
public interface IDocumentNumberGenerator
{
    string Generate(int year, int index);

    bool IsValid(string? documentNumber);

    /// <summary>Следующий свободный индекс для года по уже занятым номерам.</summary>
    int NextIndex(IEnumerable<string> existingNumbers, int year);
}
