using ErrorOr;

namespace HalcyonRecords.Shared;

public static class DomainErrors
{
    public static class Album
    {
        public static Error CatalogueEmpty() =>
            Error.NotFound(code: "Album.CatalogueEmpty", description: "No albums available.");

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

        public static Error AlreadySeeded(ReleaseMbid sourceId) =>
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

        public static Error AlreadySeeded(ArtistMbid sourceId) =>
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

    public static class Decade
    {
        public static Error NotFound(string detail) =>
            Error.NotFound(code: "Decade.NotFound", description: detail);
    }

    public static class Auth
    {
        public static Error EmailAlreadyRegistered(string email) =>
            Error.Conflict(
                code: "Auth.EmailAlreadyRegistered",
                description: $"An account with email '{email}' already exists."
            );

        public static Error InvalidCredentials() =>
            Error.Unauthorized(
                code: "Auth.InvalidCredentials",
                description: "The email or password is incorrect."
            );

        public static Error InvalidRefreshToken() =>
            Error.Unauthorized(
                code: "Auth.InvalidRefreshToken",
                description: "The refresh token is invalid, expired, or has already been used."
            );

        public static Error UserNotFound() =>
            Error.NotFound(
                code: "Auth.UserNotFound",
                description: "The authenticated user no longer exists."
            );
    }

    public static class Order
    {
        public static Error NotFound(string detail) =>
            Error.NotFound(code: "Order.NotFound", description: detail);

        public static Error CartEmpty() =>
            Error.Validation(
                code: "Order.CartEmpty",
                description: "Your bag is empty. Add something before checking out."
            );

        public static Error InsufficientStock(string title) =>
            Error.Conflict(
                code: "Order.InsufficientStock",
                description: $"Sorry, '{title}' just sold out."
            );
    }
}
