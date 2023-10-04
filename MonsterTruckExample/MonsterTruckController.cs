using System;
using System.Collections.Generic;
using System.Linq;
using _Scripts.Common.MonsterTruck;
using _Scripts.Common.MonsterTruck.Upgrades;
using _Scripts.Common.MonsterTruck.VehicleReset;
using _Scripts.Common.Unit;
using _Scripts.Enemies.Common;
using _Scripts.Interfaces;
using _Scripts.Tech;
using UnityEngine;
using UnityEngine.Serialization;

// ReSharper disable Unity.InefficientPropertyAccess

namespace _Scripts.Monster_Truck
{
    public class MonsterTruckController : MonoBehaviour
    {
        public Action _onTruckBroken;
        public Action _onWheelBroken;
        public Action<bool> _onFlyingStateChanged;
        public bool _isInGarage;

        [Header("set speed value same as truck can reach in game")] [SerializeField]
        private float _realMaxSpeed = 62f;

        public float RealMaxSpeed => _realMaxSpeed;

        [field: SerializeField] public MonsterTruckSoundManager SoundManager { get; private set; }

        #region Grounded/Flight

        public bool IsGrounded { get; private set; } = true;
        [SerializeField] private float _inAirRotationSpeed = 20f;
        [SerializeField] private float _inairStabilizationSpeed = 10f;
        private Vector3 GroundPoint { get; set; }
        private const float TimeInAirToBeConsideredFlying = .5f;
        private float _timeInAir;
        private bool _isFlying;
        public bool IsDrifting { get; private set; }

        public bool IsFlying
        {
            get => _isFlying;
            private set
            {
                _isFlying = value;
                SoundManager.PlayFlyingSound(value);
                _onFlyingStateChanged?.Invoke(value);
            }
        }

        #endregion


        [FormerlySerializedAs("torque")] [Header("Vehicle Stats")]
        public float _torque;

        [FormerlySerializedAs("angularSpeed")] [SerializeField]
        public float _angularSpeed = 100;

        public float _wheelRotationSpeed = 5;

        [FormerlySerializedAs("MaxTurnAngle")] [SerializeField]
        public float _maxTurnAngle;

        [FormerlySerializedAs("downForce")] [SerializeField]
        public float _downForce;

        [FormerlySerializedAs("allWheelDrive")] [SerializeField]
        public bool _allWheelDrive = true;

        [FormerlySerializedAs("allWheelTurn")] [SerializeField]
        public bool _allWheelTurn = false;

        [FormerlySerializedAs("allWheelBrake")] [SerializeField]
        public bool _allWheelBrake = true;

        public bool _allowReverse = true;

        [FormerlySerializedAs("Raydistance")] [SerializeField]
        public float _raydistance;

        [FormerlySerializedAs("drivable")] [SerializeField]
        public LayerMask _drivable;

        [FormerlySerializedAs("centerOfMass")] [SerializeField]
        public Transform _centerOfMass;

        [FormerlySerializedAs("steeringJoint")] [SerializeField]
        public HingeJoint[] _steeringJoint = new HingeJoint[4];

        [FormerlySerializedAs("wheelJoints")] [SerializeField]
        public List<HingeJoint> _wheelJoints = new();

        [SerializeField] private List<HingeJointDestroyListener> _wheelJointDestroyListeners = new();
        [SerializeField] private BoxCollider _bodyCollider;
        [SerializeField] private MonsterTruckInputSystemBase _inputSystem;
        [SerializeField] private Rigidbody _rb;
        [SerializeField] private List<Rigidbody> _wheelRbs = new();
        [SerializeField] private List<Rigidbody> _reserveWheelRbs = new();
        [SerializeField] private List<Rigidbody> _steeringRbs = new();
        [SerializeField] private List<TruckWheel> _wheels = new();
        private float _horizontalInput, _verticalInput;
        private bool _isEnemyTruck;
        private UnitBase _unitBase;
        public bool IsLoseAnyWheel => _wheelJoints.Count < DefaultWheelsCount;

        #region SpeedModifiers

