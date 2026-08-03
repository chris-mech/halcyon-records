using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HalcyonRecords.SeedDataGenerator.Core.Session;
using HalcyonRecords.SeedDataGenerator.Navigation;
using HalcyonRecords.Shared;

namespace HalcyonRecords.SeedDataGenerator.ViewModels;

public sealed partial class ArtistEditViewModel(
    SeedDataSession session,
    NavigationService navigationService
) : ObservableObject
{
    public IReadOnlyList<string> TypeOptions { get; } =
    ["Unknown", "Person", "Group", "Orchestra", "Choir", "Character", "Other"];

    private ArtistMbid _artistId;

    [ObservableProperty]
    public partial string Name { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string TypeSelection { get; set; } = "Unknown";

    [ObservableProperty]
    public partial string? Origin { get; set; }

    [ObservableProperty]
    public partial string? SinceYearInput { get; set; }

    [ObservableProperty]
    public partial string? Bio { get; set; }

    [ObservableProperty]
    public partial string? ImageUrl { get; set; }

    [ObservableProperty]
    public partial string? ErrorMessage { get; set; }

    [ObservableProperty]
    public partial bool IsBusy { get; set; }

    public void Initialize(ArtistSeedEntry artist)
    {
        _artistId = artist.SourceId;
        Populate(
            name: artist.Name,
            origin: artist.Origin,
            type: artist.Type,
            sinceYear: artist.SinceYear,
            bio: artist.Bio,
            imageUrl: artist.ImageUrl
        );
    }

    [RelayCommand]
    private async Task RefreshAsync()
    {
        IsBusy = true;
        ErrorMessage = null;

        var result = await session.RefreshArtistAsync(_artistId);

        IsBusy = false;

        if (result.IsError)
        {
            ErrorMessage = string.Join("; ", result.Errors.Select(e => e.Description));
            return;
        }

        var plan = result.Value;
        Populate(
            name: plan.Name,
            origin: plan.Origin,
            type: plan.Type,
            sinceYear: plan.SinceYear,
            bio: plan.Bio,
            imageUrl: plan.ImageUrl
        );
    }

    [RelayCommand]
    private void Save()
    {
        if (string.IsNullOrWhiteSpace(Name))
        {
            ErrorMessage = "Name is required.";
            return;
        }

        var sinceYear = int.TryParse(SinceYearInput, out var year) ? year : (int?)null;

        if (sinceYear is < 1000 or > 2100)
        {
            ErrorMessage = "Since year must be between 1000 and 2100.";
            return;
        }

        var type =
            TypeSelection == "Unknown" ? (ArtistType?)null : Enum.Parse<ArtistType>(TypeSelection);

        session.UpdateArtist(
            artistId: _artistId,
            name: Name,
            origin: Origin,
            type: type,
            sinceYear: sinceYear,
            bio: Bio,
            imageUrl: ImageUrl
        );
        navigationService.GoBack();
    }

    [RelayCommand]
    private void Cancel() => navigationService.GoBack();

    private void Populate(
        string name,
        string? origin,
        ArtistType? type,
        int? sinceYear,
        string? bio,
        string? imageUrl
    )
    {
        Name = name;
        Origin = origin;
        TypeSelection = type?.ToString() ?? "Unknown";
        SinceYearInput = sinceYear?.ToString();
        Bio = bio;
        ImageUrl = imageUrl;
    }
}
