using System;
using System.Collections.Generic;

namespace Errordite.Samples.Mvc3.Controllers
{
    public static class BasketHelper
    {
        private static readonly List<Product> _basket = new List<Product>();

        public static bool Add(Product product)
        {
            if (product.Description == null)
            {
                throw new InvalidOperationException("Product description cannot be null");
            }

            if (product.Price == 0)
            {
                throw new InvalidOperationException("Product price cannot be 0");
            }

            _basket.Add(product);

            return true;
        }

        public static List<Product> Get()
        {
            return _basket;
        }
    }
}