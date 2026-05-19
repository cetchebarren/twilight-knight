using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace etchebarren
{
    public class StatDescriptions : MonoBehaviour
    {
        public GameObject descriptionWindow;
        public TextMeshProUGUI descriptionText;
        public Image buttonImage;

        [TextArea] public string statDescription;

        private Color hidden = new Color(0.37f, 0.27f, 0.15f, 0.0f);
        private Color faded = new Color(0.37f, 0.27f, 0.15f, 0.6f);

        public bool shopUI = false;


        public void PointerEnter()
        {
            if(!shopUI)
                PlayerMenuManager.instance.lastStatButtonImage = buttonImage;
            else
                ShopMenuManager.instance.lastStatButtonImage = buttonImage;
            descriptionWindow.SetActive(true);
            descriptionText.text = statDescription;
            buttonImage.color = faded;
        }

        public void PointerExit()
        {
            descriptionWindow.SetActive(false);
            buttonImage.color = hidden;
        }
    }
}
