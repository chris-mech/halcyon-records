using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HalcyonRecords.SeedDataGenerator.Core.Session;
using HalcyonRecords.SeedDataGenerator.Navigation;
using HalcyonRecords.SeedDataGenerator.Views;
using HalcyonRecords.Shared;

namespace HalcyonRecords.SeedDataGenerator.ViewModels;

public sealed partial class AlbumListViewModel(
    SeedDataSession session,
    NavigationService navigationService
) : ObservableObject
{
    public ObservableCollection<AlbumSeedEntry> Albums { get; } =
        new(session.Albums.OrderBy(a => a.Title, StringComparer.CurrentCultureIgnoreCase));

    [RelayCommand]
    private void AddAlbum() => navigationService.Navigate(typeof(AlbumAddPage));

    [RelayCommand]
    private void ViewAlbum(AlbumSeedEntry album) =>
        navigationService.Navigate(typeof(AlbumDetailPage), album);
}
