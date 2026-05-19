using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace etchebarren
{
    public class DigSpot : MonoBehaviour
    {
        [Header("Settings")]
        public MinimapIconType minimapIconType = MinimapIconType.MinimapSecret;
        [Tooltip("Determines whether to standard dirt pile when digging into a digh spot. Default: off")]
        public bool instantiateDirtPile = false; 
        public UnityEvent optionalEventsOnDig;
        public float itemActivationDelay = 1f;
        public bool hideMeshOnStart = true;
        public float treasureSpawnVolume = 0.5f;
        public float worldItemSpawnVolume = 0.2f;

        [Header("Treasures (Set At Runtime)")]
        [SerializeField] private Chest[] treasureChests;
        [SerializeField] private WorldItem[] worldItems;


        [Header("Flags / Data")]
        public AudioSource audioSource;
        public GameObject minimapIcon;
        public Transform buriedItemsParent;
        public bool alreadyDug = false;
        [SerializeField] bool itemListsInit = false;
        public enum MinimapIconType
        {
            MinimapSecret,
            AlwaysDisplay,
            None
        }

        void Start()
        {
            if (hideMeshOnStart) gameObject.GetComponent<MeshRenderer>().enabled = false;
            CacheBuriedItems();
            DeactivateBuriedItems();
            SetMinimapIconType();
        }

        void OnEnable()
        {
            // check treasure items to see if
            bool active = HasActiveBuriedItems();
            if (!active) DisableDigSpot();
        }

        public void DigUpItems()
        {
            // may reapproach this logic, skip the flag and just check if its in the list 
            if (alreadyDug) return;
            alreadyDug = true;

            StartCoroutine(DelayDigUpItems());
        }

        private IEnumerator DelayDigUpItems()
        {
            yield return new WaitForSeconds(itemActivationDelay);
            optionalEventsOnDig.Invoke();
            ActivateBuriedItems();
            DisableDigSpot();
        }

        private void CacheBuriedItems()
        {
            // Get all Chest components in children (including inactive)
            treasureChests = buriedItemsParent.GetComponentsInChildren<Chest>(true);

            // Get all WorldItem components in children (including inactive)
            worldItems = buriedItemsParent.GetComponentsInChildren<WorldItem>(true);

            itemListsInit = true;
        }

        private bool HasActiveBuriedItems()
        {
            if (!itemListsInit) CacheBuriedItems();

            // --- CHESTS ---
            foreach (Chest chest in treasureChests)
            {
                // If the dictionary does NOT contain this chestID -> treat as unopened
                if (!WorldStateManager.instance.chestDictionary.TryGetValue(chest.chestID, out bool isOpen))
                    return true; // no record = unopened = active buried chest

                if (!isOpen)
                    return true; // explicitly unopened
            }

            // --- WORLD ITEMS ---
            foreach (WorldItem worldItem in worldItems)
            {
                // If the ID is NOT in the obtained list -> still active
                if (!WorldStateManager.instance.obtainedWorldItemIDs.Contains(worldItem.worldItemID))
                    return true;
            }

            return false;
        }

        private void ActivateBuriedItems()
        {
            bool chestActivated = false;
            bool worldItemActivated = false;
            // Activate unopened chests
            foreach (Chest chest in treasureChests)
            {
                if (!WorldStateManager.instance.chestDictionary.TryGetValue(chest.chestID, out bool isOpen))
                {
                    chest.gameObject.SetActive(true);
                    chestActivated = true;
                }
                else if (!isOpen)
                {
                    chest.gameObject.SetActive(true);
                    chestActivated = true;
                }
            }

            // Activate uncollected world items
            foreach (WorldItem item in worldItems)
            {
                if (!WorldStateManager.instance.obtainedWorldItemIDs.Contains(item.worldItemID))
                {
                    item.gameObject.SetActive(true);
                    worldItemActivated = true;
                }
            }

            PlayAudio(chestActivated, worldItemActivated);
        }

        private void DeactivateBuriedItems()
        {
            // Disable all chests
            foreach (Chest chest in treasureChests)
                chest.gameObject.SetActive(false);

            // Disable all world items
            foreach (WorldItem item in worldItems)
                item.gameObject.SetActive(false);
        }

        private void DisableDigSpot()
        {
            this.gameObject.SetActive(false);
            alreadyDug = true;
        }

        private void SetMinimapIconType()
        {
            switch (minimapIconType)
            {
                case MinimapIconType.MinimapSecret:
                    minimapIcon.layer = LayerMask.NameToLayer("MinimapSecret");
                    break;
                case MinimapIconType.AlwaysDisplay:
                    minimapIcon.layer = LayerMask.NameToLayer("Minimap");
                    break;
                case MinimapIconType.None:
                    minimapIcon.layer = LayerMask.NameToLayer("UI");
                    minimapIcon.SetActive(false);
                    break;
                default:
                    minimapIcon.layer = LayerMask.NameToLayer("MinimapSecret");
                    break;
            }
        }

        private void PlayAudio(bool chest, bool worldItem)
        {
            if (!chest && !worldItem) return;
            float volume = worldItemSpawnVolume;
            if (chest) volume = treasureSpawnVolume;
            audioSource.volume = volume;
            audioSource.Play();
        }


    }
}
