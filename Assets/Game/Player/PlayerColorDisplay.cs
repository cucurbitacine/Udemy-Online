using UnityEngine;

namespace Game.Player
{
    public class PlayerColorDisplay : MonoBehaviour
    {
        [SerializeField] private TankPlayer player;
        
        [Space]
        [SerializeField] private Color colorDefault = Color.white;
        [SerializeField] private TeamColorPalette palette;
        
        [Space]
        [SerializeField] private SpriteRenderer[] sprites;
        
        private void Paint(Color color)
        {
            if (sprites != null)
            {
                foreach (var sprite in sprites)
                {
                    if (sprite)
                    {
                        sprite.color = color;
                    }
                }
            }
        }

        private void HandleTeam(int previousValue, int newValue)
        {
            if (newValue >= 0)
            {
                if (palette)
                {
                    var teamColor = palette.GetTeamColor(newValue);

                    Paint(teamColor);
                }
            }
            else
            {
                Paint(colorDefault);
            }
        }

        private void Start()
        {
            if (player)
            {
                player.TeamIndex.OnValueChanged += HandleTeam;
                
                HandleTeam(-1, player.TeamIndex.Value);
            }
        }

        private void OnDestroy()
        {
            if (player)
            {
                player.TeamIndex.OnValueChanged -= HandleTeam;
            }
        }
    }
}