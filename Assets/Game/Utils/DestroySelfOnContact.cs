using System;
using Game.Combat;
using Game.Player;
using UnityEngine;

namespace Game.Utils
{
    public class DestroySelfOnContact : MonoBehaviour
    {
        [SerializeField] private Projectile _projectile;
        
        public event Action<Collider2D> OnTriggerEnter; 
        
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (_projectile && _projectile.TeamIndex >= 0)
            {
                if (other.attachedRigidbody && other.attachedRigidbody.TryGetComponent<TankPlayer>(out var player))
                {
                    if (player.TeamIndex.Value == _projectile.TeamIndex)
                    {
                        return;
                    }
                }
            }
            
            OnTriggerEnter?.Invoke(other);
            
            Destroy(gameObject);
        }
    }
}