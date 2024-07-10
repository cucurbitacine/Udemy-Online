using System.Threading.Tasks;
using Game.Utils;
using Unity.Netcode;
using Unity.Services.Core;

namespace Game.Net.Server
{
    public class ServerController : Singleton<ServerController>
    {
        public ServerGameManager GameManager { get; private set; }
        
        public async Task CreateServer(NetworkObject playerPrefab)
        {
            await UnityServices.InitializeAsync();
            
            GameManager = new ServerGameManager(ApplicationData.IP(), ApplicationData.Port(), ApplicationData.QPort(), playerPrefab);
        }

        private void OnDestroy()
        {
            GameManager?.Dispose();
        }
    }
}