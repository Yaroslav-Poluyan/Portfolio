using System;
using _Scripts.Common.MonsterTruck;
using _Scripts.Enemies.Common;
using UnityEngine;
using UnityEngine.Serialization;

namespace _Scripts.Enemies
{
    public class EnemyMonsterTruckInputSystem : MonsterTruckInputSystemBase
    {
        [SerializeField] private EnemyBase _enemyBase;
        [SerializeField] private float _missedTargetDistance = 2f;
        [SerializeField] private float _stoppingDistance = 5f;
        [SerializeField] private float _stoppingSpeed = .2f;
        [SerializeField] private float _reverseDistance = 25;
        [SerializeField] private float _deadZoneTurnAngle = 2f;
        [SerializeField] private float _softTurnAngle = 30f;
        [SerializeField] private float _hardTurnAngle = 60f;
        [Range(0, 1)] [SerializeField] private float _turnForce = 1f;
        [SerializeField] private float _wheelsRotateSpeed = 5f;
        public bool _needBrake;

        private float _forwardAmount;
        private float _turnAmount;
        private float _prevTurnAmount;


        private void OnDrawGizmos()
        {
            #region turn graphics

            Gizmos.color = Color.blue;
            var length = 10;
            //show soft turn angle
            Gizmos.DrawRay(transform.position,
                Quaternion.AngleAxis(_softTurnAngle, Vector3.up) * transform.forward * length);
            Gizmos.DrawRay(transform.position,
                Quaternion.AngleAxis(-_softTurnAngle, Vector3.up) * transform.forward * length);
            //show hard turn angle
            Gizmos.color = Color.red;
            Gizmos.DrawRay(transform.position,
                Quaternion.AngleAxis(_hardTurnAngle, Vector3.up) * transform.forward * length);
            Gizmos.DrawRay(transform.position,
                Quaternion.AngleAxis(-_hardTurnAngle, Vector3.up) * transform.forward * length);
            // from back side
            Gizmos.color = Color.blue;
            Gizmos.DrawRay(transform.position,
                Quaternion.AngleAxis(_softTurnAngle, Vector3.up) * -transform.forward * length);
            Gizmos.DrawRay(transform.position,
                Quaternion.AngleAxis(-_softTurnAngle, Vector3.up) * -transform.forward * length);
            //show hard turn angle
            Gizmos.color = Color.red;
            Gizmos.DrawRay(transform.position,
                Quaternion.AngleAxis(_hardTurnAngle, Vector3.up) * -transform.forward * length);
            Gizmos.DrawRay(transform.position,
                Quaternion.AngleAxis(-_hardTurnAngle, Vector3.up) * -transform.forward * length);

            #endregion

            #region movement graphics

            Gizmos.color = Color.magenta;
            if (_enemyBase.EnemyTruckMovementAgentHelper != null)
                Gizmos.DrawLine(transform.position, _enemyBase.EnemyTruckMovementAgentHelper.transform.position);
            Gizmos.DrawRay(transform.position,
                (_forwardAmount * transform.forward + _turnAmount * transform.right) * length);

            #endregion
        }

        public override void StopMovement()
        {
            base.StopMovement();
            _forwardAmount = 0f;
            _turnAmount = 0f;
            _needBrake = true;
            _enemyBase.EnemyTruckMovementAgentHelper.SetMovingState(false);
        }

        public override float GetHorizontalInput()
        {
            return _turnAmount;
        }

        public override float GetVerticalInput()
        {
            return _forwardAmount;
        }

        public override bool GetBreakInput()
        {
            return _needBrake;
        }

        public override float GetInAirRotationInput()
        {
            return 0;
        }

        public override void Reset()
        {
            base.Reset();
            _forwardAmount = 0f;
            _turnAmount = 0f;
            _needBrake = false;
            _enemyBase.EnemyTruckMovementAgentHelper.Reset();
        }

        protected override void Update()
        {
            base.Update();
            if (ForceStop) return;
            var targetPosition = _enemyBase.EnemyTruckMovementAgentHelper.transform.position;
            _forwardAmount = 0f;

            var distanceToTarget = Vector3.Distance(transform.position, targetPosition);
            if (distanceToTarget > _missedTargetDistance)
            {
                // Still too far, keep going
                var dirToMovePosition = (targetPosition - transform.position).normalized;
                var dot = Vector3.Dot(transform.forward, dirToMovePosition);

                if (dot > 0)
                {
                    // Target in front
                    _forwardAmount = 1f;

                    if (distanceToTarget < _stoppingDistance && GetSpeed() > _stoppingSpeed)
                    {
                        // Within stopping distance and moving forward too fast
                        _forwardAmount = -1f;
                    }
                }
                else
                {
                    // Target behind
                    if (distanceToTarget > _reverseDistance)
                    {
                        // Too far to reverse
                        _forwardAmount = 1f;
                    }
                    else
                    {
                        _forwardAmount = -1f;
                    }
                }

                var angleToDir = Vector3.SignedAngle(transform.forward, dirToMovePosition, Vector3.up);

                var turnDirection = 0f;
                if (angleToDir > 0)
                {
                    turnDirection = 1f;
                }
                else
                {
                    turnDirection = -1f;
                }

                //calc turn amount
                var angleToTarget = Vector3.Angle(transform.forward, dirToMovePosition);
                if (Math.Abs(angleToTarget) < _deadZoneTurnAngle)
                {
                    _turnAmount = 0;
                }
                else if (angleToTarget < _softTurnAngle)
                {
                    _turnAmount = turnDirection * _turnForce * (angleToTarget / _softTurnAngle);
                }
                else if (angleToTarget < _hardTurnAngle)
                {
                    _turnAmount = turnDirection * _turnForce;
                }
                else
                {
                    _turnAmount = turnDirection;
                }


                _needBrake = false;
            }
            else
            {
                _needBrake = true;
            }

            _turnAmount = Mathf.Lerp(_prevTurnAmount, _turnAmount, Time.deltaTime * _wheelsRotateSpeed);
            _prevTurnAmount = _turnAmount;
        }
    }
}