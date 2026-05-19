using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Linq;

namespace etchebarren
{
    public class PlayerInventory : MonoBehaviour
    {
        public static PlayerInventory instance;

        [Header("Script References")]
        public QuestManager questManager;
        public PlayerStats playerStats;

        [Header("DO NOT SET MANUALLY:")]
        public List<WeaponItem> weaponsInventory = new List<WeaponItem>();
        public List<ShieldItem> shieldsInventory = new List<ShieldItem>();
        public List<TorsoArmorItem> torsoArmorInventory = new List<TorsoArmorItem>();
        public List<HandsArmorItem> handsArmorInventory = new List<HandsArmorItem>();
        public List<LegsArmorItem> legsArmorInventory = new List<LegsArmorItem>();
        public List<RingItem> ringsInventory = new List<RingItem>();
        public List<AmuletItem> amuletsInventory = new List<AmuletItem>();
        public List<Spell> spellsInventory = new List<Spell>();
        public List<Consumable> consumablesInventory = new List<Consumable>();
        public List<KeyItem> keyItemsInventory = new List<KeyItem>();

        public int goldCount = 0;
        public TextMeshProUGUI goldAmountText;

        //EQUIPPED:
        public WeaponItem equippedWeapon;
        public ShieldItem equippedShield;
        public TorsoArmorItem equippedTorsoArmor;
        public HandsArmorItem equippedHandsArmor;
        public LegsArmorItem equippedLegsArmor;
        public RingItem equippedRing1;
        public RingItem equippedRing2;
        public AmuletItem equippedAmulet;

        public Spell[] equippedSpells;
        public List<Consumable> equippedConsumables = new List<Consumable>();

        //NULL/EMPTY ARMORS:
        [HideInInspector] public WeaponItem emptyWeapon;
        [HideInInspector] public ShieldItem emptyShield;
        [HideInInspector] public TorsoArmorItem emptyTorsoArmor;
        [HideInInspector] public HandsArmorItem emptyHandsArmor;
        [HideInInspector] public LegsArmorItem emptyLegsArmor;
        [HideInInspector] public RingItem emptyRing;
        [HideInInspector] public AmuletItem emptyAmulet; //currently unused

        [Header("RARITY COLORS FOR INVENTORY SLOTS")]
        private Color common = new Color(0.6f, 0.6f, 0.6f, 0.6f);
        private Color uncommon = new Color(0.236f, 0.660f, 0.165f, 0.486f);
        private Color rare = new Color(0.240f, 0.414f, 0.783f, 0.573f);
        private Color epic = new Color(0.6f, 0.240f, 0.783f, 0.573f);
        private Color legendary = new Color(0.783f, 0.728f, 0.240f, 0.545f);

        [Header("RARITY COLORS FOR SHOP SLOTS")]
        private Color commonShop = new Color(0.6f, 0.6f, 0.6f, 0.9f);
        private Color uncommonShop = new Color(0.236f, 0.660f, 0.165f, 0.9f);
        private Color rareShop = new Color(0.240f, 0.414f, 0.783f, 0.9f);
        private Color epicShop = new Color(0.6f, 0.240f, 0.783f, 0.9f);
        private Color legendaryShop = new Color(0.783f, 0.728f, 0.240f, 0.9f);

        [HideInInspector] public Color[] rarityColors;
        [HideInInspector] public Color[] shopRarityColors;

        [Header("ICONS")]
        public List<Sprite> swordIcons = new List<Sprite>();
        public List<Sprite> shieldIcons = new List<Sprite>();
        public List<Sprite> torsoArmorIcons = new List<Sprite>();
        public List<Sprite> handsArmorIcons = new List<Sprite>();
        public List<Sprite> legsArmorIcons = new List<Sprite>();
        public List<Sprite> ringIcons = new List<Sprite>();
        public List<Sprite> amuletIcons = new List<Sprite>();
        public List<Sprite> spellIcons = new List<Sprite>();
        public List<Sprite> spellIcons_45deg = new List<Sprite>();
        public List<Sprite> consumableIcons = new List<Sprite>();
        public List<Sprite> keyItemIcons = new List<Sprite>();

        [Header("BODY MODELS")]
        public List<GameObject> bodyHandsModels = new List<GameObject>();
        public List<GameObject> bodyUpperModels = new List<GameObject>();
        public List<GameObject> bodyLowerModels = new List<GameObject>();

        [Header("WEAPON MODELS")]
        public List<GameObject> weaponModelsM = new List<GameObject>();
        public List<GameObject> weaponModelsF = new List<GameObject>();

        [Header("SHIELD MODELS")]
        public List<GameObject> shieldModelsM = new List<GameObject>();
        public List<GameObject> shieldModelsF = new List<GameObject>();

        [Header("TORSO ARMOR MODELS")]
        public List<GameObject> torsoArmorModelsM = new List<GameObject>();
        public List<GameObject> torsoArmorModelsF = new List<GameObject>();

        [Header("HANDS ARMOR MODELS")]
        public List<GameObject> handsArmorModelsM = new List<GameObject>();
        public List<GameObject> handsArmorModelsF = new List<GameObject>();

        [Header("LEGS ARMOR MODELS")]
        public List<GameObject> legsArmorModelsM = new List<GameObject>();
        public List<GameObject> legsArmorModelsF = new List<GameObject>();

        [Header("Misc")]
        public TextMeshProUGUI keyCountText;

        [Header("Audio")]
        public AudioSource coinsAudioSource;
        public AudioClip[] coinSounds;
        [Range(0.0f, 1.0f)]
        public float coinsVolume = 1.0f;
        public AudioSource itemsAudioSource;
        public AudioClip itemGrab;
        [Range(0.0f, 1.0f)]
        public float itemGrabVolume = 1.0f;
        public AudioClip[] itemSounds;
        [Range(0.0f, 1.0f)]
        public float itemSoundsVolume = 1.0f;

        [Header("Loading Flags")]
        public bool loadedGame = false;

        private void Awake()
        {
            GenerateFirstSword();

            if (instance == null)
            {
                instance = this;
            }
            else
            {
                Destroy(gameObject);
            }

            rarityColors = new Color[] { common, uncommon, rare, epic, legendary };
            shopRarityColors = new Color[] { commonShop, uncommonShop, rareShop, epicShop, legendaryShop };

            equippedSpells = new Spell[8];

            if (!loadedGame)
            {
                //STARTING GEAR:
                GenerateEmptyWeapon(true);
                GenerateEmptyShield(true);
                //GenerateFirstSword();
                //GenerateFirstShield();
                GenerateFirstTorsoArmor();
                GenerateFirstHandsArmor();
                GenerateFirstLegsArmor();

                //GenerateSpell_MinorArcaneBolt();
                //GenerateSpell_Fireball();
                //GenerateSpell_Zapshock();
                //GenerateSpell_Frostshard();
                //GenerateSpell_Mend();
                //GenerateSpell_Rejuvinate();
                //GenerateSpell_Meteor();
                //GenerateSpell_GlacialBurst();
                //GenerateSpell_Stormguard();

                //EMPTY ARMORS
                GenerateEmptyTorsoArmor();
                GenerateEmptyHandsArmor();
                GenerateEmptyLegsArmor();
                GenerateEmptyRing();
            }

            // TESTING ONLY (DELETE):
            //GenerateSpell_ArcanaLumina();
            //GenerateConsumable(1, 1);
            //GenerateConsumable(2, 1);

            //GenerateConsumable(3, 1);
            //GenerateConsumable(4, 1);
            //GenerateConsumable(5, 1);

            //GenerateConsumable(6, 1);
            //GenerateConsumable(7, 1);
            //GenerateConsumable(8, 1);
        }

        private void Start()
        {
            if(!loadedGame)
            {
                //TESTING GENERATION:
                //Generate12Swords();
                //Generate13Shields();
                //GenerateXTorsoArmors(11);
                //GenerateXHandsArmors(13);
                //GenerateXLegsArmors(12);
                //GenerateXRings(14);
                //GenerateXAmulets(30);

                //GenerateConsumable(0, 5);
                //GenerateConsumable(1, 1);
                //GenerateConsumable(2, 1);

                //GenerateConsumable(3, 1);
                //GenerateConsumable(4, 1);
                //GenerateConsumable(5, 1);

                //GenerateConsumable(6, 1);
                //GenerateConsumable(7, 1);
                //GenerateConsumable(8, 1);

                //GenerateConsumable(9, 1);
                //GenerateConsumable(10, 1);
                //GenerateConsumable(11, 1);

                //GenerateConsumable(12, 1);

                //GenerateKeyItem(800, 3);
            }

            //Initialize gold text string
            goldAmountText.text = goldCount.ToString("N0");

            GenerateConsumable(0, 5);
            GenerateConsumable(1, 1);
            GenerateConsumable(2, 1);

            GenerateConsumable(3, 1);
            GenerateConsumable(4, 1);
            GenerateConsumable(5, 1);

            GenerateConsumable(6, 1);
            GenerateConsumable(7, 1);
            GenerateConsumable(8, 1);

            GenerateConsumable(9, 1);
            GenerateConsumable(10, 1);
            GenerateConsumable(11, 1);

            GenerateConsumable(12, 1);
        }

        // GOLD
        public void AddGold(int amount, bool sfx=false)
        {
            goldCount += amount;
            goldAmountText.text = goldCount.ToString("N0");
            if (sfx)
            {
                coinsAudioSource.PlayOneShot(coinSounds[Random.Range(0, coinSounds.Length)], 1.0f);
            }
        }

        public void RemoveGold(int amount)
        {
            goldCount -= amount;
            goldAmountText.text = goldCount.ToString("N0");
        }

        // UPDATE MODELS

        public void UpdateWeaponModel()
        {
            for (int i = 0; i < weaponModelsM.Count; i++)
            {
                weaponModelsM[i].SetActive(i == equippedWeapon.weaponModelID);
                weaponModelsF[i].SetActive(i == equippedWeapon.weaponModelID);
            }
        }

        public void UpdateShieldModel()
        {
            for (int i = 0; i < shieldModelsM.Count; i++)
            {
                shieldModelsM[i].SetActive(i == equippedShield.shieldModelID);
                shieldModelsF[i].SetActive(i == equippedShield.shieldModelID);
            }
        }

        public void UpdateTorsoArmorModel(bool equip)
        {
            if (equip)
            {
                for (int i = 0; i < torsoArmorModelsM.Count; i++)
                {
                    torsoArmorModelsM[i].SetActive(i == equippedTorsoArmor.torsoArmorModelID);
                    torsoArmorModelsF[i].SetActive(i == equippedTorsoArmor.torsoArmorModelID);
                }

                foreach (GameObject obj in bodyUpperModels)
                {
                    obj.SetActive(false);
                }
            }
            else
            {
                for (int i = 0; i < torsoArmorModelsM.Count; i++)
                {
                    torsoArmorModelsM[i].SetActive(false);
                    torsoArmorModelsF[i].SetActive(false);
                }

                foreach (GameObject obj in bodyUpperModels)
                {
                    obj.SetActive(true);
                }
            }
        }

        public void UpdateHandsArmorModel(bool equip)
        {
            if (equip)
            {
                for (int i = 0; i < handsArmorModelsM.Count; i++)
                {
                    handsArmorModelsM[i].SetActive(i == equippedHandsArmor.handsArmorModelID);
                    handsArmorModelsF[i].SetActive(i == equippedHandsArmor.handsArmorModelID);
                }

                foreach (GameObject obj in bodyHandsModels)
                {
                    obj.SetActive(false);
                }
            }
            else
            {
                for (int i = 0; i < handsArmorModelsM.Count; i++)
                {
                    handsArmorModelsM[i].SetActive(false);
                    handsArmorModelsF[i].SetActive(false);
                }

                foreach (GameObject obj in bodyHandsModels)
                {
                    obj.SetActive(true);
                }
            }
        }

        public void UpdateLegsArmorModel(bool equip)
        {
            if (equip)
            {
                for (int i = 0; i < legsArmorModelsM.Count; i++)
                {
                    legsArmorModelsM[i].SetActive(i == equippedLegsArmor.legsArmorModelID);
                    legsArmorModelsF[i].SetActive(i == equippedLegsArmor.legsArmorModelID);
                }

                foreach (GameObject obj in bodyLowerModels)
                {
                    obj.SetActive(false);
                }
            }
            else
            {
                for (int i = 0; i < legsArmorModelsM.Count; i++)
                {
                    legsArmorModelsM[i].SetActive(false);
                    legsArmorModelsF[i].SetActive(false);
                }

                foreach (GameObject obj in bodyLowerModels)
                {
                    obj.SetActive(true);
                }
            }
        }

        //GENERATION FUNCTIONS:

        private bool ArrayContains(int[] array, int value)
        {
            foreach (int item in array)
            {
                if (item == value)
                {
                    return true;
                }
            }
            return false;
        }

