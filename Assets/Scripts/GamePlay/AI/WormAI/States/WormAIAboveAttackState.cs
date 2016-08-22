﻿using UnityEngine;
using System.Collections;

public class WormAIAboveAttackState : WormAIBaseState
{
    private enum SubState
    {
        WARNING_PLAYER,
        JUMPING,
        EXITING
    }

    private SubState subState;

    private HexagonController destiny;

    private float currentX;
    private Vector3 lastPosition;
    private Quaternion initialRotation;
    private float rotation;
    private float destinyInRangeDistance = 1f;
    private bool destinyInRange;
    private float speed;

    private float elapsedTime;

    public WormAIAboveAttackState(WormBlackboard bb) : base(bb)
    { }

    public override void OnStateEnter()
    {
        base.OnStateEnter();

        destiny = null;

        //Set origin and destiny
        GameObject playerGO = rsc.enemyMng.SelectPlayerRandom();
        if (playerGO != null)
        {
            PlayerController player = playerGO.GetComponent<PlayerController>();
            destiny = player.GetNearestHexagon();

            if (destiny != null)
            {
                speed = (headTrf.position - destiny.transform.position).magnitude / bb.AboveAttackSettingsPhase.aboveAttackJumpDuration;

                bb.CalculateParabola(headTrf.position, destiny.transform.position);

                head.agent.enabled = false;

                rotation = 0f;
                destinyInRange = false;

                //Calculate start point and prior point
                currentX = bb.GetJumpXGivenY(0, false);
                Vector3 startPosition = bb.GetJumpPositionGivenY(0, false);
                headTrf.position = startPosition;

                lastPosition = bb.GetJumpPositionGivenX(currentX);

                float fakeNextX = currentX + Time.deltaTime * 2;
                Vector3 nextPosition = bb.GetJumpPositionGivenX(fakeNextX);
                initialRotation = Quaternion.LookRotation(nextPosition - startPosition, headTrf.up);

                bb.isHeadOverground = true;

                head.audioSource.PlayOneShot(bb.aboveAttackWarningSound);
                destiny.WormAboveAttackWarning();
                subState = SubState.WARNING_PLAYER;
            }
        }
    }

    public override void OnStateExit()
    {
        base.OnStateExit();

        bb.aboveAttackCurrentCooldownTime = 0f;
        bb.aboveAttackCurrentExposureTime = 0f;
    }

    public override WormAIBaseState Update()
    {
        if (destiny == null) return head.wanderingState;

        switch (subState)
        {
            case SubState.WARNING_PLAYER:
                headTrf.rotation = Quaternion.RotateTowards(headTrf.rotation, initialRotation, 90 / bb.AboveAttackSettingsPhase.aboveAttackWarningTime * Time.deltaTime);

                if (elapsedTime >= bb.AboveAttackSettingsPhase.aboveAttackWarningTime)
                {
                    subState = SubState.JUMPING;
                }
                else
                    elapsedTime += Time.deltaTime;

                break;

            case SubState.JUMPING:
                //While not again below underground navmesh layer advance
                currentX += Time.deltaTime * speed;
                lastPosition = headTrf.position;
                headTrf.position = bb.GetJumpPositionGivenX(currentX);

                headTrf.LookAt(headTrf.position + (headTrf.position - lastPosition), headTrf.up);

                if (rotation < bb.AboveAttackSettingsPhase.aboveAttackSelfRotation)
                {
                    float angle = bb.AboveAttackSettingsPhase.aboveAttackSelfRotation / bb.AboveAttackSettingsPhase.aboveAttackJumpDuration * Time.deltaTime;
                    headTrf.Rotate(new Vector3(0, 0, angle));
                    rotation += angle;
                }

                if (!destinyInRange)
                {
                    float distanceToDestiny = (headTrf.position - destiny.transform.position).magnitude;
                    if (distanceToDestiny <= destinyInRangeDistance)
                    {
                        destinyInRange = true;
                        WormEventInfo.eventInfo.wormBb = bb;
                        rsc.eventMng.TriggerEvent(EventManager.EventType.WORM_ATTACK, WormEventInfo.eventInfo);
                        destiny.WormAboveAttackStart();
                    }
                }

                if (headTrf.position.y < -WormBlackboard.NAVMESH_LAYER_HEIGHT)
                {
                    bb.isHeadOverground = false;
                    head.SetVisible(false);

                    subState = SubState.EXITING;
                }
                break;

            case SubState.EXITING:
                currentX += Time.deltaTime * speed;
                lastPosition = headTrf.position;
                headTrf.position = bb.GetJumpPositionGivenX(currentX);

                headTrf.LookAt(headTrf.position + (headTrf.position - lastPosition));

                if (bb.isTailUnderground)
                {
                    Vector3 pos = headTrf.position;
                    pos.y = -WormBlackboard.NAVMESH_LAYER_HEIGHT;
                    headTrf.position = pos;

                    /*bb.agent.areaMask = WormBlackboard.NAVMESH_UNDERGROUND_LAYER;
                    bb.agent.enabled = true;
                    bb.agent.speed = bb.WanderingSettingsPhase.undergroundSpeed;
                    bb.agent.SetDestination(bb.GetJumpPositionGivenY(-WormBlackboard.NAVMESH_LAYER_HEIGHT, false)); //Back to entry in the underground
                    */
                    return head.wanderingState;
                }
                break;
            default:
                break;
        }

        return null;
    }
}
