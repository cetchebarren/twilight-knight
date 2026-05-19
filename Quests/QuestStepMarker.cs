using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace etchebarren
{
    public class QuestStepMarker : MonoBehaviour
    {
        public int questStepID;

        public GameObject questWorldIcon;
        public GameObject questMarker;
        public GameObject questSearchArea;

        public SpriteRenderer spriteLocation; // For setting the sprite in MinimapOutOfBoundsIndicator.instance.SetTargetReference(gameObject);

        public bool isSearchArea = false;
        public Sprite searchAreaMinimapSprite;
        public Color searchAreaMinimapSpriteColor;
        public Sprite searchAreaWorldIconSprite;
        public Color searchAreaWorldIconColor;

        public SceneExit sceneExit; //If this is assigned, this quest marker is used for when the object is in a different scene but points to this sceneExit
        public bool dialogueAgentMarker = false;

        public GameObject defaultMinimapIconToHide; // optional: example, gray/white door icon on scene transition

        void OnEnable()
        {
            if(sceneExit == null && !dialogueAgentMarker)
            {
                if (questStepID == -1) Debug.LogError("INVALID QUEST STEP ID FOR QUESTSTEPMARKER:", gameObject);
                // Determine what marker should be active
                if (isSearchArea)
                {
                    questMarker.SetActive(false);
                    questWorldIcon.SetActive(false);
                    questSearchArea.SetActive(true);

                    spriteLocation.sprite = searchAreaMinimapSprite;
                    spriteLocation.color = searchAreaMinimapSpriteColor;
                    questWorldIcon.GetComponent<Image>().sprite = searchAreaWorldIconSprite;
                    questWorldIcon.GetComponent<Image>().color = searchAreaWorldIconColor;
                }
                else
                {
                    questMarker.SetActive(true);
                    questWorldIcon.SetActive(true);
                    questSearchArea.SetActive(false);
                }
            }
        }

        public void SetInactive()
        {
            UntrackMarker();
            gameObject.SetActive(false);
        }

        public void TrackMarker()
        {
            MinimapOutOfBoundsIndicator.instance.SetTargetReference(gameObject, spriteLocation);
            if (!isSearchArea) questMarker.SetActive(true);
            if (defaultMinimapIconToHide != null) defaultMinimapIconToHide.SetActive(true);

        }

        public void UntrackMarker()
        {
            MinimapOutOfBoundsIndicator.instance.UntrackTarget(gameObject);
            if (!isSearchArea) questMarker.SetActive(true);
            if (defaultMinimapIconToHide != null) defaultMinimapIconToHide.SetActive(true);
        }
    }
}
