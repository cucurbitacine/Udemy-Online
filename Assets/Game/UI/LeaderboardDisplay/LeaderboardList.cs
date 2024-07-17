using System.Collections.Generic;
using System.Linq;
using Game.Net.Client;
using Game.Net.Shared;
using Game.Player;
using Unity.Netcode;
using UnityEngine;

namespace Game.UI.LeaderboardDisplay
{
    public class LeaderboardList : NetworkBehaviour
    {
        [SerializeField] private LeaderboardEntityDisplay leaderboardEntityPrefab;
        
        [Header("Players")]
        [SerializeField] private Transform holder;
        [SerializeField] private int maxDisplay = 8;
        [SerializeField] private Color ownerColor = Color.yellow;
        
        [Header("Teams")]
        [SerializeField] private Transform teamHolder;
        [SerializeField] private GameObject teamLeaderboardBackground;
        [SerializeField] private string[] teamNames;
        [SerializeField] private TeamColorPalette teamPalette;
        
        private NetworkList<LeaderboardEntity> entities;
        private readonly List<LeaderboardEntityDisplay> displays = new List<LeaderboardEntityDisplay>();
        private readonly List<LeaderboardEntityDisplay> teamDisplays = new List<LeaderboardEntityDisplay>();

        private static LeaderboardEntity CreateEntity(TankPlayer player)
        {
            return new LeaderboardEntity()
            {
                clientId = player.OwnerClientId,
                teamIndex = player.TeamIndex.Value,
                playerName = player.PlayerName.Value,
                score = player.Wallet.TotalCoins.Value,
            };
        }

        private void HandlePlayerSpawned(TankPlayer player)
        {
            var entity = CreateEntity(player);

            if (entities != null)
            {
                entities.Add(entity);
            }

            player.OnCoinChanged += HandlePlayerCoinChanged;
        }

        private void HandlePlayerDespawned(TankPlayer player)
        {
            player.OnCoinChanged -= HandlePlayerCoinChanged;

            if (entities != null)
            {
                foreach (var entity in entities)
                {
                    if (entity.clientId == player.OwnerClientId)
                    {
                        entities.Remove(entity);
                        return;
                    }
                }
            }
        }

        private void HandleListChanged(NetworkListEvent<LeaderboardEntity> changeEvent)
        {
            if (!gameObject.scene.isLoaded) return;
            
            switch (changeEvent.Type)
            {
                case NetworkListEvent<LeaderboardEntity>.EventType.Add:
                {
                    if (displays.All(d => d.ClientId != changeEvent.Value.clientId))
                    {
                        var item = Instantiate(leaderboardEntityPrefab, holder);
                        

                        item.Initilalize(
                            changeEvent.Value.clientId,
                            changeEvent.Value.playerName.Value,
                            changeEvent.Value.score);

                        if (NetworkManager.Singleton.LocalClientId == changeEvent.Value.clientId)
                        {
                            item.SetColor(ownerColor);
                        }
                        
                        displays.Add(item);
                    }

                    break;
                }
                case NetworkListEvent<LeaderboardEntity>.EventType.Remove:
                {
                    var item = displays.FirstOrDefault(d => d.ClientId == changeEvent.Value.clientId);

                    if (item && displays.Remove(item))
                    {
                        item.transform.SetParent(null);
                        Destroy(item.gameObject);
                    }

                    break;
                }
                case NetworkListEvent<LeaderboardEntity>.EventType.Value:
                {
                    var item = displays.FirstOrDefault(d => d.ClientId == changeEvent.Value.clientId);

                    if (item)
                    {
                        item.UpdateScore(changeEvent.Value.score);
                    }

                    break;
                }
            }

            displays.Sort((x, y) => y.Score.CompareTo(x.Score));

            for (var i = 0; i < displays.Count; i++)
            {
                displays[i].transform.SetSiblingIndex(i);
                displays[i].UpdateText(i + 1);

                displays[i].gameObject.SetActive(i < maxDisplay);
            }

            var myDisplay = displays.FirstOrDefault(d => d.ClientId == OwnerClientId);

            if (myDisplay)
            {
                if (myDisplay.transform.GetSiblingIndex() >= maxDisplay)
                {
                    holder.GetChild(maxDisplay - 1).gameObject.SetActive(false);

                    myDisplay.gameObject.SetActive(true);
                }
            }

            if (teamLeaderboardBackground.activeSelf)
            {
                var teamDisplay = teamDisplays.FirstOrDefault(d => d.TeamIndex == changeEvent.Value.teamIndex);

                if (teamDisplay)
                {
                    if (changeEvent.Type == NetworkListEvent<LeaderboardEntity>.EventType.Remove)
                    {
                        teamDisplay.UpdateScore(teamDisplay.Score - changeEvent.Value.score);
                    }
                    else
                    {
                        var deltaScore = changeEvent.Value.score - changeEvent.PreviousValue.score;
                        teamDisplay.UpdateScore(teamDisplay.Score + deltaScore);
                    }
                }
                
                teamDisplays.Sort((x, y) => y.Score.CompareTo(x.Score));

                for (var i = 0; i < teamDisplays.Count; i++)
                {
                    teamDisplays[i].transform.SetSiblingIndex(i);
                    teamDisplays[i].UpdateText(i + 1);
                }
            }
        }

        private void HandlePlayerCoinChanged(TankPlayer player)
        {
            for (var i = 0; i < entities.Count; i++)
            {
                var entity = entities[i];

                if (entity.clientId == player.OwnerClientId)
                {
                    entity.score = player.Wallet.TotalCoins.Value;
                    
                    entities[i] = entity;
                }
            }
        }

        public override void OnNetworkSpawn()
        {
            if (IsClient)
            {
                if (ClientController.Instance.GameManager.UserData.userGamePreferences.gameQueue == GameQueue.Team)
                {
                    if (teamLeaderboardBackground)
                    {
                        teamLeaderboardBackground.SetActive(true);

                        for (var i = 0; i < teamNames.Length; i++)
                        {
                            var team = Instantiate(leaderboardEntityPrefab, teamHolder);
                            
                            team.Initilalize(i, teamNames[i], 0);
                            team.SetColor(teamPalette.GetTeamColor(i));
                            
                            teamDisplays.Add(team);
                        }
                    }
                }
                
                entities.OnListChanged += HandleListChanged;

                foreach (var entity in entities)
                {
                    var changeEvent = new NetworkListEvent<LeaderboardEntity>();
                    changeEvent.Type = NetworkListEvent<LeaderboardEntity>.EventType.Add;
                    changeEvent.Value = entity;

                    HandleListChanged(changeEvent);
                }
            }

            if (IsServer)
            {
                foreach (var player in FindObjectsByType<TankPlayer>(FindObjectsSortMode.None))
                {
                    HandlePlayerSpawned(player);
                }

                TankPlayer.OnPlayerSpawned += HandlePlayerSpawned;
                TankPlayer.OnPlayerDespawned += HandlePlayerDespawned;
            }
        }

        public override void OnNetworkDespawn()
        {
            if (IsClient)
            {
                entities.OnListChanged -= HandleListChanged;
            }

            if (IsServer)
            {
                TankPlayer.OnPlayerSpawned -= HandlePlayerSpawned;
                TankPlayer.OnPlayerDespawned -= HandlePlayerDespawned;
            }
        }

        private void Awake()
        {
            entities = new NetworkList<LeaderboardEntity>();
        }
    }
}