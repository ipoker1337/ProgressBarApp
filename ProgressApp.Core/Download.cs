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
    public bool IsFailure => !IsSuccess;
    public bool IsCanceled { get; }
    public Exception? Exception { get; }
    public long TotalBytesReceived { get; }

    private DownloadResult(long totalBytesReceived, bool isSuccess, bool isCanceled, Exception? exception = null) { 
        TotalBytesReceived = totalBytesReceived.VerifyNonNegative();
        IsSuccess = isSuccess;
        IsCanceled = isCanceled;
        Exception = exception;
    }

    public static DownloadResult 
    Success(long totalBytesReceived) => new DownloadResult(totalBytesReceived, isSuccess: true, isCanceled: false);

    public static DownloadResult 
    Failure(long totalBytesReceived, Exception? exception = null) => new DownloadResult(totalBytesReceived, isSuccess: false, isCanceled: false, exception);
}

// under development
public static class
Download {
    private static readonly HttpClient HttpClient = new HttpClient();

    public static async Task<DownloadResult>
    FileAsync(Uri requestUri, Stream stream, CancellationToken cancellationToken, IProgressHandler? progress, long firstBytePosition = 0) {
        var position = firstBytePosition.VerifyNonNegative();
        if (!stream.CanWrite)
            throw new ArgumentException("stream doesn't support write operations");

        try {
            progress?.Report("Connecting...");
            var contentLength = await GetContentLengthAsync(requestUri, cancellationToken).ConfigureAwait(false);
            
            var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            request.Headers.Range = new RangeHeaderValue(position, contentLength);
            var response = await HttpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
                throw new Exception($"The request returned {response.StatusCode}");

            progress?.Report(position, contentLength, "Downloading...");

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
                    progress?.Report(deltaValue);
                }
            }
        }
        catch (OperationCanceledException) {
            progress?.Report("Canceled");
            return DownloadResult.Failure(position);
        }
        catch (Exception ex) {
            progress?.Report($"Error: {ex.Message}" );
            return DownloadResult.Failure(position, ex);
        }
        progress?.Report("Finished");
        return DownloadResult.Success(position);

        static async Task<long?> 
        GetContentLengthAsync(Uri requestUri, CancellationToken cancellationToken) {
            var response = await HttpClient.GetAsync(requestUri, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
                throw new Exception($"The request returned {response.StatusCode}");

            return response.Content.Headers.ContentLength;
        }
    }
}

public class
TestFileDownloader : ProgressHandler {
    
    public async Task<DownloadResult>
    DownloadFileAsync(Uri address, string fileName) => 
        await Task.Run(() => DownloadFile(address, fileName, CancellationToken.None)).ConfigureAwait(false);

    public async Task<DownloadResult>
    DownloadFileAsync(Uri address, string fileName, CancellationToken cancellationToken) => 
        await Task.Run(() => DownloadFile(address, fileName, cancellationToken)).ConfigureAwait(false);

    public DownloadResult
    DownloadFile(Uri address, string fileName, CancellationToken cancellationToken) {
        Random random = new Random();
        long position = 0;
        long totalBytesToDownload = 1 + random.Next(30) * 1_000_000;
        int averageSpeedBytesPerSec = random.Next(3_000_000);

        Report("Connecting...");
        Task.Delay(2000).Wait();
        Report("Connected");

        Task.Delay(random.Next(1000 / 20)).Wait();
        Report(0, totalBytesToDownload, "Downloading...");

        while (position < totalBytesToDownload && !cancellationToken.IsCancellationRequested) {
            //Simulate network delay
            Task.Delay(random.Next(1000 / 20)).Wait();
            var deltaValue = Math.Min(totalBytesToDownload - position, random.Next(averageSpeedBytesPerSec / 20));
            position += deltaValue;
            Report(deltaValue);
        }

        if (cancellationToken.IsCancellationRequested) {
            Report("Canceled");
            Task.Delay(2000).Wait();
            Report(0, 0);
            return DownloadResult.Failure(position);
        }

        Report("Finishing...");
        Task.Delay(2000).Wait();
        Report(0, 0, "Finished");

        return DownloadResult.Success(position);
    }
}

}

    //public static async Task<DownloadResult>
    //FileAsync(Uri requestUri, Stream stream, CancellationToken cancellationToken, IProgressHandler? progress) {
    //    var position = 0;
    //    if (!stream.CanWrite)
    //        throw new ArgumentException("stream doesn't support write operations");

    //    try {
    //        progress?.Report("Connecting...");

    //        HttpClient.DefaultRequestHeaders.Range = new RangeHeaderValue(position, 0);
    //        var response = await HttpClient.GetAsync(requestUri, HttpCompletionOption.ResponseHeadersRead,
    //                                                 cancellationToken).ConfigureAwait(false);

    //        if (!response.IsSuccessStatusCode)
    //            throw new Exception($"The request returned {response.StatusCode}");
    //        progress?.Report(position, response.Content.Headers.ContentLength, "Downloading...");

    //        await using var responseStream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);

    //        var buffer = new byte[4096];
    //        var moreToRead = true;

    //        while (moreToRead) {
    //            cancellationToken.ThrowIfCancellationRequested();
    //            var deltaValue = await responseStream.ReadAsync(buffer, 0, buffer.Length, cancellationToken).ConfigureAwait(false);
    //            if (deltaValue == 0)
    //                moreToRead = false;
    //            else {
    //                await stream.WriteAsync(buffer, 0, deltaValue, cancellationToken);
    //                position += deltaValue;
    //                progress?.Report(deltaValue);
    //            }
    //        }
    //    }
    //    catch (OperationCanceledException) {
    //        progress?.Report("Paused");
    //        Task.Delay(2000).Wait();
    //        return DownloadResult.Cancel(position);
    //    }
    //    catch (Exception ex) {
    //        return DownloadResult.Error(position, ex);
    //    }

    //    progress?.Report("Finishing...");
    //    Task.Delay(2000).Wait();
    //    progress?.Report("Finished");
    //    return DownloadResult.Success(position);
    //}


