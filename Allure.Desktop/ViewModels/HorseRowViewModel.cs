using Allure.Core.Models;
using System;

namespace Allure.Desktop.ViewModels;

/// <summary>Строка таблицы лошадей.</summary>
public sealed class HorseRowViewModel
{
    public int Id { get; }
    public string Nickname { get; }
    public string BreedName { get; }
    public int BirthYear { get; }
    public string SuitColor { get; }
    public bool IsReadyForTraining { get; }
    public string ReadyText => IsReadyForTraining ? "Допущена" : "Не допущена";

    public HorseRowViewModel(Horse horse)
    {
        ArgumentNullException.ThrowIfNull(horse);
        Id = horse.Id;
        Nickname = horse.Nickname;
        BreedName = horse.Breed?.BreedName ?? string.Empty;
        BirthYear = horse.BirthYear;
        SuitColor = horse.SuitColor ?? string.Empty;
        IsReadyForTraining = horse.IsReadyForTraining;
    }
}
