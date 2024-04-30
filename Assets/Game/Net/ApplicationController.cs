using System.Threading.Tasks;
using Game.Net.Client;
using Game.Net.Host;
using Game.Utils;
using UnityEngine;
using UnityEngine.Rendering;

namespace Game.Net
{
    public class ApplicationController : Singleton<ApplicationController>
    {
        [SerializeField] private ClientController clientPrefab;
        [SerializeField] private HostController hostPrefab;

        private async Task<bool> CreateClient()
        {
            Log("Creating Client...");

            var client = Instantiate(clientPrefab);

            var created = await client.Create();

            if (created)
            {
                Log("Client was created!");
            }
            else
            {
                LogError("Client was NOT created!");
            }

            return created;
        }

        private async Task CreateHost()
        {
            Log("Creating Host...");

            var host = Instantiate(hostPrefab);

            await host.Create();

            Log("Host was created!");
        }

        private async Task LaunchInMode(bool isDedicatedServer)
        {
            if (isDedicatedServer)
            {
                //
            }
            else
            {
                await CreateHost();
                
                var clientWasCreated = await CreateClient();
                
                if (clientWasCreated)
                {
                    ClientController.Instance.GameManager.LoadMenu();
                }
            }
        }

        protected override async void OnStartSingleton()
        {
            Log("Launching...");

            var isDedicatedServer = SystemInfo.graphicsDeviceType == GraphicsDeviceType.Null;

            await LaunchInMode(isDedicatedServer);

            Log("Launched!");
        }
    }
}