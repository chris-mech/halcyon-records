using System.Reflection;

namespace HalcyonRecords.SeedDataGenerator.Core.Common;

public static class SeedToolUserAgent
{
    private static readonly string s_appVersion = Assembly.GetExecutingAssembly().GetName().Version
        is { } version
        ? $"{version.Major}.{version.Minor}"
        : "0.0";

    public static string For(string contactEmail) =>
        $"HalcyonRecordsSeedTool/{s_appVersion} ({contactEmail})";
}
