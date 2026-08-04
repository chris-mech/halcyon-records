using HalcyonRecords.SeedDataGenerator.ViewModels;
using HalcyonRecords.Shared;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;

namespace HalcyonRecords.SeedDataGenerator.Views;

public sealed partial class AlbumListPage : Page
{
    public AlbumListViewModel ViewModel { get; }

    public AlbumListPage()
    {
        ViewModel = App.Current.Services.GetRequiredService<AlbumListViewModel>();
        InitializeComponent();
    }

    private void OnAlbumItemClick(object sender, ItemClickEventArgs e)
    {
        if (e.ClickedItem is AlbumSeedEntry album)
        {
            ViewModel.ViewAlbumCommand.Execute(album);
        }
    }
}
