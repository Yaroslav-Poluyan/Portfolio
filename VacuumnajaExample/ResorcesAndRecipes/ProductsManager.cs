using System.Collections.Generic;
using System.Linq;
using MoreMountains.Tools;
using UnityEngine;

namespace _Scripts.ResorcesAndRecipes
{
    public class ProductsManager : MMSingleton<ProductsManager>
    {
        [field: SerializeField] public List<ProductData> Products { get; private set; }
#if UNITY_EDITOR
        [SerializeField] private bool _getProductsFromResources;
        private void OnValidate()
        {
            if (_getProductsFromResources)
            {
                Products = Resources.LoadAll<ProductData>("Products").ToList();
                _getProductsFromResources = false;
            }
        }
#endif
        public ProductData GetProductData(string productName)
        {
            return Products.FirstOrDefault(product => product._title == productName);
        }
    }
}