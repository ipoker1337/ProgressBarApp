using ProgressBarDemo.Domain;
using ReactiveUI;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using WpfApp2.Infrastructure.Style;

namespace WpfApp2.ViewModels {

    public class MainViewModel : ViewModelBase, IDisposable {

        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        private readonly IDisposable _cleanup;

        private bool _isDarkTheme;

        public MainViewModel() {
            var fileDownloader = new TestFileDownloader();
            ProgressBar = new ProgressBarViewModel(fileDownloader);

            _cleanup = this.WhenAnyValue(x => x.IsDarkTheme)
                            .Subscribe(x => ThemeHelper.SetTheme(x ? Theme.Dark : Theme.Light));

            var canDownload = 
                this.WhenAnyValue(x => x.ProgressBar.ProgressBarState, 
                x =>  x == ProgressBarState.Disabled);

            DownloadCommand = ReactiveCommand.CreateFromTask(Download, canDownload);

            async Task 
            Download() {
                var source = new Uri(@"http://h2n-uptoyou.azureedge.net/main/Hand2NoteInstaller.exe");
                const string destination = "Hand2NoteInstaller.exe";

                await using var stream = File.Create(destination);
                await fileDownloader.DownloadFileAsync(source, destination, _cancellationTokenSource.Token);
            }
        }

        #region Properties

        public ProgressBarViewModel ProgressBar { get; }

        public ICommand DownloadCommand { get; }

        public bool IsDarkTheme {
            get => _isDarkTheme;
            set => SetPropertyIfChanged(ref _isDarkTheme, value);
        }

        #endregion

        public void 
        Dispose() {
            _cancellationTokenSource.Dispose();
            ProgressBar.Dispose();
            _cleanup.Dispose();
        }
    }
}
