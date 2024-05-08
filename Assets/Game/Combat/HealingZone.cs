using System.Collections.Generic;
using Game.Player;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Combat
{
    public class HealingZone : NetworkBehaviour
    {
        public NetworkVariable<int> totalHeal = new NetworkVariable<int>(0);
        
        [Header("Settings")]
        [SerializeField] private int maxHealPower = 10;
        [Space]
        [SerializeField] private int healPerTick = 2;
        [SerializeField] private float healTickRate = 1f;
        [SerializeField] private float healCooldown = 30f;
        [Space]
        [SerializeField] private int coinsPerTick = 10;
        
        [Header("References")]
        [SerializeField] private Image healPowerBar;

        private readonly Dictionary<Collider2D, TankPlayer> tanks = new Dictionary<Collider2D, TankPlayer>();
        
        private float timeTick = 0;
        private float timeCooldown = 0;
        
        private float healTickPeriod => 1f  / healTickRate;
        
        private void Heal(TankPlayer player)
        {
            if (totalHeal.Value >= maxHealPower) return;

            if (player.Health.isDead) return;

            if (player.Health.CurrentHealth.Value == 0) return;
            if (player.Health.CurrentHealth.Value == player.Health.MaxHealth) return;
            
            if (!player.Wallet.Contains(coinsPerTick)) return;
            
            player.Wallet.Pick(coinsPerTick);
            player.Health.Restore(healPerTick);
            
            totalHeal.Value++;
        }
        
        private void HealAll()
        {
            foreach (var player in tanks)
            {
                Heal(player.Value);
            }
        }

        private void Tick()
        {
            if (timeTick <= 0f)
            {
                timeTick = healTickPeriod;
                
                HealAll();

                if (totalHeal.Value >= maxHealPower)
                {
                    timeCooldown = 0f;
                }
            }
            else
            {
                timeTick -= Time.deltaTime;
            }
        }
        
        private void Cooldown()
        {
            if (timeCooldown < healCooldown)
            {
                timeCooldown += Time.deltaTime;
            }
            else
            {
                timeCooldown = 0f;

                totalHeal.Value = 0;
            }
        }

        private void UpdateBar(float value)
        {
            if (healPowerBar)
            {
                healPowerBar.fillAmount = Mathf.Clamp01(value);
            }
        }

        private void HandleHealChanging(int prev, int curr)
        {
            var value = (float)(maxHealPower - totalHeal.Value) / maxHealPower;
            
            UpdateBar(value);
        }
        
        public override void OnNetworkSpawn()
        {
            UpdateBar(1f);
            
            totalHeal.OnValueChanged += HandleHealChanging;
        }

        public override void OnNetworkDespawn()
        {
            totalHeal.OnValueChanged -= HandleHealChanging;
        }

        private void Update()
        {
            if (IsServer)
            {
                if (totalHeal.Value < maxHealPower)
                {
                    Tick();
                }
                else
                {
                    Cooldown();
                }
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!IsServer) return;
            
            var root = other.attachedRigidbody ? other.attachedRigidbody.gameObject : other.gameObject;
            
            if (root.TryGetComponent<TankPlayer>(out var player))
            {
                tanks[other] = player;
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (!IsServer) return;

            tanks.Remove(other);
        }
    }
}