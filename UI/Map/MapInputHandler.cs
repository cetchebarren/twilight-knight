using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using TMPro;
using System.Linq;

namespace etchebarren 
{
    public class MapInputHandler : MonoBehaviour
    {
        public static MapInputHandler instance;

        MapControls mapInputActions;
        public Camera mapCamera;
        public GameObject mapCursor;
        public GameObject mapCursorIcon; // For setting layer
        [SerializeField] private Vector3 mapCursorOriginalScale;
        public Transform player;
        public PlayerMenuManager playerMenuManager;
        public UIChangeSelectedButton uiChangeSelectedButton;
        public InputHandler inputHandler;
        public WorldStateManager worldStateManager;
        public ControllerUIManager controllerUIManager;
        public GameObject local;
        public Image localButtonImage;
        public GameObject world;
        public Image worldButtonImage;
        public Button localButton;
        public Button worldButton;
        public Transform worldMapStartingPos;
        public GameObject localMapCompass;
        public GameObject confirmTravelWindow;
        public TextMeshProUGUI confirmTravelText;
        public GameObject cancelButton;
        public OptionsManager optionsManager;
        public SaveManager saveManager;

        [Header("Fine Tuning Variables")]
        public float cursorSnapSpeed = 50.0f;
        public float mapCameraMoveSpeed = 50.0f;
        public float scrollWheelZoomSpeed = 150.0f;
        public float zoomSpeed = 150.0f;
        public float minCameraZoom = 50.0f;
        public float maxCameraZoom = 250.0f;
        public float maxCameraZoomWorld = 160.0f;
        public float zoomReorientToPlayerSpeedTriggers = 50f; // this is the speed that the camera centers in on the player when zooming
        public float zoomReorientToPlayerSpeedScroll = 1250f; // this is the speed that the camera centers in on the player when zooming
        public Vector3 sensitivity = new Vector3(500f, 500f, 500f);
        public LayerMask localMapCullingMask;
        public LayerMask worldMapCullingMask;
        public LayerMask mapRaycastLayers;

        [Header("Icons/GameObjects to Scale with zoom")]
        public GameObject minimapPlayer;
        [SerializeField] private Vector3 minimapPlayerOriginalScale;
        private bool originalScalesRecorded = false;

        [Header("INPUTS")]
        [SerializeField] Vector2 cursorInput;
        [SerializeField] Vector2 mousePos;
        [SerializeField] Vector2 scrollWheel;
        [SerializeField] bool mouseClick = false;
        [SerializeField] bool cameraZoomIn = false;
        [SerializeField] bool cameraZoomOut = false;
        [SerializeField] bool switchToLocal = false;
        [SerializeField] bool switchToWorld = false;
        [SerializeField] bool reset = false;

        [Header("Do Not Set Manually")]
        [SerializeField] bool localMap = true;
        [SerializeField] Vector3 scaledSensitivity;
        [SerializeField] float halfWidth = 720.0f; // Placeholder value
        [SerializeField] float halfHeight = 720.0f; // Placeholder value
        [SerializeField] private Button buttonUnderCursor;
        [SerializeField] bool useController = false;
        [SerializeField] TeleportWaypoint lastSelectedWaypoint;
        [SerializeField] WorldMapTeleportButton lastSelectedWorldLocation;

        // Stored for next frame
        [SerializeField] Vector2 overflow;

        void Awake()
        {
            if (instance == null)
            {
                instance = this;
            }
            else if (instance != this)
            {
                Destroy(gameObject);
            }

            RecordOriginalScales();
            gameObject.SetActive(false);
            this.enabled = false;
        }

