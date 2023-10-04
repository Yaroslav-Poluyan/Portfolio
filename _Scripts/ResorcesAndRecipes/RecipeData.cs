using System;
using System.Collections.Generic;
using UnityEngine;

namespace _Scripts.ResorcesAndRecipes
{
    [CreateAssetMenu(fileName = "Recipe", menuName = "Scriptable Objects/Recipe", order = 1)]
    [Serializable]
    public class RecipeData : ScriptableObject
    {
        public List<ProductAndCount> _inputItems;
        public List<ProductAndCount> _optionalInputItems;
        public List<ProductAndCount> _outputItems;
        public List<ProductAndCount> _factoryCreationItems;
        public List<ProductAndCount> _factoryUpgradeItems;
        public int MaxScale = 100;
        public int ID;

        [Serializable]
        public class ProductAndCount
        {
            public ProductData _productData;
            [Min(0)] public int _count;

            public ProductAndCount(ProductData productData, int countToBuy)
            {
                _productData = productData;
                _count = countToBuy;
            }

            public void Deconstruct(out ProductData product, out int count)
            {
                product = _productData;
                count = _count;
            }
        }
    }
}