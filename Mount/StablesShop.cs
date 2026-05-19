using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using MalbersAnimations.Utilities;
using BlazeAISpace;

namespace etchebarren
{
    public class StablesShop : MonoBehaviour
    {
        public static StablesShop instance;

        [Header("Scripts")]
        public MountStats mountStats;
        public PlayerInventory playerInventory;
        public MaterialChanger materialChangerMountVer;
        public MaterialChanger materialChangerAIVer;
        public ActiveMeshes activeMeshesMountVer;
        public ActiveMeshes activeMeshesAIVer;
        public InputHandler inputHandler;
        public SetHUD setHUD;
        public BlazeAI horseAI;
        public MountManager mountManager;
        public WhistleIconManager whistleIconManager;
        public UIAudioManager uiAudioManager;
        public CompanionBehaviour companion;

        [Header("Prices")]
        public int coatCost = 25;
        public int maneCost = 50;
        public int saddleCost = 100;
        public int armorCost = 5000;
        public int hornCost = 5000;

        [Header("UI References")]
        public GameObject nameButton;
        public TextMeshProUGUI nameButtonText;
        public TextMeshProUGUI nameInputText;
        public TMP_InputField nameInputField;
        public GameObject inputField;
        public GameObject confirmCoat;
        public GameObject confirmCoatCancel;
        public GameObject confirmMane;
        public GameObject confirmManeCancel;
        public GameObject confirmSaddle;
        public GameObject confirmSaddleCancel;
        public GameObject confirmArmor;
        public GameObject confirmArmorCancel;
        public GameObject confirmHorn;
        public GameObject confirmHornCancel;
        public TextMeshProUGUI textDescription;

        [Header("Saddles")]
        public List<Material> saddles;
        public SkinnedMeshRenderer saddleMountVer;
        public SkinnedMeshRenderer saddleAIVer;

        [Header("Sliders")]
        public Slider coat;
        public float coatValue = 1;
        public Slider mane;
        public float maneValue = 0;
        public Slider saddle;
        public float saddleValue = 0;
        public Slider armor;
        public float armorValue = -1;
        public Slider horn;
        public float hornValue = -1;

        [Header("Elements (to Set Active)")]
        public GameObject goldHUD;

        public GameObject buyCoat;
        public GameObject coatColors;

        public GameObject buyMane;
        public GameObject maneColors;

        public GameObject buySaddle;
        public GameObject saddleStyles;

        public GameObject buyArmor;
        public GameObject armorStyles;

        public GameObject buyHorn;
        public GameObject hornStyles;

        public GameObject mountStatusHUD;

        public Image confirmButton; //just to set the color
        public Color interactableGreen;
        public Color uninteractableGray;

        [Header("Text Mesh Pro")]
        public TextMeshProUGUI horseNameText;

        [Header("Portrait")]
        public Camera playerCamera;      
        public GameObject horseAIVer;
        public Image horsePortrait;

        [Header("Do Not Set in Inspector")]
        public GameObject stables;
        public GameObject spotLight;
        public Camera stablesCamera;
        public PortraitCapture portraitCapture;
        public StablesTrigger currentStablesTrigger;
        public bool playAudio = false;
        public bool wasFollowing = false;

