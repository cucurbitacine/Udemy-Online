using UnityEngine;

namespace Game.Player
{
    [CreateAssetMenu(menuName = "Create TeamColorPalette", fileName = "TeamColorPalette", order = 0)]
    public class TeamColorPalette : ScriptableObject
    {
        [SerializeField] protected Color[] teamColors;

        public Color GetTeamColor(int teamIndex)
        {
            if (teamIndex < 0 || teamIndex >= teamColors.Length)
            {
                return Random.ColorHSV(0.0f, 1.0f, 1.0f, 1.0f, 0.5f, 1.0f);
            }
            
            return teamColors[teamIndex];
        }
    }
}