        public int[] GenerateDistinctRandomNumbers(int count, int min, int max)
        {
            if (count > 4)
            {
                count = 4;
            }

            int[] numbers = new int[count];

            for (int i = 0; i < numbers.Length; i++)
            {
                numbers[i] = -1;
            }

            int currentIndex = 0;

            while (currentIndex < count)
            {
                int randomNumber = Random.Range(min, max + 1);

                if (!ArrayContains(numbers, randomNumber)) // Use custom ArrayContains method
                {
                    numbers[currentIndex] = randomNumber;
                    currentIndex++;
                }
            }

            return numbers;
        }

        public void Generate12Swords()
        {
            for (int i = 1; i < 40; i++)
            {
                WeaponItem newWeapon = ScriptableObject.CreateInstance<WeaponItem>();

                newWeapon.itemName = "Knight's Sword " + (i + 1).ToString();
                newWeapon.weaponModelID = i % 12;
                newWeapon.itemIcon = swordIcons[newWeapon.weaponModelID];
                newWeapon.itemDescription = "";
                newWeapon.itemID = 0;
                newWeapon.level = Random.Range(1, 10); //101
                newWeapon.sellable = true;
                newWeapon.rarity = Random.Range(0, 5);
                newWeapon.goldValue = Random.Range(100, 1000);
                newWeapon.count = 1;
                //weapon specific
                //newWeapon.weaponModelID = 1;
                newWeapon.physicalDamage = Random.Range(10, 200);
                newWeapon.magicDamage = Random.Range(10, 200);
                newWeapon.poise = 10;
                newWeapon.baseStaminaCost = 20;

                //SET RANDOM STATS
                int[] numbers = GenerateDistinctRandomNumbers(newWeapon.rarity, 0, 11); //12 possible stats. rarity 4(legendary) = 4 stats
                foreach (int num in numbers)
                {
                    switch (num)
                    {
                        case 0:
                            newWeapon.strength = Random.Range(1, 90);
                            break;
                        case 1:
                            newWeapon.endurance = Random.Range(1, 90);
                            break;
                        case 2:
                            newWeapon.vitality = Random.Range(1, 90);
                            break;
                        case 3:
                            newWeapon.precision = Random.Range(1, 90);
                            break;
                        case 4:
                            newWeapon.dexterity = Random.Range(1, 90);
                            break;
                        case 5:
                            newWeapon.expertise = Random.Range(1, 90);
                            break;
                        case 6:
                            newWeapon.intelligence = Random.Range(1, 90);
                            break;
                        case 7:
                            newWeapon.spirit = Random.Range(1, 90);
                            break;
                        case 8:
                            newWeapon.willpower = Random.Range(1, 90);
                            break;
                        case 9:
                            newWeapon.luck = Random.Range(1, 90);
                            break;
                        case 10:
                            newWeapon.criticalChance = Random.Range(0.1f, 25.00001f);
                            break;
                        case 11:
                            newWeapon.criticalDamage = Random.Range(0.1f, 150.00001f);
                            break;
                    }
                }

                newWeapon.name = "WeaponItem_" + newWeapon.weaponModelID.ToString() + "_" + newWeapon.level.ToString() + "_" + newWeapon.physicalDamage.ToString() + "_" + newWeapon.magicDamage.ToString();

                weaponsInventory.Add(newWeapon);

            }
        }

        public void Generate13Shields()
        {
            for (int i = 1; i < 14; i++)
            {
                ShieldItem newShield = ScriptableObject.CreateInstance<ShieldItem>();

                newShield.itemName = "Knight's Shield" + (i + 1).ToString();
                newShield.shieldModelID = i;
                newShield.itemIcon = shieldIcons[newShield.shieldModelID];
                newShield.itemDescription = "";
                newShield.itemID = 0;
                newShield.level = Random.Range(1, 101);
                newShield.sellable = true;
                newShield.rarity = Random.Range(0, 5);
                newShield.goldValue = Random.Range(100, 1000);
                newShield.count = 1;

                //weapon specific
                newShield.blockRating = Random.Range(10, 200);

                //SET RANDOM STATS
                int[] numbers = GenerateDistinctRandomNumbers(newShield.rarity, 0, 12); //13 possible stats. rarity 4(legendary) = 4 stats
                foreach (int num in numbers)
                {
                    switch (num)
                    {
                        case 0:
                            newShield.strength = Random.Range(1, 90);
                            break;
                        case 1:
                            newShield.endurance = Random.Range(1, 90);
                            break;
                        case 2:
                            newShield.vitality = Random.Range(1, 90);
                            break;
                        case 3:
                            newShield.precision = Random.Range(1, 90);
                            break;
                        case 4:
                            newShield.dexterity = Random.Range(1, 90);
                            break;
                        case 5:
                            newShield.expertise = Random.Range(1, 90);
                            break;
                        case 6:
                            newShield.intelligence = Random.Range(1, 90);
                            break;
                        case 7:
                            newShield.spirit = Random.Range(1, 90);
                            break;
                        case 8:
                            newShield.willpower = Random.Range(1, 90);
                            break;
                        case 9:
                            newShield.luck = Random.Range(1, 90);
                            break;
                        case 10:
                            newShield.fireResistance = Random.Range(0.1f, 15.00001f);
                            break;
                        case 11:
                            newShield.iceResistance = Random.Range(0.1f, 15.00001f);
                            break;
                        case 12:
                            newShield.shockResistance = Random.Range(0.1f, 15.00001f);
                            break;
                    }
                }
                newShield.name = "ShieldItem_" + newShield.shieldModelID.ToString() + "_" + newShield.level.ToString() + "_" + newShield.blockRating.ToString();

                //Add to Inventory
                shieldsInventory.Add(newShield);

            }
        }

        public void GenerateXTorsoArmors(int amount)
        {
            for (int i = 1; i < amount + 1; i++)
            {
                TorsoArmorItem newTorsoArmor = ScriptableObject.CreateInstance<TorsoArmorItem>();

                newTorsoArmor.itemName = "Knight's Armor " + (i + 1).ToString();
                newTorsoArmor.torsoArmorModelID = (i % 2);
                newTorsoArmor.itemIcon = torsoArmorIcons[newTorsoArmor.torsoArmorModelID];
                newTorsoArmor.itemDescription = "";
                newTorsoArmor.itemID = 0;
                newTorsoArmor.level = Random.Range(1, 101);
                newTorsoArmor.sellable = true;
                newTorsoArmor.rarity = Random.Range(0, 5);
                newTorsoArmor.goldValue = Random.Range(100, 1000);
                newTorsoArmor.count = 1;

                //armor specific
                newTorsoArmor.physicalArmorRating = Random.Range(10, 200);
                newTorsoArmor.magicalArmorRating = Random.Range(10, 200);

                //SET RANDOM STATS
                int[] numbers = GenerateDistinctRandomNumbers(newTorsoArmor.rarity, 0, 14); //15 possible stats. rarity 4(legendary) = 4 stats
                foreach (int num in numbers)
                {
                    switch (num)
                    {
                        case 0:
                            newTorsoArmor.strength = Random.Range(1, 90);
                            break;
                        case 1:
                            newTorsoArmor.endurance = Random.Range(1, 90);
                            break;
                        case 2:
                            newTorsoArmor.vitality = Random.Range(1, 90);
                            break;
                        case 3:
                            newTorsoArmor.precision = Random.Range(1, 90);
                            break;
                        case 4:
                            newTorsoArmor.dexterity = Random.Range(1, 90);
                            break;
                        case 5:
                            newTorsoArmor.expertise = Random.Range(1, 90);
                            break;
                        case 6:
                            newTorsoArmor.intelligence = Random.Range(1, 90);
                            break;
                        case 7:
                            newTorsoArmor.spirit = Random.Range(1, 90);
                            break;
                        case 8:
                            newTorsoArmor.willpower = Random.Range(1, 90);
                            break;
                        case 9:
                            newTorsoArmor.luck = Random.Range(1, 90);
                            break;
                        case 10:
                            newTorsoArmor.criticalChance = Random.Range(0.1f, 15.00001f);
                            break;
                        case 11:
                            newTorsoArmor.criticalDamage = Random.Range(0.1f, 15.00001f);
                            break;
                        case 12:
                            newTorsoArmor.fireResistance = Random.Range(0.1f, 15.00001f);
                            break;
                        case 13:
                            newTorsoArmor.iceResistance = Random.Range(0.1f, 15.00001f);
                            break;
                        case 14:
                            newTorsoArmor.shockResistance = Random.Range(0.1f, 15.00001f);
                            break;
                    }
                }

                newTorsoArmor.name = "TorsoArmorItem_" + newTorsoArmor.torsoArmorModelID.ToString() + "_" + newTorsoArmor.level.ToString() + "_" + newTorsoArmor.physicalArmorRating.ToString() + "_" + newTorsoArmor.magicalArmorRating.ToString();

                //Add to Inventory
                torsoArmorInventory.Add(newTorsoArmor);

            }
        }

        public void GenerateXHandsArmors(int amount)
        {
            for (int i = 1; i < amount + 1; i++)
            {
                HandsArmorItem newHandsArmor = ScriptableObject.CreateInstance<HandsArmorItem>();

                newHandsArmor.itemName = "Knight's Gauntlets " + (i + 1).ToString();
                newHandsArmor.handsArmorModelID = (i % 2);
                newHandsArmor.itemIcon = handsArmorIcons[newHandsArmor.handsArmorModelID];
                newHandsArmor.itemDescription = "";
                newHandsArmor.itemID = 0;
                newHandsArmor.level = Random.Range(1, 101);
                newHandsArmor.sellable = true;
                newHandsArmor.rarity = Random.Range(0, 5);
                newHandsArmor.goldValue = Random.Range(100, 1000);
                newHandsArmor.count = 1;

                //armor specific
                newHandsArmor.physicalArmorRating = Random.Range(10, 200);
                newHandsArmor.magicalArmorRating = Random.Range(10, 200);

                //SET RANDOM STATS
                int[] numbers = GenerateDistinctRandomNumbers(newHandsArmor.rarity, 0, 6); //7 possible stats. rarity 4(legendary) = 4 stats
                foreach (int num in numbers)
                {
                    switch (num)
                    {
                        case 0:
                            newHandsArmor.strength = Random.Range(1, 90);
                            break;
                        case 1:
                            newHandsArmor.precision = Random.Range(1, 90);
                            break;
                        case 2:
                            newHandsArmor.intelligence = Random.Range(1, 90);
                            break;
                        case 3:
                            newHandsArmor.spirit = Random.Range(1, 90);
                            break;
                        case 4:
                            newHandsArmor.luck = Random.Range(1, 90);
                            break;
                        case 5:
                            newHandsArmor.criticalChance = Random.Range(0.1f, 15.00001f);
                            break;
                        case 6:
                            newHandsArmor.criticalDamage = Random.Range(0.1f, 15.00001f);
                            break;
                    }
                }

                newHandsArmor.name = "HandsArmorItem_" + newHandsArmor.handsArmorModelID.ToString() + "_" + newHandsArmor.level.ToString() + "_" + newHandsArmor.physicalArmorRating.ToString() + "_" + newHandsArmor.magicalArmorRating.ToString();

                //Add to Inventory
                handsArmorInventory.Add(newHandsArmor);

            }
        }

        public void GenerateXLegsArmors(int amount)
        {
            for (int i = 1; i < amount + 1; i++)
            {
                LegsArmorItem newLegsArmor = ScriptableObject.CreateInstance<LegsArmorItem>();

                newLegsArmor.itemName = "Knight's Greaves " + (i + 1).ToString();
                newLegsArmor.legsArmorModelID = (i % 2);
                newLegsArmor.itemIcon = legsArmorIcons[newLegsArmor.legsArmorModelID];
                newLegsArmor.itemDescription = "";
                newLegsArmor.itemID = 0;
                newLegsArmor.level = Random.Range(1, 101);
                newLegsArmor.sellable = true;
                newLegsArmor.rarity = Random.Range(0, 5);
                newLegsArmor.goldValue = Random.Range(100, 1000);
                newLegsArmor.count = 1;

                //armor specific
                newLegsArmor.physicalArmorRating = Random.Range(10, 200);
                newLegsArmor.magicalArmorRating = Random.Range(10, 200);

                //SET RANDOM STATS
                int[] numbers = GenerateDistinctRandomNumbers(newLegsArmor.rarity, 0, 7); //8 possible stats. rarity 4(legendary) = 4 stats
                foreach (int num in numbers)
                {
                    switch (num)
                    {
                        case 0:
                            newLegsArmor.endurance = Random.Range(1, 90);
                            break;
                        case 1:
                            newLegsArmor.vitality = Random.Range(1, 90);
                            break;
                        case 2:
                            newLegsArmor.dexterity = Random.Range(1, 90);
                            break;
                        case 3:
                            newLegsArmor.expertise = Random.Range(1, 90);
                            break;
                        case 4:
                            newLegsArmor.willpower = Random.Range(1, 90);
                            break;
                        case 5:
                            newLegsArmor.fireResistance = Random.Range(0.1f, 15.00001f);
                            break;
                        case 6:
                            newLegsArmor.iceResistance = Random.Range(0.1f, 15.00001f);
                            break;
                        case 7:
                            newLegsArmor.shockResistance = Random.Range(0.1f, 15.00001f);
                            break;
                    }
                }

                newLegsArmor.name = "LegsArmorItem_" + newLegsArmor.legsArmorModelID.ToString() + "_" + newLegsArmor.level.ToString() + "_" + newLegsArmor.physicalArmorRating.ToString() + "_" + newLegsArmor.magicalArmorRating.ToString();

                //Add to Inventory
                legsArmorInventory.Add(newLegsArmor);

            }
        }

