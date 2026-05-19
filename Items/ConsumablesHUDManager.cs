using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace etchebarren
{
    public class ConsumablesHUDManager : MonoBehaviour
    {
        public static ConsumablesHUDManager instance;

        public Image leftImage;
        public Image centerImage;
        public Image rightImage;

        public TextMeshProUGUI leftText;
        public TextMeshProUGUI centerText;
        public TextMeshProUGUI rightText;

        public Slider leftSlider;
        public Slider centerSlider;
        public Slider rightSlider;

        //Data:
        private List<Consumable> consumablesEquipped;
        public int selectedConsumable;
        public Sprite noItem;

        //Timer:
        public bool canUseConsumable = true;
        public float consumableCooldownTime = 5.0f;
        public float consumableTimer = 0f;

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
            consumablesEquipped = PlayerInventory.instance.equippedConsumables;

            selectedConsumable = 0;

            UpdateConsumablesHUD();
        }

        void OnEnable()
        {
            if (!canUseConsumable)
            {
                UpdateConsumablesHUD();
                StartCoroutine(StartCooldown(true));
            }
        }

        public void UpdateSelectedConsumable(bool increase)
        {
            if(consumablesEquipped.Count > 1)
            {
                if(increase)
                {
                    selectedConsumable = (selectedConsumable + 1) % consumablesEquipped.Count;
                }
                else
                {
                    selectedConsumable = (selectedConsumable - 1 + consumablesEquipped.Count) % consumablesEquipped.Count;
                }

                UpdateConsumablesHUD();
            }
            else
            {
                TextNotificationsManager.instance.NewTextNotifaction("No item to switch to. Equip more");
            }

        }

        public void RemoveEquippedConsumableByID(int IDToRemove)
        {
            consumablesEquipped.RemoveAll(consumable => consumable.consumable_ID == IDToRemove);
            selectedConsumable = 0;
        }


        public void UpdateConsumablesHUD()
        {
            //int selectedConsumable = ConsumablesHUDManager.instance.selectedConsumable;
            int consumablesCount = consumablesEquipped.Count;

            // catch error
            if(selectedConsumable >= consumablesEquipped.Count)
            {
                Debug.Log("Caught consumable selection index error (A)");
                selectedConsumable = 0;
            }
            else 
            {
                if(consumablesEquipped[selectedConsumable] == null)
                {
                    Debug.Log("Caught consumable selection index error (B)");
                    selectedConsumable = 0;
                }
            }

            // Ensure consumablesCount is not zero before calculating indices
            if (consumablesCount > 0)
            {
                // Center
                if (selectedConsumable >= 0 && selectedConsumable < consumablesCount && consumablesEquipped[selectedConsumable] != null)
                {
                    centerImage.sprite = consumablesEquipped[selectedConsumable].itemIcon;
                    centerText.text = "x" + consumablesEquipped[selectedConsumable].count.ToString();
                    if (!consumablesEquipped[selectedConsumable].showCountInConsumableHUD) centerText.text = "";
                    centerImage.enabled = true;
                }
                else
                {
                    centerImage.enabled = false;
                    centerText.text = "";
                }

                // Right
                int rightIndex = (selectedConsumable + 1) % consumablesCount;
                if (rightIndex >= 0 && rightIndex < consumablesCount && consumablesEquipped[rightIndex] != null)
                {
                    rightImage.sprite = consumablesEquipped[rightIndex].itemIcon;
                    rightText.text = "x" + consumablesEquipped[rightIndex].count.ToString();
                    if (!consumablesEquipped[rightIndex].showCountInConsumableHUD) rightText.text = "";
                    rightImage.enabled = true;
                }
                else
                {
                    rightImage.enabled = false;
                    rightText.text = "";
                }

                // Left
                int leftIndex = (selectedConsumable - 1 + consumablesCount) % consumablesCount;
                if (leftIndex >= 0 && leftIndex < consumablesCount && consumablesEquipped[leftIndex] != null)
                {
                    leftImage.sprite = consumablesEquipped[leftIndex].itemIcon;
                    leftText.text = "x" + consumablesEquipped[leftIndex].count.ToString();
                    if (!consumablesEquipped[leftIndex].showCountInConsumableHUD) leftText.text = "";
                    leftImage.enabled = true;
                }
                else
                {
                    leftImage.enabled = false;
                    leftText.text = "";
                }
            }
            else
            {
                centerImage.enabled = false;
                centerText.text = "";
                rightImage.enabled = false;
                rightText.text = "";
                leftImage.enabled = false;
                leftText.text = "";
            }
        }

        public void StartConsumableCooldown()
        {
            UpdateConsumablesHUD();
            StartCoroutine(StartCooldown());
        }

        private IEnumerator StartCooldown(bool resume = false)
        {
            canUseConsumable = false;
            centerSlider.value = 1.0f;
            leftSlider.value = centerSlider.value;
            rightSlider.value = centerSlider.value;

            if(!resume) consumableTimer = 0f;
            float cooldownTime = consumableCooldownTime; // Store the total cooldown time.

            while (consumableTimer < cooldownTime)
            {
                // Update the slider value based on the remaining cooldown time.
                float remainingTime = cooldownTime - consumableTimer;
                centerSlider.value = remainingTime / cooldownTime;
                leftSlider.value = centerSlider.value;
                rightSlider.value = centerSlider.value;

                yield return null;
                consumableTimer += Time.deltaTime;
            }

            centerSlider.value = 0f; // Set the slider to 0 when the cooldown is complete.
            leftSlider.value = 0f;
            rightSlider.value = 0f;

            canUseConsumable = true;
        }

        public bool CanUseConsumable()
        {
            string affectedStat = consumablesEquipped[selectedConsumable].affectedStat;
            bool statsOK = false;

            if (consumablesEquipped[selectedConsumable].count < 1)
            {
                TextNotificationsManager.instance.NewTextNotifaction("Not enough in inventory");
                return false;
            }

            switch(affectedStat)
            {
                case "health":
                    statsOK = PlayerStats.instance.currentHealth < PlayerStats.instance.maxHealth;
                    if(!statsOK) TextNotificationsManager.instance.NewTextNotifaction("Health already full");
                    break;
                case "mana":
                    statsOK = PlayerStats.instance.currentMana < PlayerStats.instance.maxMana;
                    if (!statsOK) TextNotificationsManager.instance.NewTextNotifaction("Mana already full");
                    break;
                case "stamina":
                    statsOK = PlayerStats.instance.currentStamina < PlayerStats.instance.maxStamina;
                    if (!statsOK) TextNotificationsManager.instance.NewTextNotifaction("Stamina already full");
                    break;
                default:
                    statsOK = true;
                    break;
            }

            return (statsOK && canUseConsumable);
        }
    }
}
