using HalcyonRecords.SeedDataGenerator.ViewModels;
using HalcyonRecords.Shared;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace HalcyonRecords.SeedDataGenerator.Views;

public sealed partial class ArtistEditPage : Page
{
    public ArtistEditViewModel ViewModel { get; }

    public ArtistEditPage()
    {
        ViewModel = App.Current.Services.GetRequiredService<ArtistEditViewModel>();
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

    private void OnSinceYearBeforeTextChanging(
        TextBox sender,
        TextBoxBeforeTextChangingEventArgs args
    ) => args.Cancel = !args.NewText.All(char.IsDigit);
}
