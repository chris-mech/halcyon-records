using HalcyonRecords.SeedDataGenerator.Core.MusicBrainz;
using HalcyonRecords.SeedDataGenerator.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace HalcyonRecords.SeedDataGenerator.Views;

public sealed partial class ArtistAddPage : Page
{
    public ArtistAddViewModel ViewModel { get; }

    public ArtistAddPage()
    {
        ViewModel = App.Current.Services.GetRequiredService<ArtistAddViewModel>();
        InitializeComponent();

        PreviewPanel.SizeChanged += OnPreviewPanelSizeChanged;
    }

    private void OnSearchResultClick(object sender, ItemClickEventArgs e)
    {
        if (e.ClickedItem is MusicBrainzArtistSearchResult result)
        {
            ViewModel.SelectSearchResultCommand.Execute(result);
        }
    }

    private void OnPreviewPanelSizeChanged(object sender, SizeChangedEventArgs e)
    {
        if (ViewModel.Plan is not null && e.NewSize.Height > 0)
        {
            PreviewPanel.StartBringIntoView();
        }
    }
}
