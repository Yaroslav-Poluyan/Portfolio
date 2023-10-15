using _Scripts.ResorcesAndRecipes;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _Scripts.UI.Stats.PricesSection
{
    public class UIProductPriceBlock : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _priceText;
        [SerializeField] private TextMeshProUGUI _titleText;
        [SerializeField] private Image _icon;
        public ProductData ProductData { get; private set; }

        public void Init(ProductData data, float price)
        {
            ProductData = data;
            _priceText.text = price.ToString("F5");
            _titleText.text = data._title;
            _icon.sprite = data._icon;
        }

        public void SetCount(float price)
        {
            _priceText.text = price.ToString("F5");
        }
    }
}