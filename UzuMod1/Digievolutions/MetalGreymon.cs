using System;
using System.Reflection;
using System.Collections.Generic;
using BepInEx;
using R2API;
using R2API.Utils;
using EntityStates;
using EntityStates.ExampleSurvivorStates;
using RoR2;
using RoR2.Skills;
using RoR2.Projectile;
using UnityEngine;
using UnityEngine.Networking;
using KinematicCharacterController;
using System.Collections;
using ExampleSurvivor.Digievolutions.EntityStates.ExampleSurvivorStates;

namespace ExampleSurvivor.Digievolutions
{

    public class MetalGreymon : MonoBehaviour
    {

        public static GameObject characterPrefabMetalGreymon;
        public GameObject characterDisplay; // the prefab used for character select
        public static GameObject GreymonMetalBlast;
        public static GameObject GigaDestroyer;
        public static GameObject ClawHook;

        public static void Init()
        {
            //Assets.PopulateAssets(); // first we load the assets from our assetbundle
            //CreatePrefab(); // then we create our character's body prefab
            CreatePrefabMetalGreymon();
            RegisterStates(); // register our skill entitystates for networking
            RegisterCharacter(); // and finally put our new survivor in the game
                                 //CreateDoppelganger(); // not really mandatory, but it's simple and not having an umbra is just kinda lame

        }

