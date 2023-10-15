using System;
using System.Threading.Tasks;
using _Scripts.Network.RoomSystem;
using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _Scripts.Network.UI.Lobby
{
    public class UIConnectedPlayerBlock : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _playerNameText;
        [SerializeField] private TextMeshProUGUI _pingText;
        [SerializeField] private TextMeshProUGUI _countryText;
        [SerializeField] private Image _readyImage;
        [SerializeField] private Image _notReadyImage;
        [SerializeField] private Button _kickButton;
        [SerializeField] private Button _readyButton;
        [SerializeField] private Image _backgroundImage;
        public VacuumnajaRoomPlayer _linkedPlayer;

        public void Initialize(VacuumnajaRoomPlayer roomPlayer)
        {
            _linkedPlayer = roomPlayer;
            _playerNameText.text = roomPlayer._playerName;
            if (roomPlayer.isLocalPlayer)
            {
                _playerNameText.color = Color.green;
                _readyButton.interactable = true;
                _kickButton.gameObject.SetActive(false);
            }
            else if (roomPlayer.isServer)
            {
                _readyButton.interactable = false;
                _kickButton.gameObject.SetActive(true);
            }
            else
            {
                _readyButton.interactable = false;
                _kickButton.gameObject.SetActive(false);
            }

            SetReadyState(roomPlayer.readyToBegin);
            roomPlayer.OnReadyStateChanged += SetReadyState;
        }

        private void OnDestroy()
        {
            _linkedPlayer.OnReadyStateChanged -= SetReadyState;
        }

        public void SwitchReadyStateButtonPressedHandler()
        {
            if (!_linkedPlayer.isLocalPlayer) throw new Exception("Only local player can change ready state");
            _linkedPlayer.CmdChangeReadyState(!_linkedPlayer.readyToBegin);
        }

        public void KickButtonPressedHandler()
        {
            _linkedPlayer.GetComponent<NetworkIdentity>().connectionToClient.Disconnect();
        }

        private void SetReadyState(bool newReadyState)
        {
            _readyImage.gameObject.SetActive(newReadyState);
            _notReadyImage.gameObject.SetActive(!newReadyState);
        }
    }
}