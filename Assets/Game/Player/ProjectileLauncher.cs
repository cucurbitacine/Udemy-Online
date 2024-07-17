using Game.Coins;
using Game.Combat;
using Inputs;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Game.Player
{
    public class ProjectileLauncher : NetworkBehaviour
    {
        public bool primaryFire = false;
        
        [Header("Settings")]
        public float projectileSpeed = 10f;
        public float fireRate = 1f;
        public float flashDuration = 0.1f;
        public int costToFire = 0;
        
        [Header("Prefab")]
        [SerializeField] private GameObject serverProjectilePrefab;
        [SerializeField] private GameObject clientProjectilePrefab;

        [Header("References")]
        [SerializeField] private TankPlayer player;
        [SerializeField] private Transform spawnPoint;
        [SerializeField] private GameObject flash;
        [SerializeField] private Collider2D playerCollider;
        [SerializeField] private CoinWallet wallet;
        [Space]
        [SerializeField] private InputReader input;

        private float timerFire = 0f;
        private float flashTimer = 0f;

        private bool isPointerOverUI;
        
        public override void OnNetworkSpawn()
        {
            if (IsOwner && input)
            {
                input.PrimaryFireEvent += OnPrimaryFire;
            }
        }

        public override void OnNetworkDespawn()
        {
            if (IsOwner && input)
            {
                input.PrimaryFireEvent -= OnPrimaryFire;
            }
        }

        private void OnPrimaryFire(bool fire)
        {
            if (fire && isPointerOverUI) return;
            
            primaryFire = fire;
        }

        private void IgnoreCollision(GameObject target)
        {
            if (playerCollider && target.TryGetComponent<Collider2D>(out var targetCollider))
            {
                Physics2D.IgnoreCollision(playerCollider, targetCollider);   
            }
        }
        
        private void Launch(GameObject missile)
        {
            if (missile.TryGetComponent<Rigidbody2D>(out var rigid))
            {
                rigid.velocity = rigid.transform.up * projectileSpeed;
            }
        }
        
        private void SpawnProjectileClient(Vector2 point, Vector2 direction, int teamIndex)
        {
            if (flash)
            {
                flash.SetActive(true);
                flashTimer = flashDuration;
            }

            var rotation = Quaternion.LookRotation(Vector3.forward, direction);
            var clientProjectile = Instantiate(clientProjectilePrefab, point, rotation);

            IgnoreCollision(clientProjectile);

            if (clientProjectile.TryGetComponent<Projectile>(out var projectile))
            {
                projectile.Initialize(teamIndex);
            }
            
            Launch(clientProjectile);
        }
        
        [ServerRpc]
        private void SpawnProjectileServerRpc(Vector2 point, Vector2 direction)
        {
            wallet.Pick(costToFire);
            
            var rotation = Quaternion.LookRotation(Vector3.forward, direction);
            var serverProjectile = Instantiate(serverProjectilePrefab, point, rotation);

            IgnoreCollision(serverProjectile);

            if (serverProjectile.TryGetComponent<Projectile>(out var projectile))
            {
                projectile.Initialize(player.TeamIndex.Value);
            }
            
            if (serverProjectile.TryGetComponent<DealDamageOnContact>(out var damage))
            {
                damage.SetOwner(OwnerClientId);
            }
            
            Launch(serverProjectile);
            
            SpawnProjectileClientRpc(point, direction, player.TeamIndex.Value);
        }

        [ClientRpc]
        private void SpawnProjectileClientRpc(Vector2 point, Vector2 direction, int teamIndex)
        {
            if (IsOwner) return;

            SpawnProjectileClient(point, direction, teamIndex);
        }

        private void UpdateFlash()
        {
            if (flash)
            {
                if (flashTimer > 0)
                {
                    flashTimer -= Time.deltaTime;
                    if (flashTimer < 0)
                    {
                        flash.SetActive(false);
                    }
                }
            }
        }

        private void UpdateFire(float deltaTime)
        {
            if (timerFire > 0)
            {
                timerFire -= deltaTime;
            }

            if (timerFire > 0) return;
            
            if (!primaryFire) return;

            if (wallet && !wallet.Contains(costToFire)) return;

            var point = spawnPoint.position;
            var direction = spawnPoint.up;

            SpawnProjectileServerRpc(point, direction);
            SpawnProjectileClient(point, direction, player.TeamIndex.Value);

            timerFire = fireRate > 0 ? 1f / fireRate : 0f;
        }
        
        private void UpdateOwner()
        {
            if (!IsOwner) return;

            isPointerOverUI = EventSystem.current.IsPointerOverGameObject();
            
            UpdateFire(Time.deltaTime);

            if (Input.GetKeyDown(KeyCode.C))
            {
                if (wallet) wallet.Put(10);
            }
        }
        
        private void Update()
        {
            UpdateFlash();

            UpdateOwner();
        }
    }
}