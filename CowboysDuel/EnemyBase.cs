using System;
using System.Collections;
using System.Collections.Generic;
using _ImportedAssets.Cowsins.Scripts.Enemies;
using _ImportedAssets.Cowsins.Scripts.Player;
using _Scripts.Core.Audio;
using _Scripts.Core.Characters;
using _Scripts.Core.Characters.Common;
using _Scripts.Core.Level.Duel;
using _Scripts.Technical;
using RootMotion.FinalIK;
using UnityEngine;

namespace _Scripts.Core.Enemies
{
    public class EnemyBase : EnemyHealth
    {
#if UNITY_EDITOR
        public bool EDITORONLYCanMove = true;
        public Transform EDITORONLYTarget;
#endif
        [field: SerializeField] public bool IsMale { get; private set; } = true;
        [field: SerializeField] public string Name { get; set; }
        [field: SerializeField] public Sprite EnemyIcon { get; private set; }
        [field: SerializeField] public Sprite SquareEnemyIcon { get; private set; }
        [field: SerializeField] public Animator Animator { get; private set; }
        [field: SerializeField] public CowboysCharacterRagdollBase RagdollManager { get; private set; }
        [field: SerializeField] public CowboysCharacterAnimationBase AnimationManager { get; private set; }
        [field: SerializeField] public EnemyMover Mover { get; private set; }
        [field: SerializeField] public EnemyShootingModule ShootingModule { get; private set; }
        [field: SerializeField] public EnemyInjuriesSystem InjuriesSystem { get; private set; }
        [field: SerializeField] public FullBodyBipedIK FullBodyBipedIk { get; private set; }
        [field: SerializeField] public LookAtIK LookAtIk { get; private set; }
        [SerializeField] private AudioModule _onDamageAudioModule;
        [SerializeField] private List<AudioClip> _onDamageAudioClips;
        [SerializeField] private List<AudioClip> _onDeathAudioClips;
        public bool IsStunned { get; private set; } = false;
        private Coroutine _stunnedCoroutine;
        public EnemyPreset Preset { get; private set; }
        private BehaveTree _behaveTree;
        private bool IsUnderAim { get; set; }
        private Coroutine _makeDecisionCoroutine;
        private PlayerStats _playerStats;

        #region Moving variables

        private const float TimeBetweenMovingPointChange = 5f;
        private float _timeSinceLastMovingPointChange = 0f;

        #endregion

        public override void Start()
        {
            base.Start();
            _behaveTree = new BehaveTree(this);
            _behaveTree.PrintTree();
            _makeDecisionCoroutine = StartCoroutine(MakeDecisionCoroutine());
        }

        public override void Update()
        {
            base.Update();
            _timeSinceLastMovingPointChange += Time.deltaTime;
        }

        public void Initialize(EnemyPreset preset, PlayerStats playerStats, DuelZoneController duelZoneController)
        {
            Preset = preset;
            _playerStats = playerStats;
            Mover.Initialize(this, playerStats, duelZoneController);
            ShootingModule.Initialize(this, preset, playerStats);
            var direction = playerStats.transform.position - transform.position;
            direction.y = 0;
            transform.rotation = Quaternion.LookRotation(direction);
            LookAtIk.solver.target = playerStats.transform;
        }

        public override void Damage(float damage, CharacterInjuriesSystem.CharacterBodyPart? part,
            RaycastHit? hit = null)
        {
            base.Damage(damage, part);
            _onDamageAudioModule.PlaySound(_onDamageAudioClips[UnityEngine.Random.Range(0, _onDamageAudioClips.Count)]);
            InjuriesSystem.AddDamageToPart(part);
            if (_stunnedCoroutine != null)
            {
                StopCoroutine(_stunnedCoroutine);
                _stunnedCoroutine = null;
            }

            _stunnedCoroutine ??= StartCoroutine(Stun(Preset.RecoveryAfterInjuryTime));
        }

        private IEnumerator Stun(float duration)
        {
            IsStunned = true;
            yield return new WaitForSeconds(duration);
            IsStunned = false;
        }

