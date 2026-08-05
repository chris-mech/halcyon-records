using HalcyonRecords.SeedDataGenerator.Core.MusicBrainz;
using HalcyonRecords.SeedDataGenerator.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Windows.System;

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

    private void OnMbidInputKeyDown(object sender, KeyRoutedEventArgs e)
    {
        if (e.Key == VirtualKey.Enter)
        {
            ViewModel.LookUpMbidCommand.Execute(null);
            e.Handled = true;
        }
    }

    private void OnSearchNameKeyDown(object sender, KeyRoutedEventArgs e)
    {
        if (e.Key == VirtualKey.Enter)
        {
            ViewModel.SearchCommand.Execute(null);
            e.Handled = true;
        }
    }
}
