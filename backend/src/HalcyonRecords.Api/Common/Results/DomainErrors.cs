using ErrorOr;

namespace HalcyonRecords.Api.Common.Results;

public static class DomainErrors
{
    public static class Album
    {
        public static Error NotFound(string detail) =>
            Error.NotFound(code: "Album.NotFound", description: detail);

        public static Error MissingSourceId() =>
            Error.Validation(
                code: "Album.MissingSourceId",
                description: "MusicBrainz release has no id."
            );

        public static Error MissingTitle() =>
            Error.Validation(
                code: "Album.MissingTitle",
                description: "MusicBrainz release has no title."
            );

        public static Error AlreadySeeded(Guid sourceId) =>
            Error.Conflict(
                code: "Album.AlreadySeeded",
                description: $"Album '{sourceId}' already exists in the seed data (merge mode)."
            );
    }

    public static class Artist
    {
        public static Error NotFound(string detail) =>
            Error.NotFound(code: "Artist.NotFound", description: detail);

        public static Error MissingSourceId() =>
            Error.Validation(
                code: "Artist.MissingSourceId",
                description: "MusicBrainz artist has no id."
            );

        public static Error MissingName() =>
            Error.Validation(
                code: "Artist.MissingName",
                description: "MusicBrainz artist has no name."
            );

        public static Error AlreadySeeded(Guid sourceId) =>
            Error.Conflict(
                code: "Artist.AlreadySeeded",
                description: $"Artist '{sourceId}' already exists in the seed data (merge mode)."
            );
    }

    public static class Genre
    {
        public static Error NotFound(string detail) =>
            Error.NotFound(code: "Genre.NotFound", description: detail);

        public static Error EmptySlug(string name) =>
            Error.Validation(
                code: "Genre.EmptySlug",
                description: $"'{name}' produced an empty slug."
            );
    }
}
