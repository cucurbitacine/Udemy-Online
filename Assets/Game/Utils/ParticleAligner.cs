using System;
using UnityEngine;

namespace Game.Utils
{
    [RequireComponent(typeof(ParticleSystem))]
    public class ParticleAligner : MonoBehaviour
    {
        private ParticleSystem _particleSystem;
        private ParticleSystem.MainModule _mainModule;
        
        private void Awake()
        {
            _particleSystem = GetComponent<ParticleSystem>();
        }

        private void Start()
        {
            _mainModule = _particleSystem.main;
        }

        private void Update()
        {
            _mainModule.startRotation = -transform.rotation.eulerAngles.z * Mathf.Deg2Rad;
        }
    }
}
