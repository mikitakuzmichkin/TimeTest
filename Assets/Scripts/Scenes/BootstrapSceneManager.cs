using System;
using DPackage.DI;
using UnityEngine;

namespace Scenes
{
    public class BootstrapSceneManager : MonoBehaviour
    {
        private void Awake()
        {
            var timeService = new TimeService();
            timeService.Init();
            DProjectContext.Bind(timeService);
        }
    }
}