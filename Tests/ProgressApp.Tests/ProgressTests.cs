using System;
using System.Diagnostics;
using NUnit.Framework;
using ProgressApp.Core;

namespace ProgressApp.Tests {

public class 
ProgressTest {

    [Test]
    public void 
    RateEstimatorTest() {
        var now = DateTime.UtcNow;

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

        TimeSpan TicksFromSeconds(double seconds) => TimeSpan.FromSeconds(seconds);
    }

    [Test]
    public void 
    ProgressHandlerTest() {
        var handler = new ProgressObserver();
        Assert.AreEqual(null, handler.Progress);

        handler.OnProgress(0, null);
        Assert.AreEqual(0, handler.Progress?.Value);
        Assert.AreEqual(null, handler.Progress?.TargetValue);
        handler.OnProgress(0, 100);
        Assert.AreEqual(100, handler.Progress?.TargetValue);

        const string test = "test";
        handler.OnProgress(10, null, test);
        Assert.AreEqual(10, handler.Progress?.Value);
        Assert.AreEqual(null, handler.Progress?.TargetValue);
        Assert.AreEqual(test, handler.Progress?.Message);

        const string hello = "hello";
        handler.OnProgress(hello);
        Assert.AreEqual(10, handler.Progress?.Value);
        Assert.AreEqual(null, handler.Progress?.TargetValue);
        Assert.AreEqual(hello, handler.Progress?.Message);

        handler.OnProgress(100);
        Assert.AreEqual(110, handler.Progress?.Value);
        Assert.AreEqual(null, handler.Progress?.TargetValue);
        Assert.AreEqual(hello, handler.Progress?.Message);

        handler.OnProgress(100, 200, test);
        Assert.AreEqual(100, handler.Progress?.Value);
        Assert.AreEqual(200, handler.Progress?.TargetValue);
        Assert.AreEqual(test, handler.Progress?.Message);
    }

}
}