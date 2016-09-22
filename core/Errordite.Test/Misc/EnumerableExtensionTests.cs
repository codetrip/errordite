using Errordite.Core.Extensions;
using NUnit.Framework;

namespace Errordite.Test.Misc
{
    [TestFixture]
    public class EnumerableExtensionTests
    {

        [Test]
        public void StringConcatNormalTest()
        {
            Assert.That(new[] { "A", "B" }.StringConcat("|"), Is.EqualTo("A|B|"));
        }

        [Test]
        public void StringConcatTrimEndTest()
        {
            Assert.That(new[] { "A", "B" }.StringConcat("|", trimEnd: true), Is.EqualTo("A|B"));
        }

        [Test]
        public void StringConcatDifferentLastDelimTest()
        {
            Assert.That(new[] { "A", "B" }.StringConcat("|", trimEnd: false, lastDelimiter: "."), Is.EqualTo("A|B."));
        }

        [Test]
        public void StringConcatDifferentLastDelimPlusTrimEndTest()
        {
            Assert.That(new[] { "A", "B", "C" }.StringConcat(", ", trimEnd: true, lastDelimiter: " + "), Is.EqualTo("A, B + C"));
        }

        [Test]
        public void StringConcatDifferentLastDelimPlusTrimEndOneElementTest()
        {
            Assert.That(new[] { "A" }.StringConcat(", ", trimEnd: true, lastDelimiter: " + "), Is.EqualTo("A"));
        }

        [Test]
        public void StringConcatOneElementTest()
        {
            Assert.That(new[] { "A" }.StringConcat("|"), Is.EqualTo("A|"));
        }

        [Test]
        public void StringConcatOneElementTrimEndTest()
        {
            Assert.That(new[] { "A" }.StringConcat("|", trimEnd: true), Is.EqualTo("A"));
        } 
    }
}