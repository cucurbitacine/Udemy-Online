using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Game.Net.Shared;
using Game.Utils;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using ConnectionApprovalRequest = Unity.Netcode.NetworkManager.ConnectionApprovalRequest;
using ConnectionApprovalResponse = Unity.Netcode.NetworkManager.ConnectionApprovalResponse;

namespace Game.Net.Server
{
    public class NetworkServer : IDisposable
    {
        public Action<UserData> OnUserJoined;
        public Action<UserData> OnUserLeft;
        public Action<string> OnClientLeft;
        
        private readonly NetworkManager networkManager;
        private readonly NetworkObject playerPrefab;

        public static Encoding Encoding => Encoding.UTF8;
        public const string PlayerNameKey = nameof(PlayerNameKey);

        private readonly Dictionary<ulong, string> clientIdToAuthId = new Dictionary<ulong, string>();
        private readonly Dictionary<string, UserData> authIdToUserData = new Dictionary<string, UserData>();
        
        public NetworkServer(NetworkManager networkManager, NetworkObject playerPrefab)
        {
            this.networkManager = networkManager;
            this.playerPrefab = playerPrefab;

            networkManager.ConnectionApprovalCallback += ApprovalCheck;
            networkManager.OnServerStarted += OnNetworkReady;
        }

        public bool TryGetUserData(ulong clientId, out UserData userData)
        {
            userData = default;

            return clientIdToAuthId.TryGetValue(clientId, out var authId) &&
                   authIdToUserData.TryGetValue(authId, out userData);
        }

        public UserData GetUserData(ulong clientId)
        {
            return TryGetUserData(clientId, out var userData) ? userData : null;
        }

        public bool OpenConnection(string ip, int port)
        {
            if (networkManager.gameObject.TryGetComponent<UnityTransport>(out var transport))
            {
                transport.SetConnectionData(ip, (ushort)port);
                
                return networkManager.StartServer();
            }

            Debug.LogError($"Have not found any \"{nameof(UnityTransport)}\"");
            return false;
        }
        
        private void OnNetworkReady()
        {
            networkManager.OnClientDisconnectCallback += OnClientDisconnect;
        }

        private void OnClientDisconnect(ulong clientId)
        {
            if (clientIdToAuthId.Remove(clientId, out var authId) && authIdToUserData.Remove(authId, out var userData))
            {
                OnUserLeft?.Invoke(userData); // TODO Different invoke then in the course 
                
                OnClientLeft?.Invoke(authId);
            }
        }

        private void ApprovalCheck(ConnectionApprovalRequest request, ConnectionApprovalResponse response)
        {
            var json = Encoding.GetString(request.Payload);
            var userData = JsonUtility.FromJson<UserData>(json);

            clientIdToAuthId[request.ClientNetworkId] = userData.userAuthID;
            authIdToUserData[userData.userAuthID] = userData;

            OnUserJoined?.Invoke(userData);

            _ = SpawnPlayerDelayed(request.ClientNetworkId);
            
            response.Approved = true;
            response.CreatePlayerObject = false;
            
            response.PlayerPrefabHash = null;
            
            //response.Position = SpawnPoint.GetRandomSpawnPoint(); // Does not work. Why? Look below
            //response.Rotation = Quaternion.identity;

            response.Reason = "It's okay";
            response.Pending = false;
            
            /*
             * IDK
             */
            
            Debug.Log($"{request.ClientNetworkId} : {response.Position} Response Position");
        }

        private async Task SpawnPlayerDelayed(ulong clientId)
        {
            await Task.Delay(1000);

            var player = GameObject.Instantiate(playerPrefab, SpawnPoint.GetRandomSpawnPoint(), Quaternion.identity);
            player.SpawnAsPlayerObject(clientId);
        }
        
        public void Dispose()
        {
            if (networkManager)
            {
                networkManager.ConnectionApprovalCallback -= ApprovalCheck;
                networkManager.OnClientDisconnectCallback -= OnClientDisconnect;
                networkManager.OnServerStarted -= OnNetworkReady;

                if (networkManager.IsListening)
                {
                    networkManager.Shutdown();
                }
            }
        }
    }
}