using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HalcyonRecords.SeedDataGenerator.Core.Session;
using HalcyonRecords.SeedDataGenerator.Navigation;
using HalcyonRecords.SeedDataGenerator.Views;
using HalcyonRecords.Shared;

namespace HalcyonRecords.SeedDataGenerator.ViewModels;

public sealed partial class GenreDetailViewModel(
    SeedDataSession session,
    NavigationService navigationService
) : ObservableObject
{
    [ObservableProperty]
    public partial GenreSeedEntry? Genre { get; set; }

    public ObservableCollection<AlbumSeedEntry> Albums { get; } = [];

    public void Initialize(GenreSeedEntry genre)
    {
        var current = session.Genres.FirstOrDefault(g => g.Slug == genre.Slug) ?? genre;
        Genre = current;

        Albums.Clear();
        foreach (var album in session.GetAlbumsReferencing(current.Slug))
        {
            Albums.Add(album);
        }
    }

    [RelayCommand]
    private void Edit()
    {
        if (Genre is { } genre)
        {
            navigationService.Navigate(typeof(GenreEditPage), genre);
        }
    }

    [RelayCommand]
    private void ConfirmDelete()
    {
        if (Genre is not { } genre)
        {
            return;
        }

        session.DeleteGenre(genre.Slug);
        navigationService.GoBack();
    }

    [RelayCommand]
    private void Back() => navigationService.GoBack();
}
