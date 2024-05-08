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
        public NetworkClient Client { get; private set; }
        
        private UnityTransport transport { get;  set; }
        private JoinAllocation allocation { get;  set; }
        
        public async Task<bool> InitializeAsync()
        {
            // Authenticate Player

            Log("Initializing Unity Services...");
            await UnityServices.InitializeAsync();
            Log("Unity Services was initialized!");

            Client = new NetworkClient(networkManager);
            
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
        
        public void LoadMenu()
        {
            LoadScene(NetworkClient.MainMenuSceneName);
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