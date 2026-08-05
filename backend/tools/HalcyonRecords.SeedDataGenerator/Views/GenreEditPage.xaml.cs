using HalcyonRecords.SeedDataGenerator.ViewModels;
using HalcyonRecords.Shared;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace HalcyonRecords.SeedDataGenerator.Views;

public sealed partial class GenreEditPage : Page
{
    public GenreEditViewModel ViewModel { get; }

    public GenreEditPage()
    {
        ViewModel = App.Current.Services.GetRequiredService<GenreEditViewModel>();
        InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        if (e.Parameter is GenreSeedEntry genre)
        {
            ViewModel.Initialize(genre);
        }
    }
}
