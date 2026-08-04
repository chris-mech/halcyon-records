using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HalcyonRecords.SeedDataGenerator.Core.Common;
using HalcyonRecords.SeedDataGenerator.Core.Discogs;
using HalcyonRecords.SeedDataGenerator.Core.MusicBrainz;
using HalcyonRecords.SeedDataGenerator.Core.Session;
using HalcyonRecords.SeedDataGenerator.Navigation;
using HalcyonRecords.Shared;

namespace HalcyonRecords.SeedDataGenerator.ViewModels;

public sealed partial class AlbumAddViewModel(
    SeedDataSession session,
    NavigationService navigationService
) : ObservableObject
{
    [ObservableProperty]
    public partial string? SearchArtistName { get; set; }

    [ObservableProperty]
    public partial string? SearchReleaseTitle { get; set; }

    [ObservableProperty]
    public partial string? MbidInput { get; set; }

    [ObservableProperty]
    public partial string? ErrorMessage { get; set; }

    [ObservableProperty]
    public partial string? CommitErrorMessage { get; set; }

    [ObservableProperty]
    public partial bool IsBusy { get; set; }

    [ObservableProperty]
    public partial AddAlbumPlan? Plan { get; set; }

    [ObservableProperty]
    public partial string PlanTitle { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string PlanReleaseDate { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string PlanLabel { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string PlanDescription { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string PlanArtistCredits { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string PlanGenres { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string PlanNewArtists { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string? UnitsInStockInput { get; set; }

    [ObservableProperty]
    public partial string? PriceInput { get; set; }

    [ObservableProperty]
    public partial string? OriginalPriceInput { get; set; }

    [ObservableProperty]
    public partial bool IsNew { get; set; }

    [ObservableProperty]
    public partial bool IsStaffPick { get; set; }

    public ObservableCollection<MusicBrainzReleaseSearchResult> SearchResults { get; } = [];

    public ObservableCollection<DiscogsSearchResult> DiscogsCandidates { get; } = [];

    [RelayCommand]
    private async Task SearchAsync()
    {
        if (string.IsNullOrWhiteSpace(SearchReleaseTitle))
        {
            ErrorMessage = "Enter a release title to search.";
            return;
        }

        IsBusy = true;
        ErrorMessage = null;
        ClearPlan();
        SearchResults.Clear();

        var results = await session.SearchAlbumsAsync(SearchArtistName, SearchReleaseTitle);

        foreach (var result in results)
        {
            SearchResults.Add(result);
        }

        IsBusy = false;
    }

    [RelayCommand]
    private async Task SelectSearchResultAsync(MusicBrainzReleaseSearchResult result)
    {
        if (result.Id is not { } id)
        {
            return;
        }

        await PreviewAsync(new ReleaseMbid(id));
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

        await PreviewAsync(new ReleaseMbid(id));
    }

    [RelayCommand]
    private async Task SelectDiscogsMasterAsync(DiscogsSearchResult candidate)
    {
        if (Plan is null || candidate.Id is not { } id)
        {
            return;
        }

        IsBusy = true;
        var updated = await session.ResolveDiscogsMasterAsync(Plan, new DiscogsMasterId(id));
        IsBusy = false;

        ApplyPlan(updated);
    }

    [RelayCommand]
    private void Commit()
    {
        if (Plan is null)
        {
            return;
        }

        if (DiscogsCandidates.Count > 0)
        {
            CommitErrorMessage = "Choose a Discogs match to resolve genres before adding.";
            return;
        }

        if (!int.TryParse(UnitsInStockInput, out var unitsInStock))
        {
            unitsInStock = 0;
        }

        if (!TryParsePrice(PriceInput, out var priceInPence))
        {
            CommitErrorMessage = "Enter a valid price.";
            return;
        }

        int? originalPriceInPence = null;
        if (!string.IsNullOrWhiteSpace(OriginalPriceInput))
        {
            if (!TryParsePrice(OriginalPriceInput, out var parsedOriginal))
            {
                CommitErrorMessage = "Enter a valid original price, or leave it blank.";
                return;
            }
            originalPriceInPence = parsedOriginal;
        }

        var commerce = new AlbumCommerceDetails(
            UnitsInStock: unitsInStock,
            PriceInPence: priceInPence,
            OriginalPriceInPence: originalPriceInPence,
            IsNew: IsNew,
            IsStaffPick: IsStaffPick
        );

        session.CommitAddAlbum(Plan, commerce);
        navigationService.GoBack();
    }

    [RelayCommand]
    private void Cancel() => navigationService.GoBack();

    private async Task PreviewAsync(ReleaseMbid releaseId)
    {
        IsBusy = true;
        ErrorMessage = null;

        var result = await session.PreviewAddAlbumAsync(releaseId);

        IsBusy = false;

        if (result.IsError)
        {
            ErrorMessage = string.Join("; ", result.Errors.Select(e => e.Description));
            ClearPlan();
            return;
        }

        ApplyPlan(result.Value);
    }

    private void ApplyPlan(AddAlbumPlan plan)
    {
        CommitErrorMessage = null;
        Plan = plan;
        PlanTitle = plan.Title;
        PlanReleaseDate = BindingHelpers.FormatDate(plan.ReleaseDate);
        PlanLabel = BindingHelpers.OrPlaceholder(plan.Label, "Unknown");
        PlanDescription = BindingHelpers.OrPlaceholder(
            plan.Description,
            "No description available."
        );
        PlanArtistCredits = FormatArtistCredits(plan);
        PlanGenres =
            plan.ResolvedGenres.Count > 0
                ? string.Join(", ", plan.ResolvedGenres.Select(g => g.Name))
                : "Unknown";
        PlanNewArtists =
            plan.MissingArtistPlans.Count > 0
                ? $"New artists that will be created: {string.Join(", ", plan.MissingArtistPlans.Select(p => p.Name))}"
                : string.Empty;

        DiscogsCandidates.Clear();
        if (plan.DiscogsCandidates is { Count: > 0 } candidates)
        {
            foreach (var candidate in candidates)
            {
                DiscogsCandidates.Add(candidate);
            }
        }
    }

    private string FormatArtistCredits(AddAlbumPlan plan)
    {
        var names = plan.ArtistCreditIds.Select(id =>
            session.Artists.FirstOrDefault(a => a.SourceId == id)?.Name
            ?? plan.MissingArtistPlans.FirstOrDefault(p => p.SourceId == id)?.Name
            ?? "Unknown artist"
        );
        return string.Join(", ", names);
    }

    private static bool TryParsePrice(string? input, out int pence)
    {
        pence = 0;
        if (!decimal.TryParse(input, out var pounds) || pounds < 0)
        {
            return false;
        }

        pence = (int)Math.Round(pounds * 100m);
        return true;
    }

    private void ClearPlan()
    {
        Plan = null;
        CommitErrorMessage = null;
        PlanTitle = string.Empty;
        PlanReleaseDate = string.Empty;
        PlanLabel = string.Empty;
        PlanDescription = string.Empty;
        PlanArtistCredits = string.Empty;
        PlanGenres = string.Empty;
        PlanNewArtists = string.Empty;
        DiscogsCandidates.Clear();
    }
}
