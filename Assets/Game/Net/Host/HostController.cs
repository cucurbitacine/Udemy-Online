using System.Threading.Tasks;
using Game.Utils;
using Unity.Netcode;

namespace Game.Net.Host
{
    public class HostController : Singleton<HostController>
    {
        public HostGameManager GameManager { get; private set; }
        
        public Task CreateHost(NetworkObject playerPrefab)
        {
            GameManager = new HostGameManager(playerPrefab);
            
            return Task.CompletedTask;
        }

        private void OnDestroy()
        {
            GameManager?.Dispose();
        }
    }
}