        [SerializeField] private List<SpeedModifierData> _speedModifiers = new();

        [Serializable]
        public class SpeedModifierData
        {
            public float SpeedModifier;
            public float ModifierDuration;
            public float ModiferAfterEnd;
            public float ModifierAfterEndDuration;
            public PermanentSpeedModifierType PermanentModifierType;

            public SpeedModifierData(float speedModifier, float modifierDuration, float modiferAfterEnd = 0f,
                float modifierAfterEndDuration = 0f,
                PermanentSpeedModifierType permanentModifierType = PermanentSpeedModifierType.None)
            {
                SpeedModifier = speedModifier;
                ModifierDuration = modifierDuration;
                ModiferAfterEnd = modiferAfterEnd;
                ModifierAfterEndDuration = modifierAfterEndDuration;
                PermanentModifierType = permanentModifierType;
            }

            public enum PermanentSpeedModifierType
            {
                None = 0,
                Mood = 1,
                AINeedExtraSpeedToReachPlayer = 2,
                AIBrakingToReachPlayer = 3,
            }
        }

        public void AddSpeedModifier(float speedModifier,
            float modifierDuration = 0f,
            float modiferAfterEnd = 0f,
            float modifierAfterEndDuration = 0f)
        {
            var modifier = new SpeedModifierData(speedModifier, modifierDuration, modiferAfterEnd,
                modifierAfterEndDuration);
            _speedModifiers.Add(modifier);
        }

        public void AddSpeedModifier(float speedModifier,
            SpeedModifierData.PermanentSpeedModifierType permanentSpeedModifierType)
        {
            if (_speedModifiers.Any(x => x.PermanentModifierType == permanentSpeedModifierType))
            {
                return;
            }

            var modifier = new SpeedModifierData(speedModifier, 0f, 0f, 0f, permanentSpeedModifierType);
            _speedModifiers.Add(modifier);
        }

        public void RemovePermanentSpeedModifier(
            SpeedModifierData.PermanentSpeedModifierType permanentSpeedModifierType)
        {
            var modifier =
                _speedModifiers.FirstOrDefault(x => x.PermanentModifierType == permanentSpeedModifierType);
            if (modifier != null)
            {
                _speedModifiers.Remove(modifier);
            }
        }

        private void CheckForSpeedModifiers()
        {
            var totalSpeedModifier = 1f;
            if (_speedModifiers.Count == 0)
            {
                SpeedModifier = 1f;
                return;
            }

            foreach (var speedModifier in _speedModifiers.ToList())
            {
                if (speedModifier.PermanentModifierType != SpeedModifierData.PermanentSpeedModifierType.None)
                {
                    totalSpeedModifier *= speedModifier.SpeedModifier;
                }
                else
                {
                    speedModifier.ModifierDuration -= Time.fixedDeltaTime;
                    if (speedModifier.ModifierDuration <= 0)
                    {
                        if (speedModifier.ModiferAfterEnd > 0)
                        {
                            speedModifier.ModifierAfterEndDuration -= Time.fixedDeltaTime;
                            if (speedModifier.ModifierAfterEndDuration <= 0)
                            {
                                _speedModifiers.Remove(speedModifier);
                            }
                        }
                        else
                        {
                            _speedModifiers.Remove(speedModifier);
                        }
                    }
                    else
                    {
                        totalSpeedModifier *= speedModifier.SpeedModifier;
                    }
                }
            }

            SpeedModifier = totalSpeedModifier;
        }

        #endregion

        #region Gears

        private const int TransmissionGears = 5;
        private int _currentGear = 1;

        #endregion

        public Rigidbody Rb => _rb;
        private float SpeedModifier { get; set; } = 1f;
        public float DefaultWheelsCount { get; private set; }
        public List<TruckWheel> Wheels => _wheels;
        public List<Rigidbody> WheelRbs => _wheelRbs;
#if UNITY_EDITOR


