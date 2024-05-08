using System;
using Unity.Netcode;
using UnityEngine;

namespace Game.Combat
{
    public class Health : NetworkBehaviour
    {
        public NetworkVariable<int> CurrentHealth = new NetworkVariable<int>();
        [field: SerializeField] public int MaxHealth { get; private set; } = 100;
        [field: SerializeField] public bool isDead { get; private set; }

        public event Action<Health> OnDie;
        
        public void Damage(int amount)
        {
            ModifyHealth(-amount);
        }
        
        public void Restore(int amount)
        {
            ModifyHealth(amount);
        }

        private void UpdateHealth(int value)
        {
            if (isDead) return;

            CurrentHealth.Value = value;
            
            if (isDead)
            {
                OnDie?.Invoke(this);
            }
        }
        
        private void ModifyHealth(int delta)
        {
            var value = Mathf.Clamp(CurrentHealth.Value + delta, 0, MaxHealth);

            UpdateHealth(value);
        }
        
        private void OnValueChanged(int previousValue, int newValue)
        {
            isDead = newValue <= 0;
        }
        
        public override void OnNetworkSpawn()
        {
            if (!IsServer) return;

            UpdateHealth(MaxHealth);
        }

        private void OnEnable()
        {
            CurrentHealth.OnValueChanged += OnValueChanged;
        }

        private void OnDisable()
        {
            CurrentHealth.OnValueChanged -= OnValueChanged;
        }
    }
}