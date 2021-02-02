using EntityStates;
using RoR2;
using UnityEngine;
using System;
using RoR2.Projectile;
using UnityEngine.Networking;

namespace EntityStates.ExampleSurvivorStates
{
    public class ExampleSurvivorFireArrow2 : BaseSkillState
    {
        public float damageCoefficient = 9f;
        public float baseDuration = 0.15f;
        public float recoil = 1f;
        public static GameObject tracerEffectPrefab = Resources.Load<GameObject>("Prefabs/Effects/Tracers/TracerToolbotRebar");

        private float duration;
        private float fireDuration;
        private bool hasFired;
        private Animator animator;
        private string muzzleString;

        public override void OnEnter()
        {
            base.OnEnter();
            /// cambiar de sitio en el futuro para el cabezaso
            this.duration = this.baseDuration / this.attackSpeedStat;
            this.fireDuration = 0.25f * this.duration;         //changed 0.25
            base.characterBody.SetAimTimer(2f);                 //changed 2
            this.animator = base.GetModelAnimator();
            this.muzzleString = "Muzzle";


            base.PlayAnimation("Gesture, Override", "FireArrow", "FireArrow.playbackRate", this.duration);
            base.PlayAnimation("Fuego", "ACharge");
            if (base.characterBody.isSprinting)
            {

                this.hasFired = true;
                base.skillLocator.secondary.skillDef.activationStateMachineName = "Body";
                this.outer.SetNextState(new ChargeAgu());
                return;
            }
        }

        public override void OnExit()
        {

            base.OnExit();
        }

