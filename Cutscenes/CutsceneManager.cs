using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace etchebarren
{
    public class CutsceneManager : MonoBehaviour
    {
        // Data Used from individual Cutscene objects
        [Header("Universal Cutscene References")]
        public GameObject cutsceneCanvas;
        public AudioListener mainAudioListener;
        public Camera mainCamera;
        public CutsceneText cutsceneText;
        public AspectRatioFitter aspectRatioFitter;
        public AudioSource voicelineAudioSource;
    }
}