        private static GameObject CreateModel1(GameObject main)
        {
            Destroy(main.transform.Find("ModelBase").gameObject);
            Destroy(main.transform.Find("CameraPivot").gameObject);
            Destroy(main.transform.Find("AimOrigin").gameObject);

            // make sure it's set up right in the unity project
            GameObject model = Assets.MainAssetBundle.LoadAsset<GameObject>("mdlMetalGreymon");

            return model;
        }
        internal static void CreatePrefabMetalGreymon()
        {
            // first clone the commando prefab so we can turn that into our own survivor
            characterPrefabMetalGreymon = PrefabAPI.InstantiateClone(Resources.Load<GameObject>("Prefabs/CharacterBodies/CommandoBody"), "ExampleSurvivorBody", true, "C:\\Users\\test\\Documents\\ror2mods\\ExampleSurvivor\\ExampleSurvivor\\ExampleSurvivor\\ExampleSurvivor.cs", "CreatePrefab", 151);

            characterPrefabMetalGreymon.GetComponent<NetworkIdentity>().localPlayerAuthority = true;

            // create the model here, we're gonna replace commando's model with our own
            GameObject model = CreateModel1(characterPrefabMetalGreymon);

            GameObject gameObject = new GameObject("ModelBase");
            gameObject.transform.parent = characterPrefabMetalGreymon.transform;
            gameObject.transform.localPosition = new Vector3(0f, -0.81f, 0f);
            gameObject.transform.localRotation = Quaternion.identity;
            gameObject.transform.localScale = new Vector3(1f, 1f, 1f);

            GameObject gameObject2 = new GameObject("CameraPivot");
            gameObject2.transform.parent = gameObject.transform;
            gameObject2.transform.localPosition = new Vector3(0f, 1.6f, 0f);
            gameObject2.transform.localRotation = Quaternion.identity;
            gameObject2.transform.localScale = Vector3.one;

            GameObject gameObject3 = new GameObject("AimOrigin");
            gameObject3.transform.parent = gameObject.transform;
            gameObject3.transform.localPosition = new Vector3(0f, 1.4f, 0f);
            gameObject3.transform.localRotation = Quaternion.identity;
            gameObject3.transform.localScale = Vector3.one;

            Transform transform = model.transform;
            transform.parent = gameObject.transform;
            transform.localPosition = Vector3.zero;
            transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
            transform.localRotation = Quaternion.identity;

            CharacterDirection characterDirection = characterPrefabMetalGreymon.GetComponent<CharacterDirection>();
            characterDirection.moveVector = Vector3.zero;
            characterDirection.targetTransform = gameObject.transform;
            characterDirection.overrideAnimatorForwardTransform = null;
            characterDirection.rootMotionAccumulator = null;
            characterDirection.modelAnimator = model.GetComponentInChildren<Animator>();
            characterDirection.driveFromRootRotation = false;
            characterDirection.turnSpeed = 720f;

            // set up the character body here
            CharacterBody bodyComponent = characterPrefabMetalGreymon.GetComponent<CharacterBody>();
            bodyComponent.bodyIndex = -1;
            bodyComponent.baseNameToken = "EXAMPLESURVIVOR_NAME"; // name token
            bodyComponent.subtitleNameToken = "EXAMPLESURVIVOR_SUBTITLE"; // subtitle token- used for umbras
            bodyComponent.bodyFlags = CharacterBody.BodyFlags.ImmuneToExecutes;
            bodyComponent.rootMotionInMainState = false;
            bodyComponent.mainRootSpeed = 0;
            bodyComponent.baseMaxHealth = 1000;
            bodyComponent.levelMaxHealth = 100; //24
            bodyComponent.baseRegen = 1f;//0.5
            bodyComponent.levelRegen = 0.6f;//25
            bodyComponent.baseMaxShield = 0;
            bodyComponent.levelMaxShield = 0;
            bodyComponent.baseMoveSpeed = 5;
            bodyComponent.levelMoveSpeed = 0;
            bodyComponent.baseAcceleration = 40;
            bodyComponent.baseJumpPower = 30; //15
            bodyComponent.levelJumpPower = 0;
            bodyComponent.baseDamage = 15;
            bodyComponent.levelDamage = 3f;
            bodyComponent.baseAttackSpeed = 1;
            bodyComponent.levelAttackSpeed = 0;
            bodyComponent.baseCrit = 1;
            bodyComponent.levelCrit = 0;
            bodyComponent.baseArmor = 0;
            bodyComponent.levelArmor = 0;
            bodyComponent.baseJumpCount = 1;
            bodyComponent.sprintingSpeedMultiplier = 1.45f;
            bodyComponent.wasLucky = false;
            bodyComponent.hideCrosshair = false;
            bodyComponent.aimOriginTransform = gameObject3.transform;
            bodyComponent.hullClassification = HullClassification.Human;
            bodyComponent.portraitIcon = Assets.charPortrait;
            bodyComponent.isChampion = false;
            bodyComponent.currentVehicle = null;
            bodyComponent.skinIndex = 0U;

            // the charactermotor controls the survivor's movement and stuff
            CharacterMotor characterMotor = characterPrefabMetalGreymon.GetComponent<CharacterMotor>();
            characterMotor.walkSpeedPenaltyCoefficient = 1f;
            characterMotor.characterDirection = characterDirection;
            characterMotor.muteWalkMotion = false;
            characterMotor.mass = 100f;
            characterMotor.airControl = 0.25f;
            characterMotor.disableAirControlUntilCollision = false;
            characterMotor.generateParametersOnAwake = true;
            //characterMotor.useGravity = true;
            //characterMotor.isFlying = false;

            InputBankTest inputBankTest = characterPrefabMetalGreymon.GetComponent<InputBankTest>();
            inputBankTest.moveVector = Vector3.zero;

            CameraTargetParams cameraTargetParams = characterPrefabMetalGreymon.GetComponent<CameraTargetParams>();
            cameraTargetParams.cameraParams = Resources.Load<GameObject>("Prefabs/CharacterBodies/CommandoBody").GetComponent<CameraTargetParams>().cameraParams;
            cameraTargetParams.cameraPivotTransform = null;
            cameraTargetParams.aimMode = CameraTargetParams.AimType.Standard;
            cameraTargetParams.recoil = Vector2.zero;
            cameraTargetParams.idealLocalCameraPos = Vector3.zero;
            cameraTargetParams.dontRaycastToPivot = false;

            // this component is used to locate the character model(duh), important to set this up here
            ModelLocator modelLocator = characterPrefabMetalGreymon.GetComponent<ModelLocator>();
            modelLocator.modelTransform = transform;
            modelLocator.modelBaseTransform = gameObject.transform;
            modelLocator.dontReleaseModelOnDeath = false;
            modelLocator.autoUpdateModelTransform = true;
            modelLocator.dontDetatchFromParent = false;
            modelLocator.noCorpse = false;
            modelLocator.normalizeToFloor = false; // set true if you want your character to rotate on terrain like acrid does
            modelLocator.preserveModel = false;

            // childlocator is something that must be set up in the unity project, it's used to find any child objects for things like footsteps or muzzle flashes
            // also important to set up if you want quality
            ChildLocator childLocator = model.GetComponent<ChildLocator>();

            // this component is used to handle all overlays and whatever on your character, without setting this up you won't get any cool effects like burning or freeze on the character
            // it goes on the model object of course
            CharacterModel characterModel = model.AddComponent<CharacterModel>();
            characterModel.body = bodyComponent;
            characterModel.baseRendererInfos = new CharacterModel.RendererInfo[]
            {
                // set up multiple rendererinfos if needed, but for this example there's only the one
                new CharacterModel.RendererInfo
                {
                    defaultMaterial = model.GetComponentInChildren<SkinnedMeshRenderer>().material,
                    renderer = model.GetComponentInChildren<SkinnedMeshRenderer>(),
                    defaultShadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On,
                    ignoreOverlays = false
                }
            };

            characterModel.autoPopulateLightInfos = true;
            characterModel.invisibilityCount = 0;
            characterModel.temporaryOverlays = new List<TemporaryOverlay>();

            TeamComponent teamComponent = null;
            if (characterPrefabMetalGreymon.GetComponent<TeamComponent>() != null) teamComponent = characterPrefabMetalGreymon.GetComponent<TeamComponent>();
            else teamComponent = characterPrefabMetalGreymon.GetComponent<TeamComponent>();
            teamComponent.hideAllyCardDisplay = false;
            teamComponent.teamIndex = TeamIndex.None;

            HealthComponent healthComponent = characterPrefabMetalGreymon.GetComponent<HealthComponent>();
            healthComponent.health = 90f;
            healthComponent.shield = 0f;
            healthComponent.barrier = 0f;
            healthComponent.magnetiCharge = 0f;
            healthComponent.body = null;
            healthComponent.dontShowHealthbar = false;
            healthComponent.globalDeathEventChanceCoefficient = 1f;

            characterPrefabMetalGreymon.GetComponent<Interactor>().maxInteractionDistance = 3f;
            characterPrefabMetalGreymon.GetComponent<InteractionDriver>().highlightInteractor = true;

            // this disables ragdoll since the character's not set up for it, and instead plays a death animation
            CharacterDeathBehavior characterDeathBehavior = characterPrefabMetalGreymon.GetComponent<CharacterDeathBehavior>();
            characterDeathBehavior.deathStateMachine = characterPrefabMetalGreymon.GetComponent<EntityStateMachine>();
            characterDeathBehavior.deathState = new SerializableEntityStateType(typeof(GenericCharacterDeath));

            // edit the sfxlocator if you want different sounds
            SfxLocator sfxLocator = characterPrefabMetalGreymon.GetComponent<SfxLocator>();
            sfxLocator.deathSound = "Play_ui_player_death";
            sfxLocator.barkSound = "";
            sfxLocator.openSound = "";
            sfxLocator.landingSound = "Play_char_land";
            sfxLocator.fallDamageSound = "Play_char_land_fall_damage";
            sfxLocator.aliveLoopStart = "";
            sfxLocator.aliveLoopStop = "";

            Rigidbody rigidbody = characterPrefabMetalGreymon.GetComponent<Rigidbody>();
            rigidbody.mass = 100f;
            rigidbody.drag = 0f;
            rigidbody.angularDrag = 0f;
            rigidbody.useGravity = false;
            rigidbody.isKinematic = true;
            rigidbody.interpolation = RigidbodyInterpolation.None;
            rigidbody.collisionDetectionMode = CollisionDetectionMode.Discrete;
            rigidbody.constraints = RigidbodyConstraints.None;

            CapsuleCollider capsuleCollider = characterPrefabMetalGreymon.GetComponent<CapsuleCollider>();
            capsuleCollider.isTrigger = false;
            capsuleCollider.material = null;
            capsuleCollider.center = new Vector3(0f, 0f, 0f);
            capsuleCollider.radius = 0.5f;
            capsuleCollider.height = 1.82f;
            capsuleCollider.direction = 1;

            KinematicCharacterMotor kinematicCharacterMotor = characterPrefabMetalGreymon.GetComponent<KinematicCharacterMotor>();
            kinematicCharacterMotor.CharacterController = characterMotor;
            kinematicCharacterMotor.Capsule = capsuleCollider;
            kinematicCharacterMotor.Rigidbody = rigidbody;

            capsuleCollider.radius = 0.5f;
            capsuleCollider.height = 1.82f;
            capsuleCollider.center = new Vector3(0, 0, 0);
            capsuleCollider.material = null;

            kinematicCharacterMotor.DetectDiscreteCollisions = false;
            kinematicCharacterMotor.GroundDetectionExtraDistance = 0f;
            kinematicCharacterMotor.MaxStepHeight = 0.2f;
            kinematicCharacterMotor.MinRequiredStepDepth = 0.1f;
            kinematicCharacterMotor.MaxStableSlopeAngle = 55f;
            kinematicCharacterMotor.MaxStableDistanceFromLedge = 0.5f;
            kinematicCharacterMotor.PreventSnappingOnLedges = false;
            kinematicCharacterMotor.MaxStableDenivelationAngle = 55f;
            kinematicCharacterMotor.RigidbodyInteractionType = RigidbodyInteractionType.None;
            kinematicCharacterMotor.PreserveAttachedRigidbodyMomentum = true;
            kinematicCharacterMotor.HasPlanarConstraint = false;
            kinematicCharacterMotor.PlanarConstraintAxis = Vector3.up;
            kinematicCharacterMotor.StepHandling = StepHandlingMethod.None;
            kinematicCharacterMotor.LedgeHandling = true;
            kinematicCharacterMotor.InteractiveRigidbodyHandling = true;
            kinematicCharacterMotor.SafeMovement = false;

            // this sets up the character's hurtbox, kinda confusing, but should be fine as long as it's set up in unity right
            HurtBoxGroup hurtBoxGroup = model.AddComponent<HurtBoxGroup>();

            HurtBox componentInChildren = model.GetComponentInChildren<CapsuleCollider>().gameObject.AddComponent<HurtBox>();
            componentInChildren.gameObject.layer = LayerIndex.entityPrecise.intVal;
            componentInChildren.healthComponent = healthComponent;
            componentInChildren.isBullseye = true;
            componentInChildren.damageModifier = HurtBox.DamageModifier.Normal;
            componentInChildren.hurtBoxGroup = hurtBoxGroup;
            componentInChildren.indexInGroup = 0;

            hurtBoxGroup.hurtBoxes = new HurtBox[]
            {
                componentInChildren
            };

            hurtBoxGroup.mainHurtBox = componentInChildren;
            hurtBoxGroup.bullseyeCount = 1;

            Modules.Helpers.CreateHitbox(model, childLocator.FindChild("HeadM"), "HeadM");
            Modules.Helpers.CreateHitbox(model, childLocator.FindChild("HeadMCh"), "HeadMCh");

            // this is for handling footsteps, not needed but polish is always good
            FootstepHandler footstepHandler = model.AddComponent<FootstepHandler>();
            footstepHandler.baseFootstepString = "Play_player_footstep";
            footstepHandler.sprintFootstepOverrideString = "";
            footstepHandler.enableFootstepDust = true;
            footstepHandler.footstepDustPrefab = Resources.Load<GameObject>("Prefabs/GenericFootstepDust");

            // ragdoll controller is a pain to set up so we won't be doing that here..
            RagdollController ragdollController = model.AddComponent<RagdollController>();
            ragdollController.bones = null;
            ragdollController.componentsToDisableOnRagdoll = null;

            // this handles the pitch and yaw animations, but honestly they are nasty and a huge pain to set up so i didn't bother
            AimAnimator aimAnimator = model.AddComponent<AimAnimator>();
            aimAnimator.inputBank = inputBankTest;
            aimAnimator.directionComponent = characterDirection;
            aimAnimator.pitchRangeMax = 55f;
            aimAnimator.pitchRangeMin = -50f;
            aimAnimator.yawRangeMin = -44f;
            aimAnimator.yawRangeMax = 44f;
            aimAnimator.pitchGiveupRange = 30f;
            aimAnimator.yawGiveupRange = 10f;
            aimAnimator.giveupDuration = 8f;


        }
        public static void RegisterStates()
        {
            // register the entitystates for networking reasons
            LoadoutAPI.AddSkill(typeof(ExampleSurvivorFireArrow));
            LoadoutAPI.AddSkill(typeof(ExampleSurvivorFireArrow2));
        }
        public static void RegisterCharacter()// Apparently i won't need it
        {
            // now that the body prefab's set up, clone it here to make the display prefab ???
            //characterDisplay = PrefabAPI.InstantiateClone(characterPrefabGreymon.GetComponent<ModelLocator>().modelBaseTransform.gameObject, "ExampleSurvivorDisplay", true, "C:\\Users\\test\\Documents\\ror2mods\\ExampleSurvivor\\ExampleSurvivor\\ExampleSurvivor\\ExampleSurvivor.cs", "RegisterCharacter", 153);
            //characterDisplay.AddComponent<NetworkIdentity>();

            // clone rex's syringe projectile prefab here to use as our own projectile ///cambiado a bola de fuego
            GreymonMetalBlast = PrefabAPI.InstantiateClone(Resources.Load<GameObject>("Prefabs/Projectiles/LunarWispTrackingBomb"), "Prefabs/Projectiles/Greymonblast", true, "C:\\Users\\test\\Documents\\ror2mods\\ExampleSurvivor\\ExampleSurvivor\\ExampleSurvivor\\ExampleSurvivor.cs", "RegisterCharacter", 155);
            
            GigaDestroyer = PrefabAPI.InstantiateClone(Resources.Load<GameObject>("Prefabs/Projectiles/LunarGolemTwinShotProjectile"), "Prefabs/Projectiles/ExampleArrowProjectile", true, "C:\\Users\\test\\Documents\\ror2mods\\ExampleSurvivor\\ExampleSurvivor\\ExampleSurvivor\\ExampleSurvivor.cs", "RegisterCharacter", 155);

            ClawHook = PrefabAPI.InstantiateClone(Resources.Load<GameObject>("Prefabs/Projectiles/LoaderYankHook"), "Prefabs/Projectiles/ClawHook", true, "C:\\Users\\test\\Documents\\ror2mods\\ExampleSurvivor\\ExampleSurvivor\\ExampleSurvivor\\ExampleSurvivor.cs", "RegisterCharacter", 155);

            //arrowProjectile3 = PrefabAPI.InstantiateClone(Resources.Load<GameObject>("Prefabs/Projectiles/MageFireboltExpanded"), "Prefabs/Projectiles/ExampleArrowProjectile2", true, "C:\\Users\\test\\Documents\\ror2mods\\ExampleSurvivor\\ExampleSurvivor\\ExampleSurvivor\\ExampleSurvivor.cs", "RegisterCharacter", 155);

            //slashattack = PrefabAPI.InstantiateClone(Resources.Load<GameObject>("Prefabs/effects/LemurianSlash"), "Prefabs/Projectiles/Slashattack", true, "C:\\Users\\test\\Documents\\ror2mods\\ExampleSurvivor\\ExampleSurvivor\\ExampleSurvivor\\ExampleSurvivor.cs", "RegisterCharacter", 155);

            // just setting the numbers to 1 as the entitystate will take care of those
            GreymonMetalBlast.GetComponent<ProjectileController>().procCoefficient = 1f;
            GreymonMetalBlast.GetComponent<ProjectileDamage>().damage = 1f;
            GreymonMetalBlast.GetComponent<ProjectileDamage>().damageType = DamageType.PercentIgniteOnHit;

            GigaDestroyer.GetComponent<ProjectileController>().procCoefficient = 1f;
            GigaDestroyer.GetComponent<ProjectileDamage>().damage = 10f;
            GigaDestroyer.GetComponent<ProjectileDamage>().damageType = DamageType.IgniteOnHit;

            ClawHook.GetComponent<ProjectileController>().procCoefficient = 1f;
            ClawHook.GetComponent<ProjectileDamage>().damage = 10f;
            ClawHook.GetComponent<ProjectileDamage>().damageType = DamageType.IgniteOnHit;


            //slashattack.GetComponent<ProjectileController>().procCoefficient = 1f;
            //slashattack.GetComponent<ProjectileDamage>().damage = 1f;
            //slashattack.GetComponent<ProjectileDamage>().damageType = DamageType.Generic;

            // register it for networking
            if (GreymonMetalBlast) PrefabAPI.RegisterNetworkPrefab(GreymonMetalBlast);
            if (GigaDestroyer) PrefabAPI.RegisterNetworkPrefab(GigaDestroyer);
            if (ClawHook) PrefabAPI.RegisterNetworkPrefab(ClawHook);

            // add it to the projectile catalog or it won't work in multiplayer
            ProjectileCatalog.getAdditionalEntries += list =>
            {
                list.Add(GreymonMetalBlast);
                list.Add(GigaDestroyer); 
                list.Add(ClawHook);

            };



            // write a clean survivor description here!
            string desc = "Greymon comes from Digiword.<color=#CCD3E0>" + Environment.NewLine + Environment.NewLine;
            desc = desc + "< ! > Sample text 1." + Environment.NewLine + Environment.NewLine;
            desc = desc + "< ! > Sample text 2." + Environment.NewLine + Environment.NewLine;
            desc = desc + "< ! > Sample Text 3." + Environment.NewLine + Environment.NewLine;
            desc = desc + "< ! > Sample Text 4.</color>" + Environment.NewLine + Environment.NewLine;

            // add the language tokens
            //LanguageAPI.Add("EXAMPLESURVIVOR_NAME", "Greymon");
            //LanguageAPI.Add("EXAMPLESURVIVOR_DESCRIPTION", desc);
            //LanguageAPI.Add("EXAMPLESURVIVOR_SUBTITLE", "Greymon");

            // add our new survivor to the game~
            //SurvivorDef survivorDefGreymon = new SurvivorDef
            //{
            //    name = "EXAMPLESURVIVOR_NAME",
            //    unlockableName = "",
            //    descriptionToken = "EXAMPLESURVIVOR_DESCRIPTION",
            //    //primaryColor = characterColor,
            //    bodyPrefab = characterPrefabGreymon,
            //    //displayPrefab = characterDisplay
            //};


            //SurvivorAPI.AddSurvivor(survivorDefGreymon);

            // set up the survivor's skills here
            SkillSetup();

            // gotta add it to the body catalog too
            //BodyCatalog.getAdditionalEntries += delegate (List<GameObject> list)
            //{
            //    list.Add(characterPrefabGreymon);
            //};
        }

