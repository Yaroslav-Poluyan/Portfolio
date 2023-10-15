using System.Collections.Generic;
using System.Linq;
using _Scripts.Core.Regions;
using PlanetGeneratorPackage.Scripts;
using UnityEngine;

namespace _Scripts.Core.World
{
    public class WorldManager : MonoBehaviour
    {
        public static WorldManager Instance { get; private set; }
        private Planet _planet;
        private RegionMarker _regionMarker;

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            var go = new GameObject("MarkingHelper");
            _regionMarker = go.AddComponent<RegionMarker>();
        }

        public void InitWorld()
        {
            var planet = PlanetGenerator.Instance.GeneratePlanet();
            planet.transform.SetParent(transform);
            _planet = planet;
            RegionsManager.Instance.AddPlanetRegions(planet);
            _regionMarker.Init(_planet);
        }

        public Planet.PolygonData GetRegionPolygonData(int regionID)
        {
            return _planet.Polygons[regionID];
        }

        public void MarkRegion(int regionID)
        {
            if (regionID == -1)
            {
                _regionMarker.MarkRegion(new List<Vector3>());
                return;
            }
            var polygon = _planet.Polygons[regionID];
            var vertices = GetVerticesByIDs(polygon.Vertices);
            _regionMarker.MarkRegion(vertices.ToList());
        }

        private Vector3[] GetVerticesByIDs(List<int> verticesIDs)
        {
            var vertices = new Vector3[verticesIDs.Count];
            for (int i = 0; i < verticesIDs.Count; i++)
            {
                vertices[i] = _planet.Vertices[verticesIDs[i]];
            }

            return vertices;
        }

        private int[] GetTrianglesByIDs(List<int> verticesIDs)
        {
            var triangles = new int[verticesIDs.Count];
            for (int i = 0; i < verticesIDs.Count; i++)
            {
                triangles[i] = _planet.Triangles[verticesIDs[i]];
            }

            return triangles;
        }

        private Planet.PolygonData GetPolygonDataByRegionID(int regionID)
        {
            return _planet.Polygons[regionID];
        }
    }
}