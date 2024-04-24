using Unity.Netcode;
using UnityEngine;

namespace Game.Coins
{
    public abstract class Coin : NetworkBehaviour
    {
        [field: SerializeField] public bool AlreadyCollected { get; protected set; } = false;
        [field: SerializeField] public int CoinValue { get; set; } = 10;
        
        [Header("References")]
        [SerializeField] private SpriteRenderer sprite;

        public abstract int Collect();

        protected void Show(bool show)
        {
            if (sprite)
            {
                sprite.enabled = show;
            }
        }
    }
}