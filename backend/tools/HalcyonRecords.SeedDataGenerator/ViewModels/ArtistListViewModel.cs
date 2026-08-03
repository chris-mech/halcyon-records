using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HalcyonRecords.SeedDataGenerator.Core.Session;
using HalcyonRecords.SeedDataGenerator.Navigation;
using HalcyonRecords.SeedDataGenerator.Views;
using HalcyonRecords.Shared;

namespace HalcyonRecords.SeedDataGenerator.ViewModels;

public sealed partial class ArtistListViewModel(
    SeedDataSession session,
    NavigationService navigationService
) : ObservableObject
{
    public ObservableCollection<ArtistSeedEntry> Artists { get; } =
        new(session.Artists.OrderBy(a => a.Name, StringComparer.CurrentCultureIgnoreCase));

    [RelayCommand]
    private void AddArtist() => navigationService.Navigate(typeof(ArtistAddPage));

    [RelayCommand]
    private void ViewArtist(ArtistSeedEntry artist) =>
        navigationService.Navigate(typeof(ArtistDetailPage), artist);
}
