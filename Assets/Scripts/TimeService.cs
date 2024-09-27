using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DefaultNamespace;
using DPackage.DI;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Networking;

public class TimeService : IDisposable
{
    private const string _URL = "https://yandex.com/time/sync.json";

    private DateTime _curTime;

    public event Action<DateTime> OnTimeChanged;

    public DateTime CurTime => _curTime;
    
    public void Init()
    {
        UpdateTimeByServer().Forget();
        UpdateTime().Forget();
        Subscribe();
    }

    private void Subscribe()
    {
        DProjectContext.GetInstance<MonoBehaviourEvents>().OnApplicationPauseEvent += UpdateAfterPause;
    }

    private void UnSubscribe()
    {
        DProjectContext.GetInstance<MonoBehaviourEvents>().OnApplicationPauseEvent -= UpdateAfterPause;
    }

    public async UniTaskVoid UpdateTimeByServer()
    {
        while (true)
        {
            TimeServerRequest().Forget();
            await UniTask.Delay(TimeSpan.FromHours(1), ignoreTimeScale: false);
        }
    }
    
    private async UniTaskVoid UpdateTime()
    {
        while (true)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(1), ignoreTimeScale: false);
                
            _curTime = _curTime.AddSeconds(1);
            OnTimeChanged?.Invoke(_curTime);
        }
    }

    private void UpdateAfterPause(bool pauseStatus)
    {
        if (pauseStatus == false)
        {
            ForceUpdateTime();
        }
    }

    public void ForceUpdateTime()
    {
        TimeServerRequest().Forget();
    }

    public async UniTaskVoid TimeServerRequest()
    {
        var request = (await UnityWebRequest.Get(_URL).SendWebRequest());
        if (request.result == UnityWebRequest.Result.Success)
        {
            var txt = request.downloadHandler?.text;
            if (txt != null)
            {
                var jObject = JObject.Parse(txt);
                var timeStr = jObject["time"]?.ToString();
                if (timeStr != null)
                {
                    double ticks = double.Parse(timeStr);
                    TimeSpan timeSpan = TimeSpan.FromMilliseconds(ticks);
                    DateTime time = (new DateTime(1970, 1, 1) + timeSpan).ToLocalTime();
                    _curTime = time;
                    OnTimeChanged?.Invoke(_curTime);
                }
            }
        }
    }

    public void Dispose()
    {
        UnSubscribe();
    }
}
