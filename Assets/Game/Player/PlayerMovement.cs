using Inputs;
using Unity.Netcode;
using UnityEngine;

namespace Game.Player
{
    public class PlayerMovement : NetworkBehaviour
    {
        public Vector2 movement;
        
        [Header("Settings")]
        public float speed = 4f;
        public float turningRate = 30f;
        
        [Header("References")]
        [SerializeField] private Transform body;
        [SerializeField] private Rigidbody2D rigid;
        
        [Space]
        [SerializeField] private InputReader input;
        
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
