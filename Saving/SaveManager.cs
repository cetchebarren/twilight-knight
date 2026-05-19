using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.IO;

namespace etchebarren
{
    public class SaveManager : MonoBehaviour
    {
        public Coroutine saving;
        public bool autosaveEnabled = true;
        public float maxDistanceCombat = 50f;

        public SaveMenu saveMenu;

        [Header("Script references for customization methods")]
        public SelectHairColor selectHairColor;
        public SelectEyeColor selectEyeColor;
        public SelectSkinTone selectSkinTone;

        [Header("Script references for Player Stats/Skills")]
        public PlayerStats playerStats;
        public SkillTreeManager skillTreeManager;

        [Header("Script references for position")]
        public PlayerLocomotion playerLocomotion;

        [Header("Script references for inventory")]
        public PlayerInventory playerInventory;
        public AnimatorHandler animHandlerM;
        public AnimatorHandler animHandlerF;

        [Header("Script references for Quests")]
        public QuestManager questManager;

        [Header("References for world state and items")]
        public WorldStateManager worldStateManager;

        [Header("Horse references")]
        public Transform horseParent;
        public GameObject horseMount;
        public GameObject horseAI;
        public MountStats mountStats;
        public MountManager mountManager;
        public Image mountPortrait;
        public Sprite defaultPortrait;

        [Header("Stables Shop References")]
        public StablesShop stablesShop;
        public GameObject[] stablesSubMenus;

        [Header("Shops")]
        public ShopMenuManager shopMenuManager;

        [Header("Camera References")]
        public CameraHandler cameraHandler;

        [Header("Script references for achievements / game stats")]
        public AchievementManager achievementManager;

        [Header("References to determine look & position of character")]
        public GameObject[] gameObjectsToSaveActiveStatus;
        public Transform playerTransform;

        [Header("References for save file snapshot")]
        public RawImage snapshotLocation;

        [Header("References for other gameobjects related to saving")]
        public SavingNotification savingNotification;

        [Header("Help menu save target")]
        public HelpMenu helpMenu;

        #region Saving

        public void Autosave()
        {
            if (!autosaveEnabled) return;
            if (InputHandler.instance.inCutscene) return;
            SaveData("saveData/save0.es3", true); 
        }

        public void NewSave()
        {
            // Get all save files
            string[] fileList = ES3.GetFiles("saveData/");

            // Filter out files that do not end in .es3 and convert to array
            fileList = fileList.Where(file => file.EndsWith(".es3")).ToArray();

            // Initialize save index to start from 1
            int saveIndex = 1;
            bool saveSlotFound = false;

            // Loop to find an empty save slot
            while (!saveSlotFound)
            {
                if(saveIndex > 1000)
                {
                    TextNotificationsManager.instance.NewTextNotifaction("Error: Too many save files. Did not save.", false); //true prevents duplicate messages for 3 seconds
                    return;
                }

                // Construct the save path for the current save index
                string savePath = $"saveData/save{saveIndex}.es3";

                // Check if a file exists at the constructed save path
                if (!ES3.FileExists(savePath))
                {
                    // If the file does not exist, call the Save method with the new save path
                    SaveData(savePath);
                    // Set the flag to true to exit the loop
                    saveSlotFound = true;
                }
                else
                {
                    // If the file exists, increment the save index and continue the loop
                    saveIndex++;
                }
            }

            if(SaveMenu.instance != null)
            {
                SaveMenu.instance.saveTab?.onClick.Invoke();
                SaveMenu.instance.UpdateSaveSlotList(false);
                SaveMenu.instance.SelectFirstSaveSlot();
            }

        }

        public void SaveData(string filepath, bool autosave=false)
        {
            if(saving == null && !playerStats.dead)
            {
                if (WorldStateManager.instance.eclipseActive)
                {
                    TextNotificationsManager.instance.NewTextNotifaction("Cannot save game during The Harvest.");
                    return;
                }
                else if (CheckForCombat())
                {
                    if (!autosave)
                        TextNotificationsManager.instance.NewTextNotifaction("Cannot save game during combat.");
                    return;
                }
                saving = StartCoroutine(SavaDataCoroutine(filepath, autosave));
            }
            else
            {
                if (autosave)
                {
                    Debug.Log("Autosave initiated but skipped, saving was already in progress.");
                }
                else
                {
                    TextNotificationsManager.instance.NewTextNotifaction("Saving already in progress. Please wait.", false); //true prevents duplicate messages for 3 seconds
                    Debug.Log("Saving already in progress. Please wait.");
                }

            }

        }

