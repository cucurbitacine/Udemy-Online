using Game.Net.Client;
using Game.Net.Host;
using TMPro;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

namespace Game.UI
{
    public class GameHUD : NetworkBehaviour
    {
        [SerializeField] private TMP_Text joinCodeText;

        private NetworkVariable<FixedString32Bytes> joinCode = new NetworkVariable<FixedString32Bytes>(string.Empty);
        
        public void LeaveGame()
        {
            if (NetworkManager.Singleton.IsHost)
            {
                HostController.Instance.GameManager.Shutdown();
            }
            
            ClientController.Instance.GameManager.Disconnect();
        }
        
        private void HandleJoinCode(FixedString32Bytes previousValue, FixedString32Bytes newValue)
        {
            if (joinCodeText)
            {
                joinCodeText.text = newValue.Value;
            }
        }
        
        public override void OnNetworkSpawn()
        {
            if (IsHost)
            {
                joinCode.Value = HostController.Instance.GameManager.JoinCode;
            }

            if (IsClient)
            {
                joinCode.OnValueChanged += HandleJoinCode;
                HandleJoinCode("", joinCode.Value.Value);
            }
        }

        public override void OnNetworkDespawn()
        {
            if (IsClient)
            {
                joinCode.OnValueChanged -= HandleJoinCode;
            }
        }
    }
}
