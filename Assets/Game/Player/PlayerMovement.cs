using System;
using Inputs;
using Unity.Netcode;
using UnityEngine;

namespace Game.Player
{
    public class PlayerMovement : NetworkBehaviour
    {
        public Vector2 movement;
        
        [Header("Settings")]
        [SerializeField] private float speed = 4f;
        [SerializeField] private float turningRate = 30f;
        [SerializeField] private float particleEmissionValue = 10;
        
        
        [Header("References")]
        [SerializeField] private Transform body;
        [SerializeField] private Rigidbody2D rigid;
        [SerializeField] private ParticleSystem dustCloud;
        
        [Space]
        [SerializeField] private InputReader input;

        private Vector3 previousPosition;
        private ParticleSystem.EmissionModule emissionModule;
        
        private const float ParticleStopThreshold = 0.005f;
        
        private void HandleMove(Vector2 move)
        {
            movement = move;
        }
        
        public override void OnNetworkSpawn()
        {
            if (IsOwner && input)
            {
                input.MoveEvent += HandleMove;
            }            
        }

        public override void OnNetworkDespawn()
        {
            if (IsOwner && input)
            {
                input.MoveEvent -= HandleMove;
            } 
        }

        private void Awake()
        {
            if (dustCloud)
            {
                emissionModule = dustCloud.emission;
            }
        }

        private void Update()
        {
            if (IsOwner)
            {
                if (body)
                {
                    var z = -movement.x * turningRate * Time.deltaTime;
                    body.Rotate(0f, 0f, z);
                }
            }
        }

        private void FixedUpdate()
        {
            if ((previousPosition - transform.position).sqrMagnitude > ParticleStopThreshold)
            {
                emissionModule.rateOverTime = particleEmissionValue;
            }
            else
            {
                emissionModule.rateOverTime = 0f;
            }
            
            previousPosition = transform.position;
            
            if (IsOwner)
            {
                if (rigid && body)
                {
                    rigid.velocity = body.up * (movement.y * speed);
                }
            }
        }
    }
}