        private IEnumerator SavaDataCoroutine(string filepath, bool autosave=false)
        {
            savingNotification.autosaving = autosave;
            savingNotification.gameObject.SetActive(true);
            yield return null;

            Debug.Log("Saving to \"" + filepath + "\"...");

            #region Scene Information
            ES3.Save<string>("currentScene", SceneManager.GetActiveScene().name, filepath);
            yield return null;
            #endregion

            #region Object Information
            // Active / Inactive GameObjects : Iterate through all GameObjects to determine active/inactive status and save to file
            bool[] activeStatuses = new bool[gameObjectsToSaveActiveStatus.Length];
            yield return null;
            for (int i = 0; i < gameObjectsToSaveActiveStatus.Length; i++)
            {
                activeStatuses[i] = gameObjectsToSaveActiveStatus[i].activeSelf;
                yield return null;
            }
            ES3.Save<bool[]>("activeGameObjects", activeStatuses, filepath);
            yield return null;
            ES3.Save<Vector3>("playerPosition", playerTransform.position, filepath);
            yield return null;
            ES3.Save<Quaternion>("playerRotation", playerTransform.rotation, filepath);
            yield return null;
            #endregion

            #region Colors
            ES3.Save<string>("skinTone", selectSkinTone.currentSkinTone, filepath);
            yield return null;
            ES3.Save<string>("eyeColor", selectEyeColor.currentEyeColor, filepath);
            yield return null;
            ES3.Save<Color>("hairColor", selectHairColor.currentHairColor, filepath);
            yield return null;
            ES3.Save<Color>("eyebrowColor", selectHairColor.currentEyebrowColor, filepath);
            yield return null;
            ES3.Save<Color>("facialHairColor", selectHairColor.currentFacialHairColor, filepath);
            yield return null;
            #endregion

            #region Player Stats
            // Level and Experience
            ES3.Save<int>("playerLevel", playerStats.playerLevel, filepath);
            yield return null;
            ES3.Save<string>("playerName", playerStats.playerName, filepath);
            yield return null;
            ES3.Save<string>("playerGender", playerStats.gender, filepath);
            yield return null;
            ES3.Save<int>("remainingPoints", playerStats.remainingPoints, filepath);
            yield return null;
            ES3.Save<int>("remainingSkillPoints", playerStats.remainingSkillPoints, filepath);
            yield return null;
            ES3.Save<int>("expNeeded", playerStats.expNeeded, filepath);
            yield return null;
            ES3.Save<int>("currentExp", playerStats.currentExp, filepath);
            yield return null;
            // Base Stats
            ES3.Save<int>("baseStrength", playerStats.baseStrength, filepath);
            yield return null;
            ES3.Save<int>("baseEndurance", playerStats.baseEndurance, filepath);
            yield return null;
            ES3.Save<int>("baseVitality", playerStats.baseVitality, filepath);
            yield return null;
            ES3.Save<int>("basePrecision", playerStats.basePrecision, filepath);
            yield return null;
            ES3.Save<int>("baseDexterity", playerStats.baseDexterity, filepath);
            yield return null;
            ES3.Save<int>("baseExpertise", playerStats.baseExpertise, filepath);
            yield return null;
            ES3.Save<int>("baseIntelligence", playerStats.baseIntelligence, filepath);
            yield return null;
            ES3.Save<int>("baseSpirit", playerStats.baseSpirit, filepath);
            yield return null;
            ES3.Save<int>("baseWillpower", playerStats.baseWillpower, filepath);
            yield return null;
            ES3.Save<int>("baseLuck", playerStats.baseLuck, filepath);
            yield return null;
            ES3.Save<float>("baseCriticalChance", playerStats.baseCriticalChance, filepath);
            yield return null;
            ES3.Save<float>("baseCriticalDamage", playerStats.baseCriticalDamage, filepath);
            yield return null;
            ES3.Save<float>("baseFireResistance", playerStats.baseFireResistance, filepath);
            yield return null;
            ES3.Save<float>("baseIceResistance", playerStats.baseIceResistance, filepath);
            yield return null;
            ES3.Save<float>("baseShockResistance", playerStats.baseShockResistance, filepath);
            yield return null;
            // Current Status
            ES3.Save<float>("currentHealth", playerStats.currentHealth, filepath);
            yield return null;
            ES3.Save<float>("currentStamina", playerStats.currentStamina, filepath);
            yield return null;
            ES3.Save<float>("currentMana", playerStats.currentMana, filepath);
            yield return null;
            // Skills
            ES3.Save<Skill[]>("skills", playerStats.skills, filepath);
            yield return null;
            // Feats and Stats
            ES3.Save<int>("attacksBlocked", playerStats.attacksBlocked, filepath);
            yield return null;
            #endregion

            #region Player Locomotion
            ES3.Save<string>("playerLocomotionGender", playerLocomotion.gender, filepath);
            yield return null;
            ES3.Save<bool>("inCombatStance", playerLocomotion.inCombat, filepath);
            yield return null;
            #endregion

            #region Camera
            ES3.Save<float>("cameraLeftAndRight", cameraHandler.leftAndRightLookAngle, filepath);
            yield return null;
            ES3.Save<float>("cameraUpAndDown", cameraHandler.upAndDownLookAngle, filepath);
            yield return null;
            #endregion

            #region Horse
            // Mount Unlocked?
            ES3.Save<bool>("mountUnlocked", mountManager.mountUnlocked, filepath);
            // Convert RenderTexture to Texture2D for Portrait image
            Texture2D portrait = ConvertImageToTexture2D(mountPortrait);
            yield return null;
            // Save the screenshot as a JPG file.
            string portraitFilepath = Path.ChangeExtension(filepath, ".png");
            yield return null;
            ES3.SaveImage(portrait, portraitFilepath);
            yield return null;
            // Clean up the created Texture2D
            Destroy(portrait);
            yield return null;
            
            ES3.Save<string>("horseName", mountStats.horseName, filepath);
            yield return null;
            ES3.Save<int>("horseLevel", mountStats.level, filepath);
            yield return null;
            ES3.Save<float>("horseExp", mountStats.exp, filepath);
            yield return null;
            ES3.Save<bool>("horseArmorOwned", mountStats.horseArmorOwned, filepath);
            yield return null;
            ES3.Save<bool>("horseHornOwned", mountStats.horseHornOwned, filepath);
            yield return null;
            ES3.Save<bool>("horseHealthy", mountManager.mountIsHealthy, filepath);
            yield return null;
            ES3.Save<float>("horseRecoveryTimer", mountManager.elapsedTime, filepath);
            yield return null;

            if (horseAI.activeSelf) // Horse Companion was Active
            {
                ES3.Save<bool>("horseActiveAI", true, filepath);
                yield return null;
                ES3.Save<bool>("horseActiveMount", false, filepath);
                yield return null;
                ES3.Save<Vector3>("horsePosition", horseAI.transform.position, filepath);
                yield return null;
                ES3.Save<Quaternion>("horseRotation", horseAI.transform.rotation, filepath);
                yield return null;
            }
            else if (horseMount.activeSelf) // Horse was mounted
            {
                ES3.Save<bool>("horseActiveAI", false, filepath);
                yield return null;
                ES3.Save<bool>("horseActiveMount", true, filepath);
                yield return null;
                ES3.Save<Vector3>("horsePosition", horseMount.transform.position, filepath);
                yield return null;
                ES3.Save<Quaternion>("horseRotation", horseMount.transform.rotation, filepath);
                yield return null;
            }
            else
            {
                ES3.Save<bool>("horseActiveAI", false, filepath);
                yield return null;
                ES3.Save<bool>("horseActiveMount", false, filepath);
                yield return null;
                ES3.Save<Vector3>("horsePosition", Vector3.zero, filepath);
                yield return null;
                ES3.Save<Quaternion>("horseRotation", Quaternion.identity, filepath);
                yield return null;
            }

            #endregion

            #region Inventory
            // Change Save Settings to save objects by value
            var scriptableObjectSettings = new ES3Settings();
            yield return null;
            scriptableObjectSettings.memberReferenceMode = ES3.ReferenceMode.ByValue;
            yield return null;

            // Equipped Items
            ES3.Save<WeaponItem>("equippedWeapon", playerInventory.equippedWeapon, filepath, scriptableObjectSettings);
            yield return null;
            ES3.Save<ShieldItem>("equippedShield", playerInventory.equippedShield, filepath, scriptableObjectSettings);
            yield return null;
            ES3.Save<TorsoArmorItem>("equippedTorsoArmor", playerInventory.equippedTorsoArmor, filepath, scriptableObjectSettings);
            yield return null;
            ES3.Save<HandsArmorItem>("equippedHandsArmor", playerInventory.equippedHandsArmor, filepath, scriptableObjectSettings);
            yield return null;
            ES3.Save<LegsArmorItem>("equippedLegsArmor", playerInventory.equippedLegsArmor, filepath, scriptableObjectSettings);
            yield return null;
            ES3.Save<RingItem>("equippedRing1", playerInventory.equippedRing1, filepath, scriptableObjectSettings);
            yield return null;
            ES3.Save<RingItem>("equippedRing2", playerInventory.equippedRing2, filepath, scriptableObjectSettings);
            yield return null;
            ES3.Save<AmuletItem>("equippedAmulet", playerInventory.equippedAmulet, filepath, scriptableObjectSettings);
            yield return null;

            // Equipped Spells
            ES3.Save<Spell[]>("equippedSpells", playerInventory.equippedSpells, filepath, scriptableObjectSettings);
            yield return null;

            // Equipped Consumables
            ES3.Save<List<Consumable>>("equippedConsumables", playerInventory.equippedConsumables, filepath, scriptableObjectSettings);
            yield return null;

            // Full Inventory
            ES3.Save<List<WeaponItem>>("weaponsInventory", playerInventory.weaponsInventory, filepath, scriptableObjectSettings);
            yield return null;
            ES3.Save<List<ShieldItem>>("shieldsInventory", playerInventory.shieldsInventory, filepath, scriptableObjectSettings);
            yield return null;
            ES3.Save<List<TorsoArmorItem>>("torsoArmorInventory", playerInventory.torsoArmorInventory, filepath, scriptableObjectSettings);
            yield return null;
            ES3.Save<List<HandsArmorItem>>("handsArmorInventory", playerInventory.handsArmorInventory, filepath, scriptableObjectSettings);
            yield return null;
            ES3.Save<List<LegsArmorItem>>("legsArmorInventory", playerInventory.legsArmorInventory, filepath, scriptableObjectSettings);
            yield return null;
            ES3.Save<List<RingItem>>("ringsInventory", playerInventory.ringsInventory, filepath, scriptableObjectSettings);
            yield return null;
            ES3.Save<List<AmuletItem>>("amuletsInventory", playerInventory.amuletsInventory, filepath, scriptableObjectSettings);
            yield return null;
            ES3.Save<List<Spell>>("spellsInventory", playerInventory.spellsInventory, filepath, scriptableObjectSettings);
            yield return null;
            ES3.Save<List<Consumable>>("consumablesInventory", playerInventory.consumablesInventory, filepath, scriptableObjectSettings);
            yield return null;
            ES3.Save<List<KeyItem>>("keyItemsInventory", playerInventory.keyItemsInventory, filepath, scriptableObjectSettings);
            yield return null;
            #endregion

            #region Quests
            questManager.SaveQuestData();
            yield return null;
            ES3.Save<List<Quest>>("questData", questManager.questData, filepath, scriptableObjectSettings);
            yield return null;
            #endregion

            #region Shops

            // Stables Shop
            float[] stablesSlidersValues = new float[] { 
                stablesShop.coatValue, 
                stablesShop.maneValue, 
                stablesShop.saddleValue, 
                stablesShop.armorValue, 
                stablesShop.hornValue 
            };
            ES3.Save<float[]>("stablesSlidersValues", stablesSlidersValues, filepath);
            yield return null;
            bool[] stablesShopOptions = new bool[]
            {
                stablesSubMenus[1].activeSelf, // Coat
                stablesSubMenus[3].activeSelf, // Mane
                stablesSubMenus[5].activeSelf, // Saddle
                stablesSubMenus[7].activeSelf, // Armor
                stablesSubMenus[9].activeSelf, // Horn
            };
            ES3.Save<bool[]>("stablesShopOptions", stablesShopOptions, filepath);
            yield return null;

            // Shops Reset Timer
            ES3.Save<float>("shopsResetTimer", shopMenuManager.timeRemaining, filepath);

            // Shops Inventory: Stalwart Camp Shop
            ES3.Save<List<WeaponItem>>("shopInventory0_weapons", shopMenuManager.weaponsInventory_0, filepath, scriptableObjectSettings);
            yield return null;
            ES3.Save<List<ShieldItem>>("shopInventory0_shields", shopMenuManager.shieldsInventory_0, filepath, scriptableObjectSettings);
            yield return null;
            ES3.Save<List<TorsoArmorItem>>("shopInventory0_torsoArmor", shopMenuManager.torsoArmorInventory_0, filepath, scriptableObjectSettings);
            yield return null;
            ES3.Save<List<HandsArmorItem>>("shopInventory0_handsArmor", shopMenuManager.handsArmorInventory_0, filepath, scriptableObjectSettings);
            yield return null;
            ES3.Save<List<LegsArmorItem>>("shopInventory0_legsArmor", shopMenuManager.legsArmorInventory_0, filepath, scriptableObjectSettings);
            yield return null;
            ES3.Save<List<RingItem>>("shopInventory0_rings", shopMenuManager.ringsInventory_0, filepath, scriptableObjectSettings);
            yield return null;
            ES3.Save<List<AmuletItem>>("shopInventory0_amulets", shopMenuManager.amuletsInventory_0, filepath, scriptableObjectSettings);
            yield return null;
            ES3.Save<List<Spell>>("shopInventory0_spells", shopMenuManager.spellsInventory_0, filepath, scriptableObjectSettings);
            yield return null;
            ES3.Save<List<Consumable>>("shopInventory0_consumables", shopMenuManager.consumablesInventory_0, filepath, scriptableObjectSettings);
            yield return null;
            ES3.Save<List<KeyItem>>("shopInventory0_keyItems", shopMenuManager.keyItemsInventory_0, filepath, scriptableObjectSettings);
            yield return null;

            #endregion

            #region World State Information

            // Dialogue Agents Already Met
            ES3.Save<List<string>>("dialogueAgentsMet", worldStateManager.dialogueAgentsAlreadyMet, filepath, scriptableObjectSettings);
            yield return null;

            // Unlocked Teleport Waypoints(Local Travel)
            ES3.Save<List<int>>("unlockedWaypoints", worldStateManager.unlockedTeleportWaypointIDs, filepath, scriptableObjectSettings);
            yield return null;

            // World Locations Unlocked (World Travel)
            ES3.Save<List<string>>("worldLocationsUnlocked", worldStateManager.worldLocationsUnlocked, filepath, scriptableObjectSettings);
            yield return null;

            // Chests (opened/unopened)
            ES3.Save<Dictionary<int, bool>>("chestDictionary", worldStateManager.chestDictionary, filepath, scriptableObjectSettings);
            yield return null;

            // Unlocked Doors/Gates
            ES3.Save<List<int>>("unlockedDoorIDs", worldStateManager.unlockedDoorIDs, filepath, scriptableObjectSettings);
            yield return null;

            // Picked Up Quest Items
            ES3.Save<Dictionary<int, bool>>("questItemDictionary", worldStateManager.questItemDictionary, filepath, scriptableObjectSettings);
            yield return null;

            // Picked Up World Items
            ES3.Save<List<int>>("obtainedWorldItemIDs", worldStateManager.obtainedWorldItemIDs, filepath, scriptableObjectSettings);
            yield return null;

            // Readable/Note IDs that have been read
            ES3.Save<List<int>>("readReadableIDs", worldStateManager.readReadableIDs, filepath, scriptableObjectSettings);
            yield return null;

            // Readable/Note IDs that have been grabbed/obtained
            ES3.Save<List<int>>("grabbedReadableIDs", worldStateManager.grabbedReadableIDs, filepath, scriptableObjectSettings);
            yield return null;

            // Dead enemies to prevent respawning on load game
            ES3.Save<List<int>>("deadEnemyIDs", worldStateManager.deadEnemyIDs, filepath, scriptableObjectSettings);
            yield return null;

            // One-time event names that have arleady been triggered
            ES3.Save<List<string>>("worldEventsAlreadyTriggered", worldStateManager.worldEventsAlreadyTriggered, filepath, scriptableObjectSettings);
            yield return null;

            // One-time tips header names that have arleady been triggered/unlocked
            ES3.Save<List<string>>("unlockedTipsHeaders", helpMenu.unlockedTipHeaders, filepath, scriptableObjectSettings);
            yield return null;

            #endregion

            #region System Information
            // Get the current date and time
            System.DateTime now = System.DateTime.Now;
            yield return null;
            // Convert to strings
            string dateString = now.ToString("yyyy-MM-dd"); // Format: Year-Month-Day
            yield return null;
            ES3.Save<string>("date", dateString, filepath);
            yield return null;
            string timeString = now.ToString("HH:mm:ss");   // Format: Hour:Minute:Second (24-hour format)
            yield return null;
            ES3.Save<string>("time", timeString, filepath);
            yield return null;

            // Screenshot
            if (autosave)
            {
                PlayerMenuManager.instance.CaptureCameraSnapshot();
                Debug.Log("Capturing screenshot for Autosave..");
            }
            if (snapshotLocation.texture is RenderTexture renderTexture)
            {
                // Convert RenderTexture to Texture2D
                Texture2D texture2D = ConvertRenderTextureToTexture2D(renderTexture);
                yield return null;
                // Save the screenshot as a JPG file.
                string screenshotFilepath = Path.ChangeExtension(filepath, ".jpg");
                yield return null;
                ES3.SaveImage(texture2D, screenshotFilepath);
                yield return null;
                // Clean up the created Texture2D
                Destroy(texture2D);
                yield return null;
            }
            else
            {
                Debug.LogError("The texture is not a RenderTexture.");
            }
            #endregion

            #region Achivements and Records
            int totalPlaytimeInSeconds = achievementManager.GetPlaytimeInSeconds();
            yield return null;
            ES3.Save<int>("playtimeInSeconds", totalPlaytimeInSeconds, filepath);
            yield return null;
            achievementManager.ResetStartTime();
            yield return null;
            #endregion

            #region Finish

            saving = null;
            Debug.Log("Save complete");
            savingNotification.Complete();

            saveMenu.UpdateSaveSlotList(saveMenu.loading);

            #endregion
        }

