﻿using UnityEngine;
using System.Collections;

public class PlayerBaseState
{   
    protected PlayerBlackboard blackboard;

    //Shoot variables
    private float nextFire;
    private const float maxSideOffset = 0.4f;
    private const float minSideOffset = 0.2f;
    private float shotSideOffset = minSideOffset;
    private float sideOffsetVariation = -0.05f;

    private ColoredObjectsManager coloredObjMng;

    public virtual void Init(PlayerBlackboard bb)
    {
        blackboard = bb;

        coloredObjMng = rsc.coloredObjectsMng;
    }

    public virtual void OnStateEnter() { }

    public virtual void OnStateExit() { }

    public virtual void RetrieveInput()
    {
        //Movement
        //float h = Input.GetAxisRaw(blackboard.moveHorizontal);
        //float v = Input.GetAxisRaw(blackboard.moveVertical);
        float h = blackboard.controller.LeftStickX.Value;
        float v = blackboard.controller.LeftStickY.Value;

        blackboard.moveVector = Vector3.zero;

        if (Mathf.Abs(v) > 0 || Mathf.Abs(h) > 0)
        {
            blackboard.moveVector.x = h;
            blackboard.moveVector.y = v;
            if (blackboard.moveVector.magnitude > 1f)
                blackboard.moveVector.Normalize();

            blackboard.movePressed = true;
            blackboard.animator.SetBool("Walking", true);
            blackboard.keyPressed = true;
        }

        //Aiming
        //h = Input.GetAxisRaw(blackboard.aimHorizontal);
        //v = Input.GetAxisRaw(blackboard.aimVertical);
        h = blackboard.controller.RightStickX.Value;
        v = blackboard.controller.RightStickY.Value;

        blackboard.aimVector = Vector3.zero;

        if (Mathf.Abs(v) >= blackboard.player.aimThreshold || Mathf.Abs(h) >= blackboard.player.aimThreshold)
        {
            blackboard.aimVector.x = h;
            blackboard.aimVector.y = v;

            blackboard.aimPressed = true;
            blackboard.animator.SetBool("Aiming", true);
            blackboard.keyPressed = true;
        }

        //Shoot
        //if (Input.GetAxisRaw(blackboard.fire) > 0.1f)
        if (blackboard.controller.RightTrigger.Value > 0.1f)
        {
            blackboard.shootPressed = true;
            blackboard.keyPressed = true;
            blackboard.animator.SetBool("Shooting", true);
        }
        else
        {
            blackboard.firstShot = true;
            blackboard.player.StopNoShoot();
        }

        //Dash
        //if (Input.GetButtonDown(blackboard.dash))
        if(blackboard.controller.LeftBumper.WasPressed)
        {
            blackboard.dashPressed = true;
            blackboard.keyPressed = true;
        }

        //Special
        //if (Input.GetButtonDown(blackboard.special))
        if (blackboard.controller.RightBumper.WasPressed)
        {
            blackboard.specialPressed = true;
            blackboard.keyPressed = true;
        }

        //A Button
        //if (Input.GetButton(blackboard.greenButton))
        if (blackboard.controller.Action1.IsPressed)
        {
            blackboard.greenPressed = true;
            blackboard.colorButtonsPressed = true;
            blackboard.keyPressed = true;
        }

        //B Button
        //if (Input.GetButton(blackboard.redButton))
        if (blackboard.controller.Action2.IsPressed)
        {
            blackboard.redPressed = true;
            blackboard.colorButtonsPressed = true;
            blackboard.keyPressed = true;
        }

        //X Button
        //if (Input.GetButton(blackboard.blueButton))
        if (blackboard.controller.Action3.IsPressed)
        {
            blackboard.bluePressed = true;
            blackboard.colorButtonsPressed = true;
            blackboard.keyPressed = true;
        }

        //Y Button
        //if (Input.GetButton(blackboard.yellowButton))
        if (blackboard.controller.Action4.IsPressed)
        {
            blackboard.yellowPressed = true;
            blackboard.colorButtonsPressed = true;
            blackboard.keyPressed = true;
        }
    }

    public virtual PlayerBaseState Update()
    {
        /*actions check list:
        is he grounded?
        is he in a border?
        can he do a special?
        can he do a dash?
        can he charge a capacitor?
        can he disinfect a device?
        can he turn?
        can he shoot?
        can he move?
        */

        return null;
    }

    protected Vector3 GetScreenRelativeDirection(Vector3 direction)
    {
        return rsc.camerasMng.GetDirection(blackboard.player.transform.position, direction, blackboard.playerRayCastMask);
    }

