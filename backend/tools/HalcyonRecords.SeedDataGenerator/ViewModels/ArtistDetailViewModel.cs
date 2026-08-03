using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HalcyonRecords.SeedDataGenerator.Core.Session;
using HalcyonRecords.SeedDataGenerator.Navigation;
using HalcyonRecords.SeedDataGenerator.Views;
using HalcyonRecords.Shared;

namespace HalcyonRecords.SeedDataGenerator.ViewModels;

public sealed partial class ArtistDetailViewModel(
    SeedDataSession session,
    NavigationService navigationService
) : ObservableObject
{
    [ObservableProperty]
    public partial ArtistSeedEntry? Artist { get; set; }

    public ObservableCollection<AlbumSeedEntry> Albums { get; } = [];

    public void Initialize(ArtistSeedEntry artist)
    {
        var current = session.Artists.FirstOrDefault(a => a.SourceId == artist.SourceId) ?? artist;
        Artist = current;

        Albums.Clear();
        foreach (var album in session.GetAlbumsReferencing(current.SourceId))
        {
            Albums.Add(album);
        }
    }

    [RelayCommand]
    private void Edit()
    {
        if (Artist is { } artist)
        {
            navigationService.Navigate(typeof(ArtistEditPage), artist);
        }
    }

    [RelayCommand]
    private void ConfirmDelete()
    {
        if (Artist is not { } artist)
        {
            return;
        }

        session.DeleteArtist(artist.SourceId);
        navigationService.GoBack();
    }

    [RelayCommand]
    private void Back() => navigationService.GoBack();
}
