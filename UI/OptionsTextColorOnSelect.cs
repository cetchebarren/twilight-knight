using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

namespace etchebarren
{
    public class OptionsTextColorOnSelect : MonoBehaviour, ISelectHandler, IDeselectHandler
    {
        // Reference to the TextMeshPro component
        public TextMeshProUGUI textMeshPro;

        // The original color of the text
        private Color originalColor;
        private bool originalColorSet = false;

        // The color to change to when selected
        public Color selectedColor;

        [Header("Optional: Character Creation Details Screen Only")]
        // Optional setting, used in character creation details screen:
        public MiscCharOptions miscCharOptions;

        void Awake()
        {
            // Store the original color of the text
            SetOriginalColor();
        }

        void OnEnable()
        {
            ResetColor();
        }

        // Called when the object is selected
        public void OnSelect(BaseEventData eventData)
        {
            // Store the original color of the text
            SetOriginalColor();

            // Reset colors on other slider texts if used in character details screen
            miscCharOptions?.ResetTextColors();

            // Change the color of the text to the selected color
            textMeshPro.color = selectedColor;
        }

        // Called when the object is deselected
        public void OnDeselect(BaseEventData eventData)
        {
            // Revert the color of the text back to its original color
            ResetColor();
        }

        public void ResetColor()
        {
            // Revert the color of the text back to its original color
            textMeshPro.color = originalColor;
        }

        private void SetOriginalColor()
        {
            if (!originalColorSet)
            {
                originalColorSet = true;
                originalColor = textMeshPro.color;
            }
        }
    }
}
