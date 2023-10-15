using _Scripts.ResorcesAndRecipes;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _Scripts.UI.ResourcesAndProduction.UIBlocks
{
    public class UIProductItemBlock : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _countText;
        [SerializeField] private TextMeshProUGUI _titleText;
        [SerializeField] private Image _icon;
        public ProductData ProductData { get; private set; }

        public void Init(ProductData data, int count)
        {
            ProductData = data;
            _countText.text = count.ToString();
            _titleText.text = data._title;
            _icon.sprite = data._icon;
        }

        public void SetCount(int value)
        {
            _countText.text = value.ToString();
        }
    }
}