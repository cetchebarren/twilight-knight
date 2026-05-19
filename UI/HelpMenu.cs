using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

namespace etchebarren
{
    public class HelpMenu : MonoBehaviour
    {
        public static HelpMenu instance;

        [Header("Settings")]
        public bool popups = true; // se via options
        public int startingIndexForTutorials = 3;
        public float minTimeBeforeClose = 3f;

        [System.Serializable]
        public class Tip
        {
            //text
            public string header;
            [TextArea(2, 20)]
            public string body;
            //window info: empty/0 is ignored
            public int windowWidth;
            public int windowHeight;
            //image 1 (optional)
            public Sprite image1sprite;
            public Vector3 image1scale;
            public Vector3 image1pos;
            public Material image1material;
            //image 2 (optional)
            public Sprite image2sprite;
            public Vector3 image2scale;
            public Vector3 image2pos;
            public Material image2material;
            //image 3 (optional)
            public Sprite image3sprite;
            public Vector3 image3scale;
            public Vector3 image3pos;
            public Material image3material;
            //flags (set automatically)
            public bool unlocked;
            public bool showInMenu;
        }

        [System.Serializable]
        public struct ControlString
        {
            public string placeholder;   // e.g. "<Map Input>"
            public string PCstring;
            public string PSstring;
            public string XBOXstring;
        }


        public Tip[] tips;
        public List<string> unlockedTipHeaders = new List<string>();

        private Dictionary<string, Tip> tipDict = new Dictionary<string, Tip>();
        private HashSet<string> unlockedTipHeadersSet = new HashSet<string>();
        public List<string> queuedPopupHeaders = new List<string>();

        [Header("Control Strings")]
        public ControlString[] controls;

        [Header("Element References")]
        public PlayerMenuManager playerMenuManager;
        public RectTransform helpMenuRect;
        public GameObject tutorialLabel;
        public TextMeshProUGUI headerTMP;
        public TextMeshProUGUI bodyTMP;
        public GameObject closeButtonPC;
        public Image image1;
        public Image image2;
        public Image image3;
        public GameObject viewTutorialsListItemText;
        public TMP_SpriteAsset psSprites;
        public TMP_SpriteAsset xboxSprites;
        public TMP_SpriteAsset pcSprites;
        public Image[] closefills;

        [Header("Menu Propterties (player menu)")]
        public Vector3 inMenuPosition = new Vector3(442f, 550f, 0f);
        public Vector2 inMenuAnchorMin = new Vector2(0.5f, 0f);
        public Vector2 inMenuAnchorMax = new Vector2(0.5f, 0f);
        public Vector3 inMenuScale = new Vector3(0.35f, 0.35f, 0.35f);
        public Vector2 inMenuSizeDelta = new Vector2(2292f, 1659f);

        [Header("Menu Propterties (popup)")]
        public Vector3 inGamePosition = new Vector3(0f, 0f, 0f);
        public Vector2 inGameAnchorMin = new Vector2(0.5f, 0.5f);
        public Vector2 inGameAnchorMax = new Vector2(0.5f, 0.5f);
        public Vector3 inGameScale = new Vector3(0.35f, 0.35f, 0.35f);
        public Vector2 inGameSizeDelta = new Vector2(2292f, 1659f);

        // instantiation
        public GameObject listItemPrefab;
        public Transform contentsParent;

        [Header("Misc. Flags")]
        private bool pc = false;
        private bool ps = false;
        private bool xbox = false;
        public bool force_pc = false;
        public bool force_ps = false;
        public bool force_xbox = false;

        [Header("Runtime Values")]
        public float lifetime = 0f;
        private Coroutine lifetimeCoroutine;

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
            if (tips!=null) BuiltTipDict();
            // Set Inactive After Creating Instance and Dict
            helpMenuRect.gameObject.SetActive(false);
            // Enable Fixed Elements
            helpMenuRect.GetComponent<Image>().enabled = true;
            tutorialLabel.SetActive(true);
            headerTMP.gameObject.SetActive(true);
            bodyTMP.gameObject.SetActive(true);
        }

