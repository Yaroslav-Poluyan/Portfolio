using UnityEngine;

namespace _Scripts.Core.EconomySimulation.Pops
{
    [CreateAssetMenu(fileName = "PopsConfig", menuName = "EconomySimulation/Configs", order = 0)]
    public class PopsConfig : ScriptableObject
    {
        public float _growthRate = 1.05f;
        public float _deathRate = 0.05f;
        public float _populationPerScale = 100;
    }
}