using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;

namespace etchebarren
{
    public class Cutscene : MonoBehaviour
    {
        [System.Serializable]
        public class Scene
        {
            public string sceneName;
            public GameObject[] sceneObjects;
            public float sceneDuration;
            public float sceneLightingIntensity;
            public float sceneReflectionIntensity;
            public Material customSkybox;
            public EventGroup[] eventGroups;
            public CameraCut[] cameraCuts;
        }

        [System.Serializable]
        public struct EventGroup
        {
            public float startDelay;
            public UnityEvent unityEvent;
        }

        [System.Serializable]
        public struct CameraCut
        {
            public GameObject camera;
            public Transform newTransform;
            public float time;
        }

        [System.Serializable]
        public struct TextLine
        {
            [TextArea(2, 10)]
            public string text;
            public float startDelay;
            public float duration;
            public AudioClip clip;
            public float volume;
        }

        public enum SceneBorderType
        {
            BlackBars,
            ClipSides
        }

        [Header("Scene Information")]
        public Scene[] scenes;
        public SceneBorderType sceneBorderType;
        public UnityEvent preSceneEvents;
        public UnityEvent postSceneEvents;

        [Header("Scene Text")]
        public TextLine[] textLines;

        [Header("Scene: Eclipse - References")]
        public GameObject moon; // for animating movement
        public Material eclipsePlaneMat;

        [Header("Scene Eclipse Start - Settings")]
        public Vector3 moonStartPos_A;
        public float moonMoveSpeed_A = -5f;
        public float moonMoveDuration_A = 30f;
        public float eclipseStartDelay_A = 1f;
        public float eclipseFadeInDuration_A = 2f;
        public float eclipseStartAlpha_A = 0.0f;
        public float eclipseTargetAlpha_A = 0.8f;

        [Header("Scene Eclipse End - Settings")]
        public Vector3 moonStartPos_B;
        public float moonMoveSpeed_B = -5f;
        public float moonMoveDuration_B = 30f;
        public float eclipseStartDelay_B = 1f;
        public float eclipseFadeInDuration_B = 2f;
        public float eclipseStartAlpha_B = 0.8f;
        public float eclipseTargetAlpha_B = 0.0f;

        [Header("Debugging Options")]
        public bool skipCutscene = false;

        [Header("Temp Values/Refs, Do Not Set Manually")]
        public Scene previousScene = null;

        // Scene Preperation
        private void PrepareScene()
        {
            if(sceneBorderType == SceneBorderType.BlackBars) 
                ScenePersistentPlayerObject.instance.cutsceneManager.aspectRatioFitter.aspectMode = AspectRatioFitter.AspectMode.FitInParent;
            else 
                ScenePersistentPlayerObject.instance.cutsceneManager.aspectRatioFitter.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;

            // Stop Any Currently Running Coroutines before Starting
            StopAllCoroutines();
            previousScene = null;
            //Enable cutscene canvas and disable main camera audio listener
            ScenePersistentPlayerObject.instance.cutsceneManager.cutsceneCanvas.SetActive(true);
            ScenePersistentPlayerObject.instance.cutsceneManager.mainAudioListener.enabled = false;
            ScenePersistentPlayerObject.instance.cutsceneManager.mainCamera.enabled = false;

            foreach (Scene scene in scenes)
            {
                ObjectsSetActive(scene.sceneObjects, false);
            }
        }

        // Play Individual Scene

        public void PlayScene(int index)
        {
            preSceneEvents.Invoke();
            PrepareScene();
            StartCoroutine(PlaySceneCoroutine(index));
        }

        private IEnumerator PlaySceneCoroutine(int index)
        {
            if(scenes[index].customSkybox != null)
            {
                RenderSettings.skybox = scenes[index].customSkybox;
            }

            // Update scene lighting
            RenderSettings.ambientIntensity = scenes[index].sceneLightingIntensity;
            RenderSettings.reflectionIntensity = scenes[index].sceneReflectionIntensity;

            // Set relevant objects active
            ObjectsSetActive(scenes[index].sceneObjects, true);

            // Start processes for all event groups (these are distinguised by start time/delay)
            foreach(EventGroup eventGroup in scenes[index].eventGroups)
            {
                StartCoroutine(InvokeAfterDelay(eventGroup));
            }

            // Start processes for all camera cuts
            foreach(CameraCut cut in scenes[index].cameraCuts)
            {
                StartCoroutine(JumpCutTimer(cut.camera, cut.newTransform, cut.time));
            }

            // Wait until end of scene according to set duration
            yield return new WaitForSeconds(scenes[index].sceneDuration);
            Debug.Log("Scene Finished");
        }

