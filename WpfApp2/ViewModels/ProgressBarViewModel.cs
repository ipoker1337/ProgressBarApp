using ProgressBarDemo.Domain;
using ProgressBarDemo.Domain.Interfaces;
using System;
using WpfApp2.Infrastructure;

namespace WpfApp2.ViewModels {

    public enum ProgressBarState
    {
        EnablePending,
        Enabled,
        Disabled
    }

    /// <summary>
    /// ProgressBarViewModel
    /// </summary>
    public class 
    ProgressBarViewModel : ViewModelBase, IDisposable
    {
        private readonly IProgressProvider _progressProvider;
        private readonly double _refreshInterval;
        private const string DefaultText = "ProgressBar";

        private IDownloadProgress? _downloadProgress;
        private ProgressBarState _progressBarState;
        private string _text = string.Empty;

        private long _progressMaximum;
        private long _progressValue;
        private bool _isVisible;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="progressProvider"></param>
        /// <param name="refreshIntervalMs">Интервал обновления OnPropertyChanged для Value and ProgressStatusText</param>
        public ProgressBarViewModel(IProgressProvider progressProvider, double refreshIntervalMs = 0) {

            ResetToDefault();

            _refreshInterval = refreshIntervalMs;
            _progressProvider = progressProvider;
            _progressProvider.ProgressChanged += OnProgressChanged;
        }

        private void 
        OnProgressChanged(object? sender, IDownloadProgress p) {

            ChangeState(p.Status);
            UpdateProgress(p);

            // Изменяет состояние ProgressBar в зависимости от состояния операции
            void
            ChangeState(DownloadStatus status) {
                switch (status) {
                    case DownloadStatus.NotStarted:
                        ResetToDefault();
                        break;
                    case DownloadStatus.StartPending:
                        this.ProgressBarState = ProgressBarState.EnablePending;
                        break;
                    case DownloadStatus.Running:
                        this.ProgressBarState = ProgressBarState.Enabled;
                        break;
                    case DownloadStatus.Completed:
                        ResetToDefault();
                        break;
                }
            }

            // Обновление 
            void
            UpdateProgress(IDownloadProgress progress) {
                Maximum = progress.TotalBytesToReceive;
                Value += progress.LastBytesDownloaded;
                Text = progress.Message;
                Progress = progress;
            }
        }

        #region Properties

        private readonly ThrottleDispatcher _valueDispatcher = new ThrottleDispatcher();
        /// <summary>
        /// Возвращает или задает текущее положение индикатора выполнения.
        /// </summary>
        public long Value {
            get => _progressValue;
            private set {
                if (_progressValue == value)
                    return;
                _progressValue = value;
                _valueDispatcher.Throttle(_refreshInterval, _=> OnPropertyChanged(nameof(Value)));
            }
        }

        /// <summary>
        /// Возвращает или задает наибольшее значение диапазона этого элемента управления.
        /// </summary>
        public long Maximum {
            get => _progressMaximum;
            private set => this.SetPropertyIfChanged(ref _progressMaximum, value);
        }

        public IDownloadProgress? Progress {
            get => _downloadProgress;
            set => SetPropertyIfChanged(ref _downloadProgress, value);
        }

        /// <summary>
        /// Возвращает или задает текст текущей операции.
        /// </summary>
        public string Text {
            get => _text;
            set => this.SetPropertyIfChanged(ref _text, value);
        }


        public ProgressBarState ProgressBarState {
            get => _progressBarState;
            set => this.SetPropertyIfChanged(ref _progressBarState, value);
        }

        public bool IsVisible {
            get => _isVisible;
            set => this.SetPropertyIfChanged(ref _isVisible, value);
        }

        #endregion

        #region Methods

        private void ResetToDefault() {
            ProgressBarState = ProgressBarState.Disabled;
            Maximum = 100;
            Value = 0;
            Text = DefaultText;
            IsVisible = true;
        }

        /// <summary>
        /// Dispose
        /// </summary>
        public void 
        Dispose() => _progressProvider.ProgressChanged -= OnProgressChanged;

        #endregion

    }
}
