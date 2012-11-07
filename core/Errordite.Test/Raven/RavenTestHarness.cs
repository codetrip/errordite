
using System.Data.SqlClient;
using System.Transactions;
using NUnit.Framework;

namespace Errordite.Test.Raven
{
    public class User
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public int Age { get; set; }
    }

    [TestFixture]
    public class RavenTestHarness : RavenTestBase
    {
        [Test]
        public void StoreGetAndUpdateWithinDistributedTransaction()
        {
            using (var t = new TransactionScope(TransactionScopeOption.Required))
            {
                using (var conn = new SqlConnection("Data Source=asnav-devsql-14\\backoffice; Initial Catalog=SM_Sitecore_Core; Integrated Security=True;"))
                {
                    conn.Open();

                    using (var conn2 = new SqlConnection("Data Source=asnav-devsql-14\\backoffice; Initial Catalog=SM_Sitecore_Master; Integrated Security=True;"))
                    {
                        conn2.Open();
                    }

                    var user = new User {Name = "Nick", Age = 23};
                    var session = GetSession();
                    session.Store(user);
                    session.SaveChanges();

                    user = session.Load<User>(user.Id);
                    user.Age = 24;
                    
                    session.SaveChanges();

                    user = session.Load<User>(user.Id);
                    Assert.That(user.Name == "Nick");
                    Assert.That(user.Age == 24);
                }
            }
        }
    }
}
