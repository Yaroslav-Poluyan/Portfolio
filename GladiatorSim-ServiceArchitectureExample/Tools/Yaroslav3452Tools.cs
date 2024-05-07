#if UNITY_EDITOR
using _Scripts.CodeBase.Gameplay.Common;
using _Scripts.CodeBase.StaticData.PlayerProgressData;
using UnityEditor;
using UnityEngine;

namespace _Scripts.CodeBase.Technical.EditorUnilities
{
    public class Yaroslav3452Tools : EditorWindow
    {
        private int selectedTab = 0;
        private AnimationClip sourceClip;
        private AnimationClip targetClip;


        [MenuItem("Yaroslav3452/Tools")]
        public static void ShowWindow()
        {
            GetWindow<Yaroslav3452Tools>("Tools");
            if (!EditorPrefs.HasKey("DrawDebugTexts"))
            {
                EditorPrefs.SetBool("DrawDebugTexts", true);
            }

            if (!EditorPrefs.HasKey("InfiniteStaminaForPlayer"))
            {
                EditorPrefs.SetBool("InfiniteStaminaForPlayer", true);
            }

            if (!EditorPrefs.HasKey("SkipChecksForAbilities"))
            {
                EditorPrefs.SetBool("SkipChecksForAbilities", true);
            }
        }

        private void OnGUI()
        {
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            var tabs = new[] { "Debug", "Tab 2", "Tab 3", "AnimationsHelper" }; // Задаем названия вкладок
            selectedTab = GUILayout.Toolbar(selectedTab, tabs);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            switch (selectedTab)
            {
                case 0:
                    DrawTab1();
                    break;
                case 1:
                    DrawTab2();
                    break;
                case 2:
                    DrawTab3();
                    break;
                case 3:
                    DrawAnimationsHelperTab();
                    break;
            }
        }

        private void DrawAnimationsHelperTab()
        {
            GUILayout.Label("Animations Helper");

            sourceClip =
                EditorGUILayout.ObjectField("Source Animation Clip", sourceClip, typeof(AnimationClip), true) as
                    AnimationClip;
            targetClip =
                EditorGUILayout.ObjectField("Target Animation Clip", targetClip, typeof(AnimationClip), true) as
                    AnimationClip;

            EditorGUILayout.Space();

            if (GUILayout.Button("Copy Events"))
            {
                if (sourceClip == null || targetClip == null)
                {
                    Debug.LogError("Please assign both source and target animation clips.");
                    return;
                }

                CopyAnimationEvents(sourceClip, targetClip);
                sourceClip = null;
                targetClip = null;
                Debug.Log("Animation events copied successfully.");
            }
        }

        private void CopyAnimationEvents(AnimationClip source, AnimationClip target)
        {
            AnimationEvent[] sourceEvents = AnimationUtility.GetAnimationEvents(source);
            AnimationUtility.SetAnimationEvents(target, sourceEvents);
            //save changes
            EditorUtility.SetDirty(target);
        }

        private void DrawTab1()
        {
            GUILayout.Label("Debug");
            // Вставьте код для первой вкладки
            GUILayout.Label("Draw Debug texts and figures?");
            var debugTextsStatus = EditorPrefs.GetBool("DrawDebugTexts") ? "Disable" : "Enable";
            if (GUILayout.Button(debugTextsStatus))
            {
                //toggle state
                EditorPrefs.SetBool("DrawDebugTexts", !EditorPrefs.GetBool("DrawDebugTexts"));
            }

            var infiniteStaminaStatus = EditorPrefs.GetBool("InfiniteStaminaForPlayer") ? "Disable" : "Enable";
            GUILayout.Label("Infinite stamina for player?");
            if (GUILayout.Button(infiniteStaminaStatus))
            {
                //toggle state
                EditorPrefs.SetBool("InfiniteStaminaForPlayer", !EditorPrefs.GetBool("InfiniteStaminaForPlayer"));
            }

            var skipChecksForAbilitiesStatus = EditorPrefs.GetBool("SkipChecksForAbilities") ? "Disable" : "Enable";
            GUILayout.Label("Skip checks for abilities?");
            if (GUILayout.Button(skipChecksForAbilitiesStatus))
            {
                //toggle state
                EditorPrefs.SetBool("SkipChecksForAbilities", !EditorPrefs.GetBool("SkipChecksForAbilities"));
            }
        }

        private void DrawTab2()
        {
            GUILayout.Label("Money etc.");
            if (GUILayout.Button("Add 1k money"))
            {
                PlayerProgressData.DebugAddMoney(1000);
            }

            if (GUILayout.Button("Add 10k money"))
            {
                PlayerProgressData.DebugAddMoney(10000);
            }
        }

        private void DrawTab3()
        {
            GUILayout.Label("Abilities");
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Increase Rage Level"))
            {
                PlayerProgressData.DebugIncreaseAbilityLevel(AbilityType.Rage);
            }

            if (GUILayout.Button("Decrease Rage Level"))
            {
                PlayerProgressData.DebugDecreaseAbilityLevel(AbilityType.Rage);
            }

            GUILayout.EndHorizontal();
        }
    }
}
#endif