using System;
using _Scripts.Network.UI.Lobby;
using _Scripts.Technical.Saves;
using Mirror;

namespace _Scripts.Network.RoomSystem
{
    public class VacuumnajaRoomPlayer : NetworkRoomPlayer
    {
        public event Action<bool> OnReadyStateChanged;
        public static VacuumnajaRoomPlayer LocalPlayer;
        [SyncVar] public string _playerName;

        private void Awake()
        {
            _playerName = SaveManager.Get<string>("PlayerName");
        }

        public override void Start()
        {
            base.Start();
            if (isLocalPlayer)
            {
                LocalPlayer = this;
            }
        }

        public override void OnClientEnterRoom()
        {
            base.OnClientEnterRoom();
            UILobby.Instance.OnClientEnterRoom(this);
        }

        public override void OnClientExitRoom()
        {
            base.OnClientExitRoom();
            UILobby.Instance.OnClientExitRoom(this);
        }

        public override void ReadyStateChanged(bool oldReadyState, bool newReadyState)
        {
            OnReadyStateChanged?.Invoke(newReadyState);
        }
    }
}