        [SerializeField] private bool _getComponents;
        private void OnValidate()
        {
            if (_getComponents)
            {
                _wheelJoints.Clear();
                var joints = GetComponentsInChildren<HingeJoint>();
                _wheelJoints.AddRange(joints.Where(name => name.name.Contains("Wheel")));
                _steeringJoint = GetComponentsInChildren<HingeJoint>().Where(name => name.name.Contains("Suspension"))
                    .ToArray();
                for (var index = 0; index < _wheelRbs.Count; index++)
                {
                    _wheelRbs[index] = _wheelJoints[index].GetComponent<Rigidbody>();
                }

                for (var index = 0; index < _steeringRbs.Count; index++)
                {
                    _steeringRbs[index] = _steeringJoint[index].GetComponent<Rigidbody>();
                }

                _wheels = GetComponentsInChildren<TruckWheel>().ToList();
                _wheelJointDestroyListeners.Clear();
                _wheelJointDestroyListeners.AddRange(GetComponentsInChildren<HingeJointDestroyListener>());
                _getComponents = false;
            }
        }
#endif
        private void Awake()
        {
            DefaultWheelsCount = _wheelJoints.Count;
            _unitBase = GetComponent<UnitBase>();
        }

        private void Start()
        {
            Rb.centerOfMass = Vector3.zero;
            foreach (var listener in _wheelJointDestroyListeners)
            {
                listener.OnJointDestroyed += OnJointBreakHandler;
            }

            _isEnemyTruck = GetComponent<EnemyBase>() != null;
        }

        private void OnDestroy()
        {
            foreach (var listener in _wheelJointDestroyListeners)
            {
                listener.OnJointDestroyed -= OnJointBreakHandler;
            }
        }

        private void Update()
        {
            SoundManager.EngineSound(GetSpeed(), _inputSystem.GetVerticalInput() > 0);
#if UNITY_EDITOR
            if (Input.GetKeyDown("1"))
            {
                BrakeWheelManually("FL");
            }
            else if (Input.GetKeyDown("2"))
            {
                BrakeWheelManually("FR");
            }
            else if (Input.GetKeyDown("3"))
            {
                BrakeWheelManually("RL");
            }
            else if (Input.GetKeyDown("4"))
            {
                BrakeWheelManually("RR");
            }
#endif
        }

        private void FixedUpdate()
        {
            if (Pause.IsPaused || _isInGarage || _unitBase.VehicleResetter.IsResetting)
            {
                ForceBreak();
                return;
            }

            _horizontalInput = GetHorizontalInput();
            _verticalInput = GetVerticalInput();
            CheckForSpeedModifiers();
            TurnLogic();
            AccelarationLogic();
            BrakeLogic();
            InAirRotationLogic();
            InAirStabilizationLogic();
            CheckForDrifting();
            var groundedCheck = CheckForGround();
            if (groundedCheck)
            {
                AddDownForce();
                _timeInAir = 0;
                IsFlying = false;
                if (!IsGrounded)
                {
                    OnGroundedHandlerStateChanged(true);
                    IsGrounded = true;
                }
            }
            else
            {
                _timeInAir += Time.deltaTime;
                if (_timeInAir > TimeInAirToBeConsideredFlying)
                {
                    IsFlying = true;
                }

                if (IsGrounded)
                {
                    OnGroundedHandlerStateChanged(false);
                    IsGrounded = false;
                }
            }

            CheckForGearChange();
        }

        private void CheckForDrifting()
        {
            if (!IsGrounded) return; // Если грузовик в воздухе, то дрифта нет
            var velocityVector = Rb.velocity; // Вектор скорости грузовика
            velocityVector.y = 0; // Обнуляем Y, чтобы не мешался
            var orientationVector = transform.forward; // Вектор направления грузовика
            orientationVector.y = 0; // Обнуляем Y, чтобы не мешался
            // Определяем угол между направлением движения и направлением, куда смотрит грузовик
            var angleDifference = Vector3.Angle(velocityVector, orientationVector);
            // Если угол больше определенного порога, например, 30 градусов, будем считать, что грузовик занят дрифтом
            // активируем отслеживание дрифта
            // Если скорость меньше 5, то дрифта нет
            if (_rb.velocity.magnitude > 5 && Vector3.Angle(-transform.forward, _rb.velocity) >= 10)
            {
                IsDrifting = angleDifference > 30;
            }
            else IsDrifting = false;
        }


