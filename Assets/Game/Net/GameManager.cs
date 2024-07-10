using System;
using Unity.Netcode;
using Unity.Services.Lobbies;
using Unity.Services.Relay;
using UnityEngine;

namespace Game.Net
{
    public abstract class GameManager : IDisposable
    {
        public static IRelayServiceSDK relay => Relay.Instance;
        public static ILobbyServiceSDK lobbies => Lobbies.Instance;
        public static NetworkManager networkManager => NetworkManager.Singleton;

        public const string JoinCodeKey = "JoinCode";
     
        public const string MainMenuSceneName = "Menu";
        public const string GameSceneName = "Game";
        
        protected void Log(string msg)
        {
            Debug.Log($"[{GetType().Name}] {msg}");
        }

        protected void LogWarning(string msg)
        {
            Debug.LogWarning($"[{GetType().Name}] {msg}");
        }

        protected void LogError(string msg)
        {
            Debug.LogError($"[{GetType().Name}] {msg}");
        }

        public abstract void Dispose();
    }
}