using System;
using System.Globalization;
using _Scripts.Network.Client;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _Scripts.UI.InGameUI
{
    public class UIGameSpeedBlock : MonoBehaviour
    {
        [SerializeField] private Client _client;
        [SerializeField] private Slider _gameSpeedSlider;
        [SerializeField] private TextMeshProUGUI _gameSpeedText;
        [SerializeField] private Button _pauseButton;
        [SerializeField] private Button _decreaseSpeedButton;
        [SerializeField] private Button _increaseSpeedButton;

        private void Start()
        {
            _client.OnSpeedChangedEvent += OnSpeedChangedHandler;
            _gameSpeedText.text = _client.GameSpeed[_client._currentSpeedIndex].ToString(CultureInfo.InvariantCulture);
            _gameSpeedSlider.maxValue = _client.GameSpeed.Length - 1;
            _gameSpeedSlider.value = _client._currentSpeedIndex;
        }

        private void OnSpeedChangedHandler()
        {
            _gameSpeedText.text = _client.GameSpeed[_client._currentSpeedIndex].ToString(CultureInfo.InvariantCulture);
            _gameSpeedSlider.value = _client._currentSpeedIndex;
        }

        public void RequestSpeedDecrease()
        {
            Client.Instance.CmdGameSpeedDecrease();
        }

        public void RequestSpeedIncrease()
        {
            Client.Instance.CmdGameSpeedIncrease();
        }

        public void RequestSwitchPause()
        {
            var currentpause = Client.Instance._isPaused;
            if (currentpause)
            {
                Client.Instance.CmdGameSpeedContinue();
                _pauseButton.GetComponentInChildren<TextMeshProUGUI>().text = "| |";
            }
            else
            {
                Client.Instance.CmdGameSpeedPause();
                _pauseButton.GetComponentInChildren<TextMeshProUGUI>().text = ">";
            }
        }

        public void RequestContinue()
        {
            Client.Instance.CmdGameSpeedContinue();
        }
    }
}