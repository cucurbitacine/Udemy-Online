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
        public UnityTransport transport { get; private set; }
        public JoinAllocation allocation { get; private set; }
        public NetworkClient client { get; private set; }
        
        public async Task<bool> InitializeAsync()
        {
            // Authenticate Player

            Log("Initializing Unity Services...");
            await UnityServices.InitializeAsync();
            Log("Unity Services was initialized!");

            client = new NetworkClient(networkManager);
            
            Log("Authenticating...");
            var authState = await AuthenticationWrapper.Authenticate();

            if (authState == AuthState.Authenticated)
            {
                // Good
                Log("Authenticated!");
                return true;
            }

            // Bad
            LogError("Authentication was failed!");
            return false;
        }
        
        public async Task StartClientAsync(string joinCode)
        {
            // Joining to Allocation via JoinCode
            try
            {
                allocation = await relay.JoinAllocationAsync(joinCode);
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

            // Creating User Data
            var userData = new UserData();
            userData.userName = PlayerPrefs.GetString(NetworkServer.PlayerNameKey, string.Empty);
            userData.userAuthID = AuthenticationService.Instance.PlayerId; 
                    
            // Generating Payloads
            var json = JsonUtility.ToJson(userData);
            var payload = NetworkServer.Encoding.GetBytes(json);
            networkManager.NetworkConfig.ConnectionData = payload;
                
            // Start Client
            networkManager.StartClient();
        }
        
        public void LoadMenu()
        {
            LoadScene(NetworkClient.MainMenuSceneName);
        }
        
        private void LoadScene(string sceneName)
        {
            Log($"Loading \"{sceneName}\" scene...");
            
            SceneManager.LoadScene(sceneName);
        }

        public override void Dispose()
        {
            client?.Dispose();
        }
    }
}