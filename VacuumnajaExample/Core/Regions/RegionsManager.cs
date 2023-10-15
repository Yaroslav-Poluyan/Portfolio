using System.Collections.Generic;
using System.Linq;
using _Scripts.Core.World;
using _Scripts.UI.ResourcesAndProduction;
using PlanetGeneratorPackage.Scripts;
using UnityEditor.PackageManager;
using UnityEngine;
using Client = _Scripts.Network.Client.Client;

// ReSharper disable InconsistentNaming

namespace _Scripts.Core.Regions
{
    public class RegionsManager : MonoBehaviour
    {
        public static RegionsManager Instance { get; private set; }
        public List<Region> Regions { get; private set; } = new();

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        public Region GetRegion(int regionID)
        {
            if (regionID >= Regions.Count || regionID < 0 || Regions[regionID].ID != regionID)
            {
                return Regions.Find(region => region.ID == regionID);
            }

            return Regions[regionID];
        }

        public bool TryGetRegion(int regionID, out Region region)
        {
            region = GetRegion(regionID);
            return region != null;
        }

        public List<Region> GetPlayerRegions(uint localPlayerNetId)
        {
            return Regions.Where(region => region.OwnerID == localPlayerNetId).ToList();
        }

        #region RegionsInitializing

        public void AddPlanetRegions(Planet planet)
        {
            var newRegions = new Region[planet.Polygons.Count];
            for (int i = 0; i < newRegions.Length; i++)
            {
                newRegions[i] = new Region(Regions.Count + i, Client.Instance.LocalPlayer.netId);
            }

            for (int i = 0; i < newRegions.Length; i++)
            {
                var region = newRegions[i];
                var polygon = planet.Polygons[i];
                region.Adjacents = new List<Region>(polygon.Adjacent.Count);
                for (int j = 0; j < polygon.Adjacent.Count; j++)
                {
                    region.Adjacents.Add(newRegions[polygon.Adjacent[j]]);
                }
            }

            Regions.AddRange(newRegions);
        }

        #endregion

        public (Region region, Planet.PolygonData polygonData) GetRegionByPosition(Vector3 hitInfoPoint)
        {
            Region nearestRegion = null;
            Planet.PolygonData nearestPolygon = null;
            float minDistance = float.MaxValue;

            for (var index = 0; index < Regions.Count; index++)
            {
                var region = Regions[index];
                var polygon = WorldManager.Instance.GetRegionPolygonData(region.ID);
                if (Vector3.Dot(hitInfoPoint, polygon.Center) < 0) // проверяем полушарие
                    continue; // пропускаем регион, если он находится в другом полушарии

                float distance = Vector3.Distance(hitInfoPoint, polygon.Center);

                if (distance < minDistance)
                {
                    minDistance = distance;
                    nearestRegion = region;
                    nearestPolygon = polygon;
                }
            }

            return (nearestRegion, nearestPolygon);
        }
    }
}