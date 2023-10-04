using System;
using System.Collections.Generic;
using PlanetGeneratorPackage.Scripts;
using UnityEngine;
using UnityEngine.Rendering;

namespace _Scripts.Core.World
{
    public class RegionMarker : MonoBehaviour
    {
        private MeshRenderer _meshRenderer;
        private MeshFilter _meshFilter;
        private Mesh _mesh;

        public void Init(Planet planet)
        {
            _meshRenderer = gameObject.AddComponent<MeshRenderer>();
            _meshFilter = gameObject.AddComponent<MeshFilter>();
            _mesh = new Mesh();
            _meshFilter.mesh = _mesh;
            _meshRenderer.material = new Material(Shader.Find($"Unlit/Color"))
            {
                color = Color.red
            };
            _meshRenderer.shadowCastingMode = ShadowCastingMode.Off;
            _meshRenderer.receiveShadows = false;
            _meshRenderer.enabled = false;
        }

        public void MarkRegion(List<Vector3> vertices)
        {
            if (vertices.Count == 0)
            {
                _meshRenderer.enabled = false;
                return;
            }

            _meshRenderer.enabled = true;
            _mesh.Clear();

            for (var index = 0; index < vertices.Count; index++)
            {
                var vertex = vertices[index];
                vertex.Scale(1.0005f * Vector3.one);
                vertices[index] = vertex;
            }

            _mesh.vertices = vertices.ToArray();

            // Calculate triangles dynamically based on the number of vertices
            var triangles = new List<int>();

            // Add outer triangles
            for (var i = 0; i < vertices.Count - 2; i++)
            {
                triangles.Add(0);
                triangles.Add(i + 1);
                triangles.Add(i + 2);
            }

            _mesh.triangles = triangles.ToArray();
        }
    }
}