using System;
using System.Threading;
using System.Diagnostics;
using System.Threading.Tasks;

namespace ProgressApp.Core {

public interface 
IFileDownloader {
    Task<DownloadResult> DownloadFileAsync(Uri address, string fileName, CancellationToken cancellationToken);
    Task<DownloadResult> DownloadFileAsync(Uri address, string fileName);
    DownloadResult DownloadFile(Uri address, string fileName, CancellationToken cancellationToken);
}

public readonly struct 
DownloadResult {
}

public class
TestFileDownloader : ProgressReporter, IFileDownloader {
    
    public async Task<DownloadResult>
    DownloadFileAsync(Uri address, string fileName) => 
        await Task.Run(() => DownloadFile(address, fileName, CancellationToken.None)).ConfigureAwait(false);

    public async Task<DownloadResult>
    DownloadFileAsync(Uri address, string fileName, CancellationToken cancellationToken) => 
        await Task.Run(() => DownloadFile(address, fileName, cancellationToken)).ConfigureAwait(false);

    public DownloadResult
    DownloadFile(Uri address, string fileName, CancellationToken cancellationToken) {
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
            Report(0, 0, "Finished");
        }
        else {
            Report("Canceled");
            Task.Delay(2000).Wait();
            Report(0, 0);
            return new DownloadResult();
        }

        return new DownloadResult();
    }
}

public readonly struct 
ImportResult {
}

public static class 
FileImporter {

    public static async Task<ImportResult>
    ImportAsync(IProgressReporter progress, CancellationToken cancellationToken) =>
        await Task.Run( () => Import(progress, cancellationToken)).ConfigureAwait(false);

    public static ImportResult 
    Import(IProgressReporter progress, CancellationToken cancellationToken) {
        progress.Report("Initializing...");

        Random random = new Random();
        long position = 0;
        long totalFilesToImport = 1 + random.Next(30) * 1_000;
        int averageSpeedPerSec = random.Next(250);

        Task.Delay(2000).Wait();

        progress.Report(0, totalFilesToImport, "Importing files...");
        while (position < totalFilesToImport && !cancellationToken.IsCancellationRequested) {

            Task.Delay(random.Next(1000 / 20)).Wait();
            var deltaValue = Math.Min(totalFilesToImport - position, random.Next(averageSpeedPerSec / 20));
            position += deltaValue;
            progress.Report(deltaValue);
        }

         if (!cancellationToken.IsCancellationRequested) {
            progress.Report("Finishing...");
            Task.Delay(2000).Wait();
            progress.Report(0, 0, "Finished");
        }
        else {
            progress.Report("Canceled");
            Task.Delay(2000).Wait();
            progress.Report(0, 0, "");
            return new ImportResult();
        }

         return new ImportResult();
    }

}

}
