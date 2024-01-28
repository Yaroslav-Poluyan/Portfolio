using System.Collections.Generic;
using _Scripts.Core.Characters.Common;
using UnityEditor;
using UnityEngine;

namespace _Scripts.Shooting
{
    public class TagAttribute : PropertyAttribute
    {
    }
#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(TagAttribute))]
    public class TagDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            property.stringValue = EditorGUI.TagField(position, label, property.stringValue);
        }
    }
#endif
    public class LayersAndTagsManager : MonoBehaviour
    {
        #region Singleton

        public static LayersAndTagsManager Instance;

        #endregion

        [Tag] [SerializeField] private string _playerTag;
        [Tag] [SerializeField] private string _enemyTag;
        [Tag] [SerializeField] private string _bodyTag;
        [Tag] [SerializeField] private string _leftArmTag;
        [Tag] [SerializeField] private string _rightArmTag;
        [Tag] [SerializeField] private string _legsTag;
        [Tag] [SerializeField] private string _headTag;
        [Tag] [SerializeField] private string _shootForceable;
        public Dictionary<string, CharacterInjuriesSystem.CharacterBodyPart> BodyPartByTag { get; private set; }
        public Dictionary<CharacterInjuriesSystem.CharacterBodyPart, string> TagByBodyPart { get; private set; }

        public string BodyTag => _bodyTag;
        public string LeftArmTag => _leftArmTag;
        public string HeadTag => _headTag;

        public string EnemyTag => _enemyTag;

        public string LegsTag => _legsTag;
        public string RightArmTag => _rightArmTag;
        public string PlayerTag => _playerTag;

        public string ShootForceable => _shootForceable;
        //EnemyLayer
        public int EnemyLayer => LayerMask.NameToLayer("Enemy");

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(this);
                return;
            }

            Instance = this;
            BodyPartByTag = new Dictionary<string, CharacterInjuriesSystem.CharacterBodyPart>
            {
                {HeadTag, CharacterInjuriesSystem.CharacterBodyPart.Head},
                {BodyTag, CharacterInjuriesSystem.CharacterBodyPart.Body},
                {LeftArmTag, CharacterInjuriesSystem.CharacterBodyPart.LeftArm},
                {RightArmTag, CharacterInjuriesSystem.CharacterBodyPart.RightArm},
                {LegsTag, CharacterInjuriesSystem.CharacterBodyPart.Legs}
            };
            TagByBodyPart = new Dictionary<CharacterInjuriesSystem.CharacterBodyPart, string>();
            foreach (var bodyPart in BodyPartByTag)
            {
                TagByBodyPart.Add(bodyPart.Value, bodyPart.Key);
            }
        }
    }
}