        private void FireArrow()
        {
            if (!this.hasFired)
            {
                AkSoundEngine.PostEvent(2535423261, base.gameObject);
                this.hasFired = true;

                base.characterBody.AddSpreadBloom(0.75f);
                Ray aimRay = base.GetAimRay();
                EffectManager.SimpleMuzzleFlash(Commando.CommandoWeapon.FirePistol.effectPrefab, base.gameObject, this.muzzleString, false);

                if (base.isAuthority)
                {
                    ProjectileManager.instance.FireProjectile(ExampleSurvivor.ExampleSurvivor.arrowProjectile2, aimRay.origin, Util.QuaternionSafeLookRotation(aimRay.direction), base.gameObject, this.damageCoefficient * this.damageStat, 0f, Util.CheckRoll(this.critStat, base.characterBody.master), DamageColorIndex.Default, null, -1f);
                }
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (base.fixedAge >= this.fireDuration)
            {
                FireArrow();
            }

            if (base.fixedAge >= this.duration && base.isAuthority)
            {

                this.outer.SetNextStateToMain();
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Skill;
        }
    }
    public class ShoulderBash : BaseSkillState
    {
        public static float baseDuration = 1.65f;
        public static float chargeDamageCoefficient = 4.5f;
        public static float knockbackDamageCoefficient = 7f;
        public static float massThresholdForKnockback = 150;
        public static float knockbackForce = 24f;
        public static float smallHopVelocity = 12f;

        public static float initialSpeedCoefficient = 6f;
        public static float finalSpeedCoefficient = 0.1f;

        private float dashSpeed;
        private Vector3 forwardDirection;
        private Vector3 previousPosition;

        private bool shieldCancel;
        private float duration;
        private float hitPauseTimer;
        private OverlapAttack attack;
        private bool inHitPause;
        /// private List<HealthComponent> victimsStruck = new List<HealthComponent>();

        public override void OnEnter()
        {
            base.OnEnter();
            this.duration = ShoulderBash.baseDuration;
            this.shieldCancel = false;

            
            base.characterBody.isSprinting = true;

            Util.PlayScaledSound(Croco.Leap.leapSoundString, base.gameObject, 1.75f);


            if (base.isAuthority && base.inputBank && base.characterDirection)
            {
                this.forwardDirection = ((base.inputBank.moveVector == Vector3.zero) ? base.characterDirection.forward : base.inputBank.moveVector).normalized;
            }

            this.RecalculateSpeed();

            if (base.characterMotor && base.characterDirection)
            {
                base.characterMotor.velocity.y *= 0.5f;
                base.characterMotor.velocity = this.forwardDirection * this.dashSpeed;
            }

            Vector3 b = base.characterMotor ? base.characterMotor.velocity : Vector3.zero;
            this.previousPosition = base.transform.position - b;

            HitBoxGroup hitBoxGroup = null;
            Transform modelTransform = base.GetModelTransform();

            if (modelTransform)
            {
                hitBoxGroup = Array.Find<HitBoxGroup>(modelTransform.GetComponents<HitBoxGroup>(), (HitBoxGroup element) => element.groupName == "HeadH");
            }
            //hitBoxGroup = Array.Find<HitBoxGroup>(modelTransform.GetComponents<HitBoxGroup>(), (HitBoxGroup element) => element.groupName == "HeadH");

            this.attack = new OverlapAttack();
            this.attack.attacker = base.gameObject;
            this.attack.inflictor = base.gameObject;
            this.attack.teamIndex = base.GetTeam();
            this.attack.damage = ShoulderBash.chargeDamageCoefficient * this.damageStat;
            this.attack.hitEffectPrefab = Loader.SwingChargedFist.overchargeImpactEffectPrefab;
            this.attack.forceVector = Vector3.up * Toolbot.ToolbotDash.upwardForceMagnitude;
            this.attack.pushAwayForce = Toolbot.ToolbotDash.awayForceMagnitude;
            this.attack.hitBoxGroup = hitBoxGroup;
            this.attack.isCrit = base.RollCrit();

            // if (base.isAuthority) EffectManager.SimpleMuzzleFlash(EnforcerPlugin.Assets.shoulderBashFX, base.gameObject, "ShieldHitbox", true);
        }

        private void RecalculateSpeed()
        {
            this.dashSpeed = (4 + (0.25f * this.moveSpeedStat)) * Mathf.Lerp(ShoulderBash.initialSpeedCoefficient, ShoulderBash.finalSpeedCoefficient, base.fixedAge / this.duration);
        }

        public override void OnExit()
        {
            if (base.characterBody)
            {
                if (this.shieldCancel) base.characterBody.isSprinting = false;
                else base.characterBody.isSprinting = true;
            }

            if (base.characterMotor) base.characterMotor.disableAirControlUntilCollision = false;// this should be a thing on all movement skills tbh

            if (base.skillLocator) base.skillLocator.secondary.skillDef.activationStateMachineName = "Weapon";

            if (base.cameraTargetParams)
            {
                base.cameraTargetParams.fovOverride = -1f;
            }

            base.OnExit();
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            base.characterBody.isSprinting = true;

            if (base.fixedAge >= this.duration)
            {
                this.outer.SetNextStateToMain();
                return;
            }

            this.RecalculateSpeed();

            if (base.cameraTargetParams)
            {
                base.cameraTargetParams.fovOverride = Mathf.Lerp(Commando.DodgeState.dodgeFOV, 60f, base.fixedAge / this.duration);
            }

            if (base.isAuthority)
            {
                if (base.skillLocator && base.inputBank)
                {
                    if (base.inputBank.skill4.down && base.fixedAge >= 0.4f * this.duration)
                    {
                        this.shieldCancel = true;
                        base.characterBody.isSprinting = false;
                        base.skillLocator.special.ExecuteIfReady();
                    }
                }

                if (!this.inHitPause)
                {
                    Vector3 normalized = (base.transform.position - this.previousPosition).normalized;

                    if (base.characterDirection)
                    {
                        if (normalized != Vector3.zero)
                        {
                            Vector3 vector = normalized * this.dashSpeed;
                            float d = Mathf.Max(Vector3.Dot(vector, this.forwardDirection), 0f);
                            vector = this.forwardDirection * d;
                            vector.y = base.characterMotor.velocity.y;
                            base.characterMotor.velocity = vector;
                        }

                        base.characterDirection.forward = this.forwardDirection;
                    }

                    this.previousPosition = base.transform.position;

                    this.attack.damage = this.damageStat * ShoulderBash.chargeDamageCoefficient;
                }
            }
        }





        public override InterruptPriority GetMinimumInterruptPriority()
        {
            if (this.shieldCancel) return InterruptPriority.Any;
            else return InterruptPriority.Frozen;
        }

        public override void OnSerialize(NetworkWriter writer)
        {
            base.OnSerialize(writer);
            writer.Write(this.forwardDirection);
        }

        public override void OnDeserialize(NetworkReader reader)
        {
            base.OnDeserialize(reader);
            this.forwardDirection = reader.ReadVector3();
        }
    }

    public class ChargeAgu : BaseState
    {

        public static float damageCoefficient = 10f;
        public float baseDuration = 1.5f;
        public static float attackRecoil = 0.5f;
        public static float hitHopVelocity = 5.5f;
        public static float earlyExitTime = 0.575f;
        public int swingIndex;


        private OverlapAttack attack;

        private float stopwatch;
        private bool cancelling;
        private Animator animator;
        private BaseState.HitStopCachedState hitStopCachedState;
        private ChildLocator childLocator;
        private Transform sphereCheckTransform;

        private static float chargeMovementSpeedCoefficient = 100f;
        private float overlapResetStopwatch = 1f;
        private static float overlapResetFrequency = 1f;
        private static float selfStunDuration = 1f;
        private HitBoxGroup hitboxGroup = null;

        private static float turnSmoothTime = 1f;
        private static float turnSpeed = 2f;
        private static float overlapSphereRadius = 0.1f;
        private static Vector3 selfStunForce;
        private static float chargeDuration = 0.5f;
        private Vector3 targetMoveVector;
        private Vector3 targetMoveVectorVelocity;

        //private PaladinSwordController swordController;

        // Token: 0x0600400F RID: 16399 RVA: 0x0010C464 File Offset: 0x0010A664
        public override void OnEnter()
        {
            base.OnEnter();
            this.animator = base.GetModelAnimator();
            this.childLocator = this.animator.GetComponent<ChildLocator>();


            base.PlayCrossfade("Body", "ChargeForward", 0.2f);
            this.ResetOverlapAttack();
            this.SetSprintEffectActive(true);
            if (this.childLocator)
            {
                this.sphereCheckTransform = this.childLocator.FindChild("SphereCheckTransform");
            }
            if (!this.sphereCheckTransform && base.characterBody)
            {
                this.sphereCheckTransform = base.characterBody.coreTransform;
            }
            if (!this.sphereCheckTransform)
            {
                this.sphereCheckTransform = base.transform;
            }
        }

        // Token: 0x06004010 RID: 16400 RVA: 0x0010C559 File Offset: 0x0010A759
        private void SetSprintEffectActive(bool active)
        {

        }

        // Token: 0x06004011 RID: 16401 RVA: 0x0010C588 File Offset: 0x0010A788
        public override void OnExit()
        {
            base.OnExit();
            base.characterMotor.moveDirection = Vector3.zero;

            Util.PlaySound("stop_bison_charge_attack_loop", base.gameObject);
            this.SetSprintEffectActive(false);
            FootstepHandler component = this.animator.GetComponent<FootstepHandler>();
            if (component)
            {

            }
        }

        // Token: 0x06004012 RID: 16402 RVA: 0x0010C5F4 File Offset: 0x0010A7F4
        public override void FixedUpdate()
        {
            this.targetMoveVector = Vector3.ProjectOnPlane(Vector3.SmoothDamp(this.targetMoveVector, base.inputBank.aimDirection, ref this.targetMoveVectorVelocity, ChargeAgu.turnSmoothTime, ChargeAgu.turnSpeed), Vector3.up).normalized;
            base.characterDirection.moveVector = this.targetMoveVector;
            Vector3 forward = base.characterDirection.forward;
            float value = this.moveSpeedStat * ChargeAgu.chargeMovementSpeedCoefficient;
            base.characterMotor.moveDirection = forward * ChargeAgu.chargeMovementSpeedCoefficient;
            this.animator.SetFloat("forwardSpeed", value);
            if (base.isAuthority && this.attack.Fire(null))
            {

            }
            if (this.overlapResetStopwatch >= 1f / ChargeAgu.overlapResetFrequency)
            {
                this.overlapResetStopwatch -= 1f / ChargeAgu.overlapResetFrequency;
            }
            if (base.isAuthority && Physics.OverlapSphere(this.sphereCheckTransform.position, ChargeAgu.overlapSphereRadius, LayerIndex.world.mask).Length != 0)
            {

                base.healthComponent.TakeDamageForce(forward * 1f, true, false);
                StunState stunState = new StunState();
                stunState.stunDuration = ChargeAgu.selfStunDuration;
                this.outer.SetNextState(stunState);
                return;
            }
            this.stopwatch += Time.fixedDeltaTime;
            this.overlapResetStopwatch += Time.fixedDeltaTime;
            if (this.stopwatch > ChargeAgu.chargeDuration)
            {
                this.outer.SetNextStateToMain();
            }
            base.FixedUpdate();
        }

        // Token: 0x06004013 RID: 16403 RVA: 0x0010C7BC File Offset: 0x0010A9BC
        private void ResetOverlapAttack()
        {

            if (!this.hitboxGroup)
            {
                Transform modelTransform = base.GetModelTransform();
                if (modelTransform)
                {
                    this.hitboxGroup = Array.Find<HitBoxGroup>(modelTransform.GetComponents<HitBoxGroup>(), (HitBoxGroup element) => element.groupName == "HeadCh");
                }
            }
            this.attack = new OverlapAttack();
            this.attack.attacker = base.gameObject;
            this.attack.inflictor = base.gameObject;
            this.attack.teamIndex = TeamComponent.GetObjectTeam(this.attack.attacker);
            this.attack.damage = ChargeAgu.damageCoefficient * this.damageStat;

            this.attack.forceVector = Vector3.up * 1f;
            this.attack.pushAwayForce = 1f;
            this.attack.hitBoxGroup = this.hitboxGroup;
        }

        // Token: 0x06004014 RID: 16404 RVA: 0x0000CFF7 File Offset: 0x0000B1F7
        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Skill;
        }
    }
}
