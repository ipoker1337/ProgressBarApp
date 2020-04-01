using System.Diagnostics;
using NUnit.Framework;
using ProgressApp.Core;

namespace ProgressApp.Tests {

public class 
ProgressTest {

    [Test]
    public void 
    RateEstimatorTest() {
        
        var now = Stopwatch.GetTimestamp();

        var rateEstimator = new RateEstimator(10, now);
        Assert.AreEqual(0, rateEstimator.GetCurrentRate(0, now));

        Assert.AreEqual(50, rateEstimator.GetCurrentRate(50, now));

        now += TicksFromSeconds(0.8);
        Assert.AreEqual(100, rateEstimator.GetCurrentRate(50, now));

        now += TicksFromSeconds(3);
        Assert.AreEqual(25, rateEstimator.GetCurrentRate(0, now));
        Assert.AreEqual(40, rateEstimator.GetCurrentRate(60, now));

        now += TicksFromSeconds(4);
        Assert.AreEqual(20, rateEstimator.GetCurrentRate(0, now));

        now += TicksFromSeconds(2);
        Assert.AreEqual(16, rateEstimator.GetCurrentRate(0, now));
        Assert.AreEqual(26, rateEstimator.GetCurrentRate(100, now));

        now += TicksFromSeconds(1);
        Assert.AreEqual(16, rateEstimator.GetCurrentRate(0, now));
        Assert.AreEqual(26, rateEstimator.GetCurrentRate(100, now));

        now += TicksFromSeconds(40);
        Assert.AreEqual(0, rateEstimator.GetCurrentRate(0, now));
        Assert.AreEqual(100, rateEstimator.GetCurrentRate(100, now));

        long TicksFromSeconds(double seconds) => (long)(Stopwatch.Frequency * seconds);
    }

    [Test]
    public void 
    ProgressHandlerTest() {
        var handler = new ProgressHandler();
        Assert.AreEqual(null, handler.Progress);

        handler.Report(0, null);
        Assert.AreEqual(0, handler.Progress?.Value);
        Assert.AreEqual(null, handler.Progress?.TargetValue);
        handler.Report(0, 100);
        Assert.AreEqual(100, handler.Progress?.TargetValue);

        const string test = "test";
        handler.Report(10, null, test);
        Assert.AreEqual(10, handler.Progress?.Value);
        Assert.AreEqual(null, handler.Progress?.TargetValue);
        Assert.AreEqual(test, handler.Progress?.Message);

        const string hello = "hello";
        handler.Report(hello);
        Assert.AreEqual(10, handler.Progress?.Value);
        Assert.AreEqual(null, handler.Progress?.TargetValue);
        Assert.AreEqual(hello, handler.Progress?.Message);

        handler.Report(100);
        Assert.AreEqual(110, handler.Progress?.Value);
        Assert.AreEqual(null, handler.Progress?.TargetValue);
        Assert.AreEqual(hello, handler.Progress?.Message);

        handler.Report(100, 200, test);
        Assert.AreEqual(100, handler.Progress?.Value);
        Assert.AreEqual(200, handler.Progress?.TargetValue);
        Assert.AreEqual(test, handler.Progress?.Message);
    }

}
}