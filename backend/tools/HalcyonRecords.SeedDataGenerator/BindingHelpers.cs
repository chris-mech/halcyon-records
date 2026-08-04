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

    public static string OrPlaceholder(string? value, string placeholder) =>
        string.IsNullOrWhiteSpace(value) ? placeholder : value;

    public static string FormatArtistType(ArtistType? type) => type?.ToString() ?? "Unknown";
}
