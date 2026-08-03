using HalcyonRecords.Shared;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace HalcyonRecords.SeedDataGenerator.Views;

public sealed partial class ArtistEditPage : Page
{
    public ArtistEditPage() => InitializeComponent();

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        if (e.Parameter is ArtistSeedEntry artist)
        {
            DetailText.Text = $"Artist edit — coming soon ({artist.Name})";
        }
    }
}
