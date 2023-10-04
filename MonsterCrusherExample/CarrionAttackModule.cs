using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using _Scripts.Enemy.BaseScripts;
using _Scripts.TentacleScripts.CarrionVersion.AttackModules.AttackRange;
using _Scripts.TentacleScripts.CarrionVersion.AttackModules.CarrionSpecialAttacks;
using UnityEngine;

namespace _Scripts.TentacleScripts.CarrionVersion.AttackModules
{
    public class CarrionAttackModule : MonoBehaviour
    {
        [SerializeField] private CarrionBase _carrionBase;
        [SerializeField] private CarrionShooterModule _shooterModule;
        [SerializeField] private CarrionSpecialAttacksModule _specialAttacksModule;
        [SerializeField] private SphereCollider _attackCollider;
        [SerializeField] private int _defaultMaxAttackedEnemies = 5;
        [SerializeField] private int _defaultMaxTentaclesPerEnemy = 1;
        [SerializeField] private float _defaultDamage = 10f;
        [SerializeField] private AttackRangeSprite _attackRangeSprite;

        public readonly
            List<(EnemyBase enemy, List<CarrionTentaclesManager.Tentacle> tentacle, Transform pointToAttack)>
            AttackedEnemies = new();

        private readonly
            List<(EnemyBase enemy, List<CarrionTentaclesManager.Tentacle> tentacle, Transform pointToAttack)>
            _enemiesInEatProcess = new();

        private readonly List<EnemyBase> _enemiesAwaitingTentacles = new();

        #region Variables : CurrentUpgradeValues

        private float _defaultAttackRange;
        private int _currentMaxAttackedEnemies;
        private float _currentDamage;
        private int _currentMaxTentaclesPerEnemy;

        #endregion

        private bool _isReadyToAttack;

        public CarrionSpecialAttacksModule SpecialAttacksModule => _specialAttacksModule;

        public CarrionShooterModule ShooterModule => _shooterModule;

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, _attackCollider.radius);
        }
