using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using _Scripts.ResorcesAndRecipes;
using _Scripts.Stats;
using ChartAndGraph;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _Scripts.UI.Stats.Graph
{
    public class UIGraph : MonoBehaviour
    {
        [SerializeField] private GraphType _graphType;
        [SerializeField] private Button _switchVisibilityButtonPrefab;
        [SerializeField] private Transform _switchVisibilityButtonsLayoutGroup;
        [SerializeField] private GraphChart _graph;
        [SerializeField] private Material _lineMaterial;
        [SerializeField] private double _lineThickness = 0.1f;
        [SerializeField] private MaterialTiling _lineTiling;
        [SerializeField] private Material _fillMaterial;
        [SerializeField] private bool _stretchFill;
        [SerializeField] private Material _pointMaterial;
        [SerializeField] private float _pointSize = 0.1f;
        [SerializeField] private ChartItemEffect _pointHoverPrefab;
        [SerializeField] private ChartItemEffect _lineHoverPrefab;
        private Dictionary<ProductData, bool> _categoryVisibility = new();
        private readonly List<Button> _switchVisibilityButtons = new();
        private int _drawingTicksCount = 50;

        private readonly List<Color> _colorsForProducts = new()
        {
            Color.red,
            Color.green,
            Color.blue,
            Color.yellow,
            Color.cyan,
            Color.magenta,
            Color.gray,
            Color.black,
            Color.white,
        };

        private enum GraphType
        {
            AverageSellPrice = 0,
            Production = 1,
            Population = 2,
            Satisfaction = 3,
            StorageFill = 4,
        }

        private static readonly int From = Shader.PropertyToID("_ColorFrom");
        private static readonly int To = Shader.PropertyToID("_ColorTo");
        private Coroutine _redrawCoroutine;

        private void Awake()
        {
            for (var index = 0; index < ProductsManager.Instance.Products.Count; index++)
            {
                var product = ProductsManager.Instance.Products[index];
                //color init
                var newMaterial = new Material(_lineMaterial);
                var newFromColor = Color.Lerp(_colorsForProducts[index], Color.white, 0.5f);
                var newToColor = _colorsForProducts[index];
                newMaterial.SetColor(From, newFromColor);
                newMaterial.SetColor(To, newToColor);
                newMaterial.name = product.name + " material";
                //
                _graph.DataSource.AddCategory(product.name, newMaterial, _lineThickness, _lineTiling,
                    _fillMaterial, _stretchFill, _pointMaterial, _pointSize,
                    _lineHoverPrefab, _pointHoverPrefab);
                //button creation
                var button = Instantiate(_switchVisibilityButtonPrefab, _switchVisibilityButtonsLayoutGroup);
                button.GetComponentInChildren<TextMeshProUGUI>().text = product.name;
                _switchVisibilityButtons.Add(button);
                button.image.color = _colorsForProducts[index];
                var copyIndex = index;
                button.onClick.AddListener(() => SwitchCategoryVisibilityButtonPressedHandler(copyIndex));
                _categoryVisibility.Add(product, true);
            }
        }

        private void OnEnable()
        {
            if (_redrawCoroutine != null)
            {
                StopCoroutine(_redrawCoroutine);
                _redrawCoroutine = null;
            }

            _redrawCoroutine ??= StartCoroutine(RedrawCoroutine());
        }

        private void OnDisable()
        {
            if (_redrawCoroutine != null)
            {
                StopCoroutine(_redrawCoroutine);
                _redrawCoroutine = null;
            }
        }

        public void SetDrawingTicksCount(int count)
        {
            _drawingTicksCount = count;
            Redraw();
        }

        private void SwitchCategoryVisibilityButtonPressedHandler(int idx)
        {
            var newVisibilityState = !_categoryVisibility[ProductsManager.Instance.Products[idx]];
            _categoryVisibility[ProductsManager.Instance.Products[idx]] = newVisibilityState;
            _switchVisibilityButtons[idx].image.color = newVisibilityState ? _colorsForProducts[idx] : Color.gray;
            if (!newVisibilityState)
            {
                _graph.DataSource.ClearCategory(ProductsManager.Instance.Products[idx].name);
            }
            else
            {
                Redraw();
            }
        }

        private IEnumerator RedrawCoroutine()
        {
            var delay = new WaitForSeconds(.5f);
            while (this)
            {
                Redraw();
                yield return delay;
            }
        }

        private void Redraw()
        {
            var stats = StatsManager.Instance.Stats;
            _graph.DataSource.StartBatch();
            foreach (var product in ProductsManager.Instance.Products)
            {
                if (stats.Any(stat => !stat.ProductsTickStat.ContainsKey(product))) continue;
                var datas = _graphType switch
                {
                    GraphType.AverageSellPrice => stats.Select(stat => stat.ProductsTickStat[product].AverageSellPrice)
                        .ToList(),
                    GraphType.Production => stats.Select(stat => (float) stat.ProductsTickStat[product].ProducedCount)
                        .ToList(),
                    GraphType.Population => StatsManager.Instance.Stats.Select(stat => stat.Population).ToList(),
                    GraphType.Satisfaction => StatsManager.Instance.Stats.Select(stat => stat.SatisfyLevel).ToList(),
                    GraphType.StorageFill => stats
                        .Select(stat => stat.ProductsTickStat[product].AverageStorageFillPercent)
                        .ToList(),
                    _ => throw new ArgumentOutOfRangeException()
                };

                if (datas.Count > _drawingTicksCount)
                {
                    datas = datas.Skip(Math.Max(0, datas.Count - _drawingTicksCount)).ToList();
                }

                _graph.DataSource.ClearCategory(product.name);
                if (!_categoryVisibility[product]) continue;
                for (var i = 0; i < datas.Count; i++)
                {
                    _graph.DataSource.AddPointToCategory(product.name, i, datas[i]);
                }
            }

            _graph.DataSource.EndBatch();
        }
    }
}