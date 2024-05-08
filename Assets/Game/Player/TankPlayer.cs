using System;
using Cinemachine;
using Game.Coins;
using Game.Combat;
using Game.Net.Host;
using Game.Utils;
using Unity.Collections;
using Unity.Netcode;
using Unity.Netcode.Components;
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

        [Header("Minimap")]
        [SerializeField] private Color colorPlayer = Color.yellow;
        [SerializeField] private SpriteRenderer minimapIcon;
        
        public NetworkVariable<FixedString32Bytes> PlayerName = new NetworkVariable<FixedString32Bytes>();

        public event Action<TankPlayer> OnDie; 
        public event Action<TankPlayer> OnCoinChanged; 
        
        public static event Action<TankPlayer> OnPlayerSpawned; 
        public static event Action<TankPlayer> OnPlayerDespawned;
        
        [Rpc(SendTo.Owner)]
        public void TeleportRpc(Vector3 newPosition, Quaternion newRotation, Vector3 newScale)
        {
            if (TryGetComponent<NetworkTransform>(out var netTransform))
            {
                netTransform.Teleport(newPosition, newRotation, newScale);
            }
        }

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
                var userData = HostController.Instance.GameManager.Server.GetUserData(OwnerClientId);

                PlayerName.Value = userData.userName;
                
                Health.OnDie += HandleDie;
                
                Wallet.TotalCoins.OnValueChanged += HandleCoinChanged;
                
                OnPlayerSpawned?.Invoke(this);
            }
            
            if (IsOwner && virtualCamera)
            {
                virtualCamera.Priority = ownerPriority;

                if (IsClient && minimapIcon)
                {
                    minimapIcon.color = colorPlayer;
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