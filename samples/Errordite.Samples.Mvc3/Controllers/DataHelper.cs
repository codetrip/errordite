using System.Collections.Generic;
using System.IO;
using System.Web;
using System.Linq;
using Massive;

namespace Errordite.Samples.Mvc3.Controllers
{
    public class Products : DynamicModel
    {
        public Products() 
            : base("ErrorditeTest", "Products", "Id")
        {
        }
    }


    public static class DataHelper
    {
        public static Product Get(string id)
        {
            dynamic table = new Products();
            dynamic products = table.Find(Id: id);

            foreach(var p in products)
            return GetProduct(p);

            return null;
            //return All().TryGetValue(id, out product) ? product : null;
        }

        private static Product GetProduct(dynamic p)
        {
            return new Product()
                {
                    Description = p.Description,
                    Id = p.Id,
                    ImageUrl = p.ImageUrl,
                    Name = p.Name,
                    Price = p.Price,
                };
        }

        private static string Clean(string s)
        {
            return s == "" ? null : s;
        }

        public static IEnumerable<Product> MoreLike(string id)
        {
            dynamic table = new Products();

            var products = table.All(where: "WHERE id != @0", args: id);

            foreach (var p in products)
            {
                yield return GetProduct(p);
            }

            //return All().Where(p => p.Key != id).Select(p => p.Value);
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
}