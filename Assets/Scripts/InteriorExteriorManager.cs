using UnityEngine;
using UnityEngine.UI;
using System.Collections;
#if UNITY_TMP
using TMPro;
#endif

/// <summary>
/// Manages switching between exterior and interior modes.
/// Handles fade transitions, location saving, and mode-specific features.
/// </summary>
public class InteriorExteriorManager : MonoBehaviour
{
    [Header("Mode State")]
    public bool isInteriorMode = false;
    public Vector3 savedExteriorPosition;
    public Quaternion savedExteriorRotation;
    
    [Header("References")]
    public Camera mainCamera;
    public CameraModeController cameraModeController;
    public DroneController droneController;
    public WalkController walkController;
    
    [Header("Interior Settings")]
    [Tooltip("Light intensity multiplier for interior mode")]
    public float interiorLightIntensity = 2f;
    
    [Tooltip("List of room entry points (doors/triggers)")]
    public RoomEntryPoint[] roomEntryPoints;
    
    [Header("UI")]
    public Button exitInteriorButton;
    public GameObject exitButtonContainer;
    public Button goBackButton;
    
    [Header("Interior Panel")]
    [Tooltip("Panel that displays interior information (building name, floor, etc.)")]
    public GameObject interiorPanel;
    
    [Tooltip("TextMeshPro or Text component in InteriorPanel to display location info")]
    public Component interiorPanelText;
    
    // Cache the actual text component to avoid repeated lookups
    private TMPro.TextMeshProUGUI cachedTMPUGUI;
    private TMPro.TextMeshPro cachedTMPPro;
    private Text cachedText;
    
    // Helper method to initialize cached text component references
    void InitializeTextComponentCache()
    {
        if (interiorPanelText == null) return;
        
        // Clear caches first
        cachedTMPUGUI = null;
        cachedTMPPro = null;
        cachedText = null;
        
        // Try TextMeshProUGUI first
        cachedTMPUGUI = interiorPanelText as TMPro.TextMeshProUGUI;
        if (cachedTMPUGUI == null)
        {
            // Try TextMeshPro
            cachedTMPPro = interiorPanelText as TMPro.TextMeshPro;
            if (cachedTMPPro == null)
            {
                // Try Unity Text
                cachedText = interiorPanelText as Text;
            }
        }
        
        // If casting didn't work, try getting from GameObject
        if (cachedTMPUGUI == null && cachedTMPPro == null && cachedText == null && interiorPanelText != null)
        {
            GameObject textObj = interiorPanelText.gameObject;
            cachedTMPUGUI = textObj.GetComponent<TMPro.TextMeshProUGUI>();
            if (cachedTMPUGUI == null)
            {
                cachedTMPPro = textObj.GetComponent<TMPro.TextMeshPro>();
                if (cachedTMPPro == null)
                {
                    cachedText = textObj.GetComponent<Text>();
                }
            }
        }
        
        Debug.Log($"[InteriorExteriorManager] Initialized text component cache - TMPUGUI: {(cachedTMPUGUI != null ? cachedTMPUGUI.gameObject.name : "NULL")}, TMPPro: {(cachedTMPPro != null ? cachedTMPPro.gameObject.name : "NULL")}, Text: {(cachedText != null ? cachedText.gameObject.name : "NULL")}");
    }
    
    [Header("Fade Settings")]
    public Image fadeImage; // Full-screen image for fading
    public float fadeDuration = 1f;
    public Color fadeColor = Color.black;
    
    [Header("Camera Movement")]
    [Tooltip("Speed for moving camera into interior (units per second)")]
    public float interiorMovementSpeed = 5f;
    
    [Header("Current Room")]
    public RoomData currentRoom;
    
    [Header("Door System")]
    public DoorRaycastSystem doorRaycastSystem;
    
    [Header("Systems")]
    [Tooltip("Reference to BuildingEntrySystem (for getting building names from doors)")]
    public BuildingEntrySystem buildingEntrySystem;
    
    [Tooltip("Reference to StairRaycastSystem (for getting current floor information)")]
    public StairRaycastSystem stairRaycastSystem;
    
