using Inputs;
using Unity.Netcode;
using UnityEngine;

namespace Game.Player
{
    public class PlayerAiming : NetworkBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform turret;
        
        [Space]
        [SerializeField] private InputReader input;

        private Camera cameraMain => Camera.main;
        
        private void LateUpdate()
        {
            if (IsOwner)
            {
                if (input && turret)
                {
                    var aim = cameraMain.ScreenToWorldPoint(input.AimPosition);

                    turret.rotation = Quaternion.LookRotation(Vector3.forward, aim - turret.position);
                }
            }
        }
    }
}
