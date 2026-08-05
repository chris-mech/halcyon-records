using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HalcyonRecords.SeedDataGenerator.Core.Session;
using HalcyonRecords.SeedDataGenerator.Navigation;
using HalcyonRecords.SeedDataGenerator.Views;
using HalcyonRecords.Shared;

namespace HalcyonRecords.SeedDataGenerator.ViewModels;

public sealed partial class GenreListViewModel(
    SeedDataSession session,
    NavigationService navigationService
) : ObservableObject
{
    public ObservableCollection<GenreSeedEntry> Genres { get; } =
        new(session.Genres.OrderBy(g => g.Name, StringComparer.CurrentCultureIgnoreCase));

    [RelayCommand]
    private void AddGenre() => navigationService.Navigate(typeof(GenreAddPage));

    [RelayCommand]
    private void ViewGenre(GenreSeedEntry genre) =>
        navigationService.Navigate(typeof(GenreDetailPage), genre);
}
