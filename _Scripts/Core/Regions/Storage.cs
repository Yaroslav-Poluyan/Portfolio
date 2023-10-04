using System.Collections.Generic;
using _Scripts.ResorcesAndRecipes;

namespace _Scripts.Core.Regions
{
    public class Storage
    {
        public readonly Dictionary<ProductData, int> Products = new();

        public Storage()
        {
            foreach (var productData in ProductsManager.Instance.Products)
            {
                Products.Add(productData, 0);
            }
        }
    }
}