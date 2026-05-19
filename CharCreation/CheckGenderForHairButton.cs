using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace etchebarren
{
    public class CheckGenderForHairButton : MonoBehaviour
    {
        public GameObject marsHairButton;
        public GameObject venusHairButton;

        public void CheckGenderForHair()
        {
            if (PlayerStats.instance.gender == "mars")
            {
                Debug.Log("GENDER HAIR: mars");
                UIChangeSelectedButton.instance.ChangeSelectedButtonTo(marsHairButton);
            }
            else if (PlayerStats.instance.gender == "venus")
            {
                Debug.Log("GENDER HAIR: venus");
                UIChangeSelectedButton.instance.ChangeSelectedButtonTo(venusHairButton);
            }
        }
    }
}

