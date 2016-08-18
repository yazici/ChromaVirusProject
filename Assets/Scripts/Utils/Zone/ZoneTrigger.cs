﻿using UnityEngine;
using System.Collections;

public class ZoneTrigger : MonoBehaviour {

    public int zoneId;
    public bool triggerOnce = true;
    public bool triggerOnEnter = true;

    private bool triggered;

    void OnEnable()
    {
        triggered = false;
    }

    void OnTriggerEnter(Collider other)
    {
        if (!triggerOnEnter) return;

        if (other.tag == "Player1" || other.tag == "Player2")
        {
            if (!triggerOnce || !triggered)
            {
                ZoneReachedInfo.eventInfo.zoneId = zoneId;
                ZoneReachedInfo.eventInfo.playerTag = other.tag;
                rsc.eventMng.TriggerEvent(EventManager.EventType.ZONE_REACHED, ZoneReachedInfo.eventInfo);
            }

            triggered = true;
        }       
    }

    void OnTriggerExit(Collider other)
    {
        if (triggerOnEnter) return;

        if (other.tag == "Player1" || other.tag == "Player2")
        {
            if (!triggerOnce || !triggered)
            {
                ZoneReachedInfo.eventInfo.zoneId = zoneId;
                ZoneReachedInfo.eventInfo.playerTag = other.tag;
                rsc.eventMng.TriggerEvent(EventManager.EventType.ZONE_REACHED, ZoneReachedInfo.eventInfo);
            }

            triggered = true;
        }
    }
}
