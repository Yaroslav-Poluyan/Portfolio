using System.Collections.Generic;
using _Scripts.Common.Missiles;
using _Scripts.Common.Unit;
using _Scripts.Tech;
using UnityEngine;

// ReSharper disable Unity.InefficientPropertyAccess

namespace _Scripts.Common.ShootModules.Turrets
{
    public abstract class TurretBase : MonoBehaviour
    {
        [field: SerializeField] public bool CanShootInManyTargets { get; protected set; }
        [SerializeField] private Animator _turretAnimator;
        [SerializeField] protected Transform _turretHead;
        [SerializeField] protected AntiTruckMissile _missilePrefab;
        [SerializeField] protected Transform _spawnPoint;
        [SerializeField] protected float _xRotationInFirePreparing = -15f;
        [SerializeField] protected List<SpriteRenderer> _aimingCircles = new();
        [SerializeField] public float _aimingTime = 1f;
        public float _currentReloadTime;
        [SerializeField] protected SpriteRenderer _aimingCirclePrefab;
        [SerializeField] protected Sprite _aimingCircle;
        [SerializeField] protected Sprite _aimTakenCircle;
        [SerializeField] protected float Scatter = 50f;
        public bool CanShoot { get; set; }
        public UnitBase CurrentAimingTarget { get; protected set; }

        public List<UnitBase> NearUnitBases = new();
        protected float LineRendererWidth;
        protected const float HeadRotationSpeed = 10f;
        protected UnitBase ParentUnitBase;
        private bool _isInShootPosition;
        private static readonly int SetFire = Animator.StringToHash("SetFire");
        private static readonly int SetIdle = Animator.StringToHash("SetIdle");
        [SerializeField] protected float _reloadTime = 1f;
        [SerializeField] protected int _missilesCount = 1;
        [SerializeField] protected float _damage = 1f;

        private void Awake()
        {
            ParentUnitBase = GetComponentInParent<UnitBase>();
            _currentReloadTime = _reloadTime;
            ParentUnitBase.UnitStatsModule.AddNewMeshRendererToBlink(GetComponentsInChildren<SkinnedMeshRenderer>());
        }

        private void OnDestroy()
        {
            SetVisibilityForAllLineRenderers(false);
        }

        private void Update()
        {
            if (Pause.IsPaused || ParentUnitBase.UnitStatsModule.IsDead) return;
            _currentReloadTime -= Time.deltaTime;
            if (_currentReloadTime <= 0)
            {
                if (!_isInShootPosition) SetTurretInFireState();
                else CanShoot = true;
            }

            if (NearUnitBases.Count > 0)
            {
                RotationAndTargetChoosingLogic();
            }
            else
            {
                var newY = Quaternion.LookRotation(ParentUnitBase.transform.forward).eulerAngles.y;
                var newRotation = Quaternion.Euler(_xRotationInFirePreparing, newY, _turretHead.rotation.z);
                _turretHead.rotation =
                    Quaternion.Lerp(_turretHead.rotation, newRotation, Time.deltaTime * HeadRotationSpeed);
            }

            if (CurrentAimingTarget == null || !CanShoot || _currentReloadTime > 0)
            {
                SetVisibilityForAllLineRenderers(false);
            }
        }

        protected UnitBase GetNearestEnemy()
        {
            NearUnitBases.RemoveAll(x => x == null);
            NearUnitBases.Sort((x, y) =>
                Vector3.Distance(transform.position, x.transform.position)
                    .CompareTo(Vector3.Distance(transform.position, y.transform.position)));
            return NearUnitBases.Count > 0 ? NearUnitBases[0] : null;
        }

        //animation event call this method

        public void TurretIsReadyToFire()
        {
            CanShoot = true;
        }

        //animation event call this method

        public void TurretIsNotReadyTofFire()
        {
            CanShoot = false;
        }

        private void SetTurretInFireState()
        {
            _turretAnimator.SetTrigger(SetFire);
            _isInShootPosition = true;
        }

        protected void SetTurretInIdleState()
        {
            _turretAnimator.SetTrigger(SetIdle);
            _isInShootPosition = false;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent(out UnitBase unitBase))
            {
                if (ParentUnitBase == unitBase) return;
                NearUnitBases.Add(unitBase);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.TryGetComponent(out UnitBase unitBase))
            {
                if (ParentUnitBase == unitBase) return;
                if (NearUnitBases.Contains(unitBase)) NearUnitBases.Remove(unitBase);
                if (CurrentAimingTarget == unitBase) CurrentAimingTarget = null;
            }
        }

        public float GetReloadPercent()
        {
            if (_currentReloadTime <= 0) return 1f;
            return (_reloadTime - _currentReloadTime) / _reloadTime;
        }

        private void SetVisibilityForAllLineRenderers(bool isVisible)
        {
            foreach (var circle in _aimingCircles)
            {
                circle.enabled = isVisible switch
                {
                    true when !circle.enabled => true,
                    false when circle.enabled => false,
                    _ => circle.enabled
                };
            }
        }


        protected abstract void RotationAndTargetChoosingLogic();
        public abstract void Shoot(UnitBase truckGunCurrentAimingTarget, float aimPercent);
        public abstract void Shoot(List<UnitBase> targets, float truckGunAimingTime);
        public abstract void DrawAimArcToTarget(UnitBase target, float aimPercent);
        public abstract void DrawAimArcToTarget(List<UnitBase> targets, float aimPercent);

        public void SetDamage(float value)
        {
            _damage = value;
        }

        public void SetReloadTime(float value)
        {
            _reloadTime = value;
            _currentReloadTime = _reloadTime;
        }

        public void SetBulletCount(float value)
        {
            _missilesCount = (int) value;
        }
    }
}