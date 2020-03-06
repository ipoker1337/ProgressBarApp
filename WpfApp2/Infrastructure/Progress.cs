using ProgressBarDemo.Domain.Extensions;
using System;
using ProgressBarDemo.Domain.Interfaces;

namespace ProgressBarDemo.Domain
{

public enum DownloadStatus {

        NotStarted, StartPending, Running, Paused, Completed, Failed,
    }

public class
ProgressResult {
    
    public Exception? Error = null;
    public DownloadStatus Status { get; }
    public long Value { get; }

    }

public class 
DownloadProgress {

    /// <summary>
    /// Возвращает число полученных байтов с последнего обновления.
    /// </summary>
    public long LastBytesDownloaded { get; }

    /// <summary>
    /// Возвращает общее количество байтов, которое будет получено.
    /// </summary>
    public long TotalBytesToReceive { get; }

    /// <summary>
    /// Возвращает число полученных байтов за все время операции.
    /// </summary>
    public long BytesReceived { get; }

    /// <summary>
    /// Возвращает текущую скорость загрузки в байтах в секунду.
    /// </summary>
    public double TransferSpeed { get; }

    /// <summary>
    /// Возвращает статус операции загрузки.
    /// </summary>
    public DownloadStatus Status { get; }

    public DateTime? StartedAt { get; }

    /// <summary>
    /// Возвращает сообщение о текущем ходе операции.
    /// </summary>
    public string Message { get; }

    /// <summary>
    /// Возвращает значение, показывающее, какая ошибка произошла в течение асинхронной операции.
    /// </summary>
    public Exception? Error { get; }

    #region Constructors

    private DownloadProgress(long lastBytesDownloaded,
                             long totalBytesToReceive,
                             long bytesReceived,
                             double transferSpeed,
                             DownloadStatus downloadStatus = DownloadStatus.NotStarted,
                             string message = "",
                             Exception? error = null,
                             DateTime? startedAt = null) {
        LastBytesDownloaded = lastBytesDownloaded;
        TotalBytesToReceive = totalBytesToReceive;
        BytesReceived = bytesReceived;
        TransferSpeed = transferSpeed;
        Status = downloadStatus;
        Message = message;
        Error = error;
        StartedAt = startedAt;
        }

    #endregion

    #region Static methods

    public static DownloadProgress
    Create() {
        return new DownloadProgress(
        lastBytesDownloaded: 0,
        totalBytesToReceive: 0,
        bytesReceived: 0,
        transferSpeed: 0,
        downloadStatus: DownloadStatus.NotStarted,
        message: string.Empty);
        }

    public static DownloadProgress
    Connecting(DateTime? startedAt = null) {
        return new DownloadProgress(
        lastBytesDownloaded: 0,
        totalBytesToReceive: 0,
        bytesReceived: 0,
        transferSpeed: 0,
        downloadStatus: DownloadStatus.StartPending,
        message: "Connecting...",
        startedAt: startedAt ?? DateTime.Now);
    }

    public static DownloadProgress
    Connected(long totalBytesToReceive, long bytesReceived = 0) {
        return new DownloadProgress(
        lastBytesDownloaded: 0,
        totalBytesToReceive: totalBytesToReceive,
        bytesReceived: bytesReceived,
        transferSpeed: 0,
        downloadStatus: DownloadStatus.StartPending,
        message: "Connected");
    }

    public static DownloadProgress
    Downloading(long lastBytesDownloaded, long totalBytesToReceive, long bytesReceived, double transferSpeed = 0) {
        return new DownloadProgress(
        lastBytesDownloaded: lastBytesDownloaded,
        totalBytesToReceive: totalBytesToReceive,
        bytesReceived: bytesReceived,
        transferSpeed: transferSpeed,
        downloadStatus: DownloadStatus.Running,
        message: "Downloading...");
    }

    public static DownloadProgress
    Finishing() {
        return new DownloadProgress(
        lastBytesDownloaded: 0,
        totalBytesToReceive: 0,
        bytesReceived: 0,
        transferSpeed: 0,
        downloadStatus: DownloadStatus.Running,
        message: "Finishing...");
    }

    public static DownloadProgress
    Finished(long totalBytesToReceive, long bytesReceived) {
        return new DownloadProgress(
        lastBytesDownloaded: 0,
        totalBytesToReceive: totalBytesToReceive,
        bytesReceived: bytesReceived,
        transferSpeed: 0,
        downloadStatus: DownloadStatus.Completed,
        message: "Finished");
    }

    #endregion

    #region Methods

    /// <summary>
    /// Возвращает DownloadProgress полученный в результате изменения свойства
    /// </summary>
    /// <param name="lastBytesDownloaded">Количество полученных байтов с последнего обновления</param>
    public DownloadProgress
    WithLastBytesDownloaded(long lastBytesDownloaded) {

        lastBytesDownloaded.VerifyGreaterEqualZero();

        return new DownloadProgress(
        lastBytesDownloaded: lastBytesDownloaded,
        totalBytesToReceive: TotalBytesToReceive,
        bytesReceived: BytesReceived,
        transferSpeed: TransferSpeed,
        downloadStatus: Status,
        message: Message,
        error: Error);
    }

    /// <summary>
    /// Возвращает DownloadProgress полученный в результате изменения свойства
    /// </summary>
    /// <param name="totalBytesToReceive">Общее количество байтов, которое будет получено.</param>
    public DownloadProgress
    WithTotalBytesToReceive(long totalBytesToReceive) {

        totalBytesToReceive.VerifyGreaterEqualZero();

        return new DownloadProgress(
        lastBytesDownloaded: LastBytesDownloaded,
        totalBytesToReceive: totalBytesToReceive,
        bytesReceived: BytesReceived,
        transferSpeed: TransferSpeed,
        downloadStatus: Status,
        message: Message,
        error: Error);
    }

    /// <summary>
    /// Возвращает DownloadProgress полученный в результате изменения свойства
    /// </summary>
    /// <param name="bytesReceived">Общее количество байтов, которое будет получено.</param>
    public DownloadProgress
    WithBytesReceived(long bytesReceived) {

        bytesReceived.VerifyGreaterEqualZero();

        return new DownloadProgress(
        lastBytesDownloaded: LastBytesDownloaded,
        totalBytesToReceive: TotalBytesToReceive,
        bytesReceived: bytesReceived,
        transferSpeed: TransferSpeed,
        downloadStatus: Status,
        message: Message,
        error: Error);
    }

    /// <summary>
    /// Возвращает DownloadProgress полученный в результате изменения свойства
    /// </summary>
    /// <param name="transferSpeed">Текущая скорость загрузки в байтах в секунду</param>
    public DownloadProgress
    WithTransferSpeed(double transferSpeed) {
        transferSpeed.VerifyGreaterEqualZero();
        return new DownloadProgress(
        lastBytesDownloaded: LastBytesDownloaded,
        totalBytesToReceive: TotalBytesToReceive,
        bytesReceived: BytesReceived,
        transferSpeed: transferSpeed,
        downloadStatus: Status,
        message: Message,
        error: Error);
    }

    /// <summary>
    /// Возвращает DownloadProgress полученный в результате изменения свойства
    /// </summary>
    /// <param name="downloadStatus">Статус операции загрузки</param>
    public DownloadProgress WithStatus(DownloadStatus downloadStatus) {
        return new DownloadProgress(
        lastBytesDownloaded: LastBytesDownloaded,
        totalBytesToReceive: TotalBytesToReceive,
        bytesReceived: BytesReceived,
        transferSpeed: TransferSpeed,
        downloadStatus: downloadStatus,
        message: Message,
        error: Error);
    }


    /// <summary>
    /// Возвращает DownloadProgress полученный в результате изменения свойства
    /// </summary>
    /// <param name="message">Сообщение о текущем ходе операции</param>
    public DownloadProgress
    WithMessage(string message) {
        return new DownloadProgress(
        lastBytesDownloaded: LastBytesDownloaded,
        totalBytesToReceive: TotalBytesToReceive,
        bytesReceived: BytesReceived,
        transferSpeed: TransferSpeed,
        downloadStatus: Status,
        message: message,
        error: Error);
    }

    /// <summary>
    /// Возвращает DownloadProgress полученный в результате изменения свойства
    /// </summary>
    /// <param name="error">Значение, показывающее, какая ошибка произошла в течение асинхронной операции</param>
    public DownloadProgress
    WithError(Exception? error) {
        return new DownloadProgress(
        lastBytesDownloaded: LastBytesDownloaded,
        totalBytesToReceive: TotalBytesToReceive,
        bytesReceived: BytesReceived,
        transferSpeed: TransferSpeed,
        downloadStatus: Status,
        message: Message,
        error: error);
        }

    public override string ToString() =>
    $"55% ({BytesReceived} / {TotalBytesToReceive}), {BytesReceived} MB | {TransferSpeed} MB/s";

        #endregion

    }

}