        void Start()
        {
            unlockedTipHeadersSet = new HashSet<string>(unlockedTipHeaders);
            SyncTipsUnlockedStatus();
        }

        void OnDisable()
        {
            StopLifetimeCoroutine();
        }

        private void BuiltTipDict()
        {
            foreach(Tip tip in tips)
            {
                tipDict.Add(tip.header, tip);
            }
            Debug.Log("Tips in Dict: " + tipDict.Count);
        }

        public Tip GetTipByHeader(string header)
        {
            if (tipDict.TryGetValue(header, out Tip value)){
                return value;
            }
            else
            {
                return null;
            }
        }

        public void UnlockTip(string header, bool notification=true)
        {
            Tip activeTip = GetTipByHeader(header);
            // If haven't seen this header before
            if (activeTip != null && !TipAlreadyUnlocked(header))
            {
                unlockedTipHeaders.Add(header);
                unlockedTipHeadersSet.Add(header);
                activeTip.unlocked = true;
                if (activeTip.showInMenu)
                    TextNotificationsManager.instance.NewTextNotifaction("Tutorial \"" + activeTip.header + "\" recorded in menu", true);            
            }
        }

        private bool TipAlreadyUnlocked(string header)
        {
            return unlockedTipHeadersSet.Contains(header);
        }

        public void DisplayFromMenu(string header)
        {
            Display(header, true);
        }

        public void DisplayInGame(string header, bool testing=false)
        {
            if (TipAlreadyUnlocked(header) && !testing) return;
            Display(header, false);
        }

        private void Display(string header, bool fromMenu)
        {
            if (!fromMenu && InputHandler.instance.tutorialPopupActive)
            {
                AddHeaderToQueue(header);
                return;
            }          
            float targetWidth = inMenuSizeDelta.x; 
            float targetHeight = inMenuSizeDelta.y;
            Tip activeTip = GetTipByHeader(header);
            if (activeTip.windowWidth > 0f) targetWidth = activeTip.windowWidth;
            if (activeTip.windowHeight > 0f) targetHeight = activeTip.windowHeight;
            Vector2 targetSizeDelta = new Vector2(targetWidth, targetHeight);

            if (fromMenu)
            {
                closeButtonPC.SetActive(false);
                helpMenuRect.anchoredPosition = inMenuPosition;
                helpMenuRect.sizeDelta = inMenuSizeDelta;
                //helpMenuRect.sizeDelta = targetSizeDelta;
                helpMenuRect.localScale = inMenuScale;
                helpMenuRect.anchorMin = inMenuAnchorMin;
                helpMenuRect.anchorMax = inMenuAnchorMax;
            }
            else if (popups) // tutorial messages enabled in settings
            {
                // instead of closing the plaeyr menu, we want to hide it and keep a record of the last button that was selected
                if (!playerMenuManager.playerMenuWasOpen)
                {
                    if (playerMenuManager.playerMenuWindow.activeSelf)
                    {
                        playerMenuManager.playerMenuWasOpen = true;
                        playerMenuManager.lastSelectedObjectBeforeHelpMenu = EventSystem.current.currentSelectedGameObject;
                        playerMenuManager.playerMenuWindow.SetActive(false);
                    }
                    else
                    {
                        playerMenuManager.playerMenuWasOpen = false;
                        playerMenuManager.lastSelectedObjectBeforeHelpMenu = null;
                    }
                }

                InputHandler.instance.tutorialPopupActive = true;
                Time.timeScale = 0f;
                playerMenuManager.skillTreeManager.Notification("OFF");
                ControllerUIManager.instance.SetAllInactive();
                if (ControllerUIManager.instance.isUsingController())
                {
                    ControllerUIManager.instance.SetBackText("Close Tutorial");
                    closeButtonPC.SetActive(false);
                }
                else
                {
                    Cursor.lockState = CursorLockMode.None;
                    closeButtonPC.SetActive(true);
                }
                helpMenuRect.anchoredPosition = inGamePosition;
                helpMenuRect.sizeDelta = targetSizeDelta;
                helpMenuRect.localScale = inGameScale;
                helpMenuRect.anchorMin = inGameAnchorMin;
                helpMenuRect.anchorMax = inGameAnchorMax;
                
                UIAudioManager.instance.PlayOpenMenuAudio();
            }
            else
            {
                if (header != null) UnlockTip(header);
                return;
            }

            helpMenuRect.gameObject.SetActive(true);
            if (!fromMenu && popups) RestartLifeTimeCoroutine();
            headerTMP.text = activeTip.header;
            string processedText = ReplaceControlPlaceholders(activeTip.body);
            bodyTMP.text = processedText;

            image1.sprite = activeTip.image1sprite;
            image1.GetComponent<RectTransform>().localScale = activeTip.image1scale;
            image1.GetComponent<RectTransform>().localPosition = activeTip.image1pos;
            image1.material = activeTip.image1material;
            image1.gameObject.SetActive(image1.sprite != null);

            image2.sprite = activeTip.image2sprite;
            image2.GetComponent<RectTransform>().localScale = activeTip.image2scale;
            image2.GetComponent<RectTransform>().localPosition = activeTip.image2pos;
            image2.material = activeTip.image2material;
            image2.gameObject.SetActive(image2.sprite != null);

            image3.sprite = activeTip.image3sprite;
            image3.GetComponent<RectTransform>().localScale = activeTip.image3scale;
            image3.GetComponent<RectTransform>().localPosition = activeTip.image3pos;
            image3.material = activeTip.image3material;
            image3.gameObject.SetActive(image3.sprite != null);

            if (header != "Tutorials")
                AddHeaderToQueue("Tutorials");
        }

