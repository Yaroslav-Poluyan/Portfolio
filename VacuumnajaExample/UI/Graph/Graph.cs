using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace _Scripts.UI.Graph
{
    public class Graph<XType, YType> where XType : IComparable, IConvertible
        where YType : IComparable, IConvertible
    {
        public List<SubGraph> SubGraphs;
        public Vector2Int Resolution;
        public Color BackgroundColor;

        public class SubGraph
        {
            public List<(XType x, YType y)> Points;
            public Color LineColor;
            public int LineThickness;

            public SubGraph(List<(XType x, YType y)> points, Color lineColor, int lineThickness)
            {
                Points = points;
                LineColor = lineColor;
                LineThickness = lineThickness;
            }
        }

        public Graph(Vector2Int resolution, Color backgroundColor)
        {
            Resolution = resolution;
            BackgroundColor = backgroundColor;
            SubGraphs = new List<SubGraph>();
        }

        public void AddSubGraph(List<(XType x, YType y)> points, Color lineColor, int lineThickness)
        {
            SubGraphs.Add(new SubGraph(points, lineColor, lineThickness));
        }

        private void DrawLine(Color[] pixels, Vector2Int start, Vector2Int end, Color lineColor,
            int lineThickness)
        {
            DrawLine(pixels, start.x, start.y, end.x, end.y, lineColor, lineThickness);
        }

        private void DrawLine(Color[] pixels, int x1, int y1, int x2, int y2, Color lineColor,
            int lineThickness)
        {
            var dx = Math.Abs(x2 - x1);
            var dy = Math.Abs(y2 - y1);
            var sx = x1 < x2 ? 1 : -1;
            var sy = y1 < y2 ? 1 : -1;
            var err = dx - dy;
            var curWidth = Resolution.x;
            var curHeight = Resolution.y;

            while (true)
            {
                if (x1 >= 0 && x1 < curWidth && y1 >= 0 && y1 < curHeight)
                {
                    for (var t = 0; t < lineThickness; t++)
                    {
                        if (x1 + t < curWidth) // Check to avoid going out of bounds
                        {
                            pixels[(x1 + t) + y1 * curWidth] = lineColor;
                        }
                    }
                }

                if (x1 == x2 && y1 == y2)
                {
                    break;
                }

                var e2 = 2 * err;
                if (e2 > -dy)
                {
                    err -= dy;
                    x1 += sx;
                }

                if (e2 < dx)
                {
                    err += dx;
                    y1 += sy;
                }

                if (Math.Abs(x1 - x2) > curWidth || Math.Abs(y1 - y2) > curHeight)
                {
                    Debug.LogWarning("Unexpected data, exit loop to prevent infinite loop");
                    break;
                }
            }
        }

        public Texture2D Draw()
        {
            if (SubGraphs.Count == 0) return new Texture2D(Resolution.x, Resolution.y);
            if (!SubGraphs.SelectMany(subGraph => subGraph.Points).Any())
                return new Texture2D(Resolution.x, Resolution.y);
            var maxY = Convert.ToSingle(SubGraphs.SelectMany(subGraph => subGraph.Points).Max(point => point.y));
            var maxX = Convert.ToSingle(SubGraphs.SelectMany(subGraph => subGraph.Points).Max(point => point.x));
            var minY = Convert.ToSingle(SubGraphs.SelectMany(subGraph => subGraph.Points).Min(point => point.y));
            var minX = Convert.ToSingle(SubGraphs.SelectMany(subGraph => subGraph.Points).Min(point => point.x));
            var scale = Vector2.one;
            if (maxX - minX != 0) scale.x = Resolution.x / (maxX - minX);
            if (maxY - minY != 0) scale.y = Resolution.y / (maxY - minY);
            var texture = new Texture2D(Resolution.x, Resolution.y);
            var pixels = new Color[Resolution.x * Resolution.y];

            for (var i = 0; i < pixels.Length; i++)
            {
                pixels[i] = BackgroundColor;
            }

            foreach (var subGraph in SubGraphs)
            {
                for (var index = 1; index < subGraph.Points.Count; index++)
                {
                    var point1 = subGraph.Points[index - 1];
                    var point2 = subGraph.Points[index];
                    var start = new Vector2Int((int) ((Convert.ToSingle(point1.x) - minX) * scale.x),
                        (int) ((Convert.ToSingle(point1.y) - minY) * scale.y));
                    var end = new Vector2Int((int) ((Convert.ToSingle(point2.x) - minX) * scale.x),
                        (int) ((Convert.ToSingle(point2.y) - minY) * scale.y));
                    DrawLine(pixels, start, end, subGraph.LineColor, subGraph.LineThickness);
                }
            }

            texture.SetPixels(pixels);
            texture.Apply();
            return texture;
        }
    }
}