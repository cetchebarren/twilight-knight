using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace etchebarren
{
    public class SpellsHUDManager : MonoBehaviour
    {
        public static SpellsHUDManager instance;
        public ControllerUIManager controllerUIManager;
        public PlayerInventory playerInventory;
        public InputHandler inputHandler;
        public PlayerMenuManager playerMenuManager;
        public PlayerStats playerStats;
        public UIChangeSelectedButton uIChangeSelectedButton;
        public OptionsManager optionsManager;

        [Header("Configuration")]
        public WaitForSeconds waitTime = new WaitForSeconds(0.3f);

        //CHOOSE SPELL
        [Header("Settings For Swapping Spells")]
        public GameObject spellSlotsGroup;
        public HorizontalLayoutGroup spellSlotsHorizontalLayoutGroup;
        public GameObject spellSlotWindow;
        public Image spell1select;
        public Image spell2select;
        public Image spell3select;
        public Image spell4select;

        public Image spell5select;
        public Image spell6select;
        public Image spell7select;
        public Image spell8select;

        public Spell selectedSpell;

        public Image[] spellButtons;

        public GameObject[] buttonParents;

        public TextMeshProUGUI[] buttonText;

        public GameObject[] lockedIcons;

        [Header("Settings For Changing To/From Controller UI")]
        public GameObject spellSlotParent;
        public GameObject spellSlotParent2;
        public GameObject nameBox;
        public GameObject controllerBackground;
        public Image swapSpellBoxRoot;
        public GameObject closeButton;

        //SPELL HUD
        [Header("Settings For Spell HUD")]
        public Image[] spellImages;
        public Image[] menuAndKeyboardSpellImages; // These are the buttons that will appear WITHIN spells HUD 1 in the menu and with PC control scheme only

        [SerializeField] private GameObject activeSpellsHUD;
        [SerializeField] private GameObject inactiveSpellsHUD;
        private Coroutine checkSpellMana;
        public GameObject spellsHUD;
        public GameObject spellsHUD2;
        public GameObject[] buttons;
        public GameObject[] menuAndKeyboardButtons; // These are the buttons that will appear WITHIN spells HUD 1 in the menu and with PC control scheme only
        public GameObject[] allSpellHUD1Buttons;
        public GameObject[] allSpellHUD2Buttons;
        public TextMeshProUGUI[] spellHUDtexts;
        public GameObject leftTriggerIcon;
        public Image[] buttonSprites;
        public GameObject[] unlockableSpellSlotSelectedBorders;
        public GameObject spellsContent; // Used to reset selected button after equipping a spell      

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

        private void Start()
        {
            ResetCheckManaCoroutine();
        }

        public void SwitchSpellMenu()
        {
            // If second spell HUD is not yet unlocked, disable display for it
            bool secondSpellsHUD = playerInventory.equippedSpells[4] != null
                || playerInventory.equippedSpells[5] != null
                || playerInventory.equippedSpells[6] != null
                || playerInventory.equippedSpells[7] != null;
            if (!secondSpellsHUD)
            {
                activeSpellsHUD = spellsHUD;
                inactiveSpellsHUD = spellsHUD2;
                return;
            }

            if(PrimarySpellHUDisActive())
            {
                activeSpellsHUD = spellsHUD2;
                inactiveSpellsHUD = spellsHUD;
            }
            else
            {
                activeSpellsHUD = spellsHUD;
                inactiveSpellsHUD = spellsHUD2;
            }
            UpdateSpellHUD();
        }

        public bool PrimarySpellHUDisActive()
        {
            return (activeSpellsHUD == spellsHUD);
        }

        void OnEnable()
        {
            bool controller = controllerUIManager.isUsingController();

            SpellMenuActive(false, controller);
            UpdateSpellHUD();
            ResetCheckManaCoroutine();
        }

        void OnDisable()
        {
            StopManaCheck();
        }

        public void StopManaCheck()
        {
            if(checkSpellMana != null)
                StopCoroutine(checkSpellMana);
        }

        public void SpellMenuActive(bool opened, bool controller)
        {
            if(opened)
            {
                if(controller)
                {
                    leftTriggerIcon.SetActive(false);
                    activeSpellsHUD.transform.localScale = new Vector3(0.9f, 0.9f, 0.9f);
                    inactiveSpellsHUD.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
                }
            }
            else
            {
                if(controller)
                {
                    leftTriggerIcon.SetActive(true);
                    activeSpellsHUD.transform.localScale = new Vector3(0.8f, 0.8f, 0.8f);
                    inactiveSpellsHUD.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
                }
            }
        }

        public void UpdateSpellHUD()
        {
            int spellSlot = 1;
            bool controller = controllerUIManager.isUsingController();

            if (controller && !inputHandler.playerMenuOpen)
            {
                foreach (GameObject button in menuAndKeyboardButtons)
                {
                    button.SetActive(false);
                }
                // If second spell HUD is not yet unlocked, disable display for it
                bool secondSpellsHUD = playerInventory.equippedSpells[4] != null 
                    || playerInventory.equippedSpells[5] != null 
                    || playerInventory.equippedSpells[6] != null 
                    || playerInventory.equippedSpells[7] != null;
                if (!secondSpellsHUD)
                {
                    activeSpellsHUD = spellsHUD;
                    inactiveSpellsHUD = spellsHUD2;
                    inactiveSpellsHUD.SetActive(false);
                }
                else
                {
                    inactiveSpellsHUD.SetActive(true);
                }

                activeSpellsHUD.SetActive(true);

                SpellMenuActive(inputHandler.openSpells, true);

                for (int i = 0; i < 4; i++)
                {
                    allSpellHUD1Buttons[i].SetActive(true);
                    allSpellHUD2Buttons[i].SetActive(true);
                }

                activeSpellsHUD.GetComponent<GridLayoutGroup>().constraintCount = 2;
                activeSpellsHUD.GetComponent<GridLayoutGroup>().spacing = new Vector2(25.0f, 25.0f);
                activeSpellsHUD.transform.rotation = Quaternion.Euler(0.0f, 0.0f, -45.0f);

                inactiveSpellsHUD.GetComponent<GridLayoutGroup>().constraintCount = 2;
                inactiveSpellsHUD.GetComponent<GridLayoutGroup>().spacing = new Vector2(25.0f, 25.0f);
                inactiveSpellsHUD.transform.rotation = Quaternion.Euler(0.0f, 0.0f, -45.0f);

                //Debug.Log(spellsHUD.GetComponent<RectTransform>().anchoredPosition);
                activeSpellsHUD.GetComponent<RectTransform>().anchoredPosition = new Vector2(-261.0f, 351.0f);
                inactiveSpellsHUD.GetComponent<RectTransform>().anchoredPosition = new Vector2(-43.0f, 320.0f);

                leftTriggerIcon.SetActive(true);
                foreach(TextMeshProUGUI text in spellHUDtexts)
                {
                    text.text = "";
                }
                foreach(GameObject button in buttons)
                {
                    button.SetActive(false);
                }

                // Update button icons to spell icons, 45 degree variants
                for (int i = 0; i < spellImages.Length; i++)
                {
                    if (playerInventory.equippedSpells[i] != null)
                    {

                        spellImages[i].sprite = playerInventory.spellIcons_45deg[playerInventory.equippedSpells[i].spell_ID];
                    }
                    else
                    {
                        spellImages[i].sprite = null;
                    }

                    spellImages[i].enabled = (spellImages[i].sprite != null);
                }


            }
            else // PC Controls or In Menu Spells HUD Modification
            {
                activeSpellsHUD = spellsHUD;
                inactiveSpellsHUD = spellsHUD2;
                foreach (GameObject button in menuAndKeyboardButtons)
                {
                    button.SetActive(true);
                }
                activeSpellsHUD.SetActive(true);
                inactiveSpellsHUD.SetActive(false);

                activeSpellsHUD.GetComponent<GridLayoutGroup>().constraintCount = 8;
                activeSpellsHUD.GetComponent<GridLayoutGroup>().spacing = new Vector2(26.0f, 26.0f);
                activeSpellsHUD.transform.rotation = Quaternion.Euler(0.0f, 0.0f, 0.0f);

                leftTriggerIcon.SetActive(false);

                if (inputHandler.playerMenuOpen)
                {
                    // Menu Spells HUD position/scale
                    activeSpellsHUD.GetComponent<RectTransform>().anchoredPosition = new Vector3(-480f, 56f, 0f);
                    activeSpellsHUD.transform.localScale = new Vector3(0.93f, 0.93f, 0.93f);
                }
                else
                {
                    // Gameplay Spells HUD Position/scale
                    activeSpellsHUD.GetComponent<RectTransform>().anchoredPosition = new Vector3(-358f, -30f, 0f);
                    activeSpellsHUD.transform.localScale = new Vector3(0.85f, 0.85f, 0.85f);
                }

                foreach (TextMeshProUGUI text in spellHUDtexts)
                {
                    if (!controller)
                    {
                        text.text = spellSlot.ToString();
                        spellSlot++;
                    }

                }
                foreach (GameObject button in buttons)
                {
                    if(controller)
                    {
                        if (controllerUIManager.controllerTypePlaystation)
                        {
                            buttonSprites[0].sprite = controllerUIManager.triangle;
                            buttonSprites[2].sprite = controllerUIManager.square;
                            buttonSprites[1].sprite = controllerUIManager.O;
                            buttonSprites[3].sprite = controllerUIManager.X;
                            buttonSprites[4].sprite = controllerUIManager.triangle;
                            buttonSprites[6].sprite = controllerUIManager.square;
                            buttonSprites[5].sprite = controllerUIManager.O;
                            buttonSprites[7].sprite = controllerUIManager.X;
                            buttonSprites[8].sprite = controllerUIManager.triangle;
                            buttonSprites[10].sprite = controllerUIManager.square;
                            buttonSprites[9].sprite = controllerUIManager.O;
                            buttonSprites[11].sprite = controllerUIManager.X;
                        }
                        else
                        {
                            buttonSprites[0].sprite = controllerUIManager.Y;
                            buttonSprites[2].sprite = controllerUIManager.xboxX;
                            buttonSprites[1].sprite = controllerUIManager.B;
                            buttonSprites[3].sprite = controllerUIManager.A;
                            buttonSprites[4].sprite = controllerUIManager.Y;
                            buttonSprites[6].sprite = controllerUIManager.xboxX;
                            buttonSprites[5].sprite = controllerUIManager.B;
                            buttonSprites[7].sprite = controllerUIManager.A;
                            buttonSprites[8].sprite = controllerUIManager.Y;
                            buttonSprites[10].sprite = controllerUIManager.xboxX;
                            buttonSprites[9].sprite = controllerUIManager.B;
                            buttonSprites[11].sprite = controllerUIManager.A;
                        }
                        button.SetActive(true);
                    }
                    else
                    {
                        buttonSprites[0].sprite = controllerUIManager.computerKey;
                        buttonSprites[2].sprite = controllerUIManager.computerKey;
                        buttonSprites[1].sprite = controllerUIManager.computerKey;
                        buttonSprites[3].sprite = controllerUIManager.computerKey;
                        buttonSprites[4].sprite = controllerUIManager.computerKey;
                        buttonSprites[6].sprite = controllerUIManager.computerKey;
                        buttonSprites[5].sprite = controllerUIManager.computerKey;
                        buttonSprites[7].sprite = controllerUIManager.computerKey;
                        buttonSprites[8].sprite = controllerUIManager.computerKey;
                        buttonSprites[10].sprite = controllerUIManager.computerKey;
                        buttonSprites[9].sprite = controllerUIManager.computerKey;
                        buttonSprites[11].sprite = controllerUIManager.computerKey;
                    }
                    button.SetActive(true);

                }

                // Update button icons to spell icons, original variants
                for (int i = 0; i < spellImages.Length; i++)
                {
                    spellImages[i].sprite = playerInventory.equippedSpells[i] != null ? playerInventory.equippedSpells[i].itemIcon : null;
                    spellImages[i].enabled = (spellImages[i].sprite != null);
                    allSpellHUD1Buttons[i].SetActive(spellImages[i].sprite != null);
                }

                for(int i = 0; i < 4; i++)
                {
                    menuAndKeyboardSpellImages[i].sprite = playerInventory.equippedSpells[i+4] != null ? playerInventory.equippedSpells[i+4].itemIcon : null;
                    menuAndKeyboardButtons[i].SetActive(menuAndKeyboardSpellImages[i].sprite != null);
                }
            }


            for(int i = 0; i < buttons.Length; i++)
            {
                buttons[i].transform.rotation = Quaternion.Euler(0.0f, 0.0f, 0.0f);
            }

            ResetCheckManaCoroutine();

        }

        public void OpenSelectSpellSlotScreen()
        {
            int slotIndex = 1;

            bool controller = controllerUIManager.isUsingController();

            if (controller)
            {
                controllerUIManager.SetBackText("Back");
                //Handle Background
                controllerBackground.SetActive(true);
                swapSpellBoxRoot.enabled = false;
                spellSlotsHorizontalLayoutGroup.spacing = -340f;
                spellSlotsGroup.transform.localPosition = new Vector3(-431f, -83f, 0f);
                nameBox.transform.localPosition = new Vector3(-34f, 140f, 0f);
                closeButton.transform.localPosition = new Vector3(345.8f, 138.7f, 0f);

                spellSlotParent.GetComponent<GridLayoutGroup>().constraintCount = 2;
                spellSlotParent.GetComponent<GridLayoutGroup>().spacing = new Vector2(30.0f, 30.0f);
                spellSlotParent.transform.rotation = Quaternion.Euler(0.0f, 0.0f, -45.0f);

                spellSlotParent2.GetComponent<GridLayoutGroup>().constraintCount = 2;
                spellSlotParent2.GetComponent<GridLayoutGroup>().spacing = new Vector2(30.0f, 30.0f);
                spellSlotParent2.transform.rotation = Quaternion.Euler(0.0f, 0.0f, -45.0f);

                foreach (GameObject parent in buttonParents)
                {
                    parent.transform.rotation = Quaternion.Euler(0.0f, 0.0f, 0.0f);
                    //Debug.Log(parent.transform.localPosition);
                    parent.transform.localPosition = new Vector3(-36.5f, 35.5f, 0.0f);
                    parent.transform.GetChild(0).transform.localScale = new Vector3(1.1f, 1.1f, 1.1f);
                }
                foreach (Image image in spellButtons)
                {

                    switch (slotIndex)
                    {
                        case 1:
                        case 5:
                            if (controllerUIManager.controllerTypePlaystation)
                            {
                                image.sprite = controllerUIManager.triangle;
                            }
                            else
                            {
                                image.sprite = controllerUIManager.Y;
                            }
                            break;
                        case 2:
                        case 6:
                            if (controllerUIManager.controllerTypePlaystation)
                            {
                                image.sprite = controllerUIManager.O;
                            }
                            else
                            {
                                image.sprite = controllerUIManager.B;
                            }
                            break;
                        case 3:
                        case 7:
                            if (controllerUIManager.controllerTypePlaystation)
                            {
                                image.sprite = controllerUIManager.square;
                            }
                            else
                            {
                                image.sprite = controllerUIManager.xboxX;
                            }
                            break;
                        case 4:
                        case 8:
                            if (controllerUIManager.controllerTypePlaystation)
                            {
                                image.sprite = controllerUIManager.X;
                            }
                            else
                            {
                                image.sprite = controllerUIManager.A;
                            }
                            break;
                        default:
                            break;
                    }
                    slotIndex++;
                }
                foreach (TextMeshProUGUI text in buttonText)
                {
                    text.text = "";
                }
                //Debug.Log(spellSlotParent.GetComponent<RectTransform>().anchoredPosition);
                spellSlotParent.GetComponent<RectTransform>().anchoredPosition = new Vector2(156.0f, -190.0f);
                spellSlotParent2.GetComponent<RectTransform>().anchoredPosition = new Vector2(156.0f, -190.0f);
            }
            else
            {
                //Handle Background
                controllerBackground.SetActive(false);
                swapSpellBoxRoot.enabled = true;
                spellSlotsGroup.transform.localPosition = new Vector3(-601f, 85f, 0f);
                spellSlotsHorizontalLayoutGroup.spacing = -90f;
                nameBox.transform.localPosition = new Vector3(0f, 107f, 0f);
                closeButton.transform.localPosition = new Vector3(642f, 97f, 0f);

                spellSlotParent.GetComponent<GridLayoutGroup>().constraintCount = 4;
                spellSlotParent.GetComponent<GridLayoutGroup>().spacing = new Vector2(50.0f, 50.0f);
                spellSlotParent.transform.rotation = Quaternion.Euler(0.0f, 0.0f, 0.0f);

                spellSlotParent2.GetComponent<GridLayoutGroup>().constraintCount = 4;
                spellSlotParent2.GetComponent<GridLayoutGroup>().spacing = new Vector2(50.0f, 50.0f);
                spellSlotParent2.transform.rotation = Quaternion.Euler(0.0f, 0.0f, 0.0f);

                foreach (GameObject parent in buttonParents)
                {
                    parent.transform.rotation = Quaternion.Euler(0.0f, 0.0f, 0.0f);
                    parent.transform.GetChild(0).transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
                    parent.transform.localPosition = new Vector3(-32.6f, 34.0f, 0.0f);
                }
                foreach (Image image in spellButtons)
                {
                    image.sprite = controllerUIManager.computerKey;
                }
                foreach (TextMeshProUGUI text in buttonText)
                {
                    text.text = slotIndex.ToString();
                    slotIndex++;
                }
                spellSlotParent.GetComponent<RectTransform>().anchoredPosition = new Vector2(-0.05f, -10.35f);
                spellSlotParent2.GetComponent<RectTransform>().anchoredPosition = new Vector2(-0.05f, -10.35f);
            }

            spellSlotWindow.SetActive(true);

            if(controller)
            {
                spell1select.sprite = playerInventory.equippedSpells[0] != null ? playerInventory.spellIcons_45deg[playerInventory.equippedSpells[0].spell_ID] : null;
                spell2select.sprite = playerInventory.equippedSpells[2] != null ? playerInventory.spellIcons_45deg[playerInventory.equippedSpells[2].spell_ID] : null;
                spell3select.sprite = playerInventory.equippedSpells[1] != null ? playerInventory.spellIcons_45deg[playerInventory.equippedSpells[1].spell_ID] : null;
                spell4select.sprite = playerInventory.equippedSpells[3] != null ? playerInventory.spellIcons_45deg[playerInventory.equippedSpells[3].spell_ID] : null;

                spell5select.sprite = playerInventory.equippedSpells[4] != null ? playerInventory.spellIcons_45deg[playerInventory.equippedSpells[4].spell_ID] : null;
                spell6select.sprite = playerInventory.equippedSpells[6] != null ? playerInventory.spellIcons_45deg[playerInventory.equippedSpells[6].spell_ID] : null;
                spell7select.sprite = playerInventory.equippedSpells[5] != null ? playerInventory.spellIcons_45deg[playerInventory.equippedSpells[5].spell_ID] : null;
                spell8select.sprite = playerInventory.equippedSpells[7] != null ? playerInventory.spellIcons_45deg[playerInventory.equippedSpells[7].spell_ID] : null;
            }
            else
            {
                spell1select.sprite = playerInventory.equippedSpells[0] != null ? playerInventory.equippedSpells[0].itemIcon : null;
                spell2select.sprite = playerInventory.equippedSpells[2] != null ? playerInventory.equippedSpells[2].itemIcon : null;
                spell3select.sprite = playerInventory.equippedSpells[1] != null ? playerInventory.equippedSpells[1].itemIcon : null;
                spell4select.sprite = playerInventory.equippedSpells[3] != null ? playerInventory.equippedSpells[3].itemIcon : null;

                spell5select.sprite = playerInventory.equippedSpells[4] != null ? playerInventory.equippedSpells[4].itemIcon : null;
                spell6select.sprite = playerInventory.equippedSpells[6] != null ? playerInventory.equippedSpells[6].itemIcon : null;
                spell7select.sprite = playerInventory.equippedSpells[5] != null ? playerInventory.equippedSpells[5].itemIcon : null;
                spell8select.sprite = playerInventory.equippedSpells[7] != null ? playerInventory.equippedSpells[7].itemIcon : null;
            }

            spell1select.enabled = (spell1select.sprite != null);
            spell2select.enabled = (spell2select.sprite != null);
            spell3select.enabled = (spell3select.sprite != null);
            spell4select.enabled = (spell4select.sprite != null);
            spell5select.enabled = (spell5select.sprite != null);
            spell6select.enabled = (spell6select.sprite != null);
            spell7select.enabled = (spell7select.sprite != null);
            spell8select.enabled = (spell8select.sprite != null);

            // Enable/Disable Locked Icons depending on how many player has unlocked
            for (int i = 0; i < 4; i++)
            {
                if (controller)
                {
                    lockedIcons[i].GetComponent<RectTransform>().localPosition = new Vector3(4.7f, -5.09f, 0f);
                    lockedIcons[i].GetComponent<RectTransform>().localRotation = Quaternion.Euler(0f,0f,45f);
                }
                else
                {
                    lockedIcons[i].GetComponent<RectTransform>().localPosition = new Vector3(3.7f, -7f, 0f);
                    lockedIcons[i].GetComponent<RectTransform>().localRotation = Quaternion.Euler(0f, 0f, 0f);
                }
                int requiredRank = i + 1; //in order to be set to active, [0] requires rank of 1, [1] requires rank of 2, so on
                lockedIcons[i].SetActive(playerStats.GetSkillRankByID(31) < requiredRank);

                //Debug.Log("for i = " + i + " (spellIndex " + (i + 4) + "), required rank equates to " + requiredRank);
                //Debug.Log("In this case, playerStats.skills[31].rank <= requiredRank == " + playerStats.skills[31].rank + " < " + requiredRank + " is " + (playerStats.skills[31].rank < requiredRank));
            }

            controllerUIManager.SetSelectText("Equip");
        }

        public void SelectSpellSlot(int spellIndex)
        {
            // possible spell indices: 0,2,1,3 // 4,6,5,7

            // Determine required rank for that spell index
            // We use a switch intead of a simple calc like int requiredRank = spellIndex < 4 ? 0 : spellIndex - 3; because
            // our spell slots for 2,1 and 6,5 are reversed to allow for better controls sync across keyboard & controller setups
            // It is an added bonus that the switch increases readability at the expense of slightly chunkier code
            int requiredRank;
            switch (spellIndex)
            {
                case 4:
                    requiredRank = 1;
                        break;
                case 5:
                    requiredRank = 3;
                        break;
                case 6:
                    requiredRank = 2;
                        break;
                case 7:
                    requiredRank = 4;
                        break;
                default:
                    requiredRank = 0;
                    break;
            }
            // If rank is not sufficient, do not equip the spell and do nothing
            if (playerStats.GetSkillRankByID(31) < requiredRank) return;

            // Disable the selected borders on all the optional buttons if pressed 
            foreach(GameObject selectedBorder in unlockableSpellSlotSelectedBorders) selectedBorder.SetActive(false);

            if(playerInventory.equippedSpells[spellIndex] != null)
            {
                playerInventory.equippedSpells[spellIndex].equipped = false;
            }

            playerInventory.equippedSpells[spellIndex] = selectedSpell;
            playerInventory.equippedSpells[spellIndex].equipped = true;

            playerMenuManager.UpdateUI();

            spellSlotWindow.SetActive(false);

            UpdateSpellHUD();

            UIAudioManager.instance.PlayEquipSpellAudio();

            uIChangeSelectedButton.ChangeSelectedButtonToFirstChildOf(spellsContent);

            HelpMenu.instance.AddHeaderToQueue("Magic: Status Effects");
            HelpMenu.instance.AddHeaderToQueue("Magic: Elemental Reactions");
        }

        private IEnumerator UpdateWithInterval()
        {
            while (true)
            {
                CheckManaAndSetColor();
                yield return waitTime;
            }
        }

        public void ResetCheckManaCoroutine()
        {
            if (inputHandler.mapMenuOpen || inputHandler.playerMenuOpen) return;
            if (checkSpellMana != null) StopCoroutine(checkSpellMana);
            if (gameObject.activeSelf)
            {
                checkSpellMana = StartCoroutine(UpdateWithInterval());
            }
        }

        public void CheckManaAndSetColor()
        {
            //Debug.Log("Check Mana And Set Color Called");
            for (int i = 0; i < spellImages.Length; i++)
            {
                if (spellImages[i].sprite != null)
                {
                    if (PlayerStats.instance.currentMana >= playerInventory.equippedSpells[i].manaCost)
                    {
                        spellImages[i].color = new Color(1.0f, 1.0f, 1.0f, 1.0f);
                        if(i > 3)
                        {
                            menuAndKeyboardSpellImages[i-4].color = new Color(1.0f, 1.0f, 1.0f, 1.0f);
                        }
                    }
                    else
                    {
                        spellImages[i].color = new Color(0.5f, 0.5f, 0.5f, 0.6f);
                        if (i > 3)
                        {
                            menuAndKeyboardSpellImages[i - 4].color = new Color(0.5f, 0.5f, 0.5f, 0.6f);
                        }
                    }
                }
            }
        }
    }
}