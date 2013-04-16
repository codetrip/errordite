using System;
using System.Diagnostics;
using System.Threading;
using Errordite.Client;
using Errordite.Client.Configuration;
using Errordite.Client.Web;
using Errordite.Core.Domain.Error;
using Errordite.Core.Matching;
using NUnit.Framework;
using CodeTrip.Core.Extensions;
using System.Linq;

namespace Errordite.Test.Automation.Tests
{
    public class RuleMatchTests : AutomatedTestBase
    {
        [Test]
        public void SameExceptionSameIssue()
        {
            Armoury.ErrorditeDriver.Register();
            var app = Armoury.ErrorditeDriver.AddApplication("testapp");

            var error = Armoury.ErrorditeClientDriver.SendException(app, new Exception());
            var error2 = Armoury.ErrorditeClientDriver.SendException(app, new Exception());

            Assert.That(error.IssueId, Is.EqualTo(error2.IssueId));
        }

        [Test]
        public void DifferentExceptionTypeDifferentIssue()
        {
            Armoury.ErrorditeDriver.Register();
            var app = Armoury.ErrorditeDriver.AddApplication("testapp");
            
            var error = Armoury.ErrorditeClientDriver.SendException(app, new Exception());
            var error2 = Armoury.ErrorditeClientDriver.SendException(app, new InvalidOperationException());

            Assert.That(error.IssueId, Is.Not.EqualTo(error2.IssueId));
        }

        [Test]
        public void SplitErrorsByChangingRules()
        {
            Armoury.ErrorditeDriver.Register();
            var app = Armoury.ErrorditeDriver.AddApplication("testapp");

            var error1 = Armoury.ErrorditeClientDriver.SendException(app, new Exception("message1"));
            var error2 = Armoury.ErrorditeClientDriver.SendException(app, new Exception("message2"));

            Assert.That(error1.IssueId, Is.EqualTo(error2.IssueId));

            Armoury.ErrorditeDriver.AddRule(error1.IssueId, "Message", StringOperator.Equals, "message1");
            Armoury.ErrorditeDriver.ApplyRuleUpdates("AdjustedIssue", "UnadjustedIssue");

            string error1IssueId = error1.IssueId;
            string error2IssueId = error2.IssueId;

            Armoury.AppSession.Raven.Advanced.Refresh(error1);
            Armoury.AppSession.Raven.Advanced.Refresh(error2);
            
            Assert.That(error1.IssueId, Is.EqualTo(error1IssueId));
            Assert.That(error2.IssueId, Is.Not.EqualTo(error2IssueId));

            var issue1 = Armoury.AppSession.Raven.Load<Issue>(error1.IssueId);
            Assert.That(issue1.ErrorCount, Is.EqualTo(1));

            var issue2 = Armoury.AppSession.Raven.Load<Issue>(error2.IssueId);
            Assert.That(issue2.ErrorCount, Is.EqualTo(1));
        }

        [Test]
        public void UnclassifiedErrorsCollectedByRuleChange()
        {
            Armoury.ErrorditeDriver.Register();
            var app = Armoury.ErrorditeDriver.AddApplication("testapp");

            var error1 = Armoury.ErrorditeClientDriver.SendException(app, new Exception("message1"));
            var error2 = Armoury.ErrorditeClientDriver.SendException(app, new InvalidOperationException("message1"));

            Assert.That(error1.IssueId, Is.Not.EqualTo(error2.IssueId));

            string error1IssueId = error1.IssueId;
            string error2IssueId = error2.IssueId;

            var issue2 = Armoury.AppSession.Raven.Load<Issue>(error2IssueId);

            Armoury.ErrorditeDriver.ChangeRule(error1.IssueId, "Type", StringOperator.Equals, null, "Message", StringOperator.StartsWith, "message");
            Armoury.ErrorditeDriver.ApplyRuleUpdates("AdjustedIssue", "UnadjustedIssue");

            Wait.ThenAssert(() =>
                                {
                                    try
                                    {
                                        Armoury.AppSession.Raven.Advanced.Refresh(issue2);
                                    }
                                    catch (InvalidOperationException ex)
                                    {
                                        return ex.Message ==
                                               "Document '{0}' no longer exists and was probably deleted".FormatWith(
                                                   issue2.Id);
                                    }
                                    
                                    return false;
                                }, 20, "error not deleted");

        }
    }

    public static class Wait
    {
        public static bool For(Func<bool> action, int timeoutSeconds)
        {
            var sw = new Stopwatch();
            sw.Start();

            while (sw.Elapsed.TotalSeconds < timeoutSeconds)
            {
                if (action())
                    return true;

                Thread.Sleep(1000);
            }

            return false;
        }

        public static void ThenAssert(Func<bool> action, int timeoutSeconds, string assertMessage )
        {
            if (!Wait.For(action, timeoutSeconds))
                Assert.Fail(assertMessage);
        }
    }

    public class AutomatedTestsException : Exception
    {
        public AutomatedTestsException(string message)
            :base(message)
        {
            
        }
    }
}