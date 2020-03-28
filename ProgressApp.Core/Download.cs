﻿using System;
using System.Threading;
using System.Net.Http;
using System.Threading.Tasks;
using System.Linq;

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
TestFileDownloader : ProgressProvider, IFileDownloader {
    
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

// under development
public class
FileDownloader : ProgressProvider, IFileDownloader {
    private static readonly HttpClient _httpClient = new HttpClient();

    public async Task<DownloadResult>
    DownloadFileAsync(Uri address, string fileName, CancellationToken cancellationToken)  {
        Report("Connecting...");
        var response = await _httpClient.GetAsync(address, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
        if (!response.IsSuccessStatusCode)
            throw new Exception($"The request returned with HTTP status code {response.StatusCode}");
        Report(0, response.Content.Headers.ContentLength, "Downloading...");

        using (var stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false)) {
            var position = 0L;
            var buffer = new byte[4096];
            var isMoreToRead = true;

            while (isMoreToRead && !cancellationToken.IsCancellationRequested) {
                var deltaValue = await stream.ReadAsync(buffer, 0, buffer.Length).ConfigureAwait(false);
                if (deltaValue == 0) isMoreToRead = false;
                else {
                    var data = new byte[deltaValue];
                    buffer.ToList().CopyTo(0, data, 0, deltaValue);
                    // write to disk
                    position += deltaValue;
                    Report(deltaValue);
                }
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


    public Task<DownloadResult> DownloadFileAsync(Uri address, string fileName) =>
    throw new NotImplementedException();

    public DownloadResult DownloadFile(Uri address, string fileName, CancellationToken cancellationToken) =>
    throw new NotImplementedException();
}
//

}


