using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Game.Net;
using Game.Net.Client;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;

namespace Game.UI
{
    public class LobbiesList : MonoBehaviour
    {
        [SerializeField] private LobbyItem lobbyItemPrefab;
        [SerializeField] private Transform lobbyItemParent;

        [field: SerializeField] public bool isJoining { get; private set; }
        [field: SerializeField] public bool isRefreshing { get; private set; }

        public async Task JoinAsync(Lobby lobby)
        {
            if (isJoining) return;

            isJoining = true;

            try
            {
                var joiningLobby = await Lobbies.Instance.JoinLobbyByIdAsync(lobby.Id);

                if (joiningLobby.Data.TryGetValue(GameManager.JoinCodeKey, out var joinCode))
                {
                    await ClientController.Instance.GameManager.StartClientAsync(joinCode.Value);
                }
            }
            catch (LobbyServiceException lobbyException)
            {
                Debug.LogError(lobbyException);
            }

            isJoining = false;
        }

        public async void RefreshList()
        {
            if (isRefreshing) return;

            isRefreshing = true;

            try
            {
                var options = new QueryLobbiesOptions();
                options.Count = 25;
                options.Filters = new List<QueryFilter>()
                {
                    new QueryFilter(QueryFilter.FieldOptions.AvailableSlots, "0", QueryFilter.OpOptions.GT),
                    new QueryFilter(QueryFilter.FieldOptions.IsLocked, "0", QueryFilter.OpOptions.EQ),
                };

                var lobbies = await Lobbies.Instance.QueryLobbiesAsync(options);

                foreach (Transform child in lobbyItemParent)
                {
                    Destroy(child.gameObject);
                }

                foreach (var lobby in lobbies.Results)
                {
                    var item = Instantiate(lobbyItemPrefab, lobbyItemParent);

                    item.Initialize(this, lobby);
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
            
            isRefreshing = false;
        }

        private void OnEnable()
        {
            RefreshList();
        }
    }
}