        static void SkillSetup()
        {
            // get rid of the original skills first, otherwise we'll have commando's loadout and we don't want that
            foreach (GenericSkill obj in characterPrefabMetalGreymon.GetComponentsInChildren<GenericSkill>())
            {
                BaseUnityPlugin.DestroyImmediate(obj);
            }

            PassiveSetup();
            PrimarySetup();
            SecondarySetup();
            UtilitySetup();
            SpecialSetup();
        }

        static void PassiveSetup()
        {
            // set up the passive skill here if you want
            SkillLocator component = characterPrefabMetalGreymon.GetComponent<SkillLocator>();

            LanguageAPI.Add("EXAMPLESURVIVOR_PASSIVE_NAME", "Passive");
            LanguageAPI.Add("EXAMPLESURVIVOR_PASSIVE_DESCRIPTION", "<style=cIsUtility>Doot</style> <style=cIsHealing>doot</style>.");

            component.passiveSkill.enabled = true;
            component.passiveSkill.skillNameToken = "EXAMPLESURVIVOR_PASSIVE_NAME";
            component.passiveSkill.skillDescriptionToken = "EXAMPLESURVIVOR_PASSIVE_DESCRIPTION";
            component.passiveSkill.icon = Assets.iconP;
        }

        static void PrimarySetup()
        {
            SkillLocator component = characterPrefabMetalGreymon.GetComponent<SkillLocator>();

            LanguageAPI.Add("EXAMPLESURVIVOR_PRIMARY_CROSSBOW_NAME", "Fire");
            LanguageAPI.Add("EXAMPLESURVIVOR_PRIMARY_CROSSBOW_DESCRIPTION", "Fire up to <style=cIsDamage>3</style> flames, dealing <style=cIsDamage>200% damage</style> each.");

            // set up your primary skill def here!

            SkillDef mySkillDef = ScriptableObject.CreateInstance<SkillDef>();
            mySkillDef.activationState = new SerializableEntityStateType(typeof(GreymonBlaster));
            mySkillDef.activationStateMachineName = "Weapon";
            mySkillDef.baseMaxStock = 1;
            mySkillDef.baseRechargeInterval = 0f;
            mySkillDef.beginSkillCooldownOnSkillEnd = false;
            mySkillDef.canceledFromSprinting = false;
            mySkillDef.fullRestockOnAssign = true;
            mySkillDef.interruptPriority = InterruptPriority.Any;
            mySkillDef.isBullets = false;
            mySkillDef.isCombatSkill = true;
            mySkillDef.mustKeyPress = false;
            mySkillDef.noSprint = false;
            mySkillDef.rechargeStock = 1;
            mySkillDef.requiredStock = 1;
            mySkillDef.shootDelay = 2f;
            mySkillDef.stockToConsume = 1;
            mySkillDef.icon = Assets.icon1;
            mySkillDef.skillDescriptionToken = "EXAMPLESURVIVOR_PRIMARY_CROSSBOW_DESCRIPTION";
            mySkillDef.skillName = "EXAMPLESURVIVOR_PRIMARY_CROSSBOW_NAME";
            mySkillDef.skillNameToken = "EXAMPLESURVIVOR_PRIMARY_CROSSBOW_NAME";

            LoadoutAPI.AddSkillDef(mySkillDef);

            component.primary = characterPrefabMetalGreymon.AddComponent<GenericSkill>();
            SkillFamily newFamily = ScriptableObject.CreateInstance<SkillFamily>();
            newFamily.variants = new SkillFamily.Variant[1];
            LoadoutAPI.AddSkillFamily(newFamily);
            component.primary.SetFieldValue("_skillFamily", newFamily);
            SkillFamily skillFamily = component.primary.skillFamily;

            skillFamily.variants[0] = new SkillFamily.Variant
            {
                skillDef = mySkillDef,
                unlockableName = "",
                viewableNode = new ViewablesCatalog.Node(mySkillDef.skillNameToken, false, null)
            };


            // add this code after defining a new skilldef if you're adding an alternate skill

            /*Array.Resize(ref skillFamily.variants, skillFamily.variants.Length + 1);
            skillFamily.variants[skillFamily.variants.Length - 1] = new SkillFamily.Variant
            {
                skillDef = newSkillDef,
                unlockableName = "",
                viewableNode = new ViewablesCatalog.Node(newSkillDef.skillNameToken, false, null)
            };*/
            //////////////////7
            //LoadoutAPI.AddSkill(typeof(Slash));

            //mySkillDef = ScriptableObject.CreateInstance<SkillDef>();
            //mySkillDef.activationState = new SerializableEntityStateType(typeof(Slash));
            //mySkillDef.activationStateMachineName = "Weapon";
            //mySkillDef.baseMaxStock = 1;
            //mySkillDef.baseRechargeInterval = 0f;
            //mySkillDef.beginSkillCooldownOnSkillEnd = false;
            //mySkillDef.canceledFromSprinting = false;
            //mySkillDef.fullRestockOnAssign = true;
            //mySkillDef.interruptPriority = InterruptPriority.Any;
            //mySkillDef.isBullets = false;
            //mySkillDef.isCombatSkill = true;
            //mySkillDef.mustKeyPress = false;
            //mySkillDef.noSprint = true;
            //mySkillDef.rechargeStock = 1;
            //mySkillDef.requiredStock = 1;
            //mySkillDef.shootDelay = 0.5f;
            //mySkillDef.stockToConsume = 1;
            //mySkillDef.icon = Assets.icon1;
            //mySkillDef.skillDescriptionToken = "EXAMPLESURVIVOR_PRIMARY_CROSSBOW_DESCRIPTION";
            //mySkillDef.skillName = "EXAMPLESURVIVOR_PRIMARY_CROSSBOW_NAME";
            //mySkillDef.skillNameToken = "EXAMPLESURVIVOR_PRIMARY_CROSSBOW_NAME";

            //LoadoutAPI.AddSkillDef(mySkillDef);

            //component.primary = characterPrefab.AddComponent<GenericSkill>();
            // newFamily = ScriptableObject.CreateInstance<SkillFamily>();
            //newFamily.variants = new SkillFamily.Variant[1];
            //LoadoutAPI.AddSkillFamily(newFamily);
            //component.primary.SetFieldValue("_skillFamily", newFamily);
            // skillFamily = component.primary.skillFamily;


            ////////////////////
            //Array.Resize(ref skillFamily.variants, skillFamily.variants.Length + 1);
            //skillFamily.variants[skillFamily.variants.Length - 1] = new SkillFamily.Variant
            //{
            //    skillDef = mySkillDef,
            //    unlockableName = "",
            //    viewableNode = new ViewablesCatalog.Node(mySkillDef.skillNameToken, false, null)
            //};


        }

