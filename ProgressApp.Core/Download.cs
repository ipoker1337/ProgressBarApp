using System;
using System.IO;
using System.Threading;
using System.Net.Http;
using System.Threading.Tasks;
using System.Net.Http.Headers;
using ProgressApp.Core.Common;

namespace ProgressApp.Core {

public static class
Download {
    private static readonly HttpClient HttpClient = new HttpClient();

    public static async Task<Result>
    FileAsync(Uri requestUri, Stream stream, CancellationToken cancellationToken, IProgressObserver progress, long firstBytePosition = 0) {
        var position = firstBytePosition.VerifyNonNegative();
        if (!stream.CanWrite)
            throw new ArgumentException("stream doesn't support write operations");

        try {
            progress.OnProgress("Connecting...");
            var contentLength = await GetContentLengthAsync(requestUri, cancellationToken).ConfigureAwait(false);
            
            var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            request.Headers.Range = new RangeHeaderValue(position, contentLength);
            var response = await HttpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
                throw new HttpRequestException($"The request returned {response.StatusCode}");

            progress.OnProgress(position, contentLength, "Downloading...");

            await using var responseStream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
            var buffer = new byte[4096];
            var moreToRead = true;

            while (moreToRead) {
                cancellationToken.ThrowIfCancellationRequested();
                var deltaValue = await responseStream.ReadAsync(buffer, 0, buffer.Length, cancellationToken).ConfigureAwait(false);
                if (deltaValue == 0)
                    moreToRead = false;
                else {
                    await stream.WriteAsync(buffer, 0, deltaValue).ConfigureAwait(false);
                    position += deltaValue;
                    progress.OnProgress(deltaValue);
                }
            }
        }
        catch (OperationCanceledException) {
            return Result.Failure(position);
        }
        catch (Exception ex) {
            return Result.Failure(position, ex);
        }

        return Result.Success();
    }

    private static async Task<long?> 
    GetContentLengthAsync(Uri requestUri, CancellationToken cancellationToken) {
        var response = await HttpClient.GetAsync(requestUri, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
        if (!response.IsSuccessStatusCode)
            throw new HttpRequestException($"The request returned {response.StatusCode}");
        return response.Content.Headers.ContentLength;
    }
}

}