        void OnEnable()
        {
            if (mapInputActions == null)
            {
                mapInputActions = new MapControls();
                mapInputActions.MapInput.CursorMovement.performed += i => cursorInput = i.ReadValue<Vector2>();
                mapInputActions.MapInput.CursorClick.performed += i => mouseClick = true;

                mapInputActions.MapInput.MousePosition.performed += i => mousePos = i.ReadValue<Vector2>();

                mapInputActions.MapInput.ScrollWheelZoom.performed += i => scrollWheel = i.ReadValue<Vector2>();

                mapInputActions.MapInput.ZoomIn.performed += i => cameraZoomIn = true;
                mapInputActions.MapInput.ZoomIn.canceled += i => cameraZoomIn = false;

                mapInputActions.MapInput.ZoomOut.performed += i => cameraZoomOut = true;
                mapInputActions.MapInput.ZoomOut.canceled += i => cameraZoomOut = false;

                mapInputActions.MapInput.Reset.performed += i => reset = true;

                mapInputActions.MapInput.SwitchToLocal.performed += i => switchToLocal = true;
                mapInputActions.MapInput.SwitchToWorld.performed += i => switchToWorld = true;
            }
            // Calculate the half width and half height of the screen
            halfWidth = Screen.width * 0.5f;
            halfHeight = Screen.height * 0.5f;
            // Enable the controls
            mapInputActions.Enable();
            // Get initial mouse position
            mousePos = Mouse.current.position.ReadValue();
        }

        public void OnDisable()
        {
            if (mapInputActions != null)
                mapInputActions.Disable();
        }

