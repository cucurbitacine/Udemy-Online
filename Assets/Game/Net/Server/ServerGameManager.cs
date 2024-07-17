using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Game.Net.Server.Services;
using Game.Net.Shared;
using Unity.Netcode;
using Unity.Services.Matchmaker.Models;
using UnityEngine;

namespace Game.Net.Server
{
    public class ServerGameManager : GameManager
    {
        public NetworkServer Server { get; private set; }
        private MultiplayAllocationService MultiplayAllocationService { get; set; }
        
        private readonly string _ip;
        private readonly int _port;
        private int _queryPort;
        
        private readonly Dictionary<string, int> teamIdToTeamIndex = new Dictionary<string, int>();
        
        private MatchplayBackfiller backfiller;

        public ServerGameManager(string serverIP, int serverPort, int serverQPort, NetworkObject playerPrefab)
        {
            _ip = serverIP;
            _port = serverPort;
            _queryPort = serverQPort;
            
            Server = new NetworkServer(networkManager, playerPrefab);
            MultiplayAllocationService = new MultiplayAllocationService();
        }
        
        public async Task StartGameServerAsync()
        {
            await MultiplayAllocationService.BeginServerCheck();

            try
            {
                var matchmakerPayloads = await GetMatchmakerPayload();

                if (matchmakerPayloads != null)
                {
                    await StartBackfill(matchmakerPayloads);

                    Server.OnUserJoined += UserJoined;
                    Server.OnUserLeft += UserLeft;
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning(e.Message);
            }
            
            if (!Server.OpenConnection(_ip, _port))
            {
                Debug.LogWarning($"NetworkServer did not start as expected.");
                
                return;
            }
        }
        
        private async Task<MatchmakingResults> GetMatchmakerPayload()
        {
            var matchmakerTask = MultiplayAllocationService.SubscribeAndAwaitMatchmakerAllocation();

            if (await Task.WhenAny(matchmakerTask, Task.Delay(20000)) == matchmakerTask)
            {
                return matchmakerTask.Result;
            }

            Debug.LogWarning("Matchmaker payload timed out");
            return null;
        }
        
        private async Task StartBackfill(MatchmakingResults payload)
        {
            backfiller = new MatchplayBackfiller($"{_ip}:{_port}", payload.QueueName, payload.MatchProperties, 20);

            if (backfiller.NeedsPlayers())
            {
                await backfiller.BeginBackfilling();
            }
        }
        
        private void UserJoined(UserData user)
        {
            var team = backfiller.GetTeamByUserId(user.userAuthID);
            Debug.Log($"Team: {user.userAuthID} {team.TeamId}");

            if (!teamIdToTeamIndex.TryGetValue(team.TeamId, out var teamIndex))
            {
                teamIndex = teamIdToTeamIndex.Count;
                teamIdToTeamIndex.Add(team.TeamId, teamIndex);
            }

            user.teamIndex = teamIndex;
            
            //backfiller.AddPlayerToMatch(user);
            MultiplayAllocationService.AddPlayer();
            if (!backfiller.NeedsPlayers() && backfiller.IsBackfilling)
            {
                _ = backfiller.StopBackfill();
            }
        }
        
        private void UserLeft(UserData user)
        {
            var playerCount = backfiller.RemovePlayerFromMatch(user.userAuthID);
            MultiplayAllocationService.RemovePlayer();
            if (playerCount <= 0)
            {
                CloseServer();
                
                return;
            }

            if (backfiller.NeedsPlayers() && !backfiller.IsBackfilling)
            {
                _ = backfiller.BeginBackfilling();
            }
        }

        private async void CloseServer()
        {
            await backfiller.StopBackfill();

            Dispose();

            Application.Quit();
        }
        
        public override void Dispose()
        {
            if (Server != null)
            {
                Server.OnUserJoined -= UserJoined;
                Server.OnUserLeft -= UserLeft;
                
                Server.Dispose();
            }
            
            MultiplayAllocationService?.Dispose();
            
            backfiller?.Dispose();
        }
    }
}