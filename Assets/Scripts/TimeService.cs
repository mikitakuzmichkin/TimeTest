using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

public class TimeService
{
    private const string _URL = "https://yandex.com/time/sync.json";

    private DateTime _curTime;

    public event Action<DateTime> OnTimeChanged;

    public DateTime GetCurTime => _curTime;
    
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
        var txt = (await UnityWebRequest.Get(_URL).SendWebRequest()).downloadHandler.text;
        Debug.Log(txt);
    }
}
