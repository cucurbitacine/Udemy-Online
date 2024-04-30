using UnityEngine;

namespace Game.Utils
{
    public abstract class Singleton<T> : MonoBehaviour where T : Singleton<T>
    {
        private static T _instance;

        public static T Instance
        {
            get
            {
                if (_instance) return _instance;

                _instance = FindObjectOfType<T>();

                if (_instance == null)
                {
                    Debug.LogError($"No \"{typeof(T).Name}\" in the Scene");
                }

                return _instance;
            }
        }

        public bool debugLog = true;
        
        protected virtual void OnStartSingleton()
        {
        }

        private string GetName()
        {
            return $"{name} ({GetType().Name}";
        }

        protected void Log(string msg)
        {
            if (debugLog) Debug.Log($"[{GetName()}] {msg}");
        }
        
        protected void LogWarning(string msg)
        {
            if (debugLog) Debug.LogWarning($"[{GetName()}] {msg}");
        }
        
        protected void LogError(string msg)
        {
            if (debugLog) Debug.LogError($"[{GetName()}] {msg}");
        }
        
        private void Start()
        {
            if (Instance == this)
            {
                DontDestroyOnLoad(gameObject);

                OnStartSingleton();
            }
        }
    }
}