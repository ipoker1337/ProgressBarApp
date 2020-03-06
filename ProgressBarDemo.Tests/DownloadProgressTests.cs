using System;
using NUnit.Framework;
using ProgressBarDemo.Domain;
using ProgressBarDemo.Domain.Interfaces;

namespace ProgressBarDemo.Tests {
    internal class
    DownloadProgressTests {

        private DownloadProgress _model = DownloadProgress.Create();

        [SetUp]
        public void 
        Setup() {
            _model = DownloadProgress.Create();
        }

        [Test]
        public void
        ConstructorTest() {
            Assert.AreEqual(0, _model.TotalBytesToReceive);
            Assert.AreEqual(0, _model.TransferSpeed);
            Assert.AreEqual(0, _model.LastBytesDownloaded);
            Assert.AreEqual(null, _model.StartedAt);
            Assert.AreEqual(DownloadStatus.NotStarted, _model.Status);
            Assert.IsEmpty(_model.Message);
            Assert.IsNull(_model.Error);
        }

        [Test]
        public void 
        WithLastBytesDownloaded_Test() {
            var newModel = _model.WithLastBytesDownloaded(50);
            Assert.AreEqual(50, newModel.LastBytesDownloaded);
            Assert.AreEqual(0, _model.LastBytesDownloaded);
            Assert.Throws<ArgumentOutOfRangeException>(() => { _model.WithLastBytesDownloaded(-1); });
        }

        [Test]
        public void 
        WithTotalBytesToReceive_Test() {
            var newModel = _model.WithTotalBytesToReceive(50);
            Assert.AreEqual(50, newModel.TotalBytesToReceive);
            Assert.AreEqual(0, _model.TotalBytesToReceive);
            Assert.Throws<ArgumentOutOfRangeException>(() => { _model.WithLastBytesDownloaded(-2); });
        }

        [Test]
        public void 
        WithBytesToReceive_Test() {
            var newModel = _model.WithBytesReceived(50);
            Assert.AreEqual(50, newModel.BytesReceived);
            Assert.AreEqual(0, _model.BytesReceived);
            Assert.Throws<ArgumentOutOfRangeException>(() => { _model.WithBytesReceived(-2); });
        }

        [Test]
        public void 
        WithTransferSpeed_Test() {
            var newModel = _model.WithTransferSpeed(50);
            Assert.AreEqual(50, newModel.TransferSpeed);
            Assert.AreEqual(0, _model.TransferSpeed);
            Assert.Throws<ArgumentOutOfRangeException>(() => { _model.WithTransferSpeed(-1); });
        }

        [Test]
        public void 
        WithStatus_Test() {
            var newModel = _model.WithStatus(DownloadStatus.Failed);
            Assert.AreEqual(DownloadStatus.Failed, newModel.Status);
            Assert.AreEqual(DownloadStatus.NotStarted, _model.Status);
        }

        [Test]
        public void 
        WithMessage_Test() {
            const string test = "test";
            var newModel = _model.WithMessage(test);
            Assert.AreEqual(test, newModel.Message);
            Assert.IsEmpty(_model.Message);
        }

        [Test]
        public void
        WithError_Test() {
            var newModel = _model.WithError(new System.Exception());
            Assert.IsNotNull(newModel.Error);
            Assert.IsNull(_model.Error);
        }

        [Test]
        public void
        ConnectingMethod_Test() {
            var dateTime = DateTime.MaxValue;
            _model = DownloadProgress.Connecting(dateTime);
            
            Assert.AreEqual(0, _model.TotalBytesToReceive);
            Assert.AreEqual(0, _model.TransferSpeed);
            Assert.AreEqual(0, _model.LastBytesDownloaded);
            Assert.AreEqual(DownloadStatus.StartPending, _model.Status);
            Assert.AreEqual(dateTime, _model.StartedAt);
            Assert.IsNull(_model.Error);
        }

        [Test]
        public void
        ConnectedMethod_Test() {
            _model = DownloadProgress.Connected(150, 100);

            Assert.AreEqual(150, _model.TotalBytesToReceive);
            Assert.AreEqual(0, _model.TransferSpeed);
            Assert.AreEqual(100, _model.BytesReceived);
            Assert.AreEqual(DownloadStatus.StartPending, _model.Status);
            Assert.IsNull(_model.Error);
        }

        [Test]
        public void
        DownloadingMethod_Test() {
            _model = DownloadProgress.Downloading(500, 1500, 100, 200);

            Assert.AreEqual(500, _model.LastBytesDownloaded);
            Assert.AreEqual(1500, _model.TotalBytesToReceive);
            Assert.AreEqual(100, _model.BytesReceived);
            Assert.AreEqual(200, _model.TransferSpeed);
            Assert.AreEqual(DownloadStatus.Running, _model.Status);
            Assert.IsNull(_model.Error);
        }

        [Test]
        public void
        FinishingMethod_Test() {
            _model = DownloadProgress.Finishing();

            Assert.AreEqual(0, _model.TotalBytesToReceive);
            Assert.AreEqual(0, _model.TransferSpeed);
            Assert.AreEqual(0, _model.LastBytesDownloaded);
            Assert.AreEqual(DownloadStatus.Running, _model.Status);
            Assert.IsNull(_model.Error);
        }

        [Test]
        public void
        FinishedMethod_Test() {
            _model = DownloadProgress.Finished(50, 55);

            Assert.AreEqual(50, _model.TotalBytesToReceive);
            Assert.AreEqual(55, _model.BytesReceived);
            Assert.AreEqual(0, _model.TransferSpeed);
            Assert.AreEqual(0, _model.LastBytesDownloaded);
            Assert.AreEqual(DownloadStatus.Completed, _model.Status);
            Assert.IsNull(_model.Error);
        }
    }
}
