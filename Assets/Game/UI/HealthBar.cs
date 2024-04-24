using Game.Combat;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
    public class HealthBar : NetworkBehaviour
    {
        [Header("References")]
        [SerializeField] private Health health;

        [Space]
        [SerializeField] private Image bar;

        private void HandleHealth(int prev, int curr)
        {
            if (bar && health)
            {
                bar.fillAmount = (float)curr / health.MaxHealth;
            }
        }
        
        public override void OnNetworkSpawn()
        {
            if (IsClient && health)
            {
                health.CurrentHealth.OnValueChanged += HandleHealth;

                HandleHealth(0, health.CurrentHealth.Value);
            }
        }

        public override void OnNetworkDespawn()
        {
            if (IsClient && health)
            {
                health.CurrentHealth.OnValueChanged -= HandleHealth;
            }
        }
    }
}