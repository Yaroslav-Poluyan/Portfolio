using System;
using _Scripts.ResorcesAndRecipes;
using UnityEngine;

namespace _Scripts.Core.Regions
{
    [Serializable]
    public class Production
    {
        public static int IDCounter = 0;
        public int ID{ get; private set;}
        public Action OnProductionChangedEvent;
        [SerializeField] private int _scale;
        [SerializeField] private RecipeData _recipeData;

        ~Production()
        {
            OnProductionChangedEvent = null;
        }

        public int Scale
        {
            get => _scale;
            set
            {
                if (_scale != value)
                {
                    _scale = Mathf.Clamp(value, 0, RecipeData.MaxScale);
                    OnProductionChangedEvent?.Invoke();
                }
            }
        }

        public int MaxScale => RecipeData.MaxScale;

        public RecipeData RecipeData
        {
            get => _recipeData;
            set
            {
                if (_recipeData != value)
                {
                    _recipeData = value;
                    OnProductionChangedEvent?.Invoke();
                }
            }
        }

        public bool IsMaxScale => Scale == MaxScale;

        public Production(RecipeData recipeData, int scale = 1)
        {
            _scale = scale;
            _recipeData = recipeData;
            ID = IDCounter++;
        }
    }
}