using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using _Scripts.LevelsAndScenes.Levels.Interactables;
using _Scripts.Player;
using UnityEngine;
using Random = UnityEngine.Random;

// ReSharper disable InconsistentNaming

namespace _Scripts.TentacleScripts.CarrionVersion
{
    public class CarrionMover : MonoBehaviour
    {
        [SerializeField] private CarrionBase _carrionBase;
        [SerializeField] private float _extraTentacleLengthModifier = 1.1f;
        [SerializeField] private float _speedInfluenceModifier = .5f;
        [SerializeField] private float _safeZoneRadius = .5f;
        [SerializeField] private float _tentacleLength = 3f;
        [SerializeField] private float _maxTentacleLength = 10f;

        [SerializeField]
        private PlayerMover.PlayerMovementType _currentPlayerMovementType = PlayerMover.PlayerMovementType.Land;

        [SerializeField] private float _unusedTentacleLength = .5f;
        private readonly List<(int, Vector3, Vector3, Quaternion)> _tentaclesToMove = new();
        private Vector3 _moveDirection;
        private Vector3 _oldPosition;
        private int _raycastMask;
        private const float _tentacleLengthChangeSpeed = 4f;
        private float _defaultMoveSpeed;
        public Vector3 MoveDirection => _moveDirection;

        private PlayerMover.PlayerMovementType CurrentPlayerMovementType
        {
            get => _currentPlayerMovementType;
            set => _currentPlayerMovementType = value;
        }

        private void Start()
        {
            _raycastMask = ~(1 << LayerMask.NameToLayer("Player"));
        }

        private void Update()
        {
            foreach (var tentacle in _carrionBase.TentaclesManager.Tentacles) ChangeTentacleLength(tentacle);

            _moveDirection = transform.position - _oldPosition;
            _moveDirection.y = 0;
            _moveDirection.Normalize();
            _oldPosition = transform.position;
        }

        public void SetTentaclesPositionOnEvolution()
        {
            foreach (var tentacle in _carrionBase.TentaclesManager.Tentacles)
            {
                var targetPosition = Vector3.zero;
                if (Physics.Raycast(tentacle.tentacleAnimator.transform.position,
                        -tentacle.tentacleAnimator.transform.up, out var hit,
                        _maxTentacleLength, _raycastMask))
                    targetPosition = hit.point;

                if (targetPosition == Vector3.zero) continue;
                tentacle.legPoint.position = targetPosition;
            }
        }

