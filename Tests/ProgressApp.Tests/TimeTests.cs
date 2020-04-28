using NUnit.Framework;
using ProgressApp.Core.Common;

namespace ProgressApp.Tests {
public class 
TimeTests {

    [Test]
    public void 
    TestStrings() {
        Assert.AreEqual("1 sec", 1.Seconds().ToReadable());
        Assert.AreEqual("1 min", 1.Minutes().ToReadable());
        Assert.AreEqual("1 hour", 1.Hours().ToReadable());

        Assert.AreEqual("2 secs", 2.Seconds().ToReadable());
        Assert.AreEqual("2 mins", 2.Minutes().ToReadable());
        Assert.AreEqual("2 hours", 2.Hours().ToReadable());
    }
}
}
