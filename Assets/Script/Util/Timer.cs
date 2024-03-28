using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class TimeFunc
{
    public float durationSec;
    public float restTime;
    public Action f;
    public TimeFunc(float durationSec, Action f)
    {
        this.durationSec = durationSec;
        this.restTime = durationSec;
        this.f = f;
    }
}
public class Timer : MonoBehaviour
{
    public static Timer m_instance;
    private Dictionary<string, TimeFunc> timeFuncDict = new Dictionary<string, TimeFunc>();
    List<string> removeIDList = new List<string>();

    private void Awake()
    {
        if(m_instance == null)
        {
            DontDestroyOnLoad(this);
            m_instance = this;
        }
    }
    public void Update()
    {
        foreach(KeyValuePair<string, TimeFunc> item in timeFuncDict)
        {
            TimeFunc timeFunc = item.Value;
            timeFunc.restTime -= Time.deltaTime;
            if(timeFunc.restTime <= 0)
            {  
                timeFunc.f();
                timeFunc.restTime = timeFunc.durationSec;
            }
        }
        foreach (var timerID in removeIDList) {
            timeFuncDict.Remove(timerID);
        }
    }
    public void AddTimer(string timerID, float durationSec, Action timeF)
    {
        if (!timeFuncDict.ContainsKey( timerID )) {
            timeFuncDict.Remove(timerID);
        }
        timeFuncDict[ timerID ] = new TimeFunc(durationSec, timeF);
    }

    public void RemoveTimer(string timerID)
    {
        removeIDList.Add(timerID);
    }
}
