using UnityEngine;
using UnityEngine.UI;

namespace _Scripts.UI.ResourcesAndProduction.UIBlocks
{
    public class UICreateNewProductionBlock : MonoBehaviour
    {
        [SerializeField] private Button _button;
        private UIRegionProductionBlock _linkedRegionProductionBlock;

        public void Init(UIRegionProductionBlock regionProductionBlock)
        {
            _linkedRegionProductionBlock = regionProductionBlock;
            _button.onClick.AddListener(OnButtonClick);
        }

        private void OnButtonClick()
        {
            RegionsUI.Instance.UIRegionInfo.OpenNewProductionCreationPanel();
        }
    }
}