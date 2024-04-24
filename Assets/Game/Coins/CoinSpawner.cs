using System;
using Unity.Netcode;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Game.Coins
{
    public class CoinSpawner : NetworkBehaviour
    {
        [Header("Settings")]
        [SerializeField] private int maxCoins = 50;
        [SerializeField] private int coinValue = 10;
        [SerializeField] private Vector2 xSpawnRange = new Vector2(-1, 1);
        [SerializeField] private Vector2 ySpawnRange = new Vector2(-1, 1);
        [SerializeField] private LayerMask layerMask = 1;
        
        [Header("Prefab")]
        [SerializeField] private RespawningCoin coinPrefab;

        private float _radius = 0.5f;

        private readonly Collider2D[] _overlap = new Collider2D[1]; 
        
        private Vector2 GetSpawnPoint()
        {
            var point = Vector2.zero; 
            
            while (true)
            {
                point.x = Random.Range(xSpawnRange.x, xSpawnRange.y);
                point.y = Random.Range(ySpawnRange.x, ySpawnRange.y);
                
                if (Physics2D.OverlapCircleNonAlloc(point, _radius, _overlap, layerMask) == 0)
                {
                    return point;
                }
            }
        }

        private void HandleCoinCollected(RespawningCoin coin)
        {
            coin.transform.position = GetSpawnPoint();
            coin.Reset();
        }
        
        private void SpawnCoin()
        {
            var coin = Instantiate(coinPrefab, GetSpawnPoint(), Quaternion.identity);

            coin.CoinValue = coinValue;
            coin.GetComponent<NetworkObject>().Spawn();

            coin.OnCollected += HandleCoinCollected;
        }

        public override void OnNetworkSpawn()
        {
            if (!IsServer) return;

            if (coinPrefab.TryGetComponent<CircleCollider2D>(out var circle))
            {
                _radius = circle.radius;
            }

            for (var i = 0; i < maxCoins; i++)
            {
                SpawnCoin();
            }
        }

        private void OnDrawGizmosSelected()
        {
            var center = Vector2.zero;
            center.x = (xSpawnRange.x + xSpawnRange.y) * 0.5f;
            center.y = (ySpawnRange.x + ySpawnRange.y) * 0.5f;
            
            var size = Vector2.zero;
            size.x = xSpawnRange.y - xSpawnRange.x;
            size.y = ySpawnRange.y - ySpawnRange.x;
            
            Gizmos.DrawWireCube(center, size);
        }
    }
}