﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using _Scripts.Common.Unit;
using _Scripts.Enemies.Common;
using UnityEngine;

namespace _Scripts.Common.ShootModules.Turrets
{
    public class Gatling : TurretBase
    {
        protected override void RotationAndTargetChoosingLogic()
        {
            var nearestEnemy = GetNearestEnemy();
            if (nearestEnemy == null) return;
            LookAtTarget(nearestEnemy);
            if (CanShoot && _currentReloadTime <= 0 && CurrentAimingTarget != nearestEnemy)
            {
                if (CurrentAimingTarget == null) CurrentAimingTarget = nearestEnemy;
                if (ParentUnitBase is EnemyBase) Shoot(nearestEnemy, .5f);
            }
        }

        private void LookAtTarget(UnitBase enemyToLook)
        {
            if (enemyToLook == null) return;
            var newY = Quaternion.LookRotation(enemyToLook.transform.position - _turretHead.position)
                .eulerAngles.y;
            var newRotation = Quaternion.Euler(_xRotationInFirePreparing, newY, _turretHead.rotation.z);
            _turretHead.rotation =
                Quaternion.Lerp(_turretHead.rotation, newRotation, Time.deltaTime * HeadRotationSpeed);
        }

        public override void Shoot(UnitBase truckGunCurrentAimingTarget, float aimPercent)
        {
            StartCoroutine(ShootMissilesInTarget(CurrentAimingTarget, aimPercent));
        }

        public override void Shoot(List<UnitBase> targets, float truckGunAimingTime)
        {
            StartCoroutine(ShootMissilesInTarget(targets.FirstOrDefault(), truckGunAimingTime));
        }

        public override void DrawAimArcToTarget(UnitBase target, float aimPercent)
        {
            DrawAimArcToTarget(new List<UnitBase> {target}, aimPercent);
        }

        public override void DrawAimArcToTarget(List<UnitBase> targets, float aimPercent)
        {
            if (targets.Count <= _aimingCircles.Count)
            {
                for (var i = targets.Count; i < _aimingCircles.Count; i++)
                {
                    _aimingCircles[i].enabled = false;
                    _aimingCircles[i].sprite = _aimTakenCircle;
                }
            }

            if (_aimingCircles.Count < targets.Count)
            {
                var neededExtraAimingCirclesCount = targets.Count - _aimingCircles.Count;
                for (var i = 0; i < neededExtraAimingCirclesCount; i++)
                {
                    var newAimingCircle = Instantiate(_aimingCirclePrefab, targets[i].transform.position,
                        Quaternion.identity);
                    newAimingCircle.transform.SetParent(transform);
                    _aimingCircles.Add(newAimingCircle);
                }
            }

            foreach (var target in targets)
            {
                if (target == null) continue;
                var aimingCircleToUse = _aimingCircles[targets.IndexOf(target)];
                aimingCircleToUse.enabled = true;
                aimingCircleToUse.transform.position = target.transform.position + Vector3.up * 6f;
                if (1 - aimPercent >= 0.3f)
                {
                    aimingCircleToUse.transform.localScale = _aimingCirclePrefab.transform.localScale * Vector3.one.x /
                        transform.localScale.x * (1 - aimPercent);
                    aimingCircleToUse.sprite = _aimingCircle;
                }
                else
                {
                    aimingCircleToUse.transform.localScale = _aimingCirclePrefab.transform.localScale * Vector3.one.x /
                        transform.localScale.x * .7f;
                    aimingCircleToUse.sprite = _aimTakenCircle;
                }

                aimingCircleToUse.transform.rotation =
                    Quaternion.LookRotation(transform.position - target.transform.position);
            }
        }

        private IEnumerator ShootMissilesInTarget(UnitBase unitBase, float aimPercent)
        {
            var delay = new WaitForSeconds(0.05f);
            var target = unitBase;
            CanShoot = false;
            _currentReloadTime = _reloadTime;
            for (var i = 0; i < _missilesCount; i++)
            {
                var nearestEnemy = GetNearestEnemy();
                if (nearestEnemy == null)
                {
                    yield return delay;
                    continue;
                }

                if (nearestEnemy != target)
                {
                    target = nearestEnemy;
                }

                var missile = Instantiate(_missilePrefab, _spawnPoint.position, Quaternion.identity);
                //DrawAimArcToTarget(target, aimPercent);
                var offset = Random.insideUnitSphere * (Scatter * (1 - aimPercent));
                if (aimPercent == 1)
                {
                    offset = Vector3.zero;
                }

                offset.y = 0;
                missile.Init(target, offset, ParentUnitBase, _damage);
                print("Missile shooted with offset " + offset + " and aim percent " + aimPercent);
                yield return delay;
            }

            SetTurretInIdleState();
        }
    }
}