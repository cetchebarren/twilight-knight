using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

namespace etchebarren
{
    public class MiscCharOptions : MonoBehaviour
    {
        [System.Serializable]
        public struct Detail
        {
            public GameObject obj;
            public int ID;
        }

        [Header("FACIAL HAIR")]
        public Slider facialHair;
        public GameObject[] facialHairM;
        public GameObject[] facialHairF;

        [Header("FACIAL MARKINGS")]
        public Slider marking1;
        public Slider marking2;
        public Slider marking3;
        [SerializeField] List<Detail> markingsListM = new List<Detail>();
        [SerializeField] List<Detail> markingsListF = new List<Detail>();
        [SerializeField] private Detail[] currentMarkingM;
        [SerializeField] private Detail[] currentMarkingF;

        [Header("FACIAL PIERCINGS")]
        public Slider piercing1;
        public Slider piercing2;
        public Slider piercing3;
        [SerializeField] List<Detail> piercingsListM = new List<Detail>();
        [SerializeField] List<Detail> piercingsListF = new List<Detail>();
        [SerializeField] private Detail[] currentPiercingM;
        [SerializeField] private Detail[] currentPiercingF;

        [Header("EARRINGS (LOWER)")]
        public Slider earringLeft;
        public Slider earringRight;
        public GameObject[] leftEarringsM;
        public GameObject[] leftEarringsF;
        public GameObject[] rightEarringsM;
        public GameObject[] rightEarringsF;
        public GameObject[] dangleEarringsLeft; // We use this array to check if we need to disable/enable earring animation processes if un/necessary
        public GameObject[] dangleEarringsRight; // We use this array to check if we need to disable/enable earring animation processes if un/necessary

        [Header("EARRING (UPPER)")]
        public Slider earringLeftUpper;
        public Slider earringRightUpper;
        public GameObject[] leftEarringsUpperM;
        public GameObject[] leftEarringsUpperF;
        public GameObject[] rightEarringsUpperM;
        public GameObject[] rightEarringsUpperF;

        [Header("MISC. REFERENCES / SETTINGS")]
        public OptionsTextColorOnSelect[] allSliders;
        [SerializeField] AnimatorHandler animatorHandlerM;
        [SerializeField] AnimatorHandler animatorHandlerF;

        private void Awake()
        {
            // Initialize currentMarkingM array with a size of 3
            currentMarkingM = new Detail[3];
            currentMarkingF = new Detail[3];
            // Initialize currentPiercingM array with a size of 3
            currentPiercingM = new Detail[3];
            currentPiercingF = new Detail[3];
        }

        // Start is called before the first frame update
        void Start()
        {
            if(markingsListM.Count != markingsListF.Count)
            {
                Debug.LogError("Markings Length should be identical! Check array.");
            }
            else if (piercingsListF.Count != piercingsListM.Count)
            {
                Debug.LogError("Piercings Length should be identical! Check array.");
            }
            else if (facialHairF.Length != facialHairM.Length)
            {
                Debug.LogError("Facial Hair Length should be identical! Check array.");
            }
            else if (leftEarringsF.Length != leftEarringsM.Length)
            {
                Debug.LogError("Left Earrings Length should be identical! Check array.");
            }
            else if (rightEarringsF.Length != rightEarringsM.Length)
            {
                Debug.LogError("right Earrings Length should be identical! Check array.");
            }
            else if (leftEarringsUpperF.Length != leftEarringsUpperM.Length)
            {
                Debug.LogError("Left Earrings Upper Length should be identical! Check array.");
            }
            else if (rightEarringsUpperF.Length != rightEarringsUpperM.Length)
            {
                Debug.LogError("right Earrings Upper Length should be identical! Check array.");
            }

            UpdateMarkingSlidersMaxValue();
            UpdatePiercingSlidersMaxValue();

            earringRight.maxValue = rightEarringsM.Length;
            earringLeft.maxValue = leftEarringsM.Length;

            earringRightUpper.maxValue = rightEarringsUpperM.Length;
            earringLeftUpper.maxValue = leftEarringsUpperM.Length;

            marking1.value = 0;
            marking2.value = 0;
            marking3.value = 0;

            piercing1.value = 0;
            piercing2.value = 0;
            piercing3.value = 0;

            earringLeft.value = 0;
            earringRight.value = 0;

            earringLeftUpper.value = 0;
            earringRightUpper.value = 0;
        }

        public void ResetTextColors()
        {
            foreach(OptionsTextColorOnSelect otcos in allSliders)
            {
                otcos.ResetColor();
            }
        }

