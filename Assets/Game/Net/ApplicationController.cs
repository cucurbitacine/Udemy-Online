using System.Collections;
using System.Threading.Tasks;
using Game.Net.Client;
using Game.Net.Host;
using Game.Net.Server;
using Game.Utils;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace Game.Net
{
    public class ApplicationController : Singleton<ApplicationController>
    {
        public static bool IsDedicatedServer => SystemInfo.graphicsDeviceType == GraphicsDeviceType.Null;
        
        [SerializeField] private ClientController clientPrefab;
        [SerializeField] private HostController hostPrefab;
        [SerializeField] private ServerController serverPrefab;

        [Space]
        [SerializeField] private NetworkObject playerPrefab; 
            
        private ApplicationData _applicationData;
        
        private async Task<bool> CreateClient()
        {
            Log("Creating Client...");

            var client = Instantiate(clientPrefab);

            var created = await client.CreateClient();

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

            await host.CreateHost(playerPrefab);

            Log("Host was created!");
        }

        private async Task CreateServer()
        {
            Application.targetFrameRate = 60;
            
            _applicationData = new ApplicationData();
            
            Log("Creating Server...");

            var server = Instantiate(serverPrefab);

            StartCoroutine(LoadGameSceneAsync(server));
        }

        private IEnumerator LoadGameSceneAsync(ServerController server)
        {
            var loading = SceneManager.LoadSceneAsync(GameManager.GameSceneName);

            while (loading != null && !loading.isDone)
            {
                yield return null;
            }
            
            var serverCreating = server.CreateServer(playerPrefab);

            yield return new WaitUntil(() => serverCreating.IsCompleted);
            
            Log("Server was created!");

            var serverStarting = server.GameManager.StartGameServerAsync();
            
            yield return new WaitUntil(() => serverStarting.IsCompleted);
            
            Log("Server was started!");
            
            // Load Game Scene
            //networkManager.SceneManager.LoadScene(GameSceneName, LoadSceneMode.Single);
        }
        
        private async Task LaunchInMode(bool isDedicatedServer)
        {
            if (isDedicatedServer)
            {
                await CreateServer();
            }
            else
            {
                await CreateHost();
                
                var clientWasCreated = await CreateClient();
                
                if (clientWasCreated)
                {
                    ClientController.Instance.GameManager.LoadMainMenu();
                }
            }
        }

        protected override async void OnStartSingleton()
        {
            Log("Launching...");

            await LaunchInMode(IsDedicatedServer);

            Log("Launched!");
        }
    }
}