        public void GenerateXRings(int amount)
        {
            for (int i = 1; i < amount + 1; i++)
            {
                RingItem newRing = ScriptableObject.CreateInstance<RingItem>();

                newRing.itemName = "Ring " + (i + 1).ToString();
                //DETERMINE RARITY
                newRing.rarity = Random.Range(0, 5);

                //LEGENDARY LOGIC
                int legendaryCounter = 0;
                if (newRing.rarity == 4)
                {
                    newRing.legendaryStat = Random.Range(0, 4); //0,1,2,3
                }

                newRing.itemDescription = "";
                newRing.itemID = 0;
                newRing.level = Random.Range(1, 101);
                newRing.sellable = true;
                newRing.count = 1;
                newRing.goldValue = Random.Range(100, 1000);

                //NOTE: currently, common rings have NO effects. may want to treat it as an uncommon ring and reduce the effect by a percentage?
                //SET RANDOM STATS
                int[] numbers = GenerateDistinctRandomNumbers(newRing.rarity + 1, 0, 14); //15 possible stats. rarity 4(legendary) = 4 stats
                foreach (int num in numbers)
                {
                    switch (num)
                    {
                        case 0:
                            newRing.strength = Random.Range(1, 90);
                            if (legendaryCounter == newRing.legendaryStat)
                                newRing.strength *= 2;
                            legendaryCounter++;
                            break;
                        case 1:
                            newRing.endurance = Random.Range(1, 90);
                            if (legendaryCounter == newRing.legendaryStat)
                                newRing.endurance *= 2;
                            legendaryCounter++;
                            break;
                        case 2:
                            newRing.vitality = Random.Range(1, 90);
                            if (legendaryCounter == newRing.legendaryStat)
                                newRing.vitality *= 2;
                            legendaryCounter++;
                            break;
                        case 3:
                            newRing.precision = Random.Range(1, 90);
                            if (legendaryCounter == newRing.legendaryStat)
                                newRing.precision *= 2;
                            legendaryCounter++;
                            break;
                        case 4:
                            newRing.dexterity = Random.Range(1, 90);
                            if (legendaryCounter == newRing.legendaryStat)
                                newRing.dexterity *= 2;
                            legendaryCounter++;
                            break;
                        case 5:
                            newRing.expertise = Random.Range(1, 90);
                            if (legendaryCounter == newRing.legendaryStat)
                                newRing.expertise *= 2;
                            legendaryCounter++;
                            break;
                        case 6:
                            newRing.intelligence = Random.Range(1, 90);
                            if (legendaryCounter == newRing.legendaryStat)
                                newRing.intelligence *= 2;
                            legendaryCounter++;
                            break;
                        case 7:
                            newRing.spirit = Random.Range(1, 90);
                            if (legendaryCounter == newRing.legendaryStat)
                                newRing.spirit *= 2;
                            legendaryCounter++;
                            break;
                        case 8:
                            newRing.willpower = Random.Range(1, 90);
                            if (legendaryCounter == newRing.legendaryStat)
                                newRing.willpower *= 2;
                            legendaryCounter++;
                            break;
                        case 9:
                            newRing.luck = Random.Range(1, 90);
                            if (legendaryCounter == newRing.legendaryStat)
                                newRing.luck *= 2;
                            legendaryCounter++;
                            break;
                        case 10:
                            newRing.criticalChance = Random.Range(0.1f, 15.00001f);
                            if (legendaryCounter == newRing.legendaryStat)
                                newRing.criticalChance *= 2;
                            legendaryCounter++;
                            break;
                        case 11:
                            newRing.criticalDamage = Random.Range(0.1f, 15.00001f);
                            if (legendaryCounter == newRing.legendaryStat)
                                newRing.criticalDamage *= 2;
                            legendaryCounter++;
                            break;
                        case 12:
                            newRing.fireResistance = Random.Range(0.1f, 15.00001f);
                            if (legendaryCounter == newRing.legendaryStat)
                                newRing.fireResistance *= 2;
                            legendaryCounter++;
                            break;
                        case 13:
                            newRing.iceResistance = Random.Range(0.1f, 15.00001f);
                            if (legendaryCounter == newRing.legendaryStat)
                                newRing.iceResistance *= 2;
                            legendaryCounter++;
                            break;
                        case 14:
                            newRing.shockResistance = Random.Range(0.1f, 15.00001f);
                            if (legendaryCounter == newRing.legendaryStat)
                                newRing.shockResistance *= 2;
                            legendaryCounter++;
                            break;
                    }
                }

                //DETERMINE ICON DEPENDING ON RARITY: can modify later to assign a specific icon based on stats
                switch (newRing.rarity)
                {
                    case 0:
                        newRing.ringModelID = Random.Range(0, 3); //0, 1, or 2
                        break;
                    case 1:
                        newRing.ringModelID = Random.Range(0, 3); //same as above
                        break;
                    case 2:
                        newRing.ringModelID = Random.Range(3, 7); //3, 4, 5, or 6
                        break;
                    case 3:
                        newRing.ringModelID = Random.Range(7, 11); //7, 6, 9, or 10
                        break;
                    case 4:
                        newRing.ringModelID = Random.Range(11, 15); //11, 12, 13, or 14
                        break;
                    default:
                        break;
                }

                newRing.itemIcon = ringIcons[newRing.ringModelID];

                newRing.name = "RingItem_" + newRing.ringModelID.ToString() + "_" + newRing.level.ToString();

                //Add to Inventory
                ringsInventory.Add(newRing);

            }
        }

        public void GenerateXAmulets(int amount)
        {
            for (int i = 1; i < amount + 1; i++)
            {
                //AmuletItem newAmulet = new AmuletItem();
                AmuletItem newAmulet = ScriptableObject.CreateInstance<AmuletItem>();

                newAmulet.itemName = "Amulet " + (i + 1).ToString();
                //DETERMINE RARITY
                newAmulet.rarity = Random.Range(0, 5);

                //LEGENDARY LOGIC
                int legendaryCounter = 0;
                if (newAmulet.rarity == 4)
                {
                    newAmulet.legendaryStat = Random.Range(0, 4); //0,1,2,3
                }

                newAmulet.itemDescription = "";
                newAmulet.itemID = 0;
                newAmulet.level = Random.Range(1, 101);
                newAmulet.sellable = true;
                newAmulet.count = 1;
                newAmulet.goldValue = Random.Range(100, 1000);

                //SET RANDOM STATS
                int[] numbers = GenerateDistinctRandomNumbers(newAmulet.rarity + 1, 0, 4); //rarity = number of stats, the last value should equal the last case.. case4->thenlast arg is 4
                foreach (int num in numbers)
                {
                    switch (num)
                    {
                        case 0:
                            newAmulet.criticalChance = Random.Range(0.1f, 15.00001f);
                            if (legendaryCounter == newAmulet.legendaryStat)
                            {
                                newAmulet.criticalChance *= 2;
                            }
                            legendaryCounter++;
                            break;
                        case 1:
                            newAmulet.criticalDamage = Random.Range(0.1f, 15.00001f);
                            if (legendaryCounter == newAmulet.legendaryStat)
                            {
                                newAmulet.criticalDamage *= 2;
                            }
                            legendaryCounter++;
                            break;
                        case 2:
                            newAmulet.fireResistance = Random.Range(0.1f, 15.00001f);
                            if (legendaryCounter == newAmulet.legendaryStat)
                            {
                                newAmulet.fireResistance *= 2;
                            }
                            legendaryCounter++;
                            break;
                        case 3:
                            newAmulet.iceResistance = Random.Range(0.1f, 15.00001f);
                            if (legendaryCounter == newAmulet.legendaryStat)
                            {
                                newAmulet.iceResistance *= 2;
                            }
                            legendaryCounter++;
                            break;
                        case 4:
                            newAmulet.shockResistance = Random.Range(0.1f, 15.00001f);
                            if (legendaryCounter == newAmulet.legendaryStat)
                            {
                                newAmulet.shockResistance *= 2;
                            }
                            legendaryCounter++;
                            break;
                    }
                }

                //DETERMINE ICON DEPENDING ON RARITY: can modify later to assign a specific icon based on stats
                switch (newAmulet.rarity)
                {
                    case 0:
                        newAmulet.amuletModelID = Random.Range(0, 5); //0, 1, 2, 3, 4
                        break;
                    case 1:
                        newAmulet.amuletModelID = Random.Range(5, 10); //5, 6, 7, 8, 9 
                        break;
                    case 2:
                        newAmulet.amuletModelID = Random.Range(10, 15); //10, 11, 12, 13, 14
                        break;
                    case 3:
                        newAmulet.amuletModelID = Random.Range(15, 20); //15, 16, 17, 18, 19
                        break;
                    case 4:
                        newAmulet.amuletModelID = Random.Range(20, 26); //20, 21, 22, 23, 24, 25
                        break;
                    default:
                        break;
                }

                newAmulet.itemIcon = amuletIcons[newAmulet.amuletModelID];

                newAmulet.name = "AmuletItem_" + newAmulet.amuletModelID.ToString() + "_" + newAmulet.level.ToString();

                //Add to Inventory
                amuletsInventory.Add(newAmulet);

            }
        }

        //GENERATE STARTER GEAR:

        public void GenerateFirstSword()
        {
            WeaponItem newWeapon = ScriptableObject.CreateInstance<WeaponItem>();

            newWeapon.itemName = "Knight's Sword";
            newWeapon.weaponModelID = 0;
            newWeapon.itemIcon = swordIcons[newWeapon.weaponModelID];
            //newWeapon.itemIcon = knightSwordIcon;
            newWeapon.itemDescription = "Each nick and scratch serves as a testament to its enduring journey, a companion that has stood steadfast in the heat of conflict, ever ready to defend its bearer.";
            newWeapon.itemID = 0;
            newWeapon.itemID = 0;
            newWeapon.level = 1;
            newWeapon.sellable = false;
            newWeapon.rarity = 0;
            newWeapon.goldValue = 123;
            //weapon specific
            newWeapon.count = 1;
            newWeapon.physicalDamage = 10;
            newWeapon.magicDamage = 10;
            newWeapon.poise = 10;
            newWeapon.baseStaminaCost = 20;

            newWeapon.name = "WeaponItem_" + newWeapon.weaponModelID.ToString() + "_" + newWeapon.level.ToString() + "_" + newWeapon.physicalDamage.ToString() + "_" + newWeapon.magicDamage.ToString();

            //EQUIP STARTER SWORD
            //newWeapon.equipped = true;
            //equippedWeapon = newWeapon;

            weaponsInventory.Add(newWeapon);
        }

        public void GenerateFirstShield()
        {
            ShieldItem newShield = ScriptableObject.CreateInstance<ShieldItem>();

            newShield.itemName = "Knight's Shield 1";

            newShield.shieldModelID = 0;
            newShield.itemIcon = shieldIcons[newShield.shieldModelID];

            newShield.itemDescription = "Each nick and scratch serves as a testament to its enduring journey, a companion that has stood steadfast in the heat of conflict, ever ready to defend its bearer.";
            newShield.itemID = 0;
            newShield.level = 5;
            newShield.sellable = false;
            newShield.rarity = 0;
            newShield.goldValue = 0;
            //weapon specific
            newShield.blockRating = 10;
            newShield.name = "ShieldItem_" + newShield.shieldModelID.ToString() + "_" + newShield.level.ToString() + "_" + newShield.blockRating.ToString();
            newShield.count = 1;
            //EQUIP STARTED SHIELD
            //newShield.equipped = true;
            //equippedShield = newShield;

            shieldsInventory.Add(newShield);
        }

        public void GenerateFirstTorsoArmor()
        {
            TorsoArmorItem newTorsoArmor = ScriptableObject.CreateInstance<TorsoArmorItem>();

            newTorsoArmor.itemName = "Knight's Armor";
            newTorsoArmor.torsoArmorModelID = 0;
            newTorsoArmor.itemIcon = torsoArmorIcons[newTorsoArmor.torsoArmorModelID];
            newTorsoArmor.itemDescription = "";
            newTorsoArmor.itemID = 0;
            newTorsoArmor.level = 1;
            newTorsoArmor.sellable = false;
            newTorsoArmor.rarity = 0;
            newTorsoArmor.goldValue = 123;
            newTorsoArmor.count = 1;

            newTorsoArmor.physicalArmorRating = 5;
            newTorsoArmor.magicalArmorRating = 5;

            newTorsoArmor.name = "TorsoArmorItem_" + newTorsoArmor.torsoArmorModelID.ToString() + "_" + newTorsoArmor.level.ToString() + "_" + newTorsoArmor.physicalArmorRating.ToString() + "_" + newTorsoArmor.magicalArmorRating.ToString();

            //EQUIP STARTER TORSO ARMOR
            newTorsoArmor.equipped = true;
            equippedTorsoArmor = newTorsoArmor;

            torsoArmorInventory.Add(newTorsoArmor);
        }

