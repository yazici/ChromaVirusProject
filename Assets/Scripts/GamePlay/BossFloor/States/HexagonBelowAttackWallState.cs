﻿using UnityEngine;
using System.Collections;

public class HexagonBelowAttackWallState : HexagonBaseState
{
    private enum SubState
    {
        GOING_UP,
        IDLE,
        GOING_DOWN
    }

    private SubState subState;
    private float totalHeight;

    public HexagonBelowAttackWallState(HexagonController hex) : base(hex) { }

    public override void OnStateEnter()
    {
        base.OnStateEnter();

        subState = SubState.GOING_UP;
        totalHeight = hex.geometryOriginalY + hex.wallHeight;
    }

    public override void OnStateExit()
    {
        base.OnStateExit();

        hex.geometryOffset.transform.position = new Vector3(hex.geometryOffset.transform.position.x, hex.geometryOriginalY, hex.geometryOffset.transform.position.z);
    }

    public override HexagonBaseState Update()
    {
        if (!hex.shouldBeWall) subState = SubState.GOING_DOWN;

        switch (subState)
        {
            case SubState.GOING_UP:
                if(hex.geometryOffset.transform.position.y < totalHeight)
                {
                    float displacement = Time.deltaTime * hex.wallSpeed;
                    hex.geometryOffset.transform.position += new Vector3(0f, displacement, 0f);
                }
                else
                {
                    hex.geometryOffset.transform.position += new Vector3(0f, totalHeight, 0f);

                    subState = SubState.IDLE;
                }
                break;

            case SubState.GOING_DOWN:
                if(hex.geometryOffset.transform.position.y > hex.geometryOriginalY)
                {
                    float displacement = Time.deltaTime * hex.wallSpeed;
                    hex.geometryOffset.transform.position -= new Vector3(0f, displacement, 0f);
                }
                else
                {
                    hex.geometryOffset.transform.position += new Vector3(0f, hex.geometryOriginalY, 0f);
                    return hex.idleState;
                }
                break;
        }

        return null;
    }
}