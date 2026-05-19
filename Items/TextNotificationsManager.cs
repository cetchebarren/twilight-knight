using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace etchebarren
{
    public class TextNotificationsManager : MonoBehaviour
    {
        public static TextNotificationsManager instance;

        public GameObject[] texts;

        private List<string> recentMessages = new List<string>();
        private float messageMemoryTime = 3.0f;

        /* How to send notification:
            TextNotificationsManager.instance.NewTextNotifaction("Not enough in inventory", true/false); //true prevents duplicate messages for 3 seconds
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
        }

        public void NewTextNotifaction(string message, bool spamFilter = true)
        {
            if (!spamFilter || !IsRecentMessage(message))
            {
                foreach (GameObject txt in texts)
                {
                    if (!txt.activeSelf)
                    {
                        // Activate Notification
                        txt.SetActive(true);
                        // Set Notification Text
                        txt.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = message;
                        // Adjust text background length
                        AdjustBackgroundLength(txt.transform.GetChild(0).GetComponent<RectTransform>(), message.Length);
                        // Move Notfication to Bottom of List
                        txt.transform.SetSiblingIndex(4);
                        // Only add to recent messages if spam filter is enabled
                        if (spamFilter) 
                        {
                            AddRecentMessage(message);
                        }
                        // Stop Here (No need to replace oldest notification)
                        return;
                    }
                }

                // All texts in use, replace the oldest one
                GameObject oldestText = gameObject.transform.GetChild(0).gameObject;
                // Activate Notification
                oldestText.SetActive(true);
                // Set Notification Text
                oldestText.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = message;
                // Adjust text background length
                AdjustBackgroundLength(oldestText.transform.GetChild(0).GetComponent<RectTransform>(), message.Length);
                // Move Notfication to Bottom of List
                oldestText.transform.SetSiblingIndex(4);
                // Only add to recent messages if spam filter is enabled
                if (spamFilter) 
                {
                    AddRecentMessage(message);
                }
                // Since this message is being reused, must reset its timer
                oldestText.transform.GetChild(1).GetComponent<TextNotification>().ResetTimer();
            }
        }

        private bool IsRecentMessage(string message)
        {
            return recentMessages.Contains(message);
        }

        private void AddRecentMessage(string message)
        {
            recentMessages.Add(message);
            StartCoroutine(RemoveRecentMessage(message));
        }

        private IEnumerator RemoveRecentMessage(string message)
        {
            float startTime = Time.realtimeSinceStartup;
            while (Time.realtimeSinceStartup - startTime < messageMemoryTime)
            {
                yield return null;
            }
            recentMessages.Remove(message);
        }

        private void AdjustBackgroundLength(RectTransform background, int characterCount)
        {
            // Define min and max widths
            float minWidth = 567f;
            float maxWidth = 960f;

            // Define character count thresholds
            int minCharacterCount = 30;
            int maxCharacterCount = 45;

            // Calculate the new width based on character count
            float newWidth;
            if (characterCount <= minCharacterCount)
            {
                newWidth = minWidth;
            }
            else if (characterCount >= maxCharacterCount)
            {
                newWidth = maxWidth;
            }
            else
            {
                // Interpolate between minWidth and maxWidth
                float t = (characterCount - minCharacterCount) / (float)(maxCharacterCount - minCharacterCount);
                newWidth = Mathf.Lerp(minWidth, maxWidth, t);
            }

            // Update the RectTransform's width
            background.sizeDelta = new Vector2(newWidth, background.sizeDelta.y);
        }

        public void NotificationViaUnityEvent(string message)
        {
            NewTextNotifaction(message);
        }

    }
}
