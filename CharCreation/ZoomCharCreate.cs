using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace etchebarren
{
    public class ZoomCharCreate : MonoBehaviour
    {
        public Sprite zoom_in, zoom_out;

        public GameObject cameraZoomedIn, cameraZoomedOut;
        public GameObject cameraZoomedInM, cameraZoomedOutM;

        public Image spriteLocation;

        public string genderChoice = "mars";

        public void Zoom()
        {
            if (genderChoice == "venus")
            {
                if (cameraZoomedIn.activeSelf)
                {
                    cameraZoomedOut.SetActive(true);
                    cameraZoomedIn.SetActive(false);

                    spriteLocation.sprite = zoom_in;
                }
                else
                {
                    cameraZoomedOut.SetActive(false);
                    cameraZoomedIn.SetActive(true);

                    spriteLocation.sprite = zoom_out;
                }
            }
            else if (genderChoice == "mars")
            {
                if (cameraZoomedInM.activeSelf)
                {
                    cameraZoomedOutM.SetActive(true);
                    cameraZoomedInM.SetActive(false);

                    spriteLocation.sprite = zoom_in;
                }
                else
                {
                    cameraZoomedOutM.SetActive(false);
                    cameraZoomedInM.SetActive(true);

                    spriteLocation.sprite = zoom_out;
                }
            }
        }

        public void ChangeGender(string mars_or_venus)
        {
            if (mars_or_venus == "mars")
            {
                genderChoice = "mars";
            }
            else if (mars_or_venus == "venus")
            {
                genderChoice = "venus";
            }
            spriteLocation.sprite = zoom_in;
        }
    }
}
