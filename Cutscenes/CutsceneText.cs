using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace etchebarren
{
    public class CutsceneText : MonoBehaviour
    {
        [Header("Text Settings")]
        public RectTransform textBackground;
        public TextMeshProUGUI text;
        public float textBoxSizeBonus = 600f;
        public float maxTextBoxWidth = 2820f;
        [Header("Text Box Height: Short")]
        public float textbox_shortThreshold = 44f;
        public float textBoxYPos_short = 22f;
        public float textBoxHeight_short = 150f;
        [Header("Text Box Height: Medium")]
        public float textbox_medThreshold = 88f;
        public float textBoxYPos_med = -56f;
        public float textBoxHeight_med = 310f;
        [Header("Text Box Height: Tall")]
        public float textbox_tallThreshold = 131f;
        public float textBoxYPos_tall = -205f;
        public float textBoxHeight_tall = 580f;

        public void SetText(string newText)
        {
            text.text = newText;
            UpdateTextBackgroundSize();
        }

        public void HideText()
        {
            text.gameObject.SetActive(false);
            textBackground.gameObject.SetActive(false);
        }

        private void UpdateTextBackgroundSize()
        {
            // Ensure the objects are active
            text.gameObject.SetActive(true);
            textBackground.gameObject.SetActive(true);
            // Now get the correct values for preferred width
            float effectiveWidth = Mathf.Clamp(text.preferredWidth + textBoxSizeBonus, 10f, maxTextBoxWidth);
            // Set height and position based on height thresholds
            float effectiveHeight, effectiveYPos;
            if (text.preferredHeight < textbox_shortThreshold)
            {
                effectiveHeight = textBoxHeight_short;
                effectiveYPos = textBoxYPos_short;
            }
            else if (text.preferredHeight < textbox_medThreshold)
            {
                effectiveHeight = textBoxHeight_med;
                effectiveYPos = textBoxYPos_med;
            }
            else
            {
                effectiveHeight = textBoxHeight_tall;
                effectiveYPos = textBoxYPos_tall;
            }
            // Apply the calculated values to the background
            textBackground.sizeDelta = new Vector2(effectiveWidth, effectiveHeight);
            textBackground.localPosition = new Vector3(textBackground.localPosition.x, effectiveYPos, textBackground.localPosition.z);
        }
    }
}
