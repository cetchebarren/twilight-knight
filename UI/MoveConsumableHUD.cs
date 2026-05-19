using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace etchebarren
{
    public class MoveConsumableHUD : MonoBehaviour
    {
        // The purpose of this script is to move the consumables HUD out of the way when displaying preview text that overlaps that position
        // The preview text (the game object this is attached to) will move the consumable HUD when enabled/disabled
        public RectTransform consumablesHUD;

        void OnEnable()
        {
            Hide();
        }

        void OnDisable()
        {
            if (LevelUpMenuActive()) Hide();
            else Show();
        }

        private bool LevelUpMenuActive()
        {
            return InputHandler.instance.levelUpMenuWindow.activeSelf;
        }

        private void Hide()
        {
            consumablesHUD.anchoredPosition = new Vector2(-2000.0f, -1000.0f);
            consumablesHUD.anchorMin = new Vector2(0.5f, 0.0f); // Anchored to the bottom-center
            consumablesHUD.anchorMax = new Vector2(0.5f, 0.0f); // Anchored to the bottom-center
        }

        private void Show()
        {
            consumablesHUD.anchoredPosition = new Vector2(-233.0f, 160.0f);
            consumablesHUD.anchorMin = new Vector2(0.5f, 0.0f); // Anchored to the bottom-center
            consumablesHUD.anchorMax = new Vector2(0.5f, 0.0f); // Anchored to the bottom-center
        }
    }
}
