using UnityEngine;

namespace Game.Utils
{
    public class SpawnOnDestroy : MonoBehaviour
    {
        [SerializeField] private GameObject prefab;

        private void OnDestroy()
        {
            if (prefab && gameObject.scene.isLoaded)
            {
                Instantiate(prefab, transform.position, Quaternion.identity);
            }
        }
    }
}