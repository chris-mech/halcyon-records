using HalcyonRecords.SeedDataGenerator.ViewModels;
using HalcyonRecords.Shared;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace HalcyonRecords.SeedDataGenerator.Views;

public sealed partial class AlbumEditPage : Page
{
    public AlbumEditViewModel ViewModel { get; }

    public AlbumEditPage()
    {
        ViewModel = App.Current.Services.GetRequiredService<AlbumEditViewModel>();
        InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        if (e.Parameter is AlbumSeedEntry album)
        {
            ViewModel.Initialize(album);
        }
    }

    private async void OnAddNewArtistClick(object sender, RoutedEventArgs e)
    {
        var dialog = new AddArtistDialog { XamlRoot = XamlRoot };
        var result = await dialog.ShowAsync();

        if (result == ContentDialogResult.Primary)
        {
            var entry = dialog.ViewModel.Commit();
            if (entry is not null)
            {
                ViewModel.AddArtistOption(entry);
            }
        }
    }

    private void OnUnitsInStockBeforeTextChanging(
        TextBox sender,
        TextBoxBeforeTextChangingEventArgs args
    ) => args.Cancel = !args.NewText.All(char.IsDigit);

    private void OnPriceBeforeTextChanging(
        TextBox sender,
        TextBoxBeforeTextChangingEventArgs args
    ) => args.Cancel = !IsValidPriceText(args.NewText);

    private static bool IsValidPriceText(string text) =>
        text.Length == 0
        || (text.All(c => char.IsDigit(c) || c == '.') && text.Count(c => c == '.') <= 1);
}
