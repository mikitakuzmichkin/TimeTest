using System;
using TMPro;
using UnityEngine;

namespace DefaultNamespace
{
    public class TimeViewUi : MonoBehaviour
    {
        [SerializeField] private TMP_InputField _hourInput;
        [SerializeField] private TMP_InputField _minuteInput;
        [SerializeField] private TMP_InputField _secondInput;
        
        public void SetTime(DateTime time)
        {
            _hourInput.text = time.Hour.ToString("00");
            _minuteInput.text = time.Minute.ToString("00");
            _secondInput.text = time.Second
                .ToString("00");
        }
    }
}