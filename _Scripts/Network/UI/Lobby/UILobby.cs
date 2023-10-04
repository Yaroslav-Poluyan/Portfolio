using System;
using System.Collections.Generic;
using System.Linq;
using _Scripts.Network.RoomSystem;
using Mirror;
using MoreMountains.Tools;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _Scripts.Network.UI.Lobby
{
    public class UILobby : MonoBehaviour
    {
        public static UILobby Instance { get; private set; }
        [Header("Lobby")] [SerializeField] private TextMeshProUGUI _lobbyNameText;
        [SerializeField] private TextMeshProUGUI _matchID;
        [SerializeField] private Button _beginGameButton;
        [SerializeField] private Transform _lobbyPlayersContainer;
        [SerializeField] private UIConnectedPlayerBlock _uiConnectedPlayerBlockPrefab;
        private readonly Dictionary<VacuumnajaRoomPlayer, UIConnectedPlayerBlock> _createdBlocks = new();
        private VacuumnajaRoomManager _vacuumnajaRoomManager;

        private void Awake()
        {
            _vacuumnajaRoomManager = NetworkManager.singleton as VacuumnajaRoomManager;
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
            }
            else
            {
                Instance = this;
            }
        }

        public void OnClientExitRoom(VacuumnajaRoomPlayer vacuumnajaRoomPlayer)
        {
            if (!_createdBlocks.ContainsKey(vacuumnajaRoomPlayer))
            {
                return;
            }

            foreach (var createdBlock in _createdBlocks.ToList())
            {
                if (_vacuumnajaRoomManager.roomSlots.Contains(createdBlock.Key))
                {
                    continue;
                }

                Destroy(createdBlock.Value.gameObject);
                _createdBlocks.Remove(createdBlock.Key);
            }
        }

        public void OnClientEnterRoom(VacuumnajaRoomPlayer vacuumnajaRoomPlayer)
        {
            if (_createdBlocks.ContainsKey(vacuumnajaRoomPlayer))
            {
                return;
            }

            var newBlock = Instantiate(_uiConnectedPlayerBlockPrefab, _lobbyPlayersContainer);
            newBlock.Initialize(vacuumnajaRoomPlayer);
            _createdBlocks.Add(vacuumnajaRoomPlayer, newBlock);
        }

        public void ShowStartGameButtonState(bool state)
        {
            if (VacuumnajaRoomPlayer.LocalPlayer.isServer)
            {
                _beginGameButton.gameObject.SetActive(state);
            }
        }
    }
}