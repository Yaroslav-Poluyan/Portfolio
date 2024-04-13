using System.Collections.Generic;
using RootMotion.Demos;
using UnityEngine;
using static _Scripts.CodeBase.Gameplay.AI.BehaveTreeBase.BehaveTreeNodes;
using Random = UnityEngine.Random;

namespace _Scripts.CodeBase.Gameplay.AI.Gladiator
{
    public class BehaveTree
    {
        private readonly SelectorNode _root;
        private readonly AIController _enemyBase;

        public BehaveTree(AIController enemyBase)
        {
            _enemyBase = enemyBase;

            #region Duel

            #region Attack

            var attackNode = new ActionNode("Attack", Attack);
            var punchNode = new ActionNode("Punch", ShieldPunch);
            var kickNode = new ActionNode("Kick", Kick);
            //
            var isEnemyNearCondition = new ConditionNode("isEnemyNearCondition", _enemyBase.IsEnemyNear);
            var readyForAttackCondition = new ConditionNode("attackCondition", IsReadyForAttack);
            var shieldPunchCondition = new ConditionNode("shieldPunchCondition", IsReadyForShieldPunch);
            var kickCondition = new ConditionNode("kickCondition", IsReadyForKick);
            var checkForAttackNode = new SequenceNode("CheckForAttack", new List<Node>
            {
                readyForAttackCondition,
                attackNode
            });
            var checkForShieldPunchNode = new SequenceNode("CheckForShieldPunch", new List<Node>
            {
                shieldPunchCondition,
                punchNode
            });
            var checkForKickNode = new SequenceNode("CheckForKick", new List<Node>
            {
                kickCondition,
                kickNode
            });
            //
            var attackSubbranch = new SelectorNode("AttackSubbranch", new List<Node>
            {
                checkForShieldPunchNode, checkForKickNode, checkForAttackNode
            });
            var attackBranch = new SequenceNode("AttackSelector", new List<Node>
            {
                isEnemyNearCondition, attackSubbranch
            });

            #endregion

            #region Defense

            // dash
            var dashNode = new ActionNode("Dash", Dash);
            var dashCondition = new ConditionNode("DashCondition", IsReadyForDash);
            var dashSubSubbranch = new SequenceNode("DashSubSubbranch", new List<Node>
            {
                dashCondition, dashNode
            });
            // evade
            var evadeNode = new ActionNode("Evade", Evade);
            var evadeCondition = new ConditionNode("EvadeCondition", IsReadyForEvade);
            var evadeSubSubbranch = new SequenceNode("EvadeSubSubbranch", new List<Node>
            {
                evadeCondition, evadeNode
            });
            var isUnderAttackCondition = new ConditionNode("isUnderAttackCondition", IsNeedDefenseActions);
            // roll
            var rollNode = new ActionNode("Roll", Roll);
            var rollCondition = new ConditionNode("RollCondition", IsReadyForRoll);
            var rollSubSubbranch = new SequenceNode("RollSubSubbranch", new List<Node>
            {
                rollCondition, rollNode
            });
            // choose defense subbranch
            var chooseDefenseSubbranch = new SelectorNode("ChooseDefenseSubbranch", new List<Node>
            {
                dashSubSubbranch, rollSubSubbranch, evadeSubSubbranch, /*blockSubSubbranch*/
            });
            // defense branch
            var defenseBranch = new SequenceNode("IsUnderAttackSequence", new List<Node>
            {
                isUnderAttackCondition, chooseDefenseSubbranch
            });

            #endregion

            #region Moving

            var movingNode = new ActionNode("Moving", Move);
            var movingBranch = new SequenceNode("MovingSequence", new List<Node>
            {
                isEnemyNearCondition.Invert(), movingNode
            });

            #endregion

            #region Tired

            var isTiredCondition = new ConditionNode("isTiredCondition", IsTired);
            var tiredNode = new ActionNode("Tired", Evade);
            var tiredBranch = new SequenceNode("TiredSequence", new List<Node>
            {
                isTiredCondition, tiredNode
            });

            #endregion

            var duelCondition = new ConditionNode("DuelCondition",
                () => _enemyBase.AIControls.CurrentState == AIControls.AIState.AttackingPlayer);

            var duelBranch = new SelectorNode("DuelBranch", new List<Node>
            {
                tiredBranch,
                defenseBranch,
                attackBranch,
                movingBranch
            });
            var duelSequence = new SequenceNode("DuelSequence", new List<Node>
            {
                duelCondition, duelBranch
            });

            #endregion

            #region MovingToSpawnPoint

            var movingToSpawnPointCondition = new ConditionNode("MovingToSpawnPointCondition",
                () => _enemyBase.AIControls.CurrentState == AIControls.AIState.MovingToSpawnPoint);
            var movingToSpawnPointNode = new ActionNode("MovingToSpawnPoint", MoveToSpawnPoint);
            var movingToSpawnPointBranch = new SequenceNode("MovingToSpawnPointSequence", new List<Node>
            {
                movingToSpawnPointCondition, movingToSpawnPointNode
            });

            #endregion

            #region StandBy

            var standByCondition = new ConditionNode("StandByCondition",
                () => _enemyBase.AIControls.CurrentState == AIControls.AIState.Standby);
            var standByNode = new ActionNode("StandBy", StandBy);
            var standByBranch = new SequenceNode("StandBySequence", new List<Node>
            {
                standByCondition, standByNode
            });

            #endregion

            _root = new SelectorNode("Root", new List<Node>
            {
                duelSequence,
                movingToSpawnPointBranch,
                standByBranch
            });
        }

