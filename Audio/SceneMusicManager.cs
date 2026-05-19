using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace etchebarren
{
    public class SceneMusicManager : MonoBehaviour
    {
        [System.Serializable]
        public struct Track
        {
            public AudioClip clip;
            public float volume;
            public float fadeInDuration;
            public float fadeOutDuration;
        }

        [Header("References & Variables:")]
        public AudioSource audioSource;
        [SerializeField] private int currentTrack = 0;

        public Track[] tracklist;

        [Header("Settings:")]
        [Tooltip("Delay in seconds before first track starts to play. Useful for fine-tuning scene introductions.")]
        public float delayBeforeFirstTrack = 0f;
        [Tooltip("Minimum delay in seconds before a track will start playing after the previous track ends. Range will be found between this value and maximum delay.")]
        public float minDelayBeforeTrack = 0f;
        [Tooltip("Maximum delay in seconds before a track will start playing after the previous track ends. Range will be found between this value and minimum delay.")]
        public float maxDelayBeforeTrack = 0f;
        [Tooltip("Determines if the tracklist should play in order or randomized. If true, it will be randomized.")]
        public bool shuffleTracklist = false;
        [Tooltip("Determines if a track can be reselected after it finishes playing. If true, the music manager will never repeat songs back to back.")]
        public bool preventRepeat = true;
        [Tooltip("Even if shuffle is on, the first track of the tracklist will be the first to play if this value is true.")]
        public bool alwaysPlayFirstTrackFirst = false;

        [Header("Boss Music:")]
        public Track[] bossTracklist;
        public bool playingBossMusic = false;

        // Coroutine variables to track running coroutines
        private Coroutine startFirstTrackCoroutine;
        private Coroutine playTrackCoroutine;
        private Coroutine playNextTrackCoroutine;
        private Coroutine bossMusicCoroutine;
        private Coroutine fadeOutBossMusicCoroutine;

        void Start()
        {
            // if there is at least one track in the tracklist, start process
            if (tracklist.Length > 0)
            {
                startFirstTrackCoroutine = StartCoroutine(StartFirstTrack());
            }
        }

        private IEnumerator StartFirstTrack()
        {
            // Start delay before first track as determined by scene music manager settings
            yield return new WaitForSeconds(delayBeforeFirstTrack);

            // If shuffle is enabled and we don't always play the first song in the tracklist
            if (shuffleTracklist && !alwaysPlayFirstTrackFirst)
            {
                // Get random index from tracklist
                currentTrack = Random.Range(0, tracklist.Length);
            }

            // Play selected track, if not shuffling current track is zero by default from inspector/default settings
            playTrackCoroutine = StartCoroutine(PlayTrack(tracklist[currentTrack]));
        }

        private IEnumerator PlayNextTrack()
        {
            // If there is only one track in the tracklist, keep it selected
            if (tracklist.Length != 1)
            {
                // Determine the next track
                if (shuffleTracklist)
                {
                    // Record previous track index to prevent repeats, if enabled in settings
                    int previousTrack = currentTrack;
                    // Get random index from tracklist
                    currentTrack = Random.Range(0, tracklist.Length);
                    // If we are preventing track repeat and there is more than one possible track
                    if (preventRepeat && tracklist.Length > 1)
                    {
                        // Loop until we have a new track index selected
                        while (currentTrack == previousTrack)
                        {
                            currentTrack = Random.Range(0, tracklist.Length);
                        }
                    }
                }
                else
                {
                    // Select the next track index in order, and loop back to index 0 (first track) if needed
                    currentTrack = (currentTrack + 1) % tracklist.Length;
                }
            }

            // Determine the delay before next track, if any
            float delay = Random.Range(minDelayBeforeTrack, maxDelayBeforeTrack);
            if (delay > 0)
            {
                yield return new WaitForSeconds(delay);
            }

            // Start the Next Track
            playTrackCoroutine = StartCoroutine(PlayTrack(tracklist[currentTrack]));
        }

        private IEnumerator PlayTrack(Track track)
        {
            // Set volume to 0 and start playing the track
            audioSource.volume = 0f;
            audioSource.clip = track.clip;
            audioSource.Play();

            // Check that fade times are valid
            if (track.fadeInDuration + track.fadeOutDuration < track.clip.length)
            {
                // Fade in
                float elapsedTime = 0f;
                while (elapsedTime < track.fadeInDuration)
                {
                    elapsedTime += Time.unscaledDeltaTime;
                    float t = elapsedTime / track.fadeInDuration;
                    audioSource.volume = Mathf.Lerp(0f, track.volume, t);
                    yield return null;
                }

                // Ensure volume is at max
                audioSource.volume = track.volume;

                // Wait for the appropriate time before starting the fade-out
                yield return new WaitForSecondsRealtime(audioSource.clip.length - track.fadeOutDuration);

                // Fade out
                elapsedTime = 0f;
                while (elapsedTime < track.fadeOutDuration)
                {
                    elapsedTime += Time.unscaledDeltaTime;
                    float t = elapsedTime / track.fadeOutDuration;
                    audioSource.volume = Mathf.Lerp(track.volume, 0f, t);
                    yield return null;
                }
            }
            else
            {
                // If there is an issue with the fade times, just wait for the clip length
                audioSource.volume = track.volume;
                yield return new WaitForSecondsRealtime(audioSource.clip.length);
            }

            // Ensure volume is at min
            audioSource.volume = 0f;

            // Stop playing the track
            audioSource.Stop();

            // Start the process for the next track
            playNextTrackCoroutine = StartCoroutine(PlayNextTrack());
        }

        public void Restart()
        {
            // Reset audio source values and coroutines
            Reset();
            // Restart the music process
            startFirstTrackCoroutine = StartCoroutine(StartFirstTrack());
        }

        public void StopBossMusic()
        {
            if (!playingBossMusic) return;
            if (bossMusicCoroutine != null)
            {
                StopCoroutine(bossMusicCoroutine);
                bossMusicCoroutine = null;
            }
            if (fadeOutBossMusicCoroutine != null)
            {
                StopCoroutine(fadeOutBossMusicCoroutine);
                fadeOutBossMusicCoroutine = null;
            }
            fadeOutBossMusicCoroutine = StartCoroutine(FadeOutBossMusic());
            playingBossMusic = false;
        }

        private IEnumerator FadeOutBossMusic()
        {
            Reset(false);

            // Fade out
            float elapsedTime = 0f;
            float fadeOutDuration = 5f;
            float startingVolume = audioSource.volume;
            while (elapsedTime < fadeOutDuration)
            {
                elapsedTime += Time.unscaledDeltaTime;
                float t = elapsedTime / fadeOutDuration;
                audioSource.volume = Mathf.Lerp(startingVolume, 0f, t);
                yield return null;
            }

            Restart();
        }

        public void Reset(bool stopMusic=true)
        {
            // Stop all currently running coroutines
            if (startFirstTrackCoroutine != null)
            {
                StopCoroutine(startFirstTrackCoroutine);
                startFirstTrackCoroutine = null;
            }
            if (playTrackCoroutine != null)
            {
                StopCoroutine(playTrackCoroutine);
                playTrackCoroutine = null;
            }
            if (playNextTrackCoroutine != null)
            {
                StopCoroutine(playNextTrackCoroutine);
                playNextTrackCoroutine = null;
            }
            if(bossMusicCoroutine != null)
            {
                StopCoroutine(bossMusicCoroutine);
                bossMusicCoroutine = null;
            }
            if(fadeOutBossMusicCoroutine != null)
            {
                StopCoroutine(fadeOutBossMusicCoroutine);
                fadeOutBossMusicCoroutine = null;
            }

            if (stopMusic)
            {
                // Stop audio playback
                audioSource.Stop();
                audioSource.clip = null;

                // Reset currentTrack to default
                currentTrack = 0;
            }
        }

        public void PlayBossMusic(int track)
        {
            if (playingBossMusic) return;
            playingBossMusic = true;
            Reset();
            if (bossMusicCoroutine != null)
            {
                StopCoroutine(bossMusicCoroutine);
                bossMusicCoroutine = null;
            }
            bossMusicCoroutine = StartCoroutine(PlayBossTrack(bossTracklist[track]));
        }

        private IEnumerator PlayBossTrack(Track track)
        {
            // Set volume to 0 and start playing the track
            audioSource.volume = 0f;
            audioSource.clip = track.clip;
            audioSource.Play();

            // Check that fade times are valid
            if (track.fadeInDuration + track.fadeOutDuration < track.clip.length)
            {
                // Fade in
                float elapsedTime = 0f;
                while (elapsedTime < track.fadeInDuration)
                {
                    elapsedTime += Time.unscaledDeltaTime;
                    float t = elapsedTime / track.fadeInDuration;
                    audioSource.volume = Mathf.Lerp(0f, track.volume, t);
                    yield return null;
                }

                // Ensure volume is at max
                audioSource.volume = track.volume;

                // Wait for the appropriate time before starting the fade-out
                yield return new WaitForSecondsRealtime(audioSource.clip.length - track.fadeOutDuration);

                // Fade out
                elapsedTime = 0f;
                while (elapsedTime < track.fadeOutDuration)
                {
                    elapsedTime += Time.unscaledDeltaTime;
                    float t = elapsedTime / track.fadeOutDuration;
                    audioSource.volume = Mathf.Lerp(track.volume, 0f, t);
                    yield return null;
                }
            }
            else
            {
                // If there is an issue with the fade times, just wait for the clip length
                audioSource.volume = track.volume;
                yield return new WaitForSecondsRealtime(audioSource.clip.length);
            }

            // Ensure volume is at min
            audioSource.volume = 0f;

            // Stop playing the track
            audioSource.Stop();

            // Loop boss music
            bossMusicCoroutine = StartCoroutine(PlayBossTrack(track));
        }
    }
}
