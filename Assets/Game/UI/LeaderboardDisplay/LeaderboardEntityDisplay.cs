using TMPro;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

namespace Game.UI.LeaderboardDisplay
{
    public class LeaderboardEntityDisplay : MonoBehaviour
    {
        [SerializeField] private TMP_Text info;
        [SerializeField] private Color myColor = Color.yellow;

        public ulong ClientId { get; private set; }
        public FixedString32Bytes PlayerName { get; private set; }
        public int Score { get; private set; }
        
        public int number;

        public void UpdateText(int number)
        {
            this.number = number;

            UpdateText();
        }
        
        public void UpdateText()
        {
            if (info)
            {
                info.text = $"{number}. {PlayerName.Value} ({Score})";
            }
        }
        
        public void UpdateColor()
        {
            if (info && NetworkManager.Singleton)
            {
                if (NetworkManager.Singleton.LocalClientId == ClientId)
                {
                    info.color = myColor;
                }
            }
        }
        
        public void UpdatePlayer(ulong clientId, FixedString32Bytes playerName, int score)
        {
            ClientId = clientId;
            PlayerName = playerName;
            
            Score = score;

            UpdateColor();
            
            UpdateText();
        }

        public void UpdateScore(int score)
        {
            Score = score;

            UpdateText();
        }
    }
}
