
using System;
using System.Collections.Generic;
using Errordite.Core;
using Errordite.Core.Paging;
using Errordite.Core.Domain.Error;
using Errordite.Core.Matching;
using NUnit.Framework;
using Errordite.Core.Extensions;
using System.Linq;

namespace Errordite.Test.Redis
{
    [TestFixture]
    public class SerializationTests
    {
        [Test]
        public void CanSerializeInterfacedCollection()
        {
            var errorClass = new Issue
            {
                ApplicationId = "ds",
                UserId = "ds/ds",
                CreatedOnUtc = DateTime.Now,
                Name = "Test",
                Id = "we",
                ErrorCount = 212,
                Rules = new List<IMatchRule>
                {
                    new PropertyMatchRule("StackTrace", StringOperator.Equals, "test")
                },
                Status = IssueStatus.Acknowledged
            };

            var bytes = SerializationHelper.ProtobufSerialize(errorClass);
            var prClass = SerializationHelper.ProtobufDeserialize<Issue>(bytes);

            Console.WriteLine("PROTOBUF:={0}".FormatWith(bytes.Length));
            Console.ReadLine();
        }

        [Test]
        public void CanSerializeAndDeserializeErrorClassWithProtoBuf()
        {
            object errorClass = new Issue
            {
                ApplicationId = "ds",
                UserId = "ds/ds",
                CreatedOnUtc = DateTime.Now,
                Name = "Test",
                Id = "we",
                ErrorCount = 212,
                Rules = new List<IMatchRule>(),
                Status = IssueStatus.Acknowledged
            };

            var bytes = SerializationHelper.ProtobufSerialize(errorClass);
            var prClass = SerializationHelper.ProtobufDeserialize<Issue>(bytes);

            Console.WriteLine("PROTOBUF:={0}".FormatWith(bytes.Length));
            Console.ReadLine();
        }

        [Test]
        public void CanSerializePage()
        {
            IEnumerable<string> items = new[] {"1", "2", "3"};
            var paging = new PagingStatus(10, 1, 122);
            var page = new Page<string>(items.ToList(), paging);

            var bytes = SerializationHelper.ProtobufSerialize(page);
            var dspage = SerializationHelper.ProtobufDeserialize<Page<string>>(bytes);

            Assert.That(dspage.Items.Count() == 3);
            Assert.That(dspage.PagingStatus.PageSize == 10);
            Assert.That(dspage.PagingStatus.HasNextPage);
            Assert.That(dspage.PagingStatus.TotalPages == 13);
        }
    }
}
