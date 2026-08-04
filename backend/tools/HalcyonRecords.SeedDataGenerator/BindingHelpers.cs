using HalcyonRecords.SeedDataGenerator.Core.MusicBrainz;
using HalcyonRecords.Shared;
using Microsoft.UI.Xaml;

namespace HalcyonRecords.SeedDataGenerator;

public static class BindingHelpers
{
    public static Visibility ToVisibility(bool value) =>
        value ? Visibility.Visible : Visibility.Collapsed;

    public static Visibility ToVisibility(object? value) =>
        value is null ? Visibility.Collapsed : Visibility.Visible;

    public static Visibility ToVisibilityFromCount(int count) =>
        count > 0 ? Visibility.Visible : Visibility.Collapsed;

    public static string FormatYear(int? year) => year?.ToString() ?? "Unknown";

    public static string FormatYear(DateOnly? date) => date?.Year.ToString() ?? "Unknown";

    public static string FormatDate(DateOnly? date) => date?.ToString("d MMMM yyyy") ?? "Unknown";

    public static string FormatList(IReadOnlyList<string>? items) =>
        items is { Count: > 0 } ? string.Join(", ", items) : "Unknown";

    public static string OrPlaceholder(string? value, string placeholder) =>
        string.IsNullOrWhiteSpace(value) ? placeholder : value;

    public static string FormatArtistType(ArtistType? type) => type?.ToString() ?? "Unknown";

    public static string FormatArtistCredit(IReadOnlyList<MusicBrainzArtistCredit>? credits) =>
        credits is { Count: > 0 }
            ? string.Join(", ", credits.Select(c => c.Name ?? c.Artist?.Name ?? "Unknown"))
            : "Unknown";

    public static string FormatPrice(int pence) => $"£{pence / 100m:0.00}";

    public static string FormatPrice(int? pence) =>
        pence is { } value ? FormatPrice(value) : string.Empty;
}