        // Play All Scenes in Scene List

        public void PlayAllScenes()
        {
            if (!PlayerStats.instance.dead)
            {
                InputHandler.instance.inCutscene = true;
                InputHandler.instance.ResetInputs();

                preSceneEvents.Invoke();
                PrepareScene();
                StartCoroutine(PlayAllScenesCoroutine());
            }
        }

        private IEnumerator PlayAllScenesCoroutine()
        {
            if (!skipCutscene)
            {
                // Start playing dialogue text lines
                PlayTextLines();
                // Will probably need to update this to disable old scene objects
                foreach (Scene scene in scenes)
                {
                    Debug.Log("Starting scene: " + scene.sceneName + " ... ");

                    // Update scene lighting
                    if (scene.customSkybox != null)
                    {
                        RenderSettings.skybox = scene.customSkybox;
                    }
                    RenderSettings.ambientIntensity = scene.sceneLightingIntensity;
                    RenderSettings.reflectionIntensity = scene.sceneReflectionIntensity;

                    // Set relevant objects active
                    ObjectsSetActive(scene.sceneObjects, true);

                    // Disable previous objects
                    if (previousScene != null)
                    {
                        Debug.Log("Previous scene was not null - disabling that scene's objects.");
                        ObjectsSetActive(previousScene.sceneObjects, false);
                    }

                    // Start processes for all event groups (these are distinguised by start time/delay)
                    foreach (EventGroup eventGroup in scene.eventGroups)
                    {
                        StartCoroutine(InvokeAfterDelay(eventGroup));
                    }

                    // Start processes for all camera cuts in this scene
                    foreach (CameraCut cut in scene.cameraCuts)
                    {
                        StartCoroutine(JumpCutTimer(cut.camera, cut.newTransform, cut.time));
                    }

                    // Wait until end of scene according to set duration
                    yield return new WaitForSeconds(scene.sceneDuration);

                    previousScene = scene;
                    //foreach loop continues on to next scene

                    Debug.Log("Scene Finished.");
                }

                // Disable previous objects
                if (previousScene != null)
                {
                    Debug.Log("Previous scene was not null - disabling that scene's objects.");
                    ObjectsSetActive(previousScene.sceneObjects, false);
                }
            }

            Debug.Log("All scenes finished.");
            previousScene = null;
            ScenePersistentPlayerObject.instance.cutsceneManager.mainCamera.enabled = true;
            ScenePersistentPlayerObject.instance.cutsceneManager.mainAudioListener.enabled = true;
            ScenePersistentPlayerObject.instance.cutsceneManager.cutsceneCanvas.SetActive(false);
            InputHandler.instance.inCutscene = false;
            postSceneEvents.Invoke();
        }

        // Text and Dialogue

        public void PlayTextLines()
        {
            ScenePersistentPlayerObject.instance.cutsceneManager.cutsceneText.HideText();
            StartCoroutine(PlayTextCoroutine());
        }

        private IEnumerator PlayTextCoroutine()
        {
            foreach(TextLine textLine in textLines)
            {
                // Delay before playing text line
                yield return new WaitForSeconds(textLine.startDelay);

                // Set custscene text
                string text = textLine.text;
                string newText = text;
                // Replace any tokens in dialogue line
                newText = text.Replace("{KNIGHT}", PlayerStats.instance.playerName);
                ScenePersistentPlayerObject.instance.cutsceneManager.cutsceneText.SetText(newText);

                // Set and play audio clip, if there is one
                if(textLine.clip != null)
                {
                    ScenePersistentPlayerObject.instance.cutsceneManager.voicelineAudioSource.Stop();
                    ScenePersistentPlayerObject.instance.cutsceneManager.voicelineAudioSource.clip = textLine.clip;
                    ScenePersistentPlayerObject.instance.cutsceneManager.voicelineAudioSource.volume = textLine.volume;
                    ScenePersistentPlayerObject.instance.cutsceneManager.voicelineAudioSource.Play();
                }

                yield return new WaitForSeconds(textLine.duration);
                ScenePersistentPlayerObject.instance.cutsceneManager.cutsceneText.HideText();
            }
        }
        
