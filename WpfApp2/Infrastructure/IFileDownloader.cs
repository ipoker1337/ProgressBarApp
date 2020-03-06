using System;
using System.Threading;
using System.Threading.Tasks;

namespace ProgressBarDemo.Domain.Interfaces
{

public interface 
IProgressProvider {
    event EventHandler<DownloadProgress>? ProgressChanged;
}

public interface 
IFileDownloader {

    Task<ProgressResult> 
    DownloadFileAsync(Uri address, string fileName, CancellationToken cancellationToken);

    Task<ProgressResult>
    DownloadFileAsync(Uri address, string fileName);

    ProgressResult 
    DownloadFile(Uri address, string fileName);
}
}
