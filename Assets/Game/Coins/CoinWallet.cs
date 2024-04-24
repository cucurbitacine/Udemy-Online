using Unity.Netcode;
using UnityEngine;

namespace Game.Coins
{
    public class CoinWallet : NetworkBehaviour
    {
        public NetworkVariable<int> TotalCoins = new NetworkVariable<int>();

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