    private bool isTransitioning = false;
    private DoorData entryDoor; // Door used to enter current interior
    private DoorData previousEntryDoor; // For secondary interiors
    private string currentBuildingName; // Name of building we're currently in
    
    void Start()
    {
        // Setup exit button
        if (exitInteriorButton != null)
        {
            exitInteriorButton.onClick.RemoveAllListeners();
            exitInteriorButton.onClick.AddListener(ExitInteriorMode);
        }

        if (goBackButton != null)
        {
            goBackButton.onClick.RemoveAllListeners();
            goBackButton.onClick.AddListener(HandleGoBackButton);
        }
        
        // Hide exit button initially
        if (exitButtonContainer != null)
        {
            exitButtonContainer.SetActive(false);
        }
        
        // Setup fade image
        if (fadeImage != null)
        {
            EnsureFadeImageIsFullScreen();
            fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, 0f);
            fadeImage.gameObject.SetActive(false);
        }
        
        // Find door raycast system if not assigned
        if (doorRaycastSystem == null)
        {
            doorRaycastSystem = FindFirstObjectByType<DoorRaycastSystem>();
        }
        
        // Find BuildingEntrySystem if not assigned
        if (buildingEntrySystem == null)
        {
            buildingEntrySystem = FindFirstObjectByType<BuildingEntrySystem>();
        }
        
        // Find StairRaycastSystem if not assigned
        if (stairRaycastSystem == null)
        {
            stairRaycastSystem = FindFirstObjectByType<StairRaycastSystem>();
        }
        
        // Auto-find interior panel text if not assigned
        if (interiorPanel != null && interiorPanelText == null)
        {
            // Try TextMeshProUGUI first (most common)
            TMPro.TextMeshProUGUI tmpUGUI = interiorPanel.GetComponentInChildren<TMPro.TextMeshProUGUI>();
            if (tmpUGUI != null)
            {
                interiorPanelText = tmpUGUI;
                Debug.Log($"[InteriorExteriorManager] Auto-found TextMeshProUGUI component in InteriorPanel: {tmpUGUI.gameObject.name}");
            }
            else
            {
                // Try TextMeshPro (world space)
                TMPro.TextMeshPro tmpPro = interiorPanel.GetComponentInChildren<TMPro.TextMeshPro>();
                if (tmpPro != null)
                {
                    interiorPanelText = tmpPro;
                    Debug.Log($"[InteriorExteriorManager] Auto-found TextMeshPro component in InteriorPanel: {tmpPro.gameObject.name}");
                }
                else
                {
                    // Try Unity Text as fallback
                    Text textComp = interiorPanel.GetComponentInChildren<Text>();
                    if (textComp != null)
                    {
                        interiorPanelText = textComp;
                        Debug.Log($"[InteriorExteriorManager] Auto-found Text component in InteriorPanel: {textComp.gameObject.name}");
                    }
                }
            }
            
            if (interiorPanelText == null)
            {
                Debug.LogWarning("[InteriorExteriorManager] Could not auto-find text component in InteriorPanel! Please assign it manually in the inspector.");
            }
        }
        
        // Initialize cached text component references
        InitializeTextComponentCache();
        
        // Hide interior panel initially
        if (interiorPanel != null)
        {
            interiorPanel.SetActive(false);
        }
        
