using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace etchebarren
{
    public class AnimEvents : MonoBehaviour
    {
        public static AnimEvents instance;

        private CameraHandler cameraHandler;
        private InputHandler inputHandler;
        private PlayerLocomotion playerLocomotion;
        private AnimatorHandler animatorHandler;
        private PlayerInventory playerInventory;
        public PlayerStats playerStats;
        public PlayerAudioManager playerAudioManager;
        public SelfEffectsManager selfEffectsManager;
        public PlayerFootstepSoundManager playerFootstepSoundManager;
        public ClimbManager climbManager;
        public QuestManager questManager;
        public ControllerUIManager controllerUIManager;
        public GameObject controlsHUD;

        // Sword Colliders
        public DamageTriggerCollider venusWeaponCollider;
        public DamageTriggerCollider marsWeaponCollider;

        // Right Fist (Unarmed) Colliders
        public DamageTriggerCollider venusRightFistCollider;
        public DamageTriggerCollider marsRightFistCollider;

        // Shield Colliders
        public DamageTriggerCollider venusWeaponShieldCollider;
        public DamageTriggerCollider marsWeaponShieldCollider;

        // Left Fist (Unarmed) Colliders
        public DamageTriggerCollider venusLeftFistCollider;
        public DamageTriggerCollider marsLeftFistCollider;

        // Kick (Unarmed Heavy) Colliders
        public DamageTriggerCollider venusKickCollider;
        public DamageTriggerCollider marsKickCollider;

        public List<GameObject> projectiles = new List<GameObject>();
        public GameObject projectileSpawnPoint;

        public bool isStaggered = false;

        [Header("Dodging")]
        public bool displayIFramesWhenDodging = false;
        public GameObject dodgeDebugSphere;
        public int i = 0;
        public LayerMask dodgeLayers;
        public List<Collider> playerDamageReceivingColliders = new List<Collider>(); // built at runtime
        public List<Collider> enemyProjectilesColliders = new List<Collider>(); // set automatically/dynamically

        [Header("Blocking")]
        public float arrowCorrectionMinDistance = 0.000001f; // min distance from cloeset two points between arrow and active sheild mesh to trigger correction
        public float arrowCorrectionBoundsBonus = 0.1f; //correction multiplier for projectile shield, sets arrows to shield mesh
        public MeshRenderer projectileShieldDebug;

        // Disable collisions between these layers entirely

        [Header("SPELL EFFECTS & REFERENCES")]
        List<string> acceptedTags = new List<string> { "Terrain", "Untagged", "Default" };
        public Transform defaultAOELocation;
        public ParticleSystem handEffectAOE;
        public ParticleSystem handEffectFlash;
        [Header("Items:")]
        public GameObject itemEffect;
        [Header("Mend:")]
        public ParticleSystem mendStartingEffect;
        public ParticleSystem mendHealEffect;
        [Header("Rejuvenate:")]
        public Coroutine rejuvenateCoroutine;
        public GameObject rejuvenateBuffIcon;
        public ParticleSystem rejuvenateStartingEffect;
        public ParticleSystem rejuvenateEffect;
        public ParticleSystem rejuvenateTickEffect;
        [SerializeField] private float rejuvenateTimer = 0;
        [SerializeField] private float lastHealTime = 0;
        [Header("Meteor:")]
        public GameObject meteorPrefab;
        public ParticleSystem meteorStartingEffect;
        public ParticleSystem meteorEffect;
        public Color meteorHandEffectColor;
        [Header("Glacial Burst:")]
        public GameObject glacialBurstInitialHitPrefab;
        public GameObject glacialBurstPrefab;
        public ParticleSystem glacialBurstStartingEffect;
        public Color glacialBurstHandEffectColor;
        [Header("Stormguard:")]
        public Coroutine stormguardCoroutine;
        public GameObject stormguardBuffIcon;
        public ParticleSystem stormguardStartingEffect;
        public ParticleSystem stormguardEffect;
        public bool stormguardActive = false;
        [SerializeField] private float stormguardTimer = 0f;
        public GameObject stormguardZapPrefab; // Used in DamageTriggerCollider
        [Header("Arcana Lumina")]
        public Coroutine arcanaLuminaCoroutine;
        public GameObject arcanaLuminaBuffIcon;
        public ParticleSystem arcanaLumina;
        public ParticleSystem arcanaLuminaRays;
        public ParticleSystem arcanaLuminaSmallStars;
        public Light arcanaLuminaLight;
        public float arcanaLuminaTimer = 0f;
        [Header("Mystic Infusion")]
        public GameObject infusionBuffIcon;
        public Image infusionTypeIcon;
        public Sprite[] infusionSprites;
        public bool canSwitchInfusionType = false;
        public bool infusedFire = false;
        public bool infusedIce = false;
        public bool infusedShock = false;
        public AudioSource fireInfusionIgniteAudio;
        public AudioSource iceInfusionIgniteAudio;
        public AudioSource shockInfusionIgniteAudio;
        public ParticleSystem fireInfusionIgnite;
        public GameObject fireInfusion;
        public GameObject iceInfusion;
        public GameObject shockInfusion;
        public float infusionTimer;

        [Header("Infusion")]
        public float infusionDuration = 20.0f;
        private Coroutine infusionCoroutine;

        [Header("Spider Venom Buff")]
        public GameObject spiderVenomBuffIcon;

        [Header("Digging")]
        public GameObject shovel;
        public bool wasUnsheathedBeforeDiging;
        public DamageTriggerCollider venusShovelDamageCollider;
        public DamageTriggerCollider marsShovelDamageCollider;
        public GameObject dirtOnShovel;
        public ParticleSystem dirtThrow;
        public GameObject dirtPilePrefab;
        public Transform dirtRayOrigin;
        public LayerMask validLayers;
        public string[] invalidDigKeywords;
        public bool validDirtPile;
        public DigSpot digSpot;
        private Terrain terrain;
        private TerrainData terrainData;
        public GameObject shovelSparksPrefab;
        public Transform shovelSparksSpawnPoint;
        public SphereCollider digCheckCollider;

        [Header("Death Object Refs")]
        public GameObject[] hideOnDeath;
        public GameObject deathScreenObj;
        public Image deathScreen;
        public Image deathVignette;
        public TextMeshProUGUI deathText;
        private Color targetColorDeathScreen;
        private Color targetColorDeathVignette;
        private Color targetColorDeathText;
        public Color transparentColor;
        private Coroutine deathCoroutine;
        private Coroutine deathBackupTimerCoroutine;
        private Coroutine fadeCoroutine;
        private Coroutine timeScaleCoroutine;
        public AnimationCurve timeScaleCurve;
        public GameObject shieldItemParent;

        private void Awake()
        {
            instance = this;
        }

        private void Start()
        {
            cameraHandler = CameraHandler.instance;
            inputHandler = InputHandler.instance;
            playerLocomotion = PlayerLocomotion.instance;
            animatorHandler = AnimatorHandler.instance;
            playerInventory = PlayerInventory.instance;

            targetColorDeathScreen = deathScreen.color;
            targetColorDeathVignette = deathVignette.color;
            targetColorDeathText = deathText.color;
            playerDamageReceivingColliders = GetDamageReceivingColliders(playerStats.gameObject, dodgeLayers);
        }

        void OnEnable()
        {
            instance = this;
        }

        public void PlayRollAudio()
        {
            playerAudioManager.PlayRollAudio();
        }

        public void PlayBackstepAudio()
        {
            playerAudioManager.PlayBackstepAudio();
        }

        public void CanQueueAttackTrue()
        {
            //Debug.Log("Can queue attack true called");
            if (!isStaggered)
                inputHandler.canQueueAttack = true;
            else
                Debug.Log("canQueueAttack NOT set true, because isStaggered");
        }

        public void CanQueueAttackFalse()
        {
            //Debug.Log("Can queue attack false called");
            inputHandler.canQueueAttack = false;
            DeactivateWeaponShield();
            DeactivateWeapon();
        }

        public void CheckAttackOneCombo()
        {
            Debug.Log("CHECKATTACKONECOMBO CALLED, but it was commented out!!!! Still Need?");
            //playerLocomotion.CheckAttackOneCombo();
        }

        public void PlayJumpAudio()
        {
            playerAudioManager.PlayJumpAudio();
            playerLocomotion.airDashTriggered = 0;
        }

        public void PlayLandAudio()
        {
            playerAudioManager.PlayLandAudio();
            playerLocomotion.airDashTriggered = 0;
            playerLocomotion.isAirDashing = false;
        }

        public void PlayWeaponSwingAudio()
        {
            playerAudioManager.PlayWeaponSwingAudio();
        }

        public void PlaySlideAudio()
        {
            playerAudioManager.PlaySlideAudio();
        }

        public void CanMove() // This is used to allow movement after jumping, but before entering Empty state (smoother)
        {
            //Debug.Log("Can Move called");
            inputHandler.canMove = true;
            //StopClimbing();
        }

        public void JumpEnd()
        {
            //StopClimbing();
            //Debug.Log("Jump End called");
            inputHandler.isJumping = false;
            DeactivateWeaponShield();
            DeactivateWeapon();
        }

        public void AirDashStart()
        {
            playerLocomotion.ImpulseOnAirDash();
            playerLocomotion.isAirDashing = true;
        }

        public void AirDashEnd()
        {
            playerLocomotion.isAirDashing = false;
        }

        public void ItemStart()
        {
            PlayParticleSystemsOnObject();
        }

        public void ItemAudio()
        {
            playerInventory.equippedConsumables[ConsumablesHUDManager.instance.selectedConsumable].PlayItemAudio();
        }

        public void UseItem()
        {
            playerInventory.equippedConsumables[ConsumablesHUDManager.instance.selectedConsumable].UseConsumable();
        }

        public void ItemDone()
        {
            playerLocomotion.usingItem = false;
            inputHandler.isPerformingAction = false;
        }

        public void ActivateWeapon(string type = "Normal")
        {
            DeactivateWeaponShield();
            DeactivateWeapon();

            if (type!="Air Dash") AirDashEnd();

            if (type == "NormalSplit")
            {
                Debug.Log("ANIMEVENT: Heavy Attack!");
                marsWeaponCollider.normalSplit = true;
                venusWeaponCollider.normalSplit = true;

                venusLeftFistCollider.normalSplit = true;
                marsLeftFistCollider.normalSplit = true;
            }
            else if (type=="Charged")
            {
                Debug.Log("ANIMEVENT: ChargeAttack!");
                marsWeaponCollider.chargedAttack = true;
                venusWeaponCollider.chargedAttack = true;

                venusRightFistCollider.chargedAttack = true;
                marsRightFistCollider.chargedAttack = true;
            }
            else if(type=="Air" || type=="Air Dash")
            {
                Debug.Log("ANIMEVENT: Air Attack! (type=" + type + ")");
                // Unarmed
                if(playerInventory.equippedWeapon == playerInventory.emptyWeapon)
                {
                    // Disable Sword Colliders
                    venusWeaponCollider.enabled = false;
                    marsWeaponCollider.enabled = false;
                    // Enable Fist Colliders
                    venusRightFistCollider.enabled = true;
                    marsRightFistCollider.enabled = true;
                    // Set Collider Modifier
                    venusRightFistCollider.airAttack = true;
                    marsRightFistCollider.airAttack = true;
                }
                // Armed
                else
                {
                    // Enable Sword Colliders
                    venusWeaponCollider.enabled = true;
                    marsWeaponCollider.enabled = true;
                    // Disable Fist Colliders
                    venusRightFistCollider.enabled = false;
                    marsRightFistCollider.enabled = false;
                    // Set Collider Modifier
                    marsWeaponCollider.airAttack = true;
                    venusWeaponCollider.airAttack = true;
                }
            }
            else if (type == "LightSprintAttack")
            {
                Debug.Log("ANIMEVENT: Light Sprint Attack!");
                if (playerInventory.equippedWeapon == playerInventory.emptyWeapon)
                {
                    // Disable Sword Colliders
                    venusWeaponCollider.enabled = false;
                    marsWeaponCollider.enabled = false;
                    // Enable Fist Colliders
                    venusRightFistCollider.enabled = true;
                    marsRightFistCollider.enabled = true;
                    // Set Collider Modifier
                    venusRightFistCollider.lightSprintAttack = true;
                    marsRightFistCollider.lightSprintAttack = true;
                }
                // Armed
                else
                {
                    // Enable Sword Colliders
                    venusWeaponCollider.enabled = true;
                    marsWeaponCollider.enabled = true;
                    // Disable Fist Colliders
                    venusRightFistCollider.enabled = false;
                    marsRightFistCollider.enabled = false;
                    // Set Collider Modifier
                    marsWeaponCollider.lightSprintAttack = true;
                    venusWeaponCollider.lightSprintAttack = true;
                }
            }
            else if (type == "HeavySprintAttack")
            {
                Debug.Log("ANIMEVENT: Heavy Sprint Attack!");
                if (playerInventory.equippedWeapon == playerInventory.emptyWeapon)
                {
                    // Disable Sword Colliders
                    venusWeaponCollider.enabled = false;
                    marsWeaponCollider.enabled = false;
                    // Enable Fist Colliders
                    venusRightFistCollider.enabled = true;
                    marsRightFistCollider.enabled = true;
                    // Set Collider Modifier
                    venusRightFistCollider.heavySprintAttack = true;
                    marsRightFistCollider.heavySprintAttack = true;
                }
                // Armed
                else
                {
                    // Enable Sword Colliders
                    venusWeaponCollider.enabled = true;
                    marsWeaponCollider.enabled = true;
                    // Disable Fist Colliders
                    venusRightFistCollider.enabled = false;
                    marsRightFistCollider.enabled = false;
                    // Set Collider Modifier
                    marsWeaponCollider.heavySprintAttack = true;
                    venusWeaponCollider.heavySprintAttack = true;
                }
            }
            else if (type == "HeavyCombo")
            {
                Debug.Log("ANIMEVENT: Heavy Attack!");
                marsWeaponCollider.heavyCombo = true;
                venusWeaponCollider.heavyCombo = true;

                venusRightFistCollider.heavyCombo = true;
                marsRightFistCollider.heavyCombo = true;
            }
            else if (type == "HeavyComboSplit")
            {
                Debug.Log("ANIMEVENT: Heavy Split Attack!");
                marsWeaponCollider.heavyComboSplit = true;
                venusWeaponCollider.heavyComboSplit = true;

                venusRightFistCollider.heavyComboSplit = true;
                marsRightFistCollider.heavyComboSplit = true;
            }
            else if (type == "AdvancedCombo")
            {
                Debug.Log("ANIMEVENT: Advanced Combo Attack!");
                marsWeaponCollider.advancedCombo = true;
                venusWeaponCollider.advancedCombo = true;

                venusRightFistCollider.advancedCombo = true;
                marsRightFistCollider.advancedCombo = true;
            }
            if (type == "AdvancedComboSplit")
            {
                Debug.Log("ANIMEVENT: Advanced Combo Split (2) Attack!");
                marsWeaponCollider.advancedComboSplit = true;
                venusWeaponCollider.advancedComboSplit = true;

                venusRightFistCollider.advancedComboSplit = true;
                marsRightFistCollider.advancedComboSplit = true;
            }
            if (type == "UnarmedRight")
            {
                venusRightFistCollider.enabled = true;
                marsRightFistCollider.enabled = true;
            }
            else if (type == "UnarmedLeft")
            {
                venusLeftFistCollider.enabled = true;
                marsLeftFistCollider.enabled = true;
            }
            else if (type == "UnarmedHeavy")
            {
                venusKickCollider.enabled = true;
                marsKickCollider.enabled = true;
            }
            else if (type == "Shovel")
            {
                venusShovelDamageCollider.enabled = true;
                marsShovelDamageCollider.enabled = true;
            }
            else if(type == "Mounted")
            {
                if(!playerLocomotion.inCombat || playerInventory.equippedWeapon == playerInventory.emptyWeapon)
                {
                    // Disable Sword Colliders
                    venusWeaponCollider.enabled = false;
                    marsWeaponCollider.enabled = false;
                    // Enable Fist Colliders
                    venusRightFistCollider.enabled = true;
                    marsRightFistCollider.enabled = true;
                    // can add modifer for mounted combat
                }
                // Armed
                else
                {
                    // Enable Sword Colliders
                    venusWeaponCollider.enabled = true;
                    marsWeaponCollider.enabled = true;
                    // Disable Fist Colliders
                    venusRightFistCollider.enabled = false;
                    marsRightFistCollider.enabled = false;
                    // can add modifer for mounted combat
                }
            }
            else
            {
                // Unarmed
                if (playerInventory.equippedWeapon == playerInventory.emptyWeapon)
                {
                    // Disable Sword Colliders
                    venusWeaponCollider.enabled = false;
                    marsWeaponCollider.enabled = false;
                    // Enable Fist Colliders
                    venusRightFistCollider.enabled = true;
                    marsRightFistCollider.enabled = true;
                }
                // Armed
                else
                {
                    // Enable Sword Colliders
                    venusWeaponCollider.enabled = true;
                    marsWeaponCollider.enabled = true;
                    // Disable Fist Colliders
                    venusRightFistCollider.enabled = false;
                    marsRightFistCollider.enabled = false;
                }
            }

            if  (type != "Shovel")
            {
                PlayWeaponSwingAudio();
            }
        }

        public void DeactivateWeapon()
        {
            // This is called when the player takes Damage
            // Disables their weapon colliders,
            // however can also be used to reset any important flags that may have had their typical process interrupted
            venusWeaponCollider.enabled = false;
            marsWeaponCollider.enabled = false;
            venusRightFistCollider.enabled = false;
            marsRightFistCollider.enabled = false;
            venusLeftFistCollider.enabled = false;
            marsLeftFistCollider.enabled = false;
            venusKickCollider.enabled = false;
            marsKickCollider.enabled = false;
            venusShovelDamageCollider.enabled = false;
            marsShovelDamageCollider.enabled = false;

            DeactivateWeaponShield();

            //Reset flags:
            playerLocomotion.usingItem = false;
            inputHandler.isBlocking = false;
        }

        public void CancelAllAttacks()
        {
            DeactivateWeapon();
            DeactivateWeaponShield();
            animatorHandler.anim.SetBool("isAttacking", false);
            inputHandler.isAttacking = false;
            inputHandler.canQueueAttack = true;
            inputHandler.comboFlag = false;
            playerLocomotion.attackCombo = 1;

        }

        public void ActivateWeaponShield(string type = "Normal")
        {
            DeactivateWeaponShield();
            
            if(playerLocomotion.inCombat)
            {
                venusWeaponShieldCollider.enabled = true;
                marsWeaponShieldCollider.enabled = true;
            }
            else
            {
                venusLeftFistCollider.enabled = true;
                marsLeftFistCollider.enabled = true;
            }

            if (type == "NormalSplit")
            {
                Debug.Log("ANIMEVENT: Heavy Attack!");
                marsWeaponCollider.normalSplit = true;
                venusWeaponCollider.normalSplit = true;

                venusLeftFistCollider.normalSplit = true;
                marsLeftFistCollider.normalSplit = true;
            }
            else if (type == "HeavyCombo")
            {
                Debug.Log("ANIMEVENT: Heavy Attack!");
                marsWeaponCollider.heavyCombo = true;
                venusWeaponCollider.heavyCombo = true;

                venusLeftFistCollider.heavyCombo = true;
                marsLeftFistCollider.heavyCombo = true;
            }
            else if (type == "HeavyComboSplit")
            {
                Debug.Log("ANIMEVENT: Heavy Attack!");
                marsWeaponCollider.heavyComboSplit = true;
                venusWeaponCollider.heavyComboSplit = true;

                venusLeftFistCollider.heavyComboSplit = true;
                marsLeftFistCollider.heavyComboSplit = true;
            }
            else if (type == "AdvancedComboSplit")
            {
                Debug.Log("ANIMEVENT: Advanced Combo Attack!");
                marsWeaponCollider.advancedCombo = true;
                venusWeaponCollider.advancedCombo = true;

                venusLeftFistCollider.advancedCombo = true;
                marsLeftFistCollider.advancedCombo = true;
            }
            else if (type == "AdvancedComboSplit")
            {
                Debug.Log("ANIMEVENT: Advanced Combo Split Attack!");
                marsWeaponCollider.advancedComboSplit = true;
                venusWeaponCollider.advancedComboSplit = true;

                venusLeftFistCollider.advancedComboSplit = true;
                marsLeftFistCollider.advancedComboSplit = true;
            }
            PlayWeaponSwingAudio();
        }

        public void DeactivateWeaponShield()
        {
            venusWeaponShieldCollider.enabled = false;
            marsWeaponShieldCollider.enabled = false;

            venusLeftFistCollider.enabled = false;
            marsLeftFistCollider.enabled = false;

            //Reset flags:
            playerLocomotion.usingItem = false;
            inputHandler.isBlocking = false;
        }

        public void DodgeStartFlag()
        {
            // Reset any flags that may have been interrupted by dodge
            inputHandler.isAttacking = false;
            inputHandler.canQueueAttack = false;
            animatorHandler.anim.SetBool("isAttacking", false);
        }

        public void DodgeFramesStart()
        {
            ToggleCollisionsWithEnemyProjectiles(true);
            playerStats.iFrames = true;
            if (ScenePersistentPlayerObject.instance.debugShowColliders) dodgeDebugSphere?.SetActive(true);
        }

        public void DodgeFramesEnd()
        {
            ToggleCollisionsWithEnemyProjectiles(false);
            playerStats.iFrames = false;
            dodgeDebugSphere?.SetActive(false);
        }

        public void DodgeEndFlag()
        {
            //Debug.Log("Root motion is: " + animatorHandler.anim.applyRootMotion);
            playerLocomotion.isDodging = false;

            if (!playerLocomotion.isCrouching)
            {
                if (playerLocomotion.CanStand())
                {
                    playerLocomotion.SetColliderHeight(false);
                }
                else
                {
                    playerLocomotion.AttemptToToggleCrouch(true, false);
                }
            }
        }

        public void CastSpellProj()
        {
            int i = playerLocomotion.currentSpellIndex;
            playerStats.SpendSpellMana(playerInventory.equippedSpells[i].manaCost);
            var spell = Instantiate(projectiles[playerInventory.equippedSpells[i].spell_ID], projectileSpawnPoint.transform.position, projectileSpawnPoint.transform.rotation);
            
            if (inputHandler.lockedOn)
            {
                Transform target = cameraHandler.currentLockOnTarget.lockOnPoint;

                Vector3 targetDirection = target.position - spell.transform.position;

                spell.transform.LookAt(spell.transform.position + targetDirection.normalized);
            }

            spell.GetComponent<HS_ProjectileMover>().SetSpellData(
                playerInventory.equippedSpells[i].baseDamage, 
                playerInventory.equippedSpells[i].spell_ID, 
                playerInventory.equippedSpells[i].elementType,
                playerInventory.equippedSpells[i].splashSize);

            // Dark Arcane Bolt
            if(spell.GetComponent<HS_ProjectileMover>().spell_ID == 5)
            {
                float healthPercentage;
                float damageMultiplier;
                switch (playerStats.GetSkillRankByID(20))
                {
                    case 2:
                        healthPercentage = 0.25f;
                        damageMultiplier = 1.5f;
                        break;
                    case 3:
                        healthPercentage = 0.30f;
                        damageMultiplier = 1.7f;
                        break;
                    default:
                        healthPercentage = 0.20f;
                        damageMultiplier = 1.3f;
                        break;
                }

                float requiredHP = playerStats.maxHealth * healthPercentage;
                if(playerStats.currentHealth > requiredHP)
                {
                    // Remove health from player
                    playerStats.currentHealth -= requiredHP;
                    playerStats.healthBar.SetStat(playerStats.currentHealth);
                    playerStats.CheckDeath();
                    // Increase spell damage
                    spell.GetComponent<HS_ProjectileMover>().spellBaseDamage *= damageMultiplier;
                }

            }
        }

        public void AttackFinished()
        {
            inputHandler.isAttacking = false;
        }

        public void CastSpellProjStartingEffect()
        {
            int i = playerLocomotion.currentSpellIndex;

            switch (playerInventory.equippedSpells[i].itemName)
            {
                case "Fireball":
                    playerAudioManager.PlayFireballCast();
                    break;
                case "Zapshock":
                    playerAudioManager.PlayZapshockCast();
                    break;
                case "Frostshard":
                    playerAudioManager.PlayFrostshardCast();
                    break;
                default:
                    playerAudioManager.PlayGeneralSpell();
                    break;
            }
        }

        public void CastSpellSelfStartingEffect()
        {
            int i = playerLocomotion.currentSpellIndex;

            switch (playerInventory.equippedSpells[i].itemName)
            {
                case "Mend":
                    playerAudioManager.PlayMendAudio();
                    mendStartingEffect.Play();
                    break;
                case "Rejuvenate":
                    playerAudioManager.PlayRejuvenateAudio();
                    rejuvenateStartingEffect.Play();
                    rejuvenateEffect.Play();
                    break;
                case "Stormguard":
                    playerAudioManager.PlayStormguardAudio();
                    stormguardEffect.Play();
                    stormguardStartingEffect.Play();
                    break;
                case "Arcana Lumina":
                    mendStartingEffect.Play();
                    playerAudioManager.PlayArcanaLuminaAudio();
                    break;
                default:
                    Debug.LogError("Invalid Self Cast Spell name detected: " + playerInventory.equippedSpells[i].itemName);
                    break;
            }
        }

        public void CastSpellSelf()
        {
            int i = playerLocomotion.currentSpellIndex;
            playerStats.SpendSpellMana(playerInventory.equippedSpells[i].manaCost);

            float healingPotency = playerInventory.equippedSpells[i].baseDamage;
            float duration = playerInventory.equippedSpells[i].duration;

            switch (playerInventory.equippedSpells[i].itemName)
            {
                case "Mend":
                    mendHealEffect.Play();
                    playerStats.RestoreHealth(playerInventory.equippedSpells[i].baseDamage, true);
                    break;
                case "Rejuvenate":
                    // If not currently rejuvenating, start
                    if(rejuvenateTimer <= 0)
                    {
                        if (rejuvenateCoroutine != null) rejuvenateCoroutine = null;
                        rejuvenateCoroutine = StartCoroutine(Rejuvenate(healingPotency, duration));
                    }
                    // Otherwise, just reset the timer
                    else
                    {
                        rejuvenateTimer = 0;
                        lastHealTime = 0;
                    }
                    break;
                case "Stormguard":
                    // If stormguard is not active, start ot up
                    if (stormguardTimer <= 0)
                    {
                        if (stormguardCoroutine != null) stormguardCoroutine = null;
                        stormguardCoroutine = StartCoroutine(Stormguard(duration));
                    }
                    // Otherwise, just reset the timer
                    else
                    {
                        stormguardTimer = 0;
                    }
                    break;
                case "Arcana Lumina":
                    // If Arcana Lumina is not active, start ot up
                    if (arcanaLuminaTimer <= 0)
                    {
                        if (arcanaLuminaCoroutine != null) arcanaLuminaCoroutine = null;
                        arcanaLuminaCoroutine = StartCoroutine(ArcanaLumina(duration));
                    }
                    // Otherwise, just reset the timer
                    else
                    {
                        arcanaLuminaTimer = 0;
                        arcanaLumina.time = 0f;
                        arcanaLuminaRays.time = 0f;
                        arcanaLuminaSmallStars.time = 0f;
                    }
                    break;
                default:
                    Debug.LogError("Invalid Self Cast Spell name detected: " + playerInventory.equippedSpells[i].itemName);
                    break;
            }
        }

        public void CastSpellAOE()
        {
            int i = playerLocomotion.currentSpellIndex;
            playerStats.SpendSpellMana(playerInventory.equippedSpells[i].manaCost);

            Transform enemyLocation;
            Vector3 targetPosition;
            Quaternion targetRotation;

            // Determine enemy location, or used locked on location
            if (cameraHandler.currentLockOnTarget != null)
            {
                enemyLocation = cameraHandler.currentLockOnTarget.transform;
            }
            else
            {
                // If we have a target, set enemy locaion else use default
                if (cameraHandler.currentLockOnTarget != null)
                {
                    enemyLocation = cameraHandler.currentLockOnTarget.transform;
                }
                else
                {
                    enemyLocation = defaultAOELocation;
                }
            }

            targetPosition = enemyLocation.position;
            targetRotation = enemyLocation.rotation;

            // Perform a single raycast            
            Vector3 raycastOffset = new Vector3(0f, 0.1f, 0f);
            if (Physics.Raycast(enemyLocation.position + raycastOffset, Vector3.down, out RaycastHit hit, 50))
            {
                // Check if the hit object's tag is within the accepted tags
                if (hit.collider.tag.StartsWith("Surface_") || acceptedTags.Contains(hit.collider.tag))
                {
                    // Instantiate at the hit point position
                    // Also apply a small offset vertically so planes are less likely to be clipped
                    targetPosition = hit.point + new Vector3(0f, 0.1f, 0f);
                }
                else
                {
                    Debug.Log("Hit object does not have an accepted tag for Cast Spell AOE.");
                }
            }
            else
            {
                Debug.Log("Raycast did not hit anything for Cast Spell AOE.");
            }


            switch (playerInventory.equippedSpells[i].itemName)
            {
                case "Meteor":
                    meteorEffect.Play();
                    var meteor = Instantiate(meteorPrefab, targetPosition, targetRotation);
                    meteor.transform.localScale *= playerStats.skills[22].CurrentEffect();
                    meteor.GetComponent<AoESpellDamageCollider>().SetSpellData(
                        playerInventory.equippedSpells[i].baseDamage,
                        playerInventory.equippedSpells[i].spell_ID,
                        playerInventory.equippedSpells[i].elementType);
                    break;

                case "Glacial Burst":
                    // Initial Hit (Spawns at enemy position)
                    
                    var glacialBurstInitial = Instantiate(glacialBurstInitialHitPrefab, enemyLocation.position, enemyLocation.rotation);
                    glacialBurstInitial.GetComponent<AoESpellDamageCollider>().SetSpellData(
                        playerInventory.equippedSpells[i].baseDamage,
                        playerInventory.equippedSpells[i].spell_ID,
                        playerInventory.equippedSpells[i].elementType);
                    // AoE (Spawns below enemy)

                    var glacialBurst = Instantiate(glacialBurstPrefab, targetPosition, Quaternion.identity);
                    glacialBurst.GetComponent<GlacialShatter>().SetSpellData(
                        playerInventory.equippedSpells[i].baseDamage,
                        playerInventory.equippedSpells[i].spell_ID,
                        playerInventory.equippedSpells[i].elementType);
                    Destroy(glacialBurstInitial, 1.0f);
                    break;

                default:
                    Debug.LogError("Invalid Self Cast Spell name detected: " + playerInventory.equippedSpells[i].itemName);
                    break;
            }
        }

        public void CastSpellAOEStartingEffect()
        {
            int i = playerLocomotion.currentSpellIndex;

            if(playerInventory.equippedSpells[i].spell_ID == 10)
            {
                // Mystic Infusion
                // Add Audio/ Effects here
                return;
            }

            // Force lock on
            cameraHandler.HandleLockOn();
            if (cameraHandler.nearestLockOnTarget != null)
            {
                cameraHandler.currentLockOnTarget = cameraHandler.nearestLockOnTarget;
                inputHandler.lockedOn = true;
                inputHandler.targetIconPosition.StartUpdatingIconPosition();
            }

            switch (playerInventory.equippedSpells[i].itemName)
            {
                case "Meteor":
                    // Play Audio for Spell
                    playerAudioManager.PlayMeteorAudio();
                    // Play Starting effect, the magic circle
                    meteorStartingEffect.Play();
                    // Set hand effect (glow) to matching color
                    handEffectFlash.startColor = meteorHandEffectColor;
                    // Play effect in character's hand
                    handEffectAOE.Play();
                    break;

                case "Glacial Burst":
                    // Play Audio for Spell
                    playerAudioManager.PlayGlacialBurstAudio();
                    // Play Starting effect, the magic circle
                    glacialBurstStartingEffect.Play();
                    // Set hand effect (glow) to matching color
                    handEffectFlash.startColor = glacialBurstHandEffectColor;
                    // Play effect in character's hand
                    handEffectAOE.Play();
                    break;

                default:
                    Debug.LogError("Invalid Self Cast Spell name detected: " + playerInventory.equippedSpells[i].itemName);
                    break;
            }

        }

        public void PlayerStaggeredStart()
        {
            //Debug.Log("Stagger Start");
            isStaggered = true;
            inputHandler.canQueueAttack = false;
            inputHandler.isAttacking = false;
            animatorHandler.anim.SetBool("isAttacking", false);
            DodgeFramesEnd();
        }

        public void PlayerStaggeredEnd()
        {
            //Debug.Log("Stagger End");
            isStaggered = false;
            //playerLocomotion.canMoveAfterAttack = true;
        }

        public void CancelAttackInput()
        {
            inputHandler.attackInput = false;
            inputHandler.heavyAttackInput = false;

            playerLocomotion.isHeavyAttacking = false;
        }

        public void AttackEnd()
        {
            CancelAttackInput();
            inputHandler.isAttacking = false;
            animatorHandler.anim.SetBool("isAttacking", false);
        }

        public void AirAttackEnd()
        {
            //Debug.Log("AirAttackEnd was called");
            animatorHandler.anim.SetBool("airAttack", false);
            inputHandler.isAttacking = false;
            animatorHandler.anim.SetBool("isAttacking", false);
            playerLocomotion.attackCombo = 1;
            inputHandler.canQueueAttack = false;
        }

        void ImpulseForce(float scale = 1.0f) // Used for aerial attacks to help keep player airborne
        {
            playerLocomotion.ImpulseOnAirAttack(scale);
        }

        public void PlaySheatheAudio()
        {
            playerAudioManager.PlaySheatheAudio();
        }

        public void PlayUnsheatheAudio()
        {
            playerAudioManager.PlayUnsheatheAudio();
        }

        public void PlayShieldEquipAudio()
        {
            playerAudioManager.PlayShieldEquipAudio();
        }

        public void PlayShieldUnequipAudio()
        {
            playerAudioManager.PlayShieldUnequipAudio();
        }

        public void MoveSword(string combat)
        {
            //Debug.Log("Set Sword called");
            if (combat == "true")
            {
                animatorHandler.MoveSword("hands");
            }
            else
            {
                animatorHandler.MoveSword("back");
            }
        }

        public void MoveShield(string combat)
        {
            //Debug.Log("Set Shield called");
            if (combat == "true")
            {
                animatorHandler.MoveShield("hands");
            }
            else
            {
                animatorHandler.MoveShield("back");
            }
        }

        public void FinishSheathe()
        {
            inputHandler.isPerformingAction = false;
        }

        public void ForceUnsheathe()
        {
            playerLocomotion.InstantSheatheUnsheathe(false);
        }

        public void SetFinalPosition()
        {
            //Debug.Log("SFP called");
            playerLocomotion.PlacePlayerOnMount();
        }

        public void FinishDismount()
        {
            playerLocomotion.FinishDismount();
        }

        public void PlayClimbAudio()
        {
            playerAudioManager.PlayClimbAudio();
        }

        // Effects:

        void PlayParticleSystemsRecursively(Transform parent)
        {

            itemEffect.SetActive(true);

            foreach (Transform child in parent)
            {
                ParticleSystem particleSystem = child.GetComponent<ParticleSystem>();
                if (particleSystem != null)
                {
                    particleSystem.Play();
                }

                PlayParticleSystemsRecursively(child);
            }
        }

        void PlayParticleSystemsOnObject()
        {
            PlayParticleSystemsRecursively(itemEffect.transform);
        }

        void ChargeAttackDone()
        {
            playerLocomotion.isHeavyAttacking = false;
        }

        void CombatToIdle()
        {
            animatorHandler.anim.SetBool("CombatToIdle", true);
        }

        void IdleToCombat()
        {
            animatorHandler.anim.SetBool("IdleToCombat", true);
        }

        // Audios

        public void PlayPlungeAttackAudio()
        {
            playerAudioManager.PlayPlungeAttackAudio();
        }

        public void PlayFootstepClip(string foot)
        {
            if(foot == "left")
            {
                playerFootstepSoundManager.CheckTextureAndPlay(true, false);
            }
            else
            {
                playerFootstepSoundManager.CheckTextureAndPlay(false, false);
            }
        }

        // Spell Coroutines
        private IEnumerator Rejuvenate(float healingPotency, float duration)
        {
            Image progress = rejuvenateBuffIcon.GetComponent<Image>();
            progress.fillAmount = 1f;
            rejuvenateBuffIcon.transform.SetAsFirstSibling();
            rejuvenateBuffIcon.SetActive(true);

            float healInterval = 2.9f;
            if (playerStats.GetSkillRankByID(21) >= 3) healInterval *= 0.9f;
            Debug.Log("healInterval: " + healInterval);

            rejuvenateTimer = 0;
            lastHealTime = 0;

            while (rejuvenateTimer <= duration)
            {
                yield return null;

                rejuvenateTimer += Time.deltaTime;

                progress.fillAmount = Mathf.Clamp01(1 - (rejuvenateTimer / duration));

                if (rejuvenateTimer - lastHealTime >= healInterval)
                {
                    // Call the Heal function with the specified potency
                    playerStats.RestoreHealth(healingPotency, true);

                    playerAudioManager.PlayRejuvenateTickAudio();
                    rejuvenateTickEffect.Play();

                    // Update the last healing timestamp
                    lastHealTime = rejuvenateTimer;
                }
            }

            rejuvenateTimer = 0;
            lastHealTime = 0;
            progress.fillAmount = 0f;
            rejuvenateBuffIcon.SetActive(false);

            rejuvenateCoroutine = null;
        }

        public void StopRejuvenate()
        {
            if (rejuvenateCoroutine != null)
            {
                StopCoroutine(rejuvenateCoroutine);
                rejuvenateCoroutine = null;
                rejuvenateTimer = 0;
                lastHealTime = 0;
                rejuvenateBuffIcon.SetActive(false);
            }
        }

        private IEnumerator Stormguard(float duration)
        {
            Image progress = stormguardBuffIcon.GetComponent<Image>();
            progress.fillAmount = 1f;
            stormguardBuffIcon.transform.SetAsFirstSibling();
            stormguardBuffIcon.SetActive(true);

            stormguardTimer = 0;
            stormguardActive = true;
            selfEffectsManager.EnableShockShield();

            while (stormguardTimer < duration)
            {
                stormguardTimer += Time.deltaTime;
                progress.fillAmount = Mathf.Clamp01(1 - (stormguardTimer / duration));
                yield return null; // Wait for the next frame
            }

            stormguardTimer = 0;
            stormguardActive = false;
            selfEffectsManager.DisableShockShield();
            progress.fillAmount = 0f;
            stormguardBuffIcon.SetActive(false);

            stormguardCoroutine = null;
        }

        public void StopStormguard()
        {
            if (stormguardCoroutine != null)
            {
                StopCoroutine(stormguardCoroutine);
                stormguardCoroutine = null;
                stormguardTimer = 0;
                stormguardActive = false;
                selfEffectsManager.DisableShockShield();
                stormguardBuffIcon.SetActive(false);
            }
        }

        private IEnumerator ArcanaLumina(float duration)
        {
            // Activate Light
            var main = arcanaLumina.main;
            main.duration = duration;
            var main2 = arcanaLuminaRays.main;
            main2.duration = duration;
            var main3 = arcanaLuminaSmallStars.main;
            main3.duration = duration;

            arcanaLumina.gameObject.SetActive(true);
            // Control Buff Icon
            Image progress = arcanaLuminaBuffIcon.GetComponent<Image>();
            progress.fillAmount = 1f;
            arcanaLuminaBuffIcon.transform.SetAsFirstSibling();
            arcanaLuminaBuffIcon.SetActive(true);

            arcanaLuminaTimer = 0;

            while (arcanaLuminaTimer < duration)
            {
                arcanaLuminaTimer += Time.deltaTime;
                progress.fillAmount = Mathf.Clamp01(1 - (arcanaLuminaTimer / duration));
                yield return null; // Wait for the next frame
            }

            arcanaLuminaTimer = 0;
            progress.fillAmount = 0f;
            arcanaLuminaBuffIcon.SetActive(false);

            arcanaLuminaCoroutine = null;
        }

        public void StopArcanaLumina()
        {
            if(arcanaLuminaCoroutine != null)
            {
                StopCoroutine(arcanaLuminaCoroutine);
                arcanaLuminaCoroutine = null;
                arcanaLumina.gameObject.SetActive(false);
                arcanaLuminaTimer = 0f;
                arcanaLuminaBuffIcon.SetActive(false);
            }
        }

        private IEnumerator InfusionTimer()
        {
            Image progress = infusionBuffIcon.GetComponent<Image>();
            progress.fillAmount = 1.0f;
            infusionBuffIcon.transform.SetAsFirstSibling();
            infusionBuffIcon.SetActive(true);

            infusionTimer = infusionDuration;

            while (infusionTimer > 0)
            {
                float timeElapsed = Time.deltaTime;
                infusionTimer -= timeElapsed;

                float fillAmount = Mathf.Clamp01(infusionTimer / infusionDuration);
                progress.fillAmount = fillAmount;

                yield return null; // Wait for the next frame
            }

            // Ensure the fill amount is set to 0 when the timer ends
            progress.fillAmount = 0.0f;
            infusionBuffIcon.SetActive(false);

            // Stop all infusions
            fireInfusion.GetComponent<ParticleSystem>().Stop();
            iceInfusion.GetComponent<ParticleSystem>().Stop();
            shockInfusion.GetComponent<ParticleSystem>().Stop();
            infusedFire = false;
            infusedIce = false;
            infusedShock = false;

            infusionCoroutine = null;
        }

        public void InfusionWindowStart()
        {
            Debug.Log("InfusionWindowStart()");

            // Allow player to switch elements
            canSwitchInfusionType = true;

            // Stop current infusion timer/effects
            if (infusionCoroutine != null)
            {
                StopCoroutine(infusionCoroutine);
                infusionCoroutine = null;
            }

            // Reset all infusions
            // Start Fire as default infusion

            fireInfusion.SetActive(true);
            fireInfusionIgnite.Play();
            fireInfusionIgniteAudio.Play();
            iceInfusion.GetComponent<ParticleSystem>().Stop();
            shockInfusion.GetComponent<ParticleSystem>().Stop();

            // How this works: Stopping a particle system will automatically disable the gameobject due to the script on the gameobject
            // Enabling the gameobject triggers the particle system to play, as well as the animated lighting (if it has one)

            infusedFire = true;
            infusedIce = false;
            infusedShock = false;

            //Enable buff Icon
            Image progress = infusionBuffIcon.GetComponent<Image>();
            progress.fillAmount = 1.0f;
            infusionTypeIcon.sprite = infusionSprites[0];
            infusionBuffIcon.SetActive(true);

            int i = playerLocomotion.currentSpellIndex;
            playerStats.SpendSpellMana(playerInventory.equippedSpells[i].manaCost);
        }

        public void InfusionWindowEnd()
        {
            // Reset attack flag to allow actions
            inputHandler.isAttacking = false;
            // Prevent player from switching elements beyond this point
            canSwitchInfusionType = false;
            // Begin timer for infusion duration
            infusionCoroutine = StartCoroutine(InfusionTimer());

            Debug.Log("InfusionWindowEnd()");
        }

        public void SwitchInfusionElement()
        {
            // Reset animation to allow time to switch
            Image progress = infusionBuffIcon.GetComponent<Image>();

            animatorHandler.anim.SetTrigger("resetInfusion");

            if (infusedFire)
            {
                fireInfusion.GetComponent<ParticleSystem>().Stop();
                fireInfusion.SetActive(false);
                iceInfusion.GetComponent<ParticleSystem>().Play();
                iceInfusion.SetActive(true);
                iceInfusionIgniteAudio.Play();
                shockInfusion.GetComponent<ParticleSystem>().Stop();

                infusedFire = false;
                infusedIce = true;
                infusedShock = false;

                infusionTypeIcon.sprite = infusionSprites[1];
            }
            else if (infusedIce)
            {
                fireInfusion.GetComponent<ParticleSystem>().Stop();
                iceInfusion.GetComponent<ParticleSystem>().Stop();
                shockInfusion.GetComponent<ParticleSystem>().Play();
                shockInfusion.SetActive(true);
                shockInfusionIgniteAudio.Play();

                infusedFire = false;
                infusedIce = false;
                infusedShock = true;

                infusionTypeIcon.sprite = infusionSprites[2];
            }
            else if (infusedShock)
            {
                fireInfusion.GetComponent<ParticleSystem>().Play();
                fireInfusion.SetActive(true);
                fireInfusionIgniteAudio.Play();
                iceInfusion.GetComponent<ParticleSystem>().Stop();
                shockInfusion.GetComponent<ParticleSystem>().Stop();

                infusedFire = true;
                infusedIce = false;
                infusedShock = false;

                infusionTypeIcon.sprite = infusionSprites[0];
            }
            else
            {
                Debug.LogError("Unexpected Value for Infusion Type");
            }
        }

        public void StopInfusion()
        {
            if (infusionCoroutine != null)
            {
                StopCoroutine(infusionCoroutine);
                infusionCoroutine = null;
            }

            infusionBuffIcon.SetActive(false);

            // Stop all infusions
            infusionTimer = 0f;
            fireInfusion.GetComponent<ParticleSystem>().Stop();
            iceInfusion.GetComponent<ParticleSystem>().Stop();
            shockInfusion.GetComponent<ParticleSystem>().Stop();
            infusedFire = false;
            infusedIce = false;
            infusedShock = false;
        }

        public void NotPerformingAction()
        {
            inputHandler.isPerformingAction = false;
        }

        public void CanSwitchInfusionTrue()
        {
            canSwitchInfusionType = true;
        }

        public void CanSwitchInfusionFalse()
        {
            canSwitchInfusionType = false;
        }

        public void StartSpiderVenom()
        {
            playerStats.poisonCharges = playerStats.startingPoisonCharges;
            Image progress = spiderVenomBuffIcon.GetComponent<Image>();
            progress.fillAmount = 1f;
            spiderVenomBuffIcon.transform.SetAsFirstSibling();
            spiderVenomBuffIcon.SetActive(true);
        }

        public void UpdateSpiderVenomBuff()
        {
            Image progress = spiderVenomBuffIcon.GetComponent<Image>();
            progress.fillAmount = (float)playerStats.poisonCharges / (float)playerStats.startingPoisonCharges;
        }

        public void StopSpiderVenom()
        {
            Image progress = spiderVenomBuffIcon.GetComponent<Image>();
            progress.fillAmount = 1f;
            playerStats.poisonCharges = 0;
            spiderVenomBuffIcon.transform.SetAsFirstSibling();
            spiderVenomBuffIcon.SetActive(false);
        }

        // Swimming

        public void StartRipples()
        {
            playerLocomotion.StartRipples();
        }

        public void StopRipples()
        {
            playerLocomotion.StopRipples();
        }

        // Climbing

        public void StartClimbing()
        {
            playerLocomotion.isClimbing = true;
            playerLocomotion.rigidbody.isKinematic = true;
            climbManager.SetPlayerPosition();
        }

        public void StopClimbing()
        {
            playerLocomotion.isClimbing = false;
            playerLocomotion.rigidbody.isKinematic = false;
        }

        // Quests
        void StartQuest(string s)
        {
            int questNumber;
            string trackQuestString;
            bool trackQuest = false;

            // Had to combine arguments into one string because Animation events do not support multiple parameters
            ParseStartQuestEvent(s, out questNumber, out trackQuestString);

            if (trackQuestString == "tracked" || trackQuestString == "Tracked") 
            {
                trackQuest = true;
            }

            Debug.Log($"Quest Number: {questNumber}, Track quest on start: {trackQuest}");

            questManager.StartQuest(questNumber, trackQuest);
        }

        private void ParseStartQuestEvent(string combined, out int i, out string s)
        {
            // Find the first non-digit character index
            int firstNonDigitIndex = Regex.Match(combined, @"\D").Index;

            // Extract the integer and string parts
            string intPart = combined.Substring(0, firstNonDigitIndex);
            string stringPart = combined.Substring(firstNonDigitIndex);

            // Convert the integer part to an int
            i = int.Parse(intPart);
            s = stringPart;
        }

        public void StartMovingObject()
        {
            Debug.Log("Object moving started");
            playerLocomotion.StartMovingObject();
        }

        public void StopMovingObject()
        {
            playerLocomotion.StopMovingObject();
            Debug.Log("Object moving stopped");
        }

        public void StartPushTimer()
        {
            playerLocomotion.StartPushTimer();
        }

        // DEATH

        public void StandUpDone()
        {
            playerStats.dead = false;
            ScenePersistentPlayerObject.instance.saveManager.Autosave();
        }

        // Timer that starts on death to ensure death screen is activated, even if player gets stuck or doesn't trigger landing animation for any reason
        public void DeathBackupTimer()
        {
            if(deathBackupTimerCoroutine != null)
            {
                StopCoroutine(deathBackupTimerCoroutine);
                deathBackupTimerCoroutine = null;
            }
            deathBackupTimerCoroutine = StartCoroutine(DeathBackupTimerCoroutine());
        }

        private IEnumerator DeathBackupTimerCoroutine()
        {
            yield return new WaitForSeconds(5f);
            Dead();
            deathBackupTimerCoroutine = null;
        }

        public void Dead()
        {
            if(deathBackupTimerCoroutine != null)
            {
                StopCoroutine(deathBackupTimerCoroutine);
                deathBackupTimerCoroutine = null;
            }

            foreach(GameObject obj in hideOnDeath)
            {
                obj.SetActive(false);
            }

            deathCoroutine = StartCoroutine(DeathCoroutine());
        }

        private IEnumerator DeathCoroutine()
        {
            // Wait before starting the fade
            yield return new WaitForSeconds(1.0f);

            fadeCoroutine = StartCoroutine(FadeDeathScreenCoroutine());

            // Wait before starting slow time
            yield return new WaitForSeconds(2.0f);

            timeScaleCoroutine = StartCoroutine(TimeScaleCoroutine());

            // Wait before allowing respawn
            yield return new WaitForSeconds(2.0f);
            
            controllerUIManager.SetAllInactive();
            controlsHUD.SetActive(true);
            controllerUIManager.SetSelectText("Respawn");
            playerStats.canRespawn = true;

            deathCoroutine = null;
        }

        private IEnumerator FadeDeathScreenCoroutine()
        {
            // Start all elements at transparent
            deathScreen.color = transparentColor;
            deathVignette.color = transparentColor;
            deathText.color = transparentColor;

            // Ensure the death screen is active
            deathScreenObj.SetActive(true);

            // Fade duration and timer
            float fadeDuration = 3.0f;
            float fadeElapsed = 0f;

            // Fade the alphas over time
            while (fadeElapsed < fadeDuration)
            {
                fadeElapsed += Time.deltaTime;
                float t = fadeElapsed / fadeDuration;

                // Lerp colors towards the target colors
                deathScreen.color = Color.Lerp(transparentColor, targetColorDeathScreen, t);
                deathVignette.color = Color.Lerp(transparentColor, targetColorDeathVignette, t);
                deathText.color = Color.Lerp(transparentColor, targetColorDeathText, t);

                //Debug.Log("Fade Coroutine elapsed time = " + fadeElapsed);

                yield return null;
            }

            // Ensure final color values are set after the loop
            deathScreen.color = targetColorDeathScreen;
            deathVignette.color = targetColorDeathVignette;
            deathText.color = targetColorDeathText;

            fadeCoroutine = null;
        }

        private IEnumerator TimeScaleCoroutine()
        {
            // Time scale adjustment variables
            float timeScaleFadeDuration = 3.0f;  // How long it takes to reduce time scale
            float initialTimeScale = Time.timeScale;
            float minimumTimeScale = 0.1f;
            float timeScaleElapsed = 0f;

            // Second loop to reduce the time scale over its own duration using a curve
            while (timeScaleElapsed < timeScaleFadeDuration)
            {
                timeScaleElapsed += Time.deltaTime;
                float t = timeScaleElapsed / timeScaleFadeDuration;

                // Evaluate the curve at this point (t) and apply to the time scale
                float curveValue = timeScaleCurve.Evaluate(t);
                Time.timeScale = Mathf.Lerp(initialTimeScale, minimumTimeScale, curveValue);
                //Debug.Log("Current time scale: " + Time.timeScale);

                yield return null;
            }

            // Ensure the final time scale is set to the minimum
            Time.timeScale = minimumTimeScale;

            timeScaleCoroutine = null;
        }

        public void CancelDeathCoroutines()
        {
            // Cancel screen fade coroutine
            if (deathCoroutine != null)
            {
                StopCoroutine(deathCoroutine);
                deathCoroutine = null;
            }
            // Cancel screen fade coroutine
            if (fadeCoroutine != null)
            {
                StopCoroutine(fadeCoroutine);
                fadeCoroutine = null;
            }
            // Ensure final color values are set after the loop
            deathScreen.color = targetColorDeathScreen;
            deathVignette.color = targetColorDeathVignette;
            deathText.color = targetColorDeathText;
            // Cancel time scale coroutine
            if (timeScaleCoroutine != null)
            {
                StopCoroutine(timeScaleCoroutine);
                timeScaleCoroutine = null;
            }
            // Ensure the final time scale is reset back to normal
            Time.timeScale = 1f;

        }

        public void UpdateEnemyProjectileCollidersList(Collider collider, bool add)
        {
            if (add)
            {
                if (!enemyProjectilesColliders.Contains(collider))
                {
                    enemyProjectilesColliders.Add(collider);

                    // If player is currently invulnerable, disable collisions for THIS projectile
                    if (playerStats.iFrames)
                    {
                        ToggleCollisionForProjectile(collider, true);
                    }
                }
            }
            else
            {
                if (enemyProjectilesColliders.Contains(collider))
                {
                    // Re-enable collisions for this projectile before removing it (good, if object pooled)
                    ToggleCollisionForProjectile(collider, false);

                    enemyProjectilesColliders.Remove(collider);
                }
            }
        }


        private void ToggleCollisionsWithEnemyProjectiles(bool ignore)
        {
            foreach (var projCol in enemyProjectilesColliders)
            {
                ToggleCollisionForProjectile(projCol, ignore);
            }
        }

        private void ToggleCollisionForProjectile(Collider projectileCol, bool ignore)
        {
            if (projectileCol == null) return;

            foreach (var playerCol in playerDamageReceivingColliders)
            {
                if (playerCol == null) continue;

                Physics.IgnoreCollision(projectileCol, playerCol, ignore);
            }
        }

        private List<Collider> GetDamageReceivingColliders(GameObject root, LayerMask layers)
        {
            List<Collider> result = new List<Collider>();

            // Get all colliders in the hierarchy
            Collider[] allColliders = root.GetComponentsInChildren<Collider>(true);

            foreach (var col in allColliders)
            {
                // Check if this collider's layer is in the LayerMask
                if (((1 << col.gameObject.layer) & layers) != 0)
                {
                    result.Add(col);
                }
            }

            return result;
        }

        public void DigStart()
        {
            shovel.SetActive(true);
            if (playerLocomotion.inCombat)
            {
                playerLocomotion.InstantSheatheUnsheathe(true);
                wasUnsheathedBeforeDiging = true;
            }
        }

        public void DigEnd()
        {
            shovel.SetActive(false);
            if (wasUnsheathedBeforeDiging)
            {
                playerLocomotion.InstantSheatheUnsheathe(false);
                wasUnsheathedBeforeDiging = false;
            }
        }

        public void DisplayDirtOnShovel(int show)
        {
            bool display = show > 0;
            if (display)
            {
                validDirtPile = InstantiateDirtPile();
                if (validDirtPile)
                {
                    dirtOnShovel.SetActive(true);
                    playerAudioManager.PlayShovelInDirt();
                }
                else
                {
                    animatorHandler.anim.SetTrigger("cancelDig");
                    //DigEnd();
                }
                
            }
            else
            {
                dirtOnShovel.SetActive(false);
            }
        }

        public bool InstantiateDirtPile()
        {
            // check for dig spot
            digSpot = null;
            // --- DIG SPOT OVERLAP CHECK ---
            Vector3 worldCenter = digCheckCollider.transform.TransformPoint(digCheckCollider.center);
            float radius = digCheckCollider.radius * Mathf.Max(
                digCheckCollider.transform.lossyScale.x,
                digCheckCollider.transform.lossyScale.y,
                digCheckCollider.transform.lossyScale.z
            );

            // Only check triggers on the DigSpot layer or tag
            Collider[] hits = Physics.OverlapSphere(worldCenter, radius, ~0, QueryTriggerInteraction.Collide);

            for (int i = 0; i < hits.Length; i++)
            {
                if (hits[i].CompareTag("DigSpot"))
                {
                    Debug.Log("Dug into DigSpot trigger: " + hits[i].gameObject.name);
                    digSpot = hits[i].GetComponent<DigSpot>();
                }
            }

            // no dig spot:
            float rayDistance = 0.9f;
            Vector3 spawnOffset = Vector3.zero;
            float minHeightOffset = -0.04f;
            float maxHeightOffset = 0.025f;

            if (Physics.Raycast(dirtRayOrigin.position, Vector3.down, out RaycastHit hit, rayDistance, validLayers))
            {
                if (digSpot == null)
                {
                    // TAG CHECK (unified keywords)
                    string tag = hit.collider.tag.ToLower();

                    for (int i = 0; i < invalidDigKeywords.Length; i++)
                    {
                        if (tag.Contains(invalidDigKeywords[i].ToLower()))
                        {
                            InstantiateShovelSparks();
                            return false;
                        }
                    }

                    // TERRAIN CHECK (unified keywords)
                    if (hit.collider is TerrainCollider)
                    {
                        int index = GetMainTexture(hit.point);
                        string texName = terrainData.splatPrototypes[index].texture.name.ToLower();

                        for (int i = 0; i < invalidDigKeywords.Length; i++)
                        {
                            if (texName.Contains(invalidDigKeywords[i].ToLower()))
                            {
                                InstantiateShovelSparks();
                                return false;
                            }
                        }
                    }
                }

                bool instantiateDirt = true;
                if (digSpot != null)
                {
                    instantiateDirt = digSpot.instantiateDirtPile || digSpot.alreadyDug;
                    digSpot.DigUpItems();
                    digSpot = null;
                }
                if (instantiateDirt)
                {
                    // VALID SURFACE -> SPAWN
                    Vector3 spawnPos = hit.point + spawnOffset;

                    float heightOffset = Random.Range(minHeightOffset, maxHeightOffset);
                    spawnPos += hit.normal * heightOffset;

                    Quaternion groundAlign = Quaternion.FromToRotation(Vector3.up, hit.normal);

                    float randomYaw = Random.Range(0f, 360f);
                    Quaternion randomRotation = Quaternion.AngleAxis(randomYaw, hit.normal);

                    Quaternion finalRot = randomRotation * groundAlign;

                    Instantiate(dirtPilePrefab, spawnPos, finalRot);
                }
                return true;
            }
            return false;
        }

        public void InstantiateShovelSparks()
        {
            if (shovelSparksPrefab == null)
                return;

            Instantiate(shovelSparksPrefab, shovelSparksSpawnPoint.position, Quaternion.identity);
            playerAudioManager.PlayShovelHitHardSurface();
        }


        private int GetMainTexture(Vector3 worldPos)
        {
            Vector3 terrainPos = worldPos - terrain.transform.position;

            int mapX = Mathf.FloorToInt(terrainPos.x / terrainData.size.x * terrainData.alphamapWidth);
            int mapZ = Mathf.FloorToInt(terrainPos.z / terrainData.size.z * terrainData.alphamapHeight);

            float[,,] splatmapData = terrainData.GetAlphamaps(mapX, mapZ, 1, 1);

            int maxIndex = 0;
            float maxMix = 0;

            for (int i = 0; i < splatmapData.GetLength(2); i++)
            {
                if (splatmapData[0, 0, i] > maxMix)
                {
                    maxIndex = i;
                    maxMix = splatmapData[0, 0, i];
                }
            }

            return maxIndex;
        }


        public void PlayShovelDirtThrow()
        {
            if (!validDirtPile) return;
            dirtThrow.gameObject.SetActive(true);
            dirtThrow.Play();
            playerAudioManager.PlayTossDirt();
        }
    }
}

