using UnityEngine;

namespace Game.Utils
{
    public class Lifetime : MonoBehaviour
    {
        public float duration = 1f;
        
        private void Start()
        {
            Destroy(gameObject, duration);
        }
    }
}