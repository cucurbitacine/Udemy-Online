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
        
        public UnityTransport transport { get; private set; }
        public NetworkServer server { get; private set; }
        public Allocation allocation { get; private set; }
        public string joinCode { get; private set; }
        public string lobbyId { get; private set; }
        public Coroutine heartbeating { get; private set; }
        
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

            server = new NetworkServer(networkManager);
                
            // Generating Payloads
            var json = JsonUtility.ToJson(userData);
            var payload = NetworkServer.Encoding.GetBytes(json);
            networkManager.NetworkConfig.ConnectionData = payload;
                
            // Start Host
            networkManager.StartHost();

            // Load Game Scene
            networkManager.SceneManager.LoadScene("Game", LoadSceneMode.Single);
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

        public override async void Dispose()
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
            
            server?.Dispose();
        }
    }
}