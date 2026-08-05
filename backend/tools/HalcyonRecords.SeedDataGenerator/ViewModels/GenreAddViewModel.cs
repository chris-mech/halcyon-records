using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HalcyonRecords.SeedDataGenerator.Core.Session;
using HalcyonRecords.SeedDataGenerator.Navigation;

namespace HalcyonRecords.SeedDataGenerator.ViewModels;

public sealed partial class GenreAddViewModel(
    SeedDataSession session,
    NavigationService navigationService
) : ObservableObject
{
    public IReadOnlyList<string> AvailableGenreNames { get; } = session.GetAvailableGenreNames();

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(AddCommand))]
    public partial string? SelectedGenreName { get; set; }

    [RelayCommand(CanExecute = nameof(CanAdd))]
    private void Add()
    {
        session.AddGenre(SelectedGenreName!);
        navigationService.GoBack();
    }

    private bool CanAdd() => SelectedGenreName is not null;

    [RelayCommand]
    private void Cancel() => navigationService.GoBack();
}
