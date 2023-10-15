using _Scripts.Core.Regions;
using _Scripts.UI.ResourcesAndProduction;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _Scripts.UI.Regions
{
    public class UIRegionBlock : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _regionNameText;
        public Region LinkedRegion { get; private set; }

        public void Init(Region region)
        {
            LinkedRegion = region;
            _regionNameText.text = region.Name;
        }

        public void OpenRegionInfo()
        {
            RegionsUI.Instance.OpenAtRegionInfo(LinkedRegion);
        }

        public void Refresh()
        {
        }
    }
}