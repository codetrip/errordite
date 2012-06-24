using System;
using CodeTrip.Core.Encryption;
using Errordite.Core.Domain.Organisation;
using NUnit.Framework;
using CodeTrip.Core.Extensions;

namespace Errordite.Test.Misc
{
    [TestFixture]
    public class EncryptorTests : ErrorditeTestBase
    {
         [Test]
        public void EncryptSomething()
         {
             Console.WriteLine(Get<IEncryptor>().Encrypt("{0}|{1}".FormatWith(1, 1)));
         }
    }
}