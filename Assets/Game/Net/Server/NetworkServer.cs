using System;
using System.Collections.Generic;
using System.Text;
using Game.Net.Shared;
using Game.Utils;
using Unity.Netcode;
using UnityEngine;
using ConnectionApprovalRequest = Unity.Netcode.NetworkManager.ConnectionApprovalRequest;
using ConnectionApprovalResponse = Unity.Netcode.NetworkManager.ConnectionApprovalResponse;

namespace Game.Net.Server
{
    public class NetworkServer : IDisposable
    {
        public Action<string> OnClientLeft;
        
        private readonly NetworkManager networkManager;

        public static Encoding Encoding => Encoding.UTF8;
        public const string PlayerNameKey = nameof(PlayerNameKey);

        private readonly Dictionary<ulong, string> clientIdToAuthId = new Dictionary<ulong, string>();
        private readonly Dictionary<string, UserData> authIdToUserData = new Dictionary<string, UserData>();
        
        public NetworkServer(NetworkManager manager)
        {
            networkManager = manager;

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
        
        private void OnNetworkReady()
        {
            networkManager.OnClientDisconnectCallback += OnClientDisconnect;
        }

        private void OnClientDisconnect(ulong clientId)
        {
            if (clientIdToAuthId.Remove(clientId, out var authId) && authIdToUserData.Remove(authId))
            {
                OnClientLeft?.Invoke(authId);
            }
        }

        private void ApprovalCheck(ConnectionApprovalRequest request, ConnectionApprovalResponse response)
        {
            var json = Encoding.GetString(request.Payload);
            var userData = JsonUtility.FromJson<UserData>(json);

            clientIdToAuthId[request.ClientNetworkId] = userData.userAuthID;
            authIdToUserData[userData.userAuthID] = userData;

            response.Approved = true;
            response.CreatePlayerObject = true;
            
            response.PlayerPrefabHash = null;
            
            response.Position = SpawnPoint.GetRandomSpawnPoint(); // Does not work. Why? Look below
            response.Rotation = Quaternion.identity;

            response.Reason = "It's okay";
            response.Pending = false;
            
            /*
             * IDK
             */
            
            Debug.Log($"{request.ClientNetworkId} : {response.Position} Response Position");
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