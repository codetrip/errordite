using System;
using System.Linq;
using Amazon;
using Errordite.Core.Configuration;
using Raven.Abstractions.Data;
using Raven.Client;
using Raven.Json.Linq;

namespace Errordite.Core.Extensions
{
    public static class DocumentStoreExtensions
    {
        public static void ConfigurePeriodicBackup(this IDocumentStore store, ErrorditeConfiguration configuration, string databaseName)
        {
            using (var session = store.OpenSession(databaseName))
            {
                session.Advanced.UseOptimisticConcurrency = true;

                var backupDoc = session.Load<PeriodicBackupSetup>(PeriodicBackupSetup.RavenDocumentKey);

                if (backupDoc == null)
                {
                    backupDoc = new PeriodicBackupSetup
                    {
                        IntervalMilliseconds = configuration.RavenBackupInterval,
                        AwsRegionEndpoint = RegionEndpoint.EUWest1.SystemName,
                        S3BucketName = databaseName,
                    };

                    session.Store(backupDoc, PeriodicBackupSetup.RavenDocumentKey);
                    session.SaveChanges();
                }
            }
        }

        public static void ConfigureBundleAndAWSSettings(this IDocumentStore store, ErrorditeConfiguration configuration, string databaseName, string bundleName)
        {
            using (var session = store.OpenSession())
            {
                session.Advanced.UseOptimisticConcurrency = true;

                var dbDoc = session.Load<RavenJObject>("Raven/Databases/" + databaseName);
                var settings = dbDoc["Settings"].Value<RavenJObject>();
                var secureSettings = dbDoc["SecuredSettings"].Value<RavenJObject>();
                var activeBundles = settings[Constants.ActiveBundles] ?? "";
                var bundles = activeBundles.Value<string>()
                    .Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => x.Trim())
                    .ToArray();

                settings["Raven/AWSAccessKey"] = configuration.AWSAccessKey;
                secureSettings["Raven/AWSSecretKey"] = configuration.AWSSecretKey;

                if (!bundles.Contains(bundleName))
                {
                    settings[Constants.ActiveBundles] = string.Join(";", bundles.Concat(new[] { bundleName }).ToArray());
                }

                session.Store(dbDoc);
                session.SaveChanges();
            }
        }
    }
}
