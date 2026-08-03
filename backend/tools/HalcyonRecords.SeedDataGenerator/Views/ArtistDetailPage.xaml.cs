using HalcyonRecords.SeedDataGenerator.ViewModels;
using HalcyonRecords.Shared;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace HalcyonRecords.SeedDataGenerator.Views;

public sealed partial class ArtistDetailPage : Page
{
    public ArtistDetailViewModel ViewModel { get; }

    public ArtistDetailPage()
    {
        ViewModel = App.Current.Services.GetRequiredService<ArtistDetailViewModel>();
        InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        if (e.Parameter is ArtistSeedEntry artist)
        {
            ViewModel.Initialize(artist);
        }
    }

    private async void OnDeleteClick(object sender, RoutedEventArgs e)
    {
        if (ViewModel.Artist is not { } artist)
        {
            return;
        }

        var dialog = new ContentDialog
        {
            XamlRoot = XamlRoot,
            Title = $"Delete {artist.Name}?",
            Content = ViewModel.Albums.Count switch
            {
                0 => "This cannot be undone.",
                var count => $"This will also delete {count} album(s): "
                    + string.Join(", ", ViewModel.Albums.Select(a => a.Title))
                    + ". This cannot be undone.",
            },
            PrimaryButtonText = "Delete",
            CloseButtonText = "Cancel",
            DefaultButton = ContentDialogButton.Close,
        };

        var result = await dialog.ShowAsync();
        if (result == ContentDialogResult.Primary)
        {
            ViewModel.ConfirmDeleteCommand.Execute(null);
        }
    }
}