        public void GenerateFirstHandsArmor()
        {
            HandsArmorItem newHandsArmor = ScriptableObject.CreateInstance<HandsArmorItem>();

            newHandsArmor.itemName = "Knight's Gauntlets";
            newHandsArmor.handsArmorModelID = 0;
            newHandsArmor.itemIcon = handsArmorIcons[newHandsArmor.handsArmorModelID];
            newHandsArmor.itemDescription = "";
            newHandsArmor.itemID = 0;
            newHandsArmor.level = 1;
            newHandsArmor.sellable = false;
            newHandsArmor.rarity = 0;
            newHandsArmor.goldValue = 123;
            newHandsArmor.count = 1;
            newHandsArmor.physicalArmorRating = 5;
            newHandsArmor.magicalArmorRating = 5;

            newHandsArmor.name = "HandsArmorItem_" + newHandsArmor.handsArmorModelID.ToString() + "_" + newHandsArmor.level.ToString() + "_" + newHandsArmor.physicalArmorRating.ToString() + "_" + newHandsArmor.magicalArmorRating.ToString();

            //EQUIP STARTER TORSO ARMOR
            newHandsArmor.equipped = true;
            equippedHandsArmor = newHandsArmor;

            handsArmorInventory.Add(newHandsArmor);
        }

        public void GenerateFirstLegsArmor()
        {
            LegsArmorItem newLegsArmor = ScriptableObject.CreateInstance<LegsArmorItem>();

            newLegsArmor.itemName = "Knight's Greaves";
            newLegsArmor.legsArmorModelID = 0;
            newLegsArmor.itemIcon = legsArmorIcons[newLegsArmor.legsArmorModelID];
            newLegsArmor.itemDescription = "";
            newLegsArmor.itemID = 0;
            newLegsArmor.level = 1;
            newLegsArmor.sellable = false;
            newLegsArmor.rarity = 0;
            newLegsArmor.goldValue = 123;
            newLegsArmor.count = 1;
            newLegsArmor.physicalArmorRating = 5;
            newLegsArmor.magicalArmorRating = 5;

            newLegsArmor.name = "LegsArmorItem_" + newLegsArmor.legsArmorModelID.ToString() + "_" + newLegsArmor.level.ToString() + "_" + newLegsArmor.physicalArmorRating.ToString() + "_" + newLegsArmor.magicalArmorRating.ToString();

            //EQUIP STARTER TORSO ARMOR
            newLegsArmor.equipped = true;
            equippedLegsArmor = newLegsArmor;

            legsArmorInventory.Add(newLegsArmor);
        }

        //GENERATE EMPTY ARMORS:

        public void GenerateEmptyWeapon(bool equipNow = false)
        {
            WeaponItem newWeaponItem = ScriptableObject.CreateInstance<WeaponItem>();

            newWeaponItem.itemName = "EMPTY WEAPON";
            newWeaponItem.weaponModelID = -1;
            newWeaponItem.level = 1;
            newWeaponItem.sellable = false;
            newWeaponItem.rarity = 0;
            newWeaponItem.goldValue = 0;

            newWeaponItem.physicalDamage = 5;
            newWeaponItem.magicDamage = 5;

            newWeaponItem.name = "EmptyWeaponItem";

            newWeaponItem.equipped = false;
            emptyWeapon = newWeaponItem;
            if(equipNow) equippedWeapon = emptyWeapon;
        }

        public void GenerateEmptyShield(bool equipNow = false)
        {
            ShieldItem newShieldItem = ScriptableObject.CreateInstance<ShieldItem>();

            newShieldItem.itemName = "EMPTY SHIELD";
            newShieldItem.shieldModelID = -1;
            newShieldItem.level = 1;
            newShieldItem.sellable = false;
            newShieldItem.rarity = 0;
            newShieldItem.goldValue = 0;

            newShieldItem.blockRating = 1;

            newShieldItem.name = "EmptyShieldItem";

            newShieldItem.equipped = false;
            emptyShield = newShieldItem;
            if (equipNow) equippedShield = emptyShield;
        }

        public void GenerateEmptyTorsoArmor()
        {
            TorsoArmorItem newTorsoArmor = ScriptableObject.CreateInstance<TorsoArmorItem>();

            newTorsoArmor.itemName = "EMPTY TORSO ARMOR";
            newTorsoArmor.level = 1;
            newTorsoArmor.sellable = false;
            newTorsoArmor.rarity = 0;
            newTorsoArmor.goldValue = 0;

            newTorsoArmor.physicalArmorRating = 0;
            newTorsoArmor.magicalArmorRating = 0;

            newTorsoArmor.name = "EmptyTorsoArmor";

            newTorsoArmor.equipped = false;
            emptyTorsoArmor = newTorsoArmor;
        }

        public void GenerateEmptyHandsArmor()
        {
            HandsArmorItem newHandsArmor = ScriptableObject.CreateInstance<HandsArmorItem>();

            newHandsArmor.itemName = "EMPTY TORSO ARMOR";
            newHandsArmor.level = 1;
            newHandsArmor.sellable = false;
            newHandsArmor.rarity = 0;
            newHandsArmor.goldValue = 0;

            newHandsArmor.physicalArmorRating = 0;
            newHandsArmor.magicalArmorRating = 0;

            newHandsArmor.name = "EmptyHandsArmor";

            newHandsArmor.equipped = false;
            emptyHandsArmor = newHandsArmor;
        }

        public void GenerateEmptyLegsArmor()
        {
            LegsArmorItem newLegsArmor = ScriptableObject.CreateInstance<LegsArmorItem>();


            newLegsArmor.itemName = "EMPTY TORSO ARMOR";
            newLegsArmor.level = 1;
            newLegsArmor.sellable = false;
            newLegsArmor.rarity = 0;
            newLegsArmor.goldValue = 0;

            newLegsArmor.physicalArmorRating = 0;
            newLegsArmor.magicalArmorRating = 0;

            newLegsArmor.name = "EmptyLegsArmor";

            newLegsArmor.equipped = false;
            emptyLegsArmor = newLegsArmor;
        }

        public void GenerateEmptyRing()
        {
            RingItem newRing = ScriptableObject.CreateInstance<RingItem>();

            newRing.itemName = "EMPTY RING";
            newRing.level = 1;
            newRing.sellable = false;
            newRing.rarity = 0;
            newRing.goldValue = 0;

            newRing.name = "EmptyRing";

            newRing.equipped = false;
            emptyRing = newRing;
        }

        public void GenerateEmptyAmulet()
        {
            AmuletItem newAmulet = ScriptableObject.CreateInstance<AmuletItem>();

            newAmulet.itemName = "EMPTY AMULET";
            newAmulet.level = 1;
            newAmulet.sellable = false;
            newAmulet.rarity = 0;
            newAmulet.goldValue = 0;

            newAmulet.name = "EmptyAmulet";

            newAmulet.equipped = false;
            emptyAmulet = newAmulet;
        }

        public void GenerateConsumable(int consumableID, int count)
        {
            int found = -1;
            for (int i = 0; i < consumablesInventory.Count; i++)
            {
                if (consumablesInventory[i].consumable_ID == consumableID)
                {
                    found = i;
                    i = consumablesInventory.Count;
                }
            }

            if (found > -1)
            {
                consumablesInventory[found].count += count;
            }
            else
            {
                Consumable newConsumable = ScriptableObject.CreateInstance<Consumable>();
                // Note: This is primarily used in ItemGeneratorManager.cs, this is only for initial inventory
                if (consumableID == 0)
                {
                    newConsumable.itemName = "Minor Health Potion";
                    newConsumable.affectedStat = "health";
                    newConsumable.potency = 30.0f;
                    newConsumable.itemIcon = consumableIcons[consumableID];
                    newConsumable.itemDescription = "Restores " + newConsumable.potency.ToString("F0") + " Health.";
                    newConsumable.sellable = true;
                    newConsumable.rarity = 0;
                    newConsumable.goldValue = 15;
                    newConsumable.itemID = 700;
                    newConsumable.consumable_ID = consumableID;
                    newConsumable.count = count;
                    newConsumable.audioType = Consumable.AudioType.Potion;
                }
                else if (consumableID == 1)
                {
                    newConsumable.itemName = "Minor Mana Potion";
                    newConsumable.affectedStat = "mana";
                    newConsumable.potency = 30.0f;
                    newConsumable.itemIcon = consumableIcons[consumableID];
                    newConsumable.itemDescription = "Restores " + newConsumable.potency.ToString("F0") + " Mana.";
                    newConsumable.sellable = true;
                    newConsumable.rarity = 0;
                    newConsumable.goldValue = 15;
                    newConsumable.itemID = 701;
                    newConsumable.consumable_ID = consumableID;
                    newConsumable.count = count;
                    newConsumable.audioType = Consumable.AudioType.Potion;
                }
                else if (consumableID == 2)
                {
                    newConsumable.itemName = "Minor Stamina Potion";
                    newConsumable.affectedStat = "stamina";
                    newConsumable.potency = 30.0f;
                    newConsumable.itemIcon = consumableIcons[consumableID];
                    newConsumable.itemDescription = "Restores " + newConsumable.potency.ToString("F0") + " Stamina.";
                    newConsumable.sellable = true;
                    newConsumable.rarity = 0;
                    newConsumable.goldValue = 15;
                    newConsumable.itemID = 702;
                    newConsumable.consumable_ID = consumableID;
                    newConsumable.count = count;
                    newConsumable.audioType = Consumable.AudioType.Potion;
                }
                else if (consumableID == 3)
                {
                    newConsumable.itemName = "Health Potion";
                    newConsumable.affectedStat = "health";
                    newConsumable.potency = 60.0f;
                    newConsumable.itemIcon = consumableIcons[consumableID];
                    newConsumable.itemDescription = "Restores " + newConsumable.potency.ToString("F0") + " Health.";
                    newConsumable.sellable = true;
                    newConsumable.rarity = 1;
                    newConsumable.goldValue = 50;
                    newConsumable.itemID = 703;
                    newConsumable.consumable_ID = consumableID;
                    newConsumable.count = count;
                    newConsumable.audioType = Consumable.AudioType.Potion;
                }
                else if (consumableID == 4)
                {
                    newConsumable.itemName = "Mana Potion";
                    newConsumable.affectedStat = "mana";
                    newConsumable.potency = 60.0f;
                    newConsumable.itemIcon = consumableIcons[consumableID];
                    newConsumable.itemDescription = "Restores " + newConsumable.potency.ToString("F0") + " Mana.";
                    newConsumable.sellable = true;
                    newConsumable.rarity = 1;
                    newConsumable.goldValue = 45;
                    newConsumable.itemID = 704;
                    newConsumable.consumable_ID = consumableID;
                    newConsumable.count = count;
                    newConsumable.audioType = Consumable.AudioType.Potion;
                }
                else if (consumableID == 5)
                {
                    newConsumable.itemName = "Stamina Potion";
                    newConsumable.affectedStat = "stamina";
                    newConsumable.potency = 60.0f;
                    newConsumable.itemIcon = consumableIcons[consumableID];
                    newConsumable.itemDescription = "Restores " + newConsumable.potency.ToString("F0") + " Stamina.";
                    newConsumable.sellable = true;
                    newConsumable.rarity = 1;
                    newConsumable.goldValue = 40;
                    newConsumable.itemID = 705;
                    newConsumable.consumable_ID = consumableID;
                    newConsumable.count = count;
                    newConsumable.audioType = Consumable.AudioType.Potion;
                }
                else if (consumableID == 6)
                {
                    newConsumable.itemName = "Major Health Potion";
                    newConsumable.affectedStat = "health";
                    newConsumable.potency = 100.0f;
                    newConsumable.itemIcon = consumableIcons[consumableID];
                    newConsumable.itemDescription = "Restores " + newConsumable.potency.ToString("F0") + " Health.";
                    newConsumable.sellable = true;
                    newConsumable.rarity = 2;
                    newConsumable.goldValue = 120;
                    newConsumable.itemID = 706;
                    newConsumable.consumable_ID = consumableID;
                    newConsumable.count = count;
                    newConsumable.audioType = Consumable.AudioType.Potion;
                }
                else if (consumableID == 7)
                {
                    newConsumable.itemName = "Major Mana Potion";
                    newConsumable.affectedStat = "mana";
                    newConsumable.potency = 100.0f;
                    newConsumable.itemIcon = consumableIcons[consumableID];
                    newConsumable.itemDescription = "Restores " + newConsumable.potency.ToString("F0") + " Mana.";
                    newConsumable.sellable = true;
                    newConsumable.rarity = 2;
                    newConsumable.goldValue = 110;
                    newConsumable.itemID = 707;
                    newConsumable.consumable_ID = consumableID;
                    newConsumable.count = count;
                    newConsumable.audioType = Consumable.AudioType.Potion;
                }
                else if (consumableID == 8)
                {
                    newConsumable.itemName = "Major Stamina Potion";
                    newConsumable.affectedStat = "stamina";
                    newConsumable.potency = 100.0f;
                    newConsumable.itemIcon = consumableIcons[consumableID];
                    newConsumable.itemDescription = "Restores " + newConsumable.potency.ToString("F0") + " Stamina.";
                    newConsumable.sellable = true;
                    newConsumable.rarity = 2;
                    newConsumable.goldValue = 90;
                    newConsumable.itemID = 708;
                    newConsumable.consumable_ID = consumableID;
                    newConsumable.count = count;
                    newConsumable.audioType = Consumable.AudioType.Potion;
                }
                else if (consumableID == 9)
                {
                    newConsumable.itemName = "Ultimate Health Potion";
                    newConsumable.affectedStat = "health";
                    newConsumable.potency = 10000.0f;
                    newConsumable.itemIcon = consumableIcons[consumableID];
                    newConsumable.itemDescription = "Fully restores Health.";
                    newConsumable.sellable = true;
                    newConsumable.rarity = 3;
                    newConsumable.goldValue = 500;
                    newConsumable.itemID = 709;
                    newConsumable.consumable_ID = consumableID;
                    newConsumable.count = count;
                    newConsumable.audioType = Consumable.AudioType.Potion;
                }
                else if (consumableID == 10)
                {
                    newConsumable.itemName = "Ultimate Mana Potion";
                    newConsumable.affectedStat = "mana";
                    newConsumable.potency = 10000.0f;
                    newConsumable.itemIcon = consumableIcons[consumableID];
                    newConsumable.itemDescription = "Fully restores Mana.";
                    newConsumable.sellable = true;
                    newConsumable.rarity = 3;
                    newConsumable.goldValue = 480;
                    newConsumable.itemID = 710;
                    newConsumable.consumable_ID = consumableID;
                    newConsumable.count = count;
                    newConsumable.audioType = Consumable.AudioType.Potion;
                }
                else if (consumableID == 11)
                {
                    newConsumable.itemName = "Ultimate Stamina Potion";
                    newConsumable.affectedStat = "stamina";
                    newConsumable.potency = 10000.0f;
                    newConsumable.itemIcon = consumableIcons[consumableID];
                    newConsumable.itemDescription = "Fully restores stamina.";
                    newConsumable.sellable = true;
                    newConsumable.rarity = 3;
                    newConsumable.goldValue = 350;
                    newConsumable.itemID = 711;
                    newConsumable.consumable_ID = consumableID;
                    newConsumable.count = count;
                    newConsumable.audioType = Consumable.AudioType.Potion;
                }
                else if (consumableID == 12)
                {
                    newConsumable.itemName = "Pure Elixir";
                    newConsumable.affectedStat = "all";
                    newConsumable.potency = 10000.0f;
                    newConsumable.itemIcon = consumableIcons[consumableID];
                    newConsumable.itemDescription = "Fully restores health, mana and stamina.";
                    newConsumable.sellable = true;
                    newConsumable.rarity = 4;
                    newConsumable.goldValue = 700;
                    newConsumable.itemID = 712;
                    newConsumable.consumable_ID = consumableID;
                    newConsumable.count = count;
                    newConsumable.audioType = Consumable.AudioType.Potion;
                }
                // 13 - spell tome, via scriptable object, not generated

                AddToInventory(newConsumable);
            }
        }

