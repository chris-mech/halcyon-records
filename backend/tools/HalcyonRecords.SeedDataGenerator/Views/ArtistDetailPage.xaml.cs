using HalcyonRecords.Shared;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace HalcyonRecords.SeedDataGenerator.Views;

public sealed partial class ArtistDetailPage : Page
{
    public ArtistDetailPage() => InitializeComponent();

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        if (e.Parameter is ArtistSeedEntry artist)
        {
            DetailText.Text = $"Artist detail — coming soon ({artist.Name})";
        }
    }
}
