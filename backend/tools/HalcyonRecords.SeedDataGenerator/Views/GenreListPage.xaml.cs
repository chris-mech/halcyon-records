using HalcyonRecords.SeedDataGenerator.ViewModels;
using HalcyonRecords.Shared;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;

namespace HalcyonRecords.SeedDataGenerator.Views;

public sealed partial class GenreListPage : Page
{
    public GenreListViewModel ViewModel { get; }

    public GenreListPage()
    {
        ViewModel = App.Current.Services.GetRequiredService<GenreListViewModel>();
        InitializeComponent();
    }

    private void OnGenreItemClick(object sender, ItemClickEventArgs e)
    {
        if (e.ClickedItem is GenreSeedEntry genre)
        {
            ViewModel.ViewGenreCommand.Execute(genre);
        }
    }
}