        public void ChangeMovementType(PlayerMover.PlayerMovementType playerMovementType)
        {
            CurrentPlayerMovementType = playerMovementType;
            switch (CurrentPlayerMovementType)
            {
                case PlayerMover.PlayerMovementType.Land:
                    break;
                case PlayerMover.PlayerMovementType.Water:
                    ChangeTentaclesForSwimming();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void TentaclesAreGenerated()
        {
            StartCoroutine(GroupMover());
        }

        private IEnumerator GroupMover()
        {
            var delay = new WaitForSeconds(1f / _carrionBase.TentaclesManager.MaxGroupCount);
            var currentGroup = 0;
            while (this)
            {
                if (CurrentPlayerMovementType != PlayerMover.PlayerMovementType.Land)
                {
                    yield return delay;
                    continue;
                }

                currentGroup = (currentGroup + 1) % _carrionBase.TentaclesManager.MaxGroupCount;
                CheckForPositionsForTentacles();
                MoveTentacles(currentGroup);
                yield return delay;
            }
        }

        private void MoveTentacles(int groupIdx)
        {
            if (_tentaclesToMove.Count == 0) return;
            foreach (var t in _tentaclesToMove)
            {
                var (index, position, normal, rotation) = t;
                if (_carrionBase.TentaclesManager.Tentacles[index].group != groupIdx) continue;
                var tentacle = _carrionBase.TentaclesManager.Tentacles[index];
                if (tentacle.IsInUse) continue;
                var legPoint = tentacle.legPoint;
                var randomOffset = Random.onUnitSphere * _safeZoneRadius;
                randomOffset = Vector3.ProjectOnPlane(randomOffset, normal);
                legPoint.position = position + randomOffset;
                tentacle.ChangeTentacleIKState(true);
                tentacle.tentacleAnimator.transform.rotation = rotation;
                tentacle.normal = normal;
            }
        }

        public void ChangeTentaclesState()
        {
            foreach (var tentacle in _carrionBase.TentaclesManager.Tentacles)
                tentacle.ChangeTentacleIKState(!tentacle.tentacleAnimator.UseIK);
        }


#if UNITY_EDITOR


        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            foreach (var tentacle in _carrionBase.TentaclesManager.Tentacles.Where(tentacle => tentacle != null)
                         .Where(tentacle => tentacle.tentacleAnimator.UseIK))
                Gizmos.DrawSphere(tentacle.tentacleAnimator.IKTarget.position, .1f);

            foreach (var tentacle in _carrionBase.TentaclesManager.Tentacles)
            {
                if (tentacle == null) continue;
                if (tentacle.IsInUse) continue;
                if (!tentacle.tentacleAnimator.UseIK) continue;
                Gizmos.color = Color.blue;
                Gizmos.DrawRay(tentacle.legPoint.position, tentacle.normal);
            }

            Gizmos.color = Color.red;
        }

#endif

        private void CheckForPositionsForTentacles()
        {
            _tentaclesToMove.Clear();
            var speed = PlayerMover.Instance.CurrentSpeed;
            for (var i = 0; i < _carrionBase.TentaclesManager.Tentacles.Count; i++)
            {
                var tentacle = _carrionBase.TentaclesManager.Tentacles[i];
                if (tentacle.IsInUse) continue;
                var direction = transform.TransformDirection(tentacle.defaultForward) * _tentacleLength +
                                _moveDirection * (speed * _speedInfluenceModifier);

                if (Physics.Raycast(tentacle.tentacleAnimator.transform.position, direction.normalized, out var hit,
                        _maxTentacleLength, _raycastMask))
                {
                    if (hit.collider.gameObject.CompareTag("InteractableSurface"))
                    {
                        var surface = hit.collider.gameObject.GetComponent<InteractableSurface>();
                        if (surface != null)
                            CarrionTentaclesManager.CheckForInteractableSurface(tentacle, surface);
                    }
                    else
                    {
                        tentacle.SetMaterial(tentacle._defaultColor);
                    }

                    var legPoint = tentacle.legPoint;
                    if (Vector3.Distance(legPoint.transform.position, hit.point) > _safeZoneRadius)
                    {
                        direction.y = 0;
                        _tentaclesToMove.Add((i, hit.point, hit.normal, GetQuaternionLookAt(direction)));
                    }

                    ChangeTentacleLength(tentacle);
                }
                else
                {
                    tentacle.SetMaterial(tentacle._defaultColor);
                    tentacle.ChangeTentacleIKState(false);
                    ChangeTentacleLength(tentacle);
                    tentacle.tentacleAnimator.transform.rotation =
                        Quaternion.LookRotation(transform.TransformDirection(tentacle.defaultForward));
                }
            }
        }

        public static Quaternion GetQuaternionLookAt(Vector3 forward)
        {
            forward.Normalize();
            var nonCollinearVector = forward;
            if (!Mathf.Approximately(forward.x, 0f))
                nonCollinearVector.x = 0f;
            else
                nonCollinearVector.x = -nonCollinearVector.x;

            var up = Vector3.Cross(forward, nonCollinearVector).normalized;
            return Quaternion.LookRotation(forward, up);
        }

        private void ChangeTentacleLength(CarrionTentaclesManager.Tentacle tentacle)
        {
            if (!tentacle.isNeedToChangeLengthToTargetPoint) return;
            float length;
            if (tentacle.tentacleAnimator.UseIK)
            {
                var distance = Vector3.Distance(tentacle.tentacleAnimator.transform.position,
                    tentacle.legPoint.transform.position);
                if (distance > _maxTentacleLength) distance = _maxTentacleLength;
                length = distance / _tentacleLength;
                length *= _extraTentacleLengthModifier;
            }
            else
            {
                length = _unusedTentacleLength;
            }

            tentacle.tentacleAnimator.LengthMultiplier = Mathf.Lerp(tentacle.tentacleAnimator.LengthMultiplier,
                length,
                _tentacleLengthChangeSpeed * Time.deltaTime);
        }

        private void ChangeTentaclesForSwimming()
        {
            foreach (var tentacle in _carrionBase.TentaclesManager.Tentacles) tentacle.ChangeTentacleToSwimming(true);
        }

        public void EDITORSetLengthForAllTentacles()
        {
            var tentacles = _carrionBase.TentaclesManager.transform
                .GetComponentsInChildren<TailDemo_SkinnedMeshGenerator>();
            foreach (var tentacle in tentacles) tentacle.ForwardLength = _tentacleLength;
        }
    }
}