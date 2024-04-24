using Game.Utils;
using Unity.Netcode;
using UnityEngine;

namespace Game.Combat
{
    [RequireComponent(typeof(DestroySelfOnContact))]
    public class DealDamageOnContact : MonoBehaviour
    {
        [field: SerializeField] public int DamageAmount { get; private set; } = 10;

        [Header("References")]
        [SerializeField] private DestroySelfOnContact destroyer;

        private ulong _ownerClientId;
        
        public void SetOwner(ulong ownerClientId)
        {
            _ownerClientId = ownerClientId;
        }
        
        private void Damage(Collider2D other)
        {
            if (other.attachedRigidbody)
            {
                if (other.attachedRigidbody.TryGetComponent<NetworkObject>(out var netObject))
                {
                    if (netObject.OwnerClientId == _ownerClientId)
                    {
                        return;
                    }
                }
                
                if (other.attachedRigidbody.TryGetComponent<Health>(out var health))
                {
                    health.Damage(DamageAmount);
                }
            }
        }

        private void Awake()
        {
            if (destroyer == null) destroyer = GetComponent<DestroySelfOnContact>();
        }

        private void OnEnable()
        {
            if (destroyer) destroyer.OnTriggerEnter += Damage;
        }

        private void OnDisable()
        {
            if (destroyer) destroyer.OnTriggerEnter -= Damage;
        }
    }
}