        static private void SecondarySetup()
        {
            SkillLocator component = characterPrefabMetalGreymon.GetComponent<SkillLocator>();

            LanguageAPI.Add("EXAMPLESURVIVOR_SECONDARY_CROSSBOW_NAME", "Mega Flame");
            LanguageAPI.Add("EXAMPLESURVIVOR_SECONDARY_CROSSBOW_DESCRIPTION", "Fire a big BIG flame, dealing <style=cIsDamage>1500% damage</style>.");



            SkillDef mySkillDef = ScriptableObject.CreateInstance<SkillDef>();
            mySkillDef.activationState = new SerializableEntityStateType(typeof(GigaDestroyerFire));
            mySkillDef.activationStateMachineName = "Weapon";
            mySkillDef.baseMaxStock = 1;
            mySkillDef.baseRechargeInterval = 5f;
            mySkillDef.beginSkillCooldownOnSkillEnd = false;
            mySkillDef.canceledFromSprinting = false;
            mySkillDef.fullRestockOnAssign = true;
            mySkillDef.interruptPriority = InterruptPriority.Any;
            mySkillDef.isBullets = false;
            mySkillDef.isCombatSkill = true;
            mySkillDef.mustKeyPress = false;
            mySkillDef.noSprint = false;
            mySkillDef.rechargeStock = 1;
            mySkillDef.requiredStock = 1;
            mySkillDef.shootDelay = 0f;
            mySkillDef.stockToConsume = 1;
            mySkillDef.icon = Assets.babyflame;
            mySkillDef.skillDescriptionToken = "EXAMPLESURVIVOR_SECONDARY_CROSSBOW_DESCRIPTION";
            mySkillDef.skillName = "EXAMPLESURVIVOR_SECONDARY_CROSSBOW_NAME";
            mySkillDef.skillNameToken = "EXAMPLESURVIVOR_SECONDARY_CROSSBOW_NAME";

            LoadoutAPI.AddSkillDef(mySkillDef);

            component.secondary = characterPrefabMetalGreymon.AddComponent<GenericSkill>();
            SkillFamily newFamily = ScriptableObject.CreateInstance<SkillFamily>();
            newFamily.variants = new SkillFamily.Variant[1];
            LoadoutAPI.AddSkillFamily(newFamily);
            component.secondary.SetFieldValue("_skillFamily", newFamily);
            SkillFamily skillFamily = component.secondary.skillFamily;

            skillFamily.variants[0] = new SkillFamily.Variant
            {
                skillDef = mySkillDef,
                unlockableName = "",
                viewableNode = new ViewablesCatalog.Node(mySkillDef.skillNameToken, false, null)
            };

        }

