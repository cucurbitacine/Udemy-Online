using TMPro;
using Unity.Collections;
using UnityEngine;

namespace Game.UI.LeaderboardDisplay
{
    public class LeaderboardEntityDisplay : MonoBehaviour
    {
        [SerializeField] private TMP_Text info;

        public ulong ClientId { get; private set; }
        public int TeamIndex { get; private set; }
        public FixedString32Bytes DisplayName { get; private set; }
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
                info.text = $"{number}. {DisplayName.Value} ({Score})";
            }
        }
        
        public void Initilalize(ulong clientId, FixedString32Bytes displayName, int score)
        {
            ClientId = clientId;
            DisplayName = displayName;
            
            Score = score;

            //UpdateColor();
            
            UpdateText();
        }

        public void Initilalize(int teamIndex, FixedString32Bytes displayName, int score)
        {
            TeamIndex = teamIndex;
            DisplayName = displayName;
            
            Score = score;

            //UpdateColor();
            
            UpdateText();
        }

        public void SetColor(Color color)
        {
            if (info)
            {
                info.color = color;
            }
        }
        
        public void UpdateScore(int score)
        {
            Score = score;

            UpdateText();
        }
    }
}