        // Initialize all room entry points
        foreach (var entryPoint in roomEntryPoints)
        {
            if (entryPoint != null && entryPoint.triggerCollider != null)
            {
                SetupRoomTrigger(entryPoint);
            }
        }
    }

    void OnEnable()
    {
        if (fadeImage != null)
        {
            EnsureFadeImageIsFullScreen();
        }
    }

    void OnValidate()
    {
        if (fadeImage != null)
        {
            EnsureFadeImageIsFullScreen();
        }
    }

    private void EnsureFadeImageIsFullScreen()
    {
        RectTransform rt = fadeImage.rectTransform;
        if (rt == null) return;

        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta = Vector2.zero;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        fadeImage.raycastTarget = true;
    }
    
    void SetupRoomTrigger(RoomEntryPoint entryPoint)
    {
        // Ensure trigger is set up correctly
        if (entryPoint.triggerCollider != null)
        {
            entryPoint.triggerCollider.isTrigger = true;
            
            // Add RoomTrigger component if not present
            RoomTrigger trigger = entryPoint.triggerCollider.GetComponent<RoomTrigger>();
            if (trigger == null)
            {
                trigger = entryPoint.triggerCollider.gameObject.AddComponent<RoomTrigger>();
            }
            trigger.manager = this;
            trigger.roomData = entryPoint.roomData;
        }
    }
    
    public void EnterInteriorMode(RoomData room)
    {
        if (isTransitioning || isInteriorMode) return;
        
        StartCoroutine(EnterInteriorCoroutine(room));
    }
    
    /// <summary>
    /// Enter interior mode using door's entry position/rotation (simplified system)
    /// </summary>
    public void EnterInteriorMode(Vector3 entryPosition, Quaternion entryRotation, DoorData door)
    {
        if (isTransitioning || isInteriorMode) return;
        
        entryDoor = door;
        StartCoroutine(EnterInteriorCoroutine(entryPosition, entryRotation));
    }
    
    /// <summary>
    /// Enter secondary interior (nested interior)
    /// </summary>
    public void EnterSecondaryInterior(RoomData secondaryRoom, DoorData door)
    {
        if (isTransitioning || !isInteriorMode) return;
        
        // Save current entry door before entering secondary
        previousEntryDoor = entryDoor;
        entryDoor = door;
        
        StartCoroutine(EnterSecondaryInteriorCoroutine(secondaryRoom));
    }
    
    IEnumerator EnterInteriorCoroutine(RoomData room)
    {
        isTransitioning = true;
        currentRoom = room;
        
        // Save exterior position
        if (mainCamera != null)
        {
            savedExteriorPosition = mainCamera.transform.position;
            savedExteriorRotation = mainCamera.transform.rotation;
        }
        
        // Fade out
        yield return StartCoroutine(FadeOut());
        
        // Switch to interior mode
        isInteriorMode = true;
        
        // Enable interior lighting
        SetInteriorLighting(true);
        
        // Setup room boundaries
        SetupRoomBoundaries(room);
        
        // Show exit button
        if (exitButtonContainer != null)
        {
            exitButtonContainer.SetActive(true);
        }
        
        // Notify door raycast system of entry door (if available)
        if (doorRaycastSystem != null && entryDoor != null)
        {
            doorRaycastSystem.SetEntryDoor(entryDoor);
        }
        
        // Smoothly move camera to room entry position
        if (mainCamera != null && room != null)
        {
            yield return StartCoroutine(MoveCameraToInteriorPosition(room.entryPosition, room.entryRotation));
        }
        
        // Fade in
        yield return StartCoroutine(FadeIn());
        
        isTransitioning = false;
        Debug.Log($"[InteriorExteriorManager] Entered interior mode: {room.roomName}");
    }
    
    /// <summary>
    /// Enter interior coroutine using door's entry position/rotation (simplified system)
    /// </summary>
    IEnumerator EnterInteriorCoroutine(Vector3 entryPosition, Quaternion entryRotation)
    {
        isTransitioning = true;
        
        // Save exterior position
        if (mainCamera != null)
        {
            savedExteriorPosition = mainCamera.transform.position;
            savedExteriorRotation = mainCamera.transform.rotation;
        }
        
        // Fade out
        yield return StartCoroutine(FadeOut());
        
        // Switch to Walk mode if not already in Walk mode (interior mode requires Walk mode)
        if (cameraModeController != null && cameraModeController.currentMode != TourMode.Walk)
        {
            Debug.Log($"[InteriorExteriorManager] Switching to Walk mode before entering interior (current mode: {cameraModeController.currentMode})");
            cameraModeController.SwitchMode(TourMode.Walk);
            yield return new WaitForSeconds(0.2f); // Wait for mode transition
        }
        
        // Switch to interior mode
        isInteriorMode = true;
        
        // Enable interior lighting (basic - you may want to adjust this)
        SetInteriorLighting(true);
        
        // Show exit button
        if (exitButtonContainer != null)
        {
            exitButtonContainer.SetActive(true);
        }
        
        // Close sidebar when entering interior mode
        CloseSidebarOnInteriorEnter();
        
        // Get building name from entry door (if available)
        if (entryDoor != null && buildingEntrySystem != null)
        {
            currentBuildingName = buildingEntrySystem.GetBuildingNameForDoor(entryDoor);
            Debug.Log($"[InteriorExteriorManager] Entering building: {currentBuildingName}");
        }
        
        // Set WalkController height override to the entry floor height (if in Walk mode)
        if (walkController != null && cameraModeController != null && cameraModeController.currentMode == TourMode.Walk)
        {
            walkController.SetCameraHeightOverride(entryPosition.y);
            Debug.Log($"[InteriorExteriorManager] Set camera height override to {entryPosition.y} when entering interior");
        }
        
        // Smoothly move camera to entry position
        if (mainCamera != null)
        {
            yield return StartCoroutine(MoveCameraToInteriorPosition(entryPosition, entryRotation));
        }
        
        // Set initial floor if stair system is available
        // Try to find the ground floor stair (or first stair) as default
        if (stairRaycastSystem != null && stairRaycastSystem.stairs != null && stairRaycastSystem.stairs.Length > 0)
        {
            stairRaycastSystem.ResetUsedStairsFlag();

            // Find ground floor stair, or use first stair if not found
            StairData initialFloor = null;
            foreach (var stair in stairRaycastSystem.stairs)
            {
                if (stair != null && stair.stairType == StairType.GroundFloor)
                {
                    initialFloor = stair;
                    break;
                }
            }
            
            // If no ground floor stair found, use first stair
            if (initialFloor == null)
            {
                initialFloor = stairRaycastSystem.stairs[0];
            }
            
            if (initialFloor != null)
            {
                stairRaycastSystem.SetCurrentFloor(initialFloor);
                Debug.Log($"[InteriorExteriorManager] Set initial floor to: {initialFloor.floorName}");
            }
        }
        
        // Update interior panel with building name and initial floor
        UpdateInteriorPanel();
        
        // Fade in
        yield return StartCoroutine(FadeIn());
        
        // Re-enable WalkController after entering interior (should be in Walk mode now)
        if (cameraModeController != null && cameraModeController.walkController != null)
        {
            if (cameraModeController.currentMode == TourMode.Walk && isInteriorMode)
            {
                cameraModeController.walkController.enabled = true;
                Debug.Log($"[InteriorExteriorManager] Re-enabled WalkController after entering interior. Mode: {cameraModeController.currentMode}, isInteriorMode: {isInteriorMode}, WalkController enabled: {cameraModeController.walkController.enabled}");
            }
            else
            {
                Debug.LogWarning($"[InteriorExteriorManager] WalkController NOT enabled after entering interior. Mode: {cameraModeController.currentMode}, isInteriorMode: {isInteriorMode}");
            }
        }
        else
        {
            Debug.LogWarning("[InteriorExteriorManager] cameraModeController or walkController is null!");
        }
        
        isTransitioning = false;
        Debug.Log($"[InteriorExteriorManager] Entered interior mode via door: {entryDoor?.doorObject?.name}. isInteriorMode: {isInteriorMode}");
    }
    
    /// <summary>
    /// Updates the interior panel text with current building name and floor
    /// </summary>
    public void UpdateInteriorPanel()
    {
        if (interiorPanel == null) return;
        
        // Only show panel in interior mode
        if (!isInteriorMode)
        {
            interiorPanel.SetActive(false);
            return;
        }
        
        interiorPanel.SetActive(true);
        
        // Ensure text component is initialized
        if (interiorPanelText == null)
        {
            Debug.LogWarning("[InteriorExteriorManager] interiorPanelText is null! Cannot update text.");
            return;
        }
        
        // Re-initialize cache if needed
        if (cachedTMPUGUI == null && cachedTMPPro == null && cachedText == null)
        {
            InitializeTextComponentCache();
        }
        
        // Build display text using building name + floor name (if available)
        string buildingText = null;
        if (!string.IsNullOrEmpty(currentBuildingName))
        {
            buildingText = currentBuildingName;
            Debug.Log($"[InteriorExteriorManager] Using building name from BuildingEntrySystem: {currentBuildingName}");
        }

        string floorText = null;
        bool shouldUseStairsFloorName = stairRaycastSystem != null && stairRaycastSystem.HasUsedStairsToChangeFloor();

        if (!shouldUseStairsFloorName && entryDoor != null && !string.IsNullOrEmpty(entryDoor.floorName))
        {
            floorText = entryDoor.floorName;
        }
        else if (stairRaycastSystem != null)
        {
            StairData currentFloor = stairRaycastSystem.GetCurrentFloor();
            if (currentFloor != null && !string.IsNullOrEmpty(currentFloor.floorName))
            {
                floorText = currentFloor.floorName;
            }
        }

        string displayText;
        if (!string.IsNullOrEmpty(buildingText) && !string.IsNullOrEmpty(floorText))
        {
            displayText = $"{buildingText} - {floorText}";
        }
        else if (!string.IsNullOrEmpty(buildingText))
        {
            displayText = buildingText;
        }
        else if (!string.IsNullOrEmpty(floorText))
        {
            displayText = floorText;
        }
        else
        {
            Debug.LogWarning("[InteriorExteriorManager] No building name or floor name available");
            displayText = "Interior";
        }
        
        // Set text using cached component references
        bool textSet = false;
        
        if (cachedTMPUGUI != null)
        {
            cachedTMPUGUI.text = displayText;
            textSet = true;
            Debug.Log($"[InteriorExteriorManager] Set TextMeshProUGUI text to: {displayText}");
        }
        else if (cachedTMPPro != null)
        {
            cachedTMPPro.text = displayText;
            textSet = true;
            Debug.Log($"[InteriorExteriorManager] Set TextMeshPro text to: {displayText}");
        }
        else if (cachedText != null)
        {
            cachedText.text = displayText;
            textSet = true;
            Debug.Log($"[InteriorExteriorManager] Set Text text to: {displayText}");
        }
        else if (interiorPanelText != null)
        {
            // Fallback: try to get component from GameObject
            GameObject textObj = interiorPanelText.gameObject;
            
            TMPro.TextMeshProUGUI tmpUGUI = textObj.GetComponent<TMPro.TextMeshProUGUI>();
            if (tmpUGUI != null)
            {
                tmpUGUI.text = displayText;
                cachedTMPUGUI = tmpUGUI;
                textSet = true;
                Debug.Log($"[InteriorExteriorManager] Set TextMeshProUGUI text (via fallback) to: {displayText}");
            }
            else
            {
                TMPro.TextMeshPro tmpPro = textObj.GetComponent<TMPro.TextMeshPro>();
                if (tmpPro != null)
                {
                    tmpPro.text = displayText;
                    cachedTMPPro = tmpPro;
                    textSet = true;
                    Debug.Log($"[InteriorExteriorManager] Set TextMeshPro text (via fallback) to: {displayText}");
                }
                else
                {
                    Text textComp = textObj.GetComponent<Text>();
                    if (textComp != null)
                    {
                        textComp.text = displayText;
                        cachedText = textComp;
                        textSet = true;
                        Debug.Log($"[InteriorExteriorManager] Set Text text (via fallback) to: {displayText}");
                    }
                }
            }
        }
        
        if (!textSet)
        {
            Debug.LogWarning($"[InteriorExteriorManager] Could not set text on interiorPanelText! Component: {(interiorPanelText != null ? interiorPanelText.GetType().ToString() : "NULL")}, Display text: {displayText}");
        }
        else
        {
            Debug.Log($"[InteriorExteriorManager] Successfully updated interior panel text to: {displayText}");
        }
    }
    
    /// <summary>
    /// Enter secondary interior coroutine
    /// </summary>
    IEnumerator EnterSecondaryInteriorCoroutine(RoomData secondaryRoom)
    {
        isTransitioning = true;
        RoomData previousRoom = currentRoom;
        currentRoom = secondaryRoom;
        
        // Fade out
        yield return StartCoroutine(FadeOut());
        
        // Setup room boundaries for secondary interior
        SetupRoomBoundaries(secondaryRoom);
        
        // Remove boundaries from previous room
        if (previousRoom != null)
        {
            RemoveRoomBoundaries(previousRoom);
        }
        
        // Move camera to secondary interior entry position
        if (mainCamera != null && secondaryRoom != null)
        {
            yield return StartCoroutine(MoveCameraToInteriorPosition(secondaryRoom.entryPosition, secondaryRoom.entryRotation));
        }
        
        // Fade in
        yield return StartCoroutine(FadeIn());
        
        isTransitioning = false;
        Debug.Log($"[InteriorExteriorManager] Entered secondary interior: {secondaryRoom.roomName}");
    }
    
    public void ExitInteriorMode()
    {
        if (isTransitioning || !isInteriorMode) return;
        
        StartCoroutine(ExitInteriorCoroutine());
    }
    
    /// <summary>
    /// Exit interior mode to a specific position and rotation (used when exiting through a door)
    /// </summary>
    public void ExitInteriorModeToPosition(Vector3 exitPosition, Quaternion exitRotation)
    {
        if (isTransitioning || !isInteriorMode) return;
        
        StartCoroutine(ExitInteriorCoroutine(exitPosition, exitRotation));
    }

    public void HandleGoBackButton()
    {
        if (isTransitioning || !isInteriorMode) return;

        bool wentDown = stairRaycastSystem != null && stairRaycastSystem.TryGoDownOneFloor();
        if (wentDown)
        {
            return;
        }

        ExitInteriorMode();
    }
    
    IEnumerator ExitInteriorCoroutine()
    {
        yield return StartCoroutine(ExitInteriorCoroutine(Vector3.zero, Quaternion.identity, useSpecificPosition: false));
    }
    
    /// <summary>
    /// Exit interior coroutine with optional specific exit position/rotation
    /// </summary>
    IEnumerator ExitInteriorCoroutine(Vector3 specificExitPosition, Quaternion specificExitRotation, bool useSpecificPosition = true)
    {
        isTransitioning = true;
        
        // Fade out
        yield return StartCoroutine(FadeOut());
        
        // Check if we're exiting from secondary interior
        if (previousEntryDoor != null)
        {
            // Exiting secondary interior - go back to previous interior
            currentRoom = null; // Will be set by previous room
            entryDoor = previousEntryDoor;
            previousEntryDoor = null;
            
            // Move camera back to previous interior position (using entry door's entry position)
            if (mainCamera != null && entryDoor != null)
            {
                yield return StartCoroutine(MoveCameraToInteriorPosition(entryDoor.entryPosition, Quaternion.Euler(entryDoor.entryRotation)));
            }
        }
        else
        {
            // Exiting to exterior
            Vector3 exitPosition = savedExteriorPosition;
            Quaternion exitRotation = savedExteriorRotation;
            
            // Use specific position if provided, otherwise use door's exit position, otherwise use saved position
            if (useSpecificPosition && specificExitPosition != Vector3.zero)
            {
                exitPosition = specificExitPosition;
                exitRotation = specificExitRotation;
            }
            else if (entryDoor != null)
            {
                if (entryDoor.exitPosition != Vector3.zero)
                {
                    exitPosition = entryDoor.exitPosition;
                }
                if (entryDoor.exitRotation != Vector3.zero)
                {
                    exitRotation = Quaternion.Euler(entryDoor.exitRotation);
                }
            }
            
            // Move camera to exit position
            if (mainCamera != null)
            {
                yield return StartCoroutine(MoveCameraToInteriorPosition(exitPosition, exitRotation));
            }
            
            // Disable interior lighting
            SetInteriorLighting(false);
            
            // Remove room boundaries
            if (currentRoom != null)
            {
                RemoveRoomBoundaries(currentRoom);
            }
            
            // Switch to exterior mode
            isInteriorMode = false;
            currentRoom = null;
            entryDoor = null;
            currentBuildingName = null;
            
            // Hide interior panel when exiting
            if (interiorPanel != null)
            {
                interiorPanel.SetActive(false);
            }
            
            // Clear camera height override when exiting to exterior (return to default ground level)
            if (walkController != null)
            {
                walkController.ClearCameraHeightOverride();
            }
            
            // Notify door raycast system that we're back in exterior
            if (doorRaycastSystem != null)
            {
                doorRaycastSystem.SetEntryDoor(null);
            }
            
            Debug.Log("[InteriorExteriorManager] Set isInteriorMode = false, sidebar should now be allowed to open");
        }
        
        // Hide exit button (only if fully exiting to exterior)
        if (!isInteriorMode && exitButtonContainer != null)
        {
            exitButtonContainer.SetActive(false);
        }
        
        // Fade in
        yield return StartCoroutine(FadeIn());
        
        // Re-enable WalkController after exiting interior (if in Walk mode)
        // Do this AFTER fade so the state is correct
        if (cameraModeController != null && cameraModeController.walkController != null && cameraModeController.currentMode == TourMode.Walk)
        {
            cameraModeController.walkController.enabled = true;
            Debug.Log("[InteriorExteriorManager] Re-enabled WalkController after exiting interior (isInteriorMode: " + isInteriorMode + ")");
        }
        
        isTransitioning = false;
        Debug.Log("[InteriorExteriorManager] Exited interior mode. isInteriorMode: " + isInteriorMode);
    }
    
    IEnumerator FadeOut()
    {
        if (fadeImage == null) yield break;

        EnsureFadeImageIsFullScreen();
        fadeImage.gameObject.SetActive(true);
        fadeImage.transform.SetAsLastSibling();
        float elapsed = 0f;
        
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Clamp01(elapsed / fadeDuration);
            fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, alpha);
            yield return null;
        }
        
        fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, 1f);
    }
    
    IEnumerator FadeIn()
    {
        if (fadeImage == null) yield break;
        
        float elapsed = 0f;
        
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Clamp01(1f - (elapsed / fadeDuration));
            fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, alpha);
            yield return null;
        }
        
        fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, 0f);
        fadeImage.gameObject.SetActive(false);
    }
    
    /// <summary>
    /// Public method to perform fade out (for use by other systems like stairs)
    /// </summary>
    public IEnumerator PerformFadeOut()
    {
        yield return StartCoroutine(FadeOut());
    }
    
    /// <summary>
    /// Public method to perform fade in (for use by other systems like stairs)
    /// </summary>
    public IEnumerator PerformFadeIn()
    {
        yield return StartCoroutine(FadeIn());
    }
    
    /// <summary>
    /// Smoothly interpolates camera from current position to interior entry position
    /// </summary>
    IEnumerator MoveCameraToInteriorPosition(Vector3 targetPosition, Quaternion targetRotation)
    {
        if (mainCamera == null)
        {
            Debug.LogWarning("[InteriorExteriorManager] Main camera is null, cannot move to interior position");
            yield break;
        }
        
        // Disable controllers during movement to prevent interference
        if (cameraModeController != null)
        {
            if (cameraModeController.walkController != null)
            {
                cameraModeController.walkController.enabled = false;
                Debug.Log("[InteriorExteriorManager] Disabled WalkController during camera movement");
            }
            if (cameraModeController.droneController != null)
            {
                cameraModeController.droneController.enabled = false;
            }
            if (cameraModeController.camera2DController != null)
            {
                cameraModeController.camera2DController.enabled = false;
            }
        }
        
        Vector3 startPos = mainCamera.transform.position;
        Quaternion startRot = mainCamera.transform.rotation;
        
        float distance = Vector3.Distance(startPos, targetPosition);
        float duration = distance / interiorMovementSpeed;
        float elapsed = 0f;
        
        Debug.Log($"[InteriorExteriorManager] Moving camera from {startPos} to {targetPosition}, rotation from {startRot.eulerAngles} to {targetRotation.eulerAngles}, duration: {duration}s");
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            t = Mathf.SmoothStep(0f, 1f, t); // Smooth ease in/out
            
            // Linearly interpolate position
            mainCamera.transform.position = Vector3.Lerp(startPos, targetPosition, t);
            
            // Spherically interpolate rotation for smooth rotation
            mainCamera.transform.rotation = Quaternion.Slerp(startRot, targetRotation, t);
            
            yield return null;
        }
        
        // Ensure exact final position and rotation
        mainCamera.transform.position = targetPosition;
        mainCamera.transform.rotation = targetRotation;
        
        Debug.Log($"[InteriorExteriorManager] Camera movement completed. Final position: {mainCamera.transform.position}, Final rotation: {mainCamera.transform.rotation.eulerAngles}");
        
        // NOTE: WalkController will be re-enabled in EnterInteriorCoroutine after fade completes
        // Don't re-enable here because isInteriorMode might not be set yet
    }
    
    void SetInteriorLighting(bool enable)
    {
        // Find all lights in the scene
        Light[] allLights = FindObjectsByType<Light>(FindObjectsSortMode.None);
        
        foreach (Light light in allLights)
        {
            // Only affect lights that are part of the current room
            if (currentRoom != null && currentRoom.roomLights != null)
            {
                foreach (Light roomLight in currentRoom.roomLights)
                {
                    if (light == roomLight)
                    {
                        if (enable)
                        {
                            light.intensity *= interiorLightIntensity;
                        }
                        else
                        {
                            light.intensity /= interiorLightIntensity;
                        }
                    }
                }
            }
        }
        
        // Also adjust ambient light
        if (enable)
        {
            RenderSettings.ambientIntensity *= interiorLightIntensity;
        }
        else
        {
            RenderSettings.ambientIntensity /= interiorLightIntensity;
        }
    }
    
    void SetupRoomBoundaries(RoomData room)
    {
        if (room == null) return;
        
        // Handle mesh colliders
        if (room.useMeshColliders && room.roomMeshParent != null)
        {
            Collider[] meshColliders = room.roomMeshParent.GetComponentsInChildren<Collider>();
            foreach (Collider col in meshColliders)
            {
                if (col != null)
                {
                    col.enabled = true;
                }
            }
        }
        
        // Handle boundary colliders
        if (room.boundaryColliders != null)
        {
            foreach (Collider col in room.boundaryColliders)
            {
                if (col != null)
                {
                    col.enabled = true;
                }
            }
        }
    }
    
    void RemoveRoomBoundaries(RoomData room)
    {
        if (room == null) return;
        
        // Handle mesh colliders
        if (room.useMeshColliders && room.roomMeshParent != null)
        {
            Collider[] meshColliders = room.roomMeshParent.GetComponentsInChildren<Collider>();
            foreach (Collider col in meshColliders)
            {
                if (col != null)
                {
                    col.enabled = false;
                }
            }
        }
        
        // Handle boundary colliders
        if (room.boundaryColliders != null)
        {
            foreach (Collider col in room.boundaryColliders)
            {
                if (col != null)
                {
                    col.enabled = false;
                }
            }
        }
    }
    
    public bool IsInteriorMode()
    {
        return isInteriorMode;
    }
    
    /// <summary>
    /// Closes sidebar when entering interior mode
    /// </summary>
    void CloseSidebarOnInteriorEnter()
    {
        // Close sidebar in CameraModeController
        if (cameraModeController != null)
        {
            cameraModeController.CloseSidebarPublic();
        }
        
        // Also close sidebar in NavigationUIManager if it exists
        NavigationUIManager navUIManager = FindFirstObjectByType<NavigationUIManager>();
        if (navUIManager != null)
        {
            // NavigationUIManager doesn't have a public close method, but OpenSidebar checks interior mode now
        }
    }
}


