using System.Threading.Tasks;
using _Scripts.CodeBase.Gameplay.Scenes.BattleChoose.UI;
using _Scripts.CodeBase.Infrastructure.SceneLoading;
using _Scripts.CodeBase.Infrastructure.StateMachine.States.Common;
using _Scripts.CodeBase.StaticData;
using _Scripts.CodeBase.StaticData.PlayerProgressData;

#if UNITY_EDITOR
using _Scripts.CodeBase.Technical.EditorUnilities;
#endif

namespace _Scripts.CodeBase.Infrastructure.StateMachine.States
{
    public class BootstrapState : ExitableStateBase, IState
    {
        private const SceneType Bootstrapper = SceneType.Bootstrapper;
        private readonly GameStateMachine _stateMachine;
        private readonly ISceneLoader _sceneLoader;
        private readonly IStaticDataService _staticData;
        private readonly PlayerProgressData _playerProgressData;

        public BootstrapState(GameStateMachine stateMachine, ISceneLoader sceneLoader, IStaticDataService staticData,
            PlayerProgressData playerProgressData) :
            base(stateMachine)
        {
            _stateMachine = stateMachine;
            _sceneLoader = sceneLoader;
            _staticData = staticData;
            _playerProgressData = playerProgressData;
            LoadData();
        }

        public void Enter()
        {
            _sceneLoader.Load(Bootstrapper, onLoaded: EnterDisclaimerState);
        }

        private void EnterDisclaimerState()
        {
#if UNITY_EDITOR
            if (SkipStatesHelper.SkipUIMenusStates)
            {
                var debugBattlePreset = new ChooseBattlePanel.BattlePreset
                {
                    _battleType = ChooseBattlePanel.BattlePreset.BattleType.OneVsAll
                };
                _stateMachine.Enter<LoadLevelState,
                    (SceneType, ChooseBattlePanel.BattlePreset)>((SceneType.Arena4, debugBattlePreset));
                return;
            }
#endif
            _stateMachine.Enter<DisclaimerState>();
        }

        private async Task LoadData()
        {
            await _staticData.Load();
            _playerProgressData.LoadAll();
        }
    }
}