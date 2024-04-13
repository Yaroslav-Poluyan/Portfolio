using System.Collections.Generic;
using _Scripts.CodeBase.Technical;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;

namespace _Scripts.CodeBase.Infrastructure.SceneLoading
{
    [CreateAssetMenu(fileName = "SceneReferencesSO", menuName = "Data/Scenes", order = 0)]
    public class SceneLoaderReferencesSO : SerializedScriptableObject
    {
        [OdinSerialize] public Dictionary<SceneType, SceneReference> Scenes { get; private set; } = new();
    }

    public enum SceneType
    {
        None = 0,
        Bootstrapper = 1,
        InitialLoading = 2,
        MainMenu = 3,
        Settings = 5,
        Camp = 6,
        GladiatorChoose = 8,
        BattleChoose = 9,
        AbilityChoose = 10,
        GladiatorForBattleChoose = 11,
        Arena1 = 21,
        Arena2 = 22,
        Arena3 = 23,
        Arena4 = 24,
        Arena5 = 25,
    }
}