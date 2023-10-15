using _Scripts.Interfaces;
using UnityEngine;
using UnityEngine.UI;

namespace _Scripts.UI.Common
{
    public class CloseButton : MonoBehaviour
    {
        private Button _button;

        private void Start()
        {
            _button = GetComponent<Button>();
            _button.onClick.AddListener(OnButtonClick);
        }

        private void OnButtonClick()
        {
            var ui = GetComponentInParent<IUI>();
            ui.Close();
        }
    }
}