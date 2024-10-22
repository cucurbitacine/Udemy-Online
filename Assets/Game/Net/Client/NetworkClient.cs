using System;
using Unity.Netcode;
using UnityEngine.SceneManagement;

namespace Game.Net.Client
{
    public class NetworkClient : IDisposable
    {
        private readonly NetworkManager networkManager;
        
        public NetworkClient(NetworkManager manager)
        {
            networkManager = manager;

            networkManager.OnClientDisconnectCallback += OnClientDisconnect;
        }

        private void OnClientDisconnect(ulong clientId)
        {
            if (clientId != 0 && clientId != networkManager.LocalClientId) return;

            Disconnect();
        }

        public void Disconnect()
        {
            if (SceneManager.GetActiveScene().name != GameManager.MainMenuSceneName)
            {
                SceneManager.LoadScene(GameManager.MainMenuSceneName);
            }

            if (networkManager.IsConnectedClient)
            {
                networkManager.Shutdown();
            }
        }
        
        public void Dispose()
        {
            if (networkManager)
            {
                networkManager.OnClientDisconnectCallback -= OnClientDisconnect;
            }
        }
    }
}