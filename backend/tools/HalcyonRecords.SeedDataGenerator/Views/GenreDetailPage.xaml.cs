using HalcyonRecords.SeedDataGenerator.ViewModels;
using HalcyonRecords.Shared;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace HalcyonRecords.SeedDataGenerator.Views;

public sealed partial class GenreDetailPage : Page
{
    public GenreDetailViewModel ViewModel { get; }

    public GenreDetailPage()
    {
        ViewModel = App.Current.Services.GetRequiredService<GenreDetailViewModel>();
        InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        if (e.Parameter is GenreSeedEntry genre)
        {
            ViewModel.Initialize(genre);
        }
    }

    private async void OnDeleteClick(object sender, RoutedEventArgs e)
    {
        if (ViewModel.Genre is not { } genre)
        {
            return;
        }

        var dialog = new ContentDialog
        {
            XamlRoot = XamlRoot,
            Title = $"Delete {genre.Name}?",
            Content = ViewModel.Albums.Count switch
            {
                0 => "This cannot be undone.",
                var count => $"This will remove the genre from {count} album(s): "
                    + string.Join(", ", ViewModel.Albums.Select(a => a.Title))
                    + ". The albums themselves are not deleted. This cannot be undone.",
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
