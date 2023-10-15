using System;
using _Scripts.Network.RoomSystem;
using Mirror;
using UnityEngine;
using UnityEngine.UI;

namespace _Scripts.Network.UI.Buttons
{
    public class HostStartGameButton : MonoBehaviour
    {
        private void Start()
        {
            var vacuumnajaRoomManager = NetworkManager.singleton as VacuumnajaRoomManager;
            GetComponent<Button>().onClick.AddListener(() => vacuumnajaRoomManager.StartGame());
        }
    }
}