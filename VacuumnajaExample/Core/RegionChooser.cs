using System;
using _Scripts.Camera;
using _Scripts.Core.Regions;
using _Scripts.Core.World;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;

namespace _Scripts.Core
{
    public class RegionChooser : MonoBehaviour
    {
        private UnityEngine.Camera _camera;
        private Mesh _chooseMesh;

        private void Awake()
        {
            _camera = UnityEngine.Camera.main;
        }

        private void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                if (CameraController.Instance.enabled) CheckForClick();
            }
        }

        private void CheckForClick()
        {
            if (EventSystem.current.IsPointerOverGameObject())
            {
                return;
            }

            var ray = _camera.ScreenPointToRay(Input.mousePosition);
            var result = new RaycastHit[1];
            if (Physics.RaycastNonAlloc(ray, result) > 0)
            {
                var hit = result[0];
                if (hit.collider != null)
                {
                    var (region, _) = RegionsManager.Instance.GetRegionByPosition(hit.point);
                    if (region != null)
                    {
                        WorldManager.Instance.MarkRegion(region.ID);
                    }
                }
            }
            else
            {
                WorldManager.Instance.MarkRegion(-1);
            }
        }
    }
}