using CommunityToolkit.Mvvm.ComponentModel;
using HalcyonRecords.Shared;

namespace HalcyonRecords.SeedDataGenerator.ViewModels;

public sealed partial class SelectableArtist(ArtistMbid id, string name, bool isSelected)
    : ObservableObject
{
    public ArtistMbid Id { get; } = id;
    public string Name { get; } = name;

    [ObservableProperty]
    public partial bool IsSelected { get; set; } = isSelected;
}