        public void Close()
        {
            if (InputHandler.instance.tutorialPopupActive && !CanClose())
            {
                return;
            }
            StopLifetimeCoroutine();
            var currentHeader = headerTMP.text;
            if (currentHeader != null) UnlockTip(currentHeader);

            helpMenuRect.gameObject.SetActive(false);

            if (InputHandler.instance.tutorialPopupActive)
            {
                InputHandler.instance.tutorialPopupActive = false;
                ControllerUIManager.instance.BackTextActive(false);
                UIAudioManager.instance.PlayCloseMenuAudio();

                if (queuedPopupHeaders.Count > 0)
                {
                    DisplayQueued();
                }
                else
                {
                    bool showCursor;
                    // If player menu was open when we opened the help menu, resume where we were
                    if (playerMenuManager.playerMenuWasOpen)
                    {
                        // Redisplay player menu & reselect the button prior to help menu display
                        playerMenuManager.playerMenuWindow.SetActive(true);
                        UIChangeSelectedButton.instance.ChangeSelectedButtonTo(playerMenuManager.lastSelectedObjectBeforeHelpMenu);
                        playerMenuManager.playerMenuWasOpen = false;
                        showCursor = !ControllerUIManager.instance.isUsingController();
                    }
                    else
                    {
                        Time.timeScale = 1f;
                        showCursor = false;
                    }
                    playerMenuManager.lastSelectedObjectBeforeMap = null;
                    InputHandler.instance.ResetInputs();

                    // determine cursor mode
                    if (showCursor)
                    {
                        Cursor.lockState = CursorLockMode.None;
                    }
                    else
                    {
                        Cursor.lockState = CursorLockMode.Locked;
                    }

                }
            }
        }

        public void DisplayQueued()
        {
            if (queuedPopupHeaders.Count > 0)
            {
                string firstHeader = queuedPopupHeaders[0];
                queuedPopupHeaders.RemoveAt(0);
                DisplayInGame(firstHeader);
            }
        }

        public void AddHeaderToQueue(string header)
        {
            if (!TipAlreadyUnlocked(header))
            {
                if (popups)
                    queuedPopupHeaders.Add(header);
                else
                    UnlockTip(header);
            }
        }

