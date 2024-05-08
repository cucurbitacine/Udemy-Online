using Game.Combat;
using Unity.Netcode;
using UnityEngine;

namespace Game.Coins
{
    public class CoinWallet : NetworkBehaviour
    {
        public NetworkVariable<int> TotalCoins = new NetworkVariable<int>();

        [Header("Settings")]
        [SerializeField] private float coinSpread = 3f;
        [SerializeField] [Range(0, 100)] private float bountyPercentage = 50f;
        [SerializeField] private LayerMask layerMask = 1;
        [SerializeField] private int bountyCoinCount = 10;
        [SerializeField] private int minBountyCoinValue = 10;
        
        [Header("References")]
        [SerializeField] private Health health;
        [SerializeField] private BountyCoin bountyCoinPrefab;
        
        private float _radius = 0.5f;
        private readonly Collider2D[] _overlap = new Collider2D[1]; 
        
        public bool Contains(int value)
        {
            return TotalCoins.Value >= value;
        }
        
        public void Put(int value)
        {
            if (IsServer)
            {
                if (value > 0)
                {
                    TotalCoins.Value += value;
                }
            }
            else
            {
                PutServerRpc(value);
            }
        }

        public void Pick(int value)
        {
            if (IsServer)
            {
                if (value > 0)
                {
                    TotalCoins.Value -= value;
                }
            }
            else
            {
                PickServerRpc(value);
            }
        }

        private void HandleDie(Health health)
        {
            var value = (int)(TotalCoins.Value * (bountyPercentage / 100f));
            var coinValue = value / bountyCoinCount;

            coinValue = Mathf.Max(minBountyCoinValue, coinValue);

            for (var i = 0; i < bountyCoinCount; i++)
            {
                var coin = Instantiate(bountyCoinPrefab, GetSpawnPoint(), Quaternion.identity);
                coin.CoinValue = coinValue;
                coin.NetworkObject.Spawn();
            }
        }
        
        private Vector2 GetSpawnPoint()
        {
            while (true)
            {
                var point = (Vector2)transform.position + Random.insideUnitCircle * coinSpread;
                
                if (Physics2D.OverlapCircleNonAlloc(point, _radius, _overlap, layerMask) == 0)
                {
                    return point;
                }
            }
        }
        
        [ServerRpc]
        private void PutServerRpc(int value)
        {
            Put(value);
        }
        
        [ServerRpc]
        private void PickServerRpc(int value)
        {
            Pick(value);
        }

        public override void OnNetworkSpawn()
        {
            if (!IsServer) return;

            _radius = bountyCoinPrefab.GetComponent<CircleCollider2D>().radius;
            
            health.OnDie += HandleDie;
        }

        public override void OnNetworkDespawn()
        {
            health.OnDie -= HandleDie;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.TryGetComponent<Coin>(out var coin))
            {
                var value = coin.Collect();

                if (IsServer)
                {
                    Put(value);
                }
            }
        }
    }
}