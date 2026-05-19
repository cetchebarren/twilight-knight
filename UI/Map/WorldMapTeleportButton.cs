using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace etchebarren
{
    public class WorldMapTeleportButton : MonoBehaviour
    {
        [Header("Location Information")]
        public string locationName;
        public int sceneIndex;
        [Header("Script References")]
        public WorldStateManager worldStateManager;
        public MapInputHandler mapInputHandler;

        [SerializeField] private Vector3 originalScale;
        private bool scaleSet = false;

        // Start is called before the first frame update
        void Start()
        {
            if (worldStateManager.worldLocationsUnlocked.Contains(locationName))
            {
                gameObject.SetActive(true);
            }
            else
            {
                gameObject.SetActive(false);
            }

            SetOriginalScale();
        }

        public void OpenConfirmationWindow()
        {
            mapInputHandler.OpenConfirmationWindow(this);
        }

        public void RestoreIconScale()
        {
            if (!scaleSet) SetOriginalScale();

            transform.GetChild(0).localScale = originalScale;
        }

        public void SetIconScale(float mutliplier)
        {
            if (!scaleSet) SetOriginalScale();

            transform.GetChild(0).localScale = originalScale * mutliplier;
        }

        private void SetOriginalScale()
        {
            originalScale = transform.GetChild(0).localScale;
            scaleSet = true;
        }
    }
}
