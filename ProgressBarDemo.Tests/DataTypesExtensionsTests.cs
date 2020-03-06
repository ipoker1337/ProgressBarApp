using NUnit.Framework;
using System;
using ProgressBarDemo.Domain.Extensions;

namespace ProgressBarDemo.Tests {
    internal class DataTypesExtensionsTests {

        [Test]
        public void
        IntExtensionsTest() {
            int value = 0;

            //VerifyInRange
            Assert.Throws<ArgumentOutOfRangeException>(() => value.VerifyInRange(1, 10));
            Assert.Throws<ArgumentOutOfRangeException>(() => value.VerifyInRange(-10, -1));
            Assert.AreEqual(0, value.VerifyInRange(0,1));
        }

        [Test]
        public void
        LongExtensionsTest() {
            long value = 5;

            //VerifyGreaterThanOrEqual
            //VerifyGreaterEqualZero
            Assert.AreEqual(5, value.VerifyGreaterThanOrEqual(5));
            Assert.AreEqual(5, value.VerifyGreaterThanOrEqual(4));
            Assert.Throws<ArgumentOutOfRangeException>(() => value.VerifyGreaterThanOrEqual(6));

            //VerifyInRange
             value = 0;
            Assert.Throws<ArgumentOutOfRangeException>(() => value.VerifyInRange(1, 10));
            Assert.Throws<ArgumentOutOfRangeException>(() => value.VerifyInRange(-10, -1));
            Assert.AreEqual(0, value.VerifyInRange(0, 1));
        }

        [Test]
        public void
        DoubleExtensionsTest() {
            double value = 5;

            //VerifyGreaterEqualZero
            Assert.AreEqual(5, value.VerifyGreaterEqualZero());
            value = -1;
            Assert.Throws<ArgumentOutOfRangeException>(() => value.VerifyGreaterEqualZero());

        }

        [Test]
        public void
        StringExtensionsTest() {

            const string value = "nonempty";
            const string emptyValue = "";

            Assert.AreEqual(value, value.VerifyNonEmpty());
            Assert.Throws<ArgumentException>(() => emptyValue.VerifyNonEmpty());
        }
    }
}
