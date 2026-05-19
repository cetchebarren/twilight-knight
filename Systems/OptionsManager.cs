using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using TMPro;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace etchebarren
{
    public class OptionsManager : MonoBehaviour
    {
        [Header("Script References")]
        public MapInputHandler mapInputHandler;
        public CameraHandler cameraHandler;
        public UIAudioManager uiAudioManager;
        public ControllerUIManager controllerUIManager;
        public InputHandler inputHandler;
        public HelpMenu helpMenu;

        [Header("Volume Settings")]
        public AudioMixer mixer;
        public Slider masterVolume;
        public Slider musicVolume;
        public Slider sfxVolume;
        public Slider menuVolume;
        public Slider ambientVolume;

        [Header("Controls Settings")]
        public int maxHSens = 300;
        public int minHSens = 30;
        public int maxVSens = 300;
        public int minVSens = 15;
        public Slider gameCameraHorizontalSensitivity;
        public Slider gameCameraVerticalSensitivity;
        public Slider gameCameraDistance;
        public Slider gameCameraReorientSpeed;
        public Slider mapCursorSensitivity;
        public Slider controllerType;
        public TMP_Dropdown inputTypeSelection;
        public int inputType = 0;
        public GameObject pcControls;
        public GameObject playstationControls;
        public GameObject xboxControls;
        public GameObject closeControlsButton;
        public TMP_Dropdown mouseTargetingSelection;

        [Header("Visual Settings")]
        public Slider minimapZoom;
        public Camera minimapCamera;
        public Slider minimapSizeSlider;
        public Slider trackedQuestSizeSlider;
        public RectTransform minimapAndTrackedQuestGroup;
        public GameObject minimapWindowHUD;
        public GameObject trackedQuestHUD;
        public TMP_Dropdown displayModeSelection;
        public TMP_Dropdown aspectRatioSelection;
        public TMP_Dropdown resolutionSelection;
        public TMP_Dropdown qualityPresetSelection;
        public TMP_Dropdown antialiasingSelection;
        public TMP_Dropdown vsyncSelection;
        public TMP_Dropdown textureQualitySelection;
        public TMP_Dropdown lightingQualitySelection;
        public TMP_Dropdown shadowQualitySelection;
        public TMP_Dropdown animationQualitySelection;
        public TMP_Dropdown lodBiasSelection;

        public bool isApplyingPreset = false;
        public bool isFullscreen = true;
        public int aspectRatio = 169;
        public float resolution = 1080;
        public int qualityPreset = 0;

        [Header("Misc Settings")]
        public TMP_Dropdown tipsSelection;

        [Header("Controller Type References")]
        public Image xboxOption;
        public Image playstationOption;

        [Header("Option/Button Colors")]
        private Color semiTransparent = new Color(1, 1, 1, 0.275f);
        private Color opaque = new Color(1, 1, 1, 1);

        [Header("Do Not Set Manually")]
        public GameObject lastGameObjectSelected; // Used to return to last button when pressing back

        // Since this script isn't active at the beginning of the game (prior to opening menu), default settings function calls have been moved to WorldStateManager.cs

        #region PlayerPrefs
        public void LoadVolumeSettings()
        {
            if(PlayerPrefs.HasKey("masterVolume")
                && PlayerPrefs.HasKey("musicVolume")
                && PlayerPrefs.HasKey("menuVolume")
                && PlayerPrefs.HasKey("sfxVolume")
                && PlayerPrefs.HasKey("ambientVolume"))
            {
                masterVolume.value = PlayerPrefs.GetFloat("masterVolume");
                SetMasterVolume(false);
                musicVolume.value = PlayerPrefs.GetFloat("musicVolume");
                SetMusicVolume(false);
                menuVolume.value = PlayerPrefs.GetFloat("menuVolume");
                SetMenuVolume(false);
                sfxVolume.value = PlayerPrefs.GetFloat("sfxVolume");
                SetSFXVolume(false);
                ambientVolume.value = PlayerPrefs.GetFloat("ambientVolume");
                SetAmbientVolume(false);
            }
            else
            {
                RestoreDefaultAudioSettings();
            }    
        }

        public void LoadControlSettings()
        {
            if (PlayerPrefs.HasKey("horizontalCameraSensitivity")
                && PlayerPrefs.HasKey("verticalCameraSensitivity")
                && PlayerPrefs.HasKey("cameraDistanceMultiplier")
                && PlayerPrefs.HasKey("cameraReorientSpeed")
                && PlayerPrefs.HasKey("mapCursorSensitivity")
                && PlayerPrefs.HasKey("inputType")
                && PlayerPrefs.HasKey("controllerType")
                && PlayerPrefs.HasKey("mouseTargeting"))
            {
                gameCameraHorizontalSensitivity.value = SliderValueFromSetting(
                    minHSens, maxHSens, PlayerPrefs.GetFloat("horizontalCameraSensitivity"),
                    gameCameraHorizontalSensitivity.minValue, gameCameraHorizontalSensitivity.maxValue);
                //gameCameraHorizontalSensitivity.value = PlayerPrefs.GetFloat("horizontalCameraSensitivity");
                SetGameCameraSensitivityH(false);

                gameCameraVerticalSensitivity.value = SliderValueFromSetting(
                    minVSens, maxVSens, PlayerPrefs.GetFloat("verticalCameraSensitivity"),
                    gameCameraVerticalSensitivity.minValue, gameCameraVerticalSensitivity.maxValue);
                //gameCameraVerticalSensitivity.value = PlayerPrefs.GetFloat("verticalCameraSensitivity");
                SetGameCameraSensitivityV(false);

                gameCameraDistance.value = PlayerPrefs.GetFloat("cameraDistanceMultiplier");
                SetGameCameraDistanceMultiplier(false);

                gameCameraReorientSpeed.value = PlayerPrefs.GetFloat("cameraReorientSpeed");
                SetGameCameraReorientSpeed(false);

                mapCursorSensitivity.value = PlayerPrefs.GetFloat("mapCursorSensitivity");
                SetMapCursorSensitivity(false);

                inputTypeSelection.value = PlayerPrefs.GetInt("inputType");
                SetInputType(false);

                controllerType.value = PlayerPrefs.GetFloat("controllerType");
                SetControllerType(false);

                mouseTargetingSelection.value = PlayerPrefs.GetInt("mouseTargeting");
                SetMouseTargeting(false);
            }
            else
            {
                RestoreDefaultControlSettings();
            }
        }

        public void LoadVisualSettings()
        {
            if (PlayerPrefs.HasKey("minimapZoom")
                && PlayerPrefs.HasKey("minimapScale")
                && PlayerPrefs.HasKey("trackedQuestScale"))
            {
                minimapZoom.value = PlayerPrefs.GetFloat("minimapZoom");
                SetMinimapZoom(false);

                minimapSizeSlider.value = PlayerPrefs.GetFloat("minimapScale");
                SetMinimapScale(false);

                trackedQuestSizeSlider.value = PlayerPrefs.GetFloat("trackedQuestScale");
                SetTrackedQuestScale(false);
            }
            else
            {
                RestoreDefaultVisualSettings();
            }
        }

        public void LoadGraphicSettings()
        {
            if (PlayerPrefs.HasKey("fullscreen")
                && PlayerPrefs.HasKey("resolution")
                && PlayerPrefs.HasKey("aspectRatio")
                && PlayerPrefs.HasKey("antiAliasing")
                && PlayerPrefs.HasKey("vsync")
                && PlayerPrefs.HasKey("lodBias")
                && PlayerPrefs.HasKey("textureQuality")
                && PlayerPrefs.HasKey("lightingQuality")
                && PlayerPrefs.HasKey("shadowQuality")
                && PlayerPrefs.HasKey("animationQuality")
                && PlayerPrefs.HasKey("qualityPreset"))
            {
                // Store the existing event listeners so we can reset them after disabling events and changing values
                var displayModeSelectionExistingListeners = displayModeSelection.onValueChanged;
                var resolutionSelectionExistingListeners = resolutionSelection.onValueChanged;
                var aspectRatioSelectionExistingListeners = aspectRatioSelection.onValueChanged;

                // Remove all listeners
                displayModeSelection.onValueChanged = new TMP_Dropdown.DropdownEvent();
                resolutionSelection.onValueChanged = new TMP_Dropdown.DropdownEvent();
                aspectRatioSelection.onValueChanged = new TMP_Dropdown.DropdownEvent();

                // Set the value without triggering the value changed events
                displayModeSelection.value = PlayerPrefs.GetInt("fullscreen");
                resolutionSelection.value = PlayerPrefs.GetInt("resolution");
                aspectRatioSelection.value = PlayerPrefs.GetInt("aspectRatio");

                // Re-add the value changed listeners
                displayModeSelection.onValueChanged = displayModeSelectionExistingListeners;
                resolutionSelection.onValueChanged = resolutionSelectionExistingListeners;
                aspectRatioSelection.onValueChanged = aspectRatioSelectionExistingListeners;

                // Update isFullscreen, aspectRatio and resolution and call ApplyResolution
                SetFullscreen(false);
                SetResolution(false);
                SetAspectRatio(false);

                // If the preset is custom, apply individal settings
                if (PlayerPrefs.GetInt("qualityPreset") == 6)
                {
                    qualityPresetSelection.value = PlayerPrefs.GetInt("qualityPreset");
                    SetQualityPreset(true);
                    antialiasingSelection.value = PlayerPrefs.GetInt("antiAliasing");
                    SetAntiAliasing(true);
                    vsyncSelection.value = PlayerPrefs.GetInt("vsync");
                    SetVSync(true);
                    textureQualitySelection.value = PlayerPrefs.GetInt("textureQuality");
                    SetTextureQuality(true);
                    lightingQualitySelection.value = PlayerPrefs.GetInt("lightingQuality");
                    SetLightingQuality(true);
                    shadowQualitySelection.value = PlayerPrefs.GetInt("shadowQuality");
                    SetShadowQuality(true);
                    animationQualitySelection.value = PlayerPrefs.GetInt("animationQuality");
                    SetAnimationQuality(true);
                    lodBiasSelection.value = PlayerPrefs.GetInt("lodBias");
                    SetLODBias(true);
                }
                else // Not a custom preset, apply preset (ultra, high, etc.) to settings
                {
                    qualityPresetSelection.value = PlayerPrefs.GetInt("qualityPreset");
                    SetQualityPreset(true);
                    isApplyingPreset = true;
                    SetAntiAliasing(true);
                    SetVSync(true);
                    SetTextureQuality(true);
                    SetLightingQuality(true);
                    SetShadowQuality(true);
                    SetAnimationQuality(true);
                    SetLODBias(true);
                    isApplyingPreset = false;
                }
            }
            else
            {
                RestoreDefaultGraphicalSettings();
            }
        }

        public void LoadMiscSettings()
        {
            if (PlayerPrefs.HasKey("tipPopups"))
            {
                tipsSelection.value = PlayerPrefs.GetInt("tipPopups");
                SetTipsDisplay(true);
            }
            else
            {
                RestoreDefaultMiscSettings();
            }
        }
        #endregion

        #region Menu Traversal
        public void SetLastSelectedGameObjectOption(GameObject gameObj)
        {
            uiAudioManager.PlayPressButtonAudio();
            // If there is already a selected button we need to clear it (and its text color) first
            if(lastGameObjectSelected != null)
            {
                SetLastSelectedGameObjectOptionToNull(false);
            }
            lastGameObjectSelected = gameObj;
        }

        public void SetLastSelectedGameObjectOptionToNull(bool audio=true)
        {
            if(audio)uiAudioManager.PlayDecreaseStatAudio();
            if (lastGameObjectSelected == null) return;

            OptionsTextColorOnSelect component = null;

            if (lastGameObjectSelected != null && lastGameObjectSelected.transform.childCount > 1)
            {
                // index 1 corresponds to the second child
                component = lastGameObjectSelected.transform.GetChild(1).GetComponent<OptionsTextColorOnSelect>();
            }

            if (component != null)
            {
                //Debug.Log("Text Color Reset");
                component.ResetColor();
            }

            lastGameObjectSelected = null;
        }

        public void BackToPreviousSelectedOption()
        {
            // If there is no UI gameobject (i.e. button) to go back to, return
            if (lastGameObjectSelected == null) return;

            controllerUIManager.BackTextActive(false);
            controllerUIManager.SetSelectText("Select");

            // Only do this if using controller
            if (!controllerUIManager.isUsingController()) return;

            Button toButton = lastGameObjectSelected.GetComponent<Button>();
            if(toButton != null)
            {
                // Select the button
                toButton.Select();
                // Now that we have gone back, clear the last selected gameobject as we can no longer go back
                SetLastSelectedGameObjectOptionToNull();
                return;
            }

            Slider toSlider = lastGameObjectSelected.GetComponent<Slider>();
            if (toSlider != null)
            {
                toSlider.Select();
                SetLastSelectedGameObjectOptionToNull();
                return;
            }
        }
        #endregion

        #region Volume Options
        public void SetMasterVolume(bool saveToPlayerPrefs)
        {
            float vol;
            Debug.Log("Setting master volume...");

            mixer.GetFloat("masterVolume", out vol);
            Debug.Log("Volume before: " + vol);

            // Convert linear volume value back to logarithmic scale
            mixer.SetFloat("masterVolume", Mathf.Log10(masterVolume.value)*20);
            if (saveToPlayerPrefs) PlayerPrefs.SetFloat("masterVolume", masterVolume.value);

            mixer.GetFloat("masterVolume", out vol);
            Debug.Log("Volume after: " + vol);
        }

        public void SetMusicVolume(bool saveToPlayerPrefs)
        {
            // Convert linear volume value back to logarithmic scale
            mixer.SetFloat("musicVolume", Mathf.Log10(musicVolume.value) * 20);
            if (saveToPlayerPrefs) PlayerPrefs.SetFloat("musicVolume", musicVolume.value);
        }

        public void SetSFXVolume(bool saveToPlayerPrefs)
        {
            // Convert linear volume value back to logarithmic scale
            mixer.SetFloat("sfxVolume", Mathf.Log10(sfxVolume.value) * 20);
            if (saveToPlayerPrefs) PlayerPrefs.SetFloat("sfxVolume", sfxVolume.value);
        }

        public void SetMenuVolume(bool saveToPlayerPrefs)
        {
            // Convert linear volume value back to logarithmic scale
            mixer.SetFloat("menuVolume", Mathf.Log10(menuVolume.value) * 20);
            if (saveToPlayerPrefs) PlayerPrefs.SetFloat("menuVolume", menuVolume.value);
        }

        public void SetAmbientVolume(bool saveToPlayerPrefs)
        {
            // Convert linear volume value back to logarithmic scale
            mixer.SetFloat("ambientVolume", Mathf.Log10(ambientVolume.value) * 20);
            if (saveToPlayerPrefs) PlayerPrefs.SetFloat("ambientVolume", ambientVolume.value);
        }

        public void SetSlidersBasedOnVolumeLevels()
        {
            float masterVolumeValue;
            mixer.GetFloat("masterVolume", out masterVolumeValue);
            // Convert logarithmic volume value back to linear scale
            masterVolume.value = Mathf.Pow(10f, masterVolumeValue / 20f);

            float musicVolumeValue;
            mixer.GetFloat("musicVolume", out musicVolumeValue);
            // Convert logarithmic volume value back to linear scale
            musicVolume.value = Mathf.Pow(10f, musicVolumeValue / 20f);

            float sfxVolumeValue;
            mixer.GetFloat("sfxVolume", out sfxVolumeValue);
            // Convert logarithmic volume value back to linear scale
            sfxVolume.value = Mathf.Pow(10f, sfxVolumeValue / 20f);

            float menuVolumeValue;
            mixer.GetFloat("menuVolume", out menuVolumeValue);
            // Convert logarithmic volume value back to linear scale
            menuVolume.value = Mathf.Pow(10f, menuVolumeValue / 20f);

            float ambientVolumeValue;
            mixer.GetFloat("ambientVolume", out ambientVolumeValue);
            // Convert logarithmic volume value back to linear scale
            ambientVolume.value = Mathf.Pow(10f, ambientVolumeValue / 20f);
        }

        public void RestoreDefaultAudioSettings()
        {
            mixer.SetFloat("masterVolume", Mathf.Log10(1f) * 20);
            PlayerPrefs.SetFloat("masterVolume", 1f);

            mixer.SetFloat("musicVolume", Mathf.Log10(0.4f) * 20);
            PlayerPrefs.SetFloat("musicVolume", 0.4f);

            mixer.SetFloat("sfxVolume", Mathf.Log10(0.75f) * 20);
            PlayerPrefs.SetFloat("sfxVolume", 0.75f);

            mixer.SetFloat("menuVolume", Mathf.Log10(0.65f) * 20);
            PlayerPrefs.SetFloat("menuVolume", 0.65f);

            mixer.SetFloat("ambientVolume", Mathf.Log10(0.7f) * 20);
            PlayerPrefs.SetFloat("ambientVolume", 0.7f);

            SetSlidersBasedOnVolumeLevels();
        }

        #endregion

        #region Controls Options

        public void DetermineControlsDiagram()
        {
            bool controller = controllerUIManager.isUsingController();

            // Display close button if no controller is connected even if inputType is set to Controller
            if (!controller || inputType == 2)
            {
                closeControlsButton.SetActive(true);
            }
            else
            {
                closeControlsButton.SetActive(false);
            }

            // Input Type: 0 Auto, 1 Controller, 2 Keyboard & Mouse
            if (inputType == 1) controller = true;
            else if (inputType == 2) controller = false;

            if (controller)
            {
                if (controllerUIManager.controllerTypePlaystation)
                {
                    playstationControls.SetActive(true);
                    xboxControls.SetActive(false);
                    pcControls.SetActive(false);
                }
                else
                {
                    playstationControls.SetActive(false);
                    xboxControls.SetActive(true);
                    pcControls.SetActive(false);
                }
                controllerUIManager.SetBackText("Back");
                controllerUIManager.SetRightTriggerText("Switch Platform");
            }
            else
            {
                playstationControls.SetActive(false);
                xboxControls.SetActive(false);
                pcControls.SetActive(true);
                controllerUIManager.SetRightStickText("Switch Platform");
            }

            controllerUIManager.SelectTextActive(false);

            // Determine Rect Transform scale / positions to accomodate for aspect ratios
            float aspectRatio = (float)Screen.width / (float)Screen.height;
            Debug.Log("aspect ratio: " + aspectRatio);

            RectTransform pcRect = pcControls.GetComponent<RectTransform>();
            RectTransform playstationRect = playstationControls.GetComponent<RectTransform>();
            RectTransform xboxRect = xboxControls.GetComponent<RectTransform>();

            if (aspectRatio < 1.4f) // 4:3 is 1.33, less than 1.4 is safe range 
            {
                // Update scales respectively
                pcRect.localScale = new Vector3(1.25f, 1.25f, 1.25f);
                playstationRect.localScale = new Vector3(1.25f, 1.25f, 1.25f);
                xboxRect.localScale = new Vector3(1.25f, 1.25f, 1.25f);
                // Update positions respectively
                pcRect.anchoredPosition = new Vector2(0f, -785f); // -48f
                playstationRect.anchoredPosition = new Vector2(playstationRect.anchoredPosition.x, -785f);
                xboxRect.anchoredPosition = new Vector2(xboxRect.anchoredPosition.x, -785f);
            }
            else
            {
                // Update scales respectively
                pcRect.localScale = Vector3.one;
                playstationRect.localScale = Vector3.one;
                xboxRect.localScale = Vector3.one;
                // Update positions respectively
                pcRect.anchoredPosition = new Vector2(-48f, -580f);
                playstationRect.anchoredPosition = new Vector2(playstationRect.anchoredPosition.x, -625f);
                xboxRect.anchoredPosition = new Vector2(xboxRect.anchoredPosition.x, -625f);
            }
        }

        public void SwitchControlsLayout()
        {
            Debug.Log("Switch Controls Layout Called");

            if (pcControls.activeSelf)
            {
                pcControls.SetActive(false);
                playstationControls.SetActive(true);
                xboxControls.SetActive(false);
            }
            else if (playstationControls.activeSelf)
            {
                playstationControls.SetActive(false);
                xboxControls.SetActive(true);
                pcControls.SetActive(false);
            }
            else if (xboxControls.activeSelf)
            {
                xboxControls.SetActive(false);
                pcControls.SetActive(true);
                playstationControls.SetActive(false);
            }
            uiAudioManager.PlaySwitchMenuTabAudio();
        }

        private float MapSlider(float min, float max, float sliderValue, float sliderMin, float sliderMax)
        {
            float t = (sliderValue - sliderMin) / (sliderMax - sliderMin);
            return min + t * (max - min);
        }

        private float SliderValueFromSetting(float min, float max, float setting, float sliderMin, float sliderMax)
        {
            float t = (setting - min) / (max - min);
            return Mathf.Lerp(sliderMin, sliderMax, t);
        }


        public void SetGameCameraSensitivityH(bool saveToPlayerPrefs)
        {
            int sliderMin = (int)gameCameraHorizontalSensitivity.minValue;
            int sliderMax = (int)gameCameraHorizontalSensitivity.maxValue;
            int sliderVal = (int)gameCameraHorizontalSensitivity.value;

            float horizontal = MapSlider(minHSens, maxHSens, sliderVal, sliderMin, sliderMax);

            cameraHandler.SetCameraSensitivityH(horizontal);
            if (saveToPlayerPrefs) PlayerPrefs.SetFloat("horizontalCameraSensitivity", horizontal);
        }

        public void SetGameCameraSensitivityV(bool saveToPlayerPrefs)
        {
            int sliderMin = (int)gameCameraVerticalSensitivity.minValue;
            int sliderMax = (int)gameCameraVerticalSensitivity.maxValue;
            int sliderVal = (int)gameCameraVerticalSensitivity.value;

            float vertical = MapSlider(minVSens, maxVSens, sliderVal, sliderMin, sliderMax);

            cameraHandler.SetCameraSensitivityV(vertical);
            if (saveToPlayerPrefs) PlayerPrefs.SetFloat("verticalCameraSensitivity", vertical);
        }

        public void SetGameCameraDistanceMultiplier(bool saveToPlayerPrefs)
        {
            float multiplier = gameCameraDistance.value;
            cameraHandler.SetCameraDistanceMutliplier(multiplier);
            if (saveToPlayerPrefs) PlayerPrefs.SetFloat("cameraDistanceMultiplier", multiplier);
        }

        public void SetGameCameraReorientSpeed(bool saveToPlayerPrefs)
        {
            float speed = gameCameraReorientSpeed.value;
            float duration;
            //0.30 0.25 0.20 0.15 0.10 0.05 0.00
            //0    1     2     3    4    5    6
            switch (speed)
            {
                case 0:
                    duration = 0.3f;
                    break;
                case 1:
                    duration = 0.25f;
                    break;
                case 2:
                    duration = 0.2f;
                    break;
                case 3:
                    duration = 0.15f;
                    break;
                case 4:
                    duration = 0.1f;
                    break;
                case 5:
                    duration = 0.05f;
                    break;
                case 6:
                    duration = 0.0f;
                    break;
                default:
                    duration = 0.1f;
                    break;
            }

            cameraHandler.SetReorientDuration(duration);

            if (saveToPlayerPrefs) PlayerPrefs.SetFloat("cameraReorientSpeed", speed);
        }

        public void SetMapCursorSensitivity(bool saveToPlayerPrefs)
        {
            // Convert linear volume value back to logarithmic scale
            Vector3 newSensitivity = new Vector3(mapCursorSensitivity.value, mapCursorSensitivity.value, mapCursorSensitivity.value);
            mapInputHandler.sensitivity = newSensitivity;
            if (saveToPlayerPrefs) PlayerPrefs.SetFloat("mapCursorSensitivity", mapCursorSensitivity.value);
        }

        public void SetInputType(bool saveToPlayerPrefs)
        {
            Debug.Log("Set Input Type called");

            inputType = inputTypeSelection.value;

            if(inputType == 2)
            {
                EnableGamepads(false);
            }
            else
            {
                EnableGamepads(true);
            }

            controllerUIManager.SetUIBasedOnInputType();
            cameraHandler.ConfigureSensitivityBasedOnInput();

            if (saveToPlayerPrefs) PlayerPrefs.SetInt("inputType", inputType);
        }

        public void RestoreDefaultControlSettings()
        {
            // Reset Map Cursor Speed
            mapInputHandler.sensitivity = new Vector3(500.0f, 500.0f, 500.0f);
            PlayerPrefs.SetFloat("mapCursorSensitivity", 500.0f);
            // Reset game camera sensitivity Horizontal
            cameraHandler.SetCameraSensitivityH(100.0f);
            PlayerPrefs.SetFloat("horizontalCameraSensitivity", 100.0f);
            // Reset game camera sensitivity Vertical
            cameraHandler.SetCameraSensitivityV(50.0f);
            PlayerPrefs.SetFloat("verticalCameraSensitivity", 50.0f);
            // Reset game camera distance multiplier
            cameraHandler.SetCameraDistanceMutliplier(1.0f);
            PlayerPrefs.SetFloat("cameraDistanceMultiplier", 1.0f);
            // Reset camera reorientation speed
            cameraHandler.SetReorientDuration(0.1f);
            PlayerPrefs.SetFloat("cameraReorientSpeed", 4f);
            // Reset Input Type
            inputType = 0;
            PlayerPrefs.SetInt("inputType", inputType);
            // Reset mouse targeting
            inputHandler.mouseSwitchesTargets = true;
            PlayerPrefs.SetInt("mouseTargeting", 0);

            SetControllerType(true);

            SetSlidersBasedOnControls();

            Debug.Log("Restore Default Control Settings Called");
        }

        public void SetSlidersBasedOnControls()
        {
            // Map cursor sensitivity
            mapCursorSensitivity.value = mapInputHandler.sensitivity.x;
            // Turn sensitivity
            /*
            gameCameraHorizontalSensitivity.value = SliderValueFromSetting(
                minHSens, maxHSens, cameraHandler.leftAndRightSpeedSetting, 
                gameCameraHorizontalSensitivity.minValue, 
                gameCameraHorizontalSensitivity.maxValue);*/
            //gameCameraHorizontalSensitivity.value = cameraHandler.leftAndRightSpeedSetting; //old
            gameCameraHorizontalSensitivity.value = 14;

            gameCameraVerticalSensitivity.value = SliderValueFromSetting(
                minVSens, maxVSens, cameraHandler.upAndDownSpeedSetting,
                gameCameraVerticalSensitivity.minValue,
                gameCameraVerticalSensitivity.maxValue);
            //gameCameraVerticalSensitivity.value = cameraHandler.upAndDownSpeedSetting;

            // Camera distance
            gameCameraDistance.value = cameraHandler.cameraDistanceMultiplier;
            // Contrller type
            int _controllerType = 0;
            if (ControllerUIManager.instance.controllerTypePlaystation) _controllerType = 1;
            controllerType.value = _controllerType;
            // Reorient camera speed
            switch (cameraHandler.reorientationDuration)
            {
                case 0.3f:
                    gameCameraReorientSpeed.value = 0;
                    break;
                case 0.25f:
                    gameCameraReorientSpeed.value = 1;
                    break;
                case 0.2f:
                    gameCameraReorientSpeed.value = 2;
                    break;
                case 0.15f:
                    gameCameraReorientSpeed.value = 3;
                    break;
                case 0.10f:
                    gameCameraReorientSpeed.value = 4;
                    break;
                case 0.05f:
                    gameCameraReorientSpeed.value = 5;
                    break;
                case 0.0f:
                    gameCameraReorientSpeed.value = 6;
                    break;
                default:
                    gameCameraReorientSpeed.value = 4;
                    break;
            }

            inputTypeSelection.value = inputType;
        }

        public void SetControllerType(bool saveToPlayerPrefs)
        {
            int type = (int)controllerType.value;
            if(type == 0)
            {
                controllerUIManager.SetPlayStationController(false);
            }
            else
            {
                controllerUIManager.SetPlayStationController(true);
            }
            SetImageTransparencies();
            if (saveToPlayerPrefs) PlayerPrefs.SetFloat("controllerType", controllerType.value);   
        }

        public void SetMouseTargeting(bool saveToPlayerPrefs)
        {
            inputHandler.mouseSwitchesTargets = (mouseTargetingSelection.value == 0);

            if (saveToPlayerPrefs) PlayerPrefs.SetInt("mouseTargeting", mouseTargetingSelection.value);
        }

        public void EnableGamepads(bool enable)
        {      
            foreach (var gamepad in Gamepad.all)
            {
                if (enable)
                {
                    InputSystem.EnableDevice(gamepad);
                }
                else
                {
                    InputSystem.DisableDevice(gamepad);
                }
            }          
        }

        #endregion

        #region Visual Options

        public void SetMinimapZoom(bool saveToPlayerPrefs)
        {
            minimapCamera.orthographicSize = minimapZoom.value;
            MinimapOutOfBoundsIndicator.instance.minimapRange = minimapCamera.orthographicSize;
            if (saveToPlayerPrefs) PlayerPrefs.SetFloat("minimapZoom", minimapZoom.value);
        }

        public void SetMinimapScale(bool saveToPlayerPrefs)
        {
            float newScale = minimapSizeSlider.value;
            minimapWindowHUD.transform.localScale = new Vector3(newScale, newScale, newScale);
            LayoutRebuilder.ForceRebuildLayoutImmediate(minimapAndTrackedQuestGroup);
            if (saveToPlayerPrefs) PlayerPrefs.SetFloat("minimapScale", minimapSizeSlider.value);
        }

        public void SetTrackedQuestScale(bool saveToPlayerPrefs)
        {
            float newScale = trackedQuestSizeSlider.value;
            trackedQuestHUD.transform.localScale = new Vector3(newScale, newScale, newScale);
            LayoutRebuilder.ForceRebuildLayoutImmediate(minimapAndTrackedQuestGroup);
            if (saveToPlayerPrefs) PlayerPrefs.SetFloat("trackedQuestScale", trackedQuestSizeSlider.value);
        }

        public void RestoreDefaultVisualSettings()
        {
            // Reset minimap zoom
            minimapCamera.orthographicSize = 25f;
            PlayerPrefs.SetFloat("minimapZoom", 25f);
            // Reset minimap scale
            minimapWindowHUD.transform.localScale = new Vector3(1.15f, 1.15f, 1.15f);
            PlayerPrefs.SetFloat("minimapScale", 1.15f);
            // Reset tracked quest scale
            trackedQuestHUD.transform.localScale = Vector3.one;
            PlayerPrefs.SetFloat("trackedQuestScale", 1.0f);

            SetSlidersBasedOnVisuals();
        }

        public void SetSlidersBasedOnVisuals()
        {
            // mini map zoom
            minimapZoom.value = minimapCamera.orthographicSize;
            // minimap size
            minimapSizeSlider.value = minimapWindowHUD.transform.localScale.x;
            // tracked quest HUD size
            trackedQuestSizeSlider.value = trackedQuestHUD.transform.localScale.x;
        }

        // Update the fullscreen value (not the fullscreen setting itself) and flag whether it has changed or not
        public void SetFullscreen(bool saveToPlayerPrefs)
        {
            bool previouslyFullscreen = isFullscreen;

            switch (displayModeSelection.value)
            {
                case 0:
                    isFullscreen = true;
                    break;
                case 1:
                    isFullscreen = false;
                    break;
                default:
                    break;
            }

            if (previouslyFullscreen != isFullscreen)
            {
                ApplyResolution(saveToPlayerPrefs);
            }

            Debug.Log("Full screen has been set to: " + isFullscreen);
        }

        // Update the aspect ratio value (not the aspect ratio itself) and flag whether it has changed or not
        public void SetAspectRatio(bool saveToPlayerPrefs)
        {
            int previousAspectRatio = aspectRatio;

            switch (aspectRatioSelection.value)
            {
                case 0:
                    aspectRatio = 169;
                    break;
                case 1:
                    aspectRatio = 1610;
                    break;
                case 2:
                    aspectRatio = 43;
                    break;
                case 3:
                    aspectRatio = 219;
                    break;
                default:
                    break;
            }

            if (previousAspectRatio != aspectRatio)
            {
                ApplyResolution(saveToPlayerPrefs);
            }
        }

        // Update the resolution value (not the resolution itself) and flag whether it has changed or not
        public void SetResolution(bool saveToPlayerPrefs)
        {
            float previousResolution = resolution;

            switch (resolutionSelection.value)
            {
                case 0:
                    resolution = 2160f;
                    break;
                case 1:
                    resolution = 1440f;
                    break;
                case 2:
                    resolution = 1080f;
                    break;
                case 3:
                    resolution = 720f;
                    break;
                case 4:
                    resolution = 480f;
                    break;
                default:
                    break;
            }

            if(previousResolution != resolution)
            {
                ApplyResolution(saveToPlayerPrefs);
            }
        }

        // Update the actual fullscreen, aspect ratio and resolution settings based on their values in script
        private void ApplyResolution(bool saveToPlayerPrefs)
        {
            float height = resolution;

            float width = (height * 16f / 9f); // Default we will just set to 16:9

            if (aspectRatio == 1610) // 16:10
            {
                width = (height * 16f / 10f); // Calculate width based on height
            }
            else if (aspectRatio == 43) // 4:3
            {
                width = (height * 4f / 3f); // Calculate width based on height
            }
            else if (aspectRatio == 219) // 21:9
            {
                width = (height * 21f / 9f); // Calculate width based on height
            }

            Debug.Log("Display Updated! Resolution: " + (int)width + "p x " + (int)height + "p, Fullscreen: " + isFullscreen);

            Screen.fullScreenMode = isFullscreen ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed;
            Screen.SetResolution((int)width, (int)height, Screen.fullScreenMode);

            if (saveToPlayerPrefs)
            {
                PlayerPrefs.SetInt("fullscreen", displayModeSelection.value);
                PlayerPrefs.SetInt("aspectRatio", aspectRatioSelection.value);
                PlayerPrefs.SetInt("resolution", resolutionSelection.value);

                // Recapture menu background for new resolution         
                PlayerMenuManager.instance.CaptureCameraSnapshotFromMenu();
            }
        }

        // Apply changes to all settings based on a preset selection
        public void SetQualityPreset(bool saveToPlayerPrefs)
        {
            // Enable Flag so that upcoming value changes to do not change Preset to custom
            isApplyingPreset = true;

            switch (qualityPresetSelection.value)
            {
                case 0: // Ultra
                    antialiasingSelection.value = 1;
                    vsyncSelection.value = 1;
                    textureQualitySelection.value = 0;
                    lightingQualitySelection.value = 0;
                    shadowQualitySelection.value = 0;
                    animationQualitySelection.value = 0;
                    lodBiasSelection.value = 1;
                    break;
                case 1: // Very High
                    antialiasingSelection.value = 1;
                    vsyncSelection.value = 1;
                    textureQualitySelection.value = 0;
                    lightingQualitySelection.value = 1;
                    shadowQualitySelection.value = 1;
                    animationQualitySelection.value = 1;
                    lodBiasSelection.value = 2;
                    break;
                case 2: // High
                    antialiasingSelection.value = 0;
                    vsyncSelection.value = 1;
                    textureQualitySelection.value = 0;
                    lightingQualitySelection.value = 2;
                    shadowQualitySelection.value = 2;
                    animationQualitySelection.value = 1;
                    lodBiasSelection.value = 3;
                    break;
                case 3: // Medium
                    antialiasingSelection.value = 0;
                    vsyncSelection.value = 1;
                    textureQualitySelection.value = 0;
                    lightingQualitySelection.value = 3;
                    shadowQualitySelection.value = 3;
                    animationQualitySelection.value = 1;
                    lodBiasSelection.value = 4;
                    break;
                case 4: // Low
                    antialiasingSelection.value = 0;
                    vsyncSelection.value = 0;
                    textureQualitySelection.value = 1;
                    lightingQualitySelection.value = 3;
                    shadowQualitySelection.value = 4;
                    animationQualitySelection.value = 1;
                    lodBiasSelection.value = 6;
                    break;
                case 5: // Very Low
                    antialiasingSelection.value = 0;
                    vsyncSelection.value = 0;
                    textureQualitySelection.value = 2;
                    lightingQualitySelection.value = 4;
                    shadowQualitySelection.value = 5;
                    animationQualitySelection.value = 1;
                    lodBiasSelection.value = 8;
                    break;
                case 6: // Custom
                    break;
                default:
                    break;
            }

            // Disable Flag so that upcoming value changes may change Preset to Custom
            isApplyingPreset = false;

            if (saveToPlayerPrefs)
            {
                Debug.Log("Quality Preset set in player prefs");
                PlayerPrefs.SetInt("qualityPreset", qualityPresetSelection.value);
            }
        }

        // If the selected option wasn't a preset, override the preset to Custom
        public void SetPresetToCustom()
        {
            if(!isApplyingPreset) qualityPresetSelection.value = 6;
        }

        public void SetAntiAliasing(bool saveToPlayerPrefs)
        {
            switch (antialiasingSelection.value)
            {
                case 0: // Disabled
                    QualitySettings.antiAliasing = 0;
                    break;
                case 1: // 2x Multi Sampling
                    QualitySettings.antiAliasing = 2;
                    break;
                case 2: // 4x Multi Sampling
                    QualitySettings.antiAliasing = 4;
                    break;
                case 3: // 8x Multi Sampling
                    QualitySettings.antiAliasing = 8;
                    break;
                default:
                    break;
            }

            SetPresetToCustom();

            if (saveToPlayerPrefs)
            {
                PlayerPrefs.SetInt("antiAliasing", antialiasingSelection.value);
            }
        }

        public void SetVSync(bool saveToPlayerPrefs)
        {
            switch (vsyncSelection.value)
            {
                case 0: // Off
                    QualitySettings.vSyncCount = 0;
                    break;
                case 1: // Full
                    QualitySettings.vSyncCount = 1;
                    break;
                case 2: // Half
                    QualitySettings.vSyncCount = 2;
                    break;
                default:
                    break;
            }

            SetPresetToCustom();

            if (saveToPlayerPrefs)
            {
                PlayerPrefs.SetInt("vsync", vsyncSelection.value);
            }
        }

        public void SetTextureQuality(bool saveToPlayerPrefs)
        {
            switch (textureQualitySelection.value)
            {
                case 0: // Full Res
                    QualitySettings.masterTextureLimit = 0; 
                    break;
                case 1: // Half Res
                    QualitySettings.masterTextureLimit = 1; 
                    break;
                case 2: // Quarter Res
                    QualitySettings.masterTextureLimit = 2; 
                    break;
                default:
                    break;
            }

            SetPresetToCustom();

            if (saveToPlayerPrefs)
            {
                PlayerPrefs.SetInt("textureQuality", textureQualitySelection.value);
            }
        }

        public void SetLightingQuality(bool saveToPlayerPrefs)
        {
            switch (lightingQualitySelection.value)
            {
                case 0: // Highest : Pixel Light Count 4
                    QualitySettings.pixelLightCount = 4;
                    break;
                case 1: // High : Pixel Light Count 3
                    QualitySettings.pixelLightCount = 3;
                    break;
                case 2: //  Medium : Pixel Light Count 2
                    QualitySettings.pixelLightCount = 2;
                    break;
                case 3: // Low : Pixel Light Count 1
                    QualitySettings.pixelLightCount = 1;
                    break;
                case 4: // Lowest : Pixel Light Count 0
                    QualitySettings.pixelLightCount = 0;
                    break;
                default:
                    break;
            }

            SetPresetToCustom();

            if (saveToPlayerPrefs)
            {
                PlayerPrefs.SetInt("lightingQuality", lightingQualitySelection.value);
            }
        }

        public void SetShadowQuality(bool saveToPlayerPrefs)
        {
            switch (shadowQualitySelection.value)
            {
                case 0: // Highest
                    QualitySettings.shadowmaskMode = ShadowmaskMode.DistanceShadowmask;
                    QualitySettings.shadows = ShadowQuality.All;
                    QualitySettings.shadowResolution = ShadowResolution.High;
                    QualitySettings.shadowDistance = 150f; 
                    QualitySettings.shadowCascades = 4;
                    break;
                case 1: // High
                    QualitySettings.shadowmaskMode = ShadowmaskMode.DistanceShadowmask;
                    QualitySettings.shadows = ShadowQuality.All;
                    QualitySettings.shadowResolution = ShadowResolution.High;
                    QualitySettings.shadowDistance = 70f;
                    QualitySettings.shadowCascades = 2;
                    break;
                case 2: //  Medium
                    QualitySettings.shadowmaskMode = ShadowmaskMode.DistanceShadowmask;
                    QualitySettings.shadows = ShadowQuality.All;
                    QualitySettings.shadowResolution = ShadowResolution.Medium;
                    QualitySettings.shadowDistance = 40f;
                    QualitySettings.shadowCascades = 2;
                    break;
                case 3: // Low
                    QualitySettings.shadowmaskMode = ShadowmaskMode.Shadowmask;
                    QualitySettings.shadows = ShadowQuality.HardOnly;
                    QualitySettings.shadowResolution = ShadowResolution.Low;
                    QualitySettings.shadowDistance = 40f;
                    QualitySettings.shadowCascades = 2;
                    break;
                case 4: // Very Low
                    QualitySettings.shadowmaskMode = ShadowmaskMode.Shadowmask;
                    QualitySettings.shadows = ShadowQuality.HardOnly;
                    QualitySettings.shadowResolution = ShadowResolution.Low;
                    QualitySettings.shadowDistance = 20f;
                    QualitySettings.shadowCascades = 0;
                    break;
                case 5: // Disabled
                    QualitySettings.shadowmaskMode = ShadowmaskMode.Shadowmask;
                    QualitySettings.shadows = ShadowQuality.Disable;
                    QualitySettings.shadowResolution = ShadowResolution.Low;
                    QualitySettings.shadowDistance = 20f;
                    QualitySettings.shadowCascades = 0;
                    break;
                default:
                    break;
            }

            SetPresetToCustom();

            if (saveToPlayerPrefs)
            {
                PlayerPrefs.SetInt("shadowQuality", shadowQualitySelection.value);
            }
        }

        public void SetAnimationQuality(bool saveToPlayerPrefs)
        {
            switch (animationQualitySelection.value)
            {
                case 0: // Maximum
                    QualitySettings.skinWeights = SkinWeights.Unlimited;
                    break;
                case 1: // High
                    QualitySettings.skinWeights = SkinWeights.FourBones;
                    break;
                case 2: //  Medium
                    QualitySettings.skinWeights = SkinWeights.TwoBones;
                    break;
                case 3: // Low
                    QualitySettings.skinWeights = SkinWeights.OneBone;
                    break;
                default:
                    break;
            }

            SetPresetToCustom();

            if (saveToPlayerPrefs)
            {
                PlayerPrefs.SetInt("animationQuality", animationQualitySelection.value);
            }
        }

        public void SetLODBias(bool saveToPlayerPrefs)
        {
            switch (lodBiasSelection.value)
            {
                case 0: //  10 (Best)
                    QualitySettings.lodBias = 15f;
                    break;
                case 1: // 8
                    QualitySettings.lodBias = 10f;
                    break;
                case 2: // 6
                    QualitySettings.lodBias = 7f;
                    break;
                case 3: // 4
                    QualitySettings.lodBias = 4f;
                    break;
                case 4: // 2
                    QualitySettings.lodBias = 2f;
                    break;
                case 5: // 1.5
                    QualitySettings.lodBias = 1.5f;
                    break;
                case 6: // 1
                    QualitySettings.lodBias = 1f;
                    break;
                case 7: // 0.7
                    QualitySettings.lodBias = 0.7f;
                    break;
                case 8: //  0.5 (Worst)
                    QualitySettings.lodBias = 0.5f;
                    break;
                default:
                    break;
            }

            SetPresetToCustom();

            if (saveToPlayerPrefs)
            {
                PlayerPrefs.SetInt("lodBias", lodBiasSelection.value);
            }
        }

        public void RestoreDefaultGraphicalSettings()
        {
            Debug.Log("RestoreDefaultGraphicalSettings called..");

            // Store the existing event listeners so we can reset them after disabling events and changing values
            var displayModeSelectionExistingListeners = displayModeSelection.onValueChanged;
            var resolutionSelectionExistingListeners = resolutionSelection.onValueChanged;
            var aspectRatioSelectionExistingListeners = aspectRatioSelection.onValueChanged;

            // Remove all listeners
            displayModeSelection.onValueChanged = new TMP_Dropdown.DropdownEvent();
            resolutionSelection.onValueChanged = new TMP_Dropdown.DropdownEvent();
            aspectRatioSelection.onValueChanged = new TMP_Dropdown.DropdownEvent();

            // Fullscreen
            displayModeSelection.value = 0;
            isFullscreen = true;

            // Aspect Ratio
            aspectRatioSelection.value = 0;
            aspectRatio = 169;

            // Resolution
            resolutionSelection.value = 2;
            resolution = 1080f;

            // Re-add the value changed listeners
            displayModeSelection.onValueChanged = displayModeSelectionExistingListeners;
            resolutionSelection.onValueChanged = resolutionSelectionExistingListeners;
            aspectRatioSelection.onValueChanged = aspectRatioSelectionExistingListeners;

            // Apply resolution saves settings to player prefs, so not needed for each setting above
            ApplyResolution(true);

            // Reset to Ultra settings
            qualityPresetSelection.value = 0;

            SetQualityPreset(true);
            isApplyingPreset = true;
            SetAntiAliasing(true);
            SetVSync(true);
            SetTextureQuality(true);
            SetLightingQuality(true);
            SetShadowQuality(true);
            SetAnimationQuality(true);
            SetLODBias(true);
            isApplyingPreset = false;

            SetSlidersBasedOnGraphics();
        }

        public void SetSlidersBasedOnGraphics()
        {
            // Values for Fullscren, aspect ratio and Resolution already Set in Restore Defaults          
        }

        #endregion

        #region Misc Options

        // Update the display tips value and flag whether it has changed or not
        public void SetTipsDisplay(bool saveToPlayerPrefs)
        {
            switch (tipsSelection.value)
            {
                case 0:
                    helpMenu.popups = true;
                    break;
                case 1:
                    helpMenu.popups = false;
                    break;
                default:
                    break;
            }

            if (saveToPlayerPrefs)
            {
                PlayerPrefs.SetInt("tipPopups", tipsSelection.value);
            }
        }

        public void RestoreDefaultMiscSettings()
        {
            // Reset values here
            tipsSelection.value = 0;
            SetTipsDisplay(true);

        }

        #endregion

        #region Etc.

        public void SetImageTransparencies()
        {
            // Save

            // Controls
                // Controller Type
                if(ControllerUIManager.instance.controllerTypePlaystation)
                {
                    xboxOption.color = semiTransparent;
                    playstationOption.color = opaque;
                }
                else
                {
                    xboxOption.color = opaque;
                    playstationOption.color = semiTransparent;
                }
            // Audio

            // Visual

            // Misc
        }

        // Drop Down Menus:

        public void ReselectOptionAfterDropdown()
        {
            if(this.gameObject.activeInHierarchy)
                StartCoroutine(SetSelectedObjectAfterDropdown());
        }

        private IEnumerator SetSelectedObjectAfterDropdown()
        {
            // Wait for the end of the frame to ensure the dropdown has finished processing
            yield return null;

            BackToPreviousSelectedOption();
        }
        #endregion
    }
}