        private bool IsTired()
        {
            return _enemyBase.Stamina.IsTired;
        }

        private bool IsNeedDefenseActions()
        {
            return _enemyBase.IsUnderAttack() && _enemyBase.IsEnemyNear();
        }

        private bool IsReadyForEvade()
        {
            return false;
        }

        private bool IsReadyForDash()
        {
            return false;
        }

        private bool IsReadyForRoll()
        {
            if (!_enemyBase.Stamina.IsEnoughStamina(_enemyBase.Stamina.RollStaminaCost)) return false;
            if (_enemyBase.CharacterMelee.IsKickInProgress) return false;
            if (_enemyBase.CharacterMelee.IsShieldPunchInProgress) return false;
            return Random.Range(0f, 1f) < _enemyBase.AIControls.GetEnemyPreset._rollChance;
        }

        private void Evade()
        {
            Debug.Log("AI is Evading...");
            _enemyBase.AIControls.GetState().actionIndex = ActionType.None;
            _enemyBase.AIControls.EvadeEnemies();
        }

        private void Move()
        {
            _enemyBase.AIControls.GetState().actionIndex = ActionType.None;
            _enemyBase.AIControls.MoveTowardsLockedTarget();
        }

        private void MoveToSpawnPoint()
        {
            _enemyBase.AIControls.GetState().actionIndex = ActionType.None;
            _enemyBase.AIControls.MoveToSpawnPoint();
        }

        private void StandBy()
        {
            _enemyBase.AIControls.GetState().lookPos = _enemyBase.LockedTarget.GetTransform().position;
            _enemyBase.AIControls.GetState().actionIndex = ActionType.None;
        }

        private void Dash()
        {
            _enemyBase.AIControls.GetState().actionIndex = ActionType.None;
            _enemyBase.AIControls.GetState().dash = true;
        }

        private void Roll()
        {
            _enemyBase.AIControls.GetState().actionIndex = ActionType.None;
            _enemyBase.AIControls.GetState().roll = true;
            /*
            _enemyBase.AIControls.OnRollPressed?.Invoke();
        */
        }

        private void Attack()
        {
            if (_enemyBase.AIControls.GetState().actionIndex is ActionType.AttackLeft or ActionType.AttackLeft
                or ActionType.AttackRight) return;
            if (_enemyBase.CharacterAnimationMelee.ActionInProgressAnimation is ActionType.AttackLeft
                or ActionType.AttackRight or ActionType.AttackUp) return;
            _enemyBase.FightingModule.Attack();
        }

        private void ShieldPunch()
        {
            _enemyBase.ControlsBase.ProceedSecondAbility();
            Debug.Log("AI ShieldPunch");
        }

        private void Kick()
        {
            _enemyBase.ControlsBase.ProceedFirstAbility();
            Debug.Log("AI Kick");
        }

        private bool IsReadyForAttack()
        {
            if (_enemyBase.CharacterMelee.IsShieldPunchInProgress || _enemyBase.CharacterMelee.IsKickInProgress)
                return false;
            return _enemyBase.FightingModule.IsReadyForAttack();
        }

        private bool IsReadyForShieldPunch()
        {
            if (_enemyBase.CharacterMelee.IsShieldPunchInProgress) return false;
            if (_enemyBase.CharacterMelee.IsKickInProgress) return false;
            if (!_enemyBase.CharacterMelee.IsReadyForShieldPunch()) return false;
            return Random.Range(0f, 1f) < _enemyBase.AIControls.GetEnemyPreset._firstAbilityUseChance;
        }

        private bool IsReadyForKick()
        {
            if (_enemyBase.CharacterMelee.IsKickInProgress) return false;
            if (_enemyBase.CharacterMelee.IsShieldPunchInProgress) return false;
            if (!_enemyBase.CharacterMelee.IsReadyForKick()) return false;
            return Random.Range(0f, 1f) < _enemyBase.AIControls.GetEnemyPreset._sceondAbilityUseChance;
        }

        public void ExecuteTree()
        {
            _root.Execute(0);
        }
    }
}