        #endregion

        #region Loading

        public void LoadData(string filepath)
        {
            Debug.Log("Loading game at filepath:" + filepath);

            // Ensure the array is initialized with the correct length
            bool[] activeStatuses = new bool[gameObjectsToSaveActiveStatus.Length];

            // Check if the key exists before loading
            if (ES3.KeyExists("activeGameObjects", filepath))
            {
                #region Player Object Information
                // Load the active statuses
                activeStatuses = ES3.Load<bool[]>("activeGameObjects", filepath);
                // Debug.Log("Loaded activeStatuses: " + string.Join(", ", activeStatuses));

                GameObject sceneSpawnManager = GameObject.FindWithTag("SceneSpawnManager");
                if (sceneSpawnManager != null) sceneSpawnManager.GetComponent<SceneSpawnManager>().spawnedFromSaveSlot = true;

                playerTransform.position = ES3.Load<Vector3>("playerPosition", filepath);
                playerTransform.rotation = ES3.Load<Quaternion>("playerRotation", filepath);

                #endregion

                #region Player Colors
                // Load Colors
                selectSkinTone.SetSkinTone(ES3.Load<string>("skinTone", filepath));
                selectEyeColor.SetEyeColor(ES3.Load<string>("eyeColor", filepath));
                selectHairColor.SetHairColor(ES3.Load<Color>("hairColor", filepath));
                selectHairColor.SetEyebrowColor(ES3.Load<Color>("eyebrowColor", filepath));
                selectHairColor.SetFacialHairColor(ES3.Load<Color>("facialHairColor", filepath));
                #endregion

                #region Player Stats
                // Level and Experience
                playerStats.playerLevel = ES3.Load<int>("playerLevel", filepath);
                playerStats.playerName = ES3.Load<string>("playerName", filepath);
                playerStats.gender = ES3.Load<string>("playerGender", filepath);
                playerStats.remainingPoints = ES3.Load<int>("remainingPoints", filepath);
                playerStats.remainingSkillPoints = ES3.Load<int>("remainingSkillPoints", filepath);
                playerStats.expNeeded = ES3.Load<int>("expNeeded", filepath);
                playerStats.currentExp = ES3.Load<int>("currentExp", filepath);
                // Base Stats
                playerStats.baseStrength = ES3.Load<int>("baseStrength", filepath);
                playerStats.baseEndurance = ES3.Load<int>("baseEndurance", filepath);
                playerStats.baseVitality = ES3.Load<int>("baseVitality", filepath);
                playerStats.basePrecision = ES3.Load<int>("basePrecision", filepath);
                playerStats.baseDexterity = ES3.Load<int>("baseDexterity", filepath);
                playerStats.baseExpertise = ES3.Load<int>("baseExpertise", filepath);
                playerStats.baseIntelligence = ES3.Load<int>("baseIntelligence", filepath);
                playerStats.baseSpirit = ES3.Load<int>("baseSpirit", filepath);
                playerStats.baseWillpower = ES3.Load<int>("baseWillpower", filepath);
                playerStats.baseLuck = ES3.Load<int>("baseLuck", filepath);
                playerStats.baseCriticalChance = ES3.Load<float>("baseCriticalChance", filepath);
                playerStats.baseCriticalDamage = ES3.Load<float>("baseCriticalDamage", filepath);
                playerStats.baseFireResistance = ES3.Load<float>("baseFireResistance", filepath);
                playerStats.baseIceResistance = ES3.Load<float>("baseIceResistance", filepath);
                playerStats.baseShockResistance = ES3.Load<float>("baseShockResistance", filepath);
                // Current Status
                playerStats.currentHealth = ES3.Load<float>("currentHealth", filepath);
                playerStats.currentStamina = ES3.Load<float>("currentStamina", filepath);
                playerStats.currentMana = ES3.Load<float>("currentMana", filepath);
                // Skills
                playerStats.InitializeSkills();
                SyncSkillTree(ES3.Load<Skill[]>("skills", filepath));
                // Feats and Stats
                playerStats.attacksBlocked = ES3.Load<int>("attacksBlocked", filepath, 0);
                #endregion

                #region Player Locomotion
                playerLocomotion.SetGender(ES3.Load<string>("playerLocomotionGender", filepath));
                playerLocomotion.inCombat = ES3.Load<bool>("inCombatStance", filepath);

                animHandlerM.loadedGame = true;
                animHandlerF.loadedGame = true;
                #endregion

                #region Camera
                cameraHandler.leftAndRightLookAngle = ES3.Load<float>("cameraLeftAndRight", filepath);
                cameraHandler.upAndDownLookAngle = ES3.Load<float>("cameraUpAndDown", filepath);
                #endregion

                #region Horse
                // Mount Unlocked?
                mountManager.mountUnlocked = ES3.Load<bool>("mountUnlocked", filepath);
                // Get the portrait filepath based on this save
                string portaitFilepath = Path.ChangeExtension(filepath, ".png");
                // If the portrait exists, set the image sprite, otherwise apply default portrait to handle error
                if (ES3.FileExists(portaitFilepath))
                {
                    Texture2D portrait = ES3.LoadImage(portaitFilepath);
                    Sprite portraitSprite = CreateSpriteFromTexture2D(portrait);
                    mountPortrait.sprite = portraitSprite;
                }
                else
                {
                    mountPortrait.sprite = defaultPortrait;
                }
                
                mountStats.horseName = ES3.Load<string>("horseName", filepath);

                mountStats.gameLoaded = true;

                mountStats.level = ES3.Load<int>("horseLevel", filepath, 1);
                mountStats.exp = ES3.Load<float>("horseExp", filepath, 0f);

                mountStats.horseArmorOwned = ES3.Load<bool>("horseArmorOwned", filepath);
                mountStats.horseHornOwned = ES3.Load<bool>("horseHornOwned", filepath);

                bool horseCompanionActive = ES3.Load<bool>("horseActiveAI", filepath);
                bool horseMountActive = ES3.Load<bool>("horseActiveMount", filepath);

                horseParent.position = ES3.Load<Vector3>("horsePosition", filepath);

                if (horseCompanionActive)
                {
                    //horseAI.transform.position = ES3.Load<Vector3>("horsePosition", filepath);
                    horseAI.SetActive(true);
                    horseMount.SetActive(false);
                }
                else if (horseMountActive)
                {
                    //horseMount.transform.position = ES3.Load<Vector3>("horsePosition", filepath);
                    horseAI.SetActive(true);
                    horseMount.SetActive(false);
                }

                mountManager.elapsedTime = ES3.Load<float>("horseRecoveryTimer", filepath);
                bool horseHealthy = ES3.Load<bool>("horseHealthy", filepath);
                if (!horseHealthy)
                {
                    mountManager.DeadOnLoad();
                }
                #endregion

                #region Inventory
                // Equipped Items
                playerInventory.loadedGame = true;
                playerInventory.equippedWeapon = ES3.Load<WeaponItem>("equippedWeapon", filepath);
                playerInventory.equippedShield = ES3.Load<ShieldItem>("equippedShield", filepath);
                playerInventory.equippedTorsoArmor = ES3.Load<TorsoArmorItem>("equippedTorsoArmor", filepath);
                playerInventory.equippedHandsArmor = ES3.Load<HandsArmorItem>("equippedHandsArmor", filepath);
                playerInventory.equippedLegsArmor = ES3.Load<LegsArmorItem>("equippedLegsArmor", filepath);
                playerInventory.equippedRing1 = ES3.Load<RingItem>("equippedRing1", filepath);
                playerInventory.equippedRing2 = ES3.Load<RingItem>("equippedRing2", filepath);
                playerInventory.equippedAmulet = ES3.Load<AmuletItem>("equippedAmulet", filepath);

                // Check for EMPTY items and assign empty versions where necessary
                if (playerInventory.equippedWeapon.itemName == "EMPTY WEAPON")
                {
                    playerInventory.equippedWeapon = playerInventory.emptyWeapon;
                }

                if (playerInventory.equippedShield.itemName == "EMPTY SHIELD")
                {
                    playerInventory.equippedShield = playerInventory.emptyShield;
                }

                if (playerInventory.equippedTorsoArmor.itemName == "EMPTY TORSO ARMOR")
                {
                    playerInventory.equippedTorsoArmor = playerInventory.emptyTorsoArmor;
                }

                if (playerInventory.equippedHandsArmor.itemName == "EMPTY HANDS ARMOR")
                {
                    playerInventory.equippedHandsArmor = playerInventory.emptyHandsArmor;
                }

                if (playerInventory.equippedLegsArmor.itemName == "EMPTY LEGS ARMOR")
                {
                    playerInventory.equippedLegsArmor = playerInventory.emptyLegsArmor;
                }

                // Equipped Spells
                playerInventory.equippedSpells = ES3.Load<Spell[]>("equippedSpells", filepath);

                // Equipped Consumables
                playerInventory.equippedConsumables = ES3.Load<List<Consumable>>("equippedConsumables", filepath);

                // Full Inventory
                playerInventory.weaponsInventory = ES3.Load<List<WeaponItem>>("weaponsInventory", filepath);
                playerInventory.shieldsInventory = ES3.Load<List<ShieldItem>>("shieldsInventory", filepath);
                playerInventory.torsoArmorInventory = ES3.Load<List<TorsoArmorItem>>("torsoArmorInventory", filepath);
                playerInventory.handsArmorInventory = ES3.Load<List<HandsArmorItem>>("handsArmorInventory", filepath);
                playerInventory.legsArmorInventory = ES3.Load<List<LegsArmorItem>>("legsArmorInventory", filepath);
                playerInventory.ringsInventory = ES3.Load<List<RingItem>>("ringsInventory", filepath);
                playerInventory.amuletsInventory = ES3.Load<List<AmuletItem>>("amuletsInventory", filepath);
                playerInventory.spellsInventory = ES3.Load<List<Spell>>("spellsInventory", filepath);
                playerInventory.consumablesInventory = ES3.Load<List<Consumable>>("consumablesInventory", filepath);
                playerInventory.keyItemsInventory = ES3.Load<List<KeyItem>>("keyItemsInventory", filepath);

                // Set Icons For Inventory (Since Icons/Images are Not saved Directly to text Save File)
                foreach(WeaponItem item in playerInventory.weaponsInventory)
                {
                    item.itemIcon = playerInventory.swordIcons[item.weaponModelID];
                }
                foreach (ShieldItem item in playerInventory.shieldsInventory)
                {
                    item.itemIcon = playerInventory.shieldIcons[item.shieldModelID];
                }
                foreach (TorsoArmorItem item in playerInventory.torsoArmorInventory)
                {
                    item.itemIcon = playerInventory.torsoArmorIcons[item.torsoArmorModelID];
                }
                foreach (HandsArmorItem item in playerInventory.handsArmorInventory)
                {
                    item.itemIcon = playerInventory.handsArmorIcons[item.handsArmorModelID];
                }
                foreach (LegsArmorItem item in playerInventory.legsArmorInventory)
                {
                    item.itemIcon = playerInventory.legsArmorIcons[item.legsArmorModelID];
                }
                foreach (RingItem item in playerInventory.ringsInventory)
                {
                    item.itemIcon = playerInventory.ringIcons[item.ringModelID];
                }
                foreach (AmuletItem item in playerInventory.amuletsInventory)
                {
                    item.itemIcon = playerInventory.amuletIcons[item.amuletModelID];
                }
                foreach (Spell spell in playerInventory.spellsInventory)
                {
                    spell.itemIcon = playerInventory.spellIcons[spell.spell_ID];
                }
                foreach (Consumable consumable in playerInventory.consumablesInventory)
                {
                    consumable.itemIcon = playerInventory.consumableIcons[consumable.consumable_ID];
                }
                foreach (KeyItem keyItem in playerInventory.keyItemsInventory)
                {
                    keyItem.itemIcon = playerInventory.keyItemIcons[keyItem.keyItemSpriteID];
                }
                #endregion

                #region Quests
                // Load quest date from save file
                questManager.questData = ES3.Load<List<Quest>>("questData", filepath);
                // Load Quest Data : This means load quest data into our universal quest objects
                questManager.LoadQuestData();
                // Check tracked quest (moved to Quest Manager Start() to comply with dependency on that script)

                #endregion

                #region Shops

                // Stables Shop
                float[] stablesSlidersValues = ES3.Load<float[]>("stablesSlidersValues", filepath);
                stablesShop.coat.value = stablesSlidersValues[0];
                stablesShop.mane.value = stablesSlidersValues[1];
                stablesShop.saddle.value = stablesSlidersValues[2];
                stablesShop.armor.value = stablesSlidersValues[3];
                stablesShop.horn.value = stablesSlidersValues[4];
                stablesShop.UpdateCoatSlider(true);
                stablesShop.UpdateManeSlider(true);
                stablesShop.UpdateSaddleSlider(true);
                stablesShop.UpdateArmorSlider(true);
                stablesShop.UpdateHornSlider(true);
                bool[] stablesShopOptions = ES3.Load<bool[]>("stablesShopOptions", filepath);
                stablesSubMenus[0].SetActive(!stablesShopOptions[0]);
                stablesSubMenus[1].SetActive(stablesShopOptions[0]);
                stablesSubMenus[2].SetActive(!stablesShopOptions[1]);
                stablesSubMenus[3].SetActive(stablesShopOptions[1]);
                stablesSubMenus[4].SetActive(!stablesShopOptions[2]);
                stablesSubMenus[5].SetActive(stablesShopOptions[2]);
                stablesSubMenus[6].SetActive(!stablesShopOptions[3]);
                stablesSubMenus[7].SetActive(stablesShopOptions[3]);
                stablesSubMenus[8].SetActive(!stablesShopOptions[4]);
                stablesSubMenus[9].SetActive(stablesShopOptions[4]);

                // Shops Reset Timer
                shopMenuManager.timeRemaining = ES3.Load<float>("shopsResetTimer", filepath);

                //Shops Inventory: Stalwart Camp Shop
                shopMenuManager.weaponsInventory_0 = ES3.Load<List<WeaponItem>>("shopInventory0_weapons", filepath);
                shopMenuManager.shieldsInventory_0 = ES3.Load<List<ShieldItem>>("shopInventory0_shields", filepath);
                shopMenuManager.torsoArmorInventory_0 = ES3.Load<List<TorsoArmorItem>>("shopInventory0_torsoArmor", filepath);
                shopMenuManager.handsArmorInventory_0 = ES3.Load<List<HandsArmorItem>>("shopInventory0_handsArmor", filepath);
                shopMenuManager.legsArmorInventory_0 = ES3.Load<List<LegsArmorItem>>("shopInventory0_legsArmor", filepath);
                shopMenuManager.ringsInventory_0 = ES3.Load<List<RingItem>>("shopInventory0_rings", filepath);
                shopMenuManager.amuletsInventory_0 = ES3.Load<List<AmuletItem>>("shopInventory0_amulets", filepath);
                shopMenuManager.spellsInventory_0 = ES3.Load<List<Spell>>("shopInventory0_spells", filepath);
                shopMenuManager.consumablesInventory_0 = ES3.Load<List<Consumable>>("shopInventory0_consumables", filepath);
                shopMenuManager.keyItemsInventory_0 = ES3.Load<List<KeyItem>>("shopInventory0_keyItems", filepath);

                #endregion

                #region World State and Items

                // Dialogue Agents Already Met
                worldStateManager.dialogueAgentsAlreadyMet = ES3.Load<List<string>>("dialogueAgentsMet", filepath);

                // Unlocked Teleport Waypoints (Local Travel)
                worldStateManager.unlockedTeleportWaypointIDs = ES3.Load<List<int>>("unlockedWaypoints", filepath);

                // World Locations Unlocked (World Travel)
                worldStateManager.worldLocationsUnlocked = ES3.Load<List<string>>("worldLocationsUnlocked", filepath);

                // Chests (opened/unopened)
                worldStateManager.chestDictionary = ES3.Load<Dictionary<int, bool>>("chestDictionary", filepath);

                // Unlocked Doors/Gates
                worldStateManager.unlockedDoorIDs = ES3.Load<List<int>>("unlockedDoorIDs", filepath);

                // Picked Up Quest Items
                worldStateManager.questItemDictionary = ES3.Load<Dictionary<int, bool>>("questItemDictionary", filepath);

                // Picked Up World Items
                worldStateManager.obtainedWorldItemIDs = ES3.Load<List<int>>("obtainedWorldItemIDs", filepath);

                // Readables/Notes that have been read already (prevent getting attached items twice+)
                worldStateManager.readReadableIDs = ES3.Load<List<int>>("readReadableIDs", filepath);

                // Readables/Notes that have been grabbed/obtained already (prevent spawn)
                worldStateManager.grabbedReadableIDs = ES3.Load<List<int>>("grabbedReadableIDs", filepath);

                // Dead enemies to prevent respawning on load game
                worldStateManager.deadEnemyIDs = ES3.Load<List<int>>("deadEnemyIDs", filepath);

                // One-time event names that have arleady been triggered
                worldStateManager.worldEventsAlreadyTriggered = ES3.Load<List<string>>("worldEventsAlreadyTriggered", filepath, new List<string>());

                // One-time tips header names that have arleady been triggered/unlocked
                helpMenu.unlockedTipHeaders = ES3.Load<List<string>>("unlockedTipsHeaders", filepath, new List<string>());

                #endregion

                #region Effective Stats and Updating UI
                // Moved this code to PlayerStats to work with script load order: PlayerStats.InitializeLoadGame() called in Start() if loadGame==true
                playerStats.loadGame = true;
                #endregion

                #region Achivements and Records
                achievementManager.totalPlaytimeInSeconds = ES3.Load<int>("playtimeInSeconds", filepath);
                #endregion
            }
            else
            {
                Debug.LogWarning("Key 'activeGameObjects' does not exist.");

                return;
            }

            // Ensure the lengths match
            if (activeStatuses.Length != gameObjectsToSaveActiveStatus.Length)
            {
                Debug.LogError("Mismatch in lengths: activeStatuses length is " + activeStatuses.Length +
                               ", but gameObjectsToSaveActiveStatus length is " + gameObjectsToSaveActiveStatus.Length);
                return;
            }

            // Set active statuses
            for (int i = 0; i < gameObjectsToSaveActiveStatus.Length; i++)
            {
                gameObjectsToSaveActiveStatus[i].SetActive(activeStatuses[i]);
            }

            Debug.Log("Save Data Loaded!");
        }