        public void GenerateKeyItem(int keyItemID, int count)
        {
            int found = -1;
            for (int i = 0; i < keyItemsInventory.Count; i++)
            {
                if (keyItemsInventory[i].keyItem_ID == keyItemID)
                {
                    found = i;
                    i = keyItemsInventory.Count;
                }
            }

            if (found > -1)
            {
                keyItemsInventory[found].count += count;
            }
            else
            {
                KeyItem newKeyItem = ScriptableObject.CreateInstance<KeyItem>();
                // Note: This is primarily used in ItemGeneratorManager.cs, this is only for initial inventory
                if (keyItemID == 800)
                {
                    newKeyItem.itemName = "Small Key";
                    newKeyItem.itemIcon = keyItemIcons[newKeyItem.keyItemSpriteID];
                    newKeyItem.itemDescription = "A delicate key that may be useful in opening simple locks.";
                    newKeyItem.sellable = true;
                    newKeyItem.rarity = 0;
                    newKeyItem.goldValue = 15;
                    newKeyItem.itemID = 800;
                    newKeyItem.keyItem_ID = keyItemID;
                    newKeyItem.count = count;
                }
                else if (keyItemID == 1)
                {
                    newKeyItem.itemName = "ERROR";
                    newKeyItem.itemIcon = keyItemIcons[newKeyItem.keyItemSpriteID];
                    newKeyItem.itemDescription = "ERROR";
                    newKeyItem.sellable = true;
                    newKeyItem.rarity = 0;
                    newKeyItem.goldValue = 15;
                    newKeyItem.keyItem_ID = keyItemID;
                    newKeyItem.count = count;
                }

                AddToInventory(newKeyItem);
            }
        }

