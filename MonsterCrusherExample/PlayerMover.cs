using System;
using _Scripts.TechnicalScripts;
using MoreMountains.Tools;
using UnityEngine;

namespace _Scripts.Player
{
    public class PlayerMover : MMSingleton<PlayerMover>
    {
        [SerializeField] private PlayerMovementType _playerMovementType;
        [SerializeField] private CharacterController _characterController;
        [SerializeField] private InputModule _inputModule;
        private Vector3 _direction = Vector3.zero;

        #region Variables: Gravity

        [SerializeField] private float _gravityMultiplier = 3.0f;
        private float _gravity = -9.81f;
        private float _velocity;

        #endregion

        #region SlimeMovement

        [Header("Water Movement")] [SerializeField]
        private float _interval = 2f; // интервал между пульсациями

        [SerializeField] private float _minSpeed = 1f; // минимальная скорость движения
        [SerializeField] private float _maxSpeed = 5f; // максимальная скорость движения
        private float _time;
        private float _currentSpeedWater;
        private bool _canMoveViaJoystick = true;

        #endregion

        public float CurrentSpeed { get; set; }
        public float CurrentDefaultSpeed { get; set; }

        public bool CanMoveViaJoystick
        {
            get => _canMoveViaJoystick;
            set
            {
                _canMoveViaJoystick = value;
                _inputModule.SetJoystikVisibility(value);
            }
        }

        public bool UseGravity { get; set; } = true;

        public enum PlayerMovementType
        {
            Land,
            Water
        }

        public void SetMovementState(bool state)
        {
            CanMoveViaJoystick = state;
            _characterController.enabled = state;
        }

        private void Update()
        {
            if (Pause.IsPaused) return;
            if (!CanMoveViaJoystick) return;
            _direction = _inputModule.GetInput();
            _direction = new Vector3(_direction.x, 0f, _direction.z);
            ApplyGravity();
            MoveInDirection();
        }

        public void ChangeMovementType(PlayerMovementType playerMovementType)
        {
            _playerMovementType = playerMovementType;
        }

        private void ApplyGravity()
        {
            if (!UseGravity) return;
            if (_playerMovementType == PlayerMovementType.Water) return;
            if (_characterController.isGrounded && _velocity < 0.0f)
                _velocity = -1.0f;
            else
                _velocity += _gravity * _gravityMultiplier * Time.deltaTime;

            _direction.y = _velocity;
        }

        private void MoveInDirection()
        {
            switch (_playerMovementType)
            {
                case PlayerMovementType.Land:
                    MoveCarrion();
                    break;
                case PlayerMovementType.Water:
                    MoveWater();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void MoveCarrion()
        {
            _characterController.Move(_direction * (CurrentSpeed * Time.deltaTime));
        }

        private void MoveWater()
        {
            _time += Time.deltaTime;
            if (_time < _interval / 2f)
                _currentSpeedWater = Mathf.Lerp(_minSpeed, _maxSpeed, _time / (_interval / 2f));
            else if (_time < _interval)
                _currentSpeedWater = Mathf.Lerp(_maxSpeed, _minSpeed, (_time - _interval / 2f) / (_interval / 2f));
            else
                _time = 0f;

            _characterController.Move(_direction * (_currentSpeedWater * Time.deltaTime));
        }

        public void SetNewSpeeds(float defaultMoveSpeedForStage)
        {
            CurrentSpeed = defaultMoveSpeedForStage;
            CurrentDefaultSpeed = defaultMoveSpeedForStage;
        }
    }
}