using System;
using UnityEngine;

namespace _Scripts.ResorcesAndRecipes
{
    [CreateAssetMenu(fileName = "Product", menuName = "Scriptable Objects/Product", order = 0)]
    [Serializable]
    public class ProductData : ScriptableObject
    {
        public string _title;
        public Sprite _icon;

        [ExecuteAlways]
        private void Awake()
        {
            _title = name;
        }
    }
}