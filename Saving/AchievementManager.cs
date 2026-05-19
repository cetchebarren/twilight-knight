using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace etchebarren
{
    public class AchievementManager : MonoBehaviour
    {
        public float totalPlaytimeInSeconds = 0;

        private float startTime;

        private void Start()
        {
            ResetStartTime();
        }

        public void ResetStartTime()
        {
            startTime = Time.time; // Record start time when the game or scene starts
        }

        // Example method to update total playtime and save it
        public int GetPlaytimeInSeconds()
        {
            // Calculate elapsed time since the start or last update
            float elapsedTimeInSeconds = Time.time - startTime;

            // Add the elapsed time to total playtime
            totalPlaytimeInSeconds += elapsedTimeInSeconds;

            //ResetStartTime();

            // Convert total playtime to a format you want to store (e.g., seconds, minutes, hours)
            //int totalPlaytimeInMinutes = Mathf.RoundToInt(totalPlaytimeInSeconds / 60f);

            return Mathf.RoundToInt(totalPlaytimeInSeconds);
        }
    }
}
