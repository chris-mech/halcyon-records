using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace HalcyonRecords.SeedDataGenerator.Core.Common;

public static class HttpClientExtensions
{
    extension(HttpClient httpClient)
    {
        /// <summary>
        /// Sends a GET request to <paramref name="requestUri"/> and returns the response.
        /// Returns <see langword="null"/> if the server responds 404 Not Found; any other
        /// non-success status code throws via <see cref="HttpResponseMessage.EnsureSuccessStatusCode"/>.
        /// </summary>
        public async Task<HttpResponseMessage?> GetOrNullAsync(
            string requestUri,
            CancellationToken cancellationToken = default
        )
        {
            var response = await httpClient.GetAsync(requestUri, cancellationToken);

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }

            response.EnsureSuccessStatusCode();
            return response;
        }

        /// <summary>
        /// Sends a GET request and deserializes the JSON response as <typeparamref name="T"/>,
        /// or returns <see langword="null"/> if the server responds 404 Not Found (see
        /// <see cref="GetOrNullAsync"/>).
        /// </summary>
        public async Task<T?> GetFromJsonOrNullAsync<T>(
            string requestUri,
            JsonSerializerOptions jsonOptions,
            CancellationToken cancellationToken = default
        )
            where T : class
        {
            using var response = await httpClient.GetOrNullAsync(requestUri, cancellationToken);

            return response is null
                ? null
                : await response.Content.ReadFromJsonAsync<T>(jsonOptions, cancellationToken);
        }
    }
}
