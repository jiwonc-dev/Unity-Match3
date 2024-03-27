using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class TimeFunc
{
    public float durationSec;
    public float restTime;
    public System.Action f;
    public TimeFunc(float durationSec, System.Action f)
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
    private void Awake()
    {
        if(m_instance == null)
        {
            DontDestroyOnLoad(this);
            m_instance = this;
        }
    }
    private void Update()
    {
        foreach(var timeFunc in timeFuncDict.Values)
        {
            timeFunc.restTime -= Time.deltaTime;
            if(timeFunc.restTime <= timeFunc.durationSec)
            {
                timeFunc.f();
                timeFunc.restTime = timeFunc.durationSec;
            }
        }   
    }
    public void AddTimer(string timerID, float durationSec, System.Action timeF)
    {
        if (timeFuncDict[ timerID ] != null ) {
            timeFuncDict.Remove(timerID);
        }
        timeFuncDict[ timerID ] = new TimeFunc(durationSec timeF);
    }

    public void RemoveTimer(string timerID)
    {
        timeFuncDict.Remove(timerID);
    }
}