        void Update()
        {
            if (confirmTravelWindow.activeSelf)
            {
                buttonUnderCursor = null;
                switchToLocal = false;
                switchToWorld = false;
                return;
            }
            HandleResetInput();
            HandleSwitchToLocal();
            HandleSwitchToWorld();
            HandleClickInput();
            HandleZoom();
            //HandleScrollWheel();
            Raycast();

            bool controller = controllerUIManager.isUsingController();

            if (cursorInput.magnitude < 0.1f && controller)
            {
                if (buttonUnderCursor != null)
                {
                    // Move the mapCursor to buttonUnderCursor.gameObject.transformPosition at an adjustable speed
                    // Calculate the direction from the current mapCursor position to the button position
                    Vector3 directionToButton = (buttonUnderCursor.transform.position - mapCursor.transform.position).normalized;

                    // Calculate the new position of the map cursor towards the button
                    Vector3 newPos = mapCursor.transform.position + directionToButton * Time.unscaledDeltaTime * cursorSnapSpeed;
                    newPos.y = mapCursor.transform.position.y; // Keep the current y-position
                                                               // Move the map cursor towards the button position
                    mapCursor.transform.position = newPos;
                }
                return;
            }

            Vector3 newPosition;

            // Calculate the bounds based on the camera's position
            float halfHeight = mapCamera.orthographicSize;
            float halfWidth = halfHeight * mapCamera.aspect;

            // Get the camera's current position
            Vector3 cameraPosition = mapCamera.transform.position;

            float horizontalMovement;
            float verticalMovement;

            if (!controller)
            {
                // Check if the cursor is near the edge of the screen
                // Define the percentage threshold for near the edge of the screen
                float edgeThresholdPercentage = 0.01f; // (1% of the screen width and height)

                // Calculate the threshold distance based on the screen size and percentage
                float edgeThresholdX = Screen.width * edgeThresholdPercentage;
                float edgeThresholdY = Screen.height * edgeThresholdPercentage;
                // Check if the cursor is near the edge of the screen
                bool nearEdge = Mathf.Abs(mousePos.x - Screen.width / 2f) >= Screen.width / 2f - edgeThresholdX ||
                                Mathf.Abs(mousePos.y - Screen.height / 2f) >= Screen.height / 2f - edgeThresholdY;

                // Apply offset only if the cursor is near the edge
                if (nearEdge)
                {
                    //Debug.Log("Near edge");
                    // Get the center of the screen
                    Vector2 screenCenter = new Vector3(Screen.width / 2f, Screen.height / 2f);
                    // Calculate the direction from the screen center to the mouse position
                    Vector2 cursorDirection = mousePos - screenCenter;
                    cursorDirection.Normalize();

                    scaledSensitivity = sensitivity * mapCamera.orthographicSize / 100.0f; // When changing the zoom, sensitivity needs to adjust respectively

                    // Calculate cursor movement based on mouse position relative to screen venter
                    Vector3 mouseMovement = new Vector3(cursorDirection.x * scaledSensitivity.x, 0f, cursorDirection.y * scaledSensitivity.z) * Time.unscaledDeltaTime;

                    // Calculate the new position of the map cursor in world space
                    newPosition = mapCursor.transform.position;
                    Vector3 newPos = newPosition + mouseMovement;

                    // Move the camera on the horizontal plane in the direction that the cursor is trying to move off-screen
                    if (newPos.x - cameraPosition.x <= -halfWidth || newPos.x - cameraPosition.x >= halfWidth)
                    {
                        horizontalMovement = cursorDirection.x * Time.unscaledDeltaTime * mapCameraMoveSpeed;
                        cameraPosition.x += horizontalMovement;
                    }

                    if (newPos.z - cameraPosition.z <= -halfHeight || newPos.z - cameraPosition.z >= halfHeight)
                    {
                        verticalMovement = cursorDirection.y * Time.unscaledDeltaTime * mapCameraMoveSpeed;
                        cameraPosition.z += verticalMovement;
                    }
                    newPosition = mapCamera.ScreenToWorldPoint(mousePos);
                }

                //Debug.Log("No edge");
                // If the cursor is not near the edge, set the new position directly under the mouse
                newPosition = mapCamera.ScreenToWorldPoint(mousePos);
                newPosition.y = mapCursor.transform.position.y;
                
            }
            else
            {
                //float standard = localMap ? 100.0f : 160.0f;
                scaledSensitivity = sensitivity * mapCamera.orthographicSize / 100.0f; // When changing the zoom, sensitivity needs to adjust respectively

                // Calculate cursor movement based on joystick input and sensitivity
                Vector3 cursorMovement = new Vector3(cursorInput.x * scaledSensitivity.x, 0f, cursorInput.y * scaledSensitivity.z) * Time.unscaledDeltaTime;

                // Calculate the new position of the map cursor in world space
                newPosition = mapCursor.transform.position + cursorMovement;

                // Move the camera on the horizontal plane in the direction that the cursor is trying to move off-screen
                if (newPosition.x - cameraPosition.x <= -halfWidth || newPosition.x - cameraPosition.x >= halfWidth)
                {
                    horizontalMovement = cursorInput.x * Time.unscaledDeltaTime * mapCameraMoveSpeed;
                    cameraPosition.x += horizontalMovement;
                }

                if (newPosition.z - cameraPosition.z <= -halfHeight || newPosition.z - cameraPosition.z >= halfHeight)
                {
                    verticalMovement = cursorInput.y * Time.unscaledDeltaTime * mapCameraMoveSpeed;
                    cameraPosition.z += verticalMovement;
                }


            }

            // Recalculate the bounds based on the updated camera position
            halfHeight = mapCamera.orthographicSize;
            halfWidth = halfHeight * mapCamera.aspect;

            newPosition.x = Mathf.Clamp(newPosition.x, cameraPosition.x - halfWidth, cameraPosition.x + halfWidth);
            newPosition.z = Mathf.Clamp(newPosition.z, cameraPosition.z - halfHeight, cameraPosition.z + halfHeight);

            // Set the new position of the map cursor and camera
            mapCursor.transform.position = newPosition;
            mapCamera.transform.position = cameraPosition;
        }

        public void Raycast()
        {
            // Two raycasts, one up and one down
            RaycastHit hitDown;
            RaycastHit hitUp;
            Vector3 upwardRaycastOffset = new Vector3(0f, -1f, 0f);

            if (Physics.Raycast(mapCursor.transform.position, Vector3.down, out hitDown, Mathf.Infinity, mapRaycastLayers))
            {
                // If the downward raycast hits something, check if it's a button
                buttonUnderCursor = hitDown.collider.GetComponent<Button>();
                if (buttonUnderCursor != null)
                {
                    // A button was hit downward, perform desired action
                    // Debug.Log("Button hit: " + buttonUnderCursor.gameObject.name);
                    Debug.Log("Button hit with down raycast");
                }
            }
            else if (Physics.Raycast(mapCursor.transform.position + upwardRaycastOffset, Vector3.up, out hitUp, Mathf.Infinity, mapRaycastLayers))
            {
                // If the upward raycast hits something, check if it's a button
                buttonUnderCursor = hitUp.collider.GetComponent<Button>();
                if (buttonUnderCursor != null)
                {
                    // A button was hit upward, perform desired action
                    // Debug.Log("Button hit: " + buttonUnderCursor.gameObject.name);
                    Debug.Log("Button hit with up raycast");
                }
            }           
            else
            {
                buttonUnderCursor = null;
            }
        }


