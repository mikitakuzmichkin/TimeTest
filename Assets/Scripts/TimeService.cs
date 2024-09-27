using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Networking;

public class TimeService
{
    private const string _URL = "https://yandex.com/time/sync.json";

    private DateTime _curTime;

    public event Action<DateTime> OnTimeChanged;

    public DateTime CurTime => _curTime;
    
    public void Init()
    {
        UpdateTime().Forget();
    }

    public async UniTaskVoid UpdateTime()
    {
        while (true)
        {
            TimeServerRequest().Forget();
            await UniTask.Delay(TimeSpan.FromHours(1), ignoreTimeScale: false);
        }
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
                    OnTimeChanged?.Invoke(time);
                }
            }
        }
    }
}
