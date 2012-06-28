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
             Console.WriteLine(Get<IEncryptor>().Encrypt("{0}|{1}|{2}".FormatWith(1, 1, "s@1x")));
         }

        [Test]
        public void DecryptSomething()
        {
            Console.WriteLine(Get<IEncryptor>().Decrypt("6zWOe7cZAboXtelhECTVxw=="));
        }
    }
}