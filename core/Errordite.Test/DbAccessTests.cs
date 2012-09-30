using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Castle.Core;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using CodeTrip.Core.IoC;
using Errordite.Client.Abstractions;
using Errordite.Core.Domain.Error;
using Errordite.Core.Indexing;
using Errordite.Core.IoC;
using Errordite.Core.Organisations.Commands;
using Errordite.Core.Reception.Commands;
using NServiceBus;
using NUnit.Framework;
using Raven.Client;
using System.Linq;
using Raven.Client.Indexes;
using ExceptionInfo = Errordite.Core.Domain.Error.ExceptionInfo;
using Raven.Client.Linq;

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
            _container.Register(Component.For<IBus>().ImplementedBy<MockBus>());
        }

        [Test]
        public void CreateAdminOrganisationAndUser()
        {
            var createOrg = _container.Resolve<ICreateOrganisationCommand>();
            createOrg.Invoke(new CreateOrganisationRequest()
                                 {
                                     Email = "gaz@codetrip.co.uk",
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

            var x = session.Query<Error>().Count(e => e.ExceptionInfo.Message == "test message");

            Console.WriteLine(x);
        }

        [Test]
        public void SaveExpressionTest()
        {
            var tc1 = new TestClass() {SGetter = ei => ei.ExceptionInfo.Type};

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

    public class MockBus : IBus
    {
        public T CreateInstance<T>()
        {
            throw new NotImplementedException();
        }

        public T CreateInstance<T>(Action<T> action)
        {
            throw new NotImplementedException();
        }

        public object CreateInstance(Type messageType)
        {
            throw new NotImplementedException();
        }

        public void Publish<T>(params T[] messages)
        {
            throw new NotImplementedException();
        }

        public void Publish<T>(Action<T> messageConstructor)
        {
            throw new NotImplementedException();
        }

        public void Subscribe(Type messageType)
        {
            throw new NotImplementedException();
        }

        public void Subscribe<T>()
        {
            throw new NotImplementedException();
        }

        public void Subscribe(Type messageType, Predicate<object> condition)
        {
            throw new NotImplementedException();
        }

        public void Subscribe<T>(Predicate<T> condition)
        {
            throw new NotImplementedException();
        }

        public void Unsubscribe(Type messageType)
        {
            throw new NotImplementedException();
        }

        public void Unsubscribe<T>()
        {
            throw new NotImplementedException();
        }

        public ICallback SendLocal(params object[] messages)
        {
            throw new NotImplementedException();
        }

        public ICallback SendLocal<T>(Action<T> messageConstructor)
        {
            throw new NotImplementedException();
        }

        public ICallback Send(params object[] messages)
        {
            throw new NotImplementedException();
        }

        public ICallback Send<T>(Action<T> messageConstructor)
        {
            throw new NotImplementedException();
        }

        public ICallback Send(string destination, params object[] messages)
        {
            throw new NotImplementedException();
        }

        public ICallback Send(Address address, params object[] messages)
        {
            throw new NotImplementedException();
        }

        public ICallback Send<T>(string destination, Action<T> messageConstructor)
        {
            throw new NotImplementedException();
        }

        public ICallback Send<T>(Address address, Action<T> messageConstructor)
        {
            throw new NotImplementedException();
        }

        public ICallback Send(string destination, string correlationId, params object[] messages)
        {
            throw new NotImplementedException();
        }

        public ICallback Send(Address address, string correlationId, params object[] messages)
        {
            throw new NotImplementedException();
        }

        public ICallback Send<T>(string destination, string correlationId, Action<T> messageConstructor)
        {
            throw new NotImplementedException();
        }

        public ICallback Send<T>(Address address, string correlationId, Action<T> messageConstructor)
        {
            throw new NotImplementedException();
        }

        public ICallback SendToSites(IEnumerable<string> siteKeys, params object[] messages)
        {
            throw new NotImplementedException();
        }

        public ICallback Defer(TimeSpan delay, params object[] messages)
        {
            throw new NotImplementedException();
        }

        public ICallback Defer(DateTime processAt, params object[] messages)
        {
            throw new NotImplementedException();
        }

        public void Reply(params object[] messages)
        {
            throw new NotImplementedException();
        }

        public void Reply<T>(Action<T> messageConstructor)
        {
            throw new NotImplementedException();
        }

        public void Return<T>(T errorEnum)
        {
            throw new NotImplementedException();
        }

        public void HandleCurrentMessageLater()
        {
            throw new NotImplementedException();
        }

        public void ForwardCurrentMessageTo(string destination)
        {
            throw new NotImplementedException();
        }

        public void DoNotContinueDispatchingCurrentMessageToHandlers()
        {
            throw new NotImplementedException();
        }

        public IDictionary<string, string> OutgoingHeaders { get; private set; }
        public IMessageContext CurrentMessageContext { get; private set; }
    }
}