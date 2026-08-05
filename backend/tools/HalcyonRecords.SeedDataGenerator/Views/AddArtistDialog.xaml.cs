using HalcyonRecords.SeedDataGenerator.Core.MusicBrainz;
using HalcyonRecords.SeedDataGenerator.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;

namespace HalcyonRecords.SeedDataGenerator.Views;

public sealed partial class AddArtistDialog : ContentDialog
{
    public AddArtistDialogViewModel ViewModel { get; }

    public AddArtistDialog()
    {
        ViewModel = App.Current.Services.GetRequiredService<AddArtistDialogViewModel>();
        InitializeComponent();
    }

    private void OnSearchResultClick(object sender, ItemClickEventArgs e)
    {
        if (e.ClickedItem is MusicBrainzArtistSearchResult result)
        {
            ViewModel.SelectSearchResultCommand.Execute(result);
        }
    }
}
