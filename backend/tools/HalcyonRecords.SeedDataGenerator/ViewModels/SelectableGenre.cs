using CommunityToolkit.Mvvm.ComponentModel;
using HalcyonRecords.Shared;

namespace HalcyonRecords.SeedDataGenerator.ViewModels;

public sealed partial class SelectableGenre(GenreSlug slug, string name, bool isSelected)
    : ObservableObject
{
    public GenreSlug Slug { get; } = slug;
    public string Name { get; } = name;

    [ObservableProperty]
    public partial bool IsSelected { get; set; } = isSelected;
}
