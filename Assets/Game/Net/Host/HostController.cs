using System.Threading.Tasks;
using Game.Utils;

namespace Game.Net.Host
{
    public class HostController : Singleton<HostController>
    {
        public HostGameManager GameManager { get; private set; }
        
        public async Task Create()
        {
            GameManager = new HostGameManager();
        }

        private void OnDestroy()
        {
            GameManager?.Dispose();
        }
    }
}