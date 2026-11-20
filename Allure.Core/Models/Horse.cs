namespace Allure.Core.Models;

/// <summary>
/// Спортивная лошадь (таблица horses).
/// IsReadyForTraining = false блокирует назначение лошади на тренировку (FSM-ограничение).
/// </summary>
public class Horse
{
    public int Id { get; set; }
    public int BreedId { get; set; }
    public string Nickname { get; set; } = string.Empty;
    public string PassportChipCode { get; set; } = string.Empty;
    public int BirthYear { get; set; }
    public string? SuitColor { get; set; }
    public bool IsReadyForTraining { get; set; } = true;

    public HorseBreed? Breed { get; set; }
    public ICollection<RidingSessionItem> SessionItems { get; set; } = new List<RidingSessionItem>();
}