#endif

        private void Awake()
        {
            _currentMaxAttackedEnemies = _defaultMaxAttackedEnemies;
            _currentDamage = _defaultDamage;
            _currentMaxTentaclesPerEnemy = _defaultMaxTentaclesPerEnemy;
            _defaultAttackRange = _attackCollider.radius;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!_isReadyToAttack) return;
            if (other.TryGetComponent(out EnemyBase enemy)) OnEnemyHasEnteredTrigger(enemy);
        }

        private void OnTriggerExit(Collider other)
        {
            if (!_isReadyToAttack) return;
            if (other.TryGetComponent(out EnemyBase enemy)) OnEnemyHasExitedTrigger(enemy);
        }

        public async void TentaclesAreGenerated()
        {
            ShooterModule.TentaclesAreGenerated();
            _attackRangeSprite.SetAttackRange(_attackCollider.radius);
            _isReadyToAttack = true;
            if (_attackCollider != null)
            {
                _attackCollider.enabled = false;
                await Task.Delay(100);
                if (_attackCollider != null) _attackCollider.enabled = true;
            }
        }

        private void OnEnemyHasEnteredTrigger(EnemyBase enemy)
        {
            if (enemy.IsDead)
            {
                EatKilledEnemy(enemy);
                return;
            }

            enemy.OnCarrionEnter(_carrionBase.EvolutionStageBase);
            if (enemy.CanBeHangedOut)
            {
                if (AttackedEnemies.Count >= _currentMaxAttackedEnemies)
                {
                    _enemiesAwaitingTentacles.Add(enemy);
                    return;
                }

                StartAttackingEnemy(enemy);
            }
        }

        private void OnEnemyHasExitedTrigger(EnemyBase enemy)
        {
            if (enemy == null) return;
            enemy.OnCarrionExit(_carrionBase.EvolutionStageBase);
            if (_enemiesAwaitingTentacles.Contains(enemy)) _enemiesAwaitingTentacles.Remove(enemy);
            var tuple = AttackedEnemies.FirstOrDefault(x => x.Item1 == enemy);
            if (tuple.Item1 != null) enemy.OnNotAttacked(_carrionBase);
            AttackedEnemies.Remove(tuple);
            if (tuple.Item2 == null) return;
            foreach (var attackingTentacle in tuple.Item2)
            {
                if (attackingTentacle == null) continue;
                _carrionBase.TentaclesManager.busyTentacles.Remove(attackingTentacle);
                ReturnTentacleToFreePool(attackingTentacle);
            }
        }

        private void Update()
        {
            _attackRangeSprite.SetVisibility(AttackedEnemies.Count > 0);
            for (var i = 0; i < AttackedEnemies.Count; i++)
            {
                var (enemy, tentacles, point) = AttackedEnemies[i];
                if (enemy.IsDead)
                {
                    EatKilledEnemy(enemy);
                    continue;
                }

                if (enemy == null)
                {
                    AttackedEnemies.RemoveAt(i);
                    ReturnTentacleToFreePool(tentacles);
                    continue;
                }

                enemy.TakeDamage(_currentDamage * Time.deltaTime);
                foreach (var tentacle in tentacles.ToList()
                             .Where(tentacle => tentacle != null && enemy != null && point != null))
                {
                    var position = point.position;
                    tentacle.legPoint.position = position;
                    tentacle.LookAt(position);
                }
            }

            for (var i = 0; i < _enemiesInEatProcess.Count; i++)
            {
                var (enemy, tentacles, point) = _enemiesInEatProcess[i];
                if (enemy == null)
                {
                    _enemiesInEatProcess.RemoveAt(i);
                    foreach (var attackingTentacle in tentacles)
                    {
                        _carrionBase.TentaclesManager.busyTentacles.Remove(attackingTentacle);
                        ReturnTentacleToFreePool(attackingTentacle);
                    }

                    continue;
                }

                foreach (var tentacle in tentacles.ToList()
                             .Where(tentacle => tentacle != null && enemy != null && point != null))
                {
                    var position = point.position;
                    tentacle.legPoint.position = position;
                    tentacle.LookAt(position);
                }
            }
        }

        private void StartAttackingEnemy(EnemyBase enemy, Transform pointToAttack = null)
        {
            if (AttackedEnemies.Count >= _currentMaxAttackedEnemies) return;
            if (_enemiesAwaitingTentacles.Contains(enemy)) _enemiesAwaitingTentacles.Remove(enemy);
            if (AttackedEnemies.Any(x => x.Item1 == enemy)) return;

            var point = pointToAttack == null ? enemy.AttackPoint : pointToAttack;
            enemy.DeadAction += OnEnemyDead;
            var tentacles = _carrionBase.TentaclesManager.GetFreeTentacles(_currentMaxTentaclesPerEnemy);
            if (tentacles == null) return;
            _carrionBase.TentaclesManager.busyTentacles.AddRange(tentacles);
            AttackedEnemies.Add((enemy, tentacles, point));
            enemy.OnAttacked(_carrionBase.EvolutionStageBase);
            foreach (var tentacle in tentacles) tentacle.AttackEnemy(enemy);
        }

        private void ReturnTentacleToFreePool(CarrionTentaclesManager.Tentacle tentacle)
        {
            _carrionBase.TentaclesManager.ReturnTentacleToFreePool(tentacle);
            if (AttackedEnemies.Count >= _currentMaxAttackedEnemies) return;
            var enemy = _enemiesAwaitingTentacles.FirstOrDefault();
            if (enemy == null) return;
            _enemiesAwaitingTentacles.Remove(enemy);
            StartAttackingEnemy(enemy);
        }

        private void ReturnTentacleToFreePool(List<CarrionTentaclesManager.Tentacle> tentacles)
        {
            foreach (var tentacle in tentacles)
            {
                ReturnTentacleToFreePool(tentacle);
            }
        }

        public void ReleaseAttackedEnemies()
        {
            foreach (var (enemy, tentacles, point) in AttackedEnemies)
            {
                if (enemy == null) continue;
                enemy.OnNotAttackedAndThrow(_carrionBase, enemy.transform.position);
            }

            AttackedEnemies.Clear();
            _carrionBase.TentaclesManager.busyTentacles
                .Clear();
            _carrionBase.TentaclesManager.freeTentacles
                .Clear();
            _carrionBase.TentaclesManager.freeTentacles
                .AddRange(_carrionBase.TentaclesManager.Tentacles);
        }

        private void OnEnemyDead(EnemyBase enemy)
        {
            if (enemy == null) return;
            StartCoroutine(OnEnemyDeadCoroutine(enemy));
        }

        private IEnumerator OnEnemyDeadCoroutine(EnemyBase enemy)
        {
            yield return new WaitForSeconds(0.5f);
            if (_enemiesAwaitingTentacles.Contains(enemy)) _enemiesAwaitingTentacles.Remove(enemy);
            var tuple = AttackedEnemies.FirstOrDefault(x => x.Item1 == enemy);
            if (tuple.Item1 != null) enemy.OnNotAttacked(_carrionBase);
            AttackedEnemies.Remove(tuple);
            if (tuple.Item2 == null) yield break;
            foreach (var attackingTentacle in tuple.Item2)
            {
                if (attackingTentacle == null) continue;
                _carrionBase.TentaclesManager.busyTentacles.Remove(attackingTentacle);
                ReturnTentacleToFreePool(attackingTentacle);
            }
        }

        private void EatKilledEnemy(EnemyBase enemy)
        {
            if (enemy == null) return;
            if (!enemy.CanBeHarvested) return;
            foreach (var tuple in AttackedEnemies.ToList().Where(tuple => tuple.enemy == enemy))
            {
                AttackedEnemies.Remove(tuple);
                ReturnTentacleToFreePool(tuple.tentacle);
            }

            if (enemy.IsDead)
            {
                enemy.DeadAction -= OnEnemyDead;
                var isWillBeEaten =
                    StartEatingDeadEnemy(enemy);
                if (isWillBeEaten) enemy.InitiateMovingToCarrion(_carrionBase.EvolutionStageBase);
            }
        }

        private bool StartEatingDeadEnemy(EnemyBase enemy)
        {
            if (_enemiesAwaitingTentacles.Contains(enemy)) _enemiesAwaitingTentacles.Remove(enemy);
            if (AttackedEnemies.Any(x => x.Item1 == enemy)) return false;
            var point = enemy.DeadRagdollManager.RagdollMainRigidbody.transform;
            var tentacles = _carrionBase.TentaclesManager.GetFreeTentacles(_currentMaxTentaclesPerEnemy);
            if (tentacles == null) return false;
            _enemiesInEatProcess.Add((enemy, tentacles, point));
            _carrionBase.TentaclesManager.busyTentacles.AddRange(tentacles);
            foreach (var tentacle in tentacles) tentacle.AttackEnemy(enemy);
            return true;
        }

        public void UpgradeMaxAttackingTentacles(float toAdd)
        {
            _currentMaxAttackedEnemies = _defaultMaxAttackedEnemies + Mathf.RoundToInt(toAdd);
        }

        public void UpgradeMaxTentaclePerEnemy(int toAdd)
        {
            _currentMaxTentaclesPerEnemy = _defaultMaxTentaclesPerEnemy + toAdd;
        }

        public void UpgradeTentacleAttackDamage(float toMultiply)
        {
            _currentDamage = _defaultDamage * toMultiply;
        }

        public void UpgradeAttackRange(float attackRangeValue)
        {
            _attackCollider.radius = _defaultAttackRange * attackRangeValue;
            _attackRangeSprite.SetAttackRange(_attackCollider.radius);
        }

        public void UpgradeAttackTargetsCount(int attackTargetsCountValue)
        {
            _currentMaxAttackedEnemies = _defaultMaxAttackedEnemies + attackTargetsCountValue;
        }

        public void UpgradeShooting(int levelUpgrade)
        {
            ShooterModule.UpgradeShooting(levelUpgrade);
        }
    }
}