        public void UpdateFacialHairSlider()
        {
            // Disable all Facial Hair Styles First
            foreach (GameObject facialHairStyle in facialHairM)
            {
                facialHairStyle.SetActive(false);
            }
            foreach (GameObject facialHairStyle in facialHairF)
            {
                facialHairStyle.SetActive(false);
            }

            // Set New Style
            switch (facialHair.value)
            {
                case 0:
                    // No Facial Hair
                    break;
                case 1: // Curly Mustache
                    facialHairM[2].SetActive(true);
                    facialHairF[2].SetActive(true);
                    break;
                case 2: // Bushy Mustache
                    facialHairM[1].SetActive(true);
                    facialHairF[1].SetActive(true);
                    break;
                case 3: // Goatee
                    facialHairM[3].SetActive(true);
                    facialHairF[3].SetActive(true);
                    break;
                case 4: // Goatee & Curly Mustache
                    facialHairM[2].SetActive(true);
                    facialHairF[2].SetActive(true);
                    facialHairM[3].SetActive(true);
                    facialHairF[3].SetActive(true);
                    break;
                case 5: // Goatee & Bushy Mustache
                    facialHairM[1].SetActive(true);
                    facialHairF[1].SetActive(true);
                    facialHairM[3].SetActive(true);
                    facialHairF[3].SetActive(true);
                    break;
                case 6: // Long Beard and Bushy Mustache
                    facialHairM[1].SetActive(true);
                    facialHairF[1].SetActive(true);
                    facialHairM[0].SetActive(true);
                    facialHairF[0].SetActive(true);
                    break;
                case 7: // Long Beard and Curly Mustache
                    facialHairM[2].SetActive(true);
                    facialHairF[2].SetActive(true);
                    facialHairM[0].SetActive(true);
                    facialHairF[0].SetActive(true);
                    break;
                case 8: // Long Beard
                    facialHairM[0].SetActive(true);
                    facialHairF[0].SetActive(true);
                    break;

            }
        }

        public void UpdateMarkingSlider(int sliderType)
        {
            Slider slider;
            int i = sliderType - 1;
            switch (sliderType)
            {
                case 1:
                    slider = marking1;
                    break;
                case 2:
                    slider = marking2;
                    break;
                case 3:
                    slider = marking3;
                    break;
                default:
                    slider = marking1;
                    break;
            }

            if (slider.value == 0)
            {
                ReinsertDetail(currentMarkingM[i], currentMarkingF[i], "marking");
                currentMarkingM[i].obj.SetActive(false);
                currentMarkingF[i].obj.SetActive(false);
                currentMarkingM[i].obj = null;
                currentMarkingF[i].obj = null;
            }
            else
            {
                // Disable previous marking and readd it to the list
                if (currentMarkingM[i].obj != null) currentMarkingM[i].obj.SetActive(false);
                if (currentMarkingF[i].obj != null) currentMarkingF[i].obj.SetActive(false);
                if (currentMarkingM[i].obj != null) ReinsertDetail(currentMarkingM[i], currentMarkingF[i], "marking");
                // Set new marking, remove it from list, enable it
                currentMarkingM[i] = markingsListM[(int)slider.value - 1];
                currentMarkingF[i] = markingsListF[(int)slider.value - 1];
                RemoveDetail(currentMarkingM[i], currentMarkingF[i], "marking");
                currentMarkingM[i].obj.SetActive(true);
                currentMarkingF[i].obj.SetActive(true);
            }
            UpdateMarkingSlidersMaxValue();
        }

        public void UpdatePiercingSlider(int sliderType)
        {
            Slider slider;
            int i = sliderType - 1;
            switch (sliderType)
            {
                case 1:
                    slider = piercing1;
                    break;
                case 2:
                    slider = piercing2;
                    break;
                case 3:
                    slider = piercing3;
                    break;
                default:
                    slider = piercing1;
                    break;
            }

            if (slider.value == 0)
            {
                ReinsertDetail(currentPiercingM[i], currentPiercingF[i], "piercing");
                currentPiercingM[i].obj.SetActive(false);
                currentPiercingF[i].obj.SetActive(false);
                currentPiercingM[i].obj = null;
                currentPiercingF[i].obj = null;
            }
            else
            {
                // Disable previous marking and readd it to the list
                if (currentPiercingM[i].obj != null) currentPiercingM[i].obj.SetActive(false);
                if (currentPiercingF[i].obj != null) currentPiercingF[i].obj.SetActive(false);
                if (currentPiercingM[i].obj != null) ReinsertDetail(currentPiercingM[i], currentPiercingF[i], "piercing");
                // Set new marking, remove it from list, enable it
                currentPiercingM[i] = piercingsListM[(int)slider.value - 1];
                currentPiercingF[i] = piercingsListF[(int)slider.value - 1];
                RemoveDetail(currentPiercingM[i], currentPiercingF[i],"piercing");
                currentPiercingM[i].obj.SetActive(true);
                currentPiercingF[i].obj.SetActive(true);
            }
            UpdatePiercingSlidersMaxValue();
        }

