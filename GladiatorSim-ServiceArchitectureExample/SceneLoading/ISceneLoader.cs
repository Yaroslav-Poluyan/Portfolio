using System;

namespace _Scripts.CodeBase.Infrastructure.SceneLoading
{
    public interface ISceneLoader
    {
        void Load(SceneType type, Action onLoaded = null);
    }
}