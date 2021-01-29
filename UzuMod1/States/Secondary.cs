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
                this.outer.SetNextState(new ShoulderBash());
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
            hitBoxGroup = Array.Find<HitBoxGroup>(modelTransform.GetComponents<HitBoxGroup>(), (HitBoxGroup element) => element.groupName == "HeadH");

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

    public class ShoulderBashImpact : BaseState
    {
        public HealthComponent victimHealthComponent;
        public Vector3 idealDirection;
        public bool isCrit;

        public static float baseDuration = 0.35f;
        public static float recoilAmplitude = 4.5f;

        private float duration;
        private bool shieldCancel;

        public override void OnEnter()
        {
            base.OnEnter();
            this.duration = ShoulderBashImpact.baseDuration / this.attackSpeedStat;
            //if (!base.HasBuff(EnforcerPlugin.EnforcerPlugin.skateboardBuff)) base.PlayAnimation("FullBody, Override", "BashRecoil");
            base.SmallHop(base.characterMotor, ShoulderBash.smallHopVelocity);

            //Util.PlayScaledSound(EnforcerPlugin.Sounds.ShoulderBashHit, base.gameObject, 0.5f);

            if (NetworkServer.active)
            {
                if (this.victimHealthComponent)
                {
                    DamageInfo damageInfo = new DamageInfo
                    {
                        attacker = base.gameObject,
                        damage = this.damageStat * ShoulderBash.knockbackDamageCoefficient,
                        crit = this.isCrit,
                        procCoefficient = 1f,
                        damageColorIndex = DamageColorIndex.Item,
                        damageType = DamageType.Stun1s,
                        position = base.characterBody.corePosition
                    };

                    this.victimHealthComponent.TakeDamage(damageInfo);
                    GlobalEventManager.instance.OnHitEnemy(damageInfo, this.victimHealthComponent.gameObject);
                    GlobalEventManager.instance.OnHitAll(damageInfo, this.victimHealthComponent.gameObject);
                }
            }

            if (base.characterMotor) base.characterMotor.velocity = this.idealDirection * -ShoulderBash.knockbackForce;

            if (base.isAuthority)
            {
                base.AddRecoil(-0.5f * ShoulderBashImpact.recoilAmplitude * 3f, -0.5f * ShoulderBashImpact.recoilAmplitude * 3f, -0.5f * ShoulderBashImpact.recoilAmplitude * 8f, 0.5f * ShoulderBashImpact.recoilAmplitude * 3f);
                EffectManager.SimpleImpactEffect(Loader.SwingZapFist.overchargeImpactEffectPrefab, this.victimHealthComponent.transform.position, base.characterDirection.forward, true);
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (base.inputBank && base.isAuthority)
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
            }

            if (base.fixedAge >= this.duration)
            {
                this.outer.SetNextStateToMain();
            }
        }

        public override void OnSerialize(NetworkWriter writer)
        {
            base.OnSerialize(writer);
            writer.Write(this.victimHealthComponent ? this.victimHealthComponent.gameObject : null);
            writer.Write(this.idealDirection);
            writer.Write(this.isCrit);
        }

        public override void OnDeserialize(NetworkReader reader)
        {
            base.OnDeserialize(reader);
            GameObject gameObject = reader.ReadGameObject();
            this.victimHealthComponent = (gameObject ? gameObject.GetComponent<HealthComponent>() : null);
            this.idealDirection = reader.ReadVector3();
            this.isCrit = reader.ReadBoolean();
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            if (this.shieldCancel) return InterruptPriority.Any;
            else return InterruptPriority.Frozen;
        }
    }
}
