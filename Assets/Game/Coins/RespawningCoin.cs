using System;
using UnityEngine;

namespace Game.Coins
{
    public class RespawningCoin : Coin
    {
        public event Action<RespawningCoin> OnCollected;
        
        private Vector3 lastPosition;
        
        public override int Collect()
        {
            if (IsServer)
            {
                if (AlreadyCollected) return 0;
                
                AlreadyCollected = true;
                
                OnCollected?.Invoke(this);
                
                return CoinValue;
            }
            
            Show(false);
            
            return 0;
        }

        public void Reset()
        {
            AlreadyCollected = false;
        }
        
        private void Update()
        {
            if (lastPosition != transform.position)
            {
                Show(true);
            }

            lastPosition = transform.position;
        }
    }
}