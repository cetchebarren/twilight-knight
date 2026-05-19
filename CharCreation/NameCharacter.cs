using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace etchebarren
{
    public class NameCharacter : MonoBehaviour
    {
        //public CharacterStatsManager character;
        public GameObject inputField;
        public TextMeshProUGUI nameButtonText;
        public TextMeshProUGUI statsBoxNameText;
        public string genderChoice = "mars";

        public void NameMyCharacter()
        {
            string nametext = inputField.GetComponent<TMP_InputField>().text.Trim(); ;

            Debug.Log("Character name saved: " + nametext);

            nameButtonText.text = nametext;

            bool isEmptyOrWhitespace = string.IsNullOrWhiteSpace(nametext);
            if (isEmptyOrWhitespace)
            {
                PlayerStats.instance.playerName = "Knight";
            }
            else
            {
                PlayerStats.instance.playerName = nametext;
            }

            //statsBoxNameText.text = PlayerStats.instance.playerName + " - Lv." + PlayerStats.instance.playerLevel;
        }

        public void changeGender(string mars_or_venus)
        {
             if (mars_or_venus == "mars")
            {
                genderChoice = "mars";
            }
             else if (mars_or_venus == "venus")
            {
                genderChoice = "venus";
            }
            Debug.Log("Gender selected: " + genderChoice);
        }
    }
}
