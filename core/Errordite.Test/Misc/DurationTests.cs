using System;
using Errordite.Core;
using NUnit.Framework;

namespace Errordite.Test.Misc
{
    [TestFixture]
    public class DurationTests
    {
        [Test]
         public void Decode()
        {
            var d = new Duration("5M4w3d2h");
            Assert.That(d.ToString(), Is.EqualTo("5M4w3d2h"));
        }

        [Test]
        public void Subtract()
        {
            var dt = new DateTime(2013, 04, 05);

            Assert.That(dt - new Duration(1,2,3,4), Is.EqualTo(dt.AddMonths(-1).AddDays(-17).AddHours(-4)));
        }

        [Test]
        public void Add()
        {
            var dt = new DateTime(2013, 04, 05);

            Assert.That(dt + new Duration(1, 2, 3, 4), Is.EqualTo(dt.AddMonths(1).AddDays(17).AddHours(4)));
        }
    }
}