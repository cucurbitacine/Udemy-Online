using System;
using UnityEngine;

namespace Game.Utils
{
    public class DestroySelfOnContact : MonoBehaviour
    {
        public event Action<Collider2D> OnTriggerEnter; 
        
        private void OnTriggerEnter2D(Collider2D other)
        {
            OnTriggerEnter?.Invoke(other);
            
            Destroy(gameObject);
        }
    }
}