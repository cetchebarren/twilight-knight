using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace etchebarren
{
    public class PlayerStats : MonoBehaviour
    {
        public static PlayerStats instance;

        private PlayerInventory playerInventory;
        private PlayerLocomotion playerLocomotion;
        private InputHandler inputHandler;
        public WorldStateManager worldStateManager;

        //TESTING/DEBUGGING:
        public bool infiniteStamina = false;
        public bool infiniteMana = false;
        public bool dead = false;
        public bool canRespawn = false;

        public UI_StatBar staminaBar;
        public UI_StatBar healthBar;
        public UI_StatBar manaBar;

        public UI_StatBar xpBar;
        public string playerName = "Knight";
        public string gender = "mars";
        public bool iFrames = false;
        public GameObject playerMenu;

        [Header("Level Up and Experience")]
        public int playerLevel = 1;
        public int remainingPoints = 100;
        public int remainingSkillPoints = 100;
        public int expNeeded = 0;
        public int currentExp = 0;
        public int baseXP = 100; //this is the base amount of XP from level 0 to 1
        public float expGrowthRate = 1.7f; //exponent for exponential growth
        public int maxPlayerLevel = 99;
        public int attrPtsPerLevel = 5;
        public int skillPtsPerLevel = 1;
        public GameObject levelUpText;

        #region BASE STATS
        [Header("BASE Equipment Ratings")]
        [SerializeField] int basePhysicalArmorRating = 0;
        [SerializeField] int baseMagicalArmorRating = 0;
        
        [Header("BASE Primary Stats")]
        public int baseStrength = 10; //physical damage
        public int baseEndurance = 10; //smamina calculations
        public int baseVitality = 10; //total health
        public int basePrecision = 10; //crit chance
        public int baseDexterity = 10; //crit damagev
        public int baseExpertise = 10; //armor rating bonus
        public int baseIntelligence = 10; //magic damage
        public int baseSpirit = 10; //restoriative magic potency && mag armor bonus
        public int baseWillpower = 10; //mana regen
        public int baseLuck = 10; //RNG boosts

        [Header("BASE Secondary Stats")]
        public float baseCriticalChance = 0.0f;
        public float baseCriticalDamage = 0.0f;
        public float baseFireResistance = 0.0f;
        public float baseIceResistance = 0.0f;
        public float baseShockResistance = 0.0f;
        #endregion

        #region EFFECTIVE STATS
        public TextMeshProUGUI primaryStatsText;
        public TextMeshProUGUI secondaryStatsText;
        public GameObject skillSection; //used in calculateEffectiveStats to determine if base stats are displayed

        [Header("Effective Equipment Ratings")]
        [SerializeField] int physicalArmorRating = 0;
        [SerializeField] int magicalArmorRating = 0;

        [Header("Effective Primary Stats")]
        public int strength = 10;
        [SerializeField] int endurance = 10;
        [SerializeField] int vitality = 10;
        [SerializeField] int precision = 10;
        [SerializeField] int dexterity = 10;
        [SerializeField] int expertise = 10;
        public int intelligence = 10;
        [SerializeField] int spirit = 10;
        [SerializeField] int willpower = 10;
        public int luck = 10;

        [Header("Effective Secondary Stats")]
        public float criticalChance = 5.0f;
        public float criticalDamage = 50.0f;
        public float criticalChanceFromGear = 0f;
        public float criticalDamageFromGear = 0f;
        [SerializeField] float fireResistance = 0.0f;
        [SerializeField] float iceResistance = 0.0f;
        [SerializeField] float shockResistance = 0.0f;
        #endregion

        [Header("Skills")]
        private int skillArraySize = 45;
        public Skill[] skills;
        private Dictionary<int, Skill> skillsDict = new Dictionary<int, Skill>();

        [Header("HEALTH Values")]
        public float currentHealth = 0;
        public float maxHealth = 0;

        [Header("MANA Values")]
        public float currentMana = 0;
        public float maxMana = 0;
        private float manaRegenTimer = 0; //per second
        public float manaRegenDelay = 2.0f;
        private float manaTickTimer = 0f;

        [Header("STAMINA Values")]
        public float currentStamina = 0;
        public float maxStamina = 0;

        // Regen Timer
        private float staminaRegenTimer = 0; //per second
        public float staminaRegenDelay = 0.5f;
        public float staminaTickInterval = 0.1f;
        private float staminaTickTimer = 0f;
        // Regen Amounts
        public float staminaRegenAmount = 1.0f;
        public float defaultStaminaRegenAmount = 1.0f;
        public float sheathedStaminaRegenAmount = 1.5f;
        public float blockingStaminaRegen = 0.2f;

        [Header("STAMINA COSTS")]
        public float sprintStaminaCost = 20.0f; //per second
        public float defaultSprintStaminaCost = 20.0f; //per second
        public float sheathedSprintStaminaCost = 15.0f; //per second
        public float dodgeStaminaCost = 15.0f;
        public float backstepStaminaCost = 8.0f;
        public float jumpStaminaCost = 15.0f;
        public float attackBaseStaminaCost = 15.0f;
        public float chargeAttackStaminaCostPerSecond = 10.0f;
        public float airAttackStaminaCost = 15.0f;
        public float lightSprintAttackStaminaCost = 25.0f;
        public float heavySprintAttackStaminaCost = 35.0f;

        [Header("UI References")]
        public TextMeshProUGUI[] statBoxTexts;
        public TextMeshProUGUI[] statBoxPreviewTexts;

        public TextMeshProUGUI[] statBoxTexts_shop;
        public TextMeshProUGUI[] statBoxPreviewTexts_shop;

        [Header("Unarmed Values/Flags")]
        public float unarmedAlternateMultiplier = 1.0f;
        public bool lastAttackWasRightHand = false;

        //OTHER
        private Color darkGreen = new Color(0.0f, 0.7f, 0.0f);
        public SpellsHUDManager spellsHUDManager;
        public PlayerAudioManager playerAudioManager;
        public bool loadGame = false;
        ///private int statCap = 99;
        /////TEST
        private int testingCount = 0;

        [Header("Misc. Effects")]
        public int poisonCharges = 0;
        public int startingPoisonCharges = 1;
        public float poisonMultiplier = 1f;

        [Header("Feats and Skills Prereqs")]
        public int attacksBlocked = 0;
        public int attacksBlockedToUnlockUnwaveringSkill = 50;
        [System.Serializable]
        public class EnemyRecord
        {
            public string enemyName;
            public int defeatCount;

            public EnemyRecord(string name, int count)
            {
                enemyName = name;
                defeatCount = count;
            }
        }
        public List<EnemyRecord> enemyRecords = new List<EnemyRecord>();

        private void Awake()
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

        private void Start()
        {
            playerInventory = GetComponent<PlayerInventory>();
            playerLocomotion = GetComponent<PlayerLocomotion>();
            inputHandler = GetComponent<InputHandler>();

            if (loadGame) InitializeLoadGame();
        }

        public void Initialize() // New Game       
        {   /* Moved this out of Start() so that it is not called when games are loaded but only on new characters in ConfirmCharacter.cs */
            CalculateEffectiveStats();
            //STAMINA:
            currentStamina = maxStamina;
            staminaBar.SetStat(currentStamina);
            staminaBar.SetMaxStat(maxStamina);
            //HEALTH:
            currentHealth = maxHealth;
            healthBar.SetStat(currentHealth);
            healthBar.SetMaxStat(maxHealth);
            //MANA:
            currentMana = maxMana;
            manaBar.SetStat(currentMana);
            manaBar.SetMaxStat(maxMana);
            //XP:
            currentExp = 0;
            xpBar.SetStat(currentExp);
            expNeeded = CalculateNextLevelXP(playerLevel+1, baseXP, expGrowthRate);
            xpBar.SetMaxStat(expNeeded, false);
            //SKILLS: 
            InitializeSkills();

            ScaleStatusBars(); //This is also done when player opens/closes menu in PlayerMenuManager.cs
        }

        public void InitializeLoadGame()
        {
            loadGame = false;
            // Update Effective Stats
            CalculateEffectiveStats();
            // Stammy
            staminaBar.SetStat(currentStamina);
            staminaBar.SetMaxStat(maxStamina, false);
            // Health
            healthBar.SetStat(currentHealth);
            healthBar.SetMaxStat(maxHealth, false);
            // Mana
            manaBar.SetStat(currentMana);
            manaBar.SetMaxStat(maxMana, false);
            // Experience
            expNeeded = CalculateNextLevelXP(playerLevel + 1, baseXP, expGrowthRate);
            xpBar.SetMaxStat(expNeeded, false);
            xpBar.SetStat(currentExp);
            // Scale Status Bars based on stats (if menu is not open)

            //SKILLS: handled in SaveManager Loading

            if (!playerMenu.activeSelf) ScaleStatusBars();
            if (remainingPoints > 0) PlayerMenuManager.instance.levelUpIconInGame.SetActive(true);
        }

        private void Update()
        {
            if(Time.deltaTime != 0 && staminaBar.gameObject.activeSelf && manaBar.gameObject.activeSelf)
            {
                RegenerateStamina();
                staminaBar.SetStat(currentStamina);
                RegenerateMana();
                manaBar.SetStat(currentMana);
            }
        }

        public void InitializeSkills() // Note: remember index 0 means the skill is not unlocked, if applicable the valuye for that index is the default effect (i.e. 1.0x, 0x, etc)
        {
            skills = new Skill[45];
            // MIGHT SKILLS:
            skills[0] = new Skill( // IMPLEMENTED: PlayerLocomotion.cs -> HandleAttackQueue() _and_ Enemy.cs->TakeDamage()
                0, // SKILL ID
                "Basic Combo", // SKILL NAME
                3, // MAX RANK
                new string[]
                {
                    "<i>None</i>",
                    "Adds new attack to melee combo, bringing max combo to <b>2</b> attacks. This new attack deals <b>1.1x</b> damage.",
                    "Adds new attack to melee combo, bringing max combo to <b>3</b> attacks. This new attack deals <b>1.2</b> damage.",
                    "Adds new attack to melee combo, bringing max combo to <b>4</b> attacks. This new attack deals <b>1.3x</b> damage."
                }, 
                new float[] { 1.0f, 1.1f, 1.2f, 1.3f}, // SKILL EFFECTS
                new int[] { /*STR=>*/ 1, /*END=>*/ 0, /*VIT=>*/ 0, /*PRE=>*/ 0, /*DEX=>*/ 1, /*EXP=>*/ 0, /*INT=>*/ 0, /*SPR=>*/ 0, /*WIL=>*/ 0, /*LCK=>*/ 0 }, // ATTRIBUTES REQUIRED
                new int[] {-1}, // PREVIOUS SKILL ID REQUIRED
                false); // Skill Locked at Start

            skills[1] = new Skill( //implemented in DamageTriggerCollider OnTriggerenter and PlayerLocomotion StartChargingHeavyAttack
                1, // SKILL ID
                "Heavy Hitter", // SKILL NAME
                3, // MAX RANK
                new string[] // charge times are simply based on skill rank. rank 1 = 1 second charge
                {
                    "<i>None</i>",
                    "<b>2-hit</b> heavy attack combo, high damage at the expense of significant stamina. Heavy attacks can now be charged for up to <b>1</b> second, increasing damage based on the amount of time charged.",
                    "<b>3-hit</b> heavy attack combo, high damage at the expense of significant stamina. Heavy attacks can now be charged for up to <b>2</b> seconds, increasing damage based on the amount of time charged.",
                    "Reduces stamina costs for heavy attacks by <b>25%</b> and heavy attacks can now be charged for up to <b>3</b> seconds, increasing damage based on the amount of time charged."
                }, 
                new float[] { 1.3f, 1.4f, 1.5f, 1.55f }, // SKILL EFFECTS
                new int[] { /*STR=>*/ 0, /*END=>*/ 0, /*VIT=>*/ 0, /*PRE=>*/ 0, /*DEX=>*/ 0, /*EXP=>*/ 0, /*INT=>*/ 0, /*SPR=>*/ 0, /*WIL=>*/ 0, /*LCK=>*/ 0 }, // ATTRIBUTES REQUIRED
                new int[] { 0 }, // PREVIOUS SKILL ID REQUIRED
                false); // Skill Locked at Start

            skills[2] = new Skill( // implemented 
                2, // SKILL ID
                "Paladin's Vigilance", // SKILL NAME
                5, // MAX RANK
                new string[]
                {
                    "<i>None</i>",
                    "Increases block effectiveness by <b>10%</b>",
                    "Increases block effectiveness by <b>20%</b>",
                    "Increases block effectiveness by <b>30%</b>",
                    "Increases block effectiveness by <b>40%</b>",
                    "Increases block effectiveness by <b>50%</b>"
                },
                new float[] { 0.0f, 0.1f, 0.2f, 0.3f, 0.4f, 0.5f }, // SKILL EFFECTS
                new int[] { /*STR=>*/ 0, /*END=>*/ 0, /*VIT=>*/ 0, /*PRE=>*/ 0, /*DEX=>*/ 0, /*EXP=>*/ 0, /*INT=>*/ 0, /*SPR=>*/ 0, /*WIL=>*/ 0, /*LCK=>*/ 0 }, // ATTRIBUTES REQUIRED
                new int[] {-1}, // PREVIOUS SKILL ID REQUIRED
                false); // Skill Locked at Start

            skills[3] = new Skill(
                3, // SKILL ID
                "Swift Strike", // SKILL NAME
                2, // MAX RANK
                new string[]
                {
                    "<i>None</i>",
                    "After sprinting for 1 second, <i>light attack</i> to perform a special thrust attack that deals <b>2.0x</b> damage" ,
                    "After sprinting for 1 second, <i>light attack</i> to perform a special thrust attack that deals <b>2.0x</b> damage and heavy attack to perform a special spin attack that deals <b>3.0x</b> damage per hit"
                },
                new float[] { 0, 2.0f, 3.0f }, // SKILL EFFECTS
                new int[] { /*STR=>*/ 0, /*END=>*/ 0, /*VIT=>*/ 0, /*PRE=>*/ 5, /*DEX=>*/ 5, /*EXP=>*/ 0, /*INT=>*/ 0, /*SPR=>*/ 0, /*WIL=>*/ 0, /*LCK=>*/ 0 }, // ATTRIBUTES REQUIRED
                new int[] {-1}, // PREVIOUS SKILL ID REQUIRED
                false); // Skill Locked at Start

            skills[4] = new Skill( //IMPLEMENTED: Enemy.cs->TakeDamage()
                4, // SKILL ID
                "Critical Ambush", // SKILL NAME
                3, // MAX RANK
                new string[]
                {
                    "<i>None</i>",
                    "Sneak Attacks deal 1.5x more Damage",
                    "Sneak Attacks deal 3.0x more Damage",
                    "Sneak Attacks deal 5.0x more Damage"
                },
                new float[] { 1.2f, 1.5f, 3.0f, 5.0f }, // SKILL EFFECTS
                new int[] { /*STR=>*/ 0, /*END=>*/ 0, /*VIT=>*/ 0, /*PRE=>*/ 18, /*DEX=>*/ 21, /*EXP=>*/ 0, /*INT=>*/ 0, /*SPR=>*/ 0, /*WIL=>*/ 0, /*LCK=>*/ 0 }, // ATTRIBUTES REQUIRED
                new int[] { 3 }, // PREVIOUS SKILL ID REQUIRED
                false); // Skill Locked at Start

            skills[5] = new Skill(  
                5, // SKILL ID
                "Block Breaker", // SKILL NAME
                3, // MAX RANK
                new string[]
                {
                    "<i>None</i>",
                    "Attacking a blocking opponent reduces their stamina by an extra <b>25%</b> and has a <b>10%</b> chance to break thier block",
                    "Attacking a blocking opponent reduces their stamina by an extra <b>30%</b> and has a <b>15%</b> chance to break thier block",
                    "Attacking a blocking opponent reduces their stamina by an extra <b>40%</b> and has a <b>20%</b> chance to break thier block"
                },
                new float[] { 1.0f, 1.25f, 1.3f, 1.4f, 10.0f, 15.0f, 20.0f }, // SKILL EFFECTS ... index 4 through 6 are accessed by adding +3 to skill[5].rank where implemented
                new int[] { /*STR=>*/ 0, /*END=>*/ 0, /*VIT=>*/ 0, /*PRE=>*/ 0, /*DEX=>*/ 0, /*EXP=>*/ 0, /*INT=>*/ 0, /*SPR=>*/ 0, /*WIL=>*/ 0, /*LCK=>*/ 0 }, // ATTRIBUTES REQUIRED
                new int[] { -1 }, // PREVIOUS SKILL ID REQUIRED
                false); // Skill Locked at Start

            skills[6] = new Skill(
                6, // SKILL ID
                "Aerial Mastery", // SKILL NAME
                3, // MAX RANK
                new string[]
                {
                    "<i>None</i>",
                    "Execute a <b>2-hit</b> combo when performing light attacks in mid-air. Additionally, perform a downward strike during mid-air heavy attacks, dealing bonus damage that <b>slightly<b> scales with fall duration",
                    "Execute a <b>3-hit</b> combo when performing light attacks in mid-air. Additionally, perform a downward strike during mid-air heavy attacks, dealing bonus damage that <b>moderately<b> scales with fall duration",
                    "Execute a <b>4-hit</b> combo when performing light attacks in mid-air. Additionally, perform a downward strike during mid-air heavy attacks, dealing bonus damage that <b>greatly<b> scales with fall duration"
                },
                new float[] { 0, 1.0f, 2.0f, 3.0f }, // SKILL EFFECTS
                new int[] { /*STR=>*/ 0, /*END=>*/ 0, /*VIT=>*/ 0, /*PRE=>*/ 0, /*DEX=>*/ 0, /*EXP=>*/ 0, /*INT=>*/ 0, /*SPR=>*/ 0, /*WIL=>*/ 0, /*LCK=>*/ 0 }, // ATTRIBUTES REQUIRED
                new int[] { 1 }, // PREVIOUS SKILL ID REQUIRED
                false); // Skill Locked at Start

            skills[7] = new Skill(
                7, // SKILL ID
                "Unwavering", // SKILL NAME
                3, // MAX RANK
                new string[]
                {
                    "<i>None</i>",
                    "Attacks that break your block will only deal 50% of their original damage.",
                    "Attacks that break your block do no damage.",
                    "Attacks that break your block do no damage and will no longer stagger you.",
                },
                new float[] { 0.6f, 0.3f, 0.0f, 0.0f }, // SKILL EFFECTS
                new int[] { /*STR=>*/ 0, /*END=>*/ 0, /*VIT=>*/ 0, /*PRE=>*/ 0, /*DEX=>*/ 0, /*EXP=>*/ 0, /*INT=>*/ 0, /*SPR=>*/ 0, /*WIL=>*/ 0, /*LCK=>*/ 0 }, // ATTRIBUTES REQUIRED
                new int[] { 2 }, // PREVIOUS SKILL ID REQUIRED
                true, // Skill Locked at Start
                "Gain expertise by blocking many attacks."); // Unlock conditions

            skills[8] = new Skill(
                8, // SKILL ID
                "Tempest Leap", // SKILL NAME
                1, // MAX RANK
                new string[]
                {
                    "<i>None</i>",
                    "While in midair, perform a quick evasive dash that slices past foes and carries you through incoming threats unharmed. Only one Tempest Leap can be performed before returning to ground.",
                },
                new float[] { 0, 1.0f, 2.0f }, // SKILL EFFECTS
                new int[] { /*STR=>*/ 0, /*END=>*/ 0, /*VIT=>*/ 0, /*PRE=>*/ 0, /*DEX=>*/ 0, /*EXP=>*/ 0, /*INT=>*/ 0, /*SPR=>*/ 0, /*WIL=>*/ 0, /*LCK=>*/ 0 }, // ATTRIBUTES REQUIRED
                new int[] {-1}, // PREVIOUS SKILL ID REQUIRED
                true,
                "unlock mee"); // Skill Locked at Start

            skills[9] = new Skill(
                9, // SKILL ID
                "Skill #9", // SKILL NAME
                1, // MAX RANK
                new string[]
                {
                    "<i>None</i>",
                    "Test 1",
                    "Test 2",
                    "Test 3"
                },
                new float[] { 0, 1.0f, 1.1f, 1.2f }, // SKILL EFFECTS
                new int[] { /*STR=>*/ 0, /*END=>*/ 0, /*VIT=>*/ 0, /*PRE=>*/ 0, /*DEX=>*/ 0, /*EXP=>*/ 0, /*INT=>*/ 0, /*SPR=>*/ 0, /*WIL=>*/ 0, /*LCK=>*/ 0 }, // ATTRIBUTES REQUIRED
                new int[] {-1}, // PREVIOUS SKILL ID REQUIRED
                true); // Skill Locked at Start

            skills[10] = new Skill(
                10, // SKILL ID
                "Advanced Combos", // SKILL NAME
                3, // MAX RANK
                new string[]
                {
                    "<i>None</i>",
                    "While locked on, unleash one of two advanced combos by either holding forward or back while attacking. Maximum combo is shared with the <i><b>Basic Combo</b></i> skill. These combos also do an additional 5% damage.",
                    "While locked on, unleash one of two advanced combos by either holding forward or back while attacking. Maximum combo is shared with the <i><b>Basic Combo</b></i> skill. These combos also do an additional 7.5% damage.",
                    "While locked on, unleash one of two advanced combos by either holding forward or back while attacking. Maximum combo is shared with the <i><b>Basic Combo</b></i> skill. These combos also do an additional 10% damage."
                },
                new float[] { 0.0f, 0.05f, 0.075f, 0.10f }, // SKILL EFFECTS
                new int[] { /*STR=>*/ 0, /*END=>*/ 0, /*VIT=>*/ 0, /*PRE=>*/ 0, /*DEX=>*/ 0, /*EXP=>*/ 0, /*INT=>*/ 0, /*SPR=>*/ 0, /*WIL=>*/ 0, /*LCK=>*/ 0 }, // ATTRIBUTES REQUIRED
                new int[] { 5, 6 }, // PREVIOUS SKILL ID REQUIRED
                false); // Skill Locked at Start

            skills[11] = new Skill( // Damage implemented in OnTriggerEnter(Collider) in DamageTriggerCollider, Stamina implemented in SpendAttackStamina(int attackCombo, string attackType="Standard"), Alternating Damage multiplier implemented in Damage section as well
                11, // SKILL ID
                "Way of the Fist", // SKILL NAME
                5, // MAX RANK
                new string[]
                {
                    "<i>None</i>",
                    "For each point in Strength, unarmed attacks deal 0.1 additional damage. Additionally, unarmed attacks cost 20% less stamina. Alternating fists increases damage by 5%, up to a maximum of 10%.",
                    "For each point in Strength, unarmed attacks deal 0.2 additional damage. Additionally, unarmed attacks cost 40% less stamina. Alternating fists increases damage by 5%, up to a maximum of 15%.",
                    "For each point in Strength, unarmed attacks deal 0.3 additional damage. Additionally, unarmed attacks cost 60% less stamina. Alternating fists increases damage by 5%, up to a maximum of 20%.",
                    "For each point in Strength, unarmed attacks deal 0.4 additional damage. Additionally, unarmed attacks cost 80% less stamina. Alternating fists increases damage by 5%, up to a maximum of 25%.",
                    "For each point in Strength, unarmed attacks deal 0.5 additional damage. Additionally, unarmed attacks cost no stamina. Alternating fists increases damage by 5%, up to a maximum of 30%."
                },
                new float[] { 0, 0.1f, 0.2f, 0.3f, 0.4f, 0.5f, 1.00f, 0.80f, 0.60f, 0.40f, 0.20f, 0.00f }, // SKILL EFFECTS
                new int[] { /*STR=>*/ 10, /*END=>*/ 0, /*VIT=>*/ 0, /*PRE=>*/ 0, /*DEX=>*/ 0, /*EXP=>*/ 0, /*INT=>*/ 0, /*SPR=>*/ 0, /*WIL=>*/ 0, /*LCK=>*/ 0 }, // ATTRIBUTES REQUIRED
                new int[] {-1}, // PREVIOUS SKILL ID REQUIRED
                false); // Skill Locked at Start

            skills[12] = new Skill(
                12, // SKILL ID
                "Skill #12", // SKILL NAME
                3, // MAX RANK
                new string[]
                {
                    "<i>None</i>",
                    "Test 1",
                    "Test 2",
                    "Test 3"
                },
                new float[] { 0, 1.0f, 1.1f, 1.2f }, // SKILL EFFECTS
                new int[] { /*STR=>*/ 0, /*END=>*/ 0, /*VIT=>*/ 0, /*PRE=>*/ 0, /*DEX=>*/ 0, /*EXP=>*/ 0, /*INT=>*/ 0, /*SPR=>*/ 0, /*WIL=>*/ 0, /*LCK=>*/ 0 }, // ATTRIBUTES REQUIRED
                new int[] {-1}, // PREVIOUS SKILL ID REQUIRED
                true); // Skill Locked at Start

            skills[13] = new Skill(
                13, // SKILL ID
                "Skill #13", // SKILL NAME
                1, // MAX RANK
                new string[]
                {
                    "<i>None</i>",
                    "Test 1",
                    "Test 2",
                    "Test 3"
                },
                new float[] { 0, 1.0f, 1.1f, 1.2f }, // SKILL EFFECTS
                new int[] { /*STR=>*/ 0, /*END=>*/ 0, /*VIT=>*/ 0, /*PRE=>*/ 0, /*DEX=>*/ 0, /*EXP=>*/ 0, /*INT=>*/ 0, /*SPR=>*/ 0, /*WIL=>*/ 0, /*LCK=>*/ 0 }, // ATTRIBUTES REQUIRED
                new int[] { 12 }, // PREVIOUS SKILL ID REQUIRED
                true); // Skill Locked at Start

            skills[14] = new Skill(
                14, // SKILL ID
                "Skill #14", // SKILL NAME
                1, // MAX RANK
                new string[]
                {
                    "<i>None</i>",
                    "Test 1",
                    "Test 2",
                    "Test 3"
                },
                new float[] { 0, 1.0f, 1.1f, 1.2f }, // SKILL EFFECTS
                new int[] { /*STR=>*/ 0, /*END=>*/ 0, /*VIT=>*/ 0, /*PRE=>*/ 0, /*DEX=>*/ 0, /*EXP=>*/ 0, /*INT=>*/ 0, /*SPR=>*/ 0, /*WIL=>*/ 0, /*LCK=>*/ 0 }, // ATTRIBUTES REQUIRED
                new int[] { 9 }, // PREVIOUS SKILL ID REQUIRED
                true); // Skill Locked at Start


            // MAGICK SKILLS:

            skills[15] = new Skill(
                15, // SKILL ID
                "Arcanist", // SKILL NAME
                10, // MAX RANK
                new string[]
                {
                    "<i>None</i>",
                    "Mana Cost: 5, Base Damage: 8\n Minor Arcane Bolt, an apprentice spell, conjures a shimmering bolt of arcane energy, inflicting minor damage on impact.",
                    "Increases all Arcane elemental damage from any source by <b>5%</b>.",
                    "Increases all Arcane elemental damage from any source by <b>10%</b>.",
                    "Increases all Arcane elemental damage from any source by <b>15%</b>.",
                    "Increases all Arcane elemental damage from any source by <b>20%</b>.",
                    "Increases all Arcane elemental damage from any source by <b>25%</b>.",
                    "Increases all Arcane elemental damage from any source by <b>30%</b>.",
                    "Increases all Arcane elemental damage from any source by <b>35%</b>.",
                    "Increases all Arcane elemental damage from any source by <b>40%</b>.",
                    "Increases all Arcane elemental damage from any source by <b>45%</b>."
                },
                new float[] { 1.0f, 1.0f, 1.05f, 1.10f, 1.15f, 1.20f, 1.25f, 1.30f, 1.35f, 1.40f, 1.45f }, // SKILL EFFECTS
                new int[] { /*STR=>*/ 0, /*END=>*/ 0, /*VIT=>*/ 0, /*PRE=>*/ 0, /*DEX=>*/ 0, /*EXP=>*/ 0, /*INT=>*/ 0, /*SPR=>*/ 0, /*WIL=>*/ 0, /*LCK=>*/ 0 }, // ATTRIBUTES REQUIRED
                new int[] {-1}, // PREVIOUS SKILL ID REQUIRED
                false); // Skill Locked at Start

            skills[16] = new Skill(
                16, // SKILL ID
                "Mender", // SKILL NAME
                5, // MAX RANK
                new string[]
                {
                    "<i>None</i>",
                    "Mana Cost: 20, Base Power: 20 \n Mend, a soothing enchantment that harnesses the flow of life energy to mend injuries and revitalize the weary.",
                    "Increases all curative magic efficiency from any source by <b>5%</b>.",
                    "Increases all curative magic efficiency from any source by <b>10%</b>.",
                    "Increases all curative magic efficiency from any source by <b>15%</b>.",
                    "Increases all curative magic efficiency from any source by <b>20%</b>."
                },
                new float[] { 1.0f, 1.0f, 1.05f, 1.10f, 1.15f, 1.20f }, // SKILL EFFECTS
                new int[] { /*STR=>*/ 0, /*END=>*/ 0, /*VIT=>*/ 0, /*PRE=>*/ 0, /*DEX=>*/ 0, /*EXP=>*/ 0, /*INT=>*/ 0, /*SPR=>*/ 0, /*WIL=>*/ 0, /*LCK=>*/ 0 }, // ATTRIBUTES REQUIRED
                new int[] {-1}, // PREVIOUS SKILL ID REQUIRED
                false); // Skill Locked at Start

            skills[17] = new Skill(
                17, // SKILL ID
                "Fire Mage", // SKILL NAME
                10, // MAX RANK
                new string[]
                {
                    "<i>None</i>",
                    "Mana Cost: 15, Base Damage: 10\n Fireball, a novice's spell, the  creates a blazing sphere that inflicts moderate damage, providing a crucial step in a mage's early journey to mastery.",
                    "Increases all Fire elemental damage from any source by <b>5%</b>.",
                    "Increases all Fire elemental damage from any source by <b>10%</b>.",
                    "Increases all Fire elemental damage from any source by <b>15%</b>.",
                    "Increases all Fire elemental damage from any source by <b>20%</b>.",
                    "Increases all Fire elemental damage from any source by <b>25%</b>.",
                    "Increases all Fire elemental damage from any source by <b>30%</b>.",
                    "Increases all Fire elemental damage from any source by <b>35%</b>.",
                    "Increases all Fire elemental damage from any source by <b>40%</b>.",
                    "Increases all Fire elemental damage from any source by <b>45%</b>."
                },
                new float[] { 1.0f, 1.0f, 1.05f, 1.10f, 1.15f, 1.20f, 1.25f, 1.30f, 1.35f, 1.40f, 1.45f }, // SKILL EFFECTS
                new int[] { /*STR=>*/ 0, /*END=>*/ 0, /*VIT=>*/ 0, /*PRE=>*/ 0, /*DEX=>*/ 0, /*EXP=>*/ 0, /*INT=>*/ 0, /*SPR=>*/ 0, /*WIL=>*/ 0, /*LCK=>*/ 0 }, // ATTRIBUTES REQUIRED
                new int[] {-1}, // PREVIOUS SKILL ID REQUIRED
                false); // Skill Locked at Start

            skills[18] = new Skill(
                18, // SKILL ID
                "Ice Mage", // SKILL NAME
                10, // MAX RANK
                new string[]
                {
                    "<i>None</i>",
                    "Mana Cost: 12, Base Damage: 8\n Frostshard, an adept's swift mastery, evokes a jagged ice fragment, piercing the target and chilling them to the bone.",
                    "Increases all Ice elemental damage from any source by <b>5%</b>.",
                    "Increases all Ice elemental damage from any source by <b>10%</b>.",
                    "Increases all Ice elemental damage from any source by <b>15%</b>.",
                    "Increases all Ice elemental damage from any source by <b>20%</b>.",
                    "Increases all Ice elemental damage from any source by <b>25%</b>.",
                    "Increases all Ice elemental damage from any source by <b>30%</b>.",
                    "Increases all Ice elemental damage from any source by <b>35%</b>.",
                    "Increases all Ice elemental damage from any source by <b>40%</b>.",
                    "Increases all Ice elemental damage from any source by <b>45%</b>."
                },
                new float[] { 1.0f, 1.0f, 1.05f, 1.10f, 1.15f, 1.20f, 1.25f, 1.30f, 1.35f, 1.40f, 1.45f }, // SKILL EFFECTS
                new int[] { /*STR=>*/ 0, /*END=>*/ 0, /*VIT=>*/ 0, /*PRE=>*/ 0, /*DEX=>*/ 0, /*EXP=>*/ 0, /*INT=>*/ 0, /*SPR=>*/ 0, /*WIL=>*/ 0, /*LCK=>*/ 0 }, // ATTRIBUTES REQUIRED
                new int[] {-1}, // PREVIOUS SKILL ID REQUIRED
                false); // Skill Locked at Start

            skills[19] = new Skill(
                19, // SKILL ID
                "Shock Mage", // SKILL NAME
                10, // MAX RANK
                new string[]
                {
                    "<i>None</i>",
                    "Mana Cost: 12, Base Damage: 8\n Zapshock, an apprentice's incantation, conjures a crackling surge of electricity that delivers substantial shock damage, marking a pivotal advancement on a mage's path towards expertise.",
                    "Increases all Shock elemental damage from any source by <b>5%</b>.",
                    "Increases all Shock elemental damage from any source by <b>10%</b>.",
                    "Increases all Shock elemental damage from any source by <b>15%</b>.",
                    "Increases all Shock elemental damage from any source by <b>20%</b>.",
                    "Increases all Shock elemental damage from any source by <b>25%</b>.",
                    "Increases all Shock elemental damage from any source by <b>30%</b>.",
                    "Increases all Shock elemental damage from any source by <b>35%</b>.",
                    "Increases all Shock elemental damage from any source by <b>40%</b>.",
                    "Increases all Shock elemental damage from any source by <b>45%</b>."
                },
                new float[] { 1.0f, 1.0f, 1.05f, 1.10f, 1.15f, 1.20f, 1.25f, 1.30f, 1.35f, 1.40f, 1.45f }, // SKILL EFFECTS
                new int[] { /*STR=>*/ 0, /*END=>*/ 0, /*VIT=>*/ 0, /*PRE=>*/ 0, /*DEX=>*/ 0, /*EXP=>*/ 0, /*INT=>*/ 0, /*SPR=>*/ 0, /*WIL=>*/ 0, /*LCK=>*/ 0 }, // ATTRIBUTES REQUIRED
                new int[] {-1}, // PREVIOUS SKILL ID REQUIRED
                false); // Skill Locked at Start

            skills[20] = new Skill(
                20, // SKILL ID
                "Dark Arcane Bolt", // SKILL NAME
                3, // MAX RANK
                new string[] // Health Drain and damage increase from ranks implemented at:  CastSpellProj(), adjust values there
                {
                    "<i>None</i>",
                    "Mana Cost: 18, Base Damage: 20\n  An adept variant of Arcane Bolt that has been corrupted by the emerging dark influences within its caster. If the user has at least <b>20%</b> of their maximum health, this health will be drained and this spell's damage will be increased by <b>30%</b> in exchange.",
                    "Mana Cost: 36, Base Damage: 22\n  An adept variant of Arcane Bolt that has been corrupted by the emerging dark influences within its caster. If the user has at least <b>25%</b> of their maximum health, this health will be drained and this spell's damage will be increased by <b>50%</b> in exchange.",
                    "Mana Cost: 54, Base Damage: 24\n  An adept variant of Arcane Bolt that has been corrupted by the emerging dark influences within its caster. If the user has at least <b>30%</b> of their maximum health, this health will be drained and this spell's damage will be increased by <b>70%</b> in exchange."
                },
                new float[] { 0, 1.0f, 1.1f, 1.2f }, // SKILL EFFECTS
                new int[] { /*STR=>*/ 0, /*END=>*/ 0, /*VIT=>*/ 0, /*PRE=>*/ 0, /*DEX=>*/ 0, /*EXP=>*/ 0, /*INT=>*/ 0, /*SPR=>*/ 0, /*WIL=>*/ 0, /*LCK=>*/ 0 }, // ATTRIBUTES REQUIRED
                new int[] { 15 }, // PREVIOUS SKILL ID REQUIRED
                false); // Skill Locked at Start

            skills[21] = new Skill(
                21, // SKILL ID
                "Rejuvenate", // SKILL NAME
                4, // MAX RANK
                new string[]
                {
                    "<i>None</i>",
                    "Mana Cost: 15, Base Power: 3\nRejuvenate, a spell attuned to the nurturing spirit of Mother Nature, gently renews, with the patience and tranqulity of the forest.",
                    "Increase Rejuvenate duration by <b>50%</b>.",
                    "Decrease Rejuvenate's time between heals by <b>10%</b>.",
                    "Increase Rejuvenate's base healing power by <b>50%</b>."
                },
                new float[] { 0, 1.0f, 1.1f, 1.2f }, // SKILL EFFECTS
                new int[] { /*STR=>*/ 0, /*END=>*/ 0, /*VIT=>*/ 0, /*PRE=>*/ 0, /*DEX=>*/ 0, /*EXP=>*/ 0, /*INT=>*/ 0, /*SPR=>*/ 0, /*WIL=>*/ 0, /*LCK=>*/ 0 }, // ATTRIBUTES REQUIRED
                new int[] { 16 }, // PREVIOUS SKILL ID REQUIRED
                false); // Skill Locked at Start

            skills[22] = new Skill(
                22, // SKILL ID
                "Meteor", // SKILL NAME
                3, // MAX RANK
                new string[]
                {
                    "<i>None</i>",
                    "Mana Cost: 45, Base Damage: 50 \n Meteor, a devastating spell, summons a celestial body to crash down dealing great damage to those caught in its fiery wake.",
                    "Mana Cost: 60, Base Damage: 70 \n The size and explosion of Meteor is increased by <b>25%</b>.",
                    "Mana Cost: 75, Base Damage: 90 \n The size and explosion of Meteor is increased by <b>50%</b>."
                },
                new float[] { 1.0f, 1.0f, 1.25f, 1.50f }, // SKILL EFFECTS used for scale in AnimEvents
                new int[] { /*STR=>*/ 0, /*END=>*/ 0, /*VIT=>*/ 0, /*PRE=>*/ 0, /*DEX=>*/ 0, /*EXP=>*/ 0, /*INT=>*/ 0, /*SPR=>*/ 0, /*WIL=>*/ 0, /*LCK=>*/ 0 }, // ATTRIBUTES REQUIRED
                new int[] { 17 }, // PREVIOUS SKILL ID REQUIRED
                false); // Skill Locked at Start

            skills[23] = new Skill(
                23, // SKILL ID
                "Glacial Burst", // SKILL NAME
                3, // MAX RANK
                new string[]
                {
                    "<i>None</i>",
                    "Mana Cost: 35, Base Damage: 25\n Summons multiple icy crystals, each charging up before unleashing a frosty explosion upon nearby foes, each dealing 25% of initial hit.",
                    "Increase the radius of each crystal's explosion by <b>25%</b> and decrease crystal charge time by <b>15%</b>.", // Implemented in CrystalAoE.cs
                    "Increase the radius of each crystal's explosion by <b>50%</b> and decrease crystal charge time by <b>30%</b>."
                },
                new float[] { 0, 1.0f, 1.1f, 1.2f }, // SKILL EFFECTS
                new int[] { /*STR=>*/ 0, /*END=>*/ 0, /*VIT=>*/ 0, /*PRE=>*/ 0, /*DEX=>*/ 0, /*EXP=>*/ 0, /*INT=>*/ 0, /*SPR=>*/ 0, /*WIL=>*/ 0, /*LCK=>*/ 0 }, // ATTRIBUTES REQUIRED
                new int[] { 18 }, // PREVIOUS SKILL ID REQUIRED
                false); // Skill Locked at Start

            skills[24] = new Skill(
                24, // SKILL ID
                "Stormguard", // SKILL NAME
                3, // MAX RANK
                new string[] // Effects implemented in DamageTriggerCollider.cs and 
                {
                    "<i>None</i>",
                    "Mana Cost: 25, Base Damage: 15\nFor <b>20</b> seconds, Stormguard cloaks the caster in electric energy that reduces incoming non-magic damage by <b>15%</b> while electrifying melee attackers.",
                    "Mana Cost: 30, Base Damage: 20\nFor <b>30</b> seconds, Stormguard cloaks the caster in electric energy that reduces incoming non-magic damage by <b>20%</b> while electrifying melee attackers.",
                    "Mana Cost: 35, Base Damage: 25\nFor <b>40</b> seconds, Stormguard cloaks the caster in electric energy that reduces incoming non-magic damage by <b>25%</b> while electrifying melee attackers."
                },
                new float[] { 1.0f, 0.85f, 0.80f, 0.75f }, // SKILL EFFECTS
                new int[] { /*STR=>*/ 0, /*END=>*/ 0, /*VIT=>*/ 0, /*PRE=>*/ 0, /*DEX=>*/ 0, /*EXP=>*/ 0, /*INT=>*/ 0, /*SPR=>*/ 0, /*WIL=>*/ 0, /*LCK=>*/ 0 }, // ATTRIBUTES REQUIRED
                new int[] { 19 }, // PREVIOUS SKILL ID REQUIRED
                false); // Skill Locked at Start

            skills[25] = new Skill(
                25, // SKILL ID
                "Vampiric Bolt", // SKILL NAME
                5, // MAX RANK
                new string[]
                {
                    "<i>None</i>", // Note: Must update mana cost and damage in playerInventory AddSpellFromSkillTree(int skillID) to actually change values
                    "Mana Cost: 30, Base Damage: 25\n  An incantation of pure corruption and malice that distorts the healing nature of rejuvenative magic, siphoning life force from its target for <b>10%</b> of the damage it dealt.",
                    "Mana Cost: 35, Base Damage: 27\n  An incantation of pure corruption and malice that distorts the healing nature of rejuvenative magic, siphoning life force from its target for <b>12%</b> of the damage it dealt.",
                    "Mana Cost: 40, Base Damage: 29\n  An incantation of pure corruption and malice that distorts the healing nature of rejuvenative magic, siphoning life force from its target for <b>14%</b> of the damage it dealt.",
                    "Mana Cost: 45, Base Damage: 32\n  An incantation of pure corruption and malice that distorts the healing nature of rejuvenative magic, siphoning life force from its target for <b>17%</b> of the damage it dealt.",
                    "Mana Cost: 50, Base Damage: 35\n  An incantation of pure corruption and malice that distorts the healing nature of rejuvenative magic, siphoning life force from its target for <b>20%</b> of the damage it dealt.",

                },
                new float[] { 0, 0.10f, 0.12f, 0.14f, 0.17f, 0.20f }, // SKILL EFFECTS
                new int[] { /*STR=>*/ 0, /*END=>*/ 0, /*VIT=>*/ 0, /*PRE=>*/ 0, /*DEX=>*/ 0, /*EXP=>*/ 0, /*INT=>*/ 0, /*SPR=>*/ 0, /*WIL=>*/ 0, /*LCK=>*/ 0 }, // ATTRIBUTES REQUIRED
                new int[] { 20, 21 }, // PREVIOUS SKILL ID REQUIRED
                false); // Skill Locked at Start

            skills[26] = new Skill(
                26, // SKILL ID
                "Skill #26", // SKILL NAME
                1, // MAX RANK
                new string[]
                {
                    "<i>None</i>",
                    "Test 1",
                    "Test 2",
                    "Test 3"
                },
                new float[] { 0, 1.0f, 1.1f, 1.2f }, // SKILL EFFECTS
                new int[] { /*STR=>*/ 0, /*END=>*/ 0, /*VIT=>*/ 0, /*PRE=>*/ 0, /*DEX=>*/ 0, /*EXP=>*/ 0, /*INT=>*/ 0, /*SPR=>*/ 0, /*WIL=>*/ 0, /*LCK=>*/ 0 }, // ATTRIBUTES REQUIRED
                new int[] {-1}, // PREVIOUS SKILL ID REQUIRED
                true); // Skill Locked at Start

            skills[27] = new Skill(
                27, // SKILL ID
                "Skill #27", // SKILL NAME
                1, // MAX RANK
                new string[]
                {
                    "<i>None</i>",
                    "Test 1",
                    "Test 2",
                    "Test 3"
                },
                new float[] { 0, 1.0f, 1.1f, 1.2f }, // SKILL EFFECTS
                new int[] { /*STR=>*/ 0, /*END=>*/ 0, /*VIT=>*/ 0, /*PRE=>*/ 0, /*DEX=>*/ 0, /*EXP=>*/ 0, /*INT=>*/ 0, /*SPR=>*/ 0, /*WIL=>*/ 0, /*LCK=>*/ 0 }, // ATTRIBUTES REQUIRED
                new int[] { 26 }, // PREVIOUS SKILL ID REQUIRED
                true); // Skill Locked at Start

            skills[28] = new Skill(
                28, // SKILL ID
                "Elemental Mastery", // SKILL NAME
                3, // MAX RANK
                new string[]
                {
                    "<i>None</i>",
                    // Rank 1
                    "Increase elemental reaction damage by <b>5%</b>." +// Implemented by float extraDamagePercentPerSkillPoint in Thawed(), etc. in Enemy.cs
                    "\nAdditionally, reactions will consume <b>5%</b> less of the elemental statuses used for the reaction." + // Implemented by float statusConsumptionReductionPerSKillPoint in Thawed(), etc. in Enemy.cs
                    "\n\n- All status effects last <b>20%</b> longer." + // Implemented by float extraStatusDurationPerSkillPoint in Enemy.cs, Burning(), Zapped(), Chilled() in Enemy.cs
                    "\n- Burning: Increase burn damage by <b>10%</b>." + // Implemented in Enemy.cs, in Burning()
                    "\n- Zapped: Increase stun chance by <b>3%</b>." + // Base: 10% base chance to stun every tick, bonus implemented in Enemy.cs Zapped()
                    "\n- Chilled: Decrease speed by additional <b>8%</b>.", // Implemented in Enemy.cs, private void StartChilled(float damageFromHit) Total: 38%
                    // Rank 2
                    "Increase elemental reaction damage by <b>10%</b>." +
                    "\nAdditionally, reactions will consume <b>10%</b> less of the elemental statuses used for the reaction." +
                    "\n\n- All status effects last <b>40%</b> longer." +
                    "\n- Burning: Increase burn damage by <b>20%</b>." +
                    "\n- Zapped: Increase stun chance by <b>6%</b>." +
                    "\n- Chilled: Decrease speed by additional <b>16%</b>.",//Total: 46%
                    // Rank 3
                    "Increase elemental reaction damage by <b>15%</b>." +
                    "\nAdditionally, reactions will consume <b>15%</b> less of the elemental statuses used for the reaction." +
                    "\n\n- All status effects last <b>60%</b> longer." +
                    "\n- Burning: Increase burn damage by <b>30%</b>." +
                    "\n- Zapped: Increase stun chance by <b>9%</b>." +
                    "\n- Chilled: Decrease speed by additional <b>24%</b>.",//Total: 54%
                     // Rank 4
                    "Increase elemental reaction damage by <b>20%</b>." +
                    "\nAdditionally, reactions will consume <b>20%</b> less of the elemental statuses used for the reaction." +
                    "\n\n- All status effects last <b>80%</b> longer." +
                    "\n- Burning: Increase burn damage by <b>40%</b>." +
                    "\n- Zapped: Increase stun chance by <b>12%</b>." +
                    "\n- Chilled: Decrease speed by additional <b>32%</b>.",//Total: 68%
                     // Rank 5
                    "Increase elemental reaction damage by <b>25%</b>." +
                    "\nAdditionally, reactions will consume <b>30%</b> less of the elemental statuses used for the reaction." +
                    "\n\n- All status effects last <b>100%</b> longer." +
                    "\n- Burning: Increase burn damage by <b>50%</b>." + //Total
                    "\n- Zapped: Increase stun chance by <b>15%</b>." + //Total: 25%
                    "\n- Chilled: Decrease speed by additional <b>40%</b>."//Total: 70%

                },
                new float[] { 0, 1.0f, 1.1f, 1.2f }, // SKILL EFFECTS
                new int[] { /*STR=>*/ 0, /*END=>*/ 0, /*VIT=>*/ 0, /*PRE=>*/ 0, /*DEX=>*/ 0, /*EXP=>*/ 0, /*INT=>*/ 0, /*SPR=>*/ 0, /*WIL=>*/ 0, /*LCK=>*/ 0 }, // ATTRIBUTES REQUIRED
                new int[] { 22, 23, 24 }, // PREVIOUS SKILL ID REQUIRED
                false); // Skill Locked at Start

            skills[29] = new Skill(
                29, // SKILL ID
                "Mystic Infusion", // SKILL NAME
                5, // MAX RANK
                new string[]
                {
                    "<i>None</i>",
                    "Mana Cost: 50\nFor <b>20</b> seconds, the caster's sword is infused with fire, ice or shock. During this time, all damage dealt is converted into the corresponding elemental type. However, damage dealt is still based on the weapon's Physical Damage Rating.",
                    "Mana Cost: 55\nFor <b>30</b> seconds, the caster's sword is infused with fire, ice or shock. During this time, all damage dealt is converted into the corresponding elemental type. However, damage dealt is still based on the weapon's Physical Damage Rating.",
                    "Mana Cost: 60\nFor <b>40</b> seconds, the caster's sword is infused with fire, ice or shock. During this time, all damage dealt is converted into the corresponding elemental type. However, damage dealt is still based on the weapon's Physical Damage Rating.",
                    "Mana Cost: 65\nFor <b>50</b> seconds, the caster's sword is infused with fire, ice or shock. During this time, all damage dealt is converted into the corresponding elemental type. However, damage dealt is still based on the weapon's Physical Damage Rating.",
                    "Mana Cost: 70\nFor <b>60</b> seconds, the caster's sword is infused with fire, ice or shock. During this time, all damage dealt is converted into the corresponding elemental type. However, damage dealt is still based on the weapon's Physical Damage Rating.",
                },
                new float[] { 0, 1.0f, 1.1f, 1.2f, 0, 0 }, // SKILL EFFECTS
                new int[] { /*STR=>*/ 0, /*END=>*/ 0, /*VIT=>*/ 0, /*PRE=>*/ 0, /*DEX=>*/ 0, /*EXP=>*/ 0, /*INT=>*/ 0, /*SPR=>*/ 0, /*WIL=>*/ 0, /*LCK=>*/ 0 }, // ATTRIBUTES REQUIRED
                new int[] { 28 }, // PREVIOUS SKILL ID REQUIRED
                false); // Skill Locked at Start


            // UTILITY SKILLS:

            skills[30] = new Skill(
                30, // SKILL ID
                "Cartographer", // SKILL NAME
                3, // MAX RANK
                new string[]
                {
                    "<i>None</i>",
                    "Enemies appear on the minimap.",
                    "Chests appear on the minimap",
                    "Hidden caches and treasures appear on the minimap."
                },
                new float[] { 0, 1.0f, 1.1f, 1.2f }, // SKILL EFFECTS
                new int[] { /*STR=>*/ 0, /*END=>*/ 0, /*VIT=>*/ 0, /*PRE=>*/ 0, /*DEX=>*/ 0, /*EXP=>*/ 0, /*INT=>*/ 0, /*SPR=>*/ 0, /*WIL=>*/ 0, /*LCK=>*/ 0 }, // ATTRIBUTES REQUIRED
                new int[] { -1 }, // PREVIOUS SKILL ID REQUIRED
                false); // Skill Locked at Start

            skills[31] = new Skill(
                31, // SKILL ID
                "Mystic Knowledge", // SKILL NAME
                4, // MAX RANK
                new string[]
                {
                    "<i>None</i>",
                    "Increase equipable spell slots to <b>5</b>.",
                    "Increase equipable spell slots to <b>6</b>.",
                    "Increase equipable spell slots to <b>7</b>.",
                    "Increase equipable spell slots to <b>8</b>."
                },
                new float[] { 0, 1.0f, 2.0f, 3.0f, 4.0f }, // SKILL EFFECTS
                new int[] { /*STR=>*/ 0, /*END=>*/ 0, /*VIT=>*/ 0, /*PRE=>*/ 0, /*DEX=>*/ 0, /*EXP=>*/ 0, /*INT=>*/ 0, /*SPR=>*/ 0, /*WIL=>*/ 0, /*LCK=>*/ 0 }, // ATTRIBUTES REQUIRED
                new int[] { -1 }, // PREVIOUS SKILL ID REQUIRED
                false); // Skill Locked at Start

            skills[32] = new Skill(
                32, // SKILL ID
                "Experienced", // SKILL NAME
                5, // MAX RANK
                new string[]
                {
                    "<i>None</i>",
                    "Increase all EXP gains by 5%",
                    "Increase all EXP gains by 10%",
                    "Increase all EXP gains by 15%",
                    "Increase all EXP gains by 20%",
                    "Increase all EXP gains by 25%"
                },
                new float[] { 1.00f, 1.05f, 1.10f, 1.15f, 1.20f, 1.25f }, // SKILL EFFECTS
                new int[] { /*STR=>*/ 0, /*END=>*/ 0, /*VIT=>*/ 0, /*PRE=>*/ 0, /*DEX=>*/ 0, /*EXP=>*/ 0, /*INT=>*/ 0, /*SPR=>*/ 0, /*WIL=>*/ 0, /*LCK=>*/ 0 }, // ATTRIBUTES REQUIRED
                new int[] { -1 }, // PREVIOUS SKILL ID REQUIRED
                false); // Skill Locked at Start

            skills[33] = new Skill(
                33, // SKILL ID
                "Skill #33", // SKILL NAME
                1, // MAX RANK
                new string[]
                {
                    "<i>None</i>",
                    "Test 1",
                    "Test 2",
                    "Test 3"
                },
                new float[] { 0, 1.0f, 1.1f, 1.2f }, // SKILL EFFECTS
                new int[] { /*STR=>*/ 0, /*END=>*/ 0, /*VIT=>*/ 0, /*PRE=>*/ 0, /*DEX=>*/ 0, /*EXP=>*/ 0, /*INT=>*/ 0, /*SPR=>*/ 0, /*WIL=>*/ 0, /*LCK=>*/ 0 }, // ATTRIBUTES REQUIRED
                new int[] { -1 }, // PREVIOUS SKILL ID REQUIRED
                true); // Skill Locked at Start

            skills[34] = new Skill(
                34, // SKILL ID
                "Skill #34", // SKILL NAME
                1, // MAX RANK
                new string[]
                {
                    "<i>None</i>",
                    "Test 1",
                    "Test 2",
                    "Test 3"
                },
                new float[] { 0, 1.0f, 1.1f, 1.2f }, // SKILL EFFECTS
                new int[] { /*STR=>*/ 0, /*END=>*/ 0, /*VIT=>*/ 0, /*PRE=>*/ 0, /*DEX=>*/ 0, /*EXP=>*/ 0, /*INT=>*/ 0, /*SPR=>*/ 0, /*WIL=>*/ 0, /*LCK=>*/ 0 }, // ATTRIBUTES REQUIRED
                new int[] { -1 }, // PREVIOUS SKILL ID REQUIRED
                true); // Skill Locked at Start

            skills[35] = new Skill(
                35, // SKILL ID
                "Skill #35", // SKILL NAME
                1, // MAX RANK
                new string[]
                {
                    "<i>None</i>",
                    "Test 1",
                    "Test 2",
                    "Test 3"
                },
                new float[] { 0, 1.0f, 1.1f, 1.2f }, // SKILL EFFECTS
                new int[] { /*STR=>*/ 0, /*END=>*/ 0, /*VIT=>*/ 0, /*PRE=>*/ 0, /*DEX=>*/ 0, /*EXP=>*/ 0, /*INT=>*/ 0, /*SPR=>*/ 0, /*WIL=>*/ 0, /*LCK=>*/ 0 }, // ATTRIBUTES REQUIRED
                new int[] { -1 }, // PREVIOUS SKILL ID REQUIRED
                true); // Skill Locked at Start

            skills[36] = new Skill(
                36, // SKILL ID
                "Skill #36", // SKILL NAME
                1, // MAX RANK
                new string[]
                {
                    "<i>None</i>",
                    "Test 1",
                    "Test 2",
                    "Test 3"
                },
                new float[] { 0, 1.0f, 1.1f, 1.2f }, // SKILL EFFECTS
                new int[] { /*STR=>*/ 0, /*END=>*/ 0, /*VIT=>*/ 0, /*PRE=>*/ 0, /*DEX=>*/ 0, /*EXP=>*/ 0, /*INT=>*/ 0, /*SPR=>*/ 0, /*WIL=>*/ 0, /*LCK=>*/ 0 }, // ATTRIBUTES REQUIRED
                new int[] { 16 }, // PREVIOUS SKILL ID REQUIRED
                true); // Skill Locked at Start

            skills[37] = new Skill(
                37, // SKILL ID
                "Skill #37", // SKILL NAME
                1, // MAX RANK
                new string[]
                {
                    "<i>None</i>",
                    "Test 1",
                    "Test 2",
                    "Test 3"
                },
                new float[] { 0, 1.0f, 1.1f, 1.2f }, // SKILL EFFECTS
                new int[] { /*STR=>*/ 0, /*END=>*/ 0, /*VIT=>*/ 0, /*PRE=>*/ 0, /*DEX=>*/ 0, /*EXP=>*/ 0, /*INT=>*/ 0, /*SPR=>*/ 0, /*WIL=>*/ 0, /*LCK=>*/ 0 }, // ATTRIBUTES REQUIRED
                new int[] { -1 }, // PREVIOUS SKILL ID REQUIRED
                true); // Skill Locked at Start

            skills[38] = new Skill(
                38, // SKILL ID
                "Skill #38", // SKILL NAME
                1, // MAX RANK
                new string[]
                {
                    "<i>None</i>",
                    "Test 1",
                    "Test 2",
                    "Test 3"
                },
                new float[] { 0, 1.0f, 1.1f, 1.2f }, // SKILL EFFECTS
                new int[] { /*STR=>*/ 0, /*END=>*/ 0, /*VIT=>*/ 0, /*PRE=>*/ 0, /*DEX=>*/ 0, /*EXP=>*/ 0, /*INT=>*/ 0, /*SPR=>*/ 0, /*WIL=>*/ 0, /*LCK=>*/ 0 }, // ATTRIBUTES REQUIRED
                new int[] { -1 }, // PREVIOUS SKILL ID REQUIRED
                true); // Skill Locked at Start

            skills[39] = new Skill(
                39, // SKILL ID
                "Skill #39", // SKILL NAME
                1, // MAX RANK
                new string[]
                {
                    "<i>None</i>",
                    "Test 1",
                    "Test 2",
                    "Test 3"
                },
                new float[] { 0, 1.0f, 1.1f, 1.2f }, // SKILL EFFECTS
                new int[] { /*STR=>*/ 0, /*END=>*/ 0, /*VIT=>*/ 0, /*PRE=>*/ 0, /*DEX=>*/ 0, /*EXP=>*/ 0, /*INT=>*/ 0, /*SPR=>*/ 0, /*WIL=>*/ 0, /*LCK=>*/ 0 }, // ATTRIBUTES REQUIRED
                new int[] { -1 }, // PREVIOUS SKILL ID REQUIRED
                true); // Skill Locked at Start

            skills[40] = new Skill(
                40, // SKILL ID
                "Skill #40", // SKILL NAME
                1, // MAX RANK
                new string[]
                {
                    "<i>None</i>",
                    "Test 1",
                    "Test 2",
                    "Test 3"
                },
                new float[] { 0, 1.0f, 1.1f, 1.2f }, // SKILL EFFECTS
                new int[] { /*STR=>*/ 0, /*END=>*/ 0, /*VIT=>*/ 0, /*PRE=>*/ 0, /*DEX=>*/ 0, /*EXP=>*/ 0, /*INT=>*/ 0, /*SPR=>*/ 0, /*WIL=>*/ 0, /*LCK=>*/ 0 }, // ATTRIBUTES REQUIRED
                new int[] { -1 }, // PREVIOUS SKILL ID REQUIRED
                true); // Skill Locked at Start

            skills[41] = new Skill(
                41, // SKILL ID
                "Skill #41", // SKILL NAME
                1, // MAX RANK
                new string[]
                {
                    "<i>None</i>",
                    "Test 1",
                    "Test 2",
                    "Test 3"
                },
                new float[] { 0, 1.0f, 1.1f, 1.2f }, // SKILL EFFECTS
                new int[] { /*STR=>*/ 0, /*END=>*/ 0, /*VIT=>*/ 0, /*PRE=>*/ 0, /*DEX=>*/ 0, /*EXP=>*/ 0, /*INT=>*/ 0, /*SPR=>*/ 0, /*WIL=>*/ 0, /*LCK=>*/ 0 }, // ATTRIBUTES REQUIRED
                new int[] { -1 }, // PREVIOUS SKILL ID REQUIRED
                true); // Skill Locked at Start

            skills[42] = new Skill(
                42, // SKILL ID
                "Skill #42", // SKILL NAME
                1, // MAX RANK
                new string[]
                {
                    "<i>None</i>",
                    "Test 1",
                    "Test 2",
                    "Test 3"
                },
                new float[] { 0, 1.0f, 1.1f, 1.2f }, // SKILL EFFECTS
                new int[] { /*STR=>*/ 0, /*END=>*/ 0, /*VIT=>*/ 0, /*PRE=>*/ 0, /*DEX=>*/ 0, /*EXP=>*/ 0, /*INT=>*/ 0, /*SPR=>*/ 0, /*WIL=>*/ 0, /*LCK=>*/ 0 }, // ATTRIBUTES REQUIRED
                new int[] { -1 }, // PREVIOUS SKILL ID REQUIRED
                true); // Skill Locked at Start

            skills[43] = new Skill(
                43, // SKILL ID
                "Skill #43", // SKILL NAME
                1, // MAX RANK
                new string[]
                {
                    "<i>None</i>",
                    "Test 1",
                    "Test 2",
                    "Test 3"
                },
                new float[] { 0, 1.0f, 1.1f, 1.2f }, // SKILL EFFECTS
                new int[] { /*STR=>*/ 0, /*END=>*/ 0, /*VIT=>*/ 0, /*PRE=>*/ 0, /*DEX=>*/ 0, /*EXP=>*/ 0, /*INT=>*/ 0, /*SPR=>*/ 0, /*WIL=>*/ 0, /*LCK=>*/ 0 }, // ATTRIBUTES REQUIRED
                new int[] { -1 }, // PREVIOUS SKILL ID REQUIRED
                true); // Skill Locked at Start

            skills[44] = new Skill(
                44, // SKILL ID
                "Skill #44", // SKILL NAME
                1, // MAX RANK
                new string[]
                {
                    "<i>None</i>",
                    "Test 1",
                    "Test 2",
                    "Test 3"
                },
                new float[] { 0, 1.0f, 1.1f, 1.2f }, // SKILL EFFECTS
                new int[] { /*STR=>*/ 0, /*END=>*/ 0, /*VIT=>*/ 0, /*PRE=>*/ 0, /*DEX=>*/ 0, /*EXP=>*/ 0, /*INT=>*/ 0, /*SPR=>*/ 0, /*WIL=>*/ 0, /*LCK=>*/ 0 }, // ATTRIBUTES REQUIRED
                new int[] { -1 }, // PREVIOUS SKILL ID REQUIRED
                true); // Skill Locked at Start

            // store into dictionary for quicker lookups when skills need to be referenced
            foreach (var skill in skills)
                skillsDict[skill.ID] = skill;
        }

        public float GetSkillEffectByID(int skillID)
        {
            return GetSkillByID(skillID).CurrentEffect();
        }

        public int GetSkillRankByID(int skillID)
        {
            return GetSkillByID(skillID).CurrentRank();
        }

        public string GetSkillDescriptionByID(int skillID)
        {
            return GetSkillByID(skillID).CurrentDescription();
        }

        public Skill GetSkillByID(int skillID)
        {
            if (skillsDict.TryGetValue(skillID, out Skill skill))
            {
                return skill;
            }
            else
            {
                Debug.Log($"Skill with ID {skillID} was not found.");
                return null;
            }
        }

        //CALCULATE STATS:

        #region STAMINA
        private float CalculateStaminaGain(int enduranceLevel)
        {
            if (enduranceLevel <= 5) return 10.0f;
            // Calculate the group index based on the endurance level
            int groupIndex = (enduranceLevel - 1) / 10;

            // Define the stamina gains for each group
            float[] staminaGains = { 5.0f, 3.0f, 2.5f, 1.75f, 1.5f, 1.25f, 1.0f, 0.75f, 0.5f, 0.25f };

            // Ensure the group index is within the bounds of the array
            groupIndex = Mathf.Clamp(groupIndex, 0, staminaGains.Length - 1);

            // Return the stamina gain for the corresponding group
            return staminaGains[groupIndex];
        }

        public int CalculateStaminaBasedOnEnduranceLevel(int _endurance)
        {
            if (infiniteStamina)
            {
                return 10000000;
            }

            float totalStamina = 0;

            for (int i = 0; i < _endurance; i++)
            {
                totalStamina += CalculateStaminaGain(i + 1);
            }

            return Mathf.RoundToInt(totalStamina);
        }
        #endregion

        #region HEALTH
        private float CalculateHealthGain(int vitalityLevel)
        {
            if (vitalityLevel <= 5) return 10.0f;
            // Calculate the group index based on the endurance level
            int groupIndex = (vitalityLevel - 1) / 10;

            // Define the stamina gains for each group
            float[] healthGains = { 5.0f, 3.0f, 2.5f, 1.75f, 1.5f, 1.25f, 1.0f, 0.75f, 0.5f, 0.25f };

            // Ensure the group index is within the bounds of the array
            groupIndex = Mathf.Clamp(groupIndex, 0, healthGains.Length - 1);

            // Return the stamina gain for the corresponding group
            return healthGains[groupIndex];
        }

        public int CalculateHealthBasedOnVitalityLevel(int _vitality)
        {
            float totalHealth = 0;

            for (int i = 0; i < _vitality; i++)
            {
                totalHealth += CalculateHealthGain(i + 1);
            }

            return Mathf.RoundToInt(totalHealth);
        }
        #endregion

        #region MANA
        private float CalculateManaGain(int intelligenceLevel)
        {
            if (intelligenceLevel <= 5) return 10.0f;
            // Calculate the group index based on the endurance level
            int groupIndex = (intelligenceLevel - 1) / 10;

            // Define the stamina gains for each group
            float[] manaGains = { 5.0f, 3.0f, 2.5f, 1.75f, 1.5f, 1.25f, 1.0f, 0.75f, 0.5f, 0.25f };

            // Ensure the group index is within the bounds of the array
            groupIndex = Mathf.Clamp(groupIndex, 0, manaGains.Length - 1);

            // Return the stamina gain for the corresponding group
            return manaGains[groupIndex];
        }

        public int CalculateManaBasedOnIntelligenceLevel(int _intelligence)
        {
            if (infiniteMana)
            {
                return 10000000;
            }

            float totalMana = 0;

            for (int i = 0; i < _intelligence; i++)
            {
                totalMana += CalculateManaGain(i + 1);
            }

            return Mathf.RoundToInt(totalMana);
        }
        #endregion

        public float CalculateCritChanceBasedOnPrecisionAndLuck(int _precision, int _luck)
        {
            float critChance = 0;
            critChance = (_precision / 2.4f) + (_luck / 10.0f) - 0.1666f; //simple
            return critChance;
        }

        public float CalculateCritDamageBasedOnDexterity(int _dexterity)
        {
            float critDamage = 0;
            critDamage = 32 + (_dexterity * 1.8f); 
            return critDamage;
        }

        public float CalculateStealthMultiplier()
        {
            float multiplier = GetSkillEffectByID(4);
            float dexSneakBonus = Mathf.Min(dexterity * 0.101f, 10.0f);   // 0.1 per dexterity, cap at 10x (not counting Critical Ambush)

            return multiplier + dexSneakBonus;
        }

        public void RegenerateStamina()
        {
            if (playerLocomotion.isSprinting || inputHandler.isAttacking)
            {
                staminaRegenTimer = 0;
                return;
            }

            if (inputHandler.isPerformingAction && !playerLocomotion.onMount)
            {
                staminaRegenTimer = 0;
                return;
            }

            // Not Totally sure if I want this, make sure it can't be abused or that it isnt too incentivized that player
            // feels obligated to constantly switch between sheathed and unsheathed

            staminaRegenTimer += Time.deltaTime; //update timer

            if (staminaRegenTimer >= staminaRegenDelay)
            {
                if (currentStamina < maxStamina)
                {
                    staminaTickTimer += Time.deltaTime;

                    if (staminaTickTimer >= staminaTickInterval)
                    {
                        staminaTickTimer = 0;
                        if (currentStamina + (staminaRegenAmount * staminaTickInterval) <= maxStamina)
                        {
                            currentStamina += staminaRegenAmount * staminaTickInterval;
                        }
                        else
                        {
                            currentStamina = maxStamina;
                        }
                    }
                }
            }
        }

        public void SetStaminaRegenScale(float regenSpeed)
        {
            staminaRegenAmount = regenSpeed;
        }

        public void CostStamina(float amount)
        {
            currentStamina -= amount;
            staminaBar.SetStat(currentStamina);
        }

        public void RegenerateMana()
        {
            //NOTE: CHANGE TO IS PERFORMING SPELL, etc.
            if (inputHandler.isPerformingAction)
            {
                manaRegenTimer = 0;
                return;
            }

            //CALCULATE MANA REGEN BASED ON SPIRIT
            float manaRegenAmount = willpower / 50.0f; //Simplified Update Later, add dim returns

            manaRegenTimer += Time.deltaTime; //update timer

            if (manaRegenTimer >= manaRegenDelay)
            {
                if (currentMana < maxMana)
                {
                    manaTickTimer += Time.deltaTime;

                    if (manaTickTimer >= 0.1)
                    {
                        manaTickTimer = 0;
                        if (currentMana + manaRegenAmount <= maxMana)
                        {
                            currentMana += manaRegenAmount;
                        }
                        else
                        {
                            currentMana = maxMana;
                        }
                    }
                }
            }
        }

        public void RestoreHealth(float healAmount, bool fromSpell=false) // from spell means we apply bonus from spirit stat
        {
            //playHealEffecthere/audio
            if (fromSpell)
            {
                healAmount += healAmount * spirit * 0.02f;

                float curativeEfficiencyMultiplier = skills[16].CurrentEffect(); // From Mender Skill
                healAmount *= curativeEfficiencyMultiplier;
               // Debug.Log(curativeEfficiencyMultiplier + "x -> Total Heal: " + healAmount);
            }
            currentHealth += healAmount;
            if (currentHealth > maxHealth) currentHealth = maxHealth;
            healthBar.SetStat(currentHealth);
        }

        /// <summary>
        /// Coroutine to gradually fill health over time after an initial delay.
        /// </summary>
        /// <param name="delayBeforeFilling">Time in seconds to wait before starting to fill health.</param>
        /// <param name="amountPerSecond">Amount of health to restore per second.</param>
        /// <returns></returns>
        public IEnumerator FillHealthOverTime(float delayBeforeFilling, float amountPerSecond)
        {
            // Start with health at 0 and update the health bar
            currentHealth = 0;
            healthBar.SetStat(currentHealth);
            healthBar.transitionSlider.value = 0f;

            // Wait for the specified delay before starting to fill health
            yield return new WaitForSeconds(delayBeforeFilling);

            // Fill health over time until it reaches maxHealth
            while (currentHealth < maxHealth)
            {
                // Increment health based on the amount per second and the frame's delta time
                currentHealth += amountPerSecond * Time.deltaTime;

                // Ensure not to exceed maxHealth within the loop
                if (currentHealth >= maxHealth)
                {
                    currentHealth = maxHealth;
                    healthBar.SetStat(currentHealth);
                    yield break; // Exit the coroutine
                }

                // Update the health bar with the current health
                healthBar.SetStat(currentHealth);
                yield return null; // Wait for the next frame
            }
        }



        public void RestoreMana(float healAmount)
        {
            //playHealEffecthere/audio
            currentMana += healAmount;
            if (currentMana > maxMana) currentMana = maxMana;
            manaBar.SetStat(currentMana);
        }

        public void RestoreStamina(float healAmount)
        {
            //playHealEffecthere/audio
            currentStamina += healAmount;
            if (currentStamina > maxStamina) currentStamina = maxStamina;
            staminaBar.SetStat(currentStamina);
        }

        public void TakeDamage(string attackType, float enemyAttackStat, bool blocked, bool stormguardActive=false, bool canStagger=true, float multiplier=1f)
        {
            if (!iFrames)
            {
                if (playerLocomotion.currentPushable != null)
                {
                    return;
                }

                float damageTaken = 0.0f;
                float damageReduction = 0.0f;
                float armorRating = 0.0f;

                if (attackType == "Physical")
                {
                    // Calculate the damage reduction based on the defender's defense stat
                    /*May need to adjust later for balancing:
                     *  using 100 as the constant:
                        If defenderDefenseStat is 0, damageReduction is 0 (no reduction).
                        If defenderDefenseStat is 100, damageReduction is approximately 0.5 (50% reduction).
                        If defenderDefenseStat is 200, damageReduction is approximately 0.6667 (66.67% reduction).
                        If defenderDefenseStat is 1000, damageReduction is approximately 0.9091 (90.91% reduction).
                        maybe stat cap at 200?*/
                    armorRating = physicalArmorRating + (expertise * 2);
                    //Debug.Log("physicalArmorRating: " + armorRating);
                    damageReduction = armorRating / (armorRating + 100);
                    //Debug.Log("Damage reduction: " + (damageReduction * 100) + "%");

                    // Calculate the damage taken by the defender
                    damageTaken = enemyAttackStat * (1 - damageReduction);

                    // Ensure that the damage taken is non-negative
                    damageTaken = Mathf.Max(damageTaken, 1); //if negative, become 1

                    if (stormguardActive)
                    {
                        //Debug.Log("Damage before stormguard: " + damageTaken);
                        damageTaken *= PlayerStats.instance.skills[24].CurrentEffect();
                        //Debug.Log("Damage after stormguard: " + damageTaken);
                    }
                }
                else if (attackType == "Fire")
                {
                    armorRating = magicalArmorRating + (spirit * 2);

                    // Calculate the damage reduction based on the defender's defense stat
                    damageReduction = armorRating / (armorRating + 50);

                    damageReduction += fireResistance / 100f; //example: 30%

                    // Calculate the damage taken by the defender
                    damageTaken = enemyAttackStat * (1 - damageReduction);

                    // Ensure that the damage taken is non-negative
                    damageTaken = Mathf.Max(damageTaken, 1); //if negative, become 1
                }
                else if (attackType == "Ice")
                {
                    armorRating = magicalArmorRating + spirit;

                    // Calculate the damage reduction based on the defender's defense stat
                    damageReduction = armorRating / (armorRating + 50);

                    damageReduction += iceResistance / 100f; //example: 30%

                    // Calculate the damage taken by the defender
                    damageTaken = enemyAttackStat * (1 - damageReduction);

                    // Ensure that the damage taken is non-negative
                    damageTaken = Mathf.Max(damageTaken, 1); //if negative, become 1
                }
                else if (attackType == "Shock")
                {
                    armorRating = magicalArmorRating + spirit;

                    // Calculate the damage reduction based on the defender's defense stat
                    damageReduction = armorRating / (armorRating + 50);

                    damageReduction += shockResistance / 100f; //example: 30%

                    // Calculate the damage taken by the defender
                    damageTaken = enemyAttackStat * (1 - damageReduction);

                    // Ensure that the damage taken is non-negative
                    damageTaken = Mathf.Max(damageTaken, 1); //if negative, become 1
                }

                if (blocked)
                {
                    CostStamina(CalculateBlockedStaminaRemoved(enemyAttackStat, playerInventory.equippedShield.blockRating));

                    playerAudioManager.PlayBlockedAudio();

                    if (currentStamina >= 0)
                    {
                        playerLocomotion.PlayBlockedAnimation(true);
                        damageTaken *= (1 - CalculateBlockedDamagePercentage());
                        //Debug.Log("Blocked! New Damage Taken: " + damageTaken);
                    }
                    else
                    {
                        playerLocomotion.PlayBlockedAnimation(false);
                        damageTaken *= GetSkillEffectByID(7); //take percentage of damage on broken block, based on skill
                        if (GetSkillRankByID(7) < 3 && canStagger) playerLocomotion.PlayStaggerAnimation();
                    }

                    attacksBlocked += 1;
                    if (skills[7].skillLocked)
                    {
                        if (attacksBlocked >= attacksBlockedToUnlockUnwaveringSkill)
                        {
                            skills[7].BecomeAvailable("Many attacks blocked. \"Unwavering\" skill now available to unlock.");
                        }
                    }
                }
                else
                {
                    playerAudioManager.PlayGetHitAudio(attackType == "Physical");
                    playerAudioManager.PlayGetHitVoiceAudio(gender == "mars");
                    // can consider adding a formula here to determine if staggered or not, especially for spells!
                    if(canStagger) playerLocomotion.PlayStaggerAnimation();
                    inputHandler.heavyAttackInput = false; // There was an issue where 

                }

                damageTaken *= multiplier;
                currentHealth -= damageTaken;
                healthBar.SetStat(currentHealth);

                CheckDeath();
            }
        }

        public void CheckDeath()
        {
            if(currentHealth <= 0)
            {
                if (!dead)
                {
                    dead = true;
                    if (DialogueMenu.instance != null) DialogueMenu.instance.CloseDialogueMenu(true);
                    Debug.Log("PLAYER DEAD");

                    if (playerLocomotion.onMount)
                    {
                        playerLocomotion.FinishDismount();
                        playerLocomotion.animatorHandler.PlayTargetActionAnimation("Death Air", true, true, false, false);
                    }
                    else if (playerLocomotion.isSwimming)
                    {
                        playerLocomotion.animatorHandler.PlayTargetActionAnimation("Death Swimming", true, true, false, false);
                    }
                    else if (inputHandler.isGrounded)
                    {
                        playerLocomotion.animatorHandler.PlayTargetActionAnimation("Death", true, true, false, false);
                    }
                    else
                    {
                        playerLocomotion.animatorHandler.PlayTargetActionAnimation("Death Air", true, true, false, false);
                    }

                    inputHandler.isAttacking = false;
                    inputHandler.canQueueAttack = false;

                    // Disable all attack colliders
                    AnimEvents.instance.DeactivateWeapon();

                    // Disable all dodging
                    AnimEvents.instance.DodgeFramesEnd();
                    AnimEvents.instance.DodgeEndFlag();

                    // Disable select spells/effects
                    AnimEvents.instance.StopRejuvenate();
                    AnimEvents.instance.StopStormguard();
                    AnimEvents.instance.StopInfusion();
                    AnimEvents.instance.StopArcanaLumina();
                    AnimEvents.instance.StopSpiderVenom();

                    if (inputHandler.noteUI.activeSelf) inputHandler.noteManager.Close();

                    // Reset all inputs
                    inputHandler.ResetInputs();

                    CameraHandler.instance.CancelLockOn();
                }
            }
        }

        public void Respawn()
        {
            AnimEvents animEvents = AnimEvents.instance;
            // Use AnimEvents.instance
            foreach (GameObject obj in animEvents.hideOnDeath)
            {
                obj.SetActive(true);
            }
            animEvents.deathScreenObj.SetActive(false);

            animEvents.CancelDeathCoroutines();

            // Disable all attack colliders
            animEvents.DeactivateWeapon();
            playerLocomotion.StopBlocking();

            currentMana = maxMana;
            manaBar.SetStat(currentMana);
            currentStamina = maxStamina;
            staminaBar.SetStat(currentStamina);

            MountManager.instance.ResetMount();
            playerLocomotion.touchingShallowWater = false;
            playerLocomotion.touchingDeepWater = false;
            MountManager.instance.stepsManagerAI.inShallowWater = false;
            MountManager.instance.stepsManagerMount.inShallowWater = false;
            MountManager.instance.stepsManagerAI.inDeepWater = false;
            MountManager.instance.stepsManagerMount.inDeepWater = false;


            worldStateManager.ReloadScene();
        }

        public void SpendAttackStamina(int attackCombo, string attackType="Standard")
        {
            //The following formual calculates stamina used, more stamina as the attack combo increases
            //Some applications such as sprint attacks use arbitrary "attackcombos" such as 10 to calculate an appropriate stamina cost
            //Called in PlayerLocomotion for sprint attacks and air attacks
            //With an attackBaseStamina cost of 15:
            //0: 15, 1: 15, 2: 16.5, 3: 18, 4: 19.5, 5: 21, 6: 22.5, 7: 24, 8: 25.5, 9: 27, 10: 28.5, 11: 30, 12: 31.5, 13: 33, 14: 34.5, 15: 36,
            //16: 37.5, 17: 39, 18: 40.5, 19: 42, 20: 43.5, 21: 45, 22: 46.5, 23: 48, 24: 49.5, 25: 51, 26: 52.5, 27: 54, 28: 55.5, 29: 57, 30: 58.5

            //Debug.Log("Attack Combo: " + attackCombo + ", Stamina Cost: " + (attackBaseStaminaCost * (1 + ((attackCombo - 1) * 0.1f))));

            if (attackType == "Standard")
            {
                currentStamina -= (attackBaseStaminaCost * (1 + ((attackCombo - 1) * 0.1f)));
            }
            else if (attackType == "HeavyAttack")
            {
                float staminaUseMultiplier = 2.0f;
                if (GetSkillRankByID(1) >= 3) staminaUseMultiplier = 1.5f;
                currentStamina -= ((attackBaseStaminaCost * staminaUseMultiplier) * (1 + ((attackCombo - 1) * 0.1f)));
            }
            else if(attackType=="AirAttack")
            {
                currentStamina -= airAttackStaminaCost;
            }
            else if (attackType == "LightSprintAttack")
            {
                currentStamina -= lightSprintAttackStaminaCost;
            }
            else if (attackType == "HeavySprintAttack")
            {
                currentStamina -= heavySprintAttackStaminaCost;
            }
            else if(attackType == "UnarmedLight")
            {
                float staminaMultiplier = GetSkillByID(11).GetSpecificEffect(GetSkillRankByID(11) + 6);
                currentStamina -= attackBaseStaminaCost * staminaMultiplier;
            }
            else if (attackType == "UnarmedHeavy")
            {
                float staminaMultiplier = GetSkillByID(11).GetSpecificEffect(GetSkillRankByID(11) + 6);
                currentStamina -= attackBaseStaminaCost * 2 * staminaMultiplier;
            }
            else
            {
                Debug.LogError("Invalid string for spend attack stamina");
            }

            staminaBar.SetStat(currentStamina);
            staminaRegenTimer = 0;
        }

        public void SpendSpellMana(float spellCost)
        {
            currentMana -= spellCost;
            manaBar.SetStat(currentMana);

            manaRegenTimer = 0;

            //Update Spell HUD 
            spellsHUDManager.CheckManaAndSetColor();
        }

        float CalculateBlockedStaminaRemoved(float enemyAttackStat, float playerBlockRating)
        {
            // Stop stamina regen (or rather, make it wait for stamina regen delay)
            staminaRegenTimer = 0;

            // Adjusted scaling factors for stamina and minimal damage
            float maxStaminaRemoved = 40f;  // Maximum stamina to be removed (baseline, increases with high input)
            float staminaScalingFactor = maxStaminaRemoved / (2f * enemyAttackStat + playerBlockRating);  // Adjust as needed

            // Calculate the baseline stamina cost
            float baselineStaminaCost = enemyAttackStat * 1f; // Adjust this scaling factor as needed

            // Calculate the stamina removed
            float staminaRemoved = Mathf.Max(0f, enemyAttackStat - playerBlockRating) * staminaScalingFactor + baselineStaminaCost;

            float skillBonus = 1f - skills[2].CurrentEffect();
            Debug.Log("Before bonus: " + staminaRemoved + " stamina to be removed.");
            staminaRemoved *= skillBonus;
            Debug.Log("After bonus: " + staminaRemoved + " stamina to be removed.");
            // Ensure a minimum stamina removal of 5
            staminaRemoved = Mathf.Max(staminaRemoved, 5f);

            //Debug.Log("Stamina Removed: " + staminaRemoved);
            return staminaRemoved;
        }

        float CalculateBlockedDamagePercentage()
        {
            // Define the percentage range (from 95% to 99%) and the corresponding value range (from 10 to 200)
            float minValue = 10f;
            float maxValue = 200f;
            /*By changing minPercentage, can shift the lower bound of the percentage range, and the calculation will adjust accordingly. 
              Values between the specified range will be linearly scaled based on the new minimum and maximum percentages.*/
            float minPercentage = 0.95f;
            float maxPercentage = 0.99f;

            // Get the player's shield block rating
            float shieldBlockRating = playerInventory.equippedShield.blockRating;

            // Ensure that the shieldBlockRating does not exceed the maximum value in the range
            float clampedY = Mathf.Clamp(shieldBlockRating, minValue, maxValue); // Swap minValue and maxValue

            // Calculate the scaled value based on the clamped percentage
            float percentage = minPercentage + (clampedY - minValue) * (maxPercentage - minPercentage) / (maxValue - minValue); // Swap minValue and maxValue

            return percentage;
        }

        public void CalculateEffectiveStats()
        {
            int sumPhysicalArmorRating = 0;
            int sumMagicalArmorRating = 0;
            int sumStrength = 0;
            int sumEndurance = 0;
            int sumVitality = 0;
            int sumPrecision = 0;
            int sumDexterity = 0;
            int sumExpertise = 0;
            int sumIntelligence = 0;
            int sumSpirit = 0;
            int sumWillpower = 0;
            int sumLuck = 0;
            float sumCriticalChance = 0;
            float sumCriticalDamage = 0;
            float sumFireResistance = 0;
            float sumIceResistance = 0;
            float sumShockResistance = 0;

            if (playerInventory.equippedWeapon != null)
            {
                sumStrength += playerInventory.equippedWeapon.strength;
                sumEndurance += playerInventory.equippedWeapon.endurance;
                sumVitality += playerInventory.equippedWeapon.vitality;
                sumPrecision += playerInventory.equippedWeapon.precision;
                sumDexterity += playerInventory.equippedWeapon.dexterity;
                sumExpertise += playerInventory.equippedWeapon.expertise;
                sumIntelligence += playerInventory.equippedWeapon.intelligence;
                sumSpirit += playerInventory.equippedWeapon.spirit;
                sumWillpower += playerInventory.equippedWeapon.willpower;
                sumLuck += playerInventory.equippedWeapon.luck;
                sumCriticalChance += playerInventory.equippedWeapon.criticalChance;
                sumCriticalDamage += playerInventory.equippedWeapon.criticalDamage;
            }

            if (playerInventory.equippedShield != null)
            {
                sumStrength += playerInventory.equippedShield.strength;
                sumEndurance += playerInventory.equippedShield.endurance;
                sumVitality += playerInventory.equippedShield.vitality;
                sumPrecision += playerInventory.equippedShield.precision;
                sumDexterity += playerInventory.equippedShield.dexterity;
                sumExpertise += playerInventory.equippedShield.expertise;
                sumIntelligence += playerInventory.equippedShield.intelligence;
                sumSpirit += playerInventory.equippedShield.spirit;
                sumWillpower += playerInventory.equippedShield.willpower;
                sumLuck += playerInventory.equippedShield.luck;
                sumFireResistance += playerInventory.equippedShield.fireResistance;
                sumIceResistance += playerInventory.equippedShield.iceResistance;
                sumShockResistance += playerInventory.equippedShield.shockResistance;
            }

            if (playerInventory.equippedTorsoArmor != null)
            {
                sumPhysicalArmorRating += playerInventory.equippedTorsoArmor.physicalArmorRating;
                sumMagicalArmorRating += playerInventory.equippedTorsoArmor.magicalArmorRating;
                sumStrength += playerInventory.equippedTorsoArmor.strength;
                sumEndurance += playerInventory.equippedTorsoArmor.endurance;
                sumVitality += playerInventory.equippedTorsoArmor.vitality;
                sumPrecision += playerInventory.equippedTorsoArmor.precision;
                sumDexterity += playerInventory.equippedTorsoArmor.dexterity;
                sumExpertise += playerInventory.equippedTorsoArmor.expertise;
                sumIntelligence += playerInventory.equippedTorsoArmor.intelligence;
                sumSpirit += playerInventory.equippedTorsoArmor.spirit;
                sumWillpower += playerInventory.equippedTorsoArmor.willpower;
                sumLuck += playerInventory.equippedTorsoArmor.luck;
                sumCriticalChance += playerInventory.equippedTorsoArmor.criticalChance;
                sumCriticalDamage += playerInventory.equippedTorsoArmor.criticalDamage;
                sumFireResistance += playerInventory.equippedTorsoArmor.fireResistance;
                sumIceResistance += playerInventory.equippedTorsoArmor.iceResistance;
                sumShockResistance += playerInventory.equippedTorsoArmor.shockResistance;
            }


            if (playerInventory.equippedHandsArmor != null)
            {
                sumPhysicalArmorRating += playerInventory.equippedHandsArmor.physicalArmorRating;
                sumMagicalArmorRating += playerInventory.equippedHandsArmor.magicalArmorRating;
                sumStrength += playerInventory.equippedHandsArmor.strength;
                sumPrecision += playerInventory.equippedHandsArmor.precision;
                sumIntelligence += playerInventory.equippedHandsArmor.intelligence;
                sumSpirit += playerInventory.equippedHandsArmor.spirit;
                sumLuck += playerInventory.equippedHandsArmor.luck;
                sumCriticalChance += playerInventory.equippedHandsArmor.criticalChance;
                sumCriticalDamage += playerInventory.equippedHandsArmor.criticalDamage;
            }

            if (playerInventory.equippedLegsArmor != null)
            {
                sumPhysicalArmorRating += playerInventory.equippedLegsArmor.physicalArmorRating;
                sumMagicalArmorRating += playerInventory.equippedLegsArmor.magicalArmorRating;
                sumEndurance += playerInventory.equippedLegsArmor.endurance;
                sumVitality += playerInventory.equippedLegsArmor.vitality;
                sumDexterity += playerInventory.equippedLegsArmor.dexterity;
                sumExpertise += playerInventory.equippedLegsArmor.expertise;
                sumWillpower += playerInventory.equippedLegsArmor.willpower;
                sumFireResistance += playerInventory.equippedLegsArmor.fireResistance;
                sumIceResistance += playerInventory.equippedLegsArmor.iceResistance;
                sumShockResistance += playerInventory.equippedLegsArmor.shockResistance;
            }

            if (playerInventory.equippedRing1 != null)
            {
                sumStrength += playerInventory.equippedRing1.strength;
                sumEndurance += playerInventory.equippedRing1.endurance;
                sumVitality += playerInventory.equippedRing1.vitality;
                sumPrecision += playerInventory.equippedRing1.precision;
                sumDexterity += playerInventory.equippedRing1.dexterity;
                sumExpertise += playerInventory.equippedRing1.expertise;
                sumIntelligence += playerInventory.equippedRing1.intelligence;
                sumSpirit += playerInventory.equippedRing1.spirit;
                sumWillpower += playerInventory.equippedRing1.willpower;
                sumLuck += playerInventory.equippedRing1.luck;
                sumCriticalChance += playerInventory.equippedRing1.criticalChance;
                sumCriticalDamage += playerInventory.equippedRing1.criticalDamage;
                sumFireResistance += playerInventory.equippedRing1.fireResistance;
                sumIceResistance += playerInventory.equippedRing1.iceResistance;
                sumShockResistance += playerInventory.equippedRing1.shockResistance;
            }

            if (playerInventory.equippedRing2 != null)
            {
                sumStrength += playerInventory.equippedRing2.strength;
                sumEndurance += playerInventory.equippedRing2.endurance;
                sumVitality += playerInventory.equippedRing2.vitality;
                sumPrecision += playerInventory.equippedRing2.precision;
                sumDexterity += playerInventory.equippedRing2.dexterity;
                sumExpertise += playerInventory.equippedRing2.expertise;
                sumIntelligence += playerInventory.equippedRing2.intelligence;
                sumSpirit += playerInventory.equippedRing2.spirit;
                sumWillpower += playerInventory.equippedRing2.willpower;
                sumLuck += playerInventory.equippedRing2.luck;
                sumCriticalChance += playerInventory.equippedRing2.criticalChance;
                sumCriticalDamage += playerInventory.equippedRing2.criticalDamage;
                sumFireResistance += playerInventory.equippedRing2.fireResistance;
                sumIceResistance += playerInventory.equippedRing2.iceResistance;
                sumShockResistance += playerInventory.equippedRing2.shockResistance;
            }

            if (playerInventory.equippedAmulet != null)
            {
                sumCriticalChance += playerInventory.equippedAmulet.criticalChance;
                sumCriticalDamage += playerInventory.equippedAmulet.criticalDamage;
                sumFireResistance += playerInventory.equippedAmulet.fireResistance;
                sumIceResistance += playerInventory.equippedAmulet.iceResistance;
                sumShockResistance += playerInventory.equippedAmulet.shockResistance;
            }

            //ADD ALL:
            physicalArmorRating = basePhysicalArmorRating + sumPhysicalArmorRating;
            magicalArmorRating = baseMagicalArmorRating + sumMagicalArmorRating;
            strength = baseStrength + sumStrength;
            endurance = baseEndurance + sumEndurance;
            vitality = baseVitality + sumVitality;
            precision = basePrecision + sumPrecision;
            dexterity = baseDexterity + sumDexterity;
            expertise = baseExpertise + sumExpertise;
            intelligence = baseIntelligence + sumIntelligence;
            spirit = baseSpirit + sumSpirit;
            willpower = baseWillpower + sumWillpower;
            luck = baseLuck + sumLuck;
            criticalChanceFromGear = sumCriticalChance;
            criticalDamageFromGear = sumCriticalDamage;
            criticalChance = Mathf.Min(baseCriticalChance + sumCriticalChance + CalculateCritChanceBasedOnPrecisionAndLuck(precision, luck), 100f);
            criticalDamage = baseCriticalDamage + sumCriticalDamage + CalculateCritDamageBasedOnDexterity(dexterity);
            fireResistance = Mathf.Min(baseFireResistance + sumFireResistance, 100f);
            iceResistance = Mathf.Min(baseIceResistance + sumIceResistance, 100f);
            shockResistance = Mathf.Min(baseShockResistance + sumShockResistance, 100f);


            maxStamina = CalculateStaminaBasedOnEnduranceLevel(endurance);
            maxHealth = CalculateHealthBasedOnVitalityLevel(vitality);
            maxMana = CalculateManaBasedOnIntelligenceLevel(intelligence);

            staminaBar.SetStat(currentStamina);
            staminaBar.SetMaxStat(maxStamina, false);
            healthBar.SetStat(currentHealth);
            healthBar.SetMaxStat(maxHealth, false);
            manaBar.SetStat(currentMana);
            manaBar.SetMaxStat(maxMana, false);

            // Prevent stat overflow, cap to new maximum
            currentHealth = Mathf.Clamp(currentHealth, 1f, maxHealth);
            currentMana = Mathf.Clamp(currentMana, 1f, maxMana);
            currentStamina = Mathf.Clamp(currentStamina, 1f, maxStamina);

            if (playerMenu.activeSelf) // Prevent undefined reference if equipping without being in menu (before it is instantiated)
            {
                UpdateStatsScreen(skillSection.activeSelf);
            }

        }

        //STAT PREVIEWS:

        public void UpdateStatsScreen(bool baseStatsVersion = false)
        {
            if (playerInventory.equippedWeapon != null)
            {
                if(playerInventory.equippedWeapon == playerInventory.emptyWeapon)
                {
                    float damage = playerInventory.emptyWeapon.physicalDamage + strength + (strength * skills[11].CurrentEffect());
                    statBoxTexts[0].text = Mathf.Round(damage).ToString();
                    statBoxTexts_shop[0].text = Mathf.Round(damage).ToString();
                }
                else
                {
                    statBoxTexts[0].text = (strength + playerInventory.equippedWeapon.physicalDamage).ToString();
                    statBoxTexts_shop[0].text = (strength + playerInventory.equippedWeapon.physicalDamage).ToString();
                }

                statBoxTexts[1].text = (intelligence + playerInventory.equippedWeapon.magicDamage).ToString();
                statBoxTexts_shop[1].text = (intelligence + playerInventory.equippedWeapon.magicDamage).ToString();
            }
            statBoxTexts[2].text = playerInventory.equippedShield.blockRating.ToString();
            statBoxTexts[3].text = physicalArmorRating.ToString();
            statBoxTexts[4].text = magicalArmorRating.ToString();

            statBoxTexts_shop[2].text = playerInventory.equippedShield.blockRating.ToString();
            statBoxTexts_shop[3].text = physicalArmorRating.ToString();
            statBoxTexts_shop[4].text = magicalArmorRating.ToString();
            if (baseStatsVersion)
            {
                DisableAllStatPreviews();
                primaryStatsText.text = "BASE STATS (PRIMARY)";
                secondaryStatsText.text = "BASE STATS (SECONDARY)";
                statBoxTexts[5].text = baseStrength.ToString();
                statBoxTexts[6].text = baseEndurance.ToString();
                statBoxTexts[7].text = baseVitality.ToString();
                statBoxTexts[8].text = basePrecision.ToString();
                statBoxTexts[9].text = baseDexterity.ToString();
                statBoxTexts[10].text = baseExpertise.ToString();
                statBoxTexts[11].text = baseIntelligence.ToString();
                statBoxTexts[12].text = baseSpirit.ToString();
                statBoxTexts[13].text = baseWillpower.ToString();
                statBoxTexts[14].text = baseLuck.ToString();
                statBoxTexts[15].text = baseCriticalChance.ToString("0.0") + "%";
                statBoxTexts[16].text = baseCriticalDamage.ToString("0.0") + "%";
                statBoxTexts[17].text = baseFireResistance.ToString("0.0") + "%";
                statBoxTexts[18].text = baseIceResistance.ToString("0.0") + "%";
                statBoxTexts[19].text = baseShockResistance.ToString("0.0") + "%";
            }
            else
            {
                primaryStatsText.text = "PRIMARY STATS";
                secondaryStatsText.text = "SECONDARY STATS";
                statBoxTexts[5].text = strength.ToString();
                statBoxTexts[6].text = endurance.ToString();
                statBoxTexts[7].text = vitality.ToString();
                statBoxTexts[8].text = precision.ToString();
                statBoxTexts[9].text = dexterity.ToString();
                statBoxTexts[10].text = expertise.ToString();
                statBoxTexts[11].text = intelligence.ToString();
                statBoxTexts[12].text = spirit.ToString();
                statBoxTexts[13].text = willpower.ToString();
                statBoxTexts[14].text = luck.ToString();
                statBoxTexts[15].text = criticalChance.ToString("0.0") + "%";
                statBoxTexts[16].text = criticalDamage.ToString("0.0") + "%";
                statBoxTexts[17].text = fireResistance.ToString("0.0") + "%";
                statBoxTexts[18].text = iceResistance.ToString("0.0") + "%";
                statBoxTexts[19].text = shockResistance.ToString("0.0") + "%";

                statBoxTexts_shop[5].text = strength.ToString();
                statBoxTexts_shop[6].text = endurance.ToString();
                statBoxTexts_shop[7].text = vitality.ToString();
                statBoxTexts_shop[8].text = precision.ToString();
                statBoxTexts_shop[9].text = dexterity.ToString();
                statBoxTexts_shop[10].text = expertise.ToString();
                statBoxTexts_shop[11].text = intelligence.ToString();
                statBoxTexts_shop[12].text = spirit.ToString();
                statBoxTexts_shop[13].text = willpower.ToString();
                statBoxTexts_shop[14].text = luck.ToString();
                statBoxTexts_shop[15].text = criticalChance.ToString("0.0") + "%";
                statBoxTexts_shop[16].text = criticalDamage.ToString("0.0") + "%";
                statBoxTexts_shop[17].text = fireResistance.ToString("0.0") + "%";
                statBoxTexts_shop[18].text = iceResistance.ToString("0.0") + "%";
                statBoxTexts_shop[19].text = shockResistance.ToString("0.0") + "%";
            }


            if(0 < currentHealth && currentHealth < 1)
            {
                statBoxTexts[20].text = "1 / " + maxHealth.ToString() + " HP";

                statBoxTexts_shop[20].text = "1 / " + maxHealth.ToString() + " HP";
            }
            else
            {
                statBoxTexts[20].text = currentHealth.ToString("0.") + " / " + maxHealth.ToString() + " HP";

                statBoxTexts_shop[20].text = currentHealth.ToString("0.") + " / " + maxHealth.ToString() + " HP";
            }
            statBoxTexts[21].text = currentMana.ToString("0.") + " / " + maxMana.ToString() + " MP";
            statBoxTexts[22].text = currentStamina.ToString("0.") + " / " + maxStamina.ToString() + " SP";

            statBoxTexts_shop[21].text = currentMana.ToString("0.") + " / " + maxMana.ToString() + " MP";
            statBoxTexts_shop[22].text = currentStamina.ToString("0.") + " / " + maxStamina.ToString() + " SP";
        }

        public void DisableAllStatPreviews()
        {
            for (int i = 0; i < statBoxPreviewTexts.Length; i++)
            {
                statBoxPreviewTexts[i].transform.gameObject.SetActive(false);
            }
            for (int i = 0; i < statBoxPreviewTexts_shop.Length; i++)
            {
                statBoxPreviewTexts_shop[i].transform.gameObject.SetActive(false);
            }
        }

        public void PreviewWeaponStatChanges(WeaponItem newWeapon)
        {      
            WeaponItem currentWeapon = playerInventory.equippedWeapon;

            //First, find stats without current weapon
            int newStrength = (strength - currentWeapon.strength) + newWeapon.strength;
            int newEndurance = (endurance - currentWeapon.endurance) + newWeapon.endurance;
            int newVitality = (vitality - currentWeapon.vitality) + newWeapon.vitality;
            int newPrecision = (precision - currentWeapon.precision) + newWeapon.precision;
            int newDexterity = (dexterity - currentWeapon.dexterity) + newWeapon.dexterity;
            int newExpertise = (expertise - currentWeapon.expertise) + newWeapon.expertise;
            int newIntelligence = (intelligence - currentWeapon.intelligence) + newWeapon.intelligence;
            int newSpirit = (spirit - currentWeapon.spirit) + newWeapon.spirit;
            int newWillpower = (willpower - currentWeapon.willpower) + newWeapon.willpower;
            int newLuck = (luck - currentWeapon.luck) + newWeapon.luck;

            // Crit (effected by attributes AND other gear stats)
            float totalCriticalChanceFromGear = criticalChanceFromGear - currentWeapon.criticalChance + newWeapon.criticalChance;
            float newCriticalChance = Mathf.Min(baseCriticalChance + totalCriticalChanceFromGear + CalculateCritChanceBasedOnPrecisionAndLuck(newPrecision, newLuck), 100f);

            float totalCriticalDamageFromGear = criticalDamageFromGear - currentWeapon.criticalDamage + newWeapon.criticalDamage;
            float newCriticalDamage = baseCriticalDamage + totalCriticalDamageFromGear + CalculateCritDamageBasedOnDexterity(newDexterity);

            float newMaxHealth = CalculateHealthBasedOnVitalityLevel(newVitality);
            float newMaxMana = CalculateManaBasedOnIntelligenceLevel(newIntelligence);
            float newMaxStamina = CalculateStaminaBasedOnEnduranceLevel(newEndurance);

            // If not unequipping
            if (newWeapon != currentWeapon)
            {
                if(currentWeapon == playerInventory.emptyWeapon)
                {
                    float damage = playerInventory.emptyWeapon.physicalDamage + strength + (strength * skills[11].CurrentEffect());
                    UpdatePhysicalDamagePreviewText(newWeapon.physicalDamage + newStrength, Mathf.Round(damage));
                }
                else
                {
                    UpdatePhysicalDamagePreviewText(newWeapon.physicalDamage + newStrength, currentWeapon.physicalDamage + strength);
                }
                UpdateMagicDamagePreviewText(newWeapon.magicDamage + newIntelligence, currentWeapon.magicDamage + intelligence);
                UpdateStrengthPreviewText(newStrength);
                UpdateEndurancePreviewText(newEndurance);
                UpdateVitalityPreviewText(newVitality);
                UpdatePrecisionPreviewText(newPrecision);
                UpdateDexterityPreviewText(newDexterity);
                UpdateExpertisePreviewText(newExpertise);
                UpdateIntelligencePreviewText(newIntelligence);
                UpdateSpiritPreviewText(newSpirit);
                UpdateWillpowerPreviewText(newWillpower);
                UpdateLuckPreviewText(newLuck);

                UpdateCriticalChancePreviewText(newCriticalChance);
                UpdateCriticalDamagePreviewText(newCriticalDamage);

                UpdateMaxHealthPreviewText(newMaxHealth);
                UpdateMaxManaPreviewText(newMaxMana);
                UpdateMaxStaminaPreviewText(newMaxStamina);
            }
            else // If unequipping
            {
                float strengthUnequip = strength - currentWeapon.strength;
                float damage = playerInventory.emptyWeapon.physicalDamage + strengthUnequip + (strengthUnequip * skills[11].CurrentEffect());
                UpdatePhysicalDamagePreviewText(Mathf.Round(damage), currentWeapon.physicalDamage + strength);

                float intelligenceUnequip = intelligence - currentWeapon.intelligence;
                UpdateMagicDamagePreviewText(playerInventory.emptyWeapon.magicDamage + intelligenceUnequip, currentWeapon.magicDamage + intelligence);
                UpdateStrengthPreviewText(strengthUnequip);
                UpdateEndurancePreviewText(endurance - currentWeapon.endurance);
                UpdateVitalityPreviewText(vitality - currentWeapon.vitality);
                UpdatePrecisionPreviewText(precision - currentWeapon.precision);
                UpdateDexterityPreviewText(dexterity - currentWeapon.dexterity);
                UpdateExpertisePreviewText(expertise - currentWeapon.expertise);
                UpdateIntelligencePreviewText(intelligence - currentWeapon.intelligence);
                UpdateSpiritPreviewText(spirit - currentWeapon.spirit);
                UpdateWillpowerPreviewText(willpower - currentWeapon.willpower);
                UpdateLuckPreviewText(luck - currentWeapon.luck);

                // Crit (effected by attributes AND other gear stats)
                float criticalChanceUnequip = Mathf.Min(baseCriticalChance + criticalChanceFromGear - currentWeapon.criticalChance + CalculateCritChanceBasedOnPrecisionAndLuck(precision - currentWeapon.precision, luck - currentWeapon.luck), 100f);
                float criticalDamageUnequip = baseCriticalDamage + criticalDamageFromGear - currentWeapon.criticalDamage + CalculateCritDamageBasedOnDexterity(dexterity - currentWeapon.dexterity);

                UpdateCriticalChancePreviewText(criticalChanceUnequip);          
                UpdateCriticalDamagePreviewText(criticalDamageUnequip);
  
                float maxHealthUnequip = CalculateHealthBasedOnVitalityLevel(vitality - currentWeapon.vitality);
                float maxManaUnequip = CalculateManaBasedOnIntelligenceLevel(intelligence - currentWeapon.intelligence);
                float maxStaminaUnequip = CalculateStaminaBasedOnEnduranceLevel(endurance - currentWeapon.endurance);
                UpdateMaxHealthPreviewText(maxHealthUnequip);
                UpdateMaxManaPreviewText(maxManaUnequip);
                UpdateMaxStaminaPreviewText(maxStaminaUnequip);
            }
        }

        public void PreviewShieldStatChanges(ShieldItem newShield)
        {
            ShieldItem currentShield = playerInventory.equippedShield;

            //First, find stats without current weapon
            int newStrength = (strength - currentShield.strength) + newShield.strength;
            int newEndurance = (endurance - currentShield.endurance) + newShield.endurance;
            int newVitality = (vitality - currentShield.vitality) + newShield.vitality;
            int newPrecision = (precision - currentShield.precision) + newShield.precision;
            int newDexterity = (dexterity - currentShield.dexterity) + newShield.dexterity;
            int newExpertise = (expertise - currentShield.expertise) + newShield.expertise;
            int newIntelligence = (intelligence - currentShield.intelligence) + newShield.intelligence;
            int newSpirit = (spirit - currentShield.spirit) + newShield.spirit;
            int newWillpower = (willpower - currentShield.willpower) + newShield.willpower;
            int newLuck = (luck - currentShield.luck) + newShield.luck;

            float newFireResistance = (fireResistance - currentShield.fireResistance) + newShield.fireResistance;
            float newIceResistance = (iceResistance - currentShield.iceResistance) + newShield.iceResistance;
            float newShockResistance = (shockResistance - currentShield.shockResistance) + newShield.shockResistance;

            float newMaxHealth = CalculateHealthBasedOnVitalityLevel(newVitality);
            float newMaxMana = CalculateManaBasedOnIntelligenceLevel(newIntelligence);
            float newMaxStamina = CalculateStaminaBasedOnEnduranceLevel(newEndurance);

            if (newShield != currentShield) //equipping
            {
                UpdateBlockRatingPreviewText(newShield.blockRating, currentShield.blockRating);

                UpdateStrengthPreviewText(newStrength);
                UpdateEndurancePreviewText(newEndurance);
                UpdateVitalityPreviewText(newVitality);
                UpdatePrecisionPreviewText(newPrecision);
                UpdateDexterityPreviewText(newDexterity);
                UpdateExpertisePreviewText(newExpertise);
                UpdateIntelligencePreviewText(newIntelligence);
                UpdateSpiritPreviewText(newSpirit);
                UpdateWillpowerPreviewText(newWillpower);
                UpdateLuckPreviewText(newLuck);

                UpdateFireResistancePreviewText(newFireResistance);
                UpdateIceResistancePreviewText(newIceResistance);
                UpdateShockResistancePreviewText(newShockResistance);

                UpdateMaxHealthPreviewText(newMaxHealth);
                UpdateMaxManaPreviewText(newMaxMana);
                UpdateMaxStaminaPreviewText(newMaxStamina);
            }
            else // unequipping
            {
                UpdateBlockRatingPreviewText(playerInventory.emptyShield.blockRating, currentShield.blockRating);

                UpdateStrengthPreviewText(strength - currentShield.strength);
                UpdateEndurancePreviewText(endurance - currentShield.endurance);
                UpdateVitalityPreviewText(vitality - currentShield.vitality);
                UpdatePrecisionPreviewText(precision - currentShield.precision);
                UpdateDexterityPreviewText(dexterity - currentShield.dexterity);
                UpdateExpertisePreviewText(expertise - currentShield.expertise);
                UpdateIntelligencePreviewText(intelligence - currentShield.intelligence);
                UpdateSpiritPreviewText(spirit - currentShield.spirit);
                UpdateWillpowerPreviewText(willpower - currentShield.willpower);
                UpdateLuckPreviewText(luck - currentShield.luck);

                UpdateFireResistancePreviewText(fireResistance - currentShield.fireResistance);
                UpdateIceResistancePreviewText(iceResistance - currentShield.iceResistance);
                UpdateShockResistancePreviewText(shockResistance - currentShield.shockResistance);

                UpdateMaxHealthPreviewText(CalculateHealthBasedOnVitalityLevel(vitality - currentShield.vitality));
                UpdateMaxManaPreviewText(CalculateManaBasedOnIntelligenceLevel(intelligence - currentShield.intelligence));
                UpdateMaxStaminaPreviewText(CalculateStaminaBasedOnEnduranceLevel(endurance - currentShield.endurance));
            }
        }

        public void PreviewTorsoArmorStatChanges(TorsoArmorItem newTorsoArmor)
        {
            TorsoArmorItem currentTorsoArmor = playerInventory.equippedTorsoArmor;

            //First, find stats without current armor piece
            int newStrength = (strength - currentTorsoArmor.strength) + newTorsoArmor.strength;
            int newEndurance = (endurance - currentTorsoArmor.endurance) + newTorsoArmor.endurance;
            int newVitality = (vitality - currentTorsoArmor.vitality) + newTorsoArmor.vitality;
            int newPrecision = (precision - currentTorsoArmor.precision) + newTorsoArmor.precision;
            int newDexterity = (dexterity - currentTorsoArmor.dexterity) + newTorsoArmor.dexterity;
            int newExpertise = (expertise - currentTorsoArmor.expertise) + newTorsoArmor.expertise;
            int newIntelligence = (intelligence - currentTorsoArmor.intelligence) + newTorsoArmor.intelligence;
            int newSpirit = (spirit - currentTorsoArmor.spirit) + newTorsoArmor.spirit;
            int newWillpower = (willpower - currentTorsoArmor.willpower) + newTorsoArmor.willpower;
            int newLuck = (luck - currentTorsoArmor.luck) + newTorsoArmor.luck;

            // Crit (effected by attributes AND other gear stats)
            float totalCriticalChanceFromGear = criticalChanceFromGear - currentTorsoArmor.criticalChance + newTorsoArmor.criticalChance;
            float newCriticalChance = Mathf.Min(baseCriticalChance + totalCriticalChanceFromGear + CalculateCritChanceBasedOnPrecisionAndLuck(newPrecision, newLuck), 100f);

            float totalCriticalDamageFromGear = criticalDamageFromGear - currentTorsoArmor.criticalDamage + newTorsoArmor.criticalDamage;
            float newCriticalDamage = baseCriticalDamage + totalCriticalDamageFromGear + CalculateCritDamageBasedOnDexterity(newDexterity);

            float newFireResistance = (fireResistance - currentTorsoArmor.fireResistance) + newTorsoArmor.fireResistance;
            float newIceResistance = (iceResistance - currentTorsoArmor.iceResistance) + newTorsoArmor.iceResistance;
            float newShockResistance = (shockResistance - currentTorsoArmor.shockResistance) + newTorsoArmor.shockResistance;

            float newMaxHealth = CalculateHealthBasedOnVitalityLevel(newVitality);
            float newMaxMana = CalculateManaBasedOnIntelligenceLevel(newIntelligence);
            float newMaxStamina = CalculateStaminaBasedOnEnduranceLevel(newEndurance);

            if (newTorsoArmor != currentTorsoArmor) //equipping
            {
                UpdatePhysicalArmorRatingPreviewText(newTorsoArmor.physicalArmorRating, currentTorsoArmor.physicalArmorRating);
                UpdateMagicalArmorRatingPreviewText(newTorsoArmor.magicalArmorRating, currentTorsoArmor.magicalArmorRating);

                UpdateStrengthPreviewText(newStrength);
                UpdateEndurancePreviewText(newEndurance);
                UpdateVitalityPreviewText(newVitality);
                UpdatePrecisionPreviewText(newPrecision);
                UpdateDexterityPreviewText(newDexterity);
                UpdateExpertisePreviewText(newExpertise);
                UpdateIntelligencePreviewText(newIntelligence);
                UpdateSpiritPreviewText(newSpirit);
                UpdateWillpowerPreviewText(newWillpower);
                UpdateLuckPreviewText(newLuck);

                UpdateCriticalChancePreviewText(newCriticalChance);
                UpdateCriticalDamagePreviewText(newCriticalDamage);

                UpdateFireResistancePreviewText(newFireResistance);
                UpdateIceResistancePreviewText(newIceResistance);
                UpdateShockResistancePreviewText(newShockResistance);

                UpdateMaxHealthPreviewText(newMaxHealth);
                UpdateMaxManaPreviewText(newMaxMana);
                UpdateMaxStaminaPreviewText(newMaxStamina);
            }
            else
            {
                UpdatePhysicalArmorRatingPreviewText(playerInventory.emptyTorsoArmor.physicalArmorRating, currentTorsoArmor.physicalArmorRating);
                UpdateMagicalArmorRatingPreviewText(playerInventory.emptyTorsoArmor.magicalArmorRating, currentTorsoArmor.magicalArmorRating);

                UpdateStrengthPreviewText(strength - currentTorsoArmor.strength);
                UpdateEndurancePreviewText(endurance - currentTorsoArmor.endurance);
                UpdateVitalityPreviewText(vitality - currentTorsoArmor.vitality);
                UpdatePrecisionPreviewText(precision - currentTorsoArmor.precision);
                UpdateDexterityPreviewText(dexterity - currentTorsoArmor.dexterity);
                UpdateExpertisePreviewText(expertise - currentTorsoArmor.expertise);
                UpdateIntelligencePreviewText(intelligence - currentTorsoArmor.intelligence);
                UpdateSpiritPreviewText(spirit - currentTorsoArmor.spirit);
                UpdateWillpowerPreviewText(willpower - currentTorsoArmor.willpower);
                UpdateLuckPreviewText(luck - currentTorsoArmor.luck);

                // Crit (effected by attributes AND other gear stats)
                float criticalChanceUnequip = Mathf.Min(baseCriticalChance + criticalChanceFromGear - currentTorsoArmor.criticalChance + CalculateCritChanceBasedOnPrecisionAndLuck(precision - currentTorsoArmor.precision, luck - currentTorsoArmor.luck), 100f);
                float criticalDamageUnequip = baseCriticalDamage + criticalDamageFromGear - currentTorsoArmor.criticalDamage + CalculateCritDamageBasedOnDexterity(dexterity - currentTorsoArmor.dexterity);
                UpdateCriticalChancePreviewText(criticalChanceUnequip);
                UpdateCriticalDamagePreviewText(criticalDamageUnequip);

                UpdateFireResistancePreviewText(fireResistance - currentTorsoArmor.fireResistance);
                UpdateIceResistancePreviewText(iceResistance - currentTorsoArmor.iceResistance);
                UpdateShockResistancePreviewText(shockResistance - currentTorsoArmor.shockResistance);

                UpdateMaxHealthPreviewText(CalculateHealthBasedOnVitalityLevel(vitality - currentTorsoArmor.vitality));
                UpdateMaxManaPreviewText(CalculateManaBasedOnIntelligenceLevel(intelligence - currentTorsoArmor.intelligence));
                UpdateMaxStaminaPreviewText(CalculateStaminaBasedOnEnduranceLevel(endurance - currentTorsoArmor.endurance));
            }
        }

        public void PreviewHandsArmorStatChanges(HandsArmorItem newHandsArmor)
        {
            HandsArmorItem currentHandsArmor = playerInventory.equippedHandsArmor;

            //First, find stats without current armor piece
            int newStrength = (strength - currentHandsArmor.strength) + newHandsArmor.strength;
            int newPrecision = (precision - currentHandsArmor.precision) + newHandsArmor.precision;
            int newIntelligence = (intelligence - currentHandsArmor.intelligence) + newHandsArmor.intelligence;
            int newSpirit = (spirit - currentHandsArmor.spirit) + newHandsArmor.spirit;
            int newLuck = (luck - currentHandsArmor.luck) + newHandsArmor.luck;

            // Crit (effected by attributes AND other gear stats)
            float totalCriticalChanceFromGear = criticalChanceFromGear - currentHandsArmor.criticalChance + newHandsArmor.criticalChance;
            float newCriticalChance = Mathf.Min(baseCriticalChance + totalCriticalChanceFromGear + CalculateCritChanceBasedOnPrecisionAndLuck(newPrecision, newLuck), 100f);

            float totalCriticalDamageFromGear = criticalDamageFromGear - currentHandsArmor.criticalDamage + newHandsArmor.criticalDamage;
            float newCriticalDamage = baseCriticalDamage + totalCriticalDamageFromGear + CalculateCritDamageBasedOnDexterity(dexterity);

            float newMaxMana = CalculateManaBasedOnIntelligenceLevel(newIntelligence);

            if (newHandsArmor != currentHandsArmor)
            {
                UpdatePhysicalArmorRatingPreviewText(newHandsArmor.physicalArmorRating, currentHandsArmor.physicalArmorRating);
                UpdateMagicalArmorRatingPreviewText(newHandsArmor.magicalArmorRating, currentHandsArmor.magicalArmorRating);

                UpdateStrengthPreviewText(newStrength);
                UpdatePrecisionPreviewText(newPrecision);
                UpdateIntelligencePreviewText(newIntelligence);
                UpdateSpiritPreviewText(newSpirit);
                UpdateLuckPreviewText(newLuck);

                UpdateCriticalChancePreviewText(newCriticalChance);
                UpdateCriticalDamagePreviewText(newCriticalDamage);

                UpdateMaxManaPreviewText(newMaxMana);
            }
            else
            {
                UpdatePhysicalArmorRatingPreviewText(playerInventory.emptyHandsArmor.physicalArmorRating, currentHandsArmor.physicalArmorRating);
                UpdateMagicalArmorRatingPreviewText(playerInventory.emptyHandsArmor.magicalArmorRating, currentHandsArmor.magicalArmorRating);

                UpdateStrengthPreviewText(strength - currentHandsArmor.strength);
                UpdatePrecisionPreviewText(precision - currentHandsArmor.precision);
                UpdateIntelligencePreviewText(intelligence - currentHandsArmor.intelligence);
                UpdateSpiritPreviewText(spirit - currentHandsArmor.spirit);
                UpdateLuckPreviewText(luck - currentHandsArmor.luck);

                // Crit (effected by attributes AND other gear stats)
                float criticalChanceUnequip = Mathf.Min(baseCriticalChance + criticalChanceFromGear - currentHandsArmor.criticalChance + CalculateCritChanceBasedOnPrecisionAndLuck(precision - currentHandsArmor.precision, luck - currentHandsArmor.luck), 100f);
                float criticalDamageUnequip = baseCriticalDamage + criticalDamageFromGear - currentHandsArmor.criticalDamage + CalculateCritDamageBasedOnDexterity(dexterity);
                UpdateCriticalChancePreviewText(criticalChanceUnequip);
                UpdateCriticalDamagePreviewText(criticalDamageUnequip);

                UpdateMaxManaPreviewText(newMaxMana);
            }
        }

        public void PreviewLegsArmorStatChanges(LegsArmorItem newLegsArmor)
        {
            LegsArmorItem currentLegsArmor = playerInventory.equippedLegsArmor;

            //First, find stats without current armor piece
            int newEndurance = (endurance - currentLegsArmor.endurance) + newLegsArmor.endurance;
            int newVitality = (vitality - currentLegsArmor.vitality) + newLegsArmor.vitality;
            int newDexterity = (dexterity - currentLegsArmor.dexterity) + newLegsArmor.dexterity;
            int newExpertise = (expertise - currentLegsArmor.expertise) + newLegsArmor.expertise;
            int newWillpower = (willpower - currentLegsArmor.willpower) + newLegsArmor.willpower;

            float newFireResistance = (fireResistance - currentLegsArmor.fireResistance) + newLegsArmor.fireResistance;
            float newIceResistance = (iceResistance - currentLegsArmor.iceResistance) + newLegsArmor.iceResistance;
            float newShockResistance = (shockResistance - currentLegsArmor.shockResistance) + newLegsArmor.shockResistance;

            float newMaxHealth = CalculateHealthBasedOnVitalityLevel(newVitality);
            float newMaxStamina = CalculateStaminaBasedOnEnduranceLevel(newEndurance);

            if (newLegsArmor != currentLegsArmor)
            {
                UpdatePhysicalArmorRatingPreviewText(newLegsArmor.physicalArmorRating, currentLegsArmor.physicalArmorRating);
                UpdateMagicalArmorRatingPreviewText(newLegsArmor.magicalArmorRating, currentLegsArmor.magicalArmorRating);

                UpdateEndurancePreviewText(newEndurance);
                UpdateVitalityPreviewText(newVitality);
                UpdateDexterityPreviewText(newDexterity);
                UpdateExpertisePreviewText(newExpertise);
                UpdateWillpowerPreviewText(newWillpower);

                UpdateFireResistancePreviewText(newFireResistance);
                UpdateIceResistancePreviewText(newIceResistance);
                UpdateShockResistancePreviewText(newShockResistance);

                UpdateMaxHealthPreviewText(newMaxHealth);
                UpdateMaxStaminaPreviewText(newMaxStamina);
            }
            else
            {
                UpdatePhysicalArmorRatingPreviewText(playerInventory.emptyLegsArmor.physicalArmorRating, currentLegsArmor.physicalArmorRating);
                UpdateMagicalArmorRatingPreviewText(playerInventory.emptyLegsArmor.magicalArmorRating, currentLegsArmor.magicalArmorRating);

                UpdateEndurancePreviewText(endurance - currentLegsArmor.endurance);
                UpdateVitalityPreviewText(vitality - currentLegsArmor.vitality);
                UpdateDexterityPreviewText(dexterity - currentLegsArmor.dexterity);
                UpdateExpertisePreviewText(expertise - currentLegsArmor.expertise);
                UpdateWillpowerPreviewText(willpower - currentLegsArmor.willpower);

                UpdateFireResistancePreviewText(fireResistance - currentLegsArmor.fireResistance);
                UpdateIceResistancePreviewText(iceResistance - currentLegsArmor.iceResistance);
                UpdateShockResistancePreviewText(shockResistance - currentLegsArmor.shockResistance);

                UpdateMaxHealthPreviewText(CalculateHealthBasedOnVitalityLevel(vitality - currentLegsArmor.vitality));
                UpdateMaxStaminaPreviewText(CalculateStaminaBasedOnEnduranceLevel(endurance - currentLegsArmor.endurance));
            }
        }

        public void PreviewRingStatChanges(int ringSlot, RingItem newRing)
        {
            RingItem currentRing = null;

            if (ringSlot == 0) //there was an empty ring slot
            {
                currentRing = playerInventory.emptyRing;
            }
            else if(ringSlot == 1)
            {
                currentRing = playerInventory.equippedRing1;
            }
            else if (ringSlot == 2)
            {
                currentRing = playerInventory.equippedRing2;
            }


            int newStrength = (strength - currentRing.strength) + newRing.strength;
            int newEndurance = (endurance - currentRing.endurance) + newRing.endurance;
            int newVitality = (vitality - currentRing.vitality) + newRing.vitality;
            int newPrecision = (precision - currentRing.precision) + newRing.precision;
            int newDexterity = (dexterity - currentRing.dexterity) + newRing.dexterity;
            int newExpertise = (expertise - currentRing.expertise) + newRing.expertise;
            int newIntelligence = (intelligence - currentRing.intelligence) + newRing.intelligence;
            int newSpirit = (spirit - currentRing.spirit) + newRing.spirit;
            int newWillpower = (willpower - currentRing.willpower) + newRing.willpower;
            int newLuck = (luck - currentRing.luck) + newRing.luck;

            // Crit (effected by attributes AND other gear stats)
            float totalCriticalChanceFromGear = criticalChanceFromGear - currentRing.criticalChance + newRing.criticalChance;
            float newCriticalChance = Mathf.Min(baseCriticalChance + totalCriticalChanceFromGear + CalculateCritChanceBasedOnPrecisionAndLuck(newPrecision, newLuck), 100f);

            float totalCriticalDamageFromGear = criticalDamageFromGear - currentRing.criticalDamage + newRing.criticalDamage;
            float newCriticalDamage = baseCriticalDamage + totalCriticalDamageFromGear + CalculateCritDamageBasedOnDexterity(newDexterity);

            float newFireResistance = (fireResistance - currentRing.fireResistance) + newRing.fireResistance;
            float newIceResistance = (iceResistance - currentRing.iceResistance) + newRing.iceResistance;
            float newShockResistance = (shockResistance - currentRing.shockResistance) + newRing.shockResistance;

            float newMaxHealth = CalculateHealthBasedOnVitalityLevel(newVitality);
            float newMaxMana = CalculateManaBasedOnIntelligenceLevel(newIntelligence);
            float newMaxStamina = CalculateStaminaBasedOnEnduranceLevel(newEndurance);

            if (newRing != currentRing)
            {
                UpdateStrengthPreviewText(newStrength);
                UpdateEndurancePreviewText(newEndurance);
                UpdateVitalityPreviewText(newVitality);
                UpdatePrecisionPreviewText(newPrecision);
                UpdateDexterityPreviewText(newDexterity);
                UpdateExpertisePreviewText(newExpertise);
                UpdateIntelligencePreviewText(newIntelligence);
                UpdateSpiritPreviewText(newSpirit);
                UpdateWillpowerPreviewText(newWillpower);
                UpdateLuckPreviewText(newLuck);

                UpdateCriticalChancePreviewText(newCriticalChance);
                UpdateCriticalDamagePreviewText(newCriticalDamage);

                UpdateFireResistancePreviewText(newFireResistance);
                UpdateIceResistancePreviewText(newIceResistance);
                UpdateShockResistancePreviewText(newShockResistance);

                UpdateMaxHealthPreviewText(newMaxHealth);
                UpdateMaxManaPreviewText(newMaxMana);
                UpdateMaxStaminaPreviewText(newMaxStamina);

            }
            else
            {
                UpdateStrengthPreviewText(strength - currentRing.strength);
                UpdateEndurancePreviewText(endurance - currentRing.endurance);
                UpdateVitalityPreviewText(vitality - currentRing.vitality);
                UpdatePrecisionPreviewText(precision - currentRing.precision);
                UpdateDexterityPreviewText(dexterity - currentRing.dexterity);
                UpdateExpertisePreviewText(expertise - currentRing.expertise);
                UpdateIntelligencePreviewText(intelligence - currentRing.intelligence);
                UpdateSpiritPreviewText(spirit - currentRing.spirit);
                UpdateWillpowerPreviewText(willpower - currentRing.willpower);
                UpdateLuckPreviewText(luck - currentRing.luck);

                // Crit (effected by attributes AND other gear stats)
                float criticalChanceUnequip = Mathf.Min(baseCriticalChance + criticalChanceFromGear - currentRing.criticalChance + CalculateCritChanceBasedOnPrecisionAndLuck(precision - currentRing.precision, luck - currentRing.luck), 100f);
                float criticalDamageUnequip = baseCriticalDamage + criticalDamageFromGear - currentRing.criticalDamage + CalculateCritDamageBasedOnDexterity(dexterity - currentRing.dexterity);
                UpdateCriticalChancePreviewText(criticalChanceUnequip);
                UpdateCriticalDamagePreviewText(criticalDamageUnequip);

                UpdateFireResistancePreviewText(fireResistance - currentRing.fireResistance);
                UpdateIceResistancePreviewText(iceResistance - currentRing.iceResistance);
                UpdateShockResistancePreviewText(shockResistance - currentRing.shockResistance);

                UpdateMaxHealthPreviewText(CalculateHealthBasedOnVitalityLevel(vitality - currentRing.vitality));
                UpdateMaxManaPreviewText(CalculateManaBasedOnIntelligenceLevel(intelligence - currentRing.intelligence));
                UpdateMaxStaminaPreviewText(CalculateStaminaBasedOnEnduranceLevel(endurance - currentRing.endurance));
            }
        }

        public void PreviewAmuletStatChanges(AmuletItem newAmulet)
        {
            float newCriticalChance = 0;
            float newCriticalDamage = 0;

            float newFireResistance = 0;
            float newIceResistance = 0;
            float newShockResistance = 0;
  
            if (playerInventory.equippedAmulet != null)
            {
                AmuletItem currentAmulet = playerInventory.equippedAmulet;

                if (currentAmulet != newAmulet)
                {
                    //First, find stats without current armor piece
                    newCriticalChance = (criticalChance - currentAmulet.criticalChance) + newAmulet.criticalChance;
                    newCriticalDamage = (criticalDamage - currentAmulet.criticalDamage) + newAmulet.criticalDamage;

                    newFireResistance = (fireResistance - currentAmulet.fireResistance) + newAmulet.fireResistance;
                    newIceResistance = (iceResistance - currentAmulet.iceResistance) + newAmulet.iceResistance;
                    newShockResistance = (shockResistance - currentAmulet.shockResistance) + newAmulet.shockResistance;
                }
                else
                {
                    //unequipping
                    newCriticalChance = (criticalChance - currentAmulet.criticalChance);
                    newCriticalDamage = (criticalDamage - currentAmulet.criticalDamage);

                    newFireResistance = (fireResistance - currentAmulet.fireResistance);
                    newIceResistance = (iceResistance - currentAmulet.iceResistance);
                    newShockResistance = (shockResistance - currentAmulet.shockResistance);
                }
            }
            else // no current amulet equipped
            {
                newCriticalChance = criticalChance + newAmulet.criticalChance;
                newCriticalDamage = criticalDamage + newAmulet.criticalDamage;

                newFireResistance = fireResistance + newAmulet.fireResistance;
                newIceResistance = iceResistance + newAmulet.iceResistance;
                newShockResistance = shockResistance + newAmulet.shockResistance;
            }

            UpdateCriticalChancePreviewText(newCriticalChance);
            UpdateCriticalDamagePreviewText(newCriticalDamage);

            UpdateFireResistancePreviewText(newFireResistance);
            UpdateIceResistancePreviewText(newIceResistance);
            UpdateShockResistancePreviewText(newShockResistance);
        }

        //PREVIEW VALUE UPDATE FUNCTIONS:

        private void UpdateStatPreviewText(int statIndex, float newValue, float oldValue, bool hasDecimal = false)
        {
            if (newValue > oldValue)
            {
                statBoxPreviewTexts[statIndex].transform.gameObject.SetActive(true);
                statBoxPreviewTexts_shop[statIndex].transform.gameObject.SetActive(true);
                if (!hasDecimal)
                {
                    statBoxPreviewTexts[statIndex].text = "+" + (newValue - oldValue).ToString();
                    statBoxPreviewTexts_shop[statIndex].text = "+" + (newValue - oldValue).ToString();
                }
                else
                {
                    statBoxPreviewTexts[statIndex].text = "+" + (newValue - oldValue).ToString("0.0") + "%";
                    statBoxPreviewTexts_shop[statIndex].text = "+" + (newValue - oldValue).ToString("0.0") + "%";
                }

                statBoxPreviewTexts[statIndex].color = darkGreen;
                statBoxPreviewTexts_shop[statIndex].color = darkGreen;
            }
            else if (newValue < oldValue)
            {
                statBoxPreviewTexts[statIndex].transform.gameObject.SetActive(true);
                statBoxPreviewTexts_shop[statIndex].transform.gameObject.SetActive(true);
                if (!hasDecimal)
                {
                    statBoxPreviewTexts[statIndex].text = "-" + Mathf.Abs(newValue - oldValue).ToString();
                    statBoxPreviewTexts_shop[statIndex].text = "-" + Mathf.Abs(newValue - oldValue).ToString();
                }
                else
                {
                    statBoxPreviewTexts[statIndex].text = "-" + Mathf.Abs(newValue - oldValue).ToString("0.0") + "%";
                    statBoxPreviewTexts_shop[statIndex].text = "-" + Mathf.Abs(newValue - oldValue).ToString("0.0") + "%";
                }

                statBoxPreviewTexts[statIndex].color = Color.red;
                statBoxPreviewTexts_shop[statIndex].color = Color.red;
            }
            else
            {
                statBoxPreviewTexts[statIndex].transform.gameObject.SetActive(false);
                statBoxPreviewTexts_shop[statIndex].transform.gameObject.SetActive(false);
            }
            if (statIndex == 20)
            {
                statBoxPreviewTexts[statIndex].text += " Max Health";
                statBoxPreviewTexts_shop[statIndex].text += " Max Health";
            }
            else if (statIndex == 21)
            {
                statBoxPreviewTexts[statIndex].text += " Max Mana";
                statBoxPreviewTexts_shop[statIndex].text += " Max Mana";
            }
            else if (statIndex == 22)
            {
                statBoxPreviewTexts[statIndex].text += " Max Stamina";
                statBoxPreviewTexts_shop[statIndex].text += " Max Stamina";
            }
        }

        public void UpdatePhysicalDamagePreviewText(float newPhysicalDamage, float oldPhysicalDamage)
        {
            UpdateStatPreviewText(0, newPhysicalDamage, oldPhysicalDamage);
        }

        public void UpdateMagicDamagePreviewText(float newMagicDamage, float oldMagicDamage)
        {
            UpdateStatPreviewText(1, newMagicDamage, oldMagicDamage);
        }

        public void UpdateBlockRatingPreviewText(float newBlockRating, float oldBlockRating)
        {
            UpdateStatPreviewText(2, newBlockRating, oldBlockRating);
        }

        public void UpdatePhysicalArmorRatingPreviewText(float newPhysicalArmorRating, float oldPhysicalArmorRating)
        {
            UpdateStatPreviewText(3, newPhysicalArmorRating, oldPhysicalArmorRating);
        }

        public void UpdateMagicalArmorRatingPreviewText(float newMagicalArmorRating, float oldMagicalArmorRating)
        {
            UpdateStatPreviewText(4, newMagicalArmorRating, oldMagicalArmorRating);
        }

        public void UpdateStrengthPreviewText(float newStrength)
        {
            UpdateStatPreviewText(5, newStrength, strength);
        }

        public void UpdateEndurancePreviewText(float newEndurance)
        {
            UpdateStatPreviewText(6, newEndurance, endurance);
        }

        public void UpdateVitalityPreviewText(float newVitality)
        {
            UpdateStatPreviewText(7, newVitality, vitality);
        }

        public void UpdatePrecisionPreviewText(float newPrecision)
        {
            UpdateStatPreviewText(8, newPrecision, precision);
        }

        public void UpdateDexterityPreviewText(float newDexterity)
        {
            UpdateStatPreviewText(9, newDexterity, dexterity);
        }

        public void UpdateExpertisePreviewText(float newExpertise)
        {
            UpdateStatPreviewText(10, newExpertise, expertise);
        }

        public void UpdateIntelligencePreviewText(float newIntelligence)
        {
            UpdateStatPreviewText(11, newIntelligence, intelligence);
        }

        public void UpdateSpiritPreviewText(float newSpirit)
        {
            UpdateStatPreviewText(12, newSpirit, spirit);
        }

        public void UpdateWillpowerPreviewText(float newWillpower)
        {
            UpdateStatPreviewText(13, newWillpower, willpower);
        }

        public void UpdateLuckPreviewText(float newLuck)
        {
            UpdateStatPreviewText(14, newLuck, luck);
        }

        public void UpdateCriticalChancePreviewText(float newCriticalChance)
        {
            UpdateStatPreviewText(15, newCriticalChance, criticalChance, true);
        }

        public void UpdateCriticalDamagePreviewText(float newCriticalDamage)
        {
            UpdateStatPreviewText(16, newCriticalDamage, criticalDamage, true);
        }

        public void UpdateFireResistancePreviewText(float newFireResistance)
        {
            UpdateStatPreviewText(17, newFireResistance, fireResistance, true);
        }

        public void UpdateIceResistancePreviewText(float newIceResistance)
        {
            UpdateStatPreviewText(18, newIceResistance, iceResistance, true);
        }

        public void UpdateShockResistancePreviewText(float newShockResistance)
        {
            UpdateStatPreviewText(19, newShockResistance, shockResistance, true);
        }

        public void UpdateMaxHealthPreviewText(float newMaxHealth)
        {
            UpdateStatPreviewText(20, newMaxHealth, maxHealth);
        }

        public void UpdateMaxManaPreviewText(float newMaxMana)
        {
            UpdateStatPreviewText(21, newMaxMana, maxMana);
        }

        public void UpdateMaxStaminaPreviewText(float newMaxStamina)
        {
            UpdateStatPreviewText(22, newMaxStamina, maxStamina);
        }

        //LEVEL UP HELPER FUNCTIONS
        public void GetBaseStats(int[] baseStatsArray) // Update the provided array with base stat values
        {
            baseStatsArray[0] = baseStrength;
            baseStatsArray[1] = baseEndurance;
            baseStatsArray[2] = baseVitality;
            baseStatsArray[3] = basePrecision;
            baseStatsArray[4] = baseDexterity;
            baseStatsArray[5] = baseExpertise;
            baseStatsArray[6] = baseIntelligence;
            baseStatsArray[7] = baseSpirit;
            baseStatsArray[8] = baseWillpower;
            baseStatsArray[9] = baseLuck;
        }

        public void GetSecondaryEffectiveStats(float[] secondaryStatsArray)
        {
            secondaryStatsArray[0] = criticalChance;
            secondaryStatsArray[1] = criticalDamage;
            //baseStatsArray[2] = baseVitality;
            //baseStatsArray[3] = basePrecision;
            //baseStatsArray[4] = baseDexterity;
            secondaryStatsArray[2] = maxHealth;
            secondaryStatsArray[3] = maxMana;
            secondaryStatsArray[4] = maxStamina;
        }

        public void UpdateBaseStats(int[] statAllocations, int remainingPts)
        {
            baseStrength += statAllocations[0];  //end 1 vit2 int 6
            baseEndurance += statAllocations[1];
            baseVitality += statAllocations[2];
            basePrecision += statAllocations[3];
            baseDexterity += statAllocations[4];
            baseExpertise += statAllocations[5];
            baseIntelligence += statAllocations[6];
            baseSpirit += statAllocations[7];
            baseWillpower += statAllocations[8];
            baseLuck += statAllocations[9];

            CalculateEffectiveStats();

            remainingPoints = remainingPts;
        }

        //XP & LEVEL UP FUNCTIONALITY
        public int CalculateNextLevelXP(int nextLevel, int baseXP, float growthRate)
        {
            // Calculate the XP required for the next level
            float result = baseXP * Mathf.Pow(growthRate, nextLevel - 1);
            return Mathf.RoundToInt(result);
        }

        public void AwardPlayerXP(int XP)
        {
            int experience = Mathf.CeilToInt(XP * skills[32].CurrentEffect());
            currentExp += experience;

            Debug.Log("Experience Awarded: " + experience);
            Debug.Log("Current Experience: " + currentExp);

            // Keep leveling up until the player doesn't have enough XP for the next level
            while (currentExp >= expNeeded && playerLevel < maxPlayerLevel)
            {
                Debug.Log($"Before Level Up: Level {playerLevel}, Current Exp: {currentExp}, Exp Needed: {expNeeded}");

                playerLevel++;
                StartCoroutine(LevelUpText(4.0f));
                SelfEffectsManager.instance.LevelUpEffect();

                currentExp -= expNeeded;
                Debug.Log("Current Experience after Level Up: " + currentExp);

                expNeeded = CalculateNextLevelXP(playerLevel + 1, baseXP, expGrowthRate);
                Debug.Log("New Exp Needed for Level " + playerLevel + ": " + expNeeded);

                remainingPoints += attrPtsPerLevel; // Increase attribute points 
                remainingSkillPoints += skillPtsPerLevel; // Increase skill points

                // Update XP bar and notify the player
                xpBar.SetStat(currentExp);
                xpBar.SetMaxStat(expNeeded, false);
                TextNotificationsManager.instance.NewTextNotifaction("Level Up! Lv. " + playerLevel, false);
                HelpMenu.instance.DisplayInGame("Leveling Up");
            }

            // Update XP bar and notify the player with the remaining XP
            xpBar.SetStat(currentExp);
            xpBar.SetMaxStat(expNeeded, false);
            TextNotificationsManager.instance.NewTextNotifaction("+" + experience.ToString() + " XP", false);
            Debug.Log("Final Experience: " + currentExp);
        }

        private IEnumerator LevelUpText(float textLifetime)
        {
            levelUpText.SetActive(true);
            yield return new WaitForSeconds(textLifetime);
            levelUpText.SetActive(false);
            if (remainingPoints > 0) PlayerMenuManager.instance.levelUpIconInGame.SetActive(true);
        }

        public void ScaleStatusBars(bool inMenu=false)
        {
            float healthScale;
            float manaScale;
            float staminaScale;

            float maxHealthRequiredForMaxScale = 300f;
            float maxManaRequiredForMaxScale = 300f;
            float maxStaminaRequiredForMaxScale = 300f;
            float maxScale = 1.5f;
            float minScale = 0.5f;
            float statValueForMinScale = 50f;

            if (!inMenu)
            {
                // Calculate the slope dynamically to be used for health bar scale
                float healthSlope = (maxScale - minScale) / (maxHealthRequiredForMaxScale - statValueForMinScale);
                healthScale = healthSlope * (maxHealth - statValueForMinScale) + minScale;
                if (healthScale > maxScale)
                {
                    healthScale = maxScale;
                }

                // Calculate the slope dynamically to be used for health bar scale
                float manaSlope = (maxScale - minScale) / (maxManaRequiredForMaxScale - statValueForMinScale);
                manaScale = manaSlope * (maxMana - statValueForMinScale) + minScale;
                if (manaScale > maxScale)
                {
                    manaScale = maxScale;
                }

                // Calculate the slope dynamically to be used for stamina bar scale
                float staminaSlope = (maxScale - minScale) / (maxStaminaRequiredForMaxScale - statValueForMinScale);
                staminaScale = staminaSlope * (maxStamina - statValueForMinScale) + minScale;
                if (staminaScale > maxScale)
                {
                    staminaScale = maxScale;
                }

            }
            else //In menu
            {
                healthScale = 1f;
                manaScale = 1f;
                staminaScale = 1f;
            }

            healthBar.ScaleBar(healthScale);
            manaBar.ScaleBar(manaScale);
            staminaBar.ScaleBar(staminaScale);
        }

        public void AddEnemyDefeat(string name)
        {
            for (int i = 0; i < enemyRecords.Count; i++)
            {
                if (enemyRecords[i].enemyName == name)
                {
                    enemyRecords[i].defeatCount++;
                    return;
                }
            }

            // If not found, add a new entry
            enemyRecords.Add(new EnemyRecord(name, 1));
        }

        public int CollectEnemyDefeat(string enemyName)
        {
            for (int i = 0; i < enemyRecords.Count; i++)
            {
                if (enemyRecords[i].enemyName == enemyName)
                {
                    return enemyRecords[i].defeatCount;
                }
            }

            // If enemy not found, return 0
            return 0;
        }




    }
}