        private void SyncTipsUnlockedStatus()
        {
            foreach (Tip tip in tips)
            {
                if (unlockedTipHeadersSet.Contains(tip.header))
                {
                    tip.unlocked = true;
                }
            }
        }

        private GameObject GetOrCreate(int index)
        {
            // If the parent already has a child at this index, reuse it
            if (index < contentsParent.childCount)
            {
                GameObject existing = contentsParent.GetChild(index).gameObject;
                existing.SetActive(true);
                return existing;
            }

            // Otherwise instantiate a new one
            GameObject newObj = Instantiate(listItemPrefab, contentsParent);
            return newObj;
        }

        public void InstantiateElementsUI()
        {
            int start = startingIndexForTutorials;
            int elementCount = 0;

            // Filter in the order they appear in the dictionary
            var visibleTips = tipDict.Values
                .Where(t => t.unlocked && t.showInMenu)
                .ToList();

            viewTutorialsListItemText.SetActive(visibleTips.Count > 0);

            for (int i = 0; i < visibleTips.Count; i++)
            {
                int targetIndex = start + i;

                GameObject tipListItem = GetOrCreate(targetIndex);
                tipListItem.transform.GetChild(0)
                    .GetComponent<TextMeshProUGUI>().text = visibleTips[i].header;

                tipListItem.name = visibleTips[i].header;
                tipListItem.SetActive(true);

                elementCount++;
            }

            // Disable unused
            int firstUnusedIndex = start + elementCount;
            for (int i = firstUnusedIndex; i < contentsParent.childCount; i++)
            {
                contentsParent.GetChild(i).gameObject.SetActive(false);
            }
        }

        private string ReplaceControlPlaceholders(string input)
        {
            pc = ps = xbox = false;
            if (ControllerUIManager.instance.isUsingController())
            {
                if (ControllerUIManager.instance.controllerTypePlaystation)
                {
                    ps = true;
                    bodyTMP.spriteAsset = psSprites;
                }
                else
                {
                    xbox = true;
                    bodyTMP.spriteAsset = xboxSprites;
                }
            }
            else
            {
                pc = true;
                bodyTMP.spriteAsset = pcSprites;
            }

            //debuging purposes
            if (force_pc)
            {
                bodyTMP.spriteAsset = pcSprites;
                pc = ps = xbox = false;
                pc = true;
            }
            else if (force_ps)
            {
                pc = ps = xbox = false;
                ps = true;
                bodyTMP.spriteAsset = psSprites;
            }
            else if (force_xbox)
            {
                pc = ps = xbox = false;
                xbox = true;
                bodyTMP.spriteAsset = xboxSprites;
            }              

            foreach (var c in controls)
            {
                string replacement = c.PCstring; // default

                if (ps) replacement = c.PSstring;
                else if (xbox) replacement = c.XBOXstring;
                else if (pc) replacement = c.PCstring;

                if (!string.IsNullOrEmpty(c.placeholder))
                {
                    input = input.Replace(c.placeholder, replacement);
                }
            }

            return input;
        }

        private bool CanClose()
        {
            return lifetime >= minTimeBeforeClose;
        }

        private void RestartLifeTimeCoroutine()
        {
            StopLifetimeCoroutine();
            lifetimeCoroutine = StartCoroutine(LifetimeRoutine());
        }

        private IEnumerator LifetimeRoutine()
        {
            lifetime = 0f;

            while (true)
            {
                lifetime += Time.unscaledDeltaTime;

                float progress = Mathf.Clamp01(lifetime / minTimeBeforeClose);

                foreach (var img in closefills)
                    img.fillAmount = progress;

                yield return null;
            }
        }


        private void StopLifetimeCoroutine()
        {
            if (lifetimeCoroutine != null)
            {
                StopCoroutine(lifetimeCoroutine);
                lifetimeCoroutine = null;
            }

            foreach (var img in closefills)
                img.fillAmount = 1f;
        }

    }
}
