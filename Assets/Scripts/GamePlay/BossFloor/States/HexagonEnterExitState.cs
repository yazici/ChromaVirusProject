﻿using UnityEngine;
using System.Collections;

public class HexagonEnterExitState : HexagonBaseState
{
    public HexagonEnterExitState(HexagonController hex) : base(hex) { }

    public override void OnStateEnter()
    {
        base.OnStateEnter();

        hex.StartPlaneInfectionAnimation();

        hex.columnBlinkController.InvalidateMaterials();
        hex.columnBlinkController.BlinkWhiteNoStop(hex.enterExitBlinkInterval, hex.enterExitBlinkInterval);

        if (!hex.continousPurple.isPlaying)
            hex.continousPurple.Play();
    }

    public override void OnStateExit()
    {
        base.OnStateExit();

        hex.columnBlinkController.StopPreviousBlinkings();
        hex.continousPurple.Stop();

        hex.StopPlaneInfectionAnimation();
    }

    public override HexagonBaseState Update()
    {
        ReturnToPlace();

        return null;
    }

    public override HexagonBaseState WormTailExited()
    {
        hex.CurrentInfectionDuration = hex.infectionTimeAfterEnterExit;
        return hex.infectedState;
    }

    public override HexagonBaseState PlayerStay(PlayerController player)
    {
        player.ReceiveInfection(hex.enterExitDamage, hex.transform.position, hex.infectionForces);
        return null;
    }
}