        static void UtilitySetup()
        {
            SkillLocator component = characterPrefabMetalGreymon.GetComponent<SkillLocator>();

            LanguageAPI.Add("EXAMPLESURVIVOR_PRIMARY_CROSSBOW3_NAME", "Dash");
            LanguageAPI.Add("EXAMPLESURVIVOR_PRIMARY_CROSSBOW3_DESCRIPTION", "Perform a <style=cIsDamage> Dash covered in flames</style>.");

            // set up your primary skill def here!

            SkillDef mySkillDef = ScriptableObject.CreateInstance<SkillDef>();
            mySkillDef.activationState = new SerializableEntityStateType(typeof(ClawLaunch));
            mySkillDef.activationStateMachineName = "Weapon";
            mySkillDef.baseMaxStock = 1;
            mySkillDef.baseRechargeInterval = 2.5f;
            mySkillDef.beginSkillCooldownOnSkillEnd = false;
            mySkillDef.canceledFromSprinting = false;
            mySkillDef.fullRestockOnAssign = true;
            mySkillDef.interruptPriority = InterruptPriority.Any;
            mySkillDef.isBullets = false;
            mySkillDef.isCombatSkill = true;
            mySkillDef.mustKeyPress = false;
            mySkillDef.noSprint = true;
            mySkillDef.rechargeStock = 1;
            mySkillDef.requiredStock = 1;
            mySkillDef.shootDelay = 0f;
            mySkillDef.stockToConsume = 1;
            mySkillDef.icon = Assets.icon3;
            mySkillDef.skillDescriptionToken = "EXAMPLESURVIVOR_PRIMARY_CROSSBOW3_DESCRIPTION";
            mySkillDef.skillName = "EXAMPLESURVIVOR_PRIMARY_CROSSBOW3_NAME";
            mySkillDef.skillNameToken = "EXAMPLESURVIVOR_PRIMARY_CROSSBOW3_NAME";

            LoadoutAPI.AddSkillDef(mySkillDef);

            component.utility = characterPrefabMetalGreymon.AddComponent<GenericSkill>();
            SkillFamily newFamily = ScriptableObject.CreateInstance<SkillFamily>();
            newFamily.variants = new SkillFamily.Variant[1];
            LoadoutAPI.AddSkillFamily(newFamily);
            component.utility.SetFieldValue("_skillFamily", newFamily);
            SkillFamily skillFamily = component.utility.skillFamily;

            skillFamily.variants[0] = new SkillFamily.Variant
            {
                skillDef = mySkillDef,
                unlockableName = "",
                viewableNode = new ViewablesCatalog.Node(mySkillDef.skillNameToken, false, null)
            };


            // add this code after defining a new skilldef if you're adding an alternate skill

            /*Array.Resize(ref skillFamily.variants, skillFamily.variants.Length + 1);
            skillFamily.variants[skillFamily.variants.Length - 1] = new SkillFamily.Variant
            {
                skillDef = newSkillDef,
                unlockableName = "",
                viewableNode = new ViewablesCatalog.Node(newSkillDef.skillNameToken, false, null)
            };*/
        }

        static private void SpecialSetup()
        {
            SkillLocator component = characterPrefabMetalGreymon.GetComponent<SkillLocator>();

            LanguageAPI.Add("AXL_SPECIAL_NAME", "Ray Gun");
            LanguageAPI.Add("AXL_SPECIAL_DESCRIPTION", "A rapid firing laser that can shock some enemies, dealing <style=cIsDamage>50% damage</style>, <style=cIsDamage>60% damage</style> and <style=cIsDamage>80% damage</style>. ");

            // set up your primary skill def here!

            SkillDef mySkillDef = ScriptableObject.CreateInstance<SkillDef>();
            mySkillDef.activationState = new SerializableEntityStateType(typeof(MegaDigievolution));
            mySkillDef.activationStateMachineName = "Weapon";
            mySkillDef.baseMaxStock = 2;
            mySkillDef.baseRechargeInterval = 4.8f;
            mySkillDef.beginSkillCooldownOnSkillEnd = false;
            mySkillDef.canceledFromSprinting = false;
            mySkillDef.fullRestockOnAssign = true;
            mySkillDef.interruptPriority = InterruptPriority.Any;
            mySkillDef.isBullets = false;
            mySkillDef.isCombatSkill = true;
            mySkillDef.mustKeyPress = false;
            mySkillDef.noSprint = true;
            mySkillDef.rechargeStock = 1;
            mySkillDef.requiredStock = 1;
            mySkillDef.shootDelay = 0.2f;
            mySkillDef.stockToConsume = 1;
            mySkillDef.icon = Assets.icon4;
            mySkillDef.skillDescriptionToken = "AXL_SPECIAL_DESCRIPTION";
            mySkillDef.skillName = "AXL_SPECIAL_NAME";
            mySkillDef.skillNameToken = "AXL_SPECIAL_NAME";

            LoadoutAPI.AddSkillDef(mySkillDef);

            component.special = characterPrefabMetalGreymon.AddComponent<GenericSkill>();
            SkillFamily newFamily = ScriptableObject.CreateInstance<SkillFamily>();
            newFamily.variants = new SkillFamily.Variant[1];
            LoadoutAPI.AddSkillFamily(newFamily);
            component.special.SetFieldValue("_skillFamily", newFamily);
            SkillFamily skillFamily = component.special.skillFamily;

            skillFamily.variants[0] = new SkillFamily.Variant
            {
                skillDef = mySkillDef,
                unlockableName = "",
                viewableNode = new ViewablesCatalog.Node(mySkillDef.skillNameToken, false, null)
            };


            // add this code after defining a new skilldef if you're adding an alternate skill

            /*Array.Resize(ref skillFamily.variants, skillFamily.variants.Length + 1);
            skillFamily.variants[skillFamily.variants.Length - 1] = new SkillFamily.Variant
            {
                skillDef = newSkillDef,
                unlockableName = "",
                viewableNode = new ViewablesCatalog.Node(newSkillDef.skillNameToken, false, null)
            };*/
        }
    }

    namespace EntityStates.ExampleSurvivorStates
    {

        public class GreymonMetalBlaster : BaseSkillState
        {
            public float damageCoefficient = 2f;
            public float baseDuration = 0.15f;
            public float recoil = 2f;
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
                this.fireDuration = 0.025f * this.duration;         //changed 0.25
                base.characterBody.SetAimTimer(0.5f);                 //changed 2
                this.animator = base.GetModelAnimator();
                this.muzzleString = "Muzzle";


                base.PlayAnimation("Fuego", "GOpenMouth");
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

