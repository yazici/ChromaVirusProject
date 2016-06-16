﻿using UnityEngine;
using System.Collections;

public class MosquitoAttackingPlayerAIState : MosquitoAIActionsBaseState
{
    public MosquitoAttackingPlayerAIState(MosquitoBlackboard bb) : base(bb)
    { }

    public override void OnStateEnter()
    {
        rsc.enemyMng.blackboard.MosquitoStartsAttacking();
        base.OnStateEnter();
    }

    public override void OnStateExit()
    {
        rsc.enemyMng.blackboard.MosquitoStopsAttacking();
        base.OnStateExit();
    }

    public override AIBaseState Update()
    {
        int updateResult = UpdateExecution();

        if (updateResult == AIAction.LIST_FINISHED)
            return mosquitoBlackboard.patrolingState;

        return ProcessUpdateExecutionResult(updateResult);
    }
}
