using System.Collections.Generic;
using System.IO;
using System.Web;
using System.Linq;

namespace Errordite.Samples.Mvc3.Controllers
{
    public static class DataHelper
    {
        public static Product Get(string id)
        {
            Product product;
            return All().TryGetValue(id, out product) ? product : null;
        }

        private static string Clean(string s)
        {
            return s == "" ? null : s;
        }

        public static IEnumerable<Product> MoreLike(string id)
        {
            return All().Where(p => p.Key != id).Select(p => p.Value);
        }

        private static Dictionary<string, Product> All()
        {
            var products = new Dictionary<string, Product>();

            using (var sr = new StreamReader(HttpContext.Current.Server.MapPath(@"~\App_Data\Database.txt")))
            {
                while (!sr.EndOfStream)
                {
                    var productParts = sr.ReadLine().Split('|');
                    products.Add(productParts[0], new Product()
                    {
                        Id = Clean(productParts[0]),
                        Name = Clean(productParts[1]),
                        Description = Clean(productParts[2]),
                        Price = int.Parse(productParts[3]),
                        ImageUrl = Clean(productParts[4]),
                    });
                }
            }
            return products;
    }
}