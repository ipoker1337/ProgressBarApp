using System;
using System.Threading;
using System.Diagnostics;
using System.Threading.Tasks;

namespace ProgressApp.Core {

public interface 
IFileDownloader {
    Task<DownloadResult> 
    DownloadFileAsync(Uri address, string fileName, CancellationToken cancellationToken);

    Task<DownloadResult> 
    DownloadFileAsync(Uri address, string fileName);

    DownloadResult
    DownloadFile(Uri address, string fileName, CancellationToken cancellationToken);
}

public readonly struct 
DownloadResult {
}

public class
TestFileDownloader : ProgressProvider, IFileDownloader {
    
    public async Task<DownloadResult>
    DownloadFileAsync(Uri address, string fileName) => 
        await Task.Run(() => DownloadFile(address, fileName, CancellationToken.None)).ConfigureAwait(false);

    public async Task<DownloadResult>
    DownloadFileAsync(Uri address, string fileName, CancellationToken cancellationToken) => 
        await Task.Run(() => DownloadFile(address, fileName, cancellationToken)).ConfigureAwait(false);

    public DownloadResult
    DownloadFile(Uri address, string fileName, CancellationToken cancellationToken) {
        var timer = new Stopwatch();
        timer.Start();

        Report("Connecting...");

        Random random = new Random();
        long position = 0;
        long totalBytesToDownload = 1 + random.Next(30) * 1_000_000;
        int averageSpeedBytesPerSec = random.Next(3_000_000);

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

        if (!cancellationToken.IsCancellationRequested) {
            Report("Finishing...");
            Task.Delay(2000).Wait();
            Report(0, 100, "Finished");
        }
        else {
            Report("Please wait...");
            Task.Delay(2500).Wait();
            Report(0, 100, "Canceled");
            return new DownloadResult();
        }

        return new DownloadResult();
    }
}
}
