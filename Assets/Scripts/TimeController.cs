using System;
using Cysharp.Threading.Tasks;
using DPackage.DI;
using UnityEngine;

namespace DefaultNamespace
{
    public class TimeController : IDisposable
    {
        private TimeView _timeView;
        private TimeViewUi _ui;
        private TimeService _timeService;

        private bool _isDisposed = false;
        
        public void Init(TimeView timeView, TimeViewUi ui)
        {
            _isDisposed = false;
            
            _timeView = timeView;
            _ui = ui;

            _timeService = DProjectContext.GetInstance<TimeService>();
            
            var time = _timeService.CurTime;
            _timeView.SetTime(time);
            _ui.SetTime(time);
            
            Subscribe();
        }

        private void Subscribe()
        {
            _timeService.OnTimeChanged += UpdateTimeFromServer;
        }

        private void Unsubscribe()
        {
            _timeService.OnTimeChanged -= UpdateTimeFromServer;
        }

        private void UpdateTimeFromServer(DateTime time)
        {
            _timeView.SetTime(time);
            _ui.SetTime(time);
        }
        
        public void Dispose()
        {
            Unsubscribe();
            _isDisposed = true;
        }
    }
}