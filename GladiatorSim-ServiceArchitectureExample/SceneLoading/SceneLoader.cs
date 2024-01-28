using System;
using System.Collections;
using _Scripts.CodeBase.Services.Curtain;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace _Scripts.CodeBase.Infrastructure.SceneLoading
{
    public class SceneLoader : ISceneLoader
    {
        private readonly ICoroutineRunner _coroutineRunner;
        private readonly SceneLoaderReferencesSO _sceneLoaderReferencesSO;
        private readonly IProgressCurtain _progressCurtain;

        public SceneLoader(ICoroutineRunner coroutineRunner, SceneLoaderReferencesSO sceneLoaderReferencesSO,
            IProgressCurtain progressCurtain)
        {
            _coroutineRunner = coroutineRunner;
            _sceneLoaderReferencesSO = sceneLoaderReferencesSO;
            _progressCurtain = progressCurtain;
        }

        private void Load(string name, Action onLoaded = null)
        {
            _coroutineRunner.StartCoroutine(LoadScene(name, onLoaded));
        }

        public void Load(SceneType type, Action onLoaded = null)
        {
            if (type == SceneType.None)
            {
                throw new ArgumentException("SceneType.None is not allowed");
            }

            if (!_sceneLoaderReferencesSO.Scenes.ContainsKey(type))
            {
                throw new ArgumentException($"SceneType {type} is not registered in {nameof(SceneLoaderReferencesSO)}");
            }

            var scene = _sceneLoaderReferencesSO.Scenes[type];
            Load(scene.ScenePath, onLoaded);
        }

        private IEnumerator LoadScene(string nextScene, Action onLoaded = null)
        {
            if (SceneManager.GetActiveScene().name == nextScene)
            {
                onLoaded?.Invoke();
                yield break;
            }

            AsyncOperation waitNextScene = SceneManager.LoadSceneAsync(nextScene);

            while (!waitNextScene.isDone)
            {
                _progressCurtain.SetProgress(waitNextScene.progress);
                yield return null;
            }

            onLoaded?.Invoke();
        }
    }
}