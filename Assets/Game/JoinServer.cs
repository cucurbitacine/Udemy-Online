using Unity.Netcode;
using UnityEngine;

namespace Game
{
    public class JoinServer : MonoBehaviour
    {
        public void Join()
        {
            NetworkManager.Singleton.StartClient();
        }
    }
}