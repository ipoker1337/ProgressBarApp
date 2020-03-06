using System;
using System.Threading;
using System.Diagnostics;
using System.Threading.Tasks;

namespace ProgressBarApp.Core {

public interface 
IFileDownloader {

    Task<ProgressResult> 
    DownloadFileAsync(Uri address, string fileName, CancellationToken cancellationToken);

    Task<ProgressResult> 
    DownloadFileAsync(Uri address, string fileName);

    ProgressResult
    DownloadFile(Uri address, string fileName, CancellationToken cancellationToken);
}

public class
ProgressResult {
}

public class
TestFileDownloader : Progress<ProgressInfo>, IFileDownloader, IProgressProvider {
    
    public async Task<ProgressResult>
    DownloadFileAsync(Uri address, string fileName) 
        => await Task.Run(() => DownloadFile(address, fileName, CancellationToken.None)).ConfigureAwait(false);

    public async Task<ProgressResult>
    DownloadFileAsync(Uri address, string fileName, CancellationToken cancellationToken) 
        => await Task.Run(() => DownloadFile(address, fileName, cancellationToken)).ConfigureAwait(false);

    public ProgressResult
    DownloadFile(Uri address, string fileName, CancellationToken cancellationToken) {
        var timer = new Stopwatch();
        timer.Start();

        var progressInfo = ProgressInfo.Connecting();
        OnReport(progressInfo);

        Random random = new Random();
        long position = 0;
        long totalBytesToDownload = 1 + random.Next(30) * 1_000_000;
        int averageSpeedBytesPerSec = random.Next(3_000_000);

        Task.Delay(2000).Wait();

        progressInfo = ProgressInfo.Connected(position, totalBytesToDownload);
        OnReport(progressInfo);

        while (position < totalBytesToDownload && !cancellationToken.IsCancellationRequested) {
            //Simulate network delay
            var delay = random.Next(1000 / 20);
            Task.Delay(delay).Wait();
            var value = Math.Min(totalBytesToDownload - position, random.Next(averageSpeedBytesPerSec / 20));
            position += value;

            double transferSpeed = 0;
            if (timer.ElapsedMilliseconds > 0)
                transferSpeed = value * 1000.0 / timer.Elapsed.Milliseconds;
            progressInfo = progressInfo.Downloading(value,
                                                            totalBytesToDownload,
                                                            position,
                                                            transferSpeed);
            OnReport(progressInfo);
        }

        if (!cancellationToken.IsCancellationRequested) {
            progressInfo = ProgressInfo.Finishing();
            OnReport(progressInfo);

            Task.Delay(2000).Wait();

            progressInfo = ProgressInfo.Finished(totalBytesToDownload, position);
            OnReport(progressInfo);
        }
        else {
            // Paused(reporter);
            //data = data.WithMessage("Paused.");
            //reporter?.Report(data);
        }

        return new ProgressResult();
    }
}
}
