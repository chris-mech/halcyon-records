using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HalcyonRecords.SeedDataGenerator.Core.Session;
using HalcyonRecords.SeedDataGenerator.Navigation;
using HalcyonRecords.Shared;

namespace HalcyonRecords.SeedDataGenerator.ViewModels;

public sealed partial class AlbumEditViewModel(
    SeedDataSession session,
    NavigationService navigationService
) : ObservableObject
{
    private ReleaseMbid _albumId;

    [ObservableProperty]
    public partial string Title { get; set; } = string.Empty;

    [ObservableProperty]
    public partial DateTimeOffset? ReleaseDate { get; set; }

    [ObservableProperty]
    public partial string? Label { get; set; }

    [ObservableProperty]
    public partial string? Description { get; set; }

    [ObservableProperty]
    public partial string? ImageUrl { get; set; }

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

    [ObservableProperty]
    public partial string? ErrorMessage { get; set; }

    [ObservableProperty]
    public partial bool IsBusy { get; set; }

    public void Initialize(AlbumSeedEntry album)
    {
        _albumId = album.SourceId;
        PopulateOnlineFields(
            album.Title,
            album.ReleaseDate,
            album.Label,
            album.Description,
            album.ImageUrl
        );

        UnitsInStockInput = album.UnitsInStock.ToString();
        PriceInput = (album.PriceInPence / 100m).ToString("0.00");
        OriginalPriceInput = album.OriginalPriceInPence is { } original
            ? (original / 100m).ToString("0.00")
            : null;
        IsNew = album.IsNew;
        IsStaffPick = album.IsStaffPick;
    }

    [RelayCommand]
    private async Task RefreshAsync()
    {
        IsBusy = true;
        ErrorMessage = null;

        var result = await session.RefreshAlbumAsync(_albumId);

        IsBusy = false;

        if (result.IsError)
        {
            ErrorMessage = string.Join("; ", result.Errors.Select(e => e.Description));
            return;
        }

        var plan = result.Value;
        PopulateOnlineFields(
            plan.Title,
            plan.ReleaseDate,
            plan.Label,
            plan.Description,
            plan.CoverImageUrl?.ToString()
        );
    }

    [RelayCommand]
    private void Save()
    {
        if (string.IsNullOrWhiteSpace(Title))
        {
            ErrorMessage = "Title is required.";
            return;
        }

        if (!int.TryParse(UnitsInStockInput, out var unitsInStock))
        {
            unitsInStock = 0;
        }

        if (!TryParsePrice(PriceInput, out var priceInPence))
        {
            ErrorMessage = "Enter a valid price.";
            return;
        }

        int? originalPriceInPence = null;
        if (!string.IsNullOrWhiteSpace(OriginalPriceInput))
        {
            if (!TryParsePrice(OriginalPriceInput, out var parsedOriginal))
            {
                ErrorMessage = "Enter a valid original price, or leave it blank.";
                return;
            }
            originalPriceInPence = parsedOriginal;
        }

        session.UpdateAlbum(
            _albumId,
            Title,
            ReleaseDate is { } date ? DateOnly.FromDateTime(date.Date) : null,
            Label,
            Description,
            ImageUrl,
            new AlbumCommerceDetails(
                UnitsInStock: unitsInStock,
                PriceInPence: priceInPence,
                OriginalPriceInPence: originalPriceInPence,
                IsNew: IsNew,
                IsStaffPick: IsStaffPick
            )
        );
        navigationService.GoBack();
    }

    [RelayCommand]
    private void ClearReleaseDate() => ReleaseDate = null;

    [RelayCommand]
    private void Cancel() => navigationService.GoBack();

    private void PopulateOnlineFields(
        string title,
        DateOnly? releaseDate,
        string? label,
        string? description,
        string? imageUrl
    )
    {
        Title = title;
        ReleaseDate = releaseDate is { } date
            ? new DateTimeOffset(date.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero)
            : null;
        Label = label;
        Description = description;
        ImageUrl = imageUrl;
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
}
