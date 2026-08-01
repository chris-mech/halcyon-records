using System.Text;

namespace HalcyonRecords.Shared;

public static class Slugifier
{
    public static string Slugify(string value)
    {
        var withAnd = value.Replace("&", " and ");

        var slug = new StringBuilder(withAnd.Length);
        var previousWasHyphen = true;

        foreach (var c in withAnd)
        {
            if (char.IsLetterOrDigit(c))
            {
                slug.Append(char.ToLowerInvariant(c));
                previousWasHyphen = false;
            }
            else if (!previousWasHyphen)
            {
                slug.Append('-');
                previousWasHyphen = true;
            }
        }

        return slug.ToString().Trim('-');
    }
}
