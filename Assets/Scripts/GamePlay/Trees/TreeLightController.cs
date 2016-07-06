﻿using UnityEngine;
using System.Collections;

public class TreeLightController : MonoBehaviour 
{
    private Light light;

    void Awake()
    {
        light = GetComponent<Light>();
    }
	// Use this for initialization
	void Start () 
	{
        rsc.eventMng.StartListening(EventManager.EventType.COLOR_CHANGED, ColorChanged);
	}

    void OnDestroy()
    {
        if(rsc.eventMng != null)
        {
            rsc.eventMng.StopListening(EventManager.EventType.COLOR_CHANGED, ColorChanged);
        }
    }
	
	private void ColorChanged(EventInfo eventInfo)
    {
        ColorEventInfo info = (ColorEventInfo)eventInfo;
        light.color = rsc.coloredObjectsMng.GetColor(info.newColor);
    }
}