        public void HandleClickInput()
        {
            if (mouseClick)
            {
                mouseClick = false;

                SimulateMouseClick();
            }
        }

        public void HandleResetInput()
        {
            if (reset)
            {
                reset = false;
                ResetMapPositionAndZoom(localMap);
            }
        }

        public void SimulateMouseClick()
        {
            // Create a mouse click input event and trigger it
            if (buttonUnderCursor != null)
            {
                Debug.Log("Mouse clicked! Pressed:" + buttonUnderCursor);
                // Trigger button click event
                buttonUnderCursor.onClick.Invoke();
            }
            else
            {
                Debug.Log("Mouse clicked! No button detected.");
            }
        }

        public void SetMapCameraActive(bool status) // Map Opened/Closed
        {
            // Clear any possible windows
            CancelTravel();
            // Reset Position of camera and map cursor based on player
            SwitchToLocalMap();

            mapCamera.enabled = status;
            mapCursor.SetActive(status);

            // Reset Icon Scales
            WorldStateManager.instance.currentTeleportWaypointsManager.RestoreScaleForAllTeleportWaypoints();
            SetIconScales(1.0f);

            // Controls HUD
            if (status)
            {
                controllerUIManager.SetSelectText("Select");
                controllerUIManager.SetTouchpadText("Close");
                controllerUIManager.SetRightStickText("Reset");
                if (inputHandler.usingController)
                {
                    controllerUIManager.SetLeftTriggerText("Zoom Out");
                    controllerUIManager.SetRightTriggerText("Zoom In");
                }
                else
                {
                    controllerUIManager.SetScrollWheelText("Zoom");
                }

            }
            else
            {
                controllerUIManager.SelectTextActive(false);
                controllerUIManager.TouchpadTextActive(false);
                controllerUIManager.LeftTriggerTextActive(false);
                controllerUIManager.RightTriggerTextActive(false);
                controllerUIManager.ScrollWheelTextActive(false);
                controllerUIManager.RightStickTextActive(false);
            }

        }