                    if (base.isAuthority)
                    {
                        ProjectileManager.instance.FireProjectile(ExampleSurvivor.arrowProjectile, aimRay.origin, Util.QuaternionSafeLookRotation(aimRay.direction), base.gameObject, this.damageCoefficient * this.damageStat, 0f, Util.CheckRoll(this.critStat, base.characterBody.master), DamageColorIndex.Default, null, -1f);
                        ProjectileManager.instance.FireProjectile(ExampleSurvivor.arrowProjectile, aimRay.origin, Util.QuaternionSafeLookRotation(aimRay.direction), base.gameObject, this.damageCoefficient * this.damageStat, 0f, Util.CheckRoll(this.critStat, base.characterBody.master), DamageColorIndex.Default, null, -1f);
                        ProjectileManager.instance.FireProjectile(ExampleSurvivor.arrowProjectile, aimRay.origin, Util.QuaternionSafeLookRotation(aimRay.direction), base.gameObject, this.damageCoefficient * this.damageStat, 0f, Util.CheckRoll(this.critStat, base.characterBody.master), DamageColorIndex.Default, null, -1f);


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
        public class GreymonMetalBlast : BaseSkillState
        {
            public float damageCoefficient = 10f;
            public float baseDuration = 0.15f;
            public float recoil = 0.25f;
            public static GameObject tracerEffectPrefab = Resources.Load<GameObject>("Prefabs/Effects/Tracers/TracerToolbotRebar");

            private float duration;
            private float fireDuration;
            private bool hasFired;
            private Animator animator;
            private string muzzleString;

            public override void OnEnter()
            {
                base.OnEnter();
                AkSoundEngine.PostEvent(1769321799, base.gameObject);
                this.duration = this.baseDuration / this.attackSpeedStat;
                this.fireDuration = 0.05f * this.duration;         //changed 0.25
                base.characterBody.SetAimTimer(0.5f);                 //changed 2
                this.animator = base.GetModelAnimator();
                this.muzzleString = "Muzzle";


                base.PlayAnimation("Gesture, Override", "FireArrow", "FireArrow.playbackRate", this.duration);
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

                    base.characterBody.AddSpreadBloom(7.5f);
                    Ray aimRay = base.GetAimRay();

                    if (base.isAuthority)
                    {
                        ProjectileManager.instance.FireProjectile(Digievolutions.MetalGreymon.GreymonMetalBlast, aimRay.origin, Util.QuaternionSafeLookRotation(aimRay.direction), base.gameObject, this.damageCoefficient * this.damageStat, 1f, Util.CheckRoll(this.critStat, base.characterBody.master), DamageColorIndex.Default, null, -1f);
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

        public class MegaDigievolution : BaseSkillState
        {
            public float damageCoefficient = 0.5f;
            public float baseDuration = 0.10f;
            public float recoil = 0.5f;
            //public static GameObject tracerEffectPrefab = Resources.Load<GameObject>("Prefabs/Effects/Tracers/TracerToolbotRebar");
            public static GameObject tracerEffectPrefab = Resources.Load<GameObject>("prefabs/effects/tracers/TracerGolem");
            public static GameObject hitEffectPrefab = Resources.Load<GameObject>("prefabs/effects/impacteffects/Hitspark1");


            public static GameObject characterPrefabWarGreymon = WarGreymon.characterPrefabWarGreymon;


            private float duration;
            private float fireDuration;
            private bool hasFired;
            private Animator animator;
            private string muzzleString;

            public override void OnEnter()
            {
                base.OnEnter();
                AkSoundEngine.PostEvent(768011503, base.gameObject);

                base.characterBody.master.bodyPrefab = characterPrefabWarGreymon;
                base.characterBody.master.Respawn(gameObject.transform.localPosition, gameObject.transform.localRotation);
            }

            public override void OnExit()
            {
                base.OnExit();
            }

            private void FireRG()
            {
                if (!this.hasFired)
                {
                    this.hasFired = true;
                    //wololo
                    //characterPrefab=
                    base.characterBody.AddSpreadBloom(0.75f);
                    Ray aimRay = base.GetAimRay();
                    //EffectManager.SimpleMuzzleFlash(Commando.CommandoWeapon.FirePistol.effectPrefab, base.gameObject, this.muzzleString, false);

                    if (base.isAuthority)
                    {

                    }
                }
            }

            public override void FixedUpdate()
            {
                base.FixedUpdate();

                if (base.fixedAge >= this.fireDuration)
                {
                    FireRG();
                }

            }

            public override InterruptPriority GetMinimumInterruptPriority()
            {
                return InterruptPriority.Skill;
            }
        }

        public class Slashu : BaseSkillState
        {
            public static float damageCoefficient = 5000f;
            public float baseDuration = 1.5f;
            public static float attackRecoil = 0.5f;
            public static float hitHopVelocity = 5.5f;
            public static float earlyExitTime = 0.575f;
            public int swingIndex;

            private bool inCombo;
            private float earlyExitDuration;
            private float duration;
            private bool hasFired;
            private float hitPauseTimer;
            private OverlapAttack attack;
            private bool inHitPause;
            private bool hasHopped;
            private float stopwatch;
            private bool cancelling;
            private Animator animator;
            private BaseState.HitStopCachedState hitStopCachedState;
            //private PaladinSwordController swordController;

            public override void OnEnter()
            {
                base.OnEnter();
                this.duration = this.baseDuration / this.attackSpeedStat;
                this.earlyExitDuration = this.duration * 1f;
                this.hasFired = false;
                this.cancelling = false;
                this.animator = base.GetModelAnimator();
                // this.swordController = base.GetComponent<PaladinSwordController>();
                base.StartAimMode(1.5f + this.duration, false);
                base.characterBody.isSprinting = false;
                this.inCombo = false;

                //if (this.swordController) this.swordController.attacking = true;

                HitBoxGroup hitBoxGroup = null;
                Transform modelTransform = base.GetModelTransform();
                base.PlayAnimation("Fuego", "GCharge");
                if (modelTransform)
                {
                    hitBoxGroup = Array.Find<HitBoxGroup>(modelTransform.GetComponents<HitBoxGroup>(), (HitBoxGroup element) => element.groupName == "HeadG");
                }

                if (this.swingIndex > 1)
                {
                    this.swingIndex = 0;
                    this.inCombo = true;
                }

                // Util.PlaySound(Modules.Sounds.Cloth1, base.gameObject);

                string animString = "Slash" + (1 + swingIndex).ToString();

                if (this.inCombo)
                {
                    if (!this.animator.GetBool("isMoving") && this.animator.GetBool("isGrounded")) base.PlayCrossfade("FullBody, Override", "SlashCombo1", "Slash.playbackRate", this.duration, 0.05f);
                    base.PlayCrossfade("Gesture, Override", "SlashCombo1", "Slash.playbackRate", this.duration, 0.05f);
                }
                else
                {
                    if (!this.animator.GetBool("isMoving") && this.animator.GetBool("isGrounded")) base.PlayCrossfade("FullBody, Override", animString, "Slash.playbackRate", this.duration, 0.05f);
                    base.PlayCrossfade("Gesture, Override", animString, "Slash.playbackRate", this.duration, 0.05f);
                }



                this.attack = new OverlapAttack();
                this.attack.damageType = DamageType.Generic;
                this.attack.attacker = base.gameObject;
                this.attack.inflictor = base.gameObject;
                this.attack.teamIndex = base.GetTeam();
                this.attack.damage = 1f * this.damageStat;
                this.attack.procCoefficient = 1;
                //this.attack.hitEffectPrefab = this.swordController.hitEffect;
                this.attack.forceVector = Vector3.zero;
                this.attack.pushAwayForce = 750f;
                this.attack.hitBoxGroup = hitBoxGroup;
                this.attack.isCrit = base.RollCrit();
            }

            public override void OnExit()
            {
                base.OnExit();

                if (!this.hasFired) this.FireAttack();

                if (this.inHitPause)
                {
                    base.ConsumeHitStopCachedState(this.hitStopCachedState, base.characterMotor, this.animator);
                    this.inHitPause = false;
                }

                //base.PlayAnimation("FullBody, Override", "BufferEmpty");
                //base.PlayAnimation("Gesture, Override", "BufferEmpty");

                //if (this.swordController) this.swordController.attacking = false;
            }

            public void FireAttack()
            {
                if (!this.hasFired)
                {
                    this.hasFired = true;
                    //this.swordController.PlaySwingSound();

                    if (base.isAuthority)
                    {
                        string muzzleString = null;
                        if (this.swingIndex == 0) muzzleString = "SwingRight";
                        else muzzleString = "SwingLeft";

                        //base.AddRecoil(-1f * Slash.attackRecoil, -2f * Slash.attackRecoil, -0.5f * Slash.attackRecoil, 0.5f * Slash.attackRecoil);
                        //EffectManager.SimpleMuzzleFlash(this.swordController.swingEffect, base.gameObject, muzzleString, true);

                        Ray aimRay = base.GetAimRay();



                        if (this.attack.Fire())
                        {
                            //this.swordController.PlayHitSound(0);

                            if (!this.hasHopped)
                            {
                                if (base.characterMotor && !base.characterMotor.isGrounded)
                                {

                                }

                                if (base.skillLocator.utility.skillDef.skillNameToken == "PALADIN_UTILITY_DASH_NAME") base.skillLocator.utility.RunRecharge(1f);

                                this.hasHopped = true;
                            }

                            if (!this.inHitPause)
                            {
                                this.hitStopCachedState = base.CreateHitStopCachedState(base.characterMotor, this.animator, "Slash.playbackRate");
                                this.hitPauseTimer = (2f) / this.attackSpeedStat;
                                this.inHitPause = true;
                            }
                        }
                    }
                }
            }

            public override void FixedUpdate()
            {
                base.FixedUpdate();
                if (this.animator) this.animator.SetBool("inCombat", true);
                this.hitPauseTimer -= Time.fixedDeltaTime;

                if (this.hitPauseTimer <= 0f && this.inHitPause)
                {
                    base.ConsumeHitStopCachedState(this.hitStopCachedState, base.characterMotor, this.animator);
                    this.inHitPause = false;
                }

                if (!this.inHitPause)
                {
                    this.stopwatch += Time.fixedDeltaTime;
                }
                else
                {
                    if (base.characterMotor) base.characterMotor.velocity = Vector3.zero;
                    if (this.animator) this.animator.SetFloat("Slash.playbackRate", 0f);
                }

                if (this.stopwatch >= this.duration * 1.225f && this.stopwatch <= this.duration * 1.5f)
                {
                    this.FireAttack();
                }

                if (this.stopwatch >= (this.duration * 0.5f) && base.inputBank.skill2.down && base.skillLocator.secondary.skillDef.skillNameToken == "PALADIN_SECONDARY_LUNARSHARD_NAME")
                {
                    this.cancelling = true;
                    base.skillLocator.secondary.ExecuteIfReady();
                    return;
                }

                if (base.isAuthority)
                {
                    if (base.fixedAge >= this.earlyExitDuration && base.inputBank.skill1.down)
                    {
                        var nextSwing = new Slash();

                        this.outer.SetNextState(nextSwing);
                        return;
                    }

                    if (base.fixedAge >= this.duration)
                    {
                        this.outer.SetNextStateToMain();
                        return;
                    }
                }
            }

            public override InterruptPriority GetMinimumInterruptPriority()
            {
                if (this.cancelling) return InterruptPriority.Any;
                else return InterruptPriority.Skill;
            }

            public override void OnSerialize(NetworkWriter writer)
            {
                base.OnSerialize(writer);
                writer.Write(this.swingIndex);
            }

            public override void OnDeserialize(NetworkReader reader)
            {
                base.OnDeserialize(reader);
                this.swingIndex = reader.ReadInt32();
            }
        }

        public class GigaDestroyerCharge : BaseSkillState
        {
            // Token: 0x020009BB RID: 2491
            public class ChargeTwinShot : BaseState
            {
                // Token: 0x040032F8 RID: 13048
                public static float baseDuration = 3f;

                // Token: 0x040032F9 RID: 13049
                public static float laserMaxWidth = 0.2f;

                // Token: 0x040032FA RID: 13050
                public static GameObject effectPrefab=MetalGreymon.GigaDestroyer;

                // Token: 0x040032FB RID: 13051
                public static string chargeSoundString;

                // Token: 0x040032FC RID: 13052
                private float duration;

                // Token: 0x040032FD RID: 13053
                private uint chargePlayID;

                // Token: 0x040032FE RID: 13054
                private List<GameObject> chargeEffects = new List<GameObject>();

                // Token: 0x06003997 RID: 14743 RVA: 0x000EC280 File Offset: 0x000EA480
                public override void OnEnter()
                {
                    base.OnEnter();
                    this.duration = ChargeTwinShot.baseDuration / this.attackSpeedStat;
                    Transform modelTransform = base.GetModelTransform();
                    this.chargePlayID = Util.PlayScaledSound(ChargeTwinShot.chargeSoundString, base.gameObject, this.attackSpeedStat);
                    base.PlayCrossfade("Gesture, Additive", "ChargeTwinShot", "TwinShot.playbackRate", this.duration, 0.1f);
                    if (modelTransform)
                    {
                        ChildLocator component = modelTransform.GetComponent<ChildLocator>();
                        if (component)
                        {
                            List<Transform> list = new List<Transform>();
                            list.Add(component.FindChild("Shooter_R"));
                            list.Add(component.FindChild("Shooter_L"));

                            if (ChargeTwinShot.effectPrefab)
                            {
                                for (int i = 0; i < list.Count; i++)
                                {
                                    if (list[i])
                                    {
                                        GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(ChargeTwinShot.effectPrefab, list[i].position, list[i].rotation);
                                        gameObject.transform.parent = list[i];
                                        ScaleParticleSystemDuration component2 = gameObject.GetComponent<ScaleParticleSystemDuration>();
                                        if (component2)
                                        {
                                            component2.newDuration = this.duration;
                                        }
                                        this.chargeEffects.Add(gameObject);
                                    }
                                }
                            }
                        }
                    }
                    if (base.characterBody)
                    {
                        base.characterBody.SetAimTimer(this.duration);
                    }
                }

                // Token: 0x06003998 RID: 14744 RVA: 0x000EC400 File Offset: 0x000EA600
                public override void OnExit()
                {
                    AkSoundEngine.StopPlayingID(this.chargePlayID);
                    base.OnExit();
                    for (int i = 0; i < this.chargeEffects.Count; i++)
                    {
                        if (this.chargeEffects[i])
                        {
                            EntityState.Destroy(this.chargeEffects[i]);
                        }
                    }
                }

                // Token: 0x06003999 RID: 14745 RVA: 0x000D44F8 File Offset: 0x000D26F8
                public override void Update()
                {
                    base.Update();
                }

                // Token: 0x0600399A RID: 14746 RVA: 0x000EC458 File Offset: 0x000EA658
                public override void FixedUpdate()
                {
                    base.FixedUpdate();
                    if (base.fixedAge >= this.duration && base.isAuthority)
                    {
                        GigaDestroyerFire nextState = new GigaDestroyerFire();
                        this.outer.SetNextState(nextState);
                        return;
                    }
                }

                // Token: 0x0600399B RID: 14747 RVA: 0x0000CFF7 File Offset: 0x0000B1F7
                public override InterruptPriority GetMinimumInterruptPriority()
                {
                    return InterruptPriority.Skill;
                }
            }
        }

        public class GigaDestroyerFire : BaseSkillState
        {
            // Token: 0x020009BB RID: 2491


            public static GameObject projectilePrefab=MetalGreymon.GigaDestroyer;

            // Token: 0x04003301 RID: 13057
            public static GameObject effectPrefab = MetalGreymon.GigaDestroyer;

            // Token: 0x04003302 RID: 13058
            public static GameObject dustEffectPrefab = MetalGreymon.GigaDestroyer;

            // Token: 0x04003303 RID: 13059
            public static GameObject hitEffectPrefab = MetalGreymon.GigaDestroyer;

            // Token: 0x04003304 RID: 13060
            public static GameObject tracerEffectPrefab = MetalGreymon.GigaDestroyer;

            // Token: 0x04003305 RID: 13061
            public static float damageCoefficient;

            // Token: 0x04003306 RID: 13062
            public static float blastRadius;

            // Token: 0x04003307 RID: 13063
            public static float force;

            // Token: 0x04003308 RID: 13064
            public static float baseDuration = 2f;

            // Token: 0x04003309 RID: 13065
            public static string attackSoundString;

            // Token: 0x0400330A RID: 13066
            public static float aimTime = 2f;

            // Token: 0x0400330B RID: 13067
            public static string Shooter_R;

            // Token: 0x0400330C RID: 13068
            public static string Shooter_L;

            // Token: 0x0400330D RID: 13069
            public static string leftMuzzleBot;

            // Token: 0x0400330E RID: 13070
            public static string rightMuzzleBot;

            // Token: 0x0400330F RID: 13071
            private float duration;
            // Token: 0x060039A1 RID: 14753 RVA: 0x000EC4F0 File Offset: 0x000EA6F0
            public override void OnEnter()
            {
                base.OnEnter();
                this.duration = GigaDestroyerFire.baseDuration / this.attackSpeedStat;
                base.GetModelAnimator();
                base.GetModelTransform();
                Util.PlaySound(GigaDestroyerFire.attackSoundString, base.gameObject);
                if (base.characterBody)
                {
                    base.characterBody.SetAimTimer(GigaDestroyerFire.aimTime);
                }
                base.PlayAnimation("Gesture, Additive", "FireTwinShot", "TwinShot.playbackRate", this.duration);
                if (GigaDestroyerFire.effectPrefab)
                //{
                //    EffectManager.SimpleMuzzleFlash(GigaDestroyerFire.effectPrefab, base.gameObject, GigaDestroyerFire.leftMuzzleTop, false);
                //    EffectManager.SimpleMuzzleFlash(GigaDestroyerFire.effectPrefab, base.gameObject, GigaDestroyerFire.rightMuzzleTop, false);
                //    EffectManager.SimpleMuzzleFlash(GigaDestroyerFire.effectPrefab, base.gameObject, GigaDestroyerFire.leftMuzzleBot, false);
                //    EffectManager.SimpleMuzzleFlash(GigaDestroyerFire.effectPrefab, base.gameObject, GigaDestroyerFire.rightMuzzleBot, false);
                //}
                if (GigaDestroyerFire.dustEffectPrefab)
                {
                    EffectManager.SimpleMuzzleFlash(GigaDestroyerFire.dustEffectPrefab, base.gameObject, "Root", false);
                }
                Ray aimRay = base.GetAimRay();
                if (base.isAuthority && base.modelLocator && base.modelLocator.modelTransform)
                {
                    ChildLocator component = base.modelLocator.modelTransform.GetComponent<ChildLocator>();
                    if (component)
                    {
                        int childIndex = component.FindChildIndex(GigaDestroyerFire.Shooter_R);
                        int childIndex2 = component.FindChildIndex(GigaDestroyerFire.Shooter_L);

                        Transform transform = component.FindChild("Shooter_R");
                        Transform transform2 = component.FindChild("Shooter_L");
         
                        if (transform)
                        {
                            ProjectileManager.instance.FireProjectile(GigaDestroyerFire.projectilePrefab, transform.position, Util.QuaternionSafeLookRotation(aimRay.direction), base.gameObject, this.damageStat * GigaDestroyerFire.damageCoefficient, GigaDestroyerFire.force, Util.CheckRoll(this.critStat, base.characterBody.master), DamageColorIndex.Default, null, -1f);
                        }
                        if (transform2)
                        {
                            ProjectileManager.instance.FireProjectile(GigaDestroyerFire.projectilePrefab, transform2.position, Util.QuaternionSafeLookRotation(aimRay.direction, Vector3.down), base.gameObject, this.damageStat * GigaDestroyerFire.damageCoefficient, GigaDestroyerFire.force, Util.CheckRoll(this.critStat, base.characterBody.master), DamageColorIndex.Default, null, -1f);
                        }

                    }
                }
            }

            // Token: 0x060039A2 RID: 14754 RVA: 0x00032FA7 File Offset: 0x000311A7
            public override void OnExit()
            {
                base.OnExit();
            }

            // Token: 0x060039A3 RID: 14755 RVA: 0x000EC823 File Offset: 0x000EAA23
            public override void FixedUpdate()
            {
                base.FixedUpdate();
                if (base.fixedAge >= this.duration && base.isAuthority)
                {
                    this.outer.SetNextStateToMain();
                    return;
                }
            }

            // Token: 0x060039A4 RID: 14756 RVA: 0x0000CFF7 File Offset: 0x0000B1F7
            public override InterruptPriority GetMinimumInterruptPriority()
            {
                return InterruptPriority.Skill;
            }
        }

        public class ClawLaunch : BaseSkillState
        {

            // Token: 0x020009C7 RID: 2503
         

                [SerializeField]
                public GameObject projectilePrefab = MetalGreymon.ClawHook;

                // Token: 0x04003344 RID: 13124
                public static float damageCoefficient;

                // Token: 0x04003345 RID: 13125
                public static GameObject muzzleflashEffectPrefab;

                // Token: 0x04003346 RID: 13126
                public static string fireSoundString;

                // Token: 0x04003347 RID: 13127
                public GameObject hookInstance;

                // Token: 0x04003348 RID: 13128
                protected ProjectileStickOnImpact hookStickOnImpact;

                // Token: 0x04003349 RID: 13129
                private bool isStuck;

                // Token: 0x0400334A RID: 13130
                private bool hadHookInstance;

                // Token: 0x0400334B RID: 13131
                private uint soundID;
                // Token: 0x060039D3 RID: 14803 RVA: 0x000ED2E0 File Offset: 0x000EB4E0
                public  override void OnEnter()
                {
                    base.OnEnter();
                    if (base.isAuthority)
                    {
                        Ray aimRay = base.GetAimRay();
                        FireProjectileInfo fireProjectileInfo = new FireProjectileInfo
                        {
                            position = aimRay.origin,
                            rotation = Quaternion.LookRotation(aimRay.direction),
                            crit = base.characterBody.RollCrit(),
                            damage = this.damageStat * ClawLaunch.damageCoefficient,
                            force = 0f,
                            damageColorIndex = DamageColorIndex.Default,
                            procChainMask = default(ProcChainMask),
                            projectilePrefab = this.projectilePrefab,
                            owner = base.gameObject
                        };
                        ProjectileManager.instance.FireProjectile(fireProjectileInfo);
                    }
                    EffectManager.SimpleMuzzleFlash(ClawLaunch.muzzleflashEffectPrefab, base.gameObject, "MuzzleLeft", false);
                    Util.PlaySound(ClawLaunch.fireSoundString, base.gameObject);
                    base.PlayAnimation("Grapple", "FireHookIntro");
                }

                // Token: 0x060039D4 RID: 14804 RVA: 0x000ED3D2 File Offset: 0x000EB5D2
                public void SetHookReference(GameObject hook)
                {
                    this.hookInstance = hook;
                    this.hookStickOnImpact = hook.GetComponent<ProjectileStickOnImpact>();
                    this.hadHookInstance = true;
                }

                // Token: 0x060039D5 RID: 14805 RVA: 0x000ED3F0 File Offset: 0x000EB5F0
                public override void FixedUpdate()
                {
                    base.FixedUpdate();
                    if (this.hookStickOnImpact)
                    {
                        if (this.hookStickOnImpact.stuck && !this.isStuck)
                        {
                            base.PlayAnimation("Grapple", "FireHookLoop");
                        }
                        this.isStuck = this.hookStickOnImpact.stuck;
                    }
                    if (base.isAuthority && !this.hookInstance && this.hadHookInstance)
                    {
                        this.outer.SetNextStateToMain();
                    }
                }

                // Token: 0x060039D6 RID: 14806 RVA: 0x000ED46E File Offset: 0x000EB66E
                public override void OnExit()
                {
                    base.PlayAnimation("Grapple", "FireHookExit");
                    EffectManager.SimpleMuzzleFlash(ClawLaunch.muzzleflashEffectPrefab, base.gameObject, "MuzzleLeft", false);
                    base.OnExit();
                }

                // Token: 0x060039D7 RID: 14807 RVA: 0x0000DC97 File Offset: 0x0000BE97
                public override InterruptPriority GetMinimumInterruptPriority()
                {
                    return InterruptPriority.Pain;
                }
            }

        }
    }

