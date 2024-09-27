using System;
using UnityEngine;

namespace DefaultNamespace
{
    public class MonoBehaviourEvents : MonoBehaviour
    {
        public event Action<bool> OnApplicationPauseEvent; 
        private void OnApplicationPause(bool pauseStatus)
        {
            OnApplicationPauseEvent?.Invoke(pauseStatus);
        }
    }
}