        public void HandleZoom()
        {
            float targetSize = mapCamera.orthographicSize;
            float currentZoomSpeed = zoomSpeed;

            if (cameraZoomIn)
            {
                targetSize -= zoomSpeed * Time.unscaledDeltaTime;
                ReorientTowardsCursorDuringZoom(zoomReorientToPlayerSpeedTriggers);
            }
            else if (cameraZoomOut)
            {
                targetSize += zoomSpeed * Time.unscaledDeltaTime;
                //ReorientTowardsPlayerDuringZoom();
                if (localMap) ReorientTowardsPlayerDuringZoom(zoomReorientToPlayerSpeedTriggers);
            }
            else if (scrollWheel.y > 0.1f) // zooming in
            {
                targetSize -= scrollWheelZoomSpeed * Time.unscaledDeltaTime;
                currentZoomSpeed = scrollWheelZoomSpeed;
                ReorientTowardsCursorDuringZoom(zoomReorientToPlayerSpeedScroll);
            }
            else if (scrollWheel.y < -0.1f) // zooming out
            {
                targetSize += scrollWheelZoomSpeed * Time.unscaledDeltaTime;
                currentZoomSpeed = scrollWheelZoomSpeed;
                if(localMap) ReorientTowardsPlayerDuringZoom(zoomReorientToPlayerSpeedScroll);
            }
            else
            {
                return;
            }

            // Clamp the targetSize to ensure it stays within the specified bounds
            float maxZoom = localMap ? maxCameraZoom : maxCameraZoomWorld;
            targetSize = Mathf.Clamp(targetSize, minCameraZoom, maxZoom);

            // Smoothly interpolate to the targetSize
            mapCamera.orthographicSize = Mathf.MoveTowards(mapCamera.orthographicSize, targetSize, currentZoomSpeed * Time.unscaledDeltaTime);

            // Clamp cursor position within bounds
            Vector3 cursorPos = mapCursor.transform.position;
            float halfHeight = mapCamera.orthographicSize;
            float halfWidth = halfHeight * mapCamera.aspect;
            cursorPos.x = Mathf.Clamp(cursorPos.x, mapCamera.transform.position.x - halfWidth, mapCamera.transform.position.x + halfWidth);
            cursorPos.z = Mathf.Clamp(cursorPos.z, mapCamera.transform.position.z - halfHeight, mapCamera.transform.position.z + halfHeight);
            mapCursor.transform.position = cursorPos;

            // Set scale of all teleport waypoints in the scene
            //float standard = localMap ? 100.0f : 160.0f;
            float multiplier = mapCamera.orthographicSize / 100.0f;
            WorldStateManager.instance.currentTeleportWaypointsManager.SetScaleForAllTeleportWaypoints(multiplier);
            SetIconScales(multiplier);
        }

        public void ReorientTowardsPlayerDuringZoom(float speed)
        {
            // Smoothly interpolate the mapCamera position towards the player position while maintaining y-coordinate
            float step = speed * Time.unscaledDeltaTime;
            Vector3 targetPosition = player.transform.position;
            targetPosition.y = mapCamera.transform.position.y; // Maintain the same y-coordinate
            mapCamera.transform.position = Vector3.MoveTowards(mapCamera.transform.position, targetPosition, step);
        }

        public void ReorientTowardsCursorDuringZoom(float speed)
        {
            // Smoothly interpolate the mapCamera position towards the player position while maintaining y-coordinate
            float step = speed * Time.unscaledDeltaTime;
            Vector3 targetPosition = mapCursor.transform.position;
            targetPosition.y = mapCamera.transform.position.y; // Maintain the same y-coordinate
            mapCamera.transform.position = Vector3.MoveTowards(mapCamera.transform.position, targetPosition, step);
        }

        private void RecordOriginalScales()
        {
            originalScalesRecorded = true;
            // Remember original scales (changes with zoom)
            mapCursorOriginalScale = mapCursor.transform.localScale;
            minimapPlayerOriginalScale = minimapPlayer.transform.localScale;
        }

        public void SetIconScales(float multiplier)
        {
            //if (!originalScalesRecorded) RecordOriginalScales();
            minimapPlayer.transform.localScale = minimapPlayerOriginalScale * multiplier;
            mapCursor.transform.localScale = mapCursorOriginalScale * multiplier;
        }

        // Changing Map Type

        public void HandleSwitchToLocal()
        {
            if (switchToLocal)
            {
                switchToLocal = false;

                localButton.onClick.Invoke();
                
            }
        }

        public void HandleSwitchToWorld()
        {
            if (switchToWorld)
            {
                switchToWorld = false;

                worldButton.onClick.Invoke();
                
            }
        }

        public void SwitchToLocalMap()
        {
            localMap = true;
            // Switch Selected Button Colors
            localButtonImage.color = Color.white;
            worldButtonImage.color = Color.grey;
            // Change order in hierarchy for layering
            world.transform.SetAsFirstSibling();
            // Enable compass
            localMapCompass.SetActive(true);

            // Change camera culling mask
            mapCamera.cullingMask = localMapCullingMask;
            mapCursorIcon.layer = 25; // Local Map uses minimap layer (layer 25)

            ResetMapPositionAndZoom(true); // Bool true is local

            float multiplier = mapCamera.orthographicSize / 100.0f;
            SetIconScales(multiplier);
        }

