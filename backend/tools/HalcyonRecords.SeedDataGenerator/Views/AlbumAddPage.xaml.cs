using HalcyonRecords.SeedDataGenerator.Core.Discogs;
using HalcyonRecords.SeedDataGenerator.Core.MusicBrainz;
using HalcyonRecords.SeedDataGenerator.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Windows.System;

namespace HalcyonRecords.SeedDataGenerator.Views;

public sealed partial class AlbumAddPage : Page
{
    public AlbumAddViewModel ViewModel { get; }

    public AlbumAddPage()
    {
        ViewModel = App.Current.Services.GetRequiredService<AlbumAddViewModel>();
        InitializeComponent();

        PreviewPanel.SizeChanged += OnPreviewPanelSizeChanged;
    }

    private void OnSearchResultClick(object sender, ItemClickEventArgs e)
    {
        if (e.ClickedItem is MusicBrainzReleaseSearchResult result)
        {
            ViewModel.SelectSearchResultCommand.Execute(result);
        }
    }

    private void OnDiscogsCandidateClick(object sender, ItemClickEventArgs e)
    {
        if (e.ClickedItem is DiscogsSearchResult candidate)
        {
            ViewModel.SelectDiscogsMasterCommand.Execute(candidate);
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

    private void OnSearchKeyDown(object sender, KeyRoutedEventArgs e)
    {
        if (e.Key == VirtualKey.Enter)
        {
            ViewModel.SearchCommand.Execute(null);
            e.Handled = true;
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
