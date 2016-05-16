﻿using UnityEngine;
using System.Collections;

public class SpiderDyingAIState : SpiderAIBaseState
{
    private ChromaColor color;

    public SpiderDyingAIState(SpiderBlackboard bb) : base(bb)
    { }

    public override void OnStateEnter()
    {
        spiderBlackboard.canReceiveDamage = false;
        spiderBlackboard.animationEnded = false;
        spiderBlackboard.animator.SetTrigger("die");

        color = spiderBlackboard.spider.color;

        ColorEventInfo.eventInfo.newColor = color;
        rsc.eventMng.TriggerEvent(EventManager.EventType.ENEMY_DIED, ColorEventInfo.eventInfo);
    }

    public override AIBaseState Update()
    {
        if (spiderBlackboard.animationEnded)
        {
            spiderBlackboard.spider.SpawnVoxels();
            rsc.poolMng.spiderPool.AddObject(spiderBlackboard.entityGO);
        }

        return null;
    }
}
