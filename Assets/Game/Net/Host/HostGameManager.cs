using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Game.Net.Server;
using Game.Net.Shared;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

namespace Game.Net.Host
{
    public class HostGameManager : GameManager
    {
        public const int MaxConnections = 20;
        public const float HeartbeatPeriod = 15;
        
        public NetworkServer Server { get; private set; }
        
        private UnityTransport transport { get; set; }
        private Allocation allocation { get; set; }
        private string joinCode { get; set; }
        private string lobbyId { get; set; }
        private Coroutine heartbeating { get; set; }
        
        public async Task StartHostAsync()
        {
            // Creating Allocation
            try
            {
                allocation = await relay.CreateAllocationAsync(MaxConnections);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                return;
            }
            
            // Getting JoinCode
            try
            {
                joinCode = await relay.GetJoinCodeAsync(allocation.AllocationId);
                
                Debug.Log(joinCode);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                return;
            }

            // Getting Transport
            transport = networkManager.GetComponent<UnityTransport>();
            if (transport == null)
            {
                Debug.LogError($"\"{nameof(UnityTransport)}\" was NOT found!");
                return;
            }
            
            // Switching to Relay
            var relayServerData = new RelayServerData(allocation, "dtls"); // or udp
            transport.SetRelayServerData(relayServerData);

            // Creating UserData
            var userData = new UserData();
            userData.userName = PlayerPrefs.GetString(NetworkServer.PlayerNameKey, "Unnamed");
            userData.userAuthID = AuthenticationService.Instance.PlayerId;
                
            // Creating Lobby
            try
            {
                var lobbyOptions = new CreateLobbyOptions();
                lobbyOptions.IsPrivate = false;
                lobbyOptions.Data = new Dictionary<string, DataObject>()
                {
                    {JoinCodeKey, new DataObject(DataObject.VisibilityOptions.Member, joinCode)},
                };

                var lobbyName = $"{userData.userName} #{Random.Range(0, 10000):0000}";
                    
                var lobby = await lobbies.CreateLobbyAsync(lobbyName, MaxConnections, lobbyOptions);
                    
                lobbyId = lobby.Id;

                // Start Heartbeat
                heartbeating = HostController.Instance.StartCoroutine(HeartbeatLobby(HeartbeatPeriod));
            }
            catch (LobbyServiceException lobbyException)
            {
                Debug.LogError(lobbyException);
                return;
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                return;
            }

            Server = new NetworkServer(networkManager);
                
            // Generating Payloads
            var json = JsonUtility.ToJson(userData);
            var payload = NetworkServer.Encoding.GetBytes(json);
            networkManager.NetworkConfig.ConnectionData = payload;
                
            // Start Host
            networkManager.StartHost();

            Server.OnClientLeft += HandleClientLeft;
            
            // Load Game Scene
            networkManager.SceneManager.LoadScene("Game", LoadSceneMode.Single);
        }

        private async void HandleClientLeft(string authId)
        {
            try
            {
                await lobbies.RemovePlayerAsync(lobbyId, authId);
            }
            catch (LobbyServiceException e)
            {
                Debug.LogError(e);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }

        public async void Shutdown()
        {
            if (heartbeating != null)
            {
                if (HostController.Instance)
                {
                    HostController.Instance.StopCoroutine(heartbeating);
                }
            }

            if (!string.IsNullOrWhiteSpace(lobbyId))
            {
                try
                {
                    await Lobbies.Instance.DeleteLobbyAsync(lobbyId);
                }
                catch (LobbyServiceException e)
                {
                    LogError(e.Message);
                }

                lobbyId = string.Empty;
            }
            
            Server.OnClientLeft -= HandleClientLeft;
            
            Server?.Dispose();
        }
        
        private IEnumerator HeartbeatLobby(float period)
        {
            var delay = new WaitForSecondsRealtime(period);
            
            while (true)
            {
                lobbies.SendHeartbeatPingAsync(lobbyId);
                
                yield return delay;
            }
        }

        public override void Dispose()
        {
            Shutdown();
        }
    }
}