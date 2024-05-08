using TMPro;
using Unity.Services.Lobbies.Models;
using UnityEngine;

namespace Game.UI.LobbyDisplay
{
    public class LobbyItem : MonoBehaviour
    {
        [SerializeField] private TMP_Text lobbyName;
        [SerializeField] private TMP_Text lobbyPlayers;

        public LobbiesList lobbiesList { get; private set; }
        public Lobby lobby { get; private set; }
        
        public void Initialize(LobbiesList list, Lobby lobby)
        {
            this.lobbiesList = list;
            this.lobby = lobby;
            
            lobbyName.text = lobby.Name;

            lobbyPlayers.text = $"{lobby.Players.Count} / {lobby.MaxPlayers}";
        }

        public async void Join()
        {
            await lobbiesList.JoinAsync(lobby);
        }
    }
}