using _Scripts.ResorcesAndRecipes;
using _Scripts.Stats;
using TMPro;
using UnityEngine;

namespace _Scripts.UI.Stats
{
    public class UIProductStatsBlock : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _productNameText;
        [SerializeField] private TextMeshProUGUI _producedCountText;
        [SerializeField] private TextMeshProUGUI _consumedCountText;
        [SerializeField] private TextMeshProUGUI _soldCountText;
        [SerializeField] private TextMeshProUGUI _averagePriceText;
        public ProductData LinkedProduct { get; private set; }

        public void Init(ProductData productData, StatsManager.ProductTickStat productTickStat)
        {
            LinkedProduct = productData;
            Refresh(productTickStat);
        }

        public void Refresh(StatsManager.ProductTickStat productTickStat)
        {
            _productNameText.text = LinkedProduct.name;
            _producedCountText.text = productTickStat.ProducedCount.ToString();
            _consumedCountText.text = productTickStat.ConsumedCount.ToString();
            _soldCountText.text = productTickStat.SoldCount.ToString();
            _averagePriceText.text = productTickStat.AverageSellPrice.ToString("E3");
        }
    }
}