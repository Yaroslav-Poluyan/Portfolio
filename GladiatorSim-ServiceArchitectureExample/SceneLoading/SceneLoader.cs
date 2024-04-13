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
        private readonly IGameLoadCurtain _gameLoadProgressCurtain;
        private readonly ISceneLoadingCurtain _sceneLoadingCurtain;

        public SceneLoader(ICoroutineRunner coroutineRunner, SceneLoaderReferencesSO sceneLoaderReferencesSO,
            IGameLoadCurtain gameLoadProgressCurtain, ISceneLoadingCurtain sceneLoadingCurtain)
        {
            _coroutineRunner = coroutineRunner;
            _sceneLoaderReferencesSO = sceneLoaderReferencesSO;
            _gameLoadProgressCurtain = gameLoadProgressCurtain;
            _sceneLoadingCurtain = sceneLoadingCurtain;
        }

        public void Load(SceneType type, Action onLoaded = null, Action allowSceneActivation = null)
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
            Load(scene.ScenePath, onLoaded, allowSceneActivation);
        }

        private void Load(string name, Action onLoaded = null, Action allowSceneActivation = null)
        {
            _coroutineRunner.StartCoroutine(LoadScene(name, onLoaded, allowSceneActivation));
        }

        private IEnumerator LoadScene(string nextScene, Action onLoaded = null, Action allowSceneActivation = null)
        {
            if (SceneManager.GetActiveScene().name == nextScene)
            {
                onLoaded?.Invoke();
                yield break;
            }

            AsyncOperation waitNextScene = SceneManager.LoadSceneAsync(nextScene);


            if (allowSceneActivation != null)
            {
                waitNextScene.allowSceneActivation = false;
                allowSceneActivation += () => waitNextScene.allowSceneActivation = true;
            }

            while (!waitNextScene.isDone)
            {
#if UNITY_EDITOR
                //Debug.Log("<color=green>Loading progress: " + waitNextScene.progress + "</color>");
#endif
                _gameLoadProgressCurtain.SetProgress(waitNextScene.progress);
                _sceneLoadingCurtain.SetProgress(waitNextScene.progress);
                yield return null;
            }

            _gameLoadProgressCurtain.SetProgress(1f);
            onLoaded?.Invoke();
        }
    }
}