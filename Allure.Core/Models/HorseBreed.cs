namespace Allure.Core.Models;

/// <summary>Справочник пород лошадей (таблица horse_breeds).</summary>
public class HorseBreed
{
    public int Id { get; set; }
    public string BreedName { get; set; } = string.Empty;
    public string? Temperament { get; set; }
    public string? OriginCountry { get; set; }

    public ICollection<Horse> Horses { get; set; } = new List<Horse>();
}