    protected void CapacitorCharge()
    {
        if (blackboard.capacitor == null || !blackboard.colorButtonsPressed)
            return;

        if (blackboard.redPressed)
            blackboard.capacitor.ManualCharge(ChromaColor.RED);
        else if (blackboard.greenPressed)
            blackboard.capacitor.ManualCharge(ChromaColor.GREEN);
        else if (blackboard.bluePressed)
            blackboard.capacitor.ManualCharge(ChromaColor.BLUE);
        else if (blackboard.yellowPressed)
            blackboard.capacitor.ManualCharge(ChromaColor.YELLOW);
    }

    protected void DisinfectDevice()
    {
        if (blackboard.device == null || !blackboard.colorButtonsPressed)
            return;

        if (blackboard.greenPressed)
            blackboard.device.Disinfect();
        else if (blackboard.redPressed)
            blackboard.device.Infect();
    }

    private void LookAt(Vector3 destination)
    {
        Quaternion newRotation = Quaternion.LookRotation(destination);
        newRotation = Quaternion.RotateTowards(blackboard.player.transform.rotation, newRotation, blackboard.player.angularSpeed * Time.deltaTime);
        blackboard.player.transform.rotation = newRotation;
    }

    protected void Turn()
    {
        if (blackboard.aimPressed)
        {
            blackboard.aimVector = GetScreenRelativeDirection(blackboard.aimVector);
            LookAt(blackboard.aimVector);
        }
    }

    private int AngleBetween360(Vector3 v1, Vector3 v2)
    {
        Vector3 n = new Vector3(0, 1, 0);

        float signedAngle = Mathf.Atan2(Vector3.Dot(n, Vector3.Cross(v1, v2)), Vector3.Dot(v1, v2)) * Mathf.Rad2Deg;

        if (signedAngle >= 0)
            return (int)signedAngle;
        else
            return (int)(360 + signedAngle);
    }

    protected bool Move()
    {
        if(blackboard.movePressed)
        {
            float magnitude = blackboard.moveVector.magnitude;

            blackboard.moveVector = GetScreenRelativeDirection(blackboard.moveVector) * magnitude;

            blackboard.horizontalDirection = blackboard.moveVector;

            //If we are not aiming, rotate towards direction
            if (!blackboard.aimPressed)
            {
                LookAt(blackboard.moveVector);
            }
            else
            {
                int angleBetweenSticks = AngleBetween360(blackboard.aimVector, blackboard.moveVector);

                float angleRad = angleBetweenSticks * Mathf.Deg2Rad;
                float forward = Mathf.Cos(angleRad);
                float lateral = Mathf.Sin(angleRad);
                blackboard.animator.SetFloat("Forward", forward);
                blackboard.animator.SetFloat("Lateral", lateral);
            }
        }

        return blackboard.movePressed;
    }

    protected bool CanDoSpecial()
    {
        return blackboard.specialPressed &&
            ((blackboard.currentEnergy >= blackboard.player.specialAttackNecessaryEnergy) ||
            rsc.debugMng.godMode);
    }