        void Awake()
        {
            if (instance == null)
            {
                instance = this;

                //Modded to disable after creating instance
                this.gameObject.SetActive(false);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        void Start()
        {
            //update textmeshpro prices here if I want to have it automated
        }

        void OnEnable()
        {
            if (stablesCamera != null)
            {
                wasFollowing = companion.follow;
                companion.follow = false;
                spotLight.SetActive(true);
                goldHUD.SetActive(true);
                inputHandler.inMenu = true;
                inputHandler.horizontal = 0;
                inputHandler.vertical = 0;
                inputHandler.moveAmount = 0;
                stablesCamera.enabled = true;
                //playerCamera.enabled = false;
                horseAIVer.SetActive(true);
                horseAI.useRootMotion = false;
                mountManager.StopForceFollow();
                horseAI.friendly = true;
                horseAIVer.transform.position = stables.transform.position;
                horseAIVer.transform.rotation = stables.transform.rotation;
                setHUD.SetHUDActive(false);
                if(playAudio) uiAudioManager.PlayOpenMenuAudio();
                // Set Sliders (Mainly used to set values after loading game, as loading will set the float values in this script, but not affect the sliders)
                //coat.value = coatValue;
                //mane.value = maneValue;
                //saddle.value = saddleValue;
                //armor.value = armorValue;
                //horn.value = hornValue;
                UpdateHorseNameTextInMenu();
            }
        }

        void OnDisable()
        {
            //playerCamera.enabled = true;
            horseAI.useRootMotion = true;
            horseAI.friendly = false;
            companion.follow = wasFollowing;
            inputHandler.inMenu = false;
            if (stablesCamera != null) stablesCamera.enabled = false;
            setHUD.SetHUDActive(true);
            goldHUD.SetActive(false);
            if (spotLight != null) spotLight.SetActive(false);
            if(horseAIVer.activeSelf) mountStatusHUD.SetActive(true);
            if (playAudio) uiAudioManager.PlayCloseMenuAudio();
            else playAudio = true;
            inputHandler.ResetInputs();
        }

        public void NameHorse()
        {
            string nametext = inputField.GetComponent<TMP_InputField>().text.Trim();

            Debug.Log("Horse name saved: " + nametext);

            nameButtonText.text = nametext;

            bool isEmptyOrWhitespace = string.IsNullOrWhiteSpace(nametext);
            if (isEmptyOrWhitespace)
            {
                mountStats.horseName = "Whisper";
            }
            else
            {
                mountStats.horseName = nametext;
            }

            horseNameText.text = mountStats.horseName;
        }

        public void CheckFunds(string type)
        {
            // Prevent opening multiple windows
            if (confirmCoat.activeSelf || confirmMane.activeSelf || confirmSaddle.activeSelf || confirmArmor.activeSelf || confirmHorn.activeSelf)
            {
                return;
            }

            switch (type)
            {
                case "coat":
                    if (playerInventory.goldCount >= coatCost) ConfirmationWindow(type);
                    else TextNotificationsManager.instance.NewTextNotifaction("Insufficient Gold", true); //true prevents duplicate messages for 3 seconds
                    break;
                case "mane":
                    if (playerInventory.goldCount >= maneCost) ConfirmationWindow(type);
                    else TextNotificationsManager.instance.NewTextNotifaction("Insufficient Gold", true); //true prevents duplicate messages for 3 seconds
                    break;
                case "saddle":
                    if (playerInventory.goldCount >= saddleCost) ConfirmationWindow(type);
                    else TextNotificationsManager.instance.NewTextNotifaction("Insufficient Gold", true); //true prevents duplicate messages for 3 seconds
                    break;
                case "armor":
                    if (playerInventory.goldCount >= armorCost) ConfirmationWindow(type);
                    else TextNotificationsManager.instance.NewTextNotifaction("Insufficient Gold", true); //true prevents duplicate messages for 3 seconds
                    break;
                case "horn":
                    if (playerInventory.goldCount >= hornCost) ConfirmationWindow(type);
                    else TextNotificationsManager.instance.NewTextNotifaction("Insufficient Gold", true); //true prevents duplicate messages for 3 seconds
                    break;

                default:
                    Debug.LogError("Unexpected type value in CheckFunds()");
                    break;
            }
        }

        public void ConfirmationWindow(string type)
        {
            switch (type)
            {
                case "coat":
                    confirmCoat.SetActive(true);
                    UIChangeSelectedButton.instance.ChangeSelectedButtonTo(confirmCoatCancel);
                    break;
                case "mane":
                    confirmMane.SetActive(true);
                    UIChangeSelectedButton.instance.ChangeSelectedButtonTo(confirmManeCancel);
                    break;
                case "saddle":
                    confirmSaddle.SetActive(true);
                    UIChangeSelectedButton.instance.ChangeSelectedButtonTo(confirmSaddleCancel);
                    break;
                case "armor":
                    confirmArmor.SetActive(true);
                    UIChangeSelectedButton.instance.ChangeSelectedButtonTo(confirmArmorCancel);
                    break;
                case "horn":
                    confirmHorn.SetActive(true);
                    UIChangeSelectedButton.instance.ChangeSelectedButtonTo(confirmHornCancel);
                    break;
                default:
                    Debug.LogError("Unexpected type value in ConfirmationWindow()");
                    break;
            }
        }

        public void BuyCoat()
        {
            playerInventory.RemoveGold(coatCost);
        }

        public void BuyMane()
        {
            playerInventory.RemoveGold(maneCost);
        }
        public void BuySaddle()
        {
            playerInventory.RemoveGold(saddleCost);
        }

        public void BuyArmor()
        {
            mountStats.horseArmorOwned = true;
            playerInventory.RemoveGold(armorCost);
        }

        public void BuyHorn()
        {
            mountStats.horseHornOwned = true;
            playerInventory.RemoveGold(hornCost);
        }

        public int CurrentCoatIndex()
        {
            return materialChangerAIVer.CurrentMaterialIndex(0);
        }

        public int CurrentManeIndex()
        {

            // This will require checking the Active Meshes
            return materialChangerAIVer.CurrentMaterialIndex(0);
        }

        public void UpdateCoatSlider(bool loading=false)
        {
            if(!loading) PlaySliderSound();
            materialChangerMountVer.SetMaterial(0, (int)coat.value);
            materialChangerAIVer.SetMaterial(0, (int)coat.value);
            coatValue = coat.value;
        }

        public void UpdateManeSlider(bool loading = false)
        {
            if (mane.value == -1)
            {
                if (!loading) PlaySliderSound();
                activeMeshesAIVer.ChangeMesh(2, 0);
                activeMeshesMountVer.ChangeMesh(2, 0);
            }
            else if (mane.value >= 0 && mane.value <= 4)
            {
                if (!loading) PlaySliderSound();
                materialChangerMountVer.SetMaterial(1, (int)mane.value);
                materialChangerAIVer.SetMaterial(1, (int)mane.value);
                activeMeshesAIVer.ChangeMesh(2, 1);
                activeMeshesMountVer.ChangeMesh(2, 1);
            }
            else
            {
                Debug.LogError("Invalid index for horse mane mesh in StablesShop.cs UpdateManeSlider()");
            }
            maneValue = mane.value;
        }

        public void UpdateSaddleSlider(bool loading = false)
        {
            if (!loading) PlaySliderSound();
            saddleAIVer.material = saddles[(int)saddle.value];
            saddleMountVer.material = saddles[(int)saddle.value];
            saddleValue = saddle.value;
        }

        public void UpdateArmorSlider(bool loading = false)
        {
            if (armor.value == -1)
            {
                if (!loading) PlaySliderSound();
                activeMeshesAIVer.ChangeMesh(1, 0);
                activeMeshesMountVer.ChangeMesh(1, 0);
                // Reactivate mane mesh, if using one
                if(mane.value != -1)
                {
                    activeMeshesAIVer.ChangeMesh(2, 1);
                    activeMeshesMountVer.ChangeMesh(2, 1);
                }

            }
            else if (armor.value >= 0 && armor.value <= 2)
            {
                if (!loading) PlaySliderSound();
                materialChangerMountVer.SetMaterial(3, (int)armor.value);
                materialChangerAIVer.SetMaterial(3, (int)armor.value);
                activeMeshesAIVer.ChangeMesh(1, 1);
                activeMeshesMountVer.ChangeMesh(1, 1);
                //Prevent ugly clipping:
                activeMeshesAIVer.ChangeMesh(2, 0);
                activeMeshesMountVer.ChangeMesh(2, 0);
            }
            else
            {
                Debug.LogError("Invalid index for jorse armor mesh in StablesShop.cs UpdateArmorSlider()");
            }
            armorValue = armor.value;
        }

        public void UpdateHornSlider(bool loading = false)
        {
            if (horn.value == -1) // No horn 
            {
                if (!loading) PlaySliderSound();
                activeMeshesAIVer.ChangeMesh(5, 0);
                activeMeshesMountVer.ChangeMesh(5, 0);
            }
            else if (horn.value == 0 || horn.value == 1) // black and white hook horn
            {
                if (!loading) PlaySliderSound();
                materialChangerMountVer.SetMaterial(4, (int)(horn.value%2));
                materialChangerAIVer.SetMaterial(4, (int)(horn.value%2));
                activeMeshesAIVer.ChangeMesh(5, 1);
                activeMeshesMountVer.ChangeMesh(5, 1);
            }
            else if (horn.value == 2 || horn.value == 3) //black and white unicorn horn
            {
                if (!loading) PlaySliderSound();
                materialChangerMountVer.SetMaterial(4, (int)horn.value%2);
                materialChangerAIVer.SetMaterial(4, (int)horn.value%2);
                activeMeshesAIVer.ChangeMesh(5, 2);
                activeMeshesMountVer.ChangeMesh(5, 2);
            }
            else
            {
                Debug.LogError("Invalid index for horse horn mesh in StablesShop.cs UpdateHornSlider(). horn.value was " + horn.value );
            }

            hornValue = horn.value;
        }

        public void PlaySliderSound()
        {
            //play sound here
        }

        public void Confirm()
        {
            // Close all popup windows
            confirmCoat.SetActive(false);
            confirmMane.SetActive(false);
            confirmSaddle.SetActive(false);
            confirmArmor.SetActive(false);
            confirmHorn.SetActive(false);

            // Reset trigger to renable interact prompt
            currentStablesTrigger?.ResetTrigger();

            NameHorse();
            portraitCapture?.Capture();

            // Reenable interact prompt
            inputHandler.interactInput = false;
            InteractPrompt.instance.interactActive = true;
            InteractPrompt.instance.interactContainer.SetActive(true);

            // Make sure the correct whistle icon is active
            whistleIconManager.DetermineAndSetIcon(horseAIVer.activeSelf, mountManager.companionBehaviour.follow);
        }

        public void SetDescriptionText(int type)
        {
            string description = "";
            string percentage;
            switch (type)
            {
                case 0:
                    percentage = (mountStats.horseArmorDamageTakenMultiplier * 100).ToString() + "%";
                    description = "Armor decreases your mount's <i>damage taken</i> by <b> " + percentage + "</b> and will take effect permanently, regardless of armor styles or if the armor is hidden.";
                    break;
                case 1:
                    percentage = ((mountStats.horseHornDamageDealtMultiplier - 1.0f) * 100).ToString() + "%";
                    description = "Horn increases your mount's <i>damage dealt</i> by <b> " + percentage + "</b> and will take effect permanently, regardless of horn styles or if the horn is hidden.";
                    break;
                default:
                    break;
            }
            textDescription.text = description;
        }

        public void SetConfirmButtonColor(int type)
        {
            switch (type)
            {
                case 0:
                    confirmButton.color = uninteractableGray;
                    break;
                case 1:
                    confirmButton.color = interactableGreen;
                    break;
                default:
                    break;
            }
        }

        public void UpdateHorseNameTextInMenu()
        {
            nameInputField.text = mountStats.horseName;
            nameButtonText.text = mountStats.horseName;
            horseNameText.text = mountStats.horseName;
            nameInputText.text = mountStats.horseName;
        }
    }
}
