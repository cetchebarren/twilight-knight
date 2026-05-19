using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace etchebarren
{
    public class AcquiredNotifications : MonoBehaviour
    {
        public static AcquiredNotifications instance;

        public GameObject goldTextParent;
        public TextMeshProUGUI goldText;

        public TextMeshProUGUI acquiredText;

        public GameObject titleBackground;

        public GameObject[] texts;

        public int goldAcquired = 0;

        //private string[] rarityColors = { "<color=#D3D3D3>", "<color=#00FF00>", "<color=#00FFFF>", "<color=#E6E6FA>", "<color=#FFD700>" };

        [Header("Background Colors")]
        public Color blackColor = new Color(0.0f, 0.0f, 0.0f, 0.502f);
        public Color greenColor = new Color(0.0f, 1.0f, 0.161f, 0.502f);
        public Color blueColor = new Color(0.0f, 0.494f, 0.729f, 0.710f);
        public Color purpleColor = new Color(0.898f, 0.0f, 1.0f, 0.502f);
        public Color goldColor = new Color(1.0f, 0.913f, 0.0f, 0.502f);

        private Color[] rarityColors;

        /* How to send notification:
            AcquiredNotifications.instance.NewItemNotification(Item item, int count);
        */

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

            acquiredText.text = "";
            titleBackground.SetActive(false);

            rarityColors = new Color[] { blackColor, greenColor, blueColor, purpleColor, goldColor };
        }

        public void NewItemNotification(Item item, int count)
        {
            if (count < 1)
            {
                count = 1;
            }

            titleBackground.SetActive(true);
            acquiredText.text = "Acquired";
            if(item == null) //Gold
            {
                if(goldAcquired == 0)
                {
                    goldAcquired = count;
                    goldText.text = "Gold × " + goldAcquired;
                    goldTextParent.SetActive(true);
                }
                else
                {
                    goldText.GetComponent<AcquiredItemText>().ResetGoldTimer();
                    goldAcquired += count;
                    goldText.text = "Gold × " + goldAcquired;
                }
            }
            else
            {

                //Note: add colors for rarity 

                // Go through texts and find the first that is not active, this will be the one we use
                foreach (GameObject txt in texts)
                {
                    if (!txt.activeSelf)
                    {
                        txt.SetActive(true);
                        txt.transform.GetChild(0).GetComponent<Image>().color = rarityColors[item.rarity];
                        txt.transform.GetChild(1).GetComponent<Image>().sprite = item.itemIcon;
                        txt.transform.GetChild(2).GetComponent<TextMeshProUGUI>().text = item.itemName + " × " + count;
                        txt.transform.SetSiblingIndex(5);
                        return;
                    }
                }

                // If reached: All texts in use, replace the oldest one
                GameObject oldestText = gameObject.transform.GetChild(1).gameObject;
                oldestText.SetActive(true);
                oldestText.transform.GetChild(0).GetComponent<Image>().color = rarityColors[item.rarity];
                oldestText.transform.GetChild(1).GetComponent<Image>().sprite = item.itemIcon;
                oldestText.transform.GetChild(2).GetComponent<TextMeshProUGUI>().text = item.itemName + " × " + count;
                oldestText.transform.SetSiblingIndex(5);
                oldestText.transform.GetChild(2).GetComponent<AcquiredItemText>().ResetTimer();
            }

        }

        public void CheckTextsAndDeactivateTitle()
        {
            //Debug.Log("1");
            foreach (GameObject txt in texts)
            {
                if (txt.activeSelf)
                {
                    return;
                }
            }

            //Debug.Log("2");
            if (goldTextParent.activeSelf)
            {
                return;
            }

            //Debug.Log("3");
            acquiredText.text = "";
            titleBackground.SetActive(false);
        }
    }
}
