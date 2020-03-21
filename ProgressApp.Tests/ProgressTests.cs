using System;
using System.Diagnostics;
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
}
}