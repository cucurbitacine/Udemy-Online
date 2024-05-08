using Game.Player;
using TMPro;
using Unity.Collections;
using UnityEngine;

namespace Game.UI
{
    public class PlayerNameDisplay : MonoBehaviour
    {
        public TankPlayer player;
        public TMP_Text nameDisplay;

        private void SetName(string playerName)
        {
            if (nameDisplay)
            {
                nameDisplay.text = playerName;
            }
        }
        
        private void HandleName(FixedString32Bytes previousValue, FixedString32Bytes newValue)
        {
            SetName(newValue.Value);
        }
        
        private void Start()
        {
            if (player)
            {
                SetName(player.PlayerName.Value.Value);
                
                player.PlayerName.OnValueChanged += HandleName;
            }
        }
        
        private void OnDestroy()
        {
            if (player)
            {
                player.PlayerName.OnValueChanged -= HandleName;
            }
        }
    }
}