using ErrorOr;

namespace HalcyonRecords.Api.Common.Results;

public static class DomainErrors
{
    public static class Album
    {
        public static Error NotFound(string detail) =>
            Error.NotFound(code: "Album.NotFound", description: detail);
    }

    public static class Artist
    {
        public static Error NotFound(string detail) =>
            Error.NotFound(code: "Artist.NotFound", description: detail);
    }

    public static class Genre
    {
        public static Error NotFound(string detail) =>
            Error.NotFound(code: "Genre.NotFound", description: detail);
    }
}