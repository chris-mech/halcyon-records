using HalcyonRecords.SeedDataGenerator.Core.Session;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;

namespace HalcyonRecords.SeedDataGenerator;

public sealed partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        var session = App.Current.Services.GetRequiredService<SeedDataSession>();
        StatusText.Text = $"Seed data session ready ({session.Mode} mode).";
    }
}
