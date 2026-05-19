using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace etchebarren
{
    public class SavingNotification : MonoBehaviour
    {
        public float rotationSpeed = 250f; // Rotation speed in degrees per second
        public float saveCompleteNotificationDuration = 3.0f;
        public TextMeshProUGUI saveText;
        public RectTransform spinner;

        public bool autosaving = false;

        private Coroutine disableAfterTime;

        void OnEnable()
        {
            if (disableAfterTime != null)
            {
                StopCoroutine(disableAfterTime);
            }

            if (autosaving)
            {
                saveText.text = "Autosaving...";
            }
            else
            {
                saveText.text = "Saving...";
            }
            spinner.gameObject.SetActive(true);         
        }

        void Update()
        {
            // Use unscaledDeltaTime to rotate even if timeScale is 0
            spinner.Rotate(0f, 0f, rotationSpeed * Time.unscaledDeltaTime);
        }

        private IEnumerator DisableAfterTime(float time)
        {
            // Use unscaled time for the wait
            yield return new WaitForSecondsRealtime(time);
            gameObject.SetActive(false);
            disableAfterTime = null;
        }

        public void Complete()
        {
            spinner.gameObject.SetActive(false);
            saveText.text = "Save Complete!";

            if(disableAfterTime == null)
            {
                StartCoroutine(DisableAfterTime(saveCompleteNotificationDuration));
            }
            else
            {
                StopCoroutine(disableAfterTime);
                StartCoroutine(DisableAfterTime(saveCompleteNotificationDuration));
            }

        }
    }
}