    protected void Shoot()
    {
        if (blackboard.shootPressed)
        {
            if (blackboard.canShoot)
            {
                rsc.rumbleMng.AddContinousRumble(2, blackboard.player.Id, 0.22f, 0.104f);

                if (Time.time > nextFire)
                {
                    nextFire = Time.time + blackboard.player.fireRate;

                    // check if it's first shot (single projectile)...
                    if (blackboard.firstShot)
                    {
                        //Get a shot from pool
                        PlayerShotController shot = coloredObjMng.GetPlayer1Shot();
                        MuzzleController muzzle = coloredObjMng.GetPlayer1Muzzle();

                        if (shot != null && muzzle != null)
                        {
                            Transform shotSpawn = blackboard.player.shotSpawn;
                            shot.transform.position = shotSpawn.position;
                            shot.transform.rotation = shotSpawn.rotation;
                            shot.damage *= 2;
                            shot.Player = blackboard.player;
                            shot.Shoot();

                            Transform muzzlePoint = blackboard.player.muzzlePoint;
                            muzzle.transform.SetParent(muzzlePoint);
                            muzzle.transform.position = muzzlePoint.position;
                            muzzle.transform.rotation = muzzlePoint.rotation;
                            muzzle.Play();
                        }
                        blackboard.firstShot = false;
                    }
                    // ...or not (double projectile)
                    else
                    {
                        //Get two shots from pool
                        PlayerShotController shot1 = coloredObjMng.GetPlayer1Shot();
                        PlayerShotController shot2 = coloredObjMng.GetPlayer1Shot();

                        MuzzleController muzzle1 = coloredObjMng.GetPlayer1Muzzle();
                        MuzzleController muzzle2 = coloredObjMng.GetPlayer1Muzzle();

                        if (shot1 != null && shot2 != null && muzzle1 != null && muzzle2 != null)
                        {
                            Transform shotSpawn = blackboard.player.shotSpawn;
                            Transform muzzlePoint = blackboard.player.muzzlePoint;

                            shot1.transform.rotation = shotSpawn.rotation;
                            shot1.transform.position = shotSpawn.position;
                            shot1.transform.Translate(new Vector3(shotSideOffset, 0, 0));
                            shot1.Player = blackboard.player;
                            shot1.Shoot();

                            muzzle1.transform.position = muzzlePoint.position;
                            muzzle1.transform.rotation = muzzlePoint.rotation;
                            muzzle1.transform.SetParent(muzzlePoint);
                            muzzle1.transform.Translate(new Vector3(shotSideOffset, 0, 0));
                            muzzle1.Play();

                            shot2.transform.rotation = shotSpawn.rotation;
                            shot2.transform.position = shotSpawn.position;
                            shot2.transform.Translate(new Vector3(-shotSideOffset, 0, 0));
                            shot2.Player = blackboard.player;
                            shot2.Shoot();

                            muzzle2.transform.position = muzzlePoint.position;
                            muzzle2.transform.rotation = muzzlePoint.rotation;
                            muzzle2.transform.SetParent(muzzlePoint);
                            muzzle2.transform.Translate(new Vector3(-shotSideOffset, 0, 0));
                            muzzle2.Play();

                            if (shotSideOffset <= minSideOffset || shotSideOffset >= maxSideOffset)
                                sideOffsetVariation *= -1;

                            shotSideOffset += sideOffsetVariation;
                        }
                    }
                }
            }
            else
            {
                blackboard.player.StartNoShoot();
            }
        }
    }

    public virtual PlayerBaseState TakeDamage(float damage, bool triggerDamageAnim = true, bool whiteBlink = true)
    {
        if (rsc.debugMng.godMode || blackboard.isInvulnerable) return null;

        blackboard.blinkController.BlinkWhiteOnce();

        blackboard.currentHealth -= damage;

        if (blackboard.currentHealth <= 0) blackboard.currentHealth = 0;

        //Send event
        PlayerDamagedEventInfo.eventInfo.damage = damage;
        PlayerDamagedEventInfo.eventInfo.currentHealth = blackboard.currentHealth;
        rsc.eventMng.TriggerEvent(EventManager.EventType.PLAYER_DAMAGED, PlayerDamagedEventInfo.eventInfo);

        if (blackboard.currentHealth == 0)
        {           
            return blackboard.dyingState;
        }
        else
        {
            if (triggerDamageAnim)
                return blackboard.receivingDamageState;
        }

        return null;
    }

    public virtual PlayerBaseState TakeDamage(float damage, ChromaColor color, bool triggerDamageAnim = true, bool whiteBlink = true)
    {
        //Check color if needed

        return TakeDamage(damage, triggerDamageAnim);
    }

    public virtual PlayerBaseState AttackReceived(float damage, ChromaColor color, Vector3 origin)
    {
        if (rsc.debugMng.godMode || blackboard.isInvulnerable) return null;

        //Shield will be deployed either while shooting or while aiming
        bool isShieldDeployed = blackboard.shootPressed || blackboard.aimPressed;

        bool shouldTakeDamage = true;
        bool shouldRechargeEnergy = false;
        float damageRatio = 1f;

        if (isShieldDeployed)
        {
            Vector3 forward = blackboard.player.transform.forward;
            forward.y = 0;
            Vector3 playerEnemy = origin - blackboard.player.transform.position;
            playerEnemy.y = 0;

            float angle = Vector3.Angle(forward, playerEnemy);

            //Debug.Log("Angle: " + angle + " // Attack color: " + ChromaColorInfo.GetColorName(color) + " // Current color: " + ChromaColorInfo.GetColorName(blackboard.currentColor));

            if (angle < blackboard.player.maxAngleToShieldBlocking)
            {
                if (color == blackboard.currentColor)
                {
                    shouldTakeDamage = false;
                    shouldRechargeEnergy = true;
                }
                else
                {
                    damageRatio = blackboard.player.damageRatioWhenBlockedWrongColor;
                }
            }
        }

        if (shouldRechargeEnergy)
        {
            blackboard.player.PlayAttackBlocked();
            blackboard.player.RechargeEnergy(blackboard.player.energyIncreaseWhenBlockedCorrectColor);
        }

        if (shouldTakeDamage)
        {
            PlayerBaseState result = TakeDamage((damage * damageRatio), color, true, false);
            if (blackboard.isAffectedByContact || blackboard.isContactCooldown)
            {
                blackboard.player.StopCoroutine(HandleEnemyTouched());
                blackboard.isAffectedByContact = false;
                blackboard.isContactCooldown = false;
            }
            blackboard.player.StartCoroutine(HandleInvulnerabilityTime());
            return result;
        }

        return null;
    }