        public void SwitchToWorldMap()
        {
            localMap = false;
            // Switch Selected Button Colors
            localButtonImage.color = Color.grey;
            worldButtonImage.color = Color.white;
            // Change order in hierarchy for layering
            local.transform.SetAsFirstSibling();
            // Disable compass
            localMapCompass.SetActive(false);

            // Change camera culling mask
            mapCamera.cullingMask = worldMapCullingMask;
            mapCursorIcon.layer = 30; // World Map uses worldmap layer (layer 30)

            ResetMapPositionAndZoom(false); // Bool false is world

            float multiplier = mapCamera.orthographicSize / 100.0f;
            SetIconScales(multiplier);
        }

        public void ResetMapPositionAndZoom(bool local)
        {
            if (local)
            {
                // Reset Position of camera and map cursor based on player
                mapCursor.transform.position = new Vector3(player.position.x, mapCursor.transform.position.y, player.position.z);
                mapCamera.transform.position = new Vector3(player.position.x, mapCamera.transform.position.y, player.position.z);
                mapCamera.orthographicSize = 100.0f; // Reset Zoom
            }
            else
            {
                // Reset Position of camera and map cursor based on player
                mapCursor.transform.position = new Vector3(worldMapStartingPos.position.x, mapCursor.transform.position.y, worldMapStartingPos.position.z);
                mapCamera.transform.position = new Vector3(worldMapStartingPos.position.x, mapCamera.transform.position.y, worldMapStartingPos.position.z);
                mapCamera.orthographicSize = 160.0f; // Reset Zoom
            }
            float multiplier = mapCamera.orthographicSize / 100.0f;
            WorldStateManager.instance.currentTeleportWaypointsManager.SetScaleForAllTeleportWaypoints(multiplier);
            SetIconScales(multiplier);
        }

        public void OpenConfirmationWindow(TeleportWaypoint selectedWaypoint)
        {
            lastSelectedWorldLocation = null;
            lastSelectedWaypoint = selectedWaypoint;
            confirmTravelText.text = "Fast travel to\n<b>" + selectedWaypoint.locationName + "</b>?";
            confirmTravelWindow.SetActive(true);
            uiChangeSelectedButton.ChangeSelectedButtonTo(cancelButton);
        }

        public void OpenConfirmationWindow(WorldMapTeleportButton selectedWorldLocation)
        {
            lastSelectedWaypoint = null;
            lastSelectedWorldLocation = selectedWorldLocation;
            confirmTravelText.text = "Fast travel to\n<b>" + selectedWorldLocation.locationName + "</b>?";
            confirmTravelWindow.SetActive(true);
            uiChangeSelectedButton.ChangeSelectedButtonTo(cancelButton);
        }

        public void ConfirmTravel()
        {
            // Check saving
            if (saveManager.saving != null)
            {
                TextNotificationsManager.instance.NewTextNotifaction("Cannot teleport while saving. Please wait.");
                return;
            }

            // Local Fast Travel
            if (lastSelectedWaypoint != null)
            {
                //Debug.Log("Local travel");
                TeleportWaypoint temp = lastSelectedWaypoint;
                lastSelectedWaypoint = null;
                lastSelectedWorldLocation = null;
                confirmTravelWindow.SetActive(false);
                temp.TeleportPlayer();
                mouseClick = false;
            }
            // World Fast Travel (Between Scenes)
            else if(lastSelectedWorldLocation != null)
            {
                //Debug.Log("World travel");
                WorldMapTeleportButton temp = lastSelectedWorldLocation;
                lastSelectedWorldLocation = null;
                confirmTravelWindow.SetActive(false);
                mouseClick = false;
                playerMenuManager.CloseMapMenu();
                playerMenuManager.ClosePlayerMenu();
                worldStateManager.ChangeScene(temp.sceneIndex, true);
            }
        }

        public void CancelTravel()
        {
            lastSelectedWaypoint = null;
            lastSelectedWorldLocation = null;
            confirmTravelWindow.SetActive(false);
            mouseClick = false;
        }
    }
}
