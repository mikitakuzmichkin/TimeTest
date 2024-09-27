using System;
using UnityEngine;

namespace DefaultNamespace
{
    public class TimeView : MonoBehaviour
    {
        [SerializeField] private Transform _hourArrow;
        [SerializeField] private Transform _minuteArrow;
        [SerializeField] private Transform _secondArrow;
        public void SetTime(DateTime time)
        {
            _hourArrow.transform.rotation = Quaternion.Euler(0, 0, -30 * time.Hour - 0.5f * time.Minute);
            _minuteArrow.transform.rotation = Quaternion.Euler(0, 0, -6 * time.Minute - 0.1f * time.Second);
            _secondArrow.transform.rotation = Quaternion.Euler(0, 0, -6 * time.Second);
        }
    }
}