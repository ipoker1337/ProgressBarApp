using System;
using System.IO;
using System.Threading;
using System.Net.Http;
using System.Threading.Tasks;
using System.Net.Http.Headers;
using ProgressApp.Core.Common;

namespace ProgressApp.Core {

public class
Result {
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public long BytesReceived { get; }

    protected Result(bool isSuccess, long bytesReceived = 0) {
        IsSuccess = isSuccess;
        BytesReceived = bytesReceived;
    }

    public static Result
    Success() => new Result(true);

    public static Result
    Failure(long bytesReceived) => new Result(false, bytesReceived);
}

public static class
Download {
    private static readonly HttpClient HttpClient = new HttpClient();

    public static async Task<Result>
    FileAsync(Uri requestUri, Stream stream, CancellationToken cancellationToken, IProgressObserver progress, long initialBytePosition = 0) {
        var position = initialBytePosition.VerifyNonNegative();
        if (!stream.CanWrite)
            throw new ArgumentException("stream doesn't support write operations");

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
            if (cancellationToken.IsCancellationRequested)
                return Result.Failure(position);
            var deltaValue = await responseStream.ReadAsync(buffer, 0, buffer.Length).ConfigureAwait(false);
            if (deltaValue == 0)
                moreToRead = false;
            else {
                await stream.WriteAsync(buffer, 0, deltaValue).ConfigureAwait(false);
                position += deltaValue;
                progress.OnProgress(deltaValue);
            }
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