        public void UpdateEarringSlider(bool left)
        {
            if (left)
            {
                // Disable All Earrings
                foreach (GameObject earring in leftEarringsM)
                {
                    earring.SetActive(false);
                }
                foreach (GameObject earring in leftEarringsF)
                {
                    earring.SetActive(false);
                }
                // Enable Selected Earring
                if (earringLeft.value > 0)
                {
                    leftEarringsM[(int)earringLeft.value - 1].SetActive(true);
                    leftEarringsF[(int)earringLeft.value - 1].SetActive(true);
                }
            }
            else
            {
                // Disable All Earrings
                foreach (GameObject earring in rightEarringsM)
                {
                    earring.SetActive(false);
                }
                foreach (GameObject earring in rightEarringsF)
                {
                    earring.SetActive(false);
                }
                // Enable Selected Earring
                if (earringRight.value > 0)
                {
                    rightEarringsF[(int)earringRight.value - 1].SetActive(true);
                    rightEarringsM[(int)earringRight.value - 1].SetActive(true);
                }
            }

            // Check if earring animations are necessary
            bool earringAnimationRequired = false;
            bool dangleLeft = false;
            bool dangleRight = false;

            for(int i = 0; i < dangleEarringsLeft.Length; i++)
            {
                if (dangleEarringsLeft[i].activeSelf)
                {
                    earringAnimationRequired = true;
                    dangleLeft = true;
                    break;
                }
            }
            for (int i = 0; i < dangleEarringsRight.Length; i++)
            {
                if (dangleEarringsRight[i].activeSelf)
                {
                    earringAnimationRequired = true;
                    dangleRight = true;
                    break;
                }
            }

            Debug.Log("Animate:" + earringAnimationRequired);
            Debug.Log("Left:" + dangleLeft);
            Debug.Log("Right:" + dangleRight);
            animatorHandlerM.SetAnimateEarrings(earringAnimationRequired, dangleLeft, dangleRight);
            animatorHandlerF.SetAnimateEarrings(earringAnimationRequired, dangleLeft, dangleRight);
        }

        public void UpdateEarringUpperSlider(bool left)
        {
            if (left)
            {
                // Disable All Earrings
                foreach (GameObject earring in leftEarringsUpperM)
                {
                    earring.SetActive(false);
                }
                foreach (GameObject earring in leftEarringsUpperF)
                {
                    earring.SetActive(false);
                }
                // Enable Selected Earring
                if (earringLeftUpper.value > 0)
                {
                    leftEarringsUpperM[(int)earringLeftUpper.value - 1].SetActive(true);
                    leftEarringsUpperF[(int)earringLeftUpper.value - 1].SetActive(true);
                }
            }
            else
            {
                // Disable All Earrings
                foreach (GameObject earring in rightEarringsUpperM)
                {
                    earring.SetActive(false);
                }
                foreach (GameObject earring in rightEarringsUpperF)
                {
                    earring.SetActive(false);
                }
                // Enable Selected Earring
                if (earringRightUpper.value > 0)
                {
                    rightEarringsUpperF[(int)earringRightUpper.value - 1].SetActive(true);
                    rightEarringsUpperM[(int)earringRightUpper.value - 1].SetActive(true);
                }
            }
        }

        // Reinsert a GameObject to its original position
        public void ReinsertDetail(Detail detailM, Detail detailF, string type)
        {
            switch (type)
            {
                case "marking":
                    markingsListM.Add(detailM);
                    markingsListM.Sort((x, y) => x.ID.CompareTo(y.ID));
                    markingsListF.Add(detailF);
                    markingsListF.Sort((x, y) => x.ID.CompareTo(y.ID));
                    break;
                case "piercing":
                    piercingsListM.Add(detailM);
                    piercingsListM.Sort((x, y) => x.ID.CompareTo(y.ID));
                    piercingsListF.Add(detailF);
                    piercingsListF.Sort((x, y) => x.ID.CompareTo(y.ID));
                    break;
                default:
                    Debug.LogError("Unexpected case for ReinsertDetail() in MiscCharOptions.");
                    break;
            }

        }

        // Remove a GameObject from the list without updating original indices
        public void RemoveDetail(Detail detailM, Detail detailF, string type)
        {
            switch (type)
            {
                case "marking":
                    markingsListM.Remove(detailM);
                    markingsListF.Remove(detailF);
                    break;
                case "piercing":
                    piercingsListM.Remove(detailM);
                    piercingsListF.Remove(detailF);
                    break;
                default:
                    Debug.LogError("Unexpected case for RemoveDetail() in MiscCharOptions.");
                    break;
            }
        }

        public void UpdateMarkingSlidersMaxValue()
        {
            marking1.maxValue = markingsListM.Count + 1;
            marking2.maxValue = markingsListM.Count + 1;
            marking3.maxValue = markingsListM.Count + 1;
        }

        public void UpdatePiercingSlidersMaxValue()
        {
            piercing1.maxValue = piercingsListM.Count + 1;
            piercing2.maxValue = piercingsListM.Count + 1;
            piercing3.maxValue = piercingsListM.Count + 1;
        }

    }
}
