using Game.Player;
using Game.Utils;
using Unity.Netcode;
using UnityEngine;

namespace Game.Combat
{
    public class RespawnHandler : NetworkBehaviour
    {
        [SerializeField] private TankPlayer playerPrefab;
        [SerializeField] [Range(0, 100)] private float lostCoinPercentage = 50f;
/*
        private IEnumerator RespawnPlayer(ulong clientId, int coins)
        {
            yield return null;

            var player = Instantiate(playerPrefab, SpawnPoint.GetRandomSpawnPoint(), Quaternion.identity);
            
            player.NetworkObject.SpawnAsPlayerObject(clientId);
            
            player.Wallet.TotalCoins.Value += coins;
        }
*/
        private void HandlePlayerDie(TankPlayer player)
        {
            var totalCoins = player.Wallet.TotalCoins.Value;
            var lostCoins = (int)(totalCoins * (lostCoinPercentage / 100f));
            var keptCoins = totalCoins - lostCoins;

            player.Health.CurrentHealth.Value = player.Health.MaxHealth;
            player.Wallet.TotalCoins.Value = keptCoins;
            player.TeleportRpc(SpawnPoint.GetRandomSpawnPoint(), Quaternion.identity, Vector3.one);
        }
        
        private void HandlePlayerSpawned(TankPlayer player)
        {
            player.OnDie += HandlePlayerDie;
            
            player.TeleportRpc(SpawnPoint.GetRandomSpawnPoint(), Quaternion.identity, Vector3.one);
        }
        
        private void HandlePlayerDespawned(TankPlayer player)
        {
            player.OnDie -= HandlePlayerDie;
        }

        private void HandlePlayers()
        {
            var players = FindObjectsByType<TankPlayer>(FindObjectsSortMode.None);
            foreach (var player in players)
            {
                HandlePlayerSpawned(player);
            }
        }
        
        public override void OnNetworkSpawn()
        {
            if (!IsServer) return;

            HandlePlayers();

            TankPlayer.OnPlayerSpawned += HandlePlayerSpawned;
            TankPlayer.OnPlayerDespawned += HandlePlayerDespawned;
        }

        public override void OnNetworkDespawn()
        {
            if (!IsServer) return;

            TankPlayer.OnPlayerSpawned -= HandlePlayerSpawned;
            TankPlayer.OnPlayerDespawned -= HandlePlayerDespawned;
        }
    }
}