        public void AddToInventory(Item item, bool audio = false)
        {       
            /* If the added Item is a scriptable object assigned in the inspector from our project folder,
            we need to create a clone of the item, otherwise any changes to the item will affect the 
            scriptable object in the project folder (such as count) when the object should only be treated
            as a template for the item to be added. */
            
            switch (item)
            {
                case WeaponItem weaponItem:
                    WeaponItem weaponClone = ScriptableObject.CreateInstance<WeaponItem>();
                    weaponClone.itemName = weaponItem.itemName;
                    weaponClone.itemIcon = weaponItem.itemIcon;
                    weaponClone.itemDescription = weaponItem.itemDescription;
                    weaponClone.itemID = weaponItem.itemID;
                    weaponClone.sellable = weaponItem.sellable;
                    weaponClone.rarity = weaponItem.rarity;
                    weaponClone.goldValue = weaponItem.goldValue;
                    weaponClone.count = weaponItem.count;
                    weaponClone.weaponModelID = weaponItem.weaponModelID;
                    weaponClone.level = weaponItem.level;
                    weaponClone.equipped = weaponItem.equipped;
                    weaponClone.physicalDamage = weaponItem.physicalDamage;
                    weaponClone.magicDamage = weaponItem.magicDamage;
                    weaponClone.poise = weaponItem.poise;
                    weaponClone.baseStaminaCost = weaponItem.baseStaminaCost;
                    weaponClone.strength = weaponItem.strength;
                    weaponClone.endurance = weaponItem.endurance;
                    weaponClone.vitality = weaponItem.vitality;
                    weaponClone.precision = weaponItem.precision;
                    weaponClone.dexterity = weaponItem.dexterity;
                    weaponClone.expertise = weaponItem.expertise;
                    weaponClone.intelligence = weaponItem.intelligence;
                    weaponClone.spirit = weaponItem.spirit;
                    weaponClone.willpower = weaponItem.willpower;
                    weaponClone.luck = weaponItem.luck;
                    weaponClone.criticalChance = weaponItem.criticalChance;
                    weaponClone.criticalDamage = weaponItem.criticalDamage;
                    weaponClone.equipOnPickUp = weaponItem.equipOnPickUp;
                    weaponsInventory.Add(weaponClone);
                    HelpMenu.instance.DisplayInGame("Equipment & Spells");
                    if (weaponClone.equipOnPickUp)
                    {
                        if(equippedWeapon != null)
                        {
                            equippedWeapon.equipped = false;
                        }

                        //EQUIP NEW WEAPON
                        weaponClone.equipped = true;
                        equippedWeapon = weaponClone;
                        UpdateWeaponModel();
                        Debug.Log("Equipped: " + equippedWeapon.itemName);
                        playerStats.CalculateEffectiveStats();
                    }

                    break;

                case ShieldItem shieldItem:
                    ShieldItem shieldClone = ScriptableObject.CreateInstance<ShieldItem>();
                    shieldClone.itemName = shieldItem.itemName;
                    shieldClone.itemIcon = shieldItem.itemIcon;
                    shieldClone.itemDescription = shieldItem.itemDescription;
                    shieldClone.itemID = shieldItem.itemID;
                    shieldClone.sellable = shieldItem.sellable;
                    shieldClone.rarity = shieldItem.rarity;
                    shieldClone.goldValue = shieldItem.goldValue;
                    shieldClone.count = shieldItem.count;
                    shieldClone.shieldModelID = shieldItem.shieldModelID;
                    shieldClone.level = shieldItem.level;
                    shieldClone.equipped = shieldItem.equipped;
                    shieldClone.blockRating = shieldItem.blockRating;
                    shieldClone.strength = shieldItem.strength;
                    shieldClone.endurance = shieldItem.endurance;
                    shieldClone.vitality = shieldItem.vitality;
                    shieldClone.precision = shieldItem.precision;
                    shieldClone.dexterity = shieldItem.dexterity;
                    shieldClone.expertise = shieldItem.expertise;
                    shieldClone.intelligence = shieldItem.intelligence;
                    shieldClone.spirit = shieldItem.spirit;
                    shieldClone.willpower = shieldItem.willpower;
                    shieldClone.luck = shieldItem.luck;
                    shieldClone.fireResistance = shieldItem.fireResistance;
                    shieldClone.iceResistance = shieldItem.iceResistance;
                    shieldClone.shockResistance = shieldItem.shockResistance;
                    shieldClone.equipOnPickUp = shieldItem.equipOnPickUp;
                    shieldsInventory.Add(shieldClone);
                    HelpMenu.instance.DisplayInGame("Equipment & Spells");
                    if (shieldClone.equipOnPickUp)
                    {
                        if (equippedShield != null)
                        {
                            equippedShield.equipped = false;
                        }

                        //EQUIP NEW WEAPON
                        shieldClone.equipped = true;
                        equippedShield = shieldClone;
                        UpdateShieldModel();
                        Debug.Log("Equipped: " + equippedShield.itemName);
                        playerStats.CalculateEffectiveStats();
                    }
                    break;

                case TorsoArmorItem torsoArmorItem:
                    TorsoArmorItem torsoArmorClone = ScriptableObject.CreateInstance<TorsoArmorItem>();
                    torsoArmorClone.itemName = torsoArmorItem.itemName;
                    torsoArmorClone.itemIcon = torsoArmorItem.itemIcon;
                    torsoArmorClone.itemDescription = torsoArmorItem.itemDescription;
                    torsoArmorClone.itemID = torsoArmorItem.itemID;
                    torsoArmorClone.sellable = torsoArmorItem.sellable;
                    torsoArmorClone.rarity = torsoArmorItem.rarity;
                    torsoArmorClone.goldValue = torsoArmorItem.goldValue;
                    torsoArmorClone.count = torsoArmorItem.count;
                    torsoArmorClone.torsoArmorModelID = torsoArmorItem.torsoArmorModelID;
                    torsoArmorClone.level = torsoArmorItem.level;
                    torsoArmorClone.equipped = torsoArmorItem.equipped;
                    torsoArmorClone.physicalArmorRating = torsoArmorItem.physicalArmorRating;
                    torsoArmorClone.magicalArmorRating = torsoArmorItem.magicalArmorRating;
                    torsoArmorClone.strength = torsoArmorItem.strength;
                    torsoArmorClone.endurance = torsoArmorItem.endurance;
                    torsoArmorClone.vitality = torsoArmorItem.vitality;
                    torsoArmorClone.precision = torsoArmorItem.precision;
                    torsoArmorClone.dexterity = torsoArmorItem.dexterity;
                    torsoArmorClone.expertise = torsoArmorItem.expertise;
                    torsoArmorClone.intelligence = torsoArmorItem.intelligence;
                    torsoArmorClone.spirit = torsoArmorItem.spirit;
                    torsoArmorClone.willpower = torsoArmorItem.willpower;
                    torsoArmorClone.luck = torsoArmorItem.luck;
                    torsoArmorClone.criticalChance = torsoArmorItem.criticalChance;
                    torsoArmorClone.criticalDamage = torsoArmorItem.criticalDamage;
                    torsoArmorClone.fireResistance = torsoArmorItem.fireResistance;
                    torsoArmorClone.iceResistance = torsoArmorItem.iceResistance;
                    torsoArmorClone.shockResistance = torsoArmorItem.shockResistance;
                    torsoArmorInventory.Add(torsoArmorClone);
                    HelpMenu.instance.DisplayInGame("Equipment & Spells");
                    break;

                case HandsArmorItem handsArmorItem:
                    HandsArmorItem handsArmorClone = ScriptableObject.CreateInstance<HandsArmorItem>();
                    handsArmorClone.itemName = handsArmorItem.itemName;
                    handsArmorClone.itemIcon = handsArmorItem.itemIcon;
                    handsArmorClone.itemDescription = handsArmorItem.itemDescription;
                    handsArmorClone.itemID = handsArmorItem.itemID;
                    handsArmorClone.sellable = handsArmorItem.sellable;
                    handsArmorClone.rarity = handsArmorItem.rarity;
                    handsArmorClone.goldValue = handsArmorItem.goldValue;
                    handsArmorClone.count = handsArmorItem.count;
                    handsArmorClone.handsArmorModelID = handsArmorItem.handsArmorModelID;
                    handsArmorClone.level = handsArmorItem.level;
                    handsArmorClone.equipped = handsArmorItem.equipped;
                    handsArmorClone.physicalArmorRating = handsArmorItem.physicalArmorRating;
                    handsArmorClone.magicalArmorRating = handsArmorItem.magicalArmorRating;
                    handsArmorClone.strength = handsArmorItem.strength;
                    handsArmorClone.precision = handsArmorItem.precision;
                    handsArmorClone.intelligence = handsArmorItem.intelligence;
                    handsArmorClone.spirit = handsArmorItem.spirit;
                    handsArmorClone.luck = handsArmorItem.luck;
                    handsArmorClone.criticalChance = handsArmorItem.criticalChance;
                    handsArmorClone.criticalDamage = handsArmorItem.criticalDamage;
                    handsArmorInventory.Add(handsArmorClone);
                    HelpMenu.instance.DisplayInGame("Equipment & Spells");
                    break;

                case LegsArmorItem legsArmorItem:
                    LegsArmorItem legsArmorClone = ScriptableObject.CreateInstance<LegsArmorItem>();
                    legsArmorClone.itemName = legsArmorItem.itemName;
                    legsArmorClone.itemIcon = legsArmorItem.itemIcon;
                    legsArmorClone.itemDescription = legsArmorItem.itemDescription;
                    legsArmorClone.itemID = legsArmorItem.itemID;
                    legsArmorClone.sellable = legsArmorItem.sellable;
                    legsArmorClone.rarity = legsArmorItem.rarity;
                    legsArmorClone.goldValue = legsArmorItem.goldValue;
                    legsArmorClone.count = legsArmorItem.count;
                    legsArmorClone.legsArmorModelID = legsArmorItem.legsArmorModelID;
                    legsArmorClone.level = legsArmorItem.level;
                    legsArmorClone.equipped = legsArmorItem.equipped;
                    legsArmorClone.physicalArmorRating = legsArmorItem.physicalArmorRating;
                    legsArmorClone.magicalArmorRating = legsArmorItem.magicalArmorRating;
                    legsArmorClone.endurance = legsArmorItem.endurance;
                    legsArmorClone.vitality = legsArmorItem.vitality;
                    legsArmorClone.dexterity = legsArmorItem.dexterity;
                    legsArmorClone.expertise = legsArmorItem.expertise;
                    legsArmorClone.willpower = legsArmorItem.willpower;
                    legsArmorClone.fireResistance = legsArmorItem.fireResistance;
                    legsArmorClone.iceResistance = legsArmorItem.iceResistance;
                    legsArmorClone.shockResistance = legsArmorItem.shockResistance;
                    legsArmorInventory.Add(legsArmorClone);
                    HelpMenu.instance.DisplayInGame("Equipment & Spells");
                    break;

                case RingItem ringItem:
                    RingItem ringClone = ScriptableObject.CreateInstance<RingItem>();
                    ringClone.itemName = ringItem.itemName;
                    ringClone.itemIcon = ringItem.itemIcon;
                    ringClone.itemDescription = ringItem.itemDescription;
                    ringClone.itemID = ringItem.itemID;
                    ringClone.sellable = ringItem.sellable;
                    ringClone.rarity = ringItem.rarity;
                    ringClone.goldValue = ringItem.goldValue;
                    ringClone.count = ringItem.count;
                    ringClone.ringModelID = ringItem.ringModelID;
                    ringClone.level = ringItem.level;
                    ringClone.equipped = ringItem.equipped;
                    ringClone.legendaryStat = ringItem.legendaryStat;
                    ringClone.strength = ringItem.strength;
                    ringClone.endurance = ringItem.endurance;
                    ringClone.vitality = ringItem.vitality;
                    ringClone.precision = ringItem.precision;
                    ringClone.dexterity = ringItem.dexterity;
                    ringClone.expertise = ringItem.expertise;
                    ringClone.intelligence = ringItem.intelligence;
                    ringClone.spirit = ringItem.spirit;
                    ringClone.willpower = ringItem.willpower;
                    ringClone.luck = ringItem.luck;
                    ringClone.criticalChance = ringItem.criticalChance;
                    ringClone.criticalDamage = ringItem.criticalDamage;
                    ringClone.fireResistance = ringItem.fireResistance;
                    ringClone.iceResistance = ringItem.iceResistance;
                    ringClone.shockResistance = ringItem.shockResistance;
                    ringsInventory.Add(ringClone);
                    HelpMenu.instance.DisplayInGame("Equipment & Spells");
                    break;

                case AmuletItem amuletItem:
                    AmuletItem amuletClone = ScriptableObject.CreateInstance<AmuletItem>();
                    amuletClone.itemName = amuletItem.itemName;
                    amuletClone.itemIcon = amuletItem.itemIcon;
                    amuletClone.itemDescription = amuletItem.itemDescription;
                    amuletClone.itemID = amuletItem.itemID;
                    amuletClone.sellable = amuletItem.sellable;
                    amuletClone.rarity = amuletItem.rarity;
                    amuletClone.goldValue = amuletItem.goldValue;
                    amuletClone.count = amuletItem.count;
                    amuletClone.amuletModelID = amuletItem.amuletModelID;
                    amuletClone.level = amuletItem.level;
                    amuletClone.equipped = amuletItem.equipped;
                    amuletClone.legendaryStat = amuletItem.legendaryStat;
                    amuletClone.criticalChance = amuletItem.criticalChance;
                    amuletClone.criticalDamage = amuletItem.criticalDamage;
                    amuletClone.fireResistance = amuletItem.fireResistance;
                    amuletClone.iceResistance = amuletItem.iceResistance;
                    amuletClone.shockResistance = amuletItem.shockResistance;
                    amuletsInventory.Add(amuletClone);
                    HelpMenu.instance.DisplayInGame("Equipment & Spells");
                    break;

                case Spell spellItem:
                    Spell spellClone = ScriptableObject.CreateInstance<Spell>();
                    spellClone.itemName = spellItem.itemName;
                    spellClone.itemIcon = spellItem.itemIcon;
                    spellClone.itemDescription = spellItem.itemDescription;
                    spellClone.itemID = spellItem.itemID;
                    spellClone.sellable = spellItem.sellable;
                    spellClone.rarity = spellItem.rarity;
                    spellClone.goldValue = spellItem.goldValue;
                    spellClone.count = spellItem.count;
                    spellClone.equipped = spellItem.equipped;
                    spellClone.spell_ID = spellItem.spell_ID;
                    spellClone.spellType = spellItem.spellType;
                    spellClone.elementType = spellItem.elementType;
                    spellClone.baseDamage = spellItem.baseDamage;
                    spellClone.duration = spellItem.duration;
                    spellClone.castSpeed = spellItem.castSpeed;
                    spellClone.manaCost = spellItem.manaCost;
                    spellClone.splashSize = spellItem.splashSize;
                    spellsInventory.Add(spellClone);
                    HelpMenu.instance.DisplayInGame("Equipment & Spells");
                    break;

                case Consumable consumableItem:
                    CheckAndAddConsumable(consumableItem);
                    HelpMenu.instance.DisplayInGame("Items & Consumables");
                    break;

                case KeyItem keyItem:
                    CheckAndAddKeyItem(keyItem);
                    HelpMenu.instance.DisplayInGame("Items & Consumables");
                    break;

                default:
                    Debug.Log("Add to Inventory Type Error");
                    break;
            }

            if (audio)
            {
                itemsAudioSource.PlayOneShot(itemGrab, itemGrabVolume);
                StartCoroutine(DelayedItemSound(item.rarity));
            }

            questManager.CheckQuestStepsForItem(item);      
        }

        private IEnumerator DelayedItemSound(int rarity=0)
        {
            yield return new WaitForSecondsRealtime(0.15f);
            itemsAudioSource.PlayOneShot(itemSounds[rarity], itemSoundsVolume);
        }

        private void CheckAndAddConsumable(Consumable consumableItem)
        {
            int found = -1;
            for (int i = 0; i < consumablesInventory.Count; i++)
            {
                if (consumablesInventory[i].consumable_ID == consumableItem.consumable_ID)
                {
                    found = i;
                    i = consumablesInventory.Count;
                }
            }

            if (found > -1)
            {
                consumablesInventory[found].count += consumableItem.count;
                ConsumablesHUDManager.instance.UpdateConsumablesHUD();
            }
            else
            {
                // In order to make sure we are only using the scriptable object as a template
                // and not using the item itself in our inventory (because this ruins count)
                // we need to essentially create a close of the item then add it to our inventory
                Consumable clone = ScriptableObject.CreateInstance<Consumable>();
                clone.itemName =  consumableItem.itemName;
                clone.itemIcon =  consumableItem.itemIcon;
                clone.itemDescription =  consumableItem.itemDescription;
                clone.itemID =  consumableItem.itemID;
                clone.sellable =  consumableItem.sellable;
                clone.rarity =  consumableItem.rarity;
                clone.goldValue =  consumableItem.goldValue;
                clone.count =  consumableItem.count;
                clone.capacity = consumableItem.capacity;
                clone.consumable_ID = consumableItem.consumable_ID;
                clone.equipped = consumableItem.equipped;
                clone.keepInInventoryAtZeroCount = consumableItem.keepInInventoryAtZeroCount;
                clone.useDefaultAnimation = consumableItem.useDefaultAnimation;
                clone.showCountInConsumableHUD = consumableItem.showCountInConsumableHUD;
                // Values such as potency are actually set in Player Inventory, where the potions are generated
                clone.potency = consumableItem.potency;
                clone.affectedStat = consumableItem.affectedStat;
                clone.audioType = consumableItem.audioType;

                consumablesInventory.Add(clone);
            }
        }

        private void CheckAndAddKeyItem(KeyItem keyItem)
        {
            int found = -1;
            for (int i = 0; i < keyItemsInventory.Count; i++)
            {
                if (keyItemsInventory[i].keyItem_ID == keyItem.keyItem_ID)
                {
                    found = i;
                    i = keyItemsInventory.Count; // stops loop
                }
            }

            // If found, key item is already in list, update count
            if (found > -1)
            {
                keyItemsInventory[found].count += keyItem.count;
            }
            else
            {
                // In order to make sure we are only using the scriptable object as a template
                // and not using the item itself in our inventory (because this ruins count)
                // we need to essentially create a close of the item then add it to our inventory
                KeyItem clone = ScriptableObject.CreateInstance<KeyItem>();
                clone.itemName = keyItem.itemName;
                clone.itemIcon = keyItem.itemIcon;
                clone.itemDescription = keyItem.itemDescription;
                clone.itemID = keyItem.itemID;
                clone.sellable = keyItem.sellable;
                clone.rarity = keyItem.rarity;
                clone.goldValue = keyItem.goldValue;
                clone.count = keyItem.count;
                clone.keyItem_ID = keyItem.keyItem_ID;
                clone.keyItemSpriteID = keyItem.keyItemSpriteID;
                clone.lostWithUse = keyItem.lostWithUse;
                clone.note = keyItem.note;
                keyItemsInventory.Add(clone);
            }

            // If the key item is a specific item, do something
            Debug.Log("Checking Key Item ID for event. ID is: " + keyItem.keyItem_ID);
            switch (keyItem.keyItem_ID)
            {
                case 902: // Horse Ledger
                    //Debug.Log("Horse Ledger obtained. Mount unlocked.");
                    MountManager.instance.mountUnlocked = true;                   
                    break;
                default:
                    //Debug.Log("No event for key item found.");
                    break;
            }

            // Also update keys HUD(relevant when grabbing key from a chest for example)
            KeyItem smallKey = keyItemsInventory.FirstOrDefault(keyItem => keyItem.keyItem_ID == 0);
            if (smallKey != null)
            {
                keyCountText.text = " x " + smallKey.count.ToString();
            }
        }

