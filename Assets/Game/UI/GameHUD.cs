using Game.Net.Client;
using Game.Net.Host;
using Unity.Netcode;
using UnityEngine;

namespace Game.UI
{
    public class GameHUD : MonoBehaviour
    {
        public void LeaveGame()
        {
            if (NetworkManager.Singleton.IsHost)
            {
                HostController.Instance.GameManager.Shutdown();
            }
            
            ClientController.Instance.GameManager.Disconnect();
        }
    }
}