    public virtual PlayerBaseState EnemyContactOnInvulnerabilityEnd()
    {
        blackboard.contactFlag = false;

        if (rsc.debugMng.godMode || blackboard.isInvulnerable) return null;

        PlayerBaseState result = TakeDamage((blackboard.player.damageAfterInvulnerability), true, false);
        if (blackboard.isAffectedByContact || blackboard.isContactCooldown)
        {
            blackboard.player.StopCoroutine(HandleEnemyTouched());
            blackboard.isAffectedByContact = false;
            blackboard.isContactCooldown = false;
        }
        blackboard.player.StartCoroutine(HandleInvulnerabilityTime());

        return result;
    }

    public void StartInvulnerabilityTime()
    {
        blackboard.player.StartCoroutine(HandleInvulnerabilityTime());
    }

    private IEnumerator HandleInvulnerabilityTime()
    {
        blackboard.blinkController.BlinkTransparentMultipleTimes(blackboard.player.invulnerabilityTimeAfterHit);

        blackboard.isInvulnerable = true;
        Physics.IgnoreLayerCollision(blackboard.playerPhysicsLayer, blackboard.enemyPhysicsPlayer, true);

        yield return new WaitForSeconds(blackboard.player.invulnerabilityTimeAfterHit);

        blackboard.isInvulnerable = false;

        //If player are colliding with enemies when finishing invulnerability time, not reactivate collitions and set contact flat
        if (blackboard.enemiesInRange.Count > 0)
            blackboard.contactFlag = true;
        else
            Physics.IgnoreLayerCollision(blackboard.playerPhysicsLayer, blackboard.enemyPhysicsPlayer, false);
    }


    public virtual PlayerBaseState EnemyTouched()
    {
        if (rsc.debugMng.godMode) return null;

        //If touched by an enemy, speed reduction and damage take
        if (!blackboard.isAffectedByContact && !blackboard.isContactCooldown && !blackboard.isInvulnerable)
        {
            blackboard.player.StartCoroutine(HandleEnemyTouched());
            return TakeDamage(blackboard.player.damageOnContact, false);
        }

        return null;
    }

    private IEnumerator HandleEnemyTouched()
    {
        blackboard.isAffectedByContact = true;

        yield return new WaitForSeconds(blackboard.player.speedReductionTimeOnContact);

        blackboard.isAffectedByContact = false;
        blackboard.isContactCooldown = true;

        yield return new WaitForSeconds(blackboard.player.cooldownTime);

        blackboard.isContactCooldown = false;
    }


    public virtual PlayerBaseState ColorMismatch()
    {
        if (rsc.debugMng.godMode) return null;

        PlayerEventInfo.eventInfo.player = blackboard.player;
        rsc.eventMng.TriggerEvent(EventManager.EventType.PLAYER_COLOR_MISMATCH, PlayerEventInfo.eventInfo);

        if (blackboard.player.fireSuppresionTimeOnColorMismatch > 0f)
        {
            if (blackboard.canShoot)
            {
                blackboard.player.StartCoroutine(ColorMismatchHandle());
            }
        }
        return TakeDamage(blackboard.player.selfDamageOnColorMismatch, false);
        
    }

    private IEnumerator ColorMismatchHandle()
    {
        StartColorMismatch();     

        yield return new WaitForSeconds(blackboard.player.fireSuppresionTimeOnColorMismatch);

        EndColorMismatch();
    }

    protected void StartColorMismatch()
    {
        blackboard.canShoot = false;
        PlayerEventInfo.eventInfo.player = blackboard.player;
        rsc.eventMng.TriggerEvent(EventManager.EventType.PLAYER_COLOR_MISMATCH_START, PlayerEventInfo.eventInfo);
    }

    protected void EndColorMismatch()
    {
        blackboard.canShoot = true;
        blackboard.player.StopNoShoot();
        PlayerEventInfo.eventInfo.player = blackboard.player;
        rsc.eventMng.TriggerEvent(EventManager.EventType.PLAYER_COLOR_MISMATCH_END, PlayerEventInfo.eventInfo);

    }
}