        public void RemoveFromInventory(Item item, int removeQuantity)
        {
            switch (item)
            {
                case WeaponItem weaponItem:
                    weaponsInventory.Remove(weaponItem);
                    break;
                case ShieldItem shieldItem:
                    shieldsInventory.Remove(shieldItem);
                    break;
                case TorsoArmorItem torsoArmorItem:
                    torsoArmorInventory.Remove(torsoArmorItem);
                    break;
                case HandsArmorItem handsArmorItem:
                    handsArmorInventory.Remove(handsArmorItem);
                    break;
                case LegsArmorItem legsArmorItem:
                    legsArmorInventory.Remove(legsArmorItem);
                    break;
                case RingItem ringItem:
                    ringsInventory.Remove(ringItem);
                    break;
                case AmuletItem amuletItem:
                    amuletsInventory.Remove(amuletItem);
                    break;
                case Spell spellItem:
                    spellsInventory.Remove(spellItem);
                    break;
                case Consumable consumableItem:
                    if(removeQuantity >= consumableItem.count)
                    {
                        if (consumableItem.keepInInventoryAtZeroCount)
                            consumableItem.count = 0;
                        else consumablesInventory.Remove(consumableItem);
                    }
                    else
                    {
                        consumableItem.count -= removeQuantity;
                    }
                    ConsumablesHUDManager.instance.UpdateConsumablesHUD();
                    break;
                case KeyItem keyItem:
                    if (removeQuantity >= keyItem.count)
                    {
                        keyItemsInventory.Remove(keyItem);
                    }
                    else
                    {
                       keyItem.count -= removeQuantity;
                    }
                    break;
                default:
                    Debug.Log("Add to Inventory Type Error");
                    break;
            }
        }

        // SPELLS:
        public void AddSpellFromSkillTree(int skillID)
        {
            bool alreadyInInventory;
            int spellID;
            int skillRank;          

            switch (skillID)
            {
                case 15:
                    spellID = 0;
                    alreadyInInventory = SpellIsInInventory(spellID);
                    if(!alreadyInInventory) GenerateSpell_MinorArcaneBolt();
                    break;
                case 16:
                    spellID = 1;
                    alreadyInInventory = SpellIsInInventory(spellID);
                    if (!alreadyInInventory) GenerateSpell_Mend();
                    break;
                case 17:
                    spellID = 2;
                    alreadyInInventory = SpellIsInInventory(spellID);
                    if (!alreadyInInventory) GenerateSpell_Fireball();
                    break;
                case 18:
                    spellID = 3;
                    alreadyInInventory = SpellIsInInventory(spellID);
                    if (!alreadyInInventory) GenerateSpell_Frostshard();
                    break;
                case 19:
                    spellID = 4;
                    alreadyInInventory = SpellIsInInventory(spellID);
                    if (!alreadyInInventory) GenerateSpell_Zapshock();
                    break;
                case 20:
                    spellID = 5;
                    alreadyInInventory = SpellIsInInventory(spellID);
                    if (!alreadyInInventory) GenerateSpell_DarkArcaneBolt();
                    else
                    {
                        // Optional: Spell already in inventory, upgrade spell desciption based on new rank
                        // Some spells do not require or want this
                        UpdateSpellDescription(spellID, skillID);

                        foreach (Spell spell in spellsInventory) // Find spell
                        {
                            if (spell.spell_ID == spellID)
                            {
                                skillRank = PlayerStats.instance.GetSkillRankByID(skillID);
                                switch (skillRank)
                                {
                                    case 1:
                                        spell.baseDamage = 20.0f;
                                        spell.manaCost = 18.0f;
                                        break;
                                    case 2:
                                        spell.baseDamage = 22.0f;
                                        spell.manaCost = 36.0f;
                                        break;
                                    case 3:
                                        spell.baseDamage = 24.0f;
                                        spell.manaCost = 54.0f;
                                        break;
                                    default:
                                        break;
                                }
                            }
                        }
                    }
                    break;
                case 21:
                    spellID = 6;
                    alreadyInInventory = SpellIsInInventory(spellID);
                    if (!alreadyInInventory) GenerateSpell_Rejuvinate();
                    else
                    {
                        // Optional: Spell already in inventory, upgrade spell desciption based on new rank
                        // Some spells do not require or want this
                        foreach (Spell spell in spellsInventory) // Find spell
                        {
                            if (spell.spell_ID == spellID)
                            {
                                skillRank = PlayerStats.instance.GetSkillRankByID(skillID);
                                switch (skillRank)
                                {
                                    case 1:
                                        spell.baseDamage = 2.0f;
                                        spell.manaCost = 15.0f;
                                        spell.itemDescription = "Mana Cost: 15, Base Power: 2\n Rejuvenate, a spell attuned to the nurturing spirit of Mother Nature, gently renews, with the patience and tranqulity of the forest.";
                                        break;
                                    case 2:
                                        spell.duration = 45f;
                                        break;
                                    case 4:
                                        spell.baseDamage = 3.0f;
                                        spell.manaCost = 25.0f;
                                        spell.itemDescription = "Mana Cost: 25, Base Power: 3\n Rejuvenate, a spell attuned to the nurturing spirit of Mother Nature, gently renews, with the patience and tranqulity of the forest.";
                                        break;
                                    default:
                                        break;
                                }
                            }
                        }

                    }
                    break;
                case 22:
                    spellID = 7;
                    alreadyInInventory = SpellIsInInventory(spellID);
                    if (!alreadyInInventory) GenerateSpell_Meteor();
                    else
                    {
                        // Optional: Spell already in inventory, upgrade spell desciption based on new rank
                        // Some spells do not require or want this
                        foreach (Spell spell in spellsInventory) // Find spell
                        {
                            if (spell.spell_ID == spellID)
                            {
                                skillRank = PlayerStats.instance.GetSkillRankByID(skillID);
                                switch (skillRank)
                                {
                                    case 1:
                                        spell.baseDamage = 50.0f;
                                        spell.manaCost = 45.0f;
                                        spell.itemDescription = "Meteor, a devastating spell, summons a celestial body to crash down dealing great damage to those caught in its fiery wake.";
                                        break;
                                    case 2:
                                        spell.baseDamage = 70.0f;
                                        spell.manaCost = 60.0f;
                                        spell.itemDescription = "Meteor, a devastating spell, summons a celestial body to crash down dealing great damage to those caught in its fiery wake.";
                                        break;
                                    case 3:
                                        spell.baseDamage = 90.0f;
                                        spell.manaCost = 75.0f;
                                        spell.itemDescription = "Meteor, a devastating spell, summons a celestial body to crash down dealing great damage to those caught in its fiery wake.";
                                        break;
                                    default:
                                        break;
                                }
                            }
                        }
                    }
                    break;
                case 23:
                    spellID = 8;
                    alreadyInInventory = SpellIsInInventory(spellID);
                    if (!alreadyInInventory) GenerateSpell_GlacialBurst();
                    break;
                case 24:
                    spellID = 9;
                    alreadyInInventory = SpellIsInInventory(spellID);
                    if (!alreadyInInventory) GenerateSpell_Stormguard();
                    else 
                    {
                        // Optional: Spell already in inventory, upgrade spell desciption based on new rank
                        // Some spells do not require or want this
                        UpdateSpellDescription(spellID, skillID);

                        foreach (Spell spell in spellsInventory) // Find spell
                        {
                            if (spell.spell_ID == spellID)
                            {
                                skillRank = PlayerStats.instance.GetSkillRankByID(skillID);
                                switch (skillRank)
                                {
                                    case 1:
                                        spell.baseDamage = 15.0f;
                                        spell.manaCost = 25.0f;
                                        spell.duration = 20.0f;
                                        break;
                                    case 2:
                                        spell.baseDamage = 20.0f;
                                        spell.manaCost = 30.0f;
                                        spell.duration = 30.0f;
                                        break;
                                    case 3:
                                        spell.baseDamage = 25.0f;
                                        spell.manaCost = 35.0f;
                                        spell.duration = 40.0f;
                                        break;
                                    default:
                                        break;
                                }
                            }
                        }
                        
                    }
                    break;
                case 25:
                    spellID = 11;
                    alreadyInInventory = SpellIsInInventory(spellID);
                    if (!alreadyInInventory) GenerateSpell_VampiricBolt();
                    else
                    {
                        // Optional: Spell already in inventory, upgrade spell desciption based on new rank
                        // Some spells do not require or want this
                        UpdateSpellDescription(spellID, skillID);

                        foreach (Spell spell in spellsInventory) // Find spell
                        {
                            if (spell.spell_ID == spellID)
                            {
                                skillRank = PlayerStats.instance.GetSkillRankByID(skillID);
                                switch (skillRank)
                                {
                                    case 1:
                                        spell.manaCost = 30.0f;
                                        spell.duration = 25.0f;
                                        break;
                                    case 2:
                                        spell.manaCost = 35.0f;
                                        spell.duration = 27.0f;
                                        break;
                                    case 3:
                                        spell.manaCost = 40.0f;
                                        spell.duration = 29.0f;
                                        break;
                                    case 4:
                                        spell.manaCost = 45.0f;
                                        spell.duration = 32.0f;
                                        break;
                                    case 5:
                                        spell.manaCost = 50.0f;
                                        spell.duration = 35.0f;
                                        break;
                                    default:
                                        break;
                                }
                            }
                        }

                    }
                    break;
                case 29:
                    spellID = 10;
                    alreadyInInventory = SpellIsInInventory(spellID);
                    if (!alreadyInInventory) GenerateSpell_MysticInfusion();
                    else
                    {
                        // Optional: Spell already in inventory, upgrade spell desciption based on new rank
                        // Some spells do not require or want this
                        UpdateSpellDescription(spellID, skillID);

                        foreach (Spell spell in spellsInventory) // Find spell
                        {
                            if (spell.spell_ID == spellID)
                            {
                                skillRank = PlayerStats.instance.GetSkillRankByID(skillID);
                                switch (skillRank)
                                {
                                    case 1:
                                        spell.itemDescription = "For <b>20</b> seconds, the caster's sword is infused with fire, ice or shock. During this time, all damage dealt is converted into the corresponding elemental type. However, damage dealt is still based on the weapon's Physical Damage Rating.";
                                        spell.manaCost = 50.0f;
                                        spell.duration = 20.0f;
                                        break;
                                    case 2:
                                        spell.itemDescription = "For <b>30</b> seconds, the caster's sword is infused with fire, ice or shock. During this time, all damage dealt is converted into the corresponding elemental type. However, damage dealt is still based on the weapon's Physical Damage Rating.";
                                        spell.manaCost = 55.0f;
                                        spell.duration = 30.0f;
                                        break;
                                    case 3:
                                        spell.itemDescription = "For <b>40</b> seconds, the caster's sword is infused with fire, ice or shock. During this time, all damage dealt is converted into the corresponding elemental type. However, damage dealt is still based on the weapon's Physical Damage Rating.";
                                        spell.manaCost = 60.0f;
                                        spell.duration = 40.0f;
                                        break;
                                    case 4:
                                        spell.itemDescription = "For <b>50</b> seconds, the caster's sword is infused with fire, ice or shock. During this time, all damage dealt is converted into the corresponding elemental type. However, damage dealt is still based on the weapon's Physical Damage Rating.";
                                        spell.manaCost = 65.0f;
                                        spell.duration = 50.0f;
                                        break;
                                    case 5:
                                        spell.itemDescription = "For <b>60</b> seconds, the caster's sword is infused with fire, ice or shock. During this time, all damage dealt is converted into the corresponding elemental type. However, damage dealt is still based on the weapon's Physical Damage Rating.";
                                        spell.manaCost = 70.0f;
                                        spell.duration = 60.0f;
                                        break;
                                    default:
                                        break;
                                }
                            }
                        }

                    }
                    break;
                default:
                    break;
            }
        }

        private bool SpellIsInInventory(int spellID)
        {
            HelpMenu.instance.DisplayInGame("Equipment & Spells");
            foreach (Spell spell in spellsInventory)
            {
                if (spell.spell_ID == spellID) return true;
            }
            return false;
        }

        private void UpdateSpellDescription(int spellID, int skillID)
        {
            foreach (Spell spell in spellsInventory) // Find spell
            {
                if (spell.spell_ID == spellID)
                {
                    string currentDescription = PlayerStats.instance.GetSkillDescriptionByID(skillID);
                    string trimmedString;

                    int index = currentDescription.IndexOf('\n');
                    if (index >= 0)
                    {
                        // We trim off anything before \n such as "Mana Cost: 25, Base Power: 10 \n {actual desciption}"
                        trimmedString = currentDescription.Substring(index + 1);
                    }
                    else trimmedString = currentDescription;

                    spell.itemDescription = trimmedString;
                    return;
                }
            }
        }

        public void GenerateSpell_MinorArcaneBolt()
        {
            Spell newSpell = ScriptableObject.CreateInstance<Spell>();

            newSpell.itemName = "Minor Arcane Bolt";

            newSpell.spell_ID = 0; //chooses projectile from AnimEvents.cs
            newSpell.itemIcon = spellIcons[newSpell.spell_ID];

            //NOTE: use <= 230 characters
            newSpell.itemDescription = " The Minor Arcane Bolt, taught to apprentice spellcasters, conjures a shimmering bolt of arcane energy, inflicting minor damage on impact. It's a practice spell for novice casters to hone their abilities.";

            newSpell.sellable = false;
            newSpell.rarity = 1;
            newSpell.goldValue = 0;

            newSpell.count = 1;

            newSpell.spellType = "Projectile";

            newSpell.elementType = "Arcane";

            newSpell.baseDamage = 8.0f;

            newSpell.manaCost = 5.0f;

            newSpell.name = "Spell_" + newSpell.spell_ID.ToString();

            spellsInventory.Add(newSpell);
        }

