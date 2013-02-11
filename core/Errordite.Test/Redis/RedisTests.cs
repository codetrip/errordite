
using System;
using System.Collections.Generic;
using System.Diagnostics;
using CodeTrip.Core;
using CodeTrip.Core.Caching.Entities;
using CodeTrip.Core.Caching.Interfaces;
using CodeTrip.Core.IoC;
using CodeTrip.Core.Paging;
using CodeTrip.Core.Redis;
using Errordite.Core.Domain.Error;
using Errordite.Core.Domain.Organisation;
using Errordite.Core.Groups.Queries;
using Errordite.Core.Matching;
using NUnit.Framework;
using CodeTrip.Core.Extensions;

namespace Errordite.Test.Redis
{
    [TestFixture]
    public class RedisTests : ErrorditeCacheTestBase
    {
        [Test]
        public void SerialiseGroups()
        {
            var groups = new GetGroupsResponse
            {
                Groups = new Page<Group>(new List<Group>
                {
                    new Group
                        {
                            Id = "groups/1",
                            Name = "test 1",
                            OrganisationId = "organisations/1"
                        },
                    new Group
                        {
                            Id = "groups/2",
                            Name = "test 2",
                            OrganisationId = "organisations/1"
                        },
                    new Group
                        {
                            Id = "groups/3",
                            Name = "test 3",
                            OrganisationId = "organisations/1"
                        }
                },
                new PagingStatus(10, 1, 3))
            };

            var result = SerializationHelper.ProtobufSerialize(groups);
            var sgroups = SerializationHelper.ProtobufDeserialize<GetGroupsResponse>(result);
            Assert.That(sgroups.Groups != null);
        }

        [Test]
        public void SetAndGetGroups()
        {
            var redisCacheEngine = ObjectFactory.GetObject<ICacheEngine>("redis");
            var cacheConfiguration = ObjectFactory.GetObject<ICacheConfiguration>();
            var groups = new GetGroupsResponse
            {
                Groups = new Page<Group>(new List<Group>
                {
                    new Group
                        {
                            Id = "groups/1",
                            Name = "test 1",
                            OrganisationId = "organisations/1"
                        },
                    new Group
                        {
                            Id = "groups/2",
                            Name = "test 2",
                            OrganisationId = "organisations/1"
                        },
                    new Group
                        {
                            Id = "groups/3",
                            Name = "test 3",
                            OrganisationId = "organisations/1"
                        }
                }, 
                new PagingStatus(10, 1, 3))
            };
            var request = new GetGroupsRequest
            {
                OrganisationId = "organisations/1",
                Paging = new PageRequestWithSort(1, 10)
            };
            var cacheKey =
                CacheKey.ForProfile(cacheConfiguration.GetCacheProfile(CacheProfiles.Groups)).WithKey(
                    ((ICacheable)request).CacheItemKey).Create();

            redisCacheEngine.Put(new CacheItemInfo
            {
                Item = groups,
                Key = cacheKey
            });

            GetGroupsResponse fromCacheGroups;
            redisCacheEngine.Get(cacheKey, out fromCacheGroups);

            Assert.That(fromCacheGroups.Groups != null);
        }

        [Test]
        public void GetMultipleItemsFromRedis()
        {
            const int db = 15;
            var connectionManager = ObjectFactory.GetObject<IRedisSession>();
            connectionManager.TryOpenConnection();
            connectionManager.Connection.Strings.Set(db, "test-1", "1");
            connectionManager.Connection.Strings.Set(db, "test-2", "2");
            var task = connectionManager.Connection.Strings.Set(db, "test-3", "3");

            connectionManager.Connection.Wait(task);

            var items = connectionManager.Connection.Wait(connectionManager.Connection.Strings.GetString(db, new[] { "test-0", "test-1", "test-2", "test-3" }));

            Assert.That(items[0] == null);
            Assert.That(items[1] == "1");
            Assert.That(items[2] == "2");
            Assert.That(items[3] == "3");

            connectionManager.Connection.Wait(connectionManager.Connection.Strings.Increment(db, "test-1"));
            var result = connectionManager.Connection.Strings.GetString(db, "test-1");
            Assert.That(connectionManager.Connection.Wait(result) == "2");

            connectionManager.Connection.Keys.Remove(db, new[] { "test-0", "test-1", "test-2", "test-3" });
        }

