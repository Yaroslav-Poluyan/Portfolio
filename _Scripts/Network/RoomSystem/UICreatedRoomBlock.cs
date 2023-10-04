using TMPro;
using UnityEngine;

namespace _Scripts.Network.RoomSystem
{
    public class UICreatedRoomBlock : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _roomNameText;
        [SerializeField] private TextMeshProUGUI _roomPlayersText;
        [SerializeField] private TextMeshProUGUI _roomStatusText;

        public void OnButtonPressed()
        {
            Debug.Log("Room button pressed");
        }

        /*public void Initialize(Match match)
        {
            _roomNameText.text = match._matchID;
            _roomPlayersText.text = match._players.Count + "/" + match._maxPlayers;
        }*/
    }
}