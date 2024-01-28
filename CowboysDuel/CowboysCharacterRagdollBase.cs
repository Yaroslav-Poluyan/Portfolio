using System.Collections.Generic;
using System.Linq;
using _Scripts.Core.Characters.Common;
using _Scripts.Shooting;
using UnityEditor;
using UnityEngine;
using Zenject;

namespace _Scripts.Core.Characters
{
    public class CowboysCharacterRagdollBase : MonoBehaviour
    {
        [field: SerializeField] public Transform RootBone { get; private set; }
        [field: SerializeField] public List<Rigidbody> Rigidbodies { get; private set; }
        [field: SerializeField] public List<Collider> Colliders { get; private set; }
        private float _forceWhenHit = 10f;

        public bool IsRagdollEnabled()
        {
            if (Rigidbodies == null) return false;
            if (Rigidbodies.Count == 0) return false;
            return Rigidbodies.All(rb => rb != null && !rb.isKinematic);
        }

        public void SetRagdollState(bool state)
        {
            foreach (var rb in Rigidbodies)
            {
                rb.isKinematic = !state;
                rb.useGravity = state;
            }
        }
#if UNITY_EDITOR
        public void CollectColliders()
        {
            Colliders.Clear();
            if (RootBone == null) return;
            foreach (var col in RootBone.GetComponentsInChildren<Collider>())
            {
                Colliders.Add(col);
            }
        }

        public void CollectRigidbodies()
        {
            Rigidbodies.Clear();
            foreach (var rb in RootBone.GetComponentsInChildren<Rigidbody>())
            {
                Rigidbodies.Add(rb);
            }
        }
#endif
        public void AddForceToPart(CharacterInjuriesSystem.CharacterBodyPart characterBodyPart,
            Vector3 directionFromPlayer)
        {
            var neededTag = LayersAndTagsManager.Instance.TagByBodyPart[characterBodyPart];
            var neededRb = Rigidbodies.FirstOrDefault(x => x.CompareTag(neededTag));
            if (neededRb == null) return;
            neededRb.AddForce(directionFromPlayer * _forceWhenHit, ForceMode.Impulse);
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(CowboysCharacterRagdollBase))]
    public class CharachterRagdollBaseEditor : Editor
    {
        private CowboysCharacterRagdollBase _cowboysCharacterRagdollBase;

        private void OnEnable()
        {
            _cowboysCharacterRagdollBase = (CowboysCharacterRagdollBase) target;
            _cowboysCharacterRagdollBase.CollectColliders();
            _cowboysCharacterRagdollBase.CollectRigidbodies();
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            GUILayout.Label("Ragdoll elements");
            if (GUILayout.Button("Collect Ragdoll elements"))
            {
                _cowboysCharacterRagdollBase.CollectColliders();
                _cowboysCharacterRagdollBase.CollectRigidbodies();
            }

            //visualize the ragdoll enable state
            GUILayout.Label("Ragdoll enabled: " + _cowboysCharacterRagdollBase.IsRagdollEnabled());
            GUILayout.Label("Ragdoll Use Gravity: " +
                            _cowboysCharacterRagdollBase.Rigidbodies.All(rb => rb.useGravity));
            if (GUILayout.Button("Enable Ragdoll use gravity"))
            {
                foreach (var rb in _cowboysCharacterRagdollBase.Rigidbodies)
                {
                    rb.useGravity = true;
                }
            }

            if (GUILayout.Button("Disable Ragdoll use gravity"))
            {
                foreach (var rb in _cowboysCharacterRagdollBase.Rigidbodies)
                {
                    rb.useGravity = false;
                }
            }

            GUILayout.Label("Ragdoll is Kinematic: " +
                            _cowboysCharacterRagdollBase.Rigidbodies.All(rb => rb.isKinematic));
            if (GUILayout.Button("Set Ragdoll isKinematic"))
            {
                foreach (var rb in _cowboysCharacterRagdollBase.Rigidbodies)
                {
                    rb.isKinematic = true;
                }
            }

            if (GUILayout.Button("Set Ragdoll nonKinematic"))
            {
                foreach (var rb in _cowboysCharacterRagdollBase.Rigidbodies)
                {
                    rb.isKinematic = false;
                }
            }

            // 
            if (GUI.changed)
            {
                EditorUtility.SetDirty(_cowboysCharacterRagdollBase);
            }
        }
    }
#endif
}