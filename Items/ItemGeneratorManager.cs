using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace etchebarren
{
    public class ItemGeneratorManager : MonoBehaviour
    {
        public static ItemGeneratorManager instance;

        public PlayerInventory playerInventory;

        [Header("Universal Item Pool")]
        public ItemDropElement[] itemPool;

        [Header("Rarity Rates")]
        public int[] rarityChances = { 30, 25, 20, 15, 10 };
        public int[] consumableRarityChances = { 40, 30, 25, 10, 5 };

        [Header("Scale to mutliple gold drops per enemy level")]
        public float goldScale = 0.5f;
        public float[] goldMultiplier = { 1.0f, 1.15f, 1.3f, 1.45f, 1.6f };

        [Header("Equipment Base Drop Rates")]
        public float swordDropRate;
        public float shieldDropRate;
        public float torsoDropRate;
        public float handsDropRate;
        public float legsDropRate;
        public float ringDropRate;
        public float amuletDropRate;
        [SerializeField] float totalDrop = 0;
        [SerializeField] float nothingDropRate = 0;
        private float gearStatMultiplier = 10f;

        [Header("Consumable Base Drop Rates")]
        public float consumableDropRate;

        [Header("Words for Item Names")]
        public string[] swordNames;
        public string[] shieldNames;
        public string[] torsoArmorNames;
        public string[] handsArmorNames;
        public string[] legsArmorNames;
        public string[] ringNames;
        public string[] amuletNames;
        public string[] prefixes;
        public string[] suffixes;

        [Header("ItemDrop Prefab")]
        public GameObject itemDropPrefab;

        [Header("Coin References")]
        public Transform coinTarget;
        public GameObject coinPrefab;

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

        void Start()
        {
            totalDrop = swordDropRate + shieldDropRate + torsoDropRate + handsDropRate + legsDropRate + ringDropRate + amuletDropRate;
            nothingDropRate = 100 - totalDrop;

            //Tests
            //GenerateSword(10, 10, Vector3.one);
            //GenerateSword(15, 15, Vector3.one);
            //GenerateSword(20, 20, Vector3.one);
            //GenerateSword(25, 25, Vector3.one);
            //GenerateSword(30, 30, Vector3.one);
        }

        public void Test()
        {
            GenerateRandomItem(10, Vector3.one);
        }

        public void GenerateRandomItem(int enemyLevel, Vector3 position, string rarity = "random", int minimumRarity = 0, bool fromChest=false)
        {
            if (rarity == "none") return;
            // EQUIPMENT 
            float randomValue = Random.value * 100; // 0.0  to  100.0
            //Debug.Log("Item Drop RNG Value: " + randomValue);
            if (randomValue <= nothingDropRate)
            {
                Debug.Log("No equipment drop");

                // If rarity is forced, we will just choose random gear if "no drop" ever occurs:
                if (rarity != "random")
                {
                    int randomGear = Random.Range(0, 8); //range is exclusive, zero through seven
                    switch (randomGear)
                    {
                        case 0:
                            GenerateSword(PlayerStats.instance.playerLevel, enemyLevel, position, rarity, minimumRarity, fromChest);
                            break;
                        case 1:
                            GenerateShield(PlayerStats.instance.playerLevel, enemyLevel, position, rarity, minimumRarity, fromChest);
                            break;
                        case 2:
                            GenerateTorsoArmor(PlayerStats.instance.playerLevel, enemyLevel, position, rarity, minimumRarity, fromChest);
                            break;
                        case 3:
                            GenerateHandsArmor(PlayerStats.instance.playerLevel, enemyLevel, position, rarity, minimumRarity, fromChest);
                            break;
                        case 4:
                            GenerateLegsArmor(PlayerStats.instance.playerLevel, enemyLevel, position, rarity, minimumRarity, fromChest);
                            break;
                        case 5:
                            GenerateRing(PlayerStats.instance.playerLevel, enemyLevel, position, rarity, minimumRarity, fromChest);
                            break;
                        case 6:
                            GenerateAmulet(PlayerStats.instance.playerLevel, enemyLevel, position, rarity, minimumRarity, fromChest);
                            break;
                        default:
                            GenerateSword(PlayerStats.instance.playerLevel, enemyLevel, position, rarity, minimumRarity, fromChest);
                            break;
                    }
                }
            }
            else if (randomValue <= (nothingDropRate + swordDropRate))
            {
                GenerateSword(PlayerStats.instance.playerLevel, enemyLevel, position, rarity, minimumRarity, fromChest);
            }
            else if (randomValue <= (nothingDropRate + swordDropRate + shieldDropRate))
            {
                GenerateShield(PlayerStats.instance.playerLevel, enemyLevel, position, rarity, minimumRarity, fromChest);
            }
            else if (randomValue <= (nothingDropRate + swordDropRate + shieldDropRate + torsoDropRate))
            {
                GenerateTorsoArmor(PlayerStats.instance.playerLevel, enemyLevel, position, rarity, minimumRarity, fromChest);
            }
            else if (randomValue <= (nothingDropRate + swordDropRate + shieldDropRate + torsoDropRate + handsDropRate))
            {
                GenerateHandsArmor(PlayerStats.instance.playerLevel, enemyLevel, position, rarity, minimumRarity, fromChest);
            }
            else if (randomValue <= (nothingDropRate + swordDropRate + shieldDropRate + torsoDropRate + handsDropRate + legsDropRate))
            {
                GenerateLegsArmor(PlayerStats.instance.playerLevel, enemyLevel, position, rarity, minimumRarity, fromChest);
            }
            else if (randomValue <= (nothingDropRate + swordDropRate + shieldDropRate + torsoDropRate + handsDropRate + legsDropRate + ringDropRate))
            {
                GenerateRing(PlayerStats.instance.playerLevel, enemyLevel, position, rarity, minimumRarity, fromChest);
            }
            else if (randomValue <= (nothingDropRate + swordDropRate + shieldDropRate + torsoDropRate + handsDropRate + legsDropRate + ringDropRate + amuletDropRate))
            {
                GenerateAmulet(PlayerStats.instance.playerLevel, enemyLevel, position, rarity, minimumRarity, fromChest);
            }

            // CONSUMABLES
            float randomValue2 = Random.value * 100; // 0.0  to  100.0
            if (randomValue2 <= consumableDropRate)
            {
                Debug.Log("Consumable triggered");

                int maxConsumableRarity = 0;
                // Determine max consumable rarity based on enemy/chest(player) level
                if (enemyLevel > 10 && enemyLevel <= 20)
                {
                    maxConsumableRarity = 1;
                }
                else if (enemyLevel > 20 && enemyLevel <= 30)
                {
                    maxConsumableRarity = 2;
                }
                else if (enemyLevel > 20 && enemyLevel <= 30)
                {
                    maxConsumableRarity = 3;
                }
                else if (enemyLevel > 30)
                {
                    maxConsumableRarity = 4;
                }
                // determine rarity based on max possible rarity and the rarity chances array
                int consumableRarity = GenerateConsumableRarity(maxConsumableRarity);

                // determine consumable type based on rolled rarity
                // determine quantity based on rarity
                int consumableType = 0;
                int consumableQuantity = 1;
                float randomQuantity = Random.value * 100; // 0.0  to  100.0

                if (consumableRarity == 0)
                {
                    consumableType = Random.Range(0, 3); // minor potions ID 0, 1 and 2
                    consumableQuantity = Random.Range(1, 3);
                }
                else if (consumableRarity == 1)
                {
                    consumableType = Random.Range(3, 6); // standard potions ID 3, 4 and 5
                    consumableQuantity = Random.Range(1, 2);
                }
                else if (consumableRarity == 2)
                {
                    consumableType = Random.Range(6, 9); // major potions ID 6, 7 and 8
                    consumableQuantity = Random.Range(1, 1);
                }
                else if (consumableRarity == 3)
                {
                    consumableType = Random.Range(9, 12); // ultimate potions ID 9, 10 and 11
                }
                else
                {
                    consumableType = 12; // legendary potion ID 12
                }

                GenerateConsumable(consumableType, consumableQuantity, position, fromChest);
                //GenerateConsumable(1, 5, position);
                //GenerateConsumable(2, 5, position);
            }
        }

        public void GenerateRandomItemForQuestReward(int level, string rarity = "random", int minimumRarity = 0, int _gearType=-1)
        {
            // Determine Gear Type
            int gearType = -1;
            if (_gearType == -1)
            {
                gearType = Random.Range(0, 7); //range is exclusive, zero through six
            }
            else gearType = _gearType;

            switch (gearType)
            {
                case 0:
                    GenerateSword(PlayerStats.instance.playerLevel, level, Vector3.one, rarity, minimumRarity, false, null, true);
                    break;
                case 1:
                    GenerateShield(PlayerStats.instance.playerLevel, level, Vector3.one, rarity, minimumRarity, false, null, true);
                    break;
                case 2:
                    GenerateTorsoArmor(PlayerStats.instance.playerLevel, level, Vector3.one, rarity, minimumRarity, false, null, true);
                    break;
                case 3:
                    GenerateHandsArmor(PlayerStats.instance.playerLevel, level, Vector3.one, rarity, minimumRarity, false, null, true);
                    break;
                case 4:
                    GenerateLegsArmor(PlayerStats.instance.playerLevel, level, Vector3.one, rarity, minimumRarity, false, null, true);
                    break;
                case 5:
                    GenerateRing(PlayerStats.instance.playerLevel, level, Vector3.one, rarity, minimumRarity, false, null, true);
                    break;
                case 6:
                    GenerateAmulet(PlayerStats.instance.playerLevel, level, Vector3.one, rarity, minimumRarity, false, null, true);
                    break;
                default:
                    GenerateSword(PlayerStats.instance.playerLevel, level, Vector3.one, rarity, minimumRarity, false, null, true);
                    break;
            }  
        }

        public void GenerateGold(int enemyLevel, Vector3 position, int baseGoldYield)
        {
            // GOLD
            int randomMult = Random.Range(8, 13); // 8 to 12
            float multiplier = randomMult * 0.1f; //0.8x to 1.2x

            int goldAmount = (int)((baseGoldYield * multiplier) * (enemyLevel * goldScale));
            int randomValue3 = Random.Range(0, 5);
            goldAmount = (int)(goldAmount * goldMultiplier[randomValue3]);
            Debug.Log("Total Gold Amount: " + goldAmount);

            int coinSpawns = randomValue3 / 2 + 3; // 0,1,2,3,4 -> 3,3,4,4,5

            for (int i = 0; i < coinSpawns; i++)
            {
                GameObject spawnedObject = Instantiate(coinPrefab, position, Quaternion.Euler(-90f, 0f, 0f));
                spawnedObject.GetComponent<Coin>().amount = goldAmount / coinSpawns;
            }
        }

        //RARITY
        public int GetRandomRarity(string rarity="random", int minimumRarity=0)
        {
            // override rarity values, otherwise get random rarity
            if (rarity == "uncommon") return 0;
            if (rarity == "common") return 1;
            if (rarity == "rare") return 2;
            if (rarity == "epic") return 3;
            if (rarity == "legendary") return 4;

            int rarityResult = 0;

            // Generate a random value between 0 and 100
            int randomValue = Random.Range(0, 101);

            //Debug.Log("Before luck bonus:" + randomValue);

            int luckBonus = Mathf.RoundToInt(PlayerStats.instance.luck * 0.2f); // Adjust based on player's Luck

            // Add the luckBonus to the randomValue
            randomValue += luckBonus;
            randomValue = Mathf.Clamp(randomValue, 0, 100);

            //Debug.Log("After Luck Bonus:" + randomValue);

            // Initialize the cumulative probability
            int cumulativeProbability = 0;

            // Loop through rarity chances
            for (int i = 0; i < rarityChances.Length; i++)
            {
                cumulativeProbability += rarityChances[i];

                // Check if the random value falls within the current range
                if (randomValue <= cumulativeProbability)
                {
                    rarityResult = i; // Return the rarity value
                    if (rarityResult < minimumRarity) rarityResult = minimumRarity;
                    return rarityResult;
                }
            }
            rarityResult = rarityChances.Length - 1;
            if (rarityResult < minimumRarity) rarityResult = minimumRarity;
            return rarityResult;

        }

        public int GenerateConsumableRarity(int maxConsumableRarity)
        {
            // Ensure maxConsumableRarity is within a valid range
            maxConsumableRarity = Mathf.Clamp(maxConsumableRarity, 0, consumableRarityChances.Length - 1);

            Debug.Log("max rarity: " + maxConsumableRarity);

            // Sum up chances for rarities up to maxConsumableRarity
            int totalChances = 0;
            for (int i = 0; i <= maxConsumableRarity; i++)
            {
                totalChances += consumableRarityChances[i];
            }

            // Generate a random number between 0 and the total chances
            int randomValue = Random.Range(0, totalChances);

            // Determine the rarity based on the random value
            int rarity = 0;
            int cumulativeChances = 0;

            for (int i = 0; i <= maxConsumableRarity; i++)
            {
                cumulativeChances += consumableRarityChances[i];
                if (randomValue < cumulativeChances)
                {
                    rarity = i;
                    break;
                }
            }

            return rarity;
        }

        // SPLIT STATS
        public int SplitValue(int totalStats)
        {
            float randomRange = Random.Range(25, 76) / 100.0f; // Ensure floating-point division

            int firstHalf = Mathf.CeilToInt(randomRange * totalStats);
            int secondHalf = totalStats - firstHalf;

            //Debug.Log("Splitting " + totalStats + ": " + firstHalf + " and " + secondHalf);

            return firstHalf;
        }

        // CALCULATE VALUE
        public int CalculateValue(int rarity, int level)
        {
            // Base value formula based on level
            int baseValue = level * 100;

            // Apply a modifier based on rarity (adjusted for less significance)
            float rarityModifier = 1.0f - (rarity * 0.10f); // Smaller impact of rarity
            int rarityValue = (int)(baseValue * rarityModifier);

            // Add randomness to the final value (range from 70% to 120%)
            float randomFactor = UnityEngine.Random.Range(0.7f, 1.2f);
            int finalValue = (int)(rarityValue * randomFactor);

            return finalValue;
        }

        // GENERATE NAME
        public string GenerateName(string highestStat, string secondHighestStat, string itemName)
        {
            string prefix = "";
            string suffix = "";
            string name = "";

            switch(secondHighestStat)
            {
                case "strength":
                    prefix = prefixes[0];
                    break;
                case "endurance":
                    prefix = prefixes[1];
                    break;
                case "vitality":
                    prefix = prefixes[2];
                    break;
                case "precision":
                    prefix = prefixes[3];
                    break;
                case "dexterity":
                    prefix = prefixes[4];
                    break;
                case "expertise":
                    prefix = prefixes[5];
                    break;
                case "intelligence":
                    prefix = prefixes[6];
                    break;
                case "spirit":
                    prefix = prefixes[7];
                    break;
                case "willpower":
                    prefix = prefixes[8];
                    break;
                case "luck":
                    prefix = prefixes[9];
                    break;
                case "criticalChance":
                    prefix = prefixes[10];
                    break;
                case "criticalDamage":
                    prefix = prefixes[11];
                    break;
                case "fireResistance":
                    prefix = prefixes[12];
                    break;
                case "iceResistance":
                    prefix = prefixes[13];
                    break;
                case "shockResistance":
                    prefix = prefixes[14];
                    break;
                default:
                    break;
            }

            switch (highestStat)
            {
                case "strength":
                    suffix = suffixes[0];
                    break;
                case "endurance":
                    suffix = suffixes[1];
                    break;
                case "vitality":
                    suffix = suffixes[2];
                    break;
                case "precision":
                    suffix = suffixes[3];
                    break;
                case "dexterity":
                    suffix = suffixes[4];
                    break;
                case "expertise":
                    suffix = suffixes[5];
                    break;
                case "intelligence":
                    suffix = suffixes[6];
                    break;
                case "spirit":
                    suffix = suffixes[7];
                    break;
                case "willpower":
                    suffix = suffixes[8];
                    break;
                case "luck":
                    suffix = suffixes[9];
                    break;
                case "criticalChance":
                    suffix = suffixes[10];
                    break;
                case "criticalDamage":
                    suffix = suffixes[11];
                    break;
                case "fireResistance":
                    suffix = suffixes[12];
                    break;
                case "iceResistance":
                    suffix = suffixes[13];
                    break;
                case "shockResistance":
                    suffix = suffixes[14];
                    break;
                default:
                    break;
            }

            if(prefix != "")
            {
                name += prefix + " ";
            }

            name += itemName;

            if(suffix != "")
            {
                name += " of " + suffix;
            }

            return name;
        }

        // GENERATE SWORD
        public void GenerateSword(int enemyLevel, int playerLevel, Vector3 position, string rarity, int minimumRarity, bool fromChest, List<WeaponItem> shopInventory=null, bool directToInventory=false)
        {
            // Used for Naming Item
            // Initialize highest and second highest stats
            int highestStatValue = 0;
            int secondHighestStatValue = 0;
            string highestStat = "";
            string secondHighestStat = "";

            WeaponItem newWeapon = ScriptableObject.CreateInstance<WeaponItem>();

            // Calculate Item Level
            int averageLevel = (enemyLevel + playerLevel) / 2;
            int minLevel = averageLevel - 2;
            if (minLevel < 1) minLevel = 1;
            int effectiveLevel = Random.Range(minLevel, averageLevel + 3);
            newWeapon.level = effectiveLevel;

            // Determine Item Model/Type
            int swordType = Random.Range(0, 13);
            //Debug.Log("Sword Type: " + swordType);
            newWeapon.weaponModelID = swordType;
            newWeapon.itemIcon = PlayerInventory.instance.swordIcons[newWeapon.weaponModelID];

            // Calculate Rarity
            newWeapon.rarity = GetRandomRarity(rarity, minimumRarity);

            // Miscellaneous Information
            newWeapon.itemDescription = ""; //unused by swords
            newWeapon.itemID = 0;
            newWeapon.sellable = true;
            newWeapon.goldValue = CalculateValue(newWeapon.rarity, newWeapon.level);
            newWeapon.count = 1;

            // Calculate Damage
            int totalDamage = Random.Range(newWeapon.level + 19, newWeapon.level + 29);
            float rarityMultiplier = 1.0f + (newWeapon.rarity + 1.0f) * 0.05f;
            totalDamage = Mathf.RoundToInt(totalDamage * rarityMultiplier);
            int physicalDamage = SplitValue(totalDamage);
            int magicDamage = totalDamage - physicalDamage;
            if (magicDamage < 1) magicDamage = 1;
            newWeapon.physicalDamage = physicalDamage;
            newWeapon.magicDamage = magicDamage;

            // Unused or Unchanged
            newWeapon.poise = 10;
            newWeapon.baseStaminaCost = 20;

            //SET RANDOM STATS
            int[] numbers = PlayerInventory.instance.GenerateDistinctRandomNumbers(newWeapon.rarity, 0, 11); //12 possible stats. rarity 4(legendary) = 4 stats
            foreach (int num in numbers)
            {
                switch (num)
                {
                    case 0:
                        newWeapon.strength = GenerateStatValue(effectiveLevel);

                        if (newWeapon.strength >= highestStatValue)
                        {
                            secondHighestStatValue = highestStatValue;
                            highestStatValue = newWeapon.strength;
                            secondHighestStat = highestStat;
                            highestStat = "strength";
                        }
                        else if (newWeapon.strength > secondHighestStatValue)
                        {
                            secondHighestStatValue = newWeapon.strength;
                            secondHighestStat = "strength";
                        }
                        break;

                    case 1:
                        newWeapon.endurance = GenerateStatValue(effectiveLevel);
                        if (newWeapon.endurance >= highestStatValue)
                        {
                            secondHighestStatValue = highestStatValue;
                            highestStatValue = newWeapon.endurance;
                            secondHighestStat = highestStat;
                            highestStat = "endurance";
                        }
                        else if (newWeapon.endurance > secondHighestStatValue)
                        {
                            secondHighestStatValue = newWeapon.endurance;
                            secondHighestStat = "endurance";
                        }
                        break;

                    case 2:
                        newWeapon.vitality = GenerateStatValue(effectiveLevel);
                        if (newWeapon.vitality >= highestStatValue)
                        {
                            secondHighestStatValue = highestStatValue;
                            highestStatValue = newWeapon.vitality;
                            secondHighestStat = highestStat;
                            highestStat = "vitality";
                        }
                        else if (newWeapon.vitality > secondHighestStatValue)
                        {
                            secondHighestStatValue = newWeapon.vitality;
                            secondHighestStat = "vitality";
                        }
                        break;

                    case 3:
                        newWeapon.precision = GenerateStatValue(effectiveLevel);
                        if (newWeapon.precision >= highestStatValue)
                        {
                            secondHighestStatValue = highestStatValue;
                            highestStatValue = newWeapon.precision;
                            secondHighestStat = highestStat;
                            highestStat = "precision";
                        }
                        else if (newWeapon.precision > secondHighestStatValue)
                        {
                            secondHighestStatValue = newWeapon.precision;
                            secondHighestStat = "precision";
                        }
                        break;

                    case 4:
                        newWeapon.dexterity = GenerateStatValue(effectiveLevel);
                        if (newWeapon.dexterity >= highestStatValue)
                        {
                            secondHighestStatValue = highestStatValue;
                            highestStatValue = newWeapon.dexterity;
                            secondHighestStat = highestStat;
                            highestStat = "dexterity";
                        }
                        else if (newWeapon.dexterity > secondHighestStatValue)
                        {
                            secondHighestStatValue = newWeapon.dexterity;
                            secondHighestStat = "dexterity";
                        }
                        break;

                    case 5:
                        newWeapon.expertise = GenerateStatValue(effectiveLevel);
                        if (newWeapon.expertise >= highestStatValue)
                        {
                            secondHighestStatValue = highestStatValue;
                            highestStatValue = newWeapon.expertise;
                            secondHighestStat = highestStat;
                            highestStat = "expertise";
                        }
                        else if (newWeapon.expertise > secondHighestStatValue)
                        {
                            secondHighestStatValue = newWeapon.expertise;
                            secondHighestStat = "expertise";
                        }
                        break;

                    case 6:
                        newWeapon.intelligence = GenerateStatValue(effectiveLevel);
                        if (newWeapon.intelligence >= highestStatValue)
                        {
                            secondHighestStatValue = highestStatValue;
                            highestStatValue = newWeapon.intelligence;
                            secondHighestStat = highestStat;
                            highestStat = "intelligence";
                        }
                        else if (newWeapon.intelligence > secondHighestStatValue)
                        {
                            secondHighestStatValue = newWeapon.intelligence;
                            secondHighestStat = "intelligence";
                        }
                        break;

                    case 7:
                        newWeapon.spirit = GenerateStatValue(effectiveLevel);
                        if (newWeapon.spirit >= highestStatValue)
                        {
                            secondHighestStatValue = highestStatValue;
                            highestStatValue = newWeapon.spirit;
                            secondHighestStat = highestStat;
                            highestStat = "spirit";
                        }
                        else if (newWeapon.spirit > secondHighestStatValue)
                        {
                            secondHighestStatValue = newWeapon.spirit;
                            secondHighestStat = "spirit";
                        }
                        break;

                    case 8:
                        newWeapon.willpower = GenerateStatValue(effectiveLevel);
                        if (newWeapon.willpower >= highestStatValue)
                        {
                            secondHighestStatValue = highestStatValue;
                            highestStatValue = newWeapon.willpower;
                            secondHighestStat = highestStat;
                            highestStat = "willpower";
                        }
                        else if (newWeapon.willpower > secondHighestStatValue)
                        {
                            secondHighestStatValue = newWeapon.willpower;
                            secondHighestStat = "willpower";
                        }
                        break;

                    case 9:
                        newWeapon.luck = GenerateStatValue(effectiveLevel);
                        if (newWeapon.luck >= highestStatValue)
                        {
                            secondHighestStatValue = highestStatValue;
                            highestStatValue = newWeapon.luck;
                            secondHighestStat = highestStat;
                            highestStat = "luck";
                        }
                        else if (newWeapon.luck > secondHighestStatValue)
                        {
                            secondHighestStatValue = newWeapon.luck;
                            secondHighestStat = "luck";
                        }
                        break;

                    case 10:
                        newWeapon.criticalChance = GenerateCritValue(effectiveLevel, isCritRate: true);
                        int critComparisonValue2 = Mathf.RoundToInt(newWeapon.criticalChance * 5);
                        //Debug.Log("Crit Chance Compare: " + critComparisonValue2);
                        if (critComparisonValue2 >= highestStatValue)
                        {
                            secondHighestStatValue = highestStatValue;
                            highestStatValue = critComparisonValue2;
                            secondHighestStat = highestStat;
                            highestStat = "criticalChance";
                        }
                        else if (critComparisonValue2 > secondHighestStatValue)
                        {
                            secondHighestStatValue = critComparisonValue2;
                            secondHighestStat = "criticalChance";
                        }
                        break;

                    case 11:
                        newWeapon.criticalDamage = GenerateCritValue(effectiveLevel, isCritRate: false);
                        int critComparisonValue = Mathf.RoundToInt(newWeapon.criticalDamage * 2);
                        //Debug.Log("Crit Dmg Compare: " + critComparisonValue);
                        if (critComparisonValue >= highestStatValue)
                        {
                            secondHighestStatValue = highestStatValue;
                            highestStatValue = critComparisonValue;
                            secondHighestStat = highestStat;
                            highestStat = "criticalDamage";
                        }
                        else if (critComparisonValue > secondHighestStatValue)
                        {
                            secondHighestStatValue = critComparisonValue;
                            secondHighestStat = "criticalDamage";
                        }
                        break;

                    default:
                        break;
                }
            }

            // Apply Name
            newWeapon.itemName = GenerateName(highestStat, secondHighestStat, swordNames[swordType]);
            newWeapon.name = "WeaponItem_" + newWeapon.weaponModelID.ToString() + "_" + newWeapon.level.ToString() + "_" + newWeapon.physicalDamage.ToString() + "_" + newWeapon.magicDamage.ToString();

            if(shopInventory != null)
            {
                shopInventory.Add(newWeapon);
                return;
            }
            else if (directToInventory)
            {
                // Directly to inventory (occurs when Item is Quest Reward)
                playerInventory.AddToInventory(newWeapon);
                AcquiredNotifications.instance.NewItemNotification(newWeapon, newWeapon.count);
                return;
            }

            // Drop Item
            GameObject spawnedObject = Instantiate(itemDropPrefab, position, Quaternion.Euler(-90f, 0f, 0f));
            spawnedObject.GetComponent<ItemDrop>().item = newWeapon;
            spawnedObject.GetComponent<ItemDrop>().SetColors(newWeapon.rarity);
            //PlayerInventory.instance.weaponsInventory.Add(newWeapon);
            spawnedObject.GetComponent<ItemDrop>().SetForcesAndApply(fromChest);
        }

        // GENERATE SHIELD
        public void GenerateShield(int enemyLevel, int playerLevel, Vector3 position, string rarity, int minimumRarity, bool fromChest, List<ShieldItem> shopInventory = null, bool directToInventory = false)
        {
            // Used for Naming Item
            // Initialize highest and second highest stats
            int highestStatValue = 0;
            int secondHighestStatValue = 0;
            string highestStat = "";
            string secondHighestStat = "";

            // Create Item
            ShieldItem newShield = ScriptableObject.CreateInstance<ShieldItem>();

            // Calculate Item Level
            int averageLevel = (enemyLevel + playerLevel) / 2;
            int minLevel = averageLevel - 2;
            if (minLevel < 1) minLevel = 1;
            int effectiveLevel = Random.Range(minLevel, averageLevel + 3);
            newShield.level = effectiveLevel;

            // Determine Item Model/Type
            int shieldType = Random.Range(0, 14);
            //Debug.Log("Shield Type: " + shieldType);
            newShield.shieldModelID = shieldType;
            newShield.itemIcon = PlayerInventory.instance.shieldIcons[newShield.shieldModelID];

            // Calculate Rarity
            newShield.rarity = GetRandomRarity(rarity, minimumRarity);

            // Miscellaneous Information
            newShield.itemDescription = ""; //unused by shields
            newShield.itemID = 0;
            newShield.sellable = true;
            newShield.goldValue = CalculateValue(newShield.rarity, newShield.level);
            newShield.count = 1;

            // Calculate Damage
            int totalBlock = Random.Range(newShield.level + 9, newShield.level + 19);
            float rarityMultiplier = 1.0f + (newShield.rarity + 1.0f) * 0.05f;
            totalBlock = Mathf.RoundToInt(totalBlock * rarityMultiplier);
            newShield.blockRating = totalBlock;

            //SET RANDOM STATS
            int[] numbers = PlayerInventory.instance.GenerateDistinctRandomNumbers(newShield.rarity, 0, 12); //13 possible stats. rarity 4(legendary) = 4 stats
            foreach (int num in numbers)
            {
                switch (num)
                {
                    case 0:
                        newShield.strength = GenerateStatValue(effectiveLevel);
                        if (newShield.strength >= highestStatValue)
                        {
                            secondHighestStatValue = highestStatValue;
                            highestStatValue = newShield.strength;
                            secondHighestStat = highestStat;
                            highestStat = "strength";
                        }
                        else if (newShield.strength > secondHighestStatValue)
                        {
                            secondHighestStatValue = newShield.strength;
                            secondHighestStat = "strength";
                        }
                        break;

                    case 1:
                        newShield.endurance = GenerateStatValue(effectiveLevel);
                        if (newShield.endurance >= highestStatValue)
                        {
                            secondHighestStatValue = highestStatValue;
                            highestStatValue = newShield.endurance;
                            secondHighestStat = highestStat;
                            highestStat = "endurance";
                        }
                        else if (newShield.endurance > secondHighestStatValue)
                        {
                            secondHighestStatValue = newShield.endurance;
                            secondHighestStat = "endurance";
                        }
                        break;

                    case 2:
                        newShield.vitality = GenerateStatValue(effectiveLevel);
                        if (newShield.vitality >= highestStatValue)
                        {
                            secondHighestStatValue = highestStatValue;
                            highestStatValue = newShield.vitality;
                            secondHighestStat = highestStat;
                            highestStat = "vitality";
                        }
                        else if (newShield.vitality > secondHighestStatValue)
                        {
                            secondHighestStatValue = newShield.vitality;
                            secondHighestStat = "vitality";
                        }
                        break;

                    case 3:
                        newShield.precision = GenerateStatValue(effectiveLevel);
                        if (newShield.precision >= highestStatValue)
                        {
                            secondHighestStatValue = highestStatValue;
                            highestStatValue = newShield.precision;
                            secondHighestStat = highestStat;
                            highestStat = "precision";
                        }
                        else if (newShield.precision > secondHighestStatValue)
                        {
                            secondHighestStatValue = newShield.precision;
                            secondHighestStat = "precision";
                        }
                        break;

                    case 4:
                        newShield.dexterity = GenerateStatValue(effectiveLevel);
                        if (newShield.dexterity >= highestStatValue)
                        {
                            secondHighestStatValue = highestStatValue;
                            highestStatValue = newShield.dexterity;
                            secondHighestStat = highestStat;
                            highestStat = "dexterity";
                        }
                        else if (newShield.dexterity > secondHighestStatValue)
                        {
                            secondHighestStatValue = newShield.dexterity;
                            secondHighestStat = "dexterity";
                        }
                        break;

                    case 5:
                        newShield.expertise = GenerateStatValue(effectiveLevel);
                        if (newShield.expertise >= highestStatValue)
                        {
                            secondHighestStatValue = highestStatValue;
                            highestStatValue = newShield.expertise;
                            secondHighestStat = highestStat;
                            highestStat = "expertise";
                        }
                        else if (newShield.expertise > secondHighestStatValue)
                        {
                            secondHighestStatValue = newShield.expertise;
                            secondHighestStat = "expertise";
                        }
                        break;

                    case 6:
                        newShield.intelligence = GenerateStatValue(effectiveLevel);
                        if (newShield.intelligence >= highestStatValue)
                        {
                            secondHighestStatValue = highestStatValue;
                            highestStatValue = newShield.intelligence;
                            secondHighestStat = highestStat;
                            highestStat = "intelligence";
                        }
                        else if (newShield.intelligence > secondHighestStatValue)
                        {
                            secondHighestStatValue = newShield.intelligence;
                            secondHighestStat = "intelligence";
                        }
                        break;

                    case 7:
                        newShield.spirit = GenerateStatValue(effectiveLevel);
                        if (newShield.spirit >= highestStatValue)
                        {
                            secondHighestStatValue = highestStatValue;
                            highestStatValue = newShield.spirit;
                            secondHighestStat = highestStat;
                            highestStat = "spirit";
                        }
                        else if (newShield.spirit > secondHighestStatValue)
                        {
                            secondHighestStatValue = newShield.spirit;
                            secondHighestStat = "spirit";
                        }
                        break;

                    case 8:
                        newShield.willpower = GenerateStatValue(effectiveLevel);
                        if (newShield.willpower >= highestStatValue)
                        {
                            secondHighestStatValue = highestStatValue;
                            highestStatValue = newShield.willpower;
                            secondHighestStat = highestStat;
                            highestStat = "willpower";
                        }
                        else if (newShield.willpower > secondHighestStatValue)
                        {
                            secondHighestStatValue = newShield.willpower;
                            secondHighestStat = "willpower";
                        }
                        break;

                    case 9:
                        newShield.luck = GenerateStatValue(effectiveLevel);
                        if (newShield.luck >= highestStatValue)
                        {
                            secondHighestStatValue = highestStatValue;
                            highestStatValue = newShield.luck;
                            secondHighestStat = highestStat;
                            highestStat = "luck";
                        }
                        else if (newShield.luck > secondHighestStatValue)
                        {
                            secondHighestStatValue = newShield.luck;
                            secondHighestStat = "luck";
                        }
                        break;

                    case 10:
                        float minFireRes = 0.1f + (effectiveLevel - 1) * 0.1f;
                        float maxFireRes = 10.0f + (effectiveLevel - 1) * 0.15f;
                        newShield.fireResistance = Random.Range(minFireRes, maxFireRes);
                        newShield.fireResistance = Mathf.Ceil(newShield.fireResistance * 10f) / 10f;
                        int fireResComparisonValue = Mathf.CeilToInt(newShield.fireResistance);

                        //Debug.Log("Fire Res Compare: " + fireResComparisonValue);
                        if (fireResComparisonValue >= highestStatValue)
                        {
                            secondHighestStatValue = highestStatValue;
                            highestStatValue = fireResComparisonValue;
                            secondHighestStat = highestStat;
                            highestStat = "fireResistance";
                        }
                        else if (fireResComparisonValue > secondHighestStatValue)
                        {
                            secondHighestStatValue = fireResComparisonValue;
                            secondHighestStat = "fireResistance";
                        }
                        break;

                    case 11:
                        float minIceRes = 0.1f + (effectiveLevel - 1) * 0.1f;
                        float maxIceRes = 10.0f + (effectiveLevel - 1) * 0.15f;
                        newShield.iceResistance = Random.Range(minIceRes, maxIceRes);
                        newShield.iceResistance = Mathf.Ceil(newShield.iceResistance * 10f) / 10f;
                        int iceResComparisonValue = Mathf.CeilToInt(newShield.iceResistance);

                        //Debug.Log("Ice Res Compare: " + iceResComparisonValue);
                        if (iceResComparisonValue >= highestStatValue)
                        {
                            secondHighestStatValue = highestStatValue;
                            highestStatValue = iceResComparisonValue;
                            secondHighestStat = highestStat;
                            highestStat = "iceResistance";
                        }
                        else if (iceResComparisonValue > secondHighestStatValue)
                        {
                            secondHighestStatValue = iceResComparisonValue;
                            secondHighestStat = "iceResistance";
                        }
                        break;

                    case 12:
                        float minShockRes = 0.1f + (effectiveLevel - 1) * 0.1f;
                        float maxShockRes = 10.0f + (effectiveLevel - 1) * 0.15f;
                        newShield.shockResistance = Random.Range(minShockRes, maxShockRes);
                        newShield.shockResistance = Mathf.Ceil(newShield.shockResistance * 10f) / 10f;
                        int shockResComparisonValue = Mathf.CeilToInt(newShield.shockResistance);

                        //Debug.Log("Shock Res Compare: " + shockResComparisonValue);
                        if (shockResComparisonValue >= highestStatValue)
                        {
                            secondHighestStatValue = highestStatValue;
                            highestStatValue = shockResComparisonValue;
                            secondHighestStat = highestStat;
                            highestStat = "shockResistance";
                        }
                        else if (shockResComparisonValue > secondHighestStatValue)
                        {
                            secondHighestStatValue = shockResComparisonValue;
                            secondHighestStat = "shockResistance";
                        }
                        break;

                    default:
                        break;
                }
            }

            // Apply Name
            newShield.itemName = GenerateName(highestStat, secondHighestStat, shieldNames[shieldType]);
            newShield.name = "ShieldItem_" + newShield.shieldModelID.ToString() + "_" + newShield.level.ToString() + "_" + newShield.blockRating.ToString();

            if (shopInventory != null)
            {
                shopInventory.Add(newShield);
                return;
            }
            else if (directToInventory)
            {
                // Directly to inventory (occurs when Item is Quest Reward)
                playerInventory.AddToInventory(newShield);
                AcquiredNotifications.instance.NewItemNotification(newShield, newShield.count);
                return;
            }

            // Drop Item
            GameObject spawnedObject = Instantiate(itemDropPrefab, position, Quaternion.Euler(-90f, 0f, 0f));
            spawnedObject.GetComponent<ItemDrop>().item = newShield;
            spawnedObject.GetComponent<ItemDrop>().SetColors(newShield.rarity);

            spawnedObject.GetComponent<ItemDrop>().SetForcesAndApply(fromChest);
        }

        // GENERATE TORSO ARMOR
        public void GenerateTorsoArmor(int enemyLevel, int playerLevel, Vector3 position, string rarity, int minimumRarity, bool fromChest, List<TorsoArmorItem> shopInventory = null, bool directToInventory = false)
        {
            // Used for Naming Item
            // Initialize highest and second highest stats
            int highestStatValue = 0;
            int secondHighestStatValue = 0;
            string highestStat = "";
            string secondHighestStat = "";

            // Create Item
            TorsoArmorItem newTorsoArmor = ScriptableObject.CreateInstance<TorsoArmorItem>();

            // Calculate Item Level
            int averageLevel = (enemyLevel + playerLevel) / 2;
            int minLevel = averageLevel - 2;
            if (minLevel < 1) minLevel = 1; 
            int effectiveLevel = Random.Range(minLevel, averageLevel + 3);
            newTorsoArmor.level = effectiveLevel;

            // Determine Item Model/Type
            int armorType = Random.Range(0, 2);
            //Debug.Log("Armor Type: " + armorType);
            newTorsoArmor.torsoArmorModelID = armorType;
            newTorsoArmor.itemIcon = PlayerInventory.instance.torsoArmorIcons[newTorsoArmor.torsoArmorModelID];

            // Calculate Rarity
            newTorsoArmor.rarity = GetRandomRarity(rarity, minimumRarity);

            // Miscellaneous Information
            newTorsoArmor.itemDescription = ""; //unused by shields
            newTorsoArmor.itemID = 0;
            newTorsoArmor.sellable = true;
            newTorsoArmor.goldValue = CalculateValue(newTorsoArmor.rarity, newTorsoArmor.level);
            newTorsoArmor.count = 1;

            // Calculate Damage
            int totalArmor = Random.Range(newTorsoArmor.level + 9, newTorsoArmor.level + 14);
            float rarityMultiplier = 1.0f + (newTorsoArmor.rarity + 1.0f) * 0.05f;
            totalArmor = Mathf.RoundToInt(totalArmor * rarityMultiplier);
            int physicalArmor = SplitValue(totalArmor);
            int magicalArmor = totalArmor - physicalArmor;
            if (magicalArmor < 1) magicalArmor = 1;
            newTorsoArmor.physicalArmorRating = physicalArmor;
            newTorsoArmor.magicalArmorRating = magicalArmor;

            //SET RANDOM STATS
            int[] numbers = PlayerInventory.instance.GenerateDistinctRandomNumbers(newTorsoArmor.rarity, 0, 14); //15 possible stats. rarity 4(legendary) = 4 stats
            foreach (int num in numbers)
            {
                switch (num)
                {
                    case 0:
                        newTorsoArmor.strength = GenerateStatValue(effectiveLevel);
                        if (newTorsoArmor.strength >= highestStatValue)
                        {
                            secondHighestStatValue = highestStatValue;
                            highestStatValue = newTorsoArmor.strength;
                            secondHighestStat = highestStat;
                            highestStat = "strength";
                        }
                        else if (newTorsoArmor.strength > secondHighestStatValue)
                        {
                            secondHighestStatValue = newTorsoArmor.strength;
                            secondHighestStat = "strength";
                        }
                        break;

                    case 1:
                        newTorsoArmor.endurance = GenerateStatValue(effectiveLevel);
                        if (newTorsoArmor.endurance >= highestStatValue)
                        {
                            secondHighestStatValue = highestStatValue;
                            highestStatValue = newTorsoArmor.endurance;
                            secondHighestStat = highestStat;
                            highestStat = "endurance";
                        }
                        else if (newTorsoArmor.endurance > secondHighestStatValue)
                        {
                            secondHighestStatValue = newTorsoArmor.endurance;
                            secondHighestStat = "endurance";
                        }
                        break;

                    case 2:
                        newTorsoArmor.vitality = GenerateStatValue(effectiveLevel);
                        if (newTorsoArmor.vitality >= highestStatValue)
                        {
                            secondHighestStatValue = highestStatValue;
                            highestStatValue = newTorsoArmor.vitality;
                            secondHighestStat = highestStat;
                            highestStat = "vitality";
                        }
                        else if (newTorsoArmor.vitality > secondHighestStatValue)
                        {
                            secondHighestStatValue = newTorsoArmor.vitality;
                            secondHighestStat = "vitality";
                        }
                        break;

                    case 3:
                        newTorsoArmor.precision = GenerateStatValue(effectiveLevel);
                        if (newTorsoArmor.precision >= highestStatValue)
                        {
                            secondHighestStatValue = highestStatValue;
                            highestStatValue = newTorsoArmor.precision;
                            secondHighestStat = highestStat;
                            highestStat = "precision";
                        }
                        else if (newTorsoArmor.precision > secondHighestStatValue)
                        {
                            secondHighestStatValue = newTorsoArmor.precision;
                            secondHighestStat = "precision";
                        }
                        break;

                    case 4:
                        newTorsoArmor.dexterity = GenerateStatValue(effectiveLevel);
                        if (newTorsoArmor.dexterity >= highestStatValue)
                        {
                            secondHighestStatValue = highestStatValue;
                            highestStatValue = newTorsoArmor.dexterity;
                            secondHighestStat = highestStat;
                            highestStat = "dexterity";
                        }
                        else if (newTorsoArmor.dexterity > secondHighestStatValue)
                        {
                            secondHighestStatValue = newTorsoArmor.dexterity;
                            secondHighestStat = "dexterity";
                        }
                        break;

                    case 5:
                        newTorsoArmor.expertise = GenerateStatValue(effectiveLevel);
                        if (newTorsoArmor.expertise >= highestStatValue)
                        {
                            secondHighestStatValue = highestStatValue;
                            highestStatValue = newTorsoArmor.expertise;
                            secondHighestStat = highestStat;
                            highestStat = "expertise";
                        }
                        else if (newTorsoArmor.expertise > secondHighestStatValue)
                        {
                            secondHighestStatValue = newTorsoArmor.expertise;
                            secondHighestStat = "expertise";
                        }
                        break;

                    case 6:
                        newTorsoArmor.intelligence = GenerateStatValue(effectiveLevel);
                        if (newTorsoArmor.intelligence >= highestStatValue)
                        {
                            secondHighestStatValue = highestStatValue;
                            highestStatValue = newTorsoArmor.intelligence;
                            secondHighestStat = highestStat;
                            highestStat = "intelligence";
                        }
                        else if (newTorsoArmor.intelligence > secondHighestStatValue)
                        {
                            secondHighestStatValue = newTorsoArmor.intelligence;
                            secondHighestStat = "intelligence";
                        }
                        break;

                    case 7:
                        newTorsoArmor.spirit = GenerateStatValue(effectiveLevel);
                        if (newTorsoArmor.spirit >= highestStatValue)
                        {
                            secondHighestStatValue = highestStatValue;
                            highestStatValue = newTorsoArmor.spirit;
                            secondHighestStat = highestStat;
                            highestStat = "spirit";
                        }
                        else if (newTorsoArmor.spirit > secondHighestStatValue)
                        {
                            secondHighestStatValue = newTorsoArmor.spirit;
                            secondHighestStat = "spirit";
                        }
                        break;

                    case 8:
                        newTorsoArmor.willpower = GenerateStatValue(effectiveLevel);
                        if (newTorsoArmor.willpower >= highestStatValue)
                        {
                            secondHighestStatValue = highestStatValue;
                            highestStatValue = newTorsoArmor.willpower;
                            secondHighestStat = highestStat;
                            highestStat = "willpower";
                        }
                        else if (newTorsoArmor.willpower > secondHighestStatValue)
                        {
                            secondHighestStatValue = newTorsoArmor.willpower;
                            secondHighestStat = "willpower";
                        }
                        break;

                    case 9:
                        newTorsoArmor.luck = GenerateStatValue(effectiveLevel);
                        if (newTorsoArmor.luck >= highestStatValue)
                        {
                            secondHighestStatValue = highestStatValue;
                            highestStatValue = newTorsoArmor.luck;
                            secondHighestStat = highestStat;
                            highestStat = "luck";
                        }
                        else if (newTorsoArmor.luck > secondHighestStatValue)
                        {
                            secondHighestStatValue = newTorsoArmor.luck;
                            secondHighestStat = "luck";
                        }
                        break;

                    case 10:
                        newTorsoArmor.criticalChance = GenerateCritValue(effectiveLevel, isCritRate: true);
                        int critComparisonValue2 = Mathf.RoundToInt(newTorsoArmor.criticalChance * 5);
                        //Debug.Log("Crit Chance Compare: " + critComparisonValue2);
                        if (critComparisonValue2 >= highestStatValue)
                        {
                            secondHighestStatValue = highestStatValue;
                            highestStatValue = critComparisonValue2;
                            secondHighestStat = highestStat;
                            highestStat = "criticalChance";
                        }
                        else if (critComparisonValue2 > secondHighestStatValue)
                        {
                            secondHighestStatValue = critComparisonValue2;
                            secondHighestStat = "criticalChance";
                        }
                        break;

                    case 11:
                        newTorsoArmor.criticalDamage = GenerateCritValue(effectiveLevel, isCritRate: false);
                        int critComparisonValue = Mathf.RoundToInt(newTorsoArmor.criticalDamage * 2);
                        //Debug.Log("Crit Dmg Compare: " + critComparisonValue);
                        if (critComparisonValue >= highestStatValue)
                        {
                            secondHighestStatValue = highestStatValue;
                            highestStatValue = critComparisonValue;
                            secondHighestStat = highestStat;
                            highestStat = "criticalDamage";
                        }
                        else if (critComparisonValue > secondHighestStatValue)
                        {
                            secondHighestStatValue = critComparisonValue;
                            secondHighestStat = "criticalDamage";
                        }
                        break;

                    case 12:
                        float minFireRes = 0.1f + (effectiveLevel - 1) * 0.1f;
                        float maxFireRes = 10.0f + (effectiveLevel - 1) * 0.15f;
                        newTorsoArmor.fireResistance = Random.Range(minFireRes, maxFireRes);
                        newTorsoArmor.fireResistance = Mathf.Ceil(newTorsoArmor.fireResistance * 10f) / 10f;
                        int fireResComparisonValue = Mathf.CeilToInt(newTorsoArmor.fireResistance);

                        //Debug.Log("Fire Res Compare: " + fireResComparisonValue);
                        if (fireResComparisonValue >= highestStatValue)
                        {
                            secondHighestStatValue = highestStatValue;
                            highestStatValue = fireResComparisonValue;
                            secondHighestStat = highestStat;
                            highestStat = "fireResistance";
                        }
                        else if (fireResComparisonValue > secondHighestStatValue)
                        {
                            secondHighestStatValue = fireResComparisonValue;
                            secondHighestStat = "fireResistance";
                        }
                        break;

                    case 13:
                        float minIceRes = 0.1f + (effectiveLevel - 1) * 0.1f;
                        float maxIceRes = 10.0f + (effectiveLevel - 1) * 0.15f;
                        newTorsoArmor.iceResistance = Random.Range(minIceRes, maxIceRes);
                        newTorsoArmor.iceResistance = Mathf.Ceil(newTorsoArmor.iceResistance * 10f) / 10f;
                        int iceResComparisonValue = Mathf.CeilToInt(newTorsoArmor.iceResistance);

                        //Debug.Log("Ice Res Compare: " + iceResComparisonValue);
                        if (iceResComparisonValue >= highestStatValue)
                        {
                            secondHighestStatValue = highestStatValue;
                            highestStatValue = iceResComparisonValue;
                            secondHighestStat = highestStat;
                            highestStat = "iceResistance";
                        }
                        else if (iceResComparisonValue > secondHighestStatValue)
                        {
                            secondHighestStatValue = iceResComparisonValue;
                            secondHighestStat = "iceResistance";
                        }
                        break;

                    case 14:
                        float minShockRes = 0.1f + (effectiveLevel - 1) * 0.1f;
                        float maxShockRes = 10.0f + (effectiveLevel - 1) * 0.15f;
                        newTorsoArmor.shockResistance = Random.Range(minShockRes, maxShockRes);
                        newTorsoArmor.shockResistance = Mathf.Ceil(newTorsoArmor.shockResistance * 10f) / 10f;
                        int shockResComparisonValue = Mathf.CeilToInt(newTorsoArmor.shockResistance);

                        //Debug.Log("Shock Res Compare: " + shockResComparisonValue);
                        if (shockResComparisonValue >= highestStatValue)
                        {
                            secondHighestStatValue = highestStatValue;
                            highestStatValue = shockResComparisonValue;
                            secondHighestStat = highestStat;
                            highestStat = "shockResistance";
                        }
                        else if (shockResComparisonValue > secondHighestStatValue)
                        {
                            secondHighestStatValue = shockResComparisonValue;
                            secondHighestStat = "shockResistance";
                        }
                        break;

                    default:
                        break;
                }
            }

            // Apply Name
            int randomIndex = Random.Range(0, torsoArmorNames.Length);
            newTorsoArmor.itemName = GenerateName(highestStat, secondHighestStat, torsoArmorNames[randomIndex]);
            newTorsoArmor.name = "TorsoArmorItem_" + newTorsoArmor.torsoArmorModelID.ToString() + "_" + newTorsoArmor.level.ToString();

            if (shopInventory != null)
            {
                shopInventory.Add(newTorsoArmor);
                return;
            }
            else if (directToInventory)
            {
                // Directly to inventory (occurs when Item is Quest Reward)
                playerInventory.AddToInventory(newTorsoArmor);
                AcquiredNotifications.instance.NewItemNotification(newTorsoArmor, newTorsoArmor.count);
                return;
            }

            // Drop Item
            GameObject spawnedObject = Instantiate(itemDropPrefab, position, Quaternion.Euler(-90f, 0f, 0f));
            spawnedObject.GetComponent<ItemDrop>().item = newTorsoArmor;
            spawnedObject.GetComponent<ItemDrop>().SetColors(newTorsoArmor.rarity);

            spawnedObject.GetComponent<ItemDrop>().SetForcesAndApply(fromChest);
        }

        // GENERATE HANDS ARMOR
        public void GenerateHandsArmor(int enemyLevel, int playerLevel, Vector3 position, string rarity, int minimumRarity, bool fromChest, List<HandsArmorItem> shopInventory = null, bool directToInventory = false)
        {
            // Used for Naming Item
            // Initialize highest and second highest stats
            int highestStatValue = 0;
            int secondHighestStatValue = 0;
            string highestStat = "";
            string secondHighestStat = "";

            // Create Item
            HandsArmorItem newHandsArmor = ScriptableObject.CreateInstance<HandsArmorItem>();

            // Calculate Item Level
            int averageLevel = (enemyLevel + playerLevel) / 2;
            int minLevel = averageLevel - 2;
            if (minLevel < 1) minLevel = 1;
            int effectiveLevel = Random.Range(minLevel, averageLevel + 3);
            newHandsArmor.level = effectiveLevel;

            // Determine Item Model/Type
            int armorType = Random.Range(0, 2);
            //Debug.Log("Armor Type: " + armorType);
            newHandsArmor.handsArmorModelID = armorType;
            newHandsArmor.itemIcon = PlayerInventory.instance.handsArmorIcons[newHandsArmor.handsArmorModelID];

            // Calculate Rarity
            newHandsArmor.rarity = GetRandomRarity(rarity, minimumRarity);

            // Miscellaneous Information
            newHandsArmor.itemDescription = ""; //unused by armor
            newHandsArmor.itemID = 0;
            newHandsArmor.sellable = true;
            newHandsArmor.goldValue = CalculateValue(newHandsArmor.rarity, newHandsArmor.level);
            newHandsArmor.count = 1;

            // Calculate Damage
            int totalArmor = Random.Range(newHandsArmor.level + 9, newHandsArmor.level + 14);
            float rarityMultiplier = 1.0f + (newHandsArmor.rarity + 1.0f) * 0.05f;
            totalArmor = Mathf.RoundToInt(totalArmor * rarityMultiplier);
            int physicalArmor = SplitValue(totalArmor);
            int magicalArmor = totalArmor - physicalArmor;
            if (magicalArmor < 1) magicalArmor = 1;
            newHandsArmor.physicalArmorRating = physicalArmor;
            newHandsArmor.magicalArmorRating = magicalArmor;

            //SET RANDOM STATS
            int[] numbers = PlayerInventory.instance.GenerateDistinctRandomNumbers(newHandsArmor.rarity, 0, 6); //7 possible stats. rarity 4(legendary) = 4 stats
            foreach (int num in numbers)
            {
                switch (num)
                {
                    case 0:
                        newHandsArmor.strength = GenerateStatValue(effectiveLevel);
                        if (newHandsArmor.strength >= highestStatValue)
                        {
                            secondHighestStatValue = highestStatValue;
                            highestStatValue = newHandsArmor.strength;
                            secondHighestStat = highestStat;
                            highestStat = "strength";
                        }
                        else if (newHandsArmor.strength > secondHighestStatValue)
                        {
                            secondHighestStatValue = newHandsArmor.strength;
                            secondHighestStat = "strength";
                        }
                        break;

                    case 1:
                        newHandsArmor.precision = GenerateStatValue(effectiveLevel);
                        if (newHandsArmor.precision >= highestStatValue)
                        {
                            secondHighestStatValue = highestStatValue;
                            highestStatValue = newHandsArmor.precision;
                            secondHighestStat = highestStat;
                            highestStat = "precision";
                        }
                        else if (newHandsArmor.precision > secondHighestStatValue)
                        {
                            secondHighestStatValue = newHandsArmor.precision;
                            secondHighestStat = "precision";
                        }
                        break;

                    case 2:
                        newHandsArmor.intelligence = GenerateStatValue(effectiveLevel);
                        if (newHandsArmor.intelligence >= highestStatValue)
                        {
                            secondHighestStatValue = highestStatValue;
                            highestStatValue = newHandsArmor.intelligence;
                            secondHighestStat = highestStat;
                            highestStat = "intelligence";
                        }
                        else if (newHandsArmor.intelligence > secondHighestStatValue)
                        {
                            secondHighestStatValue = newHandsArmor.intelligence;
                            secondHighestStat = "intelligence";
                        }
                        break;

                    case 3:
                        newHandsArmor.spirit = GenerateStatValue(effectiveLevel);
                        if (newHandsArmor.spirit >= highestStatValue)
                        {
                            secondHighestStatValue = highestStatValue;
                            highestStatValue = newHandsArmor.spirit;
                            secondHighestStat = highestStat;
                            highestStat = "spirit";
                        }
                        else if (newHandsArmor.spirit > secondHighestStatValue)
                        {
                            secondHighestStatValue = newHandsArmor.spirit;
                            secondHighestStat = "spirit";
                        }
                        break;

                    case 4:
                        newHandsArmor.luck = GenerateStatValue(effectiveLevel);
                        if (newHandsArmor.luck >= highestStatValue)
                        {
                            secondHighestStatValue = highestStatValue;
                            highestStatValue = newHandsArmor.luck;
                            secondHighestStat = highestStat;
                            highestStat = "luck";
                        }
                        else if (newHandsArmor.luck > secondHighestStatValue)
                        {
                            secondHighestStatValue = newHandsArmor.luck;
                            secondHighestStat = "luck";
                        }
                        break;

                    case 5:
                        newHandsArmor.criticalChance = GenerateCritValue(effectiveLevel, isCritRate: true);
                        int critComparisonValue2 = Mathf.RoundToInt(newHandsArmor.criticalChance * 5);
                        //Debug.Log("Crit Chance Compare: " + critComparisonValue2);
                        if (critComparisonValue2 >= highestStatValue)
                        {
                            secondHighestStatValue = highestStatValue;
                            highestStatValue = critComparisonValue2;
                            secondHighestStat = highestStat;
                            highestStat = "criticalChance";
                        }
                        else if (critComparisonValue2 > secondHighestStatValue)
                        {
                            secondHighestStatValue = critComparisonValue2;
                            secondHighestStat = "criticalChance";
                        }
                        break;

                    case 6:
                        newHandsArmor.criticalDamage = GenerateCritValue(effectiveLevel, isCritRate: false);
                        int critComparisonValue = Mathf.RoundToInt(newHandsArmor.criticalDamage * 2);
                        //Debug.Log("Crit Dmg Compare: " + critComparisonValue);
                        if (critComparisonValue >= highestStatValue)
                        {
                            secondHighestStatValue = highestStatValue;
                            highestStatValue = critComparisonValue;
                            secondHighestStat = highestStat;
                            highestStat = "criticalDamage";
                        }
                        else if (critComparisonValue > secondHighestStatValue)
                        {
                            secondHighestStatValue = critComparisonValue;
                            secondHighestStat = "criticalDamage";
                        }
                        break;

                    default:
                        break;
                }
            }

            // Apply Name
            int randomIndex = Random.Range(0, handsArmorNames.Length);
            newHandsArmor.itemName = GenerateName(highestStat, secondHighestStat, handsArmorNames[randomIndex]);
            newHandsArmor.name = "HandsArmorItem_" + newHandsArmor.handsArmorModelID.ToString() + "_" + newHandsArmor.level.ToString();

            if (shopInventory != null)
            {
                shopInventory.Add(newHandsArmor);
                return;
            }
            else if (directToInventory)
            {
                // Directly to inventory (occurs when Item is Quest Reward)
                playerInventory.AddToInventory(newHandsArmor);
                AcquiredNotifications.instance.NewItemNotification(newHandsArmor, newHandsArmor.count);
                return;
            }

            // Drop Item
            GameObject spawnedObject = Instantiate(itemDropPrefab, position, Quaternion.Euler(-90f, 0f, 0f));
            spawnedObject.GetComponent<ItemDrop>().item = newHandsArmor;
            spawnedObject.GetComponent<ItemDrop>().SetColors(newHandsArmor.rarity);

            spawnedObject.GetComponent<ItemDrop>().SetForcesAndApply(fromChest);
        }

        // GENERATE LEGS ARMOR
        public void GenerateLegsArmor(int enemyLevel, int playerLevel, Vector3 position, string rarity, int minimumRarity, bool fromChest, List<LegsArmorItem> shopInventory = null, bool directToInventory = false)
        {
            // Used for Naming Item
            // Initialize highest and second highest stats
            int highestStatValue = 0;
            int secondHighestStatValue = 0;
            string highestStat = "";
            string secondHighestStat = "";

            // Create Item
            LegsArmorItem newLegsArmor = ScriptableObject.CreateInstance<LegsArmorItem>();

            // Calculate Item Level
            int averageLevel = (enemyLevel + playerLevel) / 2;
            int minLevel = averageLevel - 2;
            if (minLevel < 1) minLevel = 1;
            int effectiveLevel = Random.Range(minLevel, averageLevel + 3);
            newLegsArmor.level = effectiveLevel;

            // Determine Item Model/Type
            int armorType = Random.Range(0, 2);
            //Debug.Log("Armor Type: " + armorType);
            newLegsArmor.legsArmorModelID = armorType;
            newLegsArmor.itemIcon = PlayerInventory.instance.legsArmorIcons[newLegsArmor.legsArmorModelID];

            // Calculate Rarity
            newLegsArmor.rarity = GetRandomRarity(rarity, minimumRarity);

            // Miscellaneous Information
            newLegsArmor.itemDescription = ""; //unused by armor
            newLegsArmor.itemID = 0;
            newLegsArmor.sellable = true;
            newLegsArmor.goldValue = CalculateValue(newLegsArmor.rarity, newLegsArmor.level);
            newLegsArmor.count = 1;

            // Calculate Damage
            int totalArmor = Random.Range(newLegsArmor.level + 9, newLegsArmor.level + 14);
            float rarityMultiplier = 1.0f + (newLegsArmor.rarity + 1.0f) * 0.05f;
            totalArmor = Mathf.RoundToInt(totalArmor * rarityMultiplier);
            int physicalArmor = SplitValue(totalArmor);
            int magicalArmor = totalArmor - physicalArmor;
            if (magicalArmor < 1) magicalArmor = 1;
            newLegsArmor.physicalArmorRating = physicalArmor;
            newLegsArmor.magicalArmorRating = magicalArmor;

            //SET RANDOM STATS
            int[] numbers = PlayerInventory.instance.GenerateDistinctRandomNumbers(newLegsArmor.rarity, 0, 7); //8 possible stats. rarity 4(legendary) = 4 stats
            foreach (int num in numbers)
            {
                switch (num)
                {
                    case 0:
                        newLegsArmor.endurance = GenerateStatValue(effectiveLevel);
                        if (newLegsArmor.endurance >= highestStatValue)
                        {
                            secondHighestStatValue = highestStatValue;
                            highestStatValue = newLegsArmor.endurance;
                            secondHighestStat = highestStat;
                            highestStat = "endurance";
                        }
                        else if (newLegsArmor.endurance > secondHighestStatValue)
                        {
                            secondHighestStatValue = newLegsArmor.endurance;
                            secondHighestStat = "endurance";
                        }
                        break;

                    case 1:
                        newLegsArmor.vitality = GenerateStatValue(effectiveLevel);
                        if (newLegsArmor.vitality >= highestStatValue)
                        {
                            secondHighestStatValue = highestStatValue;
                            highestStatValue = newLegsArmor.vitality;
                            secondHighestStat = highestStat;
                            highestStat = "vitality";
                        }
                        else if (newLegsArmor.vitality > secondHighestStatValue)
                        {
                            secondHighestStatValue = newLegsArmor.vitality;
                            secondHighestStat = "vitality";
                        }
                        break;

                    case 2:
                        newLegsArmor.dexterity = GenerateStatValue(effectiveLevel);
                        if (newLegsArmor.dexterity >= highestStatValue)
                        {
                            secondHighestStatValue = highestStatValue;
                            highestStatValue = newLegsArmor.dexterity;
                            secondHighestStat = highestStat;
                            highestStat = "dexterity";
                        }
                        else if (newLegsArmor.dexterity > secondHighestStatValue)
                        {
                            secondHighestStatValue = newLegsArmor.dexterity;
                            secondHighestStat = "dexterity";
                        }
                        break;

                    case 3:
                        newLegsArmor.expertise = GenerateStatValue(effectiveLevel);
                        if (newLegsArmor.expertise >= highestStatValue)
                        {
                            secondHighestStatValue = highestStatValue;
                            highestStatValue = newLegsArmor.expertise;
                            secondHighestStat = highestStat;
                            highestStat = "expertise";
                        }
                        else if (newLegsArmor.expertise > secondHighestStatValue)
                        {
                            secondHighestStatValue = newLegsArmor.expertise;
                            secondHighestStat = "expertise";
                        }
                        break;


                    case 4:
                        newLegsArmor.willpower = GenerateStatValue(effectiveLevel);
                        if (newLegsArmor.willpower >= highestStatValue)
                        {
                            secondHighestStatValue = highestStatValue;
                            highestStatValue = newLegsArmor.willpower;
                            secondHighestStat = highestStat;
                            highestStat = "willpower";
                        }
                        else if (newLegsArmor.willpower > secondHighestStatValue)
                        {
                            secondHighestStatValue = newLegsArmor.willpower;
                            secondHighestStat = "willpower";
                        }                      
                        break;

                    case 5:
                        float minFireRes = 0.1f + (effectiveLevel - 1) * 0.1f;
                        float maxFireRes = 10.0f + (effectiveLevel - 1) * 0.15f;
                        newLegsArmor.fireResistance = Random.Range(minFireRes, maxFireRes);
                        newLegsArmor.fireResistance = Mathf.Ceil(newLegsArmor.fireResistance * 10f) / 10f;
                        int fireResComparisonValue = Mathf.CeilToInt(newLegsArmor.fireResistance);

                        //Debug.Log("Fire Res Compare: " + fireResComparisonValue);
                        if (fireResComparisonValue >= highestStatValue)
                        {
                            secondHighestStatValue = highestStatValue;
                            highestStatValue = fireResComparisonValue;
                            secondHighestStat = highestStat;
                            highestStat = "fireResistance";
                        }
                        else if (fireResComparisonValue > secondHighestStatValue)
                        {
                            secondHighestStatValue = fireResComparisonValue;
                            secondHighestStat = "fireResistance";
                        }
                        break;

                    case 6:
                        float minIceRes = 0.1f + (effectiveLevel - 1) * 0.1f;
                        float maxIceRes = 10.0f + (effectiveLevel - 1) * 0.15f;
                        newLegsArmor.iceResistance = Random.Range(minIceRes, maxIceRes);
                        newLegsArmor.iceResistance = Mathf.Ceil(newLegsArmor.iceResistance * 10f) / 10f;
                        int iceResComparisonValue = Mathf.CeilToInt(newLegsArmor.iceResistance);

                        //Debug.Log("Ice Res Compare: " + iceResComparisonValue);
                        if (iceResComparisonValue >= highestStatValue)
                        {
                            secondHighestStatValue = highestStatValue;
                            highestStatValue = iceResComparisonValue;
                            secondHighestStat = highestStat;
                            highestStat = "iceResistance";
                        }
                        else if (iceResComparisonValue > secondHighestStatValue)
                        {
                            secondHighestStatValue = iceResComparisonValue;
                            secondHighestStat = "iceResistance";
                        }
                        break;

                    case 7:
                        float minShockRes = 0.1f + (effectiveLevel - 1) * 0.1f;
                        float maxShockRes = 10.0f + (effectiveLevel - 1) * 0.15f;
                        newLegsArmor.shockResistance = Random.Range(minShockRes, maxShockRes);
                        newLegsArmor.shockResistance = Mathf.Ceil(newLegsArmor.shockResistance * 10f) / 10f;
                        int shockResComparisonValue = Mathf.CeilToInt(newLegsArmor.shockResistance);

                        //Debug.Log("Shock Res Compare: " + shockResComparisonValue);
                        if (shockResComparisonValue >= highestStatValue)
                        {
                            secondHighestStatValue = highestStatValue;
                            highestStatValue = shockResComparisonValue;
                            secondHighestStat = highestStat;
                            highestStat = "shockResistance";
                        }
                        else if (shockResComparisonValue > secondHighestStatValue)
                        {
                            secondHighestStatValue = shockResComparisonValue;
                            secondHighestStat = "shockResistance";
                        }
                        break;

                    default:
                        break;
                }
            }

            // Apply Name
            int randomIndex = Random.Range(0, legsArmorNames.Length);
            newLegsArmor.itemName = GenerateName(highestStat, secondHighestStat, legsArmorNames[randomIndex]);
            newLegsArmor.name = "LegsArmorItem_" + newLegsArmor.legsArmorModelID.ToString() + "_" + newLegsArmor.level.ToString();

            if (shopInventory != null)
            {
                shopInventory.Add(newLegsArmor);
                return;
            }
            else if (directToInventory)
            {
                // Directly to inventory (occurs when Item is Quest Reward)
                playerInventory.AddToInventory(newLegsArmor);
                AcquiredNotifications.instance.NewItemNotification(newLegsArmor, newLegsArmor.count);
                return;
            }

            // Drop Item
            GameObject spawnedObject = Instantiate(itemDropPrefab, position, Quaternion.Euler(-90f, 0f, 0f));
            spawnedObject.GetComponent<ItemDrop>().item = newLegsArmor;
            spawnedObject.GetComponent<ItemDrop>().SetColors(newLegsArmor.rarity);

            spawnedObject.GetComponent<ItemDrop>().SetForcesAndApply(fromChest);
        }

        // GENERATE RING
        public void GenerateRing(int enemyLevel, int playerLevel, Vector3 position, string rarity, int minimumRarity, bool fromChest, List<RingItem> shopInventory = null, bool directToInventory = false)
        {
            // Used for Naming Item
            // Initialize highest and second highest stats
            int highestStatValue = 0;
            int secondHighestStatValue = 0;
            string highestStat = "";
            string secondHighestStat = "";

            // Create Item
            RingItem newRing = ScriptableObject.CreateInstance<RingItem>();

            // Calculate Item Level
            int averageLevel = (enemyLevel + playerLevel) / 2;
            int minLevel = averageLevel - 2;
            if (minLevel < 1) minLevel = 1;
            int effectiveLevel = Random.Range(minLevel, averageLevel + 3);
            newRing.level = effectiveLevel;

            // Calculate Rarity
            newRing.rarity = GetRandomRarity(rarity, minimumRarity);

            //LEGENDARY LOGIC
            int legendaryCounter = 0;
            if (newRing.rarity == 4)
            {
                newRing.legendaryStat = Random.Range(0, 4); //0,1,2,3
            }

            // Miscellaneous Information
            newRing.itemDescription = ""; //unused by shields
            newRing.itemID = 0;
            newRing.sellable = true;
            newRing.goldValue = CalculateValue(newRing.rarity, newRing.level * 2);
            newRing.count = 1;

            //SET RANDOM STATS
            int[] numbers = PlayerInventory.instance.GenerateDistinctRandomNumbers(newRing.rarity, 0, 14); //15 possible stats. rarity 4(legendary) = 4 stats
            foreach (int num in numbers)
            {
                switch (num)
                {
                    case 0:
                        newRing.strength = GenerateStatValue(effectiveLevel);
                        if (legendaryCounter == newRing.legendaryStat)
                            newRing.strength *= 2;
                        legendaryCounter++;
                        if (newRing.strength >= highestStatValue)
                        {
                            secondHighestStatValue = highestStatValue;
                            highestStatValue = newRing.strength;
                            secondHighestStat = highestStat;
                            highestStat = "strength";
                        }
                        else if (newRing.strength > secondHighestStatValue)
                        {
                            secondHighestStatValue = newRing.strength;
                            secondHighestStat = "strength";
                        }
                        break;

                    case 1:
                        newRing.endurance = GenerateStatValue(effectiveLevel);
                        if (legendaryCounter == newRing.legendaryStat)
                            newRing.endurance *= 2;
                        legendaryCounter++;
                        if (newRing.endurance >= highestStatValue)
                        {
                            secondHighestStatValue = highestStatValue;
                            highestStatValue = newRing.endurance;
                            secondHighestStat = highestStat;
                            highestStat = "endurance";
                        }
                        else if (newRing.endurance > secondHighestStatValue)
                        {
                            secondHighestStatValue = newRing.endurance;
                            secondHighestStat = "endurance";
                        }
                        break;

                    case 2:
                        newRing.vitality = GenerateStatValue(effectiveLevel);
                        if (legendaryCounter == newRing.legendaryStat)
                            newRing.vitality *= 2;
                        legendaryCounter++;
                        if (newRing.vitality >= highestStatValue)
                        {
                            secondHighestStatValue = highestStatValue;
                            highestStatValue = newRing.vitality;
                            secondHighestStat = highestStat;
                            highestStat = "vitality";
                        }
                        else if (newRing.vitality > secondHighestStatValue)
                        {
                            secondHighestStatValue = newRing.vitality;
                            secondHighestStat = "vitality";
                        }
                        break;

                    case 3:
                        newRing.precision = GenerateStatValue(effectiveLevel);
                        if (legendaryCounter == newRing.legendaryStat)
                            newRing.precision *= 2;
                        legendaryCounter++;
                        if (newRing.precision >= highestStatValue)
                        {
                            secondHighestStatValue = highestStatValue;
                            highestStatValue = newRing.precision;
                            secondHighestStat = highestStat;
                            highestStat = "precision";
                        }
                        else if (newRing.precision > secondHighestStatValue)
                        {
                            secondHighestStatValue = newRing.precision;
                            secondHighestStat = "precision";
                        }
                        break;

                    case 4:
                        newRing.dexterity = GenerateStatValue(effectiveLevel);
                        if (legendaryCounter == newRing.legendaryStat)
                            newRing.dexterity *= 2;
                        legendaryCounter++;
                        if (newRing.dexterity >= highestStatValue)
                        {
                            secondHighestStatValue = highestStatValue;
                            highestStatValue = newRing.dexterity;
                            secondHighestStat = highestStat;
                            highestStat = "dexterity";
                        }
                        else if (newRing.dexterity > secondHighestStatValue)
                        {
                            secondHighestStatValue = newRing.dexterity;
                            secondHighestStat = "dexterity";
                        }
                        break;

                    case 5:
                        newRing.expertise = GenerateStatValue(effectiveLevel);
                        if (legendaryCounter == newRing.legendaryStat)
                            newRing.expertise *= 2;
                        legendaryCounter++;
                        if (newRing.expertise >= highestStatValue)
                        {
                            secondHighestStatValue = highestStatValue;
                            highestStatValue = newRing.expertise;
                            secondHighestStat = highestStat;
                            highestStat = "expertise";
                        }
                        else if (newRing.expertise > secondHighestStatValue)
                        {
                            secondHighestStatValue = newRing.expertise;
                            secondHighestStat = "expertise";
                        }
                        break;

                    case 6:
                        newRing.intelligence = GenerateStatValue(effectiveLevel);
                        if (legendaryCounter == newRing.legendaryStat)
                            newRing.intelligence *= 2;
                        legendaryCounter++;
                        if (newRing.intelligence >= highestStatValue)
                        {
                            secondHighestStatValue = highestStatValue;
                            highestStatValue = newRing.intelligence;
                            secondHighestStat = highestStat;
                            highestStat = "intelligence";
                        }
                        else if (newRing.intelligence > secondHighestStatValue)
                        {
                            secondHighestStatValue = newRing.intelligence;
                            secondHighestStat = "intelligence";
                        }
                        break;

                    case 7:
                        newRing.spirit = GenerateStatValue(effectiveLevel);
                        if (legendaryCounter == newRing.legendaryStat)
                            newRing.spirit *= 2;
                        legendaryCounter++;
                        if (newRing.spirit >= highestStatValue)
                        {
                            secondHighestStatValue = highestStatValue;
                            highestStatValue = newRing.spirit;
                            secondHighestStat = highestStat;
                            highestStat = "spirit";
                        }
                        else if (newRing.spirit > secondHighestStatValue)
                        {
                            secondHighestStatValue = newRing.spirit;
                            secondHighestStat = "spirit";
                        }
                        break;

                    case 8:
                        newRing.willpower = GenerateStatValue(effectiveLevel);
                        if (legendaryCounter == newRing.legendaryStat)
                            newRing.willpower *= 2;
                        legendaryCounter++;
                        if (newRing.willpower >= highestStatValue)
                        {
                            secondHighestStatValue = highestStatValue;
                            highestStatValue = newRing.willpower;
                            secondHighestStat = highestStat;
                            highestStat = "willpower";
                        }
                        else if (newRing.willpower > secondHighestStatValue)
                        {
                            secondHighestStatValue = newRing.willpower;
                            secondHighestStat = "willpower";
                        }
                        break;

                    case 9:
                        newRing.luck = GenerateStatValue(effectiveLevel);
                        if (legendaryCounter == newRing.legendaryStat)
                            newRing.luck *= 2;
                        legendaryCounter++;
                        if (newRing.luck >= highestStatValue)
                        {
                            secondHighestStatValue = highestStatValue;
                            highestStatValue = newRing.luck;
                            secondHighestStat = highestStat;
                            highestStat = "luck";
                        }
                        else if (newRing.luck > secondHighestStatValue)
                        {
                            secondHighestStatValue = newRing.luck;
                            secondHighestStat = "luck";
                        }
                        break;

                    case 10:
                        float legendaryMultiplier1 = 1f;
                        if (legendaryCounter == newRing.legendaryStat)
                            legendaryMultiplier1 = 3f;
                        newRing.criticalChance = GenerateCritValue(effectiveLevel, isCritRate: true, legendaryMult: legendaryMultiplier1);
                        legendaryCounter++;
                        int critComparisonValue2 = Mathf.RoundToInt(newRing.criticalChance * 5);
                        //Debug.Log("Crit Chance Compare: " + critComparisonValue2);
                        if (critComparisonValue2 >= highestStatValue)
                        {
                            secondHighestStatValue = highestStatValue;
                            highestStatValue = critComparisonValue2;
                            secondHighestStat = highestStat;
                            highestStat = "criticalChance";
                        }
                        else if (critComparisonValue2 > secondHighestStatValue)
                        {
                            secondHighestStatValue = critComparisonValue2;
                            secondHighestStat = "criticalChance";
                        }
                        break;

                    case 11:
                        float legendaryMultiplier2 = 1f;
                        if (legendaryCounter == newRing.legendaryStat)
                            legendaryMultiplier2 = 3f;
                        newRing.criticalDamage = GenerateCritValue(effectiveLevel, isCritRate: false, legendaryMult: legendaryMultiplier2);
                        legendaryCounter++;
                        int critComparisonValue = Mathf.RoundToInt(newRing.criticalDamage * 2);
                        //Debug.Log("Crit Dmg Compare: " + critComparisonValue);
                        if (critComparisonValue >= highestStatValue)
                        {
                            secondHighestStatValue = highestStatValue;
                            highestStatValue = critComparisonValue;
                            secondHighestStat = highestStat;
                            highestStat = "criticalDamage";
                        }
                        else if (critComparisonValue > secondHighestStatValue)
                        {
                            secondHighestStatValue = critComparisonValue;
                            secondHighestStat = "criticalDamage";
                        }
                        break;

                    case 12:
                        float minFireRes = 0.1f + (effectiveLevel - 1) * 0.1f;
                        float maxFireRes = 10.0f + (effectiveLevel - 1) * 0.15f;
                        newRing.fireResistance = Random.Range(minFireRes, maxFireRes);
                        if (legendaryCounter == newRing.legendaryStat)
                            newRing.fireResistance *= 3;
                        newRing.fireResistance = Mathf.Ceil(newRing.fireResistance * 10f) / 10f;
                        legendaryCounter++;
                        int fireResComparisonValue = Mathf.CeilToInt(newRing.fireResistance);

                        //Debug.Log("Fire Res Compare: " + fireResComparisonValue);
                        if (fireResComparisonValue >= highestStatValue)
                        {
                            secondHighestStatValue = highestStatValue;
                            highestStatValue = fireResComparisonValue;
                            secondHighestStat = highestStat;
                            highestStat = "fireResistance";
                        }
                        else if (fireResComparisonValue > secondHighestStatValue)
                        {
                            secondHighestStatValue = fireResComparisonValue;
                            secondHighestStat = "fireResistance";
                        }
                        break;

                    case 13:
                        float minIceRes = 0.1f + (effectiveLevel - 1) * 0.1f;
                        float maxIceRes = 10.0f + (effectiveLevel - 1) * 0.15f;
                        newRing.iceResistance = Random.Range(minIceRes, maxIceRes);
                        if (legendaryCounter == newRing.legendaryStat)
                            newRing.iceResistance *= 3;
                        newRing.iceResistance = Mathf.Ceil(newRing.iceResistance * 10f) / 10f;
                        legendaryCounter++;
                        int iceResComparisonValue = Mathf.CeilToInt(newRing.iceResistance);

                        //Debug.Log("Ice Res Compare: " + iceResComparisonValue);
                        if (iceResComparisonValue >= highestStatValue)
                        {
                            secondHighestStatValue = highestStatValue;
                            highestStatValue = iceResComparisonValue;
                            secondHighestStat = highestStat;
                            highestStat = "iceResistance";
                        }
                        else if (iceResComparisonValue > secondHighestStatValue)
                        {
                            secondHighestStatValue = iceResComparisonValue;
                            secondHighestStat = "iceResistance";
                        }
                        break;

                    case 14:
                        float minShockRes = 0.1f + (effectiveLevel - 1) * 0.1f;
                        float maxShockRes = 10.0f + (effectiveLevel - 1) * 0.15f;
                        newRing.shockResistance = Random.Range(minShockRes, maxShockRes);
                        if (legendaryCounter == newRing.legendaryStat)
                            newRing.shockResistance *= 3;
                        newRing.shockResistance = Mathf.Ceil(newRing.shockResistance * 10f) / 10f;
                        legendaryCounter++;
                        int shockResComparisonValue = Mathf.CeilToInt(newRing.shockResistance);

                        //Debug.Log("Shock Res Compare: " + shockResComparisonValue);
                        if (shockResComparisonValue >= highestStatValue)
                        {
                            secondHighestStatValue = highestStatValue;
                            highestStatValue = shockResComparisonValue;
                            secondHighestStat = highestStat;
                            highestStat = "shockResistance";
                        }
                        else if (shockResComparisonValue > secondHighestStatValue)
                        {
                            secondHighestStatValue = shockResComparisonValue;
                            secondHighestStat = "shockResistance";
                        }
                        break;

                    default:
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

            newRing.itemIcon = PlayerInventory.instance.ringIcons[newRing.ringModelID];

            // Apply Name
            int randomIndex = Random.Range(0, ringNames.Length);
            newRing.itemName = GenerateName(highestStat, secondHighestStat, ringNames[randomIndex]);
            newRing.name = "RingItem_" + newRing.ringModelID.ToString() + "_" + newRing.level.ToString();

            if (shopInventory != null)
            {
                shopInventory.Add(newRing);
                return;
            }
            else if (directToInventory)
            {
                // Directly to inventory (occurs when Item is Quest Reward)
                playerInventory.AddToInventory(newRing);
                AcquiredNotifications.instance.NewItemNotification(newRing, newRing.count);
                return;
            }

            // Drop Item
            GameObject spawnedObject = Instantiate(itemDropPrefab, position, Quaternion.Euler(-90f, 0f, 0f));
            spawnedObject.GetComponent<ItemDrop>().item = newRing;
            spawnedObject.GetComponent<ItemDrop>().SetColors(newRing.rarity);

            spawnedObject.GetComponent<ItemDrop>().SetForcesAndApply(fromChest);
        }

        // GENERATE AMULET
        public void GenerateAmulet(int enemyLevel, int playerLevel, Vector3 position, string rarity, int minimumRarity, bool fromChest, List<AmuletItem> shopInventory = null, bool directToInventory = false)
        {
            // Used for Naming Item
            // Initialize highest and second highest stats
            int highestStatValue = 0;
            int secondHighestStatValue = 0;
            string highestStat = "";
            string secondHighestStat = "";

            // Create Item
            AmuletItem newAmulet = ScriptableObject.CreateInstance<AmuletItem>();

            // Calculate Item Level
            int averageLevel = (enemyLevel + playerLevel) / 2;
            int minLevel = averageLevel - 2;
            if (minLevel < 1) minLevel = 1;
            int effectiveLevel = Random.Range(minLevel, averageLevel + 3);
            newAmulet.level = effectiveLevel;

            // Calculate Rarity
            newAmulet.rarity = GetRandomRarity(rarity, minimumRarity);


            //LEGENDARY LOGIC
            int legendaryCounter = 0;
            if (newAmulet.rarity == 4)
            {
                newAmulet.legendaryStat = Random.Range(0, 4); //0,1,2,3
            }

            // Miscellaneous Information
            newAmulet.itemDescription = ""; //unused by shields
            newAmulet.itemID = 0;
            newAmulet.sellable = true;
            newAmulet.goldValue = CalculateValue(newAmulet.rarity, newAmulet.level * 2);
            newAmulet.count = 1;

            //SET RANDOM STATS
            int[] numbers = PlayerInventory.instance.GenerateDistinctRandomNumbers(newAmulet.rarity + 1, 0, 4); //rarity = number of stats, the last value should equal the last case.. case4->thenlast arg is 4
            foreach (int num in numbers)
            {
                switch (num)
                {                
                    case 0:
                        float legendaryMultiplier1 = 1f;
                        if (legendaryCounter == newAmulet.legendaryStat)
                            legendaryMultiplier1 = 4f;
                        newAmulet.criticalChance = GenerateCritValue(effectiveLevel, isCritRate: true, legendaryMult: legendaryMultiplier1);
                        legendaryCounter++;
                        int critComparisonValue2 = Mathf.RoundToInt(newAmulet.criticalChance * 5);
                        //Debug.Log("Crit Chance Compare: " + critComparisonValue2);
                        if (critComparisonValue2 >= highestStatValue)
                        {
                            secondHighestStatValue = highestStatValue;
                            highestStatValue = critComparisonValue2;
                            secondHighestStat = highestStat;
                            highestStat = "criticalChance";
                        }
                        else if (critComparisonValue2 > secondHighestStatValue)
                        {
                            secondHighestStatValue = critComparisonValue2;
                            secondHighestStat = "criticalChance";
                        }
                        break;

                    case 1:
                        float legendaryMultiplier2 = 1f;
                        if (legendaryCounter == newAmulet.legendaryStat)
                            legendaryMultiplier2 = 4f;
                        newAmulet.criticalDamage = GenerateCritValue(effectiveLevel, isCritRate: false, legendaryMult: legendaryMultiplier2);
                        legendaryCounter++;
                        int critComparisonValue = Mathf.RoundToInt(newAmulet.criticalDamage * 2);
                        //Debug.Log("Crit Dmg Compare: " + critComparisonValue);
                        if (critComparisonValue >= highestStatValue)
                        {
                            secondHighestStatValue = highestStatValue;
                            highestStatValue = critComparisonValue;
                            secondHighestStat = highestStat;
                            highestStat = "criticalDamage";
                        }
                        else if (critComparisonValue > secondHighestStatValue)
                        {
                            secondHighestStatValue = critComparisonValue;
                            secondHighestStat = "criticalDamage";
                        }
                        break;

                    case 2:
                        float minFireRes = 0.1f + (effectiveLevel - 1) * 0.1f;
                        float maxFireRes = 10.0f + (effectiveLevel - 1) * 0.15f;
                        newAmulet.fireResistance = Random.Range(minFireRes, maxFireRes);
                        if (legendaryCounter == newAmulet.legendaryStat)
                            newAmulet.fireResistance *= 4;
                        newAmulet.fireResistance = Mathf.Ceil(newAmulet.fireResistance * 10f) / 10f;
                        legendaryCounter++;
                        int fireResComparisonValue = Mathf.CeilToInt(newAmulet.fireResistance);

                        //Debug.Log("Fire Res Compare: " + fireResComparisonValue);
                        if (fireResComparisonValue >= highestStatValue)
                        {
                            secondHighestStatValue = highestStatValue;
                            highestStatValue = fireResComparisonValue;
                            secondHighestStat = highestStat;
                            highestStat = "fireResistance";
                        }
                        else if (fireResComparisonValue > secondHighestStatValue)
                        {
                            secondHighestStatValue = fireResComparisonValue;
                            secondHighestStat = "fireResistance";
                        }
                        break;

                    case 3:
                        float minIceRes = 0.1f + (effectiveLevel - 1) * 0.1f;
                        float maxIceRes = 10.0f + (effectiveLevel - 1) * 0.15f;
                        newAmulet.iceResistance = Random.Range(minIceRes, maxIceRes);
                        if (legendaryCounter == newAmulet.legendaryStat)
                            newAmulet.iceResistance *= 4;
                        newAmulet.iceResistance = Mathf.Ceil(newAmulet.iceResistance * 10f) / 10f;
                        legendaryCounter++;
                        int iceResComparisonValue = Mathf.CeilToInt(newAmulet.iceResistance);

                        //Debug.Log("Ice Res Compare: " + iceResComparisonValue);
                        if (iceResComparisonValue >= highestStatValue)
                        {
                            secondHighestStatValue = highestStatValue;
                            highestStatValue = iceResComparisonValue;
                            secondHighestStat = highestStat;
                            highestStat = "iceResistance";
                        }
                        else if (iceResComparisonValue > secondHighestStatValue)
                        {
                            secondHighestStatValue = iceResComparisonValue;
                            secondHighestStat = "iceResistance";
                        }
                        break;

                    case 4:
                        float minShockRes = 0.1f + (effectiveLevel - 1) * 0.1f;
                        float maxShockRes = 10.0f + (effectiveLevel - 1) * 0.15f;
                        newAmulet.shockResistance = Random.Range(minShockRes, maxShockRes);
                        if (legendaryCounter == newAmulet.legendaryStat)
                            newAmulet.shockResistance *= 4;
                        newAmulet.shockResistance = Mathf.Ceil(newAmulet.shockResistance * 10f) / 10f;
                        legendaryCounter++;
                        int shockResComparisonValue = Mathf.CeilToInt(newAmulet.shockResistance);

                        //Debug.Log("Shock Res Compare: " + shockResComparisonValue);
                        if (shockResComparisonValue >= highestStatValue)
                        {
                            secondHighestStatValue = highestStatValue;
                            highestStatValue = shockResComparisonValue;
                            secondHighestStat = highestStat;
                            highestStat = "shockResistance";
                        }
                        else if (shockResComparisonValue > secondHighestStatValue)
                        {
                            secondHighestStatValue = shockResComparisonValue;
                            secondHighestStat = "shockResistance";
                        }
                        break;

                    default:
                        break;
                }
            }

            //DETERMINE ICON DEPENDING ON RARITY: can modify later to assign a specific icon based on stats
            switch (newAmulet.rarity)
            {
                case 0:
                    newAmulet.amuletModelID = Random.Range(0, 3); //0, 1, or 2
                    break;
                case 1:
                    newAmulet.amuletModelID = Random.Range(0, 3); //same as above
                    break;
                case 2:
                    newAmulet.amuletModelID = Random.Range(3, 7); //3, 4, 5, or 6
                    break;
                case 3:
                    newAmulet.amuletModelID = Random.Range(7, 11); //7, 6, 9, or 10
                    break;
                case 4:
                    newAmulet.amuletModelID = Random.Range(11, 15); //11, 12, 13, or 14
                    break;
                default:
                    break;
            }

            newAmulet.itemIcon = PlayerInventory.instance.amuletIcons[newAmulet.amuletModelID];

            // Apply Name
            int randomIndex = Random.Range(0, amuletNames.Length);
            newAmulet.itemName = GenerateName(highestStat, secondHighestStat, amuletNames[randomIndex]);
            newAmulet.name = "AmuletItem_" + newAmulet.amuletModelID.ToString() + "_" + newAmulet.level.ToString();

            if (shopInventory != null)
            {
                shopInventory.Add(newAmulet);
                return;
            }
            else if(directToInventory)
            {
                // Directly to inventory (occurs when Item is Quest Reward)
                playerInventory.AddToInventory(newAmulet);
                AcquiredNotifications.instance.NewItemNotification(newAmulet, newAmulet.count);
                return;
            }

            // Drop Item
            GameObject spawnedObject = Instantiate(itemDropPrefab, position, Quaternion.Euler(-90f, 0f, 0f));
            spawnedObject.GetComponent<ItemDrop>().item = newAmulet;
            spawnedObject.GetComponent<ItemDrop>().SetColors(newAmulet.rarity);

            spawnedObject.GetComponent<ItemDrop>().SetForcesAndApply(fromChest);
        }

        // GENERATE CONSUMABLE
        public void GenerateConsumable(int consumableID, int count, Vector3 position, bool fromChest, List<Consumable> shopInventory = null)
        {
            Consumable newConsumable = ScriptableObject.CreateInstance<Consumable>();

            if (consumableID == 0)
            {
                newConsumable.itemName = "Minor Healing Potion";
                newConsumable.affectedStat = "health";
                newConsumable.potency = 30.0f;
                newConsumable.itemIcon = PlayerInventory.instance.consumableIcons[consumableID];
                newConsumable.itemDescription = "Restores " + newConsumable.potency.ToString("F0") + " Health.";
                newConsumable.sellable = true;
                newConsumable.rarity = 0;
                newConsumable.goldValue = 15;
                newConsumable.consumable_ID = consumableID;
                newConsumable.count = count;
            }
            else if (consumableID == 1)
            {
                newConsumable.itemName = "Minor Mana Potion";
                newConsumable.affectedStat = "mana";
                newConsumable.potency = 30.0f;
                newConsumable.itemIcon = PlayerInventory.instance.consumableIcons[consumableID];
                newConsumable.itemDescription = "Restores " + newConsumable.potency.ToString("F0") + " Mana.";
                newConsumable.sellable = true;
                newConsumable.rarity = 0;
                newConsumable.goldValue = 15;
                newConsumable.consumable_ID = consumableID;
                newConsumable.count = count;
            }
            else if (consumableID == 2)
            {
                newConsumable.itemName = "Minor Stamina Potion";
                newConsumable.affectedStat = "stamina";
                newConsumable.potency = 30.0f;
                newConsumable.itemIcon = PlayerInventory.instance.consumableIcons[consumableID];
                newConsumable.itemDescription = "Restores " + newConsumable.potency.ToString("F0") + " Stamina.";
                newConsumable.sellable = true;
                newConsumable.rarity = 0;
                newConsumable.goldValue = 15;
                newConsumable.consumable_ID = consumableID;
                newConsumable.count = count;
            }
            else if (consumableID == 3)
            {
                newConsumable.itemName = "Healing Potion";
                newConsumable.affectedStat = "health";
                newConsumable.potency = 60.0f;
                newConsumable.itemIcon = PlayerInventory.instance.consumableIcons[consumableID];
                newConsumable.itemDescription = "Restores " + newConsumable.potency.ToString("F0") + " Health.";
                newConsumable.sellable = true;
                newConsumable.rarity = 1;
                newConsumable.goldValue = 50;
                newConsumable.consumable_ID = consumableID;
                newConsumable.count = count;
            }
            else if (consumableID == 4)
            {
                newConsumable.itemName = "Mana Potion";
                newConsumable.affectedStat = "mana";
                newConsumable.potency = 60.0f;
                newConsumable.itemIcon = PlayerInventory.instance.consumableIcons[consumableID];
                newConsumable.itemDescription = "Restores " + newConsumable.potency.ToString("F0") + " Mana.";
                newConsumable.sellable = true;
                newConsumable.rarity = 1;
                newConsumable.goldValue = 45;
                newConsumable.consumable_ID = consumableID;
                newConsumable.count = count;
            }
            else if (consumableID == 5)
            {
                newConsumable.itemName = "Stamina Potion";
                newConsumable.affectedStat = "stamina";
                newConsumable.potency = 60.0f;
                newConsumable.itemIcon = PlayerInventory.instance.consumableIcons[consumableID];
                newConsumable.itemDescription = "Restores " + newConsumable.potency.ToString("F0") + " Stamina.";
                newConsumable.sellable = true;
                newConsumable.rarity = 1;
                newConsumable.goldValue = 40;
                newConsumable.consumable_ID = consumableID;
                newConsumable.count = count;
            }
            else if (consumableID == 6)
            {
                newConsumable.itemName = "Major Healing Potion";
                newConsumable.affectedStat = "health";
                newConsumable.potency = 100.0f;
                newConsumable.itemIcon = PlayerInventory.instance.consumableIcons[consumableID];
                newConsumable.itemDescription = "Restores " + newConsumable.potency.ToString("F0") + " Health.";
                newConsumable.sellable = true;
                newConsumable.rarity = 2;
                newConsumable.goldValue = 120;
                newConsumable.consumable_ID = consumableID;
                newConsumable.count = count;
            }
            else if (consumableID == 7)
            {
                newConsumable.itemName = "Major Mana Potion";
                newConsumable.affectedStat = "mana";
                newConsumable.potency = 100.0f;
                newConsumable.itemIcon = PlayerInventory.instance.consumableIcons[consumableID];
                newConsumable.itemDescription = "Restores " + newConsumable.potency.ToString("F0") + " Mana.";
                newConsumable.sellable = true;
                newConsumable.rarity = 2;
                newConsumable.goldValue = 110;
                newConsumable.consumable_ID = consumableID;
                newConsumable.count = count;
            }
            else if (consumableID == 8)
            {
                newConsumable.itemName = "Major Stamina Potion";
                newConsumable.affectedStat = "stamina";
                newConsumable.potency = 100.0f;
                newConsumable.itemIcon = PlayerInventory.instance.consumableIcons[consumableID];
                newConsumable.itemDescription = "Restores " + newConsumable.potency.ToString("F0") + " Stamina.";
                newConsumable.sellable = true;
                newConsumable.rarity = 2;
                newConsumable.goldValue = 90;
                newConsumable.consumable_ID = consumableID;
                newConsumable.count = count;
            }
            else if (consumableID == 9)
            {
                newConsumable.itemName = "Ultimate Healing Potion";
                newConsumable.affectedStat = "health";
                newConsumable.potency = 10000.0f;
                newConsumable.itemIcon = PlayerInventory.instance.consumableIcons[consumableID];
                newConsumable.itemDescription = "Restores " + newConsumable.potency.ToString("F0") + " Health.";
                newConsumable.sellable = true;
                newConsumable.rarity = 3;
                newConsumable.goldValue = 500;
                newConsumable.consumable_ID = consumableID;
                newConsumable.count = count;
            }
            else if (consumableID == 10)
            {
                newConsumable.itemName = "Ultimate Mana Potion";
                newConsumable.affectedStat = "mana";
                newConsumable.potency = 10000.0f;
                newConsumable.itemIcon = PlayerInventory.instance.consumableIcons[consumableID];
                newConsumable.itemDescription = "Restores " + newConsumable.potency.ToString("F0") + " Mana.";
                newConsumable.sellable = true;
                newConsumable.rarity = 3;
                newConsumable.goldValue = 480;
                newConsumable.consumable_ID = consumableID;
                newConsumable.count = count;
            }
            else if (consumableID == 11)
            {
                newConsumable.itemName = "Ultimate Stamina Potion";
                newConsumable.affectedStat = "stamina";
                newConsumable.potency = 10000.0f;
                newConsumable.itemIcon = PlayerInventory.instance.consumableIcons[consumableID];
                newConsumable.itemDescription = "Restores " + newConsumable.potency.ToString("F0") + " Stamina.";
                newConsumable.sellable = true;
                newConsumable.rarity = 3;
                newConsumable.goldValue = 350;
                newConsumable.consumable_ID = consumableID;
                newConsumable.count = count;
            }
            else if (consumableID == 12)
            {
                newConsumable.itemName = "Pure Elixir";
                newConsumable.affectedStat = "all";
                newConsumable.potency = 10000.0f;
                newConsumable.itemIcon = PlayerInventory.instance.consumableIcons[consumableID];
                newConsumable.itemDescription = "Fully restores health, mana and stamina.";
                newConsumable.sellable = true;
                newConsumable.rarity = 4;
                newConsumable.goldValue = 700;
                newConsumable.consumable_ID = consumableID;
                newConsumable.count = count;
            }

            if (shopInventory != null)
            {
                int found = -1;
                for (int i = 0; i < shopInventory.Count; i++)
                {
                    if (shopInventory[i].consumable_ID == newConsumable.consumable_ID)
                    {
                        found = i;
                        i = shopInventory.Count;
                    }
                }

                if (found > -1)
                {
                    shopInventory[found].count += newConsumable.count;
                }
                else
                {
                    shopInventory.Add(newConsumable);
                }
                
                return;
            }

            // Drop Item
            GameObject spawnedObject = Instantiate(itemDropPrefab, position, Quaternion.Euler(-90f, 0f, 0f));
            spawnedObject.GetComponent<ItemDrop>().item = newConsumable;
            spawnedObject.GetComponent<ItemDrop>().SetColors(newConsumable.rarity / 2);

            spawnedObject.GetComponent<ItemDrop>().SetForcesAndApply(fromChest);
        }

        public int GenerateStatValue(int effectiveLv)
        {
            return Random.Range(1, Mathf.CeilToInt((effectiveLv + 1f) / gearStatMultiplier) + 1);
        }

        public float GenerateCritValue(int effectiveLv, bool isCritRate = false, float legendaryMult = 1f)
        {
            float t = effectiveLv / 100f;

            if (isCritRate)
            {
                // Designer knobs
                float critRateMin_Lv1 = 0.1f;
                float critRateMax_Lv1 = 0.5f;

                float critRateMin_Lv100 = 10f;
                float critRateMax_Lv100 = 25f;

                // LERP scaling
                float minCritRate = Mathf.Lerp(critRateMin_Lv1, critRateMin_Lv100, t);
                float maxCritRate = Mathf.Lerp(critRateMax_Lv1, critRateMax_Lv100, t);

                float criticalChance = Random.Range(minCritRate, maxCritRate) * legendaryMult;
                return Mathf.Ceil(criticalChance * 10f) / 10f;
            }
            else
            {
                // Designer knobs
                float critDmgMin_Lv1 = 1f;
                float critDmgMax_Lv1 = 2f;

                float critDmgMin_Lv100 = 25f;
                float critDmgMax_Lv100 = 50f;

                // LERP scaling
                float minCritDamage = Mathf.Lerp(critDmgMin_Lv1, critDmgMin_Lv100, t);
                float maxCritDamage = Mathf.Lerp(critDmgMax_Lv1, critDmgMax_Lv100, t);

                float criticalDamage = Random.Range(minCritDamage, maxCritDamage) * legendaryMult;
                return Mathf.Ceil(criticalDamage * 10f) / 10f;
            }
        }

    }
}
