using System;
using System.Collections.Generic;
using _Scripts.Core.EconomySimulation;
using _Scripts.Core.EconomySimulation.EconomyEntities;
using _Scripts.Core.EconomySimulation.Pops;
using _Scripts.Core.Regions;
using _Scripts.Core.World;
using _Scripts.Stats;
using _Scripts.Technical.Logs;
using _Scripts.UI.ResourcesAndProduction;
using _Scripts.UI.Stats;
using Mirror;
using UnityEngine;
using Random = UnityEngine.Random;

// ReSharper disable InconsistentNaming

namespace _Scripts.Network.Client
{
    public class Client : NetworkBehaviour
    {
        public static Client Instance;
        private uint _rpcNum;

        [SyncVar(hook = nameof(OnRandomSeedChanged))]
        public int RandomSeed;

        [field: SerializeField] public PlayerAvatar LocalPlayer { get; private set; }

        #region SpeedVariables

        public event Action OnSpeedChangedEvent;
        public readonly float[] GameSpeed = {.25f, .5f, 1, 2, 3, 10};

        [SyncVar(hook = nameof(OnSpeedChanged))]
        public int _currentSpeedIndex = 3;

        [SyncVar(hook = nameof(OnPauseChanged))]
        public bool _isPaused;

        public float CurrentSpeed => GameSpeed[_currentSpeedIndex];

        #endregion

        #region Tick

        public static int Tick { get; private set; }

        [SerializeField] private float TickInterval = 1f;
        private float _currentTickTime;

        #endregion

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        private void Start()
        {
            if (isServer) CreateRandomSeed();
        }

        private void Update()
        {
            if (isServer) ServerUpdate();
        }

        [Server]
        private void CreateRandomSeed()
        {
            RandomSeed = Random.Range(0, int.MaxValue);
        }

        // ReSharper disable once UnusedParameter.Local
        private void OnRandomSeedChanged(int oldSeed, int newSeed)
        {
            _rpcNum += 1;
            RandomSeed = newSeed;
            Random.InitState(RandomSeed);
            WorldManager.Instance.InitWorld();
            EcomomyEntityManager.Instance.InitWorld();
            PopManager.Instance.InitWorld();
            VacuumnajaLogger.Instance.LogEvent(_rpcNum, "RandomSeedCreated", new Dictionary<string, string>
            {
                {"Seed", RandomSeed.ToString()}
            });
        }

        [Server]
        private void ServerUpdate()
        {
            if (_isPaused) return;
            _currentTickTime += Time.deltaTime;
            while (_currentTickTime >= TickInterval / CurrentSpeed)
            {
                _currentTickTime -= TickInterval / CurrentSpeed;
                RpcTick();
            }
        }

        [ClientRpc]
        private void RpcTick()
        {
            _rpcNum += 1;
            VacuumnajaLogger.Instance.LogTick(_rpcNum);
            StatsManager.Instance.Tick();
            EconomyManager.Instance.Tick();
            EconomyManager.Instance.EndOfTick();
            //StatsUI.Instance.Tick();
            print("Tick" + _rpcNum);
            Tick++;
        }

        public void SetLocalPlayerAvatar(PlayerAvatar playerAvatar)
        {
            LocalPlayer = playerAvatar;
        }

        #region SpeedMethods

        [Command(requiresAuthority = false)]
        public void CmdGameSpeedIncrease()
        {
            if (_currentSpeedIndex < GameSpeed.Length - 1)
            {
                _currentSpeedIndex++;
            }
        }

        [Command(requiresAuthority = false)]
        public void CmdGameSpeedDecrease()
        {
            print("CmdGameSpeedDecrease");
            if (_currentSpeedIndex > 0)
            {
                _currentSpeedIndex--;
            }
        }

        [Command(requiresAuthority = false)]
        public void CmdGameSpeedPause()
        {
            print("CmdGameSpeedPause");
            _isPaused = !_isPaused;
        }

        [Command(requiresAuthority = false)]
        public void CmdGameSpeedContinue()
        {
            print("CmdGameSpeedContinue");
            _isPaused = !_isPaused;
        }

