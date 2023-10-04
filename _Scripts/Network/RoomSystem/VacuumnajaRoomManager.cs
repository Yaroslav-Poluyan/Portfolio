using _Scripts.Network.UI.Lobby;
using Mirror;

namespace _Scripts.Network.RoomSystem
{
    public class VacuumnajaRoomManager : NetworkRoomManager
    {
        public override void OnServerConnect(NetworkConnectionToClient conn)
        {
            base.OnServerConnect(conn);
        }

        public override void OnServerDisconnect(NetworkConnectionToClient conn)
        {
            base.OnServerDisconnect(conn);
        }

        public override void OnRoomServerPlayersReady()
        {
            UILobby.Instance.ShowStartGameButtonState(true);
        }
        public override void OnRoomServerPlayersNotReady()
        {
            UILobby.Instance.ShowStartGameButtonState(false);
        }

        public void StartGame()
        {
            ServerChangeScene(GameplayScene);
        }
    }
}