        [Test]
        public void BulkPutToRedis()
        {
            var redisCacheEngine = ObjectFactory.GetObject<ICacheEngine>();

            Stopwatch watch = Stopwatch.StartNew();

            IList<string> ids = new List<string>();
            for(int i=0;i< 10000;i++)
            {
                var errorClass = new Issue
                {
                    ApplicationId = "ds",
                    UserId = "ds/ds",
                    CreatedOnUtc = DateTime.Now,
                    Name = "Test",
                    Id = Guid.NewGuid().ToString(),
                    ErrorCount = 212,
                    Rules = new List<IMatchRule>(),
                    Status = IssueStatus.Acknowledged
                };

                ids.Add(errorClass.Id);

                var cacheKey = CacheKey.ForProfile(new CacheProfile(1, "test", redisCacheEngine))
                    .WithKey(errorClass.Id)
                    .Create();

                redisCacheEngine.Put(new CacheItemInfo
                {
                    Item = errorClass,
                    Key = cacheKey
                });
            }

            Console.WriteLine("Put 10000 items to Redis in {0}ms".FormatWith(watch.ElapsedMilliseconds));

            watch = Stopwatch.StartNew();

            foreach (string id in ids)
            {
                Issue item;
                redisCacheEngine.Get(CacheKey.ForProfile(new CacheProfile(1, "test", redisCacheEngine))
                    .WithKey(id)
                    .Create(), out item);

                Assert.That(item != null);
            }

            Console.WriteLine("Retrieved 10000 items from Redis in {0}ms".FormatWith(watch.ElapsedMilliseconds));
        }

        //[Test]
        //public void LargeItemToRedis()
        //{
        //    var redisCacheEngine = ObjectFactory.GetObject<ICacheEngine>();

        //    List<Issue> classes = new List<Issue>(); 
    
        //    const string id = "classes";
        //    for (int i = 0; i < 10000; i++)
        //    {
        //        var errorClass = new Issue
        //        {
        //            ApplicationId = "ds",
        //            UserId = "ds/ds",
        //            CreatedOnUtc = DateTime.Now,
        //            Name = "Test",
        //            Id = Guid.NewGuid().ToString(),
        //            ErrorCount = 212,
        //            History = new List<IssueHistory>
        //            {
        //                new IssueHistory
        //                {
        //                    UserId = "Nick",
        //                    Message = "Test",
        //                    DateAddedUtc = DateTime.Now
        //                }
        //            },
        //            Rules = new List<IMatchRule>(),
        //            Status = IssueStatus.Acknowledged
        //        };

        //        classes.Add(errorClass);
        //    }

        //    var cacheKey = CacheKey.ForProfile(new CacheProfile("test", redisCacheEngine))
        //            .WithKey(id)
        //            .Create();

        //    Stopwatch watch = Stopwatch.StartNew();

        //    redisCacheEngine.Put(new CacheItem
        //    {
        //        Item = classes,
        //        Key = cacheKey
        //    });

        //    Console.WriteLine("Put large object in {0}ms".FormatWith(watch.ElapsedMilliseconds));

        //    watch = Stopwatch.StartNew();

        //    List<Issue> item;
        //    redisCacheEngine.Get(CacheKey.ForProfile(new CacheProfile("test", redisCacheEngine))
        //        .WithKey(id)
        //        .Create(), out item);

        //    Console.WriteLine("Get large object in {0}ms".FormatWith(watch.ElapsedMilliseconds));
        //    Assert.That(item != null);
        //    Assert.That(item.Count == 10000);
        //}

        //[Test]
        //public void FlushPrefixedItemsFromRedisCache()
        //{
        //    var redisCacheEngine = ObjectFactory.GetObject<ICacheEngine>();

        //    IList<string> ids = new List<string>();
        //    for (int i = 0; i < 100; i++)
        //    {
        //        var errorClass = new Issue
        //        {
        //            ApplicationId = "ds",
        //            UserId = "ds/ds",
        //            CreatedOnUtc = DateTime.Now,
        //            Name = "Test",
        //            Id = "prefix-{0}".FormatWith(Guid.NewGuid().ToString()),
        //            ErrorCount = 212,
        //            History = new List<IssueHistory>
        //            {
        //                new IssueHistory
        //                {
        //                    UserId = "Nick",
        //                    Message = "Test",
        //                    DateAddedUtc = DateTime.Now
        //                }
        //            },
        //            Rules = new List<IMatchRule>(),
        //            Status = IssueStatus.Acknowledged
        //        };

        //        ids.Add(errorClass.Id);

        //        var cacheKey = CacheKey.ForProfile(new CacheProfile("test", redisCacheEngine))
        //            .WithKey(errorClass.Id)
        //            .Create();

        //        redisCacheEngine.Put(new CacheItem
        //        {
        //            Item = errorClass,
        //            Key = cacheKey
        //        });
        //    }

        //    foreach(string id in ids)
        //    {
        //        Issue item;
        //        redisCacheEngine.Get(CacheKey.ForProfile(new CacheProfile("test", redisCacheEngine))
        //            .WithKey(id)
        //            .Create(), out item);

        //        Assert.That(item != null);
        //    }

        //    Stopwatch watch = Stopwatch.StartNew();
        //    redisCacheEngine.Flush("test", "prefix-");
        //    Console.WriteLine("Flush 100 prefixed items in {0}ms".FormatWith(watch.ElapsedMilliseconds));

        //    foreach (string id in ids)
        //    {
        //        Issue item;
        //        redisCacheEngine.Get(CacheKey.ForProfile(new CacheProfile("test", redisCacheEngine))
        //            .WithKey(id)
        //            .Create(), out item);

        //        Assert.That(item == null);
        //    }
        //}
    }
}
