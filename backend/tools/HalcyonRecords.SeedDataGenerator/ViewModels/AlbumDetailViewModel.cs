using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HalcyonRecords.SeedDataGenerator.Core.Session;
using HalcyonRecords.SeedDataGenerator.Navigation;
using HalcyonRecords.SeedDataGenerator.Views;
using HalcyonRecords.Shared;

namespace HalcyonRecords.SeedDataGenerator.ViewModels;

public sealed partial class AlbumDetailViewModel(
    SeedDataSession session,
    NavigationService navigationService
) : ObservableObject
{
    [ObservableProperty]
    public partial AlbumSeedEntry? Album { get; set; }

    [ObservableProperty]
    public partial string ArtistNames { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string GenreNames { get; set; } = string.Empty;

    public void Initialize(AlbumSeedEntry album)
    {
        var current = session.Albums.FirstOrDefault(a => a.SourceId == album.SourceId) ?? album;
        Album = current;

        ArtistNames =
            current.ArtistSourceIds.Count > 0
                ? string.Join(
                    ", ",
                    current.ArtistSourceIds.Select(id =>
                        session.Artists.FirstOrDefault(a => a.SourceId == id)?.Name
                        ?? "Unknown artist"
                    )
                )
                : "Unknown";

        GenreNames =
            current.GenreSlugs.Count > 0
                ? string.Join(
                    ", ",
                    current.GenreSlugs.Select(slug =>
                        session.Genres.FirstOrDefault(g => g.Slug == slug)?.Name ?? slug.Value
                    )
                )
                : "Unknown";
    }

    [RelayCommand]
    private void Edit()
    {
        if (Album is { } album)
        {
            navigationService.Navigate(typeof(AlbumEditPage), album);
        }
    }

    [RelayCommand]
    private void ConfirmDelete()
    {
        if (Album is not { } album)
        {
            return;
        }

        session.DeleteAlbum(album.SourceId);
        navigationService.GoBack();
    }

    [RelayCommand]
    private void Back() => navigationService.GoBack();
}
