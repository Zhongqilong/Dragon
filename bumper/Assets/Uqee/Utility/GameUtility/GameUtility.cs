using System;
using UnityEngine;

namespace Uqee.Utility {
    public class GameUtility {
        public static void ShowCombo(float interval, Action call_back)
        {
            JobScheduler.I.SetTimeOut(call_back, interval);
        }
    }
}