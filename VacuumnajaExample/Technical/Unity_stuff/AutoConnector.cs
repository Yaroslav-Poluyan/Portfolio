#if UNITY_EDITOR
using System;
using System.Threading.Tasks;
using _Scripts.Network.RoomSystem;
using Mirror;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace _Scripts.Technical.Unity_stuff
{
    public class AutoConnector : MonoBehaviour
    {
        private const float MaxWaitTime = 10f;
        private float _waitTime;

        private void Start()
        {
            Connect();
        }

        private async void Connect()
        {
            var autoconnect = EditorPrefs.GetBool("Autoconnect", false);
            if (!autoconnect)
            {
                return;
            }

            var isHost = EditorPrefs.GetBool("AutoconnectAsHost", false);
            if (isHost)
            {
                await Task.Delay(500);
                NetworkManager.singleton.StartHost();
                await Task.Delay(100);
                VacuumnajaRoomPlayer.LocalPlayer.CmdChangeReadyState(true);
                var vacuumnajaRoomManager = NetworkManager.singleton as VacuumnajaRoomManager;
                while (vacuumnajaRoomManager.roomSlots.Count < 2 && _waitTime < MaxWaitTime)
                {
                    await Task.Delay(100);
                    _waitTime += 0.1f;
                    if (SceneManager.GetActiveScene().name != "Lobby")
                    {
                        Debug.LogAssertion("AutoConnector: Scene changed. Aborting.");
                        return;
                    }
                }

                if (_waitTime >= MaxWaitTime)
                {
                    Debug.LogError("AutoConnector: Waited too long for other player to join. Aborting.");
                }
                else vacuumnajaRoomManager.StartGame();
            }
            else
            {
                await Task.Delay(1000);
                while (NetworkClient.connection == null && MaxWaitTime > _waitTime)
                {
                    await Task.Delay(100);
                    _waitTime += 0.1f;
                    NetworkManager.singleton.StartClient();
                    if (SceneManager.GetActiveScene().name != "Lobby")
                    {
                        Debug.LogAssertion("AutoConnector: Scene changed. Aborting.");
                        return;
                    }
                }

                if (_waitTime >= MaxWaitTime)
                {
                    Debug.LogError("AutoConnector: Waited too long for connection. Aborting.");
                }
                else VacuumnajaRoomPlayer.LocalPlayer.CmdChangeReadyState(true);
            }
        }
    }

    public class AutoConnectorWindow : EditorWindow
    {
        [MenuItem("Vacuumnaja/Auto Connector")]
        public static void ShowWindow()
        {
            var window = GetWindow<AutoConnectorWindow>("Auto Connector");
        }

        private void OnGUI()
        {
            var currentAutoconnectSavedState = EditorPrefs.GetBool("Autoconnect", false);
            var currentConnectAsHostSavedState = EditorPrefs.GetBool("AutoconnectAsHost", false);
            GUILayout.Label("Current Autoconnect: " + currentAutoconnectSavedState);
            if (GUILayout.Button("Switch"))
            {
                //toggle state
                EditorPrefs.SetBool("Autoconnect", !currentAutoconnectSavedState);
            }

            GUILayout.Label(currentConnectAsHostSavedState
                ? "Current Autoconnect type is host"
                : "Current Autoconnect type is client");

            EditorPrefs.SetBool("AutoconnectAsHost",
                GUILayout.Toggle(EditorPrefs.GetBool("AutoconnectAsHost", false), "Autoconnect as host"));
        }
    }
}
#endif