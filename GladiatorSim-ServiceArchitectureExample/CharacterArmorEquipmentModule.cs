using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using _Scripts.CodeBase.Gameplay.Common;
using _Scripts.CodeBase.Gameplay.Common.Equipment;
using _Scripts.CodeBase.Infrastructure.AssetManagement;

#if UNITY_EDITOR
using Sirenix.Utilities.Editor;
#endif

using UnityEditor;
using UnityEngine;
using Zenject;

namespace _Scripts.CodeBase.Gameplay.Equipment
{
    public class CharacterArmorEquipmentModule : MonoBehaviour
    {
        [field: SerializeField] public ControllerBase ControllerBase { get; set; }
        [SerializeField] private SkinnedMeshRenderer _bodyRenderer;
        [SerializeField] private Transform _parentOfBones;
        public Dictionary<ArmorEquipmentData.ArmorSlot, ArmorEquipmentData> _equippedArmor = new();
        public Dictionary<ArmorEquipmentData.ArmorSlot, List<SkinnedMeshRenderer>> _equippedArmorRenderers = new();
        private ArmorReferencesSO _armorReferencesModule;
        [Inject] private IAssetProvider _assetProvider;

        public async Task EquipArmor(ArmorEquipmentData armorEquipmentData)
        {
            if (_armorReferencesModule == null)
            {
                _armorReferencesModule = ControllerBase.StaticDataService.GetArmorReferencePrefab();
            }

            var prevRenderer = _equippedArmorRenderers.GetValueOrDefault(armorEquipmentData.Slot);
            if (prevRenderer != null)
            {
                foreach (var rnd in prevRenderer)
                {
                    Destroy(rnd.gameObject);
                }

                _equippedArmorRenderers.Remove(armorEquipmentData.Slot);
            }

            var renderersPaths = _armorReferencesModule.GetArmorMeshRenderersByName(armorEquipmentData.name);
            var loadTasks = new List<Task<SkinnedMeshRenderer>>();

            foreach (var renderersPath in renderersPaths)
            {
                var loadMeshRendererTask = _assetProvider.LoadAs<SkinnedMeshRenderer>(renderersPath);
                loadTasks.Add(loadMeshRendererTask);
            }

            var skinnedMeshRenderersFromReference = await Task.WhenAll(loadTasks);

            if (skinnedMeshRenderersFromReference.Length > 0)
            {
                _equippedArmorRenderers[armorEquipmentData.Slot] = new List<SkinnedMeshRenderer>();
                foreach (var referenceSkinnedMeshRenderer in skinnedMeshRenderersFromReference)
                {
                    var spawnedRenderer = Instantiate(referenceSkinnedMeshRenderer, _parentOfBones.parent);
                    spawnedRenderer.gameObject.SetActive(true);
                    _equippedArmorRenderers[armorEquipmentData.Slot].Add(spawnedRenderer);
                    SetBones(spawnedRenderer, _bodyRenderer.bones, _bodyRenderer.rootBone);
                }
            }
            else
            {
                Debug.LogError("SkinnedMeshRenderersFromReference is null");
            }

            _equippedArmor[armorEquipmentData.Slot] = armorEquipmentData;
        }


        public async Task EquipArmor(Dictionary<ArmorEquipmentData.ArmorSlot, ArmorEquipmentData> toEquip)
        {
            foreach (var (_, value) in toEquip)
            {
                await EquipArmor(value);
            }
        }

        private void SetBones(SkinnedMeshRenderer rnd, Transform[] bones, Transform boneroot)
        {
            rnd.rootBone = boneroot;
            rnd.bones = bones;
        }

        public async Task EquipArmor(Dictionary<ArmorEquipmentData.ArmorSlot, int> toEquip)
        {
            var allEquipment = ControllerBase.StaticDataService.GetEquipmentData().GetListOfAllEquipment;
            var equipTasks = new List<Task>();
            foreach (var (_, value) in toEquip)
            {
                if (value < 0 || value >= allEquipment.Count)
                {
                    Debug.LogError("Value " + value + " is out of range");
                    continue;
                }

                if (allEquipment[value] is ArmorEquipmentData armorEquipmentData)
                {
                    var task = EquipArmor(armorEquipmentData);
                    equipTasks.Add(task);
                }
                else
                {
                    Debug.LogError("ArmorEquipmentData is null");
                }
            }

            await Task.WhenAll(equipTasks);
        }

        public async Task EquipArmor(List<ArmorEquipmentData> armorEquipmentData)
        {
            var newDict = new Dictionary<ArmorEquipmentData.ArmorSlot, ArmorEquipmentData>();
            foreach (var armorData in armorEquipmentData)
            {
                newDict[armorData.Slot] = armorData;
            }

            await EquipArmor(newDict);
        }

        public Dictionary<ArmorEquipmentData.ArmorSlot, ArmorEquipmentData> GetEquippedArmor()
        {
            return _equippedArmor;
        }
    }
#if UNITY_EDITOR
    [CustomEditor(typeof(CharacterArmorEquipmentModule))]
    public class CharacterArmorEquipmentModuleEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            var module = (CharacterArmorEquipmentModule) target;
            foreach (var equipmentData in module._equippedArmor)
            {
                SirenixEditorGUI.InfoMessageBox(equipmentData.Key + " " + equipmentData.Value);
            }
        }
    }
#endif
}