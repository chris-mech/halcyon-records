using HalcyonRecords.SeedDataGenerator.ViewModels;
using HalcyonRecords.Shared;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;

namespace HalcyonRecords.SeedDataGenerator.Views;

public sealed partial class ArtistListPage : Page
{
    public ArtistListViewModel ViewModel { get; }

    public ArtistListPage()
    {
        ViewModel = App.Current.Services.GetRequiredService<ArtistListViewModel>();
        InitializeComponent();
    }

    private void OnArtistItemClick(object sender, ItemClickEventArgs e)
    {
        if (e.ClickedItem is ArtistSeedEntry artist)
        {
            ViewModel.ViewArtistCommand.Execute(artist);
        }
    }
}
