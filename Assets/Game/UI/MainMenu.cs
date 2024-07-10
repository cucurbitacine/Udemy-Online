using System;
using System.Threading.Tasks;
using Game.Net;
using Game.Net.Client;
using Game.Net.Host;
using TMPro;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
    public class MainMenu : MonoBehaviour
    {
        [Header("Match")]
        [SerializeField] private Button findButton;
        [SerializeField] private TMP_Text findButtonText;
        [SerializeField] private TMP_Text statusQueueText;
        [SerializeField] private TMP_Text timeQueueText;
        
        [Header("Lobby")]
        [SerializeField] private Button lobbyButton;
        
        [Header("Client")]
        [SerializeField] private TMP_InputField codeField;
        [SerializeField] private Button clientButton;
        
        [Header("Host")]
        [SerializeField] private Button hostButton;

        public bool isMatchmaking { get; private set; }
        public bool isCancelling { get; private set; }
        public float timeInQueue { get; private set; }
        public bool isBusy { get; private set; }
        
        private async void StartSearchMatch()
        {
            timeInQueue = 0f;
            
            if (isCancelling) return;
            
            if (isMatchmaking)
            {
                statusQueueText.text = "Cancelling...";
                isCancelling = true;

                await ClientController.Instance.GameManager.CancelMatchmaking();

                isBusy = false;
                isCancelling = false;
                isMatchmaking = false;
                findButtonText.text = "Find Match";
                statusQueueText.text = string.Empty;
                
                return;
            }

            if (isBusy) return;
            
            ClientController.Instance.GameManager.MatchmakeAsync(OnMatchmade);
            
            findButtonText.text = "Cancel";
            statusQueueText.text = "Searching...";
            isMatchmaking = true;
            isBusy = true;
        }

        private void OnMatchmade(MatchmakerPollingResult result)
        {
            switch (result)
            {
                case MatchmakerPollingResult.Success:
                    statusQueueText.text = "Connecting...";
                    break;
                default:
                    statusQueueText.text = result.ToString();
                    break;
            }
        }
        
        private async void StartHost()
        {
            if (isBusy) return;
            isBusy = true;
            
            await HostController.Instance.GameManager.StartHostAsync();
            
            isBusy = false;
        }

        private async void StartClient()
        {
            if (isBusy) return;
            isBusy = true;
            
            var joinCode = codeField.text;
            
            await ClientController.Instance.GameManager.StartClientAsync(joinCode);
            
            isBusy = false;
        }
        
        public async void StartLobbyAsync(Lobby lobby)
        {
            if (isBusy) return;

            isBusy = true;

            try
            {
                Debug.Log($"Try to join lobby by Id: {lobby.Id}");
                
                var joiningLobby = await Lobbies.Instance.JoinLobbyByIdAsync(lobby.Id);

                Debug.Log($"I joined lobby with Id: {lobby.Id}");
                Debug.Log($"Try to get Join Code");
                
                if (joiningLobby.Data.TryGetValue(GameManager.JoinCodeKey, out var joinCode))
                {
                    Debug.Log($"Join Code was found");
                    Debug.Log($"Try to start client with Join Code: {joinCode.Value}");
                    
                    await ClientController.Instance.GameManager.StartClientAsync(joinCode.Value);
                    
                    Debug.Log($"Starting client finished");
                }
                else
                {
                    Debug.LogWarning($"Join Code was not found");
                }
            }
            catch (LobbyServiceException lobbyException)
            {
                Debug.LogError(lobbyException);
            }

            isBusy = false;
        }
        
        private void OnEnable()
        {
            if (hostButton)
            {
                hostButton.onClick.AddListener(StartHost);
            }

            if (clientButton)
            {
                clientButton.onClick.AddListener(StartClient);
            }

            if (findButton)
            {
                findButton.onClick.AddListener(StartSearchMatch);
            }
        }

        private void OnDisable()
        {
            if (hostButton)
            {
                hostButton.onClick.RemoveListener(StartHost);
            }
            
            if (clientButton)
            {
                clientButton.onClick.RemoveListener(StartClient);
            }
            
            if (findButton)
            {
                findButton.onClick.RemoveListener(StartSearchMatch);
            }
        }
        
        private void Start()
        {
            if (ClientController.Instance == null) return;
            
            Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
            
            statusQueueText.text = string.Empty;
            timeQueueText.text = string.Empty;
        }

        private void Update()
        {
            if (isMatchmaking && !isCancelling)
            {
                timeInQueue += Time.deltaTime;
                
                if (timeQueueText)
                {
                    var time = TimeSpan.FromSeconds(timeInQueue);
                    timeQueueText.text = $"{time.Minutes:00}:{time.Seconds:00}";
                }
            }
            else if (timeQueueText)
            {
                timeQueueText.text = string.Empty;
            }
        }
    }
}