        public void SyncSkillTree(Skill[] savedSkills)
        {
            // Build lookup from the SAVED data
            Dictionary<int, Skill> savedLookup = new Dictionary<int, Skill>();
            //Debug.Log("Saved Skills:");
            foreach (var s in savedSkills)
            {
                savedLookup[s.ID] = s;
                //Debug.Log($"Skill ID {s.ID}: Rank {s.CurrentRank()}");
            }
                

            // Write INTO the player's live skills
            foreach (Skill skill in playerStats.skills)
            {
                if (savedLookup.TryGetValue(skill.ID, out Skill saved))
                {
                    skill.SetRank(saved.CurrentRank());
                    skill.skillLocked = saved.skillLocked;
                }
            }
        }

        #endregion

        #region Helper Functions

        private Texture2D ConvertRenderTextureToTexture2D(RenderTexture renderTexture)
        {
            // Create a new Texture2D with the same dimensions as the render texture
            Texture2D texture2D = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.RGB24, false);

            // Backup the currently active render texture
            RenderTexture previous = RenderTexture.active;

            // Set the provided RenderTexture as the currently active one
            RenderTexture.active = renderTexture;

            // Ensure we are reading within the render texture bounds
            if (renderTexture.width != texture2D.width || renderTexture.height != texture2D.height)
            {
                Debug.LogError("RenderTexture and Texture2D dimensions do not match.");
                return null;
            }

