using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MalbersAnimations;
using UnityEngine.AI;

namespace etchebarren
{
    public class MountStats : MonoBehaviour
    {
        public static MountStats instance;

        [Header("Scripts")]
        public PlayerStats playerStats;
        public BlazeAI horseAI;
        public GameObject mountObject;
        public GameObject horseAIObject;
        public MountManager mountManager;
        public Animator mountAnim;
        public Animator horseAIAnim;
        public MountLocomotion mountLocomotion;
        public PlayerLocomotion playerLocomotion;
        public UI_StatBar healthBar;
        public UI_StatBar staminaBar;
        public UI_StatBar expBar;
        public StepsManager mountStepsManager;
        public StepsManager aiStepsManager;

        [Header("Stats and Data")]
        public string horseName = "Whisper";
        public int level = 1;
        private int maxLevel = 99;
        public int attack = 10;
        private int baseAttack = 10;
        public float maxHealth = 100f;
        private float baseHealth = 90f;
        public float health;
        public float maxStamina = 150;
        private float baseStamina = 145f;
        public float stamina;
        public bool horseArmorOwned = false;
        public bool horseHornOwned = false;

        //Exp and Stats Gained
        public float exp = 0f;
        public float baseExpCost = 100f;
        public float expPerHitTaken = 1f;
        public float expPerHitDealt = 1f;
        public float expPerMountedStep = 0.1f;

        public float healthGainedPerLevel = 10f;
        public int attackGainedPerLevel = 1;
        public float staminaGainedPerLevel = 5f;

        public ParticleSystem levelUpEffectMount;
        public ParticleSystem levelUpEffectAI;

        [Header("Stamina Regen Values")]
        private float staminaRegenTimer = 0; //per second
        public float staminaRegenDelay = 2.0f;
        private float staminaTickTimer = 0f;
        public float staminaRegenAmount = 1.0f;
        public float sprintStaminaCost = 3.0f;

        [Header("Health Regen Values")]
        private float healthRegenTimer = 0; //per second
        public float healthRegenDelay = 2.0f;
        private float healthTickTimer = 0f;
        public float healthRegenAmount = 1.0f;
        public bool canRegenHealth = true;
        public float healthRegenSleepAfterHit = 10f;
        private Coroutine regenSleep;

        [Header("Settings")]
        public float horseArmorDamageTakenMultiplier = 0.5f;
        public float horseHornDamageDealtMultiplier = 1.5f;

        [Header("Reins, Scale, Positioning")]
        public Transform leftRein;
        public Transform rightRein;

        private Transform originalReinsParent;
        private Vector3 leftReinStartingPos = new Vector3(-0.123f, -0.280f, 0.151f);
        private Vector3 rightReinStartingPos = new Vector3(-0.123f, -0.280f, -0.151f);

        public Transform venusLeftReinAttach;
        public Transform venusRightReinAttach;
        public Transform marsLeftReinAttach;
        public Transform marsRightReinAttach;

        public Vector3 venusMountScale = new Vector3(0.9f, 0.9f, 0.9f);
        public Vector3 marsMountScale = new Vector3(0.95f, 0.95f, 0.95f);

        public Transform saddleTransform;
        private Vector3 venusSittingPosition = new Vector3(-0.078f, 0.392f, 0.002f);
        private Vector3 marsSittingPosition = new Vector3(-0.142f, 0.350f, 0.002f);

        [Header("Death-related References")]
        public DamageTriggerCollider attackHitBox;

        [Header("Physical (Get) Hit Audio Settings")]
        public AudioSource audioSource;
        [Range(0.0f, 1.0f)]
        public float physicalHitVolume = 0.3f;             //Volume slider bar; set this between 0 and 1 in the Inspector.
        [Range(0.0f, 0.2f)]
        public float physicalHitVolumeVariance = 0.04f;    //Variance in volume levels per footstep; set this between 0.0 and 0.2 in the inspector. Default is 0.04f.
        public AudioClip[] physicalHitCollisionSounds;

        [Header("Flags for loading game")]
        public bool gameLoaded = false;