        public void GenerateSpell_Mend()
        {
            Spell newSpell = ScriptableObject.CreateInstance<Spell>();

            newSpell.itemName = "Mend";

            newSpell.spell_ID = 1; //chooses effect from AnimEvents.cs
            newSpell.itemIcon = spellIcons[newSpell.spell_ID];

            newSpell.itemDescription = "Mend, a soothing enchantment that harnesses the flow of life energy to mend injuries and revitalaze the weary.";

            newSpell.sellable = false;
            newSpell.rarity = 2;
            newSpell.goldValue = 0;

            newSpell.count = 1;

            newSpell.spellType = "Self";

            newSpell.elementType = "Holy";

            newSpell.baseDamage = 20.0f;

            newSpell.manaCost = 20.0f;

            newSpell.name = "Spell_" + newSpell.spell_ID.ToString();

            spellsInventory.Add(newSpell);
            //equippedSpells[1] = newSpell;
        }

        public void GenerateSpell_Fireball()
        {
            Spell newSpell = ScriptableObject.CreateInstance<Spell>();

            newSpell.itemName = "Fireball";

            newSpell.spell_ID = 2; //chooses projectile from AnimEvents.cs
            newSpell.itemIcon = spellIcons[newSpell.spell_ID];

            newSpell.itemDescription = " Fireball, a novice's spell, the  creates a blazing sphere that inflicts moderate damage, providing a crucial step in a mage's early journey to mastery.";

            newSpell.sellable = false;
            newSpell.rarity = 2;
            newSpell.goldValue = 0;

            newSpell.count = 1;

            newSpell.spellType = "Projectile";

            newSpell.elementType = "Fire";

            newSpell.baseDamage = 10.0f;

            newSpell.manaCost = 15.0f;

            newSpell.name = "Spell_" + newSpell.spell_ID.ToString();

            spellsInventory.Add(newSpell);
            //equippedSpells[1] = newSpell;
        }

        public void GenerateSpell_Frostshard()
        {
            Spell newSpell = ScriptableObject.CreateInstance<Spell>();

            newSpell.itemName = "Frostshard";

            newSpell.spell_ID = 3; //chooses projectile from AnimEvents.cs
            newSpell.itemIcon = spellIcons[newSpell.spell_ID];

            newSpell.itemDescription = "Frostshard, an adept's swift mastery, evokes a jagged ice fragment, piercing the target and chilling them to the bone.";

            newSpell.sellable = false;
            newSpell.rarity = 2;
            newSpell.goldValue = 0;

            newSpell.count = 1;

            newSpell.spellType = "Projectile";

            newSpell.elementType = "Ice";

            newSpell.baseDamage = 8.0f;

            newSpell.manaCost = 12.0f;

            newSpell.name = "Spell_" + newSpell.spell_ID.ToString();

            spellsInventory.Add(newSpell);
            //equippedSpells[1] = newSpell;
        }

        public void GenerateSpell_Zapshock()
        {
            Spell newSpell = ScriptableObject.CreateInstance<Spell>();

            newSpell.itemName = "Zapshock";

            newSpell.spell_ID = 4; //chooses projectile from AnimEvents.cs
            newSpell.itemIcon = spellIcons[newSpell.spell_ID];

            newSpell.itemDescription = " Zapshock, an apprentice's incantation, conjures a crackling surge of electricity that delivers substantial shock damage, marking a pivotal advancement on a mage's path towards expertise.";


            newSpell.sellable = false;
            newSpell.rarity = 2;
            newSpell.goldValue = 0;

            newSpell.count = 1;

            newSpell.spellType = "Projectile";

            newSpell.elementType = "Shock";

            newSpell.baseDamage = 12.0f;

            newSpell.manaCost = 15.0f;

            newSpell.name = "Spell_" + newSpell.spell_ID.ToString();

            spellsInventory.Add(newSpell);
            //equippedSpells[1] = newSpell;
        }

        public void GenerateSpell_DarkArcaneBolt()
        {
            Spell newSpell = ScriptableObject.CreateInstance<Spell>();

            newSpell.itemName = "Dark Arcane Bolt";

            newSpell.spell_ID = 5; //chooses projectile from AnimEvents.cs
            newSpell.itemIcon = spellIcons[newSpell.spell_ID];

            newSpell.itemDescription = " An adept variant of Arcane Bolt that has been corrupted by the emerging dark influences within its caster. If the user has at least <b>20%</b> of their maximum health, this health will be drained and this spell's damage will be increased by <b>30%</b> in exchange.";

            newSpell.sellable = false;
            newSpell.rarity = 3;
            newSpell.goldValue = 0;

            newSpell.count = 1;

            newSpell.spellType = "Projectile";

            newSpell.elementType = "Arcane";

            newSpell.baseDamage = 20.0f;

            newSpell.manaCost = 18.0f;

            newSpell.name = "Spell_" + newSpell.spell_ID.ToString();

            spellsInventory.Add(newSpell);
            //equippedSpells[1] = newSpell;
        }

        public void GenerateSpell_Rejuvinate()
        {
            Spell newSpell = ScriptableObject.CreateInstance<Spell>();

            newSpell.itemName = "Rejuvenate";

            newSpell.spell_ID = 6; //chooses effect from AnimEvents.cs
            newSpell.itemIcon = spellIcons[newSpell.spell_ID];

            newSpell.itemDescription = "Rejuvenate, a spell attuned to the nurturing spirit of Mother Nature, gently renews, with the patience and tranqulity of the forest.";

            newSpell.sellable = false;
            newSpell.rarity = 3;
            newSpell.goldValue = 0;

            newSpell.count = 1;

            newSpell.spellType = "Self";

            newSpell.elementType = "Nature";

            newSpell.baseDamage = 2.0f;

            newSpell.duration = 30.0f;

            newSpell.manaCost = 15.0f;

            newSpell.name = "Spell_" + newSpell.spell_ID.ToString();

            spellsInventory.Add(newSpell);
            //equippedSpells[1] = newSpell;
        }

        public void GenerateSpell_Meteor()
        {
            Spell newSpell = ScriptableObject.CreateInstance<Spell>();

            newSpell.itemName = "Meteor";

            newSpell.spell_ID = 7; //chooses effect from AnimEvents.cs
            newSpell.itemIcon = spellIcons[newSpell.spell_ID];

            newSpell.itemDescription = "Meteor, a devastating spell, summons a celestial body to crash down dealing great damage to those caught in its fiery wake.";

            newSpell.sellable = false;
            newSpell.rarity = 3;
            newSpell.goldValue = 0;

            newSpell.count = 1;

            newSpell.spellType = "AOE";

            newSpell.elementType = "Fire";

            newSpell.baseDamage = 50.0f;

            newSpell.duration = 1.0f;

            newSpell.castSpeed = "verySlow";

            newSpell.manaCost = 45.0f;

            newSpell.name = "Spell_" + newSpell.spell_ID.ToString();

            spellsInventory.Add(newSpell);
            //equippedSpells[2] = newSpell;
        }

        public void GenerateSpell_GlacialBurst()
        {
            Spell newSpell = ScriptableObject.CreateInstance<Spell>();

            newSpell.itemName = "Glacial Burst";

            newSpell.spell_ID = 8; //chooses effect from AnimEvents.cs
            newSpell.itemIcon = spellIcons[newSpell.spell_ID];

            newSpell.itemDescription = "Glacial Burst summons multiple icy crystals, each charging up before unleashing a frosty explosion upon nearby foes.";

            newSpell.sellable = false;
            newSpell.rarity = 3;
            newSpell.goldValue = 0;

            newSpell.count = 1;

            newSpell.spellType = "AOE";

            newSpell.elementType = "Ice";

            newSpell.baseDamage = 25f;

            newSpell.duration = 1.0f;

            newSpell.castSpeed = "slow";

            newSpell.manaCost = 35.0f;

            newSpell.name = "Spell_" + newSpell.spell_ID.ToString();

            spellsInventory.Add(newSpell);
            //equippedSpells[3] = newSpell;
        }

        public void GenerateSpell_Stormguard()
        {
            Spell newSpell = ScriptableObject.CreateInstance<Spell>();

            newSpell.itemName = "Stormguard";

            newSpell.spell_ID = 9; //chooses effect from AnimEvents.cs
            newSpell.itemIcon = spellIcons[newSpell.spell_ID];

            newSpell.itemDescription = "For <b>20</b> seconds, Stormguard cloaks the caster in electric energy that reduces incoming non-magic damage by <b>15%</b> while electrifying melee attackers.";

            newSpell.sellable = false;
            newSpell.rarity = 3;
            newSpell.goldValue = 0;

            newSpell.count = 1;

            newSpell.spellType = "Self";

            newSpell.elementType = "Shock";

            newSpell.baseDamage = 15f;

            newSpell.duration = 20.0f;

            newSpell.castSpeed = "veryFast";

            newSpell.manaCost = 25.0f;

            newSpell.name = "Spell_" + newSpell.spell_ID.ToString();

            spellsInventory.Add(newSpell);
            //equippedSpells[1] = newSpell;
        }

        public void GenerateSpell_MysticInfusion()
        {
            Spell newSpell = ScriptableObject.CreateInstance<Spell>();

            newSpell.itemName = "Mystic Infusion";

            newSpell.spell_ID = 10; //chooses effect from AnimEvents.cs
            newSpell.itemIcon = spellIcons[newSpell.spell_ID];

            newSpell.itemDescription = "For <b>20</b> seconds, the caster's sword is infused with fire, ice or shock. During this time, all damage dealt is converted into the corresponding elemental type. However, damage dealt is still based on the weapon's Physical Damage Rating.";

            newSpell.sellable = false;
            newSpell.rarity = 4;
            newSpell.goldValue = 0;

            newSpell.count = 1;

            newSpell.spellType = "AOE";

            newSpell.elementType = "Fire/Ice/Shock";

            newSpell.baseDamage = 10f;

            newSpell.duration = 20.0f;

            newSpell.castSpeed = "fast";

            newSpell.manaCost = 50.0f;

            newSpell.name = "Spell_" + newSpell.spell_ID.ToString();

            spellsInventory.Add(newSpell);
            //equippedSpells[1] = newSpell;
        }

        public void GenerateSpell_VampiricBolt()
        {
            Spell newSpell = ScriptableObject.CreateInstance<Spell>();

            newSpell.itemName = "Vampiric Bolt";

            newSpell.spell_ID = 11; //chooses projectile from AnimEvents.cs
            newSpell.itemIcon = spellIcons[newSpell.spell_ID];

            newSpell.itemDescription = "An incantation of pure corruption and malice that distorts the healing nature of rejuvenative magic, siphoning life force from its target for <b>5%</b> of the damage it dealt.";

            newSpell.sellable = false;
            newSpell.rarity = 4;
            newSpell.goldValue = 0;

            newSpell.count = 1;

            newSpell.spellType = "Projectile";

            newSpell.elementType = "Arcane";

            newSpell.baseDamage = 30.0f;

            newSpell.manaCost = 25.0f;

            newSpell.name = "Spell_" + newSpell.spell_ID.ToString();

            spellsInventory.Add(newSpell);
        }

        public void GenerateSpell_ArcanaLumina()
        {
            // Make sure spell is not already learned
            foreach(Spell spell in spellsInventory)
            {
                if(spell.spell_ID == 12)
                {
                    Debug.LogError("Spell Already Known, not adding to Inventory");
                    return;
                }
            }

            Spell newSpell = ScriptableObject.CreateInstance<Spell>();

            newSpell.itemName = "Arcana Lumina";

            newSpell.spell_ID = 12; //chooses projectile from AnimEvents.cs
            newSpell.itemIcon = spellIcons[newSpell.spell_ID];

            //NOTE: use <= 230 characters
            newSpell.itemDescription = "A classic spell that conjures a radiant orb of light, illuminating dark spaces with a steady, gentle glow. Commonly used by travelers to navigate shadowy realms.";

            newSpell.sellable = false;
            newSpell.rarity = 3;
            newSpell.goldValue = 0;

            newSpell.count = 1;

            newSpell.spellType = "Self";

            newSpell.elementType = "Arcane";

            newSpell.baseDamage = 8.0f;

            newSpell.duration = 30.0f;

            newSpell.manaCost = 5.0f;

            newSpell.name = "Spell_" + newSpell.spell_ID.ToString();

            spellsInventory.Add(newSpell);
        }

        public T GetItem<T>(List<T> inventory, int id) where T : Item
        {
            return inventory.FirstOrDefault(item => item.itemID == id);
        }

        public int GetItemCount<T>(List<T> inventory, int id) where T : Item
        {
            Item item = GetItem(inventory, id);
            if (item == null) return 0;
            else return item.count;
        }

    }
}
