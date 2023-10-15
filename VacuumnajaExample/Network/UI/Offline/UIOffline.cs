using _Scripts.Technical.Saves;
using TMPro;
using UnityEngine;

namespace _Scripts.Network.UI.Offline
{
    public class UIOffline : MonoBehaviour
    {
        [SerializeField] private TMP_InputField _lobbyNameInputField;

        private void Start()
        {
            _lobbyNameInputField.onEndEdit.AddListener(OnLobbyNameChanged);
            _lobbyNameInputField.text =
                SaveManager.HasKey("PlayerName") ? SaveManager.Get<string>("PlayerName") : "Player";
        }

        private void OnLobbyNameChanged(string value)
        {
            SaveManager.Save("PlayerName", value);
        }
    }
}