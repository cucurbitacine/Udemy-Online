using System;
using System.Collections.Generic;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;

namespace Game.UI.LobbyDisplay
{
    public class LobbiesList : MonoBehaviour
    {
        [SerializeField] private MainMenu mainMenu;
        [SerializeField] private LobbyItem lobbyItemPrefab;
        [SerializeField] private Transform lobbyItemParent;

        [field: SerializeField] public bool isJoining { get; private set; }
        [field: SerializeField] public bool isRefreshing { get; private set; }

        public void JoinAsync(Lobby lobby)
        {
            if (mainMenu)
            {
                mainMenu.StartLobbyAsync(lobby);
            }
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