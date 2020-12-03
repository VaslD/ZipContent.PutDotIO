using System;
using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

using ZipContent.Core;

namespace ZipContent.PutDotIO
{
    public class PutIOPartialFileReader : IPartialFileReader
    {
        private readonly Uri fileUri;
        private readonly HttpClient client = new();

        public PutIOPartialFileReader(Uri url)
        {
            if (!url.Scheme.Equals("https", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("Only HTTPS protocol is supported.");
            }

            if (!url.Host.EndsWith("put.io", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("Only files on Put.io are supported.");
            }

            if (!url.LocalPath.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("Only ZIP files are supported.");
            }

            fileUri = url;
        }

        public async Task<long> ContentLength()
        {
            using var response = await client.GetAsync(fileUri, HttpCompletionOption.ResponseHeadersRead)
                                             .ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
            if (response.Content.Headers.ContentLength is { } length) return length;

            throw new InvalidOperationException("Server did not provide Content Length.");
        }

        public async Task<byte[]> GetBytes(ByteRange range)
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, fileUri);
            request.Headers.Range = new RangeHeaderValue(range.Start, range.End);

            using var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead)
                                             .ConfigureAwait(false);
            response.EnsureSuccessStatusCode();

            if (response.StatusCode != HttpStatusCode.PartialContent)
            {
                Console.WriteLine("[Warning] Server did not respond with Partial Content.");
            }

            // Verify Content-Range header against requested range.
            var responseRange = response.Content.Headers.ContentRange;
            if (responseRange == null)
            {
                Console.WriteLine("[Warning] Server did not provide Content Range.");
            }
            else
            {
                if (!responseRange.Unit.Equals("bytes", StringComparison.OrdinalIgnoreCase))
                {
                    Console.WriteLine("[Warning] Content Range was not in bytes. Length verification skipped.");
                }
                else
                {
                    if (responseRange.From != range.Start ||
                        responseRange.To != range.End)
                    {
                        var message = string.Format(CultureInfo.InvariantCulture,
                                                    "[Warning] Requested range {0}~{1} but got {2}~{3}.",
                                                    range.Start,
                                                    range.End,
                                                    responseRange.From,
                                                    responseRange.To);
                        Console.WriteLine(message);
                    }
                }
            }

            return await response.Content.ReadAsByteArrayAsync()
                                 .ConfigureAwait(false);
        }
    }
}
