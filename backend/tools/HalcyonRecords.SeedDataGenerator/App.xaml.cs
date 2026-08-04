using HalcyonRecords.SeedDataGenerator.Core.CoverArtArchive;
using HalcyonRecords.SeedDataGenerator.Core.Discogs;
using HalcyonRecords.SeedDataGenerator.Core.MusicBrainz;
using HalcyonRecords.SeedDataGenerator.Core.Session;
using HalcyonRecords.SeedDataGenerator.Core.Wikidata;
using HalcyonRecords.SeedDataGenerator.Core.Wikipedia;
using HalcyonRecords.SeedDataGenerator.Navigation;
using HalcyonRecords.SeedDataGenerator.ViewModels;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;

namespace HalcyonRecords.SeedDataGenerator;

public partial class App : Application
{
    private Window? _window;

    public IServiceProvider Services { get; }

    public App()
    {
        Services = ConfigureServices();
        InitializeComponent();
    }

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        _window = new MainWindow();
        _window.Activate();
    }

    public static new App Current => (App)Application.Current;

    private static IServiceProvider ConfigureServices()
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
            .AddUserSecrets<App>()
            .Build();

        var services = new ServiceCollection();

        services.AddMusicBrainzClient(configuration);
        services.AddDiscogsClient(configuration);
        services.AddCoverArtArchiveClient(configuration);
        services.AddWikidataClient(configuration);
        services.AddWikipediaClient(configuration);
        services.AddSeedDataSession(configuration);
        services.AddSingleton<NavigationService>();
        services.AddTransient<ArtistListViewModel>();
        services.AddTransient<ArtistAddViewModel>();
        services.AddTransient<ArtistDetailViewModel>();
        services.AddTransient<ArtistEditViewModel>();
        services.AddTransient<AlbumListViewModel>();

        return services.BuildServiceProvider();
    }
}
