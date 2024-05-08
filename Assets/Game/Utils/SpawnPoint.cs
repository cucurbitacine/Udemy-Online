using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Game.Utils
{
    public class SpawnPoint : MonoBehaviour
    {
        #region Static

        private static readonly List<SpawnPoint> Points = new List<SpawnPoint>();

        public static Vector2 GetRandomSpawnPoint()
        {
            if (Points.Count > 0)
            {
                return Points[Random.Range(0, Points.Count)].position;
            }

            return Vector2.zero;
        }

        #endregion

        public Vector2 position => transform.position;

        #region MonoBehaviour

        private void OnEnable()
        {
            Points.Add(this);
        }

        private void OnDisable()
        {
            Points.Remove(this);
        }

        #endregion

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(position, 0.5f);
        }
        
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(position, 0.5f);
        }
    }
}
