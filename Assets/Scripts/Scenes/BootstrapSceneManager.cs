using System;
using DefaultNamespace;
using DPackage.DI;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Scenes
{
    public class BootstrapSceneManager : MonoBehaviour
    {
        [SerializeField] private MonoBehaviourEvents _monoBehaviourEvents;
        private void Awake()
        {
            DontDestroyOnLoad(_monoBehaviourEvents.gameObject);
            DProjectContext.Bind(_monoBehaviourEvents);
            
            var timeService = new TimeService();
            timeService.Init();
            DProjectContext.Bind(timeService);

            SceneManager.LoadScene(1);
        }
    }
}