        protected override void Die(CharacterInjuriesSystem.CharacterBodyPart? characterBodyPart)
        {
            // Custom event on damaged
            events.OnDeath.Invoke();
            _onDamageAudioModule.PlaySound(_onDeathAudioClips[UnityEngine.Random.Range(0, _onDeathAudioClips.Count)]);
            Mover.OnDieHandler();
            AnimationManager.OnDieHandler();
            Animator.enabled = false;
            ShootingModule.OnDieHandler();
            Destroy(FullBodyBipedIk);
            Destroy(LookAtIk);
            Destroy(InjuriesSystem);
            RagdollManager.SetRagdollState(true);
            //
            if (characterBodyPart != null)
            {
                var directionFromPlayer = _playerStats.transform.position - transform.position;
                RagdollManager.AddForceToPart(characterBodyPart.Value, directionFromPlayer.normalized);
            }

            Destroy(this);
        }

        public void UnholsterWeapon()
        {
            ShootingModule.UnholsterWeapon();
        }

        private IEnumerator MakeDecisionCoroutine()
        {
            var delay = new WaitForSeconds(Preset.MakeDecisionTime);
            while (this)
            {
                if (InGamePauseManager.IsPaused)
                {
                    yield return null;
                    continue;
                }

                _behaveTree.ExecuteTree();
                yield return delay;
            }
        }

        public void SetIsUnderAim(bool state)
        {
            IsUnderAim = state;
            Mover.SetSpeedModifier(state ? 1.5f : 1f);
        }

        private abstract class Node
        {
            public string Name = "";
            public abstract bool Execute(int depth, bool needLog = false);

            protected void LogNode(int depth)
            {
                Debug.Log(new string('-', depth) + Name);
            }
        }

        private class ActionNode : Node
        {
            private readonly Action _action;

            public ActionNode(string name, Action action)
            {
                Name = name;
                _action = action;
            }

            public override bool Execute(int depth, bool needLog = false)
            {
                if (needLog) LogNode(depth);
                _action();
                return true;
            }
        }

        private class ConditionNode : Node
        {
            private readonly Func<bool> _condition;
            private readonly bool _invert;

            public ConditionNode(string name, Func<bool> condition, bool invert = false)
            {
                Name = name;
                _condition = condition;
                _invert = invert;
            }

            public ConditionNode Invert()
            {
                return new ConditionNode(Name, _condition, !_invert);
            }

            public override bool Execute(int depth, bool needLog = false)
            {
                if (needLog) LogNode(depth);
                var result = _condition();
                return _invert ? !result : result;
            }
        }

        private class SelectorNode : Node
        {
            private readonly List<Node> _nodes;

            public SelectorNode(string name, List<Node> nodes)
            {
                Name = name;
                _nodes = nodes;
            }

            public override bool Execute(int depth, bool needLog = false)
            {
                if (needLog) LogNode(depth);
                foreach (var node in _nodes)
                {
                    if (node.Execute(depth + 1))
                    {
                        //print("SelectorNode: " + node.Name + " executed");
                        return true;
                    }
                }

                return false;
            }
        }

        private class SequenceNode : Node
        {
            private readonly List<Node> _nodes;

            public SequenceNode(string name, List<Node> nodes)
            {
                Name = name;
                _nodes = nodes;
            }

            public override bool Execute(int depth, bool needLog = false)
            {
                if (needLog) LogNode(depth);
                foreach (var node in _nodes)
                {
                    if (!node.Execute(depth + 1))
                    {
                        return false;
                    }
                }

                return true;
            }
        }

        private class BehaveTree
        {
            private readonly SelectorNode _root;
            private readonly EnemyBase _enemyBase;

