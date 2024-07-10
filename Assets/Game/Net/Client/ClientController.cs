using System.Threading.Tasks;
using Game.Utils;

namespace Game.Net.Client
{
    public class ClientController : Singleton<ClientController>
    {
        public ClientGameManager GameManager { get; private set; }

        public async Task<bool> CreateClient()
        {
            GameManager = new ClientGameManager();

            Log("Initiating Game Manager...");

            var authenticated = await GameManager.InitializeAsync();

            if (authenticated)
            {
                Log("Game Manager was initiated!");
            }
            else
            {
                LogError("Game Manager was NOT initiated!");
            }

            return authenticated;
        }

        private void OnDestroy()
        {
            GameManager?.Dispose();
        }
    }
}