using System.Text;
using FluentAssertions;
using HalcyonRecords.Shared;

namespace HalcyonRecords.Api.UnitTests.Common;

public class SlugifierTests
{
    [Theory]
    [InlineData("Full Detail Album", "full-detail-album")]
    [InlineData("Rock & Roll", "rock-and-roll")]
    [InlineData("HELLO World", "hello-world")]
    [InlineData("Album   100", "album-100")]
    [InlineData("  Leading And Trailing Spaces  ", "leading-and-trailing-spaces")]
    [InlineData("Hyphen---Already--Here", "hyphen-already-here")]
    [InlineData("Déjà Vu", "déjà-vu")]
    [InlineData("你好 世界", "你好-世界")]
    [InlineData("こんにちは 世界", "こんにちは-世界")]
    [InlineData("コーヒー", "コーヒー")]
    [InlineData("안녕하세요 세계", "안녕하세요-세계")]
    [InlineData("Straße", "straße")]
    [InlineData("Øre", "øre")]
    [InlineData("ÆRLIG", "ærlig")]
    [InlineData("Suður", "suður")]
    [InlineData("Þing", "þing")]
    [InlineData("Coffee ☕ Break", "coffee-break")]
    [InlineData("🔥🔥🔥", "")]
    [InlineData("!!!", "")]
    [InlineData("", "")]
    [InlineData("&", "and")]
    [InlineData("Foo&Bar", "foo-and-bar")]
    [InlineData("Track_01", "track-01")]
    public void Slugify_ProducesExpectedSlug(string input, string expected) =>
        Slugifier.Slugify(input).Should().Be(expected);

    [Fact]
    public void Slugify_ProducesIdenticalSlugRegardlessOfUnicodeNormalizationForm()
    {
        var nfc = "Déjà Vu".Normalize(NormalizationForm.FormC);
        var nfd = "Déjà Vu".Normalize(NormalizationForm.FormD);

        Slugifier.Slugify(nfc).Should().Be("déjà-vu");
        Slugifier.Slugify(nfd).Should().Be("déjà-vu");
    }
}
