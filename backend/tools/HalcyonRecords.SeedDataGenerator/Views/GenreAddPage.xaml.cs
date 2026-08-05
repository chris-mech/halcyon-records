using HalcyonRecords.SeedDataGenerator.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;

namespace HalcyonRecords.SeedDataGenerator.Views;

public sealed partial class GenreAddPage : Page
{
    public GenreAddViewModel ViewModel { get; }

    public GenreAddPage()
    {
        ViewModel = App.Current.Services.GetRequiredService<GenreAddViewModel>();
        InitializeComponent();
    }
}
