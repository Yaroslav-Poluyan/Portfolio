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
        Game = 4,
    }
}