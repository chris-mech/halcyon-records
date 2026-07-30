using System.Net;

namespace HalcyonRecords.SeedDataGenerator.Core.Common;

public static class HttpClientExtensions
{
    extension(HttpClient httpClient)
    {
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
    }
}
