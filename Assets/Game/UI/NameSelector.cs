using Game.Net.Server;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Game.UI
{
    public class NameSelector : MonoBehaviour
    {
        [SerializeField] private int minNameLength = 3;
        [SerializeField] private int maxNameLength = 12;

        [Header("References")] [SerializeField]
        private TMP_InputField nameField;

        [SerializeField] private Button connectButton;

        private bool IsServer()
        {
            return SystemInfo.graphicsDeviceType == GraphicsDeviceType.Null;
        }
        
        private void LoadPlayerName()
        {
            var playerName = PlayerPrefs.GetString(NetworkServer.PlayerNameKey, string.Empty);

            if (nameField)
            {
                nameField.text = playerName;
            }
        }
        
        private void SavePlayerName()
        {
            if (nameField)
            {
                var playerName = nameField.text;
                
                PlayerPrefs.SetString(NetworkServer.PlayerNameKey, playerName);
            }
        }
        
        private void HandleNameChanged()
        {
            if (nameField)
            {
                HandleNameChanged(nameField.text);
            }
        }
        
        private void HandleNameChanged(string playerName)
        {
            if (connectButton)
            {
                connectButton.interactable = minNameLength <= playerName.Length && playerName.Length <= maxNameLength;
            }
        }

        private void LoadScene(int buildIndex)
        {
            SceneManager.LoadScene(buildIndex);
        }
        
        private void LoadNextScene()
        {
            LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        }
        
        private void Connect()
        {
            SavePlayerName();

            LoadNextScene();
        }
        
        private void OnEnable()
        {
            if (nameField)
            {
                nameField.onValueChanged.AddListener(HandleNameChanged);
            }

            if (connectButton)
            {
                connectButton.onClick.AddListener(Connect);
            }
        }

        private void OnDisable()
        {
            if (nameField)
            {
                nameField.onValueChanged.RemoveListener(HandleNameChanged);
            }
            
            if (connectButton)
            {
                connectButton.onClick.RemoveListener(Connect);
            }
        }

        private void Start()
        {
            if (IsServer())
            {
                LoadNextScene();
                return;
            }
            
            LoadPlayerName();
            
            HandleNameChanged();
        }
    }
}