            // Read the pixels from the active render texture into the Texture2D
            texture2D.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
            texture2D.Apply();

            // Restore the previously active render texture
            RenderTexture.active = previous;

            // Apply gamma correction if needed
            if (QualitySettings.activeColorSpace == ColorSpace.Linear)
            {
                Color[] pixels = texture2D.GetPixels();
                for (int i = 0; i < pixels.Length; i++)
                {
                    pixels[i] = pixels[i].gamma;
                }
                texture2D.SetPixels(pixels);
                texture2D.Apply();
            }

            return texture2D;
        }

        private Texture2D ConvertImageToTexture2D(Image image)
        {
            // Ensure the image has a sprite
            if (image.sprite == null)
            {
                Debug.LogError("The Image component does not have a sprite.");
                return null;
            }

            // Get the texture from the sprite
            Texture2D spriteTexture = image.sprite.texture;

            // Ensure the texture is readable
            if (!spriteTexture.isReadable)
            {
                Debug.LogError("The texture is not readable. Please enable 'Read/Write Enabled' in the texture import settings.");
                return null;
            }

            // Create a new Texture2D with the same dimensions and a supported format
            Texture2D texture2D = new Texture2D(spriteTexture.width, spriteTexture.height, TextureFormat.RGBA32, false);

            // Get the pixels from the sprite texture
            Color[] pixels = spriteTexture.GetPixels((int)image.sprite.rect.x,
                                                     (int)image.sprite.rect.y,
                                                     (int)image.sprite.rect.width,
                                                     (int)image.sprite.rect.height);

            // Set the pixels to the new Texture2D
            texture2D.SetPixels(pixels);
            texture2D.Apply();

            return texture2D;
        }

        private Sprite CreateSpriteFromTexture2D(Texture2D texture)
        {
            // Create a new sprite from the Texture2D
            return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
        }

        public bool CheckForCombat()
        {
            // Find all AttackStateBehaviour instances in the scene
            BlazeAISpace.AttackStateBehaviour[] allScripts = FindObjectsOfType<BlazeAISpace.AttackStateBehaviour>();

            // Count only those that are enabled
            int enabledCount = 0;
            int enabledWithinRange = 0;
            foreach (var script in allScripts)
            {
                if (script.enabled)
                {
                    enabledCount++;

                    if (Vector3.Distance(playerLocomotion.myTransform.position, script.gameObject.transform.position) < maxDistanceCombat)
                    {
                        enabledWithinRange++;
                    }

                }          
            }
            Debug.Log("Enabled AttackStateBehaviour scripts: " + enabledCount + ", within range: " + enabledWithinRange);
            // check which of those are within save-blocking distance
            
            return (enabledWithinRange > 0);
        }

        #endregion

    }
}
