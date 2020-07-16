using System;
using System.IO;
using System.Threading;
using System.Net.Http;
using System.Threading.Tasks;
using System.Net.Http.Headers;
using ProgressApp.Core.Common;

namespace ProgressApp.Core {
public class
DownloadResult {
    public bool IsSuccess { get; }
    public long BytesReceived { get; }

    private DownloadResult(bool isSuccess, long bytesReceived = 0) {
        IsSuccess = isSuccess;
        BytesReceived = bytesReceived;
    }

    public static readonly DownloadResult 
    Success = new DownloadResult(true);

    public static DownloadResult 
    Failure(long bytesReceived) => new DownloadResult(false, bytesReceived);
}

public static class
Download {
    private static readonly HttpClient HttpClient = new HttpClient();
    private const int BufferSize = 4096;

    public static async Task<DownloadResult>
    FileAsync(Uri requestUri, Stream stream, CancellationToken cancellationToken, IProgressObserver observer, long initialBytePosition = 0) {
        var position = initialBytePosition.VerifyNonNegative();
        if (!stream.CanWrite)
            throw new ArgumentException("stream doesn't support write operations");

        observer.OnProgress("Connecting...");

        var contentLength = await GetContentLengthAsync(requestUri, cancellationToken).ConfigureAwait(false);
        var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
        request.Headers.Range = new RangeHeaderValue(position, contentLength);

        var response = await HttpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
        if (!response.IsSuccessStatusCode)
            throw new HttpRequestException($"The request returned {response.StatusCode}");

        observer.OnProgress(position, contentLength, "Downloading...");

        await using var responseStream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
        var buffer = new byte[BufferSize];
        var moreToRead = true;

        while (moreToRead) {
            if (cancellationToken.IsCancellationRequested)
                return DownloadResult.Failure(position);
            var deltaValue = await responseStream.ReadAsync(buffer, 0, buffer.Length).ConfigureAwait(false);
            if (deltaValue == 0)
                moreToRead = false;
            else {
                await stream.WriteAsync(buffer, 0, deltaValue).ConfigureAwait(false);
                position += deltaValue;
                observer.OnProgress(deltaValue);
            }
        }
        return DownloadResult.Success;
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