        private void CheckForGearChange()
        {
            var speedPerGear = _realMaxSpeed / TransmissionGears;
            if (_currentGear < TransmissionGears && GetSpeed() > speedPerGear * (_currentGear + 1))
            {
                ChangeGear(_currentGear + 1);
            }
            else if (_currentGear > 1 && GetSpeed() < speedPerGear * (_currentGear - 1))
            {
                ChangeGear(_currentGear - 1);
            }
        }

        private void ChangeGear(int toChange)
        {
            _currentGear = toChange;
            AddSpeedModifier(0.1f, modifierDuration: .5f, modiferAfterEnd: 1.2f, modifierAfterEndDuration: .5f);
        }

        private void OnGroundedHandlerStateChanged(bool state)
        {
            if (state) SoundManager.PlayOnLandedSound(true);
            foreach (var wheel in Wheels)
            {
                wheel.TruckChangedGroundedState(state);
            }
        }

        private void OnJointBreakHandler(HingeJoint joint)
        {
            var idx = _wheelJoints.IndexOf(joint);
            _wheelJoints.Remove(joint);
            _onWheelBroken?.Invoke();
            if (idx == -1) return;
            if (_wheelJoints.Count > 2)
            {
                _reserveWheelRbs[idx].gameObject.SetActive(true);
            }
        }

        private float GetHorizontalInput()
        {
            return _inputSystem.GetHorizontalInput();
        }

        private float GetVerticalInput()
        {
            return _inputSystem.GetVerticalInput();
        }

        private bool GetBreakInput()
        {
            return _inputSystem.GetBreakInput();
        }

        private float GetInAirRotationInput()
        {
            return _inputSystem.GetInAirRotationInput();
        }

        //stabalize vehicle when its about to flip -logic starts from here  
        private void OnCollisionEnter(Collision _)
        {
            Rb.centerOfMass = _centerOfMass.localPosition;
        }

        private void OnCollisionExit(Collision _)
        {
            Rb.centerOfMass = Vector3.zero;
        }
        //stabalize vehicle when its about to flip -Logic ends here


        private bool CheckForGround() //check if vehicle is grounded or not(by shooting a ray downwards)
        {
            var hasGround = Physics.Raycast(transform.position, -transform.up, out var hit, 100, _drivable);
            if (hasGround)
            {
                GroundPoint = hit.point;
            }

            if (Vector3.Distance(transform.position, GroundPoint) > _raydistance)
            {
                hasGround = false;
            }

            return hasGround;
        }

        private void AccelarationLogic()
        {
            if (_allWheelDrive)
            {
                foreach (var wheelJt in _wheelJoints)
                {
                    var motar = wheelJt.motor;
                    motar.targetVelocity = _angularSpeed * _verticalInput * SpeedModifier;
                    var effectiveSpeed = _wheelJoints.Count / DefaultWheelsCount * 1.5f;
                    effectiveSpeed = Mathf.Clamp(effectiveSpeed, 0f, 1f);
                    if (!_allowReverse)
                    {
                        if (Mathf.Abs(_verticalInput) > 0.05f && !GetBreakInput() && _verticalInput > 0)
                        {
                            motar.force = _torque * effectiveSpeed;
                        }
                        else
                        {
                            motar.force = 0;
                        }
                    }
                    else
                    {
                        if (Mathf.Abs(_verticalInput) > 0.05f && !GetBreakInput())
                        {
                            motar.force = _torque * effectiveSpeed;
                        }
                        else
                        {
                            motar.force = 0;
                        }
                    }

                    wheelJt.motor = motar;
                }
            }
            else
            {
                var motar = _wheelJoints[3].motor;
                motar.targetVelocity = _angularSpeed * _verticalInput;
                if (Mathf.Abs(_verticalInput) > 0.05f && !GetBreakInput())
                {
                    motar.force = _torque;
                }
                else
                {
                    motar.force = 0;
                }

                _wheelJoints[2].motor = motar;
                _wheelJoints[3].motor = motar;
            }
        }

