#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace _Scripts.CodeBase.Technical.EditorUnilities
{
    [InitializeOnLoad]
    public static class DefaultSceneLoader
    {
        static DefaultSceneLoader()
        {
            EditorApplication.playModeStateChanged += LoadDefaultScene;
        }

        private static void LoadDefaultScene(PlayModeStateChange state)
        {
            if (!EditorApplication.isPlaying)
            {
                return;
            }

            if (SceneManager.GetActiveScene().buildIndex == 0)
            {
                return;
            }

            var loadDefaultSceneOnPlay = true;
            if (EditorPrefs.HasKey("LoadDefaultSceneOnPlay"))
            {
                loadDefaultSceneOnPlay = EditorPrefs.GetBool("LoadDefaultSceneOnPlay");
            }

            if (!loadDefaultSceneOnPlay)
            {
                return;
            }

            switch (state)
            {
                case PlayModeStateChange.ExitingEditMode:
                    EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
                    break;
                case PlayModeStateChange.EnteredPlayMode:
                    Debug.Log("<color=green>DefaultSceneLoader: </color>Load default scene");
                    SceneManager.LoadScene(0);
                    Debug.ClearDeveloperConsole();
                    break;
                case PlayModeStateChange.EnteredEditMode:
                    break;
                case PlayModeStateChange.ExitingPlayMode:
                    break;
                default:
                    throw new System.ArgumentOutOfRangeException(nameof(state), state, null);
            }
        }
    }

    public class DefaultSceneLoaderWindow : EditorWindow
    {
        private static readonly List<EditorBuildSettingsScene> ScenesInBuild = new();

        [MenuItem("Yaroslav3452/Default Scene Loader")]
        public static void ShowWindow()
        {
            GetWindow<DefaultSceneLoaderWindow>("Default Scene Loader");
            RefreshSceneList();
        }

        private static void RefreshSceneList()
        {
            ScenesInBuild.Clear();
            var scenes = EditorBuildSettings.scenes;
            foreach (var scene in scenes)
            {
                ScenesInBuild.Add(scene);
            }
        }

        private void OnGUI()
        {
            RefreshSceneList();
            //current isLoadDefaultScene variable state
            var currentSavedState = EditorPrefs.GetBool("LoadDefaultSceneOnPlay");
            GUILayout.Label("Current Load default scene on play: " + currentSavedState);
            if (GUILayout.Button("Switch"))
            {
                //toggle state
                EditorPrefs.SetBool("LoadDefaultSceneOnPlay", !currentSavedState);
            }

            GUILayout.Label("Select scene to load:");
            var activeScenePath = SceneManager.GetActiveScene().path;
            foreach (var scene in ScenesInBuild)
            {
                var sceneName = System.IO.Path.GetFileNameWithoutExtension(scene.path);
                var isActiveScene = scene.path == activeScenePath;
                GUI.backgroundColor = isActiveScene ? Color.green : GUI.backgroundColor;
                GUILayout.BeginHorizontal();

                if (GUILayout.Button(sceneName))
                {
                    LoadScene(scene);
                }

                bool isThisSceneSetToLoad = EditorPrefs.GetString("SkipStatesArena") == sceneName;
                GUI.backgroundColor = isThisSceneSetToLoad ? Color.green : Color.white;

                if (sceneName.Contains("Arena"))
                {
                    if (GUILayout.Button("SetToLoad"))
                    {
                        EditorPrefs.SetString("SkipStatesArena", sceneName);
                    }
                }

                GUILayout.EndHorizontal();


                GUI.backgroundColor = Color.white;
            }
        }

        private void LoadScene(EditorBuildSettingsScene scene)
        {
            if (EditorApplication.isPlaying)
            {
                Debug.LogError("Cannot load scenes while in play mode.");
                return;
            }

            EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
            EditorSceneManager.OpenScene(scene.path);
        }
    }
}
#endif