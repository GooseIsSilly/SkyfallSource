using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class TimeEvent : MonoBehaviour
{
    public bool startTimerOnStart;
    bool startTimer;
    public TimedEvent[] timedEvent;
    float Timer;

    private void Start()
    {
        if (startTimerOnStart == true) {
            startTimer = true;
        }
    }

    void Update()
    {
        if (startTimer == true) {

            Timer += Time.deltaTime;

            foreach (TimedEvent events in timedEvent)
            {
                if (Timer >= events.Time) {
                    events.afterTime.Invoke();
                }
            }
        }
    }

    public void StartTimer() {
        startTimer = true;
    }

}

[System.Serializable]
public class TimedEvent {

    public string name;
    public float Time;
    public UnityEvent afterTime;
    bool hasHappend;
    
}

