using System;
using System.IO;
using System.Threading;
using System.Net.Http;
using System.Threading.Tasks;
using System.Linq;
using ProgressApp.Core.Common;

namespace ProgressApp.Core {

public class 
DownloadResult {
    public bool IsSuccess { get; }
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
    Error(long totalBytesReceived, Exception? exception) => new DownloadResult(totalBytesReceived, isSuccess: false, isCanceled: false, exception);

    public static DownloadResult
    Cancel(long totalBytesReceived) => new DownloadResult(totalBytesReceived, isSuccess: false, isCanceled: true);
}

// under development
public static class
Download {
    private static readonly HttpClient HttpClient = new HttpClient();

    public static async Task<DownloadResult>
    FileAsync(Uri requestUri, Stream stream, CancellationToken cancellationToken, IProgressHandler? progress) {
        var position = 0L;
        if (!stream.CanWrite)
            throw new ArgumentException("stream doesn't support write operations");

        try {
            progress?.Report("Connecting...");
            var response = await HttpClient.GetAsync(requestUri, HttpCompletionOption.ResponseHeadersRead,
                                                     cancellationToken).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
                throw new Exception($"The request returned with HTTP status code {response.StatusCode}");
            progress?.Report(position, response.Content.Headers.ContentLength, "Downloading...");

            await using var responseStream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
            var buffer = new byte[4096];
            var moreToRead = true;

            while (moreToRead) {
                cancellationToken.ThrowIfCancellationRequested();
                var deltaValue = await responseStream.ReadAsync(buffer, 0, buffer.Length, cancellationToken).ConfigureAwait(false);
                if (deltaValue == 0)
                    moreToRead = false;
                else {
                    var data = new byte[deltaValue];
                    buffer.ToList().CopyTo(0, data, 0, deltaValue);
                    // write to disk
                    position += deltaValue;
                    progress?.Report(deltaValue);
                }
            }
        }
        catch (OperationCanceledException) {
            progress?.Report("Canceled");
            Task.Delay(2000).Wait();
            progress?.Reset();
            return DownloadResult.Cancel(position);
        }
        catch (Exception ex) { 
            progress?.Report(0, 0, "Error");
            return DownloadResult.Error(position, ex);
        }

        progress?.Report("Finishing...");
        Task.Delay(2000).Wait();
        progress?.Reset();
        return DownloadResult.Success(position);
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
            return DownloadResult.Cancel(position);
        }

        Report("Finishing...");
        Task.Delay(2000).Wait();
        Report(0, 0, "Finished");

        return DownloadResult.Success(position);
    }
}

}


