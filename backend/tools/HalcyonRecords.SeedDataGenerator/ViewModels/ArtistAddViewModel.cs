using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HalcyonRecords.SeedDataGenerator.Core.MusicBrainz;
using HalcyonRecords.SeedDataGenerator.Core.Session;
using HalcyonRecords.SeedDataGenerator.Navigation;
using HalcyonRecords.Shared;

namespace HalcyonRecords.SeedDataGenerator.ViewModels;

public sealed partial class ArtistAddViewModel(
    SeedDataSession session,
    NavigationService navigationService
) : ObservableObject
{
    [ObservableProperty]
    public partial string? SearchName { get; set; }

    [ObservableProperty]
    public partial string? MbidInput { get; set; }

    [ObservableProperty]
    public partial string? ErrorMessage { get; set; }

    [ObservableProperty]
    public partial bool IsBusy { get; set; }

    [ObservableProperty]
    public partial AddArtistPlan? Plan { get; set; }

    [ObservableProperty]
    public partial string PlanName { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string PlanType { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string PlanOrigin { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string PlanSinceYear { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string PlanBio { get; set; } = string.Empty;
    public ObservableCollection<MusicBrainzArtistSearchResult> SearchResults { get; } = [];

    [RelayCommand]
    private async Task SearchAsync()
    {
        if (string.IsNullOrWhiteSpace(SearchName))
        {
            ErrorMessage = "Enter an artist name to search.";
            return;
        }

        IsBusy = true;
        ErrorMessage = null;
        Plan = null;
        SearchResults.Clear();

        var results = await session.SearchArtistsAsync(SearchName);

        foreach (var result in results)
        {
            SearchResults.Add(result);
        }

        IsBusy = false;
    }

    [RelayCommand]
    private async Task SelectSearchResultAsync(MusicBrainzArtistSearchResult result)
    {
        if (result.Id is not { } id)
        {
            return;
        }

        await PreviewAsync(new ArtistMbid(id));
    }

    [RelayCommand]
    private async Task LookUpMbidAsync()
    {
        if (!Guid.TryParse(MbidInput, out var id))
        {
            ErrorMessage = "That doesn't look like a valid MusicBrainz ID.";
            ClearPlan();
            return;
        }

        await PreviewAsync(new ArtistMbid(id));
    }

    [RelayCommand]
    private void Commit()
    {
        if (Plan is null)
        {
            return;
        }

        session.CommitAddArtist(Plan);
        navigationService.GoBack();
    }

    [RelayCommand]
    private void Cancel() => navigationService.GoBack();

    private async Task PreviewAsync(ArtistMbid artistId)
    {
        IsBusy = true;
        ErrorMessage = null;

        var result = await session.PreviewAddArtistAsync(artistId);

        IsBusy = false;

        if (result.IsError)
        {
            ErrorMessage = string.Join("; ", result.Errors.Select(e => e.Description));
            ClearPlan();
            return;
        }

        Plan = result.Value;
        PlanName = Plan.Name;
        PlanType = BindingHelpers.FormatArtistType(Plan.Type);
        PlanOrigin = BindingHelpers.OrPlaceholder(Plan.Origin, "Unknown");
        PlanSinceYear = BindingHelpers.FormatYear(Plan.SinceYear);
        PlanBio = BindingHelpers.OrPlaceholder(Plan.Bio, "No bio available.");
    }

    private void ClearPlan()
    {
        Plan = null;
        PlanName = string.Empty;
        PlanType = string.Empty;
        PlanOrigin = string.Empty;
        PlanSinceYear = string.Empty;
        PlanBio = string.Empty;
    }
}