        void Awake()
        {
            if (instance == null)
            {
                instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        void Start()
        {
            originalReinsParent = leftRein.parent;

            if (gameLoaded)
            {
                gameLoaded = false;
                SetStatsBasedOnLevel(setCurrentToMax:false);
            }
            else
            {
                SetStatsBasedOnLevel(setCurrentToMax:true);
            }

            expBar.SetMaxStat(baseExpCost, false);
            expBar.SetStat(exp);
        }
        
        private void SetStatsBasedOnLevel(bool setCurrentToMax)
        {
            maxHealth = baseHealth + level * healthGainedPerLevel;
            maxStamina = baseStamina + level * staminaGainedPerLevel;
            attack = baseAttack + level * attackGainedPerLevel;

            if (setCurrentToMax)
            {
                health = maxHealth;
                stamina = maxStamina;
                healthBar.SetMaxStat(maxHealth, true);
                healthBar.SetStat(health);
                staminaBar.SetMaxStat(maxStamina, true);
                staminaBar.SetStat(stamina);
            }
            else
            {
                healthBar.SetMaxStat(maxHealth, false);
                staminaBar.SetMaxStat(maxStamina, false);
            }
        }

        public void GainExp(float expGained)
        {
            exp += expGained;
            
            while (exp >= baseExpCost && level < maxLevel)
            {
                //LEVEL UP
                exp -= baseExpCost;
                level++;
                SetStatsBasedOnLevel(setCurrentToMax: true);
                if (levelUpEffectAI.gameObject.activeInHierarchy)
                {
                    levelUpEffectAI.Play();
                    levelUpEffectAI.GetComponent<AudioSource>().Play();
                }
                if (levelUpEffectMount.gameObject.activeInHierarchy)
                {
                    levelUpEffectMount.Play();
                    levelUpEffectMount.GetComponent<AudioSource>().Play();
                }
                TextNotificationsManager.instance.NewTextNotifaction(horseName + " leveled up!", false);
            }

            //Update status bar exp
            expBar.SetStat(exp);
        }

        public void SetMaxHealth(float newMax)
        {
            maxHealth = newMax;
            if (healthBar.enabled)
            {
                healthBar.SetMaxStat(maxHealth, false);
            }
        }

        public void SetMaxStamina(float newMax)
        {
            maxStamina = newMax;
            if (staminaBar.enabled)
            {
                staminaBar.SetMaxStat(maxStamina, false);
            }
        }

        public void SetStamina(float newStamina)
        {
            stamina = newStamina;
            if (healthBar.enabled)
            {
                staminaBar.SetStat(stamina);
            }
        }

        void OnEnable()
        {
            AdjustMount();
        }
        
        void Update()
        {
            RegenerateStamina();
            RegenerateHealth();
        }

        private void AdjustMount()
        {
            //Debug.Log("SetMountScale called");
            // Set Horse Scale depending on Player size (gender)
            if (playerStats.gender == "venus")
            {
                transform.localScale = venusMountScale;
                saddleTransform.localPosition = venusSittingPosition;
            }
            else
            {
                transform.localScale = marsMountScale;
                saddleTransform.localPosition = marsSittingPosition;
            }
        }

        public void AttachReins(bool inCombat)
        {
            if (inCombat) return;

            if (playerStats.gender == "venus")
            {
                // Attach reins to venus hands
                leftRein.SetParent(venusLeftReinAttach);
                rightRein.SetParent(venusRightReinAttach);
            }
            else
            {
                // Attach reins to mars hands
                leftRein.SetParent(marsLeftReinAttach);
                rightRein.SetParent(marsRightReinAttach);
            }
            // Zero out the local position to place directly into hands
            leftRein.localPosition = Vector3.zero;
            rightRein.localPosition = Vector3.zero;
        }

        public void DetachReins()
        {
            // Unattach to hands, return to original parent
            leftRein.SetParent(originalReinsParent);
            rightRein.SetParent(originalReinsParent);

            // Reset local position back to previous pos
            leftRein.localPosition = leftReinStartingPos;
            rightRein.localPosition = rightReinStartingPos;
        }

        public void SetHealth(float amount)
        {
            health = amount;
            if(healthBar.enabled)
            {
                healthBar.SetStat(health);
            }
        }

        public void RegenerateStamina()
        {
            if (mountLocomotion.isSprinting)
            {
                staminaRegenTimer = 0;
                return;
            }

            staminaRegenTimer += Time.deltaTime; //update timer

            if (staminaRegenTimer >= staminaRegenDelay)
            {
                if (stamina < maxStamina)
                {
                    staminaTickTimer += Time.deltaTime;

                    if (staminaTickTimer >= 0.1)
                    {
                        staminaTickTimer = 0;
                        if (stamina + staminaRegenAmount <= maxStamina)
                        {
                            stamina += staminaRegenAmount;
                        }
                        else
                        {
                            stamina = maxStamina;
                        }
                    }
                    staminaBar.SetStat(stamina);
                }
            }

        }

        public void RegenerateHealth()
        {
            if (!mountManager.mountIsHealthy && canRegenHealth) return;

            healthRegenTimer += Time.deltaTime; //update timer

            if (healthRegenTimer >= healthRegenDelay)
            {
                if (health < maxHealth)
                {
                    healthTickTimer += Time.deltaTime;

                    if (healthTickTimer >= 1.8f)
                    {
                        healthTickTimer = 0;
                        if (health + healthRegenAmount <= maxHealth)
                        {
                            health += healthRegenAmount;
                        }
                        else
                        {
                            health = maxHealth;
                        }
                    }
                    healthBar.SetStat(health);
                }
            }
        }

        private IEnumerator ResetCanRegenHealthFlag()
        {
            canRegenHealth = false;
            yield return new WaitForSeconds(healthRegenSleepAfterHit);
            canRegenHealth = true;
        }

        public void TakeDamage(float enemyAttackStat, Enemy enemy)
        {
            if (!mountManager.mountIsHealthy) return;
            if (horseAI.enemyToAttack == null)
            {
                horseAI.SetTarget(enemy.gameObject);
            }
            float damageAmount = enemyAttackStat;
            if (horseArmorOwned) damageAmount *= horseArmorDamageTakenMultiplier; 
            health -= damageAmount;
            healthBar.SetStat(health);
            PlayPhysicalHitAudio();
            GainExp(expPerHitTaken);
            bool dead = CheckDeath();
            if (horseAIObject.activeSelf) // AI Mode
            {
                if (!dead) horseAI.Hit();

                if (horseAI.enemyToAttack == null)
                {
                    StartCoroutine(CheckForHostile());
                }
            }
            else
            {
                if (!dead) mountAnim.CrossFade("DirectionalHit", 0.3f);
            }
            if (regenSleep != null)
            {
                StopCoroutine(regenSleep);
                regenSleep = null;
            }
            regenSleep = StartCoroutine(ResetCanRegenHealthFlag());
        }

        public bool CheckDeath()
        {
            if(health <= 0 && mountManager.mountIsHealthy)
            {
                mountManager.mountIsHealthy = false;

                float enemyXPos;
                string deathAnimation = "DeathLeft";

                // Set horse as dead, also called when loading game with a dead horse
                SetDead();

                // AI or Mounted?
                if (horseAIObject.activeSelf) // AI Version
                {
                    // Determine death animation based on enemy direction
                    enemyXPos = horseAIAnim.GetFloat("EnemyPosX");
                    if (enemyXPos >= 0) deathAnimation = "DeathRight";
                    horseAIAnim.SetTrigger(deathAnimation);
                    ///horseAI.Death();
                }
                else // Mounted Version
                {
                    // Determine death animation based on enemy direction
                    playerLocomotion.Dismount(true);
                    enemyXPos = mountAnim.GetFloat("EnemyPosX");
                    if (enemyXPos >= 0) deathAnimation = "DeathRight";
                    mountAnim.SetTrigger(deathAnimation);

                }

                // Universal Steps
                TextNotificationsManager.instance.NewTextNotifaction(horseName + " needs a break!", false); //true prevents duplicate messages for 3 seconds
                mountManager.StartRecoveryTimer();
                attackHitBox.enabled = false;
                return true;
            }
            else
            {
                return false;
            }
        }

        private IEnumerator CheckForHostile()
        {
            horseAI.checkEnemyContact = true;
            yield return new WaitForSeconds(3.0f);
            horseAI.checkEnemyContact = false;
        }

        public void PlayPhysicalHitAudio()
        {
            float primaryVolume = Mathf.Clamp(physicalHitVolume + Random.Range(-physicalHitVolumeVariance, physicalHitVolumeVariance), 0f, 1f);
            audioSource.PlayOneShot(physicalHitCollisionSounds[Random.Range(0, physicalHitCollisionSounds.Length)], primaryVolume);
        }

        public void SetDead()
        {
            horseAIObject.GetComponent<Rigidbody>().isKinematic = true;
            mountObject.GetComponent<Rigidbody>().isKinematic = true;

            horseAI.enabled = false;

            mountObject.tag = "Dead";
            horseAIObject.tag = "Dead";
        }

    }
}
