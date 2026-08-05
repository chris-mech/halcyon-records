using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HalcyonRecords.SeedDataGenerator.Core.Session;
using HalcyonRecords.SeedDataGenerator.Navigation;
using HalcyonRecords.Shared;

namespace HalcyonRecords.SeedDataGenerator.ViewModels;

public sealed partial class GenreEditViewModel(
    SeedDataSession session,
    NavigationService navigationService
) : ObservableObject
{
    private GenreSlug _slug;

    [ObservableProperty]
    public partial string Name { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string? Description { get; set; }

    public void Initialize(GenreSeedEntry genre)
    {
        _slug = genre.Slug;
        Name = genre.Name;
        Description = genre.Description;
    }

    [RelayCommand]
    private void Save()
    {
        session.UpdateGenreDescription(_slug, Description);
        navigationService.GoBack();
    }

    [RelayCommand]
    private void Cancel() => navigationService.GoBack();
}
