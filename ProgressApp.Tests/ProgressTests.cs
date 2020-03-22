using System;
using System.Diagnostics;
using System.Threading;
using NUnit.Framework;
using ProgressApp.Core;

namespace ProgressApp.Tests {
public class 
Tests {
    private static long TicksFromSeconds(double seconds) => 
        (long)(Stopwatch.Frequency * seconds);

    [Test]
    public void 
    RateEstimatorTest() {
        var now = Stopwatch.GetTimestamp();

        var rateEstimator = new RateEstimator(10, now);
        Assert.AreEqual(0, rateEstimator.GetCurrentRate(now));

        rateEstimator.Increment(50, now);
        Assert.AreEqual(50, rateEstimator.GetCurrentRate(now));

        now += TicksFromSeconds(0.8);
        rateEstimator.Increment(50, now);
        Assert.AreEqual(100, rateEstimator.GetCurrentRate(now));

        now += TicksFromSeconds(3);
        Assert.AreEqual(25, rateEstimator.GetCurrentRate(now));
        rateEstimator.Increment(60, now);
        Assert.AreEqual(40, rateEstimator.GetCurrentRate(now));

        now += TicksFromSeconds(4);
        Assert.AreEqual(20, rateEstimator.GetCurrentRate(now));

        now += TicksFromSeconds(2);
        Assert.AreEqual(16, rateEstimator.GetCurrentRate(now));
        rateEstimator.Increment(100, now);
        Assert.AreEqual(26, rateEstimator.GetCurrentRate(now));

        now += TicksFromSeconds(1);
        Assert.AreEqual(16, rateEstimator.GetCurrentRate(now));
        rateEstimator.Increment(100, now);
        Assert.AreEqual(26, rateEstimator.GetCurrentRate(now));

        now += TicksFromSeconds(40);
        Assert.AreEqual(0, rateEstimator.GetCurrentRate(now));
        rateEstimator.Increment(100, now);
        Assert.AreEqual(100, rateEstimator.GetCurrentRate(now));
    }

    internal class 
    TestSynchronizationContext : SynchronizationContext {
        public override void Post(SendOrPostCallback d, object state) => d(state);
        public override void Send(SendOrPostCallback d, object state) => d(state);
    }

    internal class 
    TestProgressProvider : ProgressProvider {
        public TestProgressProvider(SynchronizationContext sc) : base(sc) { }
        public new void Report(long value, long? targetValue, string message = "") => base.Report(value, targetValue, message);
        public new void Report(string message) => base.Report(message);
        public new void Report(long deltaValue) => base.Report(deltaValue);
    }

    [Test]
    public void 
    ProgressProviderTest() {
        var provider = new TestProgressProvider(new TestSynchronizationContext());
        Assert.AreEqual(null, provider.Progress);

        provider.Report(0, null);
        Assert.AreEqual(0, provider.Progress?.Value);
        Assert.AreEqual(null, provider.Progress?.TargetValue);
        provider.Report(0, 100);
        Assert.AreEqual(100, provider.Progress?.TargetValue);

        const string test = "test";
        provider.Report(10, null, test);
        Assert.AreEqual(10, provider.Progress?.Value);
        Assert.AreEqual(null, provider.Progress?.TargetValue);
        Assert.AreEqual(test, provider.Progress?.Message);

        const string hello = "hello";
        provider.Report(hello);
        Assert.AreEqual(10, provider.Progress?.Value);
        Assert.AreEqual(null, provider.Progress?.TargetValue);
        Assert.AreEqual(hello, provider.Progress?.Message);

        provider.Report(100);
        Assert.AreEqual(110, provider.Progress?.Value);
        Assert.AreEqual(null, provider.Progress?.TargetValue);
        Assert.AreEqual(hello, provider.Progress?.Message);

        provider.Report(100, 200, test);
        Assert.AreEqual(100, provider.Progress?.Value);
        Assert.AreEqual(200, provider.Progress?.TargetValue);
        Assert.AreEqual(test, provider.Progress?.Message);
    }

}
}