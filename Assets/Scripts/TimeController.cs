using System;
using Cysharp.Threading.Tasks;
using DPackage.DI;

namespace DefaultNamespace
{
    public class TimeController : IDisposable
    {
        private TimeView _timeView;
        private TimeViewUi _ui;
        private TimeService _timeService;
        private DateTime _time;

        private bool _isDisposed = false;
        
        public void Init(TimeView timeView, TimeViewUi ui)
        {
            _isDisposed = false;
            
            _timeView = timeView;
            _ui = ui;

            _timeService = DProjectContext.GetInstance<TimeService>();
            
            _time = _timeService.CurTime;
            _timeView.SetTime(_time);
            _ui.SetTime(_time);
            
            Subscribe();
            
            UpdateTime().Forget();
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
            _time = time;
        }
        
        private async UniTaskVoid UpdateTime()
        {
            while (_isDisposed == false)
            {
                await UniTask.Delay(TimeSpan.FromSeconds(1), ignoreTimeScale: false);
                
                _time = _time.AddSeconds(1);
                _timeView.SetTime(_time);
                _ui.SetTime(_time);
            }
        }

        public void Dispose()
        {
            Unsubscribe();
            _isDisposed = true;
        }
    }
}