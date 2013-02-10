
using System;
using System.Collections.Generic;
using System.Threading;
using Errordite.Core.Configuration;
using Errordite.Core.Reception;
using NUnit.Framework;

namespace Errordite.Test.Misc
{
    [TestFixture]
    public class RateLimiterTests
    {
        [Test]
        public void WhenRateLimitIsExceeded()
        {
            var rule = new RateLimiterRule
            {
                Count = 10,
                Minutes = 1
            };
            const string applicationId = "test";
            var config = new ErrorditeConfiguration { RateLimiterRules = new List<RateLimiterRule>{rule}};

            var rateLimiter = new ExceptionRateLimiter(config, new UtcDateTime());

            for(int count = 1;count <= 100;count ++)
            {
                if(count <= 10)
                {
                    Assert.That(rateLimiter.Accept(applicationId) == null);
                }
                else
                {
                    Assert.That(rateLimiter.Accept(applicationId) != null);
                }
            }
        }

        [Test]
        public void WhenRateLimitIsNotExceeded()
        {
            var rule = new RateLimiterRule
            {
                Count = 1000,
                Minutes = 5
            };

            const string applicationId = "test2";

            var now = DateTime.UtcNow;
            var settableDateTime = new SettableDateTime();
            var config = new ErrorditeConfiguration { RateLimiterRules = new List<RateLimiterRule> { rule } };
            var rateLimiter = new ExceptionRateLimiter(config, settableDateTime);

            for (int count = 1; count <= 100; count++)
            {
                Assert.That(rateLimiter.Accept(applicationId) == null);
            }

            now = now.AddSeconds(121);
            settableDateTime.SetUtcNow(now);

            for (int count = 101; count <= 500; count++)
            {
                Assert.That(rateLimiter.Accept(applicationId) == null);
            }

            now = now.AddSeconds(61);
            settableDateTime.SetUtcNow(now);

            for (int count = 501; count <= 900; count++)
            {
                Assert.That(rateLimiter.Accept(applicationId) == null);
            }

            now = now.AddSeconds(61);
            settableDateTime.SetUtcNow(now);

            for (int count = 901; count <= 1000; count++)
            {
                Assert.That(rateLimiter.Accept(applicationId) == null);
            }

            for (int count = 1001; count <= 1050; count++)
            {
                Assert.That(rateLimiter.Accept(applicationId) != null);
            }

            now = now.AddSeconds(181);
            settableDateTime.SetUtcNow(now);

            for (int count = 1050; count <= 1075; count++)
            {
                Assert.That(rateLimiter.Accept(applicationId) == null);
            }
        }
    }
}