            public BehaveTree(EnemyBase enemyBase)
            {
                _enemyBase = enemyBase;

                #region ActionNodes

                var attackNode = new ActionNode("Attack", Attack);
                var attackWhileMovingNode = new ActionNode("AttackWhileMoving", AttackWhileMoving);
                var evadeNode = new ActionNode("Evade", Evade);
                var movingNode = new ActionNode("Moving", Move);
                var stunnedNode = new ActionNode("Stunned", Stun);

                #endregion

                #region ConditionNodes

                // Intuition condition
                var intuitionCondition = new ConditionNode("IsAimedByPlayer", () => _enemyBase.IsUnderAim);
                // stun condition
                var stunnedCondition = new ConditionNode("IsStunned", () => _enemyBase.IsStunned);
                var notIntuitionCondition = intuitionCondition.Invert();

                #endregion

                // An additional Sequence Node to ensure each action only runs when their condition is true

                #region Check for any long-time node in progress

                var isAimingInProgressCondition =
                    new ConditionNode("isAimingInProgressCondition", () => _enemyBase.ShootingModule.AttackInProgress);
                var breakTreeExecutionBranch = new SequenceNode("BreakTreeExecutionSequence",
                    new List<Node> {isAimingInProgressCondition});

                #endregion

                #region Attack

                // Aiming condition ----------------------------------
                var attackCondition =
                    new ConditionNode("attackCondition", IsReadyForAttack);
                // attack while moving condition ----------------------------------
                var attackWhileMovingCondition =
                    new ConditionNode("attackWhileMovingCondition", IsReadyForAttackWhileMoving);
                // inversed ----------------------------------
                var notReadyForAttack = attackCondition.Invert();
                // branches ----------------------------------
                var attackWhileMovingSubBranch = new SequenceNode("AttackWhileMovingSequence",
                    new List<Node> {attackWhileMovingCondition, attackWhileMovingNode});
                var attackAtPlaceSubBranch = new SequenceNode("AttackAtPlaceSequence",
                    new List<Node> {attackCondition, attackNode});
                var attackbranch = new SelectorNode("AttackSelector",
                    new List<Node> {attackWhileMovingSubBranch, attackAtPlaceSubBranch});

                #endregion

                var evadeSubBranch = new SequenceNode("EvadeSequence",
                    new List<Node> {intuitionCondition, evadeNode});

                var movingSubBranch = new SequenceNode("MovingSequence",
                    new List<Node> {notReadyForAttack, notIntuitionCondition, movingNode});
                var stunnedSubBranch = new SequenceNode("StunnedSequence",
                    new List<Node> {stunnedCondition, stunnedNode});

                _root = new SelectorNode("Root",
                    new List<Node> {breakTreeExecutionBranch, stunnedSubBranch, attackbranch, evadeSubBranch, movingSubBranch});
            }

            private bool IsReadyForAttack()
            {
                return _enemyBase.ShootingModule.IsReadyForAiming();
            }

            private bool IsReadyForAttackWhileMoving()
            {
                var random = UnityEngine.Random.Range(0, 1);
                var presetValueFOrAttackWhileMoving = _enemyBase.Preset.Decisions.ShootWhileMovingChance;
                var result = random <= presetValueFOrAttackWhileMoving;
                return IsReadyForAttack() && result;
                // если шанс меньше, чем рандомное число, то атаковать
                // если шанс больше, чем рандомное число, то не атаковать
                // в конце проверяем готов ли он к атаке и готов ли атаковать во время движения
            }

            private void Stun()
            {
                if (InGamePauseManager.IsPaused)
                {
                    return;
                }

                _enemyBase.Mover.StopMoving();
            }

            public void PrintTree()
            {
                _root.Execute(0, true); // Запускаем корневой узел с глубиной 0
            }

            public void ExecuteTree()
            {
                _root.Execute(0);
            }

            private void Attack()
            {
                if (InGamePauseManager.IsPaused)
                {
                    return;
                }

                _enemyBase.ShootingModule.Attack();
            }

            private void AttackWhileMoving()
            {
                if (InGamePauseManager.IsPaused)
                {
                    return;
                }

                _enemyBase.ShootingModule.Attack();
                Move();
            }

            private void Evade()
            {
                if (InGamePauseManager.IsPaused)
                {
                    _enemyBase.Mover.StopMoving();
                    return;
                }

                _enemyBase.Mover.SetEvadeDestination();
            }

            private void Move()
            {
#if UNITY_EDITOR
                if (!_enemyBase.EDITORONLYCanMove)
                {
                    _enemyBase.Mover.StopMoving();
                    return;
                }

                if (_enemyBase.EDITORONLYTarget != null)
                {
                    _enemyBase.Mover.SetDestination(_enemyBase.EDITORONLYTarget.position);
                    return;
                }
#endif
                if (InGamePauseManager.IsPaused)
                {
                    _enemyBase.Mover.StopMoving();
                    return;
                }

                if (!(_enemyBase._timeSinceLastMovingPointChange > TimeBetweenMovingPointChange))
                {
                    return;
                }

                _enemyBase._timeSinceLastMovingPointChange = 0f;
                _enemyBase.Mover.SetRandomDestination();
            }
        }
    }
}