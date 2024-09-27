using System;
using DPackage.DI;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Scenes
{
    public class BootstrapSceneManager : MonoBehaviour
    {
        private void Awake()
        {
            var timeService = new TimeService();
            timeService.Init();
            DProjectContext.Bind(timeService);

            SceneManager.LoadScene(1);
        }
    }
}