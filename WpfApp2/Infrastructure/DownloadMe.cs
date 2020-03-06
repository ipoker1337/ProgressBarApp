using System;
using System.Threading;
using System.Threading.Tasks;

namespace WpfApp2.Infrastructure
{
    public class DownloadMe
    {
        //Exposes count of received bytes from the last BytesReceived event, i.e. bytes count increment
        public event Action<int>? BytesReceived;
        public event Action? Connecting;
        public event Action? Connected;
        public event Action? Finishing;
        public event Action? Finished;
        public event Action? Paused;
        public int TotalBytesToDownload { get; }

        private readonly int _averageSpeedBytesPerSec;
        private readonly Random _random = new Random();

        public DownloadMe()
        {
            TotalBytesToDownload = _random.Next(30) * 1_000_000;
            _averageSpeedBytesPerSec = _random.Next(3_000_000);
        }

        //Continue download from the given initialPosition
        public void StartDownload(CancellationToken cancellationToken = new CancellationToken(), int initialPosition = 0)
        {
            var position = initialPosition;
            Connect();
            while (position < TotalBytesToDownload && !cancellationToken.IsCancellationRequested)
            {
                //Simulate network delay
                Task.Delay(_random.Next(1000 / 20)).Wait();
                var value = Math.Min(TotalBytesToDownload - position, _random.Next(_averageSpeedBytesPerSec / 20));
                position += value;
                OnProgress(value);
            }
            if (!cancellationToken.IsCancellationRequested)
                Finish();
            else
                Paused?.Invoke();
        }

        private void OnProgress(int increment) => BytesReceived?.Invoke(increment);

        //Simulate finalizing like checking hash, saving to disk etc...
        private void Finish()
        {
            Finishing?.Invoke();
            Task.Delay(2000).Wait();
            Finished?.Invoke();
        }

        //Simulate connecting to server...
        private void Connect()
        {
            Connecting?.Invoke();
            Task.Delay(2000).Wait();
            Connected?.Invoke();
        }
    }
}
