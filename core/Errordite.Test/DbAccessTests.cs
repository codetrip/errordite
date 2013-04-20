using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Castle.Core;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using Errordite.Core.IoC;
using Errordite.Core.Domain.Error;
using Errordite.Core.Organisations.Commands;
using Errordite.Core.Reception.Commands;
using NUnit.Framework;
using Raven.Client;
using System.Linq;
using ExceptionInfo = Errordite.Core.Domain.Error.ExceptionInfo;

namespace Errordite.Test
{
    [TestFixture]
    public class DbAccessTests : ErrorditeTestBase
    {
        private IWindsorContainer _container;

        [SetUp]
        public void SetUp()
        {
            _container = ObjectFactory.Container;
            _container.Kernel.ComponentModelCreated += Kernel_ComponentModelCreated;
        }

        [Test]
        public void CreateAdminOrganisationAndUser()
        {
            var createOrg = _container.Resolve<ICreateOrganisationCommand>();
            createOrg.Invoke(new CreateOrganisationRequest()
                                 {
                                     Email = "gaz@Errordite.co.uk",
                                     FirstName = "Gaz",
                                     LastName = "Thackeray",
                                     OrganisationName = "Code Trip",
                                     Password = "password"
                                 });

        }

        [Test]
        public void RavenTest()
        {
            var cmd = _container.Resolve<IReceiveErrorCommand>();

            cmd.Invoke(new ReceiveErrorRequest
            {
                Error = new Error
                {
                    TimestampUtc = DateTime.UtcNow,
                    ExceptionInfos = new[] {new ExceptionInfo
                    {
                        Message = "test message",
                        StackTrace = "test text 2", 
                    }}.ToArray()
                }
            });

            cmd.Invoke(new ReceiveErrorRequest
            {
                Error = new Error
                {
                    TimestampUtc = DateTime.UtcNow,
                    ExceptionInfos = new[] {new ExceptionInfo
                    {
                        Message = "test message",
                        StackTrace = "test text 2",
                    }}.ToArray()
                }
            });

            _container.Resolve<IDocumentSession>().SaveChanges();
        }

        [Test]
        public void RavenQueryTest()
        {
            var session = _container.Resolve<IDocumentSession>();

            var x = session.Query<Error>().Count(e => e.ExceptionInfos.First().Message == "test message");

            Console.WriteLine(x);
        }

        [Test]
        public void SaveExpressionTest()
        {
            var tc1 = new TestClass() { SGetter = ei => ei.ExceptionInfos.First().Type };

            var session = _container.Resolve<IDocumentSession>();

            session.Store(tc1);

            session.SaveChanges();

            var tcFromDb = session.Load<TestClass>(tc1.Id);

            var err = new Error { ExceptionInfos = new[] { new ExceptionInfo { Type = "test" } }.ToArray() };

            Assert.That(tcFromDb.SGetter.Compile()(err), Is.EqualTo("test"));
        }

        public class TestClass
        {
            public string Id { get; set; }
            public Expression<Func<Error, string>> SGetter { get; set; }
        }

        void Kernel_ComponentModelCreated(ComponentModel model)
        {
            if (model.LifestyleType == LifestyleType.PerWebRequest)
                model.LifestyleType = LifestyleType.Singleton;
        }

        //[Test]
        //public void QuerySecondaryMatch()
        //{
        //    IndexCreation.CreateIndexes(typeof(Issues).Assembly, ObjectFactory.GetObject<IDocumentStore>());

        //    var session = _container.Resolve<IAppSession>();

        //    var errors =  session.Raven.Query<BaseErrorsIncludingSecondaryMatch.CombinedResult, BaseErrorsIncludingSecondaryMatch>()
        //        //.Include(e => e.ErrorId)
        //        .Where(e => e.IssueId == "issues/449")
        //        .ToList();

        //    var errorsProper = session.Raven.Load<Error>(errors.Select(e => e.ErrorId));
        //}

        //[Test]
        //public void CreateSecondaryMatch()
        //{
        //    var session = _container.Resolve<IAppSession>();

        //    session.Raven.Store(new SecondaryMatch(){ErrorId = "errors/123"});

        //    session.Raven.SaveChanges();

        //    _rollbackTransaction = false;

        //}
    }
}