        // Camera

        public void JumpCut(GameObject camera, Transform transform, float timer)
        {
            StartCoroutine(JumpCutTimer(camera, transform, timer));
        }

        private IEnumerator JumpCutTimer(GameObject camera, Transform transform, float timer)
        {
            yield return new WaitForSeconds(timer);
            camera.transform.position = transform.position;
            camera.transform.rotation = transform.rotation;
        }

        // Misc

        private void ObjectsSetActive(GameObject[] arr, bool state)
        {
            foreach (GameObject obj in arr)
            {
                obj.SetActive(state);
            }
        }

        private IEnumerator InvokeAfterDelay(EventGroup eventGroup)
        {
            yield return new WaitForSeconds(eventGroup.startDelay);
            eventGroup.unityEvent.Invoke();
        }

        // Scene Specific Functions
        public void Eclipse(bool eclipseStart)
        {
            StartCoroutine(MoveMoon(eclipseStart));
            StartCoroutine(FadeEclipseMaterial(eclipseStart));
        }

        private IEnumerator MoveMoon(bool eclipseStart)
        {
            float moonMoveSpeed;
            Vector3 moonStartPos;
            float moonMoveDuration;

            if (eclipseStart)
            {
                moonMoveSpeed = moonMoveSpeed_A;
                moonStartPos = moonStartPos_A;
                moonMoveDuration = moonMoveDuration_A;
            }
            else
            {
                moonMoveSpeed = moonMoveSpeed_B;
                moonStartPos = moonStartPos_B;
                moonMoveDuration = moonMoveDuration_B;
            }

            float elapsedTime = 0f;
            moon.transform.position = moonStartPos;

            while (elapsedTime < moonMoveDuration)
            {
                moon.transform.position += Vector3.right * moonMoveSpeed * Time.deltaTime;
                elapsedTime += Time.deltaTime;
                yield return null;
            }
        }

        private IEnumerator FadeEclipseMaterial(bool eclipseStart)
        {
            float startDelay;
            float fadeInDuration;
            float startAlpha;
            float targetAlpha;

            if (eclipseStart)
            {
                startDelay = eclipseStartDelay_A;
                fadeInDuration = eclipseFadeInDuration_A;
                startAlpha = eclipseStartAlpha_A;
                targetAlpha = eclipseTargetAlpha_A;
            }
            else
            {
                startDelay = eclipseStartDelay_B;
                fadeInDuration = eclipseFadeInDuration_B;
                startAlpha = eclipseStartAlpha_B;
                targetAlpha = eclipseTargetAlpha_B;
            }

            if (eclipsePlaneMat == null)
            {
                Debug.LogError("No material assigned!");
                yield break;
            }

            Color matColor = eclipsePlaneMat.color;
            matColor.a = startAlpha;
            eclipsePlaneMat.color = matColor;

            // Wait for start delay
            yield return new WaitForSeconds(startDelay);

            //FadeAmbientIntensity(0.2f, fadeInDuration);
            //FadeReflectionIntensity(0.3f, fadeInDuration);

            // Fade from startAlpha to targetAlpha
            float elapsedTime = 0f;
            while (elapsedTime < fadeInDuration)
            {
                matColor.a = Mathf.Lerp(startAlpha, targetAlpha, elapsedTime / fadeInDuration);
                eclipsePlaneMat.color = matColor;
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            // Ensure exact target alpha at the end
            matColor.a = targetAlpha;
            eclipsePlaneMat.color = matColor;
        }

        public void ChangeSkybox(Material skybox)
        {
            RenderSettings.skybox = skybox;
        }

    }
}
