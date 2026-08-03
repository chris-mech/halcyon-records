using HalcyonRecords.SeedDataGenerator.Core.Session;
using HalcyonRecords.SeedDataGenerator.Navigation;
using HalcyonRecords.SeedDataGenerator.Views;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace HalcyonRecords.SeedDataGenerator;

public sealed partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        App.Current.Services.GetRequiredService<NavigationService>().Initialize(ContentFrame);

        RootGrid.Loaded += OnRootGridLoaded;
    }

    private void OnRootGridLoaded(object sender, RoutedEventArgs e)
    {
        RootGrid.Loaded -= OnRootGridLoaded;
        _ = StartSessionAsync();
    }

    private async Task StartSessionAsync()
    {
        try
        {
            var mode = await ShowModePickerAsync();

            var session = App.Current.Services.GetRequiredService<SeedDataSession>();
            await session.LoadAsync(mode);

            ContentFrame.Navigate(typeof(ArtistListPage));
            SaveAndExitButton.IsEnabled = true;
        }
        catch (Exception ex)
        {
            ContentFrame.Content = new TextBlock
            {
                Text = $"Startup failed:\n{ex}",
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(24),
            };
        }
    }

    private async Task<SeedMode> ShowModePickerAsync()
    {
        var dialog = new ContentDialog
        {
            XamlRoot = Content.XamlRoot,
            Title = "Seed data session",
            Content =
                "Merge with the existing seed data, or start fresh and allow overwriting duplicates?",
            PrimaryButtonText = "Merge",
            SecondaryButtonText = "Overwrite",
            DefaultButton = ContentDialogButton.Primary,
        };

        var result = await dialog.ShowAsync();
        return result == ContentDialogResult.Primary ? SeedMode.Merge : SeedMode.Overwrite;
    }

    private async void OnSaveAndExitClick(object sender, RoutedEventArgs e)
    {
        SaveAndExitButton.IsEnabled = false;
        SaveProgressRing.IsActive = true;
        SaveProgressRing.Visibility = Visibility.Visible;

        try
        {
            var session = App.Current.Services.GetRequiredService<SeedDataSession>();
            await session.SaveAsync();
            Application.Current.Exit();
        }
        catch (Exception ex)
        {
            SaveProgressRing.IsActive = false;
            SaveProgressRing.Visibility = Visibility.Collapsed;
            SaveAndExitButton.IsEnabled = true;

            var dialog = new ContentDialog
            {
                XamlRoot = Content.XamlRoot,
                Title = "Save failed",
                Content = ex.Message,
                CloseButtonText = "OK",
            };
            await dialog.ShowAsync();
        }
    }
}
