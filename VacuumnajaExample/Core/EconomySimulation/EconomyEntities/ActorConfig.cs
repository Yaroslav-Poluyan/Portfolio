using UnityEngine;

namespace _Scripts.Core.EconomySimulation.EconomyEntities
{
    [CreateAssetMenu(fileName = "ActorConfig", menuName = "EconomySimulation/Configs", order = 0)]
    public class ActorConfig : ScriptableObject
    {
        public float _sellPriceMultiplyer = 1.05f;
        public float _buyPriceMultiplyer = 1.05f;
        public PriceChangeType _priceChangeType;
        public float _sellPriceAddition = .05f;
        public float _buyPriceAddition = .05f;
        public float _scaleChangeChance = .1f;
        public float _addNewEconomyEntityChance = .1f;
        public int _scaleChange = 1;

        public enum PriceChangeType
        {
            Linear,
            Exponential
        }
    }
}