using System;
using DefaultNamespace;
using UnityEngine;

namespace Scenes
{
    public class TimeScene : MonoBehaviour
    {
        [SerializeField] private TimeView _timeView;
        [SerializeField] private TimeViewUi _timeViewUi;

        private void Awake()
        {
            var timeController = new TimeController();
            timeController.Init(_timeView, _timeViewUi);
        }
    }
}