        // ReSharper disable once UnusedParameter.Local
        private void OnSpeedChanged(int oldSpeed, int newSpeed)
        {
            _currentSpeedIndex = newSpeed;
            OnSpeedChangedEvent?.Invoke();
        }

        // ReSharper disable once UnusedParameter.Local
        private void OnPauseChanged(bool oldPause, bool newPause)
        {
            _isPaused = newPause;
            OnSpeedChangedEvent?.Invoke();
        }

        #endregion

        #region PlayerAvatarRequests

        /// <summary>
        /// устанавливает текущий масштаб производства
        /// </summary>
        /// <param name="economyEntityID"></param>
        /// <param name="scale"></param>
        [ClientRpc]
        private void RpcSetProductionScale(int economyEntityID, int scale)
        {
            print(_rpcNum++);
            EcomomyEntityManager.Instance.SetProductionScale(economyEntityID, scale);
            var economyEntity = EcomomyEntityManager.Instance.GetEconomyEntity(economyEntityID);
            RegionsUI.Instance.RefreshUIBlockForRegion(economyEntity.RegionID);
            VacuumnajaLogger.Instance.LogEvent(_rpcNum, "SetProductionScale", new Dictionary<string, string>
            {
                {"EconomyEntityID", economyEntityID.ToString()},
                {"Scale", scale.ToString()}
            });
        }

        /// <summary>
        ///  улучшает производство на 1 уровень, списывает необходимые товары со склада игрока
        /// </summary>
        /// <param name="economyEntityID"></param>
        [ClientRpc]
        private void RpcUpgradeProduction(int economyEntityID)
        {
            print(_rpcNum++);
            EcomomyEntityManager.Instance.UpgradeProduction(economyEntityID);
            var economyEntity = EcomomyEntityManager.Instance.GetEconomyEntity(economyEntityID);
            RegionsUI.Instance.RefreshUIBlockForRegion(economyEntity.RegionID);
            VacuumnajaLogger.Instance.LogEvent(_rpcNum, "UpgradeProduction", new Dictionary<string, string>
            {
                {"EconomyEntityID", economyEntityID.ToString()},
            });
        }

        /// <summary>
        /// добавляет в регион новое производство по рецепту, списывает необходимые товары со склада игрока
        /// </summary>
        /// <param name="regionID"></param>
        /// <param name="recipeID"></param>
        [ClientRpc]
        private void RpcAddEconomyEntity(int regionID, int recipeID)
        {
            print(_rpcNum++);
            RegionsManager.Instance.Regions[regionID].AddEconomyEntity(recipeID);
            RegionsUI.Instance.RefreshUIBlockForRegion(regionID);
            VacuumnajaLogger.Instance.LogEvent(_rpcNum, "AddProduction", new Dictionary<string, string>()
            {
                {"EconomyEntityID", regionID.ToString()},
                {"RecipeID", recipeID.ToString()}
            });
        }

        /// <summary>
        ///  запрашивает у сервера изменение текущего масштаба производства
        /// </summary>
        /// <param name="economyEntityID"></param>
        /// <param name="scale"></param>
        [Command(requiresAuthority = false)]
        public void CmdSetProductionScale(int economyEntityID, int scale)
        {
            RpcSetProductionScale(economyEntityID, scale);
        }

        /// <summary>
        ///  запрашивает у сервера улучшение производства на 1 уровень
        /// </summary>
        /// <param name="economyEntityID"></param>
        [Command(requiresAuthority = false)]
        public void CmdUpgradeProduction(int economyEntityID)
        {
            RpcUpgradeProduction(economyEntityID);
        }

        /// <summary>
        ///  запрашивает у сервера добавление в регион нового производства по рецепту
        /// </summary>
        /// <param name="regionID"></param>
        /// <param name="recipeID"></param>
        [Command(requiresAuthority = false)]
        public void CmdAddEconomyEntity(int regionID, int recipeID)
        {
            RpcAddEconomyEntity(regionID, recipeID);
        }

        #endregion
    }
}