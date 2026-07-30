using System.Reflection;

namespace HalcyonRecords.SeedDataGenerator.Core.Common;

public static class SeedToolUserAgent
{
    private static readonly string AppVersion = Assembly.GetExecutingAssembly().GetName().Version
        is { } version
        ? $"{version.Major}.{version.Minor}"
        : "0.0";

    public static string For(string contactEmail) =>
        $"HalcyonRecordsSeedTool/{AppVersion} ({contactEmail})";
}
