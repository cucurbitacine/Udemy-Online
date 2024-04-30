using Game.Net.Client;
using Game.Net.Host;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
    public class MainMenu : MonoBehaviour
    {
        [SerializeField] private Button hostButton;
        [SerializeField] private Button clientButton;
        [SerializeField] private Button lobbyButton;
        [SerializeField] private TMP_InputField codeField;

        private async void StartHost()
        {
            await HostController.Instance.GameManager.StartHostAsync();
        }

        private async void StartClient()
        {
            var joinCode = codeField.text;
            
            await ClientController.Instance.GameManager.StartClientAsync(joinCode);
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
        }
    }
}