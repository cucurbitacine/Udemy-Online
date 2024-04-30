using System;
using System.Collections.Generic;
using System.Text;
using Game.Net.Shared;
using Unity.Netcode;
using UnityEngine;
using ConnectionApprovalRequest = Unity.Netcode.NetworkManager.ConnectionApprovalRequest;
using ConnectionApprovalResponse = Unity.Netcode.NetworkManager.ConnectionApprovalResponse;

namespace Game.Net.Server
{
    public class NetworkServer : IDisposable
    {
        private readonly NetworkManager networkManager;

        public static Encoding Encoding => Encoding.UTF8;
        public const string PlayerNameKey = nameof(PlayerNameKey);

        public NetworkServer(NetworkManager manager)
        {
            networkManager = manager;

            networkManager.ConnectionApprovalCallback += ApprovalCheck;
            networkManager.OnServerStarted += OnNetworkReady;
        }

        private void OnNetworkReady()
        {
            networkManager.OnClientDisconnectCallback += OnClientDisconnect;
        }

        private void OnClientDisconnect(ulong clientId)
        {
            if (clientIdToAuthId.Remove(clientId, out var authId))
            {
                authIdToUserData.Remove(authId);
            }
        }

        private readonly Dictionary<ulong, string> clientIdToAuthId = new Dictionary<ulong, string>();
        private readonly Dictionary<string, UserData> authIdToUserData = new Dictionary<string, UserData>();

        private void ApprovalCheck(ConnectionApprovalRequest request, ConnectionApprovalResponse response)
        {
            var json = Encoding.GetString(request.Payload);
            var userData = JsonUtility.FromJson<UserData>(json);

            clientIdToAuthId[request.ClientNetworkId] = userData.userAuthID;
            authIdToUserData[userData.userAuthID] = userData;

            response.Approved = true;
            response.CreatePlayerObject = true;
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