using System;
using Cinemachine;
using Game.Coins;
using Game.Combat;
using Game.Net.Host;
using Game.Net.Server;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

namespace Game.Player
{
    public class TankPlayer : NetworkBehaviour
    {
        [field: Header("References")]
        [field: SerializeField] public Health Health { get; private set; }
        [field: SerializeField] public CoinWallet Wallet { get; private set; }
        
        [Header("Camera")]
        [SerializeField] private int ownerPriority = 15;
        [SerializeField] private CinemachineVirtualCameraBase virtualCamera;
        [SerializeField] private Texture2D crosshair;

        [Header("Minimap")]
        [SerializeField] private Color colorPlayer = Color.yellow;
        [SerializeField] private SpriteRenderer minimapIcon;
        
        public NetworkVariable<FixedString32Bytes> PlayerName = new NetworkVariable<FixedString32Bytes>();
        public NetworkVariable<int> TeamIndex = new NetworkVariable<int>();

        public event Action<TankPlayer> OnDie; 
        public event Action<TankPlayer> OnCoinChanged; 
        
        public static event Action<TankPlayer> OnPlayerSpawned; 
        public static event Action<TankPlayer> OnPlayerDespawned;

        private void HandleDie(Health health)
        {
            OnDie?.Invoke(this);
        }
        
        private void HandleCoinChanged(int prev, int curr)
        {
            OnCoinChanged?.Invoke(this);
        }
        
        public override void OnNetworkSpawn()
        {
            if (IsServer)
            {
                var server = IsHost
                    ? HostController.Instance.GameManager.Server
                    : ServerController.Instance.GameManager.Server;
                
                var userData = server.GetUserData(OwnerClientId);
                
                PlayerName.Value = userData.userName;
                TeamIndex.Value = userData.teamIndex;
                
                Health.OnDie += HandleDie;
                
                Wallet.TotalCoins.OnValueChanged += HandleCoinChanged;
                
                OnPlayerSpawned?.Invoke(this);
            }

            if (IsOwner)
            {
                if (crosshair)
                {
                    Cursor.SetCursor(crosshair, new Vector2(crosshair.width * 0.5f, crosshair.height * 0.5f), CursorMode.Auto);   
                }
                
                if (virtualCamera)
                {
                    virtualCamera.Priority = ownerPriority;

                    if (IsClient && minimapIcon)
                    {
                        minimapIcon.color = colorPlayer;
                    }
                }
            }
        }
        
        public override void OnNetworkDespawn()
        {
            if (IsServer)
            {
                Health.OnDie -= HandleDie;

                Wallet.TotalCoins.OnValueChanged -= HandleCoinChanged;
                
                OnPlayerDespawned?.Invoke(this);
            }
        }
    }
}