        private void ForceBreak()
        {
            if (_allWheelBrake)
            {
                foreach (var wheelrb in _wheelRbs)
                {
                    wheelrb.constraints = RigidbodyConstraints.FreezeRotationX;
                }
            }
            else
            {
                _wheelRbs[2].constraints = RigidbodyConstraints.FreezeRotationX;
                _wheelRbs[3].constraints = RigidbodyConstraints.FreezeRotationX;
            }
        }

        private void BrakeLogic()
        {
            if (_allWheelBrake)
            {
                if (GetBreakInput())
                {
                    foreach (var wheelrb in _wheelRbs)
                    {
                        wheelrb.constraints = RigidbodyConstraints.FreezeRotationX;
                    }
                }
                else
                {
                    foreach (var wheelrb in _wheelRbs)
                    {
                        wheelrb.constraints = RigidbodyConstraints.None;
                    }
                }
            }
            else
            {
                if (GetBreakInput())
                {
                    _wheelRbs[2].constraints = RigidbodyConstraints.FreezeRotationX;
                    _wheelRbs[3].constraints = RigidbodyConstraints.FreezeRotationX;
                }
                else
                {
                    _wheelRbs[2].constraints = RigidbodyConstraints.None;
                    _wheelRbs[3].constraints = RigidbodyConstraints.None;
                }
            }
        }

        private void TurnLogic()
        {
            var steerjointSpringF = _steeringJoint[1].spring;
            steerjointSpringF.targetPosition = Mathf.Lerp(steerjointSpringF.targetPosition,
                _horizontalInput * _maxTurnAngle, Time.fixedDeltaTime * _wheelRotationSpeed);
            _steeringJoint[0].spring = steerjointSpringF;
            _steeringJoint[1].spring = steerjointSpringF;

            if (_allWheelTurn)
            {
                var steerjointSpringB = _steeringJoint[2].spring;
                steerjointSpringB.targetPosition = Mathf.Lerp(steerjointSpringB.targetPosition,
                    _horizontalInput * -_maxTurnAngle / 2, Time.fixedDeltaTime * _wheelRotationSpeed);
                _steeringJoint[2].spring = steerjointSpringB;
                _steeringJoint[3].spring = steerjointSpringB;
            }
        }

        private void InAirRotationLogic()
        {
            var inAirRotationInput = GetInAirRotationInput();
            if (_isFlying && inAirRotationInput != 0)
            {
                var torque = inAirRotationInput switch
                {
                    > 0 => transform.right * (_inAirRotationSpeed * Time.fixedDeltaTime),
                    < 0 => -transform.right * (_inAirRotationSpeed * Time.fixedDeltaTime),
                    _ => Vector3.zero
                };
                _rb.AddTorque(torque, ForceMode.Impulse);
            }
        }

        private void InAirStabilizationLogic()
        {
            if (!_isEnemyTruck)
            {
                if (_isFlying && GetInAirRotationInput() != 0)
                {
                    if (Rb.angularVelocity.magnitude == 0) return;
                    var torque = -Rb.angularVelocity.normalized * (_inairStabilizationSpeed * Time.fixedDeltaTime);
                    _rb.AddTorque(torque, ForceMode.Impulse);
                }
            }
            else
            {
                if (_isFlying)
                {
                    var targetNormal = Vector3.up;
                    var currentNormal = transform.up;
                    //truck should try to align itself with the ground
                    var torque = Vector3.Cross(currentNormal, targetNormal) *
                                 (_inairStabilizationSpeed * 25f * Time.fixedDeltaTime);
                    _rb.AddTorque(torque, ForceMode.Impulse);
                }
            }
        }


        private void AddDownForce()
        {
            Rb.AddForce(Vector3.down * _downForce);
        }


#if UNITY_EDITOR

        #region Gizmos

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, transform.position + Vector3.down * _raydistance);

            if (!Application.isPlaying)
            {
                Gizmos.color = new Color(0, 1, 0, 0.2f);
                Gizmos.DrawCube(_bodyCollider.bounds.center, _bodyCollider.bounds.size);
                Gizmos.color = Color.green;
                Gizmos.DrawWireCube(_bodyCollider.bounds.center, _bodyCollider.bounds.size);

                var groundCheckPlane = new Vector3(_bodyCollider.size.x * 1.5f, 0.05f,
                    _bodyCollider.size.z);

                Gizmos.color = new Color(0, 0, 1, 0.5f);
                Gizmos.DrawCube(transform.position + Vector3.down * _raydistance, 1.5f * groundCheckPlane);
                Gizmos.DrawWireCube(transform.position + Vector3.down * _raydistance, 1.5f * groundCheckPlane);
            }
        }

        #endregion

        public void BrakeWheelManually(string wheelName)
        {
            var flListener = _wheelJointDestroyListeners.Find(x => x.name.Contains(wheelName));
            flListener.DestroyJointManually();
        }
#endif
        public float GetSpeed()
        {
            return Rb.velocity.magnitude;
        }

        public void Reset()
        {
            Rb.velocity = Vector3.zero;
            Rb.angularVelocity = Vector3.zero;
        }

        public void AddTorque(Vector3 torque)
        {
            Rb.AddTorque(torque, ForceMode.Impulse);
        }

        public void AddImpulse(Vector3 force)
        {
            Rb.AddForce(force, ForceMode.Impulse);
        }

        public void ChangeSpeedByMultiplyer(float value)
        {
            Rb.velocity *= value;
        }

        public void SetInGarageState(bool state)
        {
            _isInGarage = state;
            var igaragables = GetComponentsInChildren<IGaragable>().ToList();
            igaragables.AddRange(GetComponents<IGaragable>());
            foreach (var garagable in igaragables)
            {
                garagable.SetInGarage(state);
            }

            if (state)
            {
                Rb.isKinematic = true;
                var rbs = GetComponentsInChildren<Rigidbody>();
                foreach (var rb in rbs)
                {
                    rb.isKinematic = true;
                }
            }
        }

        public void OnTruckKillHandler()
        {
        }

        public void Init(List<MonsterTruckUpgradeManager.Part.TruckSpecs> getCurrentTruckSpecs)
        {
            foreach (var truckSpec in getCurrentTruckSpecs)
            {
                switch (truckSpec._improvementType)
                {
                    case MonsterTruckUpgradeManager.UpgradeType.Torque:
                        _torque = truckSpec._value;
                        break;
                    case MonsterTruckUpgradeManager.UpgradeType.AngularSpeed:
                        _angularSpeed = truckSpec._value;
                        break;
                    case MonsterTruckUpgradeManager.UpgradeType.Suspension:
                        GetComponent<SuspensionJoints>().suspensionForce = truckSpec._value;
                        break;
                    case MonsterTruckUpgradeManager.UpgradeType.Weight:
                        Rb.mass = truckSpec._value;
                        break;
                    case MonsterTruckUpgradeManager.UpgradeType.WheelHp:
                        foreach (var wheel in Wheels)
                        {
                            wheel.SetMaxHp(truckSpec._value);
                        }

                        break;
                    case MonsterTruckUpgradeManager.UpgradeType.DamagePerBullet:
                    case MonsterTruckUpgradeManager.UpgradeType.ReloadTime:
                    case MonsterTruckUpgradeManager.UpgradeType.BulletCountPerShoot:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        public void OnMudEnterState(bool state)
        {
            SoundManager.OnMudEnterState(state);
            _unitBase.UnitFeedbacksModule.PlayFeedbacks(UnitFeedbacksModule.FeedbackType.OnMudDrive, state);
        }

        public bool HasSpeedModifier(SpeedModifierData.PermanentSpeedModifierType type)
        {
            return _speedModifiers.Any(x => x.PermanentModifierType == type);
        }
    }
}