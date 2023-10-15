using _Scripts.Network.RoomSystem;
using Mirror;
using UnityEngine;
using UnityEngine.UI;

namespace _Scripts.Network.UI.Buttons
{
    public class HostButton : MonoBehaviour
    {
        private void Start()
        {
            var button = GetComponent<Button>();
            var roomManager = NetworkManager.singleton;
            button.onClick.AddListener(roomManager.StartHost);
        }
    }
}