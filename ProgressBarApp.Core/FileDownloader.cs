using System;
using System.Threading;
using System.Diagnostics;
using System.Threading.Tasks;
using ProgressReporter = System.Progress<ProgressBarApp.Core.Progress>;

namespace ProgressBarApp.Core {

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
TestFileDownloader : ProgressReporter, IFileDownloader, IProgressProvider {
    
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

        var progressInfo = Progress.Create("Connecting...");
        OnReport(progressInfo);

        Random random = new Random();
        long position = 0;
        long totalBytesToDownload = 1 + random.Next(30) * 1_000_000;
        int averageSpeedBytesPerSec = random.Next(3_000_000);

        Task.Delay(2000).Wait();

        progressInfo = Progress.Create("Connected");
        OnReport(progressInfo);

        while (position < totalBytesToDownload && !cancellationToken.IsCancellationRequested) {
            //Simulate network delay
            Task.Delay(random.Next(1000 / 20)).Wait();
            var value = Math.Min(totalBytesToDownload - position, random.Next(averageSpeedBytesPerSec / 20));
            position += value;

            progressInfo = Progress.Create(position, totalBytesToDownload, value, TimeSpan.FromMilliseconds(timer.ElapsedMilliseconds), "Downloading...");
            OnReport(progressInfo);
        }

        if (!cancellationToken.IsCancellationRequested) {
            progressInfo = progressInfo.WithMessage("Finishing...");
            OnReport(progressInfo);

            Task.Delay(2000).Wait();

            progressInfo = progressInfo.WithMessage("Finished");
            OnReport(progressInfo);
        }
        else {
            // Paused(reporter);
            //data = data.WithMessage("Paused.");
            //reporter?.Report(data);
        }

        return new DownloadResult();
    }
}
}
