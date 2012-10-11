using System.Collections.Generic;
using System.IO;
using System.Web;

namespace Errordite.Samples.Mvc3.Controllers
{
    public static class DataHelper
    {
        public static Product Get(string id)
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

            Product product;
            if (products.TryGetValue(id, out product))
                return product;

            return null;
            
        }

        private static string Clean(string s)
        {
            return s == "" ? null : s;
        }
    }
}