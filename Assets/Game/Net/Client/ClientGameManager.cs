using System;
using System.Threading.Tasks;
using Game.Net.Server;
using Game.Net.Shared;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game.Net.Client
{
    public class ClientGameManager : GameManager
    {
        private NetworkClient Client { get; set; }
        private MatchplayMatchmaker Matchmaker { get; set; }

        private UnityTransport transport;
        private JoinAllocation allocation;
        
        private UserData userData;
        
        public async Task<bool> InitializeAsync()
        {
            // Authenticate Player

            Log("Initializing Unity Services...");
            await UnityServices.InitializeAsync();
            Log("Unity Services was initialized!");

            Client = new NetworkClient(networkManager);
            Matchmaker = new MatchplayMatchmaker();
            
            Log("Authenticating...");
            var authState = await AuthenticationWrapper.Authenticate();

            if (authState == AuthState.Authenticated)
            {
                // Creating User Data
                userData = new UserData
                {
                    userName = PlayerPrefs.GetString(NetworkServer.PlayerNameKey, string.Empty),
                    userAuthID = AuthenticationService.Instance.PlayerId,
                };
                
                // Good
                Log("Authenticated!");
                return true;
            }

            // Bad
            LogError("Authentication was failed!");
            return false;
        }

        public void StartClient(string ip, int port)
        {
            // Getting Transport
            if (!networkManager.TryGetComponent<UnityTransport>(out transport))
            {
                Debug.LogError($"\"{nameof(UnityTransport)}\" was NOT found!");
                return;
            }

            transport.SetConnectionData(ip, (ushort)port);
            
            ConnectClient();
        }
        
        public async Task StartClientAsync(string joinCode)
        {
            // Joining to Allocation via JoinCode
            try
            {
                Debug.Log($"Try to Join allocation with Join Code: {joinCode}");
                
                allocation = await relay.JoinAllocationAsync(joinCode);
                
                Debug.Log($"Joining allocation finished");
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                return;
            }

            // Getting Transport
            if (!networkManager.TryGetComponent<UnityTransport>(out transport))
            {
                Debug.LogError($"\"{nameof(UnityTransport)}\" was NOT found!");
                return;
            }
            
            // Switching to Relay
            var relayServerData = new RelayServerData(allocation, "dtls"); // or udp
            transport.SetRelayServerData(relayServerData);

            ConnectClient();
        }

        private void ConnectClient()
        {
            // Generating Payloads
            var json = JsonUtility.ToJson(userData);
            var payload = NetworkServer.Encoding.GetBytes(json);
            networkManager.NetworkConfig.ConnectionData = payload;
            
            // Start Client
            var status = networkManager.StartClient();

            if (status)
            {
                Debug.Log($"Client was Started");
            }
            else
            {
                Debug.LogWarning($"Client wasn't Started");
            }
        }

        public async void MatchmakeAsync(Action<MatchmakerPollingResult> response)
        {
            if (Matchmaker.IsMatchmaking)
            {
                return;
            }

            var result = await GetMatchAsync();
            
            response?.Invoke(result);
        }
        
        private async Task<MatchmakerPollingResult> GetMatchAsync()
        {
            var match = await Matchmaker.Matchmake(userData);

            if (match.result == MatchmakerPollingResult.Success)
            {
                StartClient(match.ip, match.port);
            }

            return match.result;
        }
        
        public async Task CancelMatchmaking()
        {
            await Matchmaker.CancelMatchmaking();
        }
        
        public void LoadMainMenu()
        {
            LoadScene(MainMenuSceneName);
        }

        public void Disconnect()
        {
            Client.Disconnect();
        }
        
        private void LoadScene(string sceneName)
        {
            Log($"Loading \"{sceneName}\" scene...");
            
            SceneManager.LoadScene(sceneName);
        }

        public override void Dispose()
        {
            Client?.Dispose();
        }
    }
}