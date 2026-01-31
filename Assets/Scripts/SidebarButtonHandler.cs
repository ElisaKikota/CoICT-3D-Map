using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Simple handler for manually placed sidebar buttons
/// Moves camera to highlighter GameObject(s) in drone mode and makes them flash
/// </summary>
public class SidebarButtonHandler : MonoBehaviour
{
    [Header("Highlighter Reference")]
    [Tooltip("The highlighter GameObject to move to. For collections, you can assign multiple highlighters in the array below instead.")]
    public GameObject targetHighlighter;
    
    [Header("Collection Highlighters (For multiple items like Studying Areas, Car Parkings)")]
    [Tooltip("If this is a collection, assign all highlighters here. Leave empty for single highlighter.")]
    public GameObject[] collectionHighlighters;
    
    [Header("Camera Settings")]
    [Tooltip("Reference to CameraMovementController (optional, will find if not assigned)")]
    public CameraMovementController cameraMovementController;
    
    [Tooltip("Reference to CameraModeController (optional, will find if not assigned)")]
    public CameraModeController cameraModeController;
    
    [Header("Best Camera Position (Use Absolute Position Instead of Offset)")]
    [Tooltip("Best camera position for viewing this building. Set this directly instead of using offset.")]
    public Vector3 bestCameraPosition = Vector3.zero;
    
    [Tooltip("If bestCameraPosition is zero, fallback to using offset from highlighter")]
    public Vector3 cameraOffset = new Vector3(0f, 10f, -15f);
    
    [Tooltip("Use absolute position (bestCameraPosition) or calculate from offset")]
    public bool useAbsolutePosition = true;
    
    [Tooltip("Height to reach before horizontal movement (in drone mode). Keep this low (30 or less)")]
    public float intermediateHeight = 30f;
    
    [Tooltip("Speed of camera movement")]
    public float movementSpeed = 8f;
    
    [Header("Camera Rotation")]
    [Tooltip("Y-axis rotation (yaw) for camera viewing angle. Different for each building.")]
    [Range(0f, 360f)]
    public float cameraYRotation = 0f;
    
    [Header("Multiple Highlighters Handling")]
    [Tooltip("If targetHighlighter has children, move to center of all children")]
    public bool useCenterOfChildren = true;
    
    [Header("Highlighter Glow Settings")]
    [Tooltip("Duration of the glow effect in seconds")]
    public float glowDuration = 5f;
    
    [Tooltip("Speed of the sine wave glow (how fast it pulses)")]
    public float glowSpeed = 2f;
    
    [Tooltip("Minimum glow intensity (emission strength)")]
    public float minGlowIntensity = 0.5f;
    
    [Tooltip("Maximum glow intensity (emission strength)")]
    public float maxGlowIntensity = 3f;
    
    [Tooltip("Glow color (emission color)")]
    public Color glowColor = Color.cyan;
    
    [Header("All Highlighters Reference")]
    [Tooltip("Parent GameObject containing all highlighter objects. If not assigned, will search for 'Highlighters' in scene.")]
    public GameObject allHighlightersParent;
    
    [Header("Bottom Sheet Integration")]
    [Tooltip("Reference to BottomSheetManager to show building details when button is clicked")]
    public BottomSheetManager bottomSheetManager;
    
    [Tooltip("Whether to show the bottom sheet when this button is clicked")]
    public bool showBottomSheet = true;
    
    [Tooltip("Whether to show the 'Enter Building' button in the bottom sheet (only for buildings with interiors)")]
    public bool showEnterBuilding = false;
    
    [Header("Building Information (for Bottom Sheet)")]
    [Tooltip("Building name to display in bottom sheet")]
    public string buildingName = "";
    
    [Tooltip("Building description to display in bottom sheet")]
    [TextArea(3, 5)]
    public string buildingDescription = "";
    
    [Tooltip("Building features text (e.g., 'Contains: X, Y, Z')")]
    [TextArea(2, 4)]
    public string buildingFeatures = "";
    
    [Header("Enter Building Settings (Walk Mode)")]
    [Tooltip("Camera position when 'Enter Building' button is pressed (looking at door in walk mode)")]
    public Vector3 enterBuildingCameraPosition = Vector3.zero;
    
    [Tooltip("Camera rotation when 'Enter Building' button is pressed (looking at door in walk mode)")]
    public Vector3 enterBuildingCameraRotation = Vector3.zero;
    
    private Button button;
    private Coroutine flashCoroutine;
    private List<GameObject> allHighlighters = new List<GameObject>();
    private List<Renderer> activeHighlighters = new List<Renderer>();
    
    void Start()
    {
        // Get button component
        button = GetComponent<Button>();
        if (button != null)
        {
            button.onClick.AddListener(OnButtonClicked);
        }
        else
        {
            Debug.LogWarning($"[SidebarButtonHandler] No Button component found on {gameObject.name}!");
        }
        
        // Find references if not assigned
        if (cameraMovementController == null)
        {
            cameraMovementController = FindFirstObjectByType<CameraMovementController>();
        }
        
        if (cameraModeController == null)
        {
            cameraModeController = FindFirstObjectByType<CameraModeController>();
        }
        
        // Find bottom sheet manager if not assigned
        if (bottomSheetManager == null)
        {
            bottomSheetManager = FindFirstObjectByType<BottomSheetManager>();
        }
        
        // Find all highlighters parent if not assigned
        if (allHighlightersParent == null)
        {
            GameObject highlightersObj = GameObject.Find("Highlighters");
            if (highlightersObj != null)
            {
                allHighlightersParent = highlightersObj;
            }
        }
        
        // Collect all highlighter GameObjects
        CollectAllHighlighters();
        
        // Turn off all highlighters at startup - they should only be active when needed
        TurnOffAllHighlighters();
    }
    
    /// <summary>
    /// Collects all highlighter GameObjects from the scene
    /// </summary>
    void CollectAllHighlighters()
    {
        allHighlighters.Clear();
        
        if (allHighlightersParent != null)
        {
            // Get all children of the highlighters parent
            foreach (Transform child in allHighlightersParent.transform)
            {
                allHighlighters.Add(child.gameObject);
            }
        }
        else
        {
            // Search for objects with "Highlighter" in name
            GameObject[] allObjects = FindObjectsByType<GameObject>(FindObjectsSortMode.None);
            foreach (GameObject obj in allObjects)
            {
                if (obj.name.Contains("Highlighter") || obj.name.Contains("highlighter"))
                {
                    allHighlighters.Add(obj);
                }
            }
        }
        
        Debug.Log($"[SidebarButtonHandler] Found {allHighlighters.Count} highlighter objects");
    }
    
    void OnButtonClicked()
    {
        Debug.Log($"[SidebarButtonHandler] Button clicked: {gameObject.name}");
        Debug.Log($"[SidebarButtonHandler] useAbsolutePosition: {useAbsolutePosition}, bestCameraPosition: {bestCameraPosition}, cameraYRotation: {cameraYRotation}");

        // Close sidebar immediately when selecting an item
        if (cameraModeController != null)
        {
            cameraModeController.CloseSidebarPublic();
        }
        
        // Get building data first
        BuildingData dataToShow = GetBuildingData();
        
        // Always move camera if we have a target position (either from highlighters or bestCameraPosition)
        bool hasTarget = targetHighlighter != null;
        bool hasCollection = collectionHighlighters != null && collectionHighlighters.Length > 0;
        bool hasBestPosition = bestCameraPosition != Vector3.zero;
        
        Debug.Log($"[SidebarButtonHandler] hasTarget: {hasTarget}, hasCollection: {hasCollection}, hasBestPosition: {hasBestPosition}");
        
        // Move camera if we have any target (highlighter or best position)
        // The coroutine will handle camera movement first, then show bottom sheet
        if (hasTarget || hasCollection || hasBestPosition)
        {
            // Start the button click routine as a coroutine to handle mode switching smoothly
            StartCoroutine(OnButtonClickedCoroutine());
        }
        else
        {
            Debug.LogWarning($"[SidebarButtonHandler] No camera target set for button {gameObject.name}. Please assign targetHighlighter or set bestCameraPosition.");
            // If no camera movement, still show bottom sheet immediately (if enabled)
            if (showBottomSheet && bottomSheetManager != null && dataToShow != null)
            {
                bottomSheetManager.ShowBuildingDetails(dataToShow, this);
                Debug.Log($"[SidebarButtonHandler] Showing bottom sheet for building: {dataToShow.name}");
            }
        }
    }
    
    /// <summary>
    /// Creates BuildingData from inspector fields (always uses fields, no external BuildingData needed)
    /// </summary>
    BuildingData GetBuildingData()
    {
        // Create BuildingData from inspector fields
        if (!string.IsNullOrEmpty(buildingName))
        {
            // Use target highlighter position if available, otherwise Vector3.zero (position field is not used)
            Vector3 buildingPos = targetHighlighter != null ? targetHighlighter.transform.position : Vector3.zero;
            
            BuildingData data = new BuildingData(
                buildingName,
                buildingDescription,
                buildingPos, // Use highlighter position or zero (field is not actively used)
                bestCameraPosition != Vector3.zero ? bestCameraPosition : Vector3.zero, // Use bestCameraPosition as cameraViewPosition
                targetHighlighter != null ? targetHighlighter : null
            );
            
            // Set additional fields
            data.features = buildingFeatures;
            data.enterBuildingCameraPosition = enterBuildingCameraPosition;
            data.enterBuildingCameraRotation = enterBuildingCameraRotation;
            data.bestViewPosition = bestCameraPosition;
            data.bestViewRotation = new Vector3(30f, cameraYRotation, 0f);
            
            Debug.Log($"[SidebarButtonHandler] Created BuildingData from inspector fields: {data.name}, Features: {data.features}, Enter Position: {data.enterBuildingCameraPosition}");
            return data;
        }
        
        Debug.LogWarning($"[SidebarButtonHandler] Cannot create BuildingData - buildingName is empty for button {gameObject.name}");
        return null;
    }
    
    IEnumerator OnButtonClickedCoroutine()
    {
        Debug.Log($"[SidebarButtonHandler] OnButtonClickedCoroutine started for button {gameObject.name}");
        
        // Stop any old Highlighter components that might be interfering
        StopOldHighlighterComponents();
        
        // Stop any existing glow
        StopFlash();
        
        // Get target position first (before turning off highlighters)
        Vector3 targetPosition = GetTargetPosition();
        
        if (targetPosition == Vector3.zero)
        {
            Debug.LogWarning($"[SidebarButtonHandler] Target position is zero! Check bestCameraPosition or highlighters assignment.");
            yield break;
        }
        
        Debug.Log($"[SidebarButtonHandler] Target position: {targetPosition}");
        
        // Turn off all other highlighters (but preserve the ones we want to activate)
        TurnOffAllHighlightersExceptTarget();
        
        // Check if we need to transition from 2D to Drone mode
        bool transitioningFrom2D = false;
        float startingPitch = 30f; // Default to 30 degrees (drone mode)
        
        // Temporarily disable DroneController BEFORE mode switch to signal that we're handling movement
        DroneController droneController = null;
        bool wasDroneControllerEnabled = false;
        if (cameraModeController != null && cameraModeController.droneController != null)
        {
            droneController = cameraModeController.droneController;
            wasDroneControllerEnabled = droneController.enabled;
            droneController.enabled = false; // Disable BEFORE SwitchMode so it knows to skip automatic movement
            Debug.Log($"[SidebarButtonHandler] Disabled DroneController before mode switch to prevent automatic movement");
        }
        
        // Check if we need to transition from 2D to Drone mode
        if (cameraModeController != null && cameraModeController.currentMode != TourMode.Drone)
        {
            transitioningFrom2D = (cameraModeController.currentMode == TourMode.Mode2D);
            
            Camera mainCamera = Camera.main;
            if (mainCamera != null && transitioningFrom2D)
            {
                // Capture current camera rotation before mode switch (for smooth interpolation from 90 to 30 degrees)
                Quaternion preSwitchRotation = mainCamera.transform.rotation;
                startingPitch = preSwitchRotation.eulerAngles.x;
                if (startingPitch > 180f) startingPitch -= 360f;
            }
            
            Debug.Log($"[SidebarButtonHandler] Switching to Drone mode (from {cameraModeController.currentMode})");
            // Switch mode (it will detect DroneController is disabled and skip automatic movement)
            cameraModeController.SwitchMode(TourMode.Drone);
            
            // Wait for mode switch to complete, then disable DroneController again (SwitchMode re-enables it)
            yield return null;
            yield return null;
            
            // Disable DroneController again after SwitchMode re-enabled it
            if (cameraModeController != null && cameraModeController.droneController != null)
            {
                cameraModeController.droneController.enabled = false;
                Debug.Log($"[SidebarButtonHandler] Disabled DroneController again after mode switch");
            }
        }
        
        // Ensure DroneController is disabled during movement (in case it got re-enabled)
        if (cameraModeController != null && cameraModeController.droneController != null)
        {
            cameraModeController.droneController.enabled = false;
        }
        
        // Move camera to target
        // Only interpolate X rotation from 90 to 30 if transitioning from 2D mode
        // Otherwise, keep X rotation at 30 degrees
        if (transitioningFrom2D)
        {
            Debug.Log($"[SidebarButtonHandler] Moving camera with pitch interpolation from {startingPitch} to 30");
            yield return StartCoroutine(MoveCameraDirectlyWithStartingPitch(targetPosition, startingPitch));
        }
        else
        {
            Debug.Log($"[SidebarButtonHandler] Moving camera directly to {targetPosition}");
            yield return StartCoroutine(MoveCameraDirectly(targetPosition));
        }
        
        // Start glowing the target highlighter(s) if we have any
        bool hasTarget = targetHighlighter != null;
        bool hasCollection = collectionHighlighters != null && collectionHighlighters.Length > 0;
        if (hasTarget || hasCollection)
        {
            StartFlash();
        }
        
        string targetName = collectionHighlighters != null && collectionHighlighters.Length > 0 
            ? $"Collection ({collectionHighlighters.Length} items)" 
            : (targetHighlighter != null ? targetHighlighter.name : "Position Only");
        Debug.Log($"[SidebarButtonHandler] Camera movement completed. Moved to {targetName} at position {targetPosition}");
        
        // Now show bottom sheet with building details (after camera movement completes) - only if enabled
        if (showBottomSheet && bottomSheetManager != null)
        {
            BuildingData buildingData = GetBuildingData();
            bottomSheetManager.ShowBuildingDetails(buildingData, this);
            Debug.Log($"[SidebarButtonHandler] Showing bottom sheet for building: {buildingData.name}");
        }
        else if (!showBottomSheet)
        {
            Debug.Log($"[SidebarButtonHandler] Bottom sheet disabled for button {gameObject.name}");
        }
    }
    
    /// <summary>
    /// Stops any old Highlighter components that might be interfering
    /// </summary>
    void StopOldHighlighterComponents()
    {
        // Find all Highlighter components in the scene
        Highlighter[] oldHighlighters = FindObjectsByType<Highlighter>(FindObjectsSortMode.None);
        
        foreach (Highlighter oldHighlighter in oldHighlighters)
        {
            if (oldHighlighter != null)
            {
                // Stop any active highlighting from the old script
                oldHighlighter.StopHighlight();
                
                // Disable the component to prevent it from interfering
                oldHighlighter.enabled = false;
                
                Debug.Log($"[SidebarButtonHandler] Stopped and disabled old Highlighter component on {oldHighlighter.gameObject.name}");
            }
        }
    }
    
    /// <summary>
    /// Turns off all highlighter objects except the ones we want to activate
    /// </summary>
    void TurnOffAllHighlightersExceptTarget()
    {
        // Collect the highlighters we want to keep active
        HashSet<GameObject> targetHighlighters = new HashSet<GameObject>();
        
        if (collectionHighlighters != null && collectionHighlighters.Length > 0)
        {
            foreach (GameObject h in collectionHighlighters)
            {
                if (h != null)
                {
                    targetHighlighters.Add(h);
                }
            }
        }
        else if (targetHighlighter != null)
        {
            targetHighlighters.Add(targetHighlighter);
            
            // Also add children if using center of children
            if (useCenterOfChildren && targetHighlighter.transform.childCount > 0)
            {
                foreach (Transform child in targetHighlighter.transform)
                {
                    if (child.gameObject != null)
                    {
                        targetHighlighters.Add(child.gameObject);
                    }
                }
            }
        }
        
        // Turn off all highlighters in the list EXCEPT the target ones
        foreach (GameObject highlighter in allHighlighters)
        {
            if (highlighter != null && !targetHighlighters.Contains(highlighter))
            {
                highlighter.SetActive(false);
            }
        }
        
        // Turn off children of targetHighlighter if NOT using center of children
        if (targetHighlighter != null && !useCenterOfChildren && targetHighlighter.transform.childCount > 0)
        {
            foreach (Transform child in targetHighlighter.transform)
            {
                if (child.gameObject != null && !targetHighlighters.Contains(child.gameObject))
                {
                    child.gameObject.SetActive(false);
                }
            }
        }
    }
    
    /// <summary>
    /// Turns off all highlighter objects (used when stopping glow)
    /// </summary>
    void TurnOffAllHighlighters()
    {
        // Turn off all highlighters in the list
        foreach (GameObject highlighter in allHighlighters)
        {
            if (highlighter != null)
            {
                highlighter.SetActive(false);
            }
        }
        
        // Turn off collection highlighters if assigned
        if (collectionHighlighters != null && collectionHighlighters.Length > 0)
        {
            foreach (GameObject highlighter in collectionHighlighters)
            {
                if (highlighter != null)
                {
                    highlighter.SetActive(false);
                }
            }
        }
        
        // Also turn off any children if targetHighlighter is a parent (fallback)
        if (targetHighlighter != null && useCenterOfChildren && targetHighlighter.transform.childCount > 0)
        {
            foreach (Transform child in targetHighlighter.transform)
            {
                if (child.gameObject != null)
                {
                    child.gameObject.SetActive(false);
                }
            }
        }
        
        // Turn off single target highlighter (fallback)
        if (targetHighlighter != null)
        {
            targetHighlighter.SetActive(false);
        }
        
        // Also turn off children of collection highlighters
        if (collectionHighlighters != null && collectionHighlighters.Length > 0)
        {
            foreach (GameObject highlighter in collectionHighlighters)
            {
                if (highlighter != null && highlighter.transform.childCount > 0)
                {
                    foreach (Transform child in highlighter.transform)
                    {
                        if (child.gameObject != null)
                        {
                            child.gameObject.SetActive(false);
                        }
                    }
                }
            }
        }
    }
    
    /// <summary>
    /// Public method to start highlight glow from external sources (e.g., BottomSheetManager)
    /// </summary>
    public void StartHighlightGlow()
    {
        // Ensure this GameObject is active (coroutines can't start on inactive GameObjects)
        if (!gameObject.activeInHierarchy)
        {
            Debug.LogWarning($"[SidebarButtonHandler] Cannot start glow - GameObject '{gameObject.name}' is inactive!");
            return;
        }
        
        // Stop any existing glow
        StopFlash();
        
        // Turn off all other highlighters except target
        TurnOffAllHighlightersExceptTarget();
        
        // Start the glow
        StartFlash();
    }
    
    /// <summary>
    /// Starts the glow effect on the target highlighter(s)
    /// </summary>
    void StartFlash()
    {
        activeHighlighters.Clear();
        
        // Check if we're using a collection (array of highlighters assigned directly)
        if (collectionHighlighters != null && collectionHighlighters.Length > 0)
        {
            // Collection mode - use directly assigned highlighters (easier to handle)
            foreach (GameObject highlighter in collectionHighlighters)
            {
                if (highlighter != null)
                {
                    // Ensure parent is active (if highlighter has a parent)
                    if (highlighter.transform.parent != null)
                    {
                        highlighter.transform.parent.gameObject.SetActive(true);
                    }
                    
                    // Activate the highlighter itself
                    highlighter.SetActive(true);
                    
                    // Start AlphaWaveAnimator if component exists
                    AlphaWaveAnimator alphaAnimator = highlighter.GetComponent<AlphaWaveAnimator>();
                    if (alphaAnimator == null)
                    {
                        alphaAnimator = highlighter.GetComponentInChildren<AlphaWaveAnimator>();
                    }
                    if (alphaAnimator != null)
                    {
                        alphaAnimator.StartAnimation();
                    }
                    
                    // Get renderer from the highlighter
                    Renderer renderer = highlighter.GetComponent<Renderer>();
                    if (renderer == null)
                    {
                        renderer = highlighter.GetComponent<SpriteRenderer>();
                    }
                    
                    // Also check for renderers in children (nested structure)
                    if (renderer == null)
                    {
                        Renderer[] childRenderers = highlighter.GetComponentsInChildren<Renderer>(true); // Include inactive
                        foreach (Renderer childRenderer in childRenderers)
                        {
                            if (childRenderer != null)
                            {
                                // Activate the child if it's inactive
                                if (!childRenderer.gameObject.activeSelf)
                                {
                                    childRenderer.gameObject.SetActive(true);
                                }
                                activeHighlighters.Add(childRenderer);
                            }
                        }
                    }
                    else
                    {
                        activeHighlighters.Add(renderer);
                    }
                }
            }
        }
        // Fallback: Check if targetHighlighter has children (old method)
        else if (targetHighlighter != null && useCenterOfChildren && targetHighlighter.transform.childCount > 0)
        {
            // Multiple highlighters - activate all children
            foreach (Transform child in targetHighlighter.transform)
            {
                if (child.gameObject != null)
                {
                    child.gameObject.SetActive(true);
                    
                    // Start AlphaWaveAnimator if component exists
                    AlphaWaveAnimator alphaAnimator = child.GetComponent<AlphaWaveAnimator>();
                    if (alphaAnimator == null)
                    {
                        alphaAnimator = child.GetComponentInChildren<AlphaWaveAnimator>();
                    }
                    if (alphaAnimator != null)
                    {
                        alphaAnimator.StartAnimation();
                    }
                    
                    Renderer renderer = child.GetComponent<Renderer>();
                    if (renderer == null)
                    {
                        renderer = child.GetComponent<SpriteRenderer>();
                    }
                    
                    if (renderer == null)
                    {
                        Renderer[] childRenderers = child.GetComponentsInChildren<Renderer>();
                        foreach (Renderer childRenderer in childRenderers)
                        {
                            if (childRenderer != null)
                            {
                                activeHighlighters.Add(childRenderer);
                            }
                        }
                    }
                    else
                    {
                        activeHighlighters.Add(renderer);
                    }
                }
            }
        }
        // Single highlighter
        else if (targetHighlighter != null)
        {
            targetHighlighter.SetActive(true);
            
            // Start AlphaWaveAnimator if component exists
            AlphaWaveAnimator alphaAnimator = targetHighlighter.GetComponent<AlphaWaveAnimator>();
            if (alphaAnimator == null)
            {
                alphaAnimator = targetHighlighter.GetComponentInChildren<AlphaWaveAnimator>();
            }
            if (alphaAnimator != null)
            {
                alphaAnimator.StartAnimation();
            }
            
            Renderer renderer = targetHighlighter.GetComponent<Renderer>();
            if (renderer == null)
            {
                renderer = targetHighlighter.GetComponent<SpriteRenderer>();
            }
            
            if (renderer == null)
            {
                Renderer[] childRenderers = targetHighlighter.GetComponentsInChildren<Renderer>();
                foreach (Renderer childRenderer in childRenderers)
                {
                    if (childRenderer != null)
                    {
                        activeHighlighters.Add(childRenderer);
                    }
                }
            }
            else
            {
                activeHighlighters.Add(renderer);
            }
        }
        
        Debug.Log($"[SidebarButtonHandler] Starting glow on {activeHighlighters.Count} highlighter(s)");
        
        // Start glow coroutine
        if (activeHighlighters.Count > 0)
        {
            flashCoroutine = StartCoroutine(FlashHighlighters());
        }
        else
        {
            Debug.LogWarning($"[SidebarButtonHandler] No renderers found. Check highlighters assignment.");
        }
    }
    
    /// <summary>
    /// Stops the glow effect
    /// </summary>
    void StopFlash()
    {
        if (flashCoroutine != null)
        {
            StopCoroutine(flashCoroutine);
            flashCoroutine = null;
        }
        
        // Stop AlphaWaveAnimator on all highlighters (safely, only if component exists and is initialized)
        if (collectionHighlighters != null && collectionHighlighters.Length > 0)
        {
            foreach (GameObject highlighter in collectionHighlighters)
            {
                if (highlighter != null && highlighter.activeInHierarchy)
                {
                    AlphaWaveAnimator alphaAnimator = highlighter.GetComponent<AlphaWaveAnimator>();
                    if (alphaAnimator == null)
                    {
                        alphaAnimator = highlighter.GetComponentInChildren<AlphaWaveAnimator>();
                    }
                    if (alphaAnimator != null && alphaAnimator.enabled)
                    {
                        try
                        {
                            alphaAnimator.StopAnimation();
                        }
                        catch (System.Exception e)
                        {
                            Debug.LogWarning($"[SidebarButtonHandler] Error stopping AlphaWaveAnimator on {highlighter.name}: {e.Message}");
                        }
                    }
                }
            }
        }
        else if (targetHighlighter != null && targetHighlighter.activeInHierarchy)
        {
            AlphaWaveAnimator alphaAnimator = targetHighlighter.GetComponent<AlphaWaveAnimator>();
            if (alphaAnimator == null)
            {
                alphaAnimator = targetHighlighter.GetComponentInChildren<AlphaWaveAnimator>();
            }
            if (alphaAnimator != null && alphaAnimator.enabled)
            {
                try
                {
                    alphaAnimator.StopAnimation();
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning($"[SidebarButtonHandler] Error stopping AlphaWaveAnimator on {targetHighlighter.name}: {e.Message}");
                }
            }
        }
        
        // Clear the active highlighters list
        // The coroutine will restore original values when it stops
        activeHighlighters.Clear();
    }
    
    /// <summary>
    /// Coroutine that creates a glowing effect using sine wave
    /// Controls emission/glow intensity that increases and decreases smoothly
    /// </summary>
    IEnumerator FlashHighlighters()
    {
        float elapsedTime = 0f;
        MaterialPropertyBlock propertyBlock = new MaterialPropertyBlock();
        
        // Store original emission colors and intensities
        Dictionary<Renderer, Color> originalEmissionColors = new Dictionary<Renderer, Color>();
        Dictionary<Renderer, float> originalEmissionIntensities = new Dictionary<Renderer, float>();
        Dictionary<SpriteRenderer, Color> originalSpriteColors = new Dictionary<SpriteRenderer, Color>();
        
        // Get original emission values
        foreach (Renderer renderer in activeHighlighters)
        {
            if (renderer != null)
            {
                if (renderer is SpriteRenderer spriteRenderer)
                {
                    originalSpriteColors[spriteRenderer] = spriteRenderer.color;
                }
                else
                {
                    renderer.GetPropertyBlock(propertyBlock);
                    Color originalEmission = propertyBlock.GetColor("_EmissionColor");
                    if (originalEmission == Color.clear)
                    {
                        originalEmission = Color.black;
                    }
                    originalEmissionColors[renderer] = originalEmission;
                    
                    // Try to get original intensity
                    float intensity = propertyBlock.GetFloat("_EmissionIntensity");
                    if (intensity == 0f)
                    {
                        intensity = 1f; // Default
                    }
                    originalEmissionIntensities[renderer] = intensity;
                }
            }
        }
        
        float startTime = Time.time;
        
        while (elapsedTime < glowDuration)
        {
            // Calculate glow intensity using sine wave (oscillates between min and max)
            // Sine wave goes from -1 to 1, we map it to 0-1, then lerp between min and max
            float sineValue = Mathf.Sin((Time.time - startTime) * glowSpeed);
            float normalizedSine = (sineValue + 1f) / 2f; // Map from [-1, 1] to [0, 1]
            float glowIntensity = Mathf.Lerp(minGlowIntensity, maxGlowIntensity, normalizedSine);
            
            // Apply glow effect to all active highlighters
            foreach (Renderer renderer in activeHighlighters)
            {
                if (renderer != null)
                {
                    if (renderer is SpriteRenderer spriteRenderer)
                    {
                        // For sprite renderers, multiply color by glow intensity
                        Color originalColor = originalSpriteColors.ContainsKey(spriteRenderer) 
                            ? originalSpriteColors[spriteRenderer] 
                            : spriteRenderer.color;
                        Color glowColorWithIntensity = glowColor * glowIntensity;
                        spriteRenderer.color = originalColor + glowColorWithIntensity;
                    }
                    else
                    {
                        // For 3D renderers, use emission color and intensity
                        renderer.GetPropertyBlock(propertyBlock);
                        
                        // Set emission color with intensity
                        Color emissionColor = glowColor * glowIntensity;
                        propertyBlock.SetColor("_EmissionColor", emissionColor);
                        propertyBlock.SetColor("_Emission", emissionColor);
                        
                        // Set emission intensity if shader supports it
                        propertyBlock.SetFloat("_EmissionIntensity", glowIntensity);
                        propertyBlock.SetFloat("_EmissionPower", glowIntensity);
                        
                        // Enable emission (HDR)
                        propertyBlock.SetFloat("_EmissionScaleUI", glowIntensity);
                        
                        // Also try common glow properties
                        propertyBlock.SetFloat("_GlowIntensity", glowIntensity);
                        propertyBlock.SetColor("_GlowColor", emissionColor);
                        
                        renderer.SetPropertyBlock(propertyBlock);
                    }
                }
            }
            
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        // Restore original emission values
        foreach (Renderer renderer in activeHighlighters)
        {
            if (renderer != null)
            {
                if (renderer is SpriteRenderer spriteRenderer && originalSpriteColors.ContainsKey(spriteRenderer))
                {
                    spriteRenderer.color = originalSpriteColors[spriteRenderer];
                }
                else if (originalEmissionColors.ContainsKey(renderer))
                {
                    renderer.GetPropertyBlock(propertyBlock);
                    propertyBlock.SetColor("_EmissionColor", originalEmissionColors[renderer]);
                    propertyBlock.SetColor("_Emission", originalEmissionColors[renderer]);
                    
                    if (originalEmissionIntensities.ContainsKey(renderer))
                    {
                        propertyBlock.SetFloat("_EmissionIntensity", originalEmissionIntensities[renderer]);
                        propertyBlock.SetFloat("_EmissionPower", originalEmissionIntensities[renderer]);
                    }
                    
                    renderer.SetPropertyBlock(propertyBlock);
                }
            }
        }
        
        // After glow duration, turn off the highlighters
        TurnOffAllHighlighters();
        flashCoroutine = null;
    }
    
    /// <summary>
    /// Gets the target position - uses absolute position if set, otherwise calculates from offset
    /// When using absolute position, offsets are completely ignored
    /// </summary>
    Vector3 GetTargetPosition()
    {
        // Use absolute position if set and enabled - offsets are NOT used in this case
        if (useAbsolutePosition && bestCameraPosition != Vector3.zero)
        {
            Debug.Log($"[SidebarButtonHandler] Using absolute position: {bestCameraPosition}");
            return bestCameraPosition; // Return absolute position directly, no offset applied
        }
        
        // Otherwise calculate from highlighter position + offset (only when NOT using absolute position)
        Vector3 highlighterPos;
        
        // Check if we're using a collection (array of highlighters)
        if (collectionHighlighters != null && collectionHighlighters.Length > 0)
        {
            // Calculate center of all collection highlighters
            Vector3 center = Vector3.zero;
            int count = 0;
            
            foreach (GameObject highlighter in collectionHighlighters)
            {
                if (highlighter != null)
                {
                    center += highlighter.transform.position;
                    count++;
                }
            }
            
            if (count > 0)
            {
                highlighterPos = center / count;
            }
            else
            {
                highlighterPos = targetHighlighter != null ? targetHighlighter.transform.position : Vector3.zero;
            }
        }
        // Fallback: Check if targetHighlighter has children (multiple highlighters)
        else if (targetHighlighter != null && useCenterOfChildren && targetHighlighter.transform.childCount > 0)
        {
            // Calculate center of all children
            Vector3 center = Vector3.zero;
            int childCount = 0;
            
            foreach (Transform child in targetHighlighter.transform)
            {
                if (child.gameObject.activeInHierarchy)
                {
                    center += child.position;
                    childCount++;
                }
            }
            
            if (childCount > 0)
            {
                highlighterPos = center / childCount;
            }
            else
            {
                highlighterPos = targetHighlighter.transform.position;
            }
        }
        else if (targetHighlighter != null)
        {
            // Single highlighter - use its position
            highlighterPos = targetHighlighter.transform.position;
        }
        else
        {
            // No highlighter assigned - return zero (shouldn't happen due to validation, but safe fallback)
            highlighterPos = Vector3.zero;
        }
        
        // Only apply offset when NOT using absolute position
        return highlighterPos + cameraOffset;
    }
    
    /// <summary>
    /// Moves camera to target position using two-phase movement (up, then horizontal)
    /// </summary>
    void MoveCameraToPosition(Vector3 targetPosition)
    {
        Camera mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogWarning("[SidebarButtonHandler] Main camera not found!");
            return;
        }
        
        // Always use direct movement to have full control over intermediate height and smoothness
        // CameraMovementController has its own intermediateHeight (80) which causes issues
        StartCoroutine(MoveCameraDirectly(targetPosition));
    }
    
    /// <summary>
    /// Moves camera with explicit starting pitch for smooth X rotation interpolation from 90 to 30 degrees
    /// </summary>
    IEnumerator MoveCameraDirectlyWithStartingPitch(Vector3 targetPosition, float startingPitch)
    {
        Camera mainCamera = Camera.main;
        if (mainCamera == null) yield break;
        
        Vector3 startPos = mainCamera.transform.position;
        Vector3 targetPos = targetPosition;
        Quaternion startRotation = mainCamera.transform.rotation;
        
        // Use the provided starting pitch (captured before mode switch) for smooth interpolation
        // Normalize to -180 to 180 range
        float startPitch = startingPitch;
        if (startPitch > 180f) startPitch -= 360f;
        
        // Calculate target rotation - use absolute Y-axis rotation
        Vector3 lookTarget;
        
        if (collectionHighlighters != null && collectionHighlighters.Length > 0)
        {
            // Use center of collection highlighters
            Vector3 center = Vector3.zero;
            int count = 0;
            foreach (GameObject h in collectionHighlighters)
            {
                if (h != null)
                {
                    center += h.transform.position;
                    count++;
                }
            }
            lookTarget = count > 0 ? center / count : (targetHighlighter != null ? targetHighlighter.transform.position : Vector3.zero);
        }
        else if (targetHighlighter != null)
        {
            lookTarget = useCenterOfChildren && targetHighlighter.transform.childCount > 0
                ? GetCenterOfChildren()
                : targetHighlighter.transform.position;
        }
        else
        {
            // No highlighter assigned - use zero (shouldn't happen due to validation)
            lookTarget = Vector3.zero;
        }
        
        // Target pitch is always 30 degrees (drone mode default)
        float targetPitch = 30f;
        
        // Calculate target Y rotation
        float targetYaw;
        // When using absolute position, always use cameraYRotation (even if 0, as 0 is a valid angle)
        if (useAbsolutePosition && bestCameraPosition != Vector3.zero)
        {
            // Always use the specified absolute Y rotation
            targetYaw = cameraYRotation;
            Debug.Log($"[SidebarButtonHandler] Using absolute Y rotation: {targetYaw} degrees");
        }
        else if (cameraYRotation != 0f)
        {
            // Use absolute Y rotation if specified
            targetYaw = cameraYRotation;
            Debug.Log($"[SidebarButtonHandler] Using specified Y rotation: {targetYaw} degrees");
        }
        else
        {
            // If no Y rotation specified, calculate from direction to highlighter
            Vector3 directionToTarget = (lookTarget - targetPos).normalized;
            targetYaw = Mathf.Atan2(directionToTarget.x, directionToTarget.z) * Mathf.Rad2Deg;
            Debug.Log($"[SidebarButtonHandler] Calculated Y rotation from highlighter: {targetYaw} degrees");
        }
        
        Quaternion targetRotation = Quaternion.Euler(targetPitch, targetYaw, 0f);
        
        // Single smooth movement from current position/rotation to target
        // All changes (position, X rotation from startingPitch to 30, Y rotation) happen together smoothly
        float totalDistance = Vector3.Distance(startPos, targetPos);
        float totalDuration = totalDistance / movementSpeed;
        
        Debug.Log($"[SidebarButtonHandler] MoveCameraDirectlyWithStartingPitch: Start pos: {startPos}, Target pos: {targetPos}, Distance: {totalDistance}, Duration: {totalDuration}, Speed: {movementSpeed}");
        Debug.Log($"[SidebarButtonHandler] Start rotation: {startRotation.eulerAngles}, Target rotation: ({targetPitch}, {targetYaw}, 0)");
        
        // Ensure ALL controllers stay disabled during movement (before loop starts)
        if (cameraModeController != null)
        {
            if (cameraModeController.droneController != null)
            {
                cameraModeController.droneController.enabled = false;
            }
            if (cameraModeController.walkController != null)
            {
                cameraModeController.walkController.enabled = false;
            }
            if (cameraModeController.camera2DController != null)
            {
                cameraModeController.camera2DController.enabled = false;
            }
        }
        
        float elapsed = 0f;
        int frameCount = 0;
        
        while (elapsed < totalDuration)
        {
            // Check if GameObject is still active
            if (!gameObject.activeInHierarchy || mainCamera == null)
            {
                Debug.LogWarning($"[SidebarButtonHandler] Movement interrupted - GameObject inactive or camera null. Elapsed: {elapsed}/{totalDuration}, Frame: {frameCount}");
                break;
            }
            
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / totalDuration);
            
            // Use smooth step for very smooth ease in/out movement
            t = Mathf.SmoothStep(0f, 1f, t);
            
            // Smoothly interpolate position from current to target
            Vector3 newPos = Vector3.Lerp(startPos, targetPos, t);
            mainCamera.transform.position = newPos;
            
            // Smoothly interpolate both X rotation (pitch from startingPitch to 30 degrees) and Y rotation (yaw) together
            // Use LerpAngle for both to handle angle wraparound correctly
            float startYaw = startRotation.eulerAngles.y;
            float interpolatedPitch = Mathf.LerpAngle(startPitch, targetPitch, t);
            float interpolatedYaw = Mathf.LerpAngle(startYaw, targetYaw, t);
            mainCamera.transform.rotation = Quaternion.Euler(interpolatedPitch, interpolatedYaw, 0f);
            
            // Debug every 30 frames to track rotation progress
            if (frameCount % 30 == 0)
            {
                Debug.Log($"[SidebarButtonHandler] Movement progress: t={t:F2}, Pitch: {interpolatedPitch:F1}° (from {startPitch:F1} to {targetPitch:F1}), Yaw: {interpolatedYaw:F1}° (from {startYaw:F1} to {targetYaw:F1})");
            }
            
            // Keep ALL controllers disabled during movement (in case something re-enables them)
            if (cameraModeController != null)
            {
                if (cameraModeController.droneController != null && cameraModeController.droneController.enabled)
                {
                    cameraModeController.droneController.enabled = false;
                    Debug.LogWarning($"[SidebarButtonHandler] DroneController was re-enabled during movement! Disabled again at frame {frameCount}");
                }
                if (cameraModeController.walkController != null && cameraModeController.walkController.enabled)
                {
                    cameraModeController.walkController.enabled = false;
                    Debug.LogWarning($"[SidebarButtonHandler] WalkController was re-enabled during movement! Disabled again at frame {frameCount}");
                }
                if (cameraModeController.camera2DController != null && cameraModeController.camera2DController.enabled)
                {
                    cameraModeController.camera2DController.enabled = false;
                    Debug.LogWarning($"[SidebarButtonHandler] Camera2DController was re-enabled during movement! Disabled again at frame {frameCount}");
                }
            }
            
            frameCount++;
            yield return null;
        }
        
        Debug.Log($"[SidebarButtonHandler] Movement loop completed. Elapsed: {elapsed}, Duration: {totalDuration}, Frames: {frameCount}");
        
        // Ensure final position and rotation are exact (only if camera still exists)
        if (mainCamera != null)
        {
            mainCamera.transform.position = targetPos;
            mainCamera.transform.rotation = targetRotation;
            Debug.Log($"[SidebarButtonHandler] MoveCameraDirectlyWithStartingPitch completed. Final position: {mainCamera.transform.position}, Final rotation: {mainCamera.transform.rotation.eulerAngles}");
            Debug.Log($"[SidebarButtonHandler] Expected target rotation: ({targetPitch}, {targetYaw}, 0), Actual: {mainCamera.transform.rotation.eulerAngles}");
        }
        else
        {
            Debug.LogWarning("[SidebarButtonHandler] MoveCameraDirectlyWithStartingPitch completed but camera is null!");
        }
        
        // Wait a few frames to ensure rotation is set before re-enabling controllers
        yield return null;
        yield return null;
        yield return null;
        
        // Verify rotation is still correct before re-enabling
        if (mainCamera != null)
        {
            Vector3 currentEuler = mainCamera.transform.rotation.eulerAngles;
            float currentYaw = currentEuler.y;
            float expectedYaw = targetYaw;
            
            // Normalize angles to 0-360 range for comparison
            while (currentYaw < 0) currentYaw += 360;
            while (currentYaw >= 360) currentYaw -= 360;
            while (expectedYaw < 0) expectedYaw += 360;
            while (expectedYaw >= 360) expectedYaw -= 360;
            
            // Check if Y rotation is correct (within 1 degree tolerance)
            float yawDifference = Mathf.Abs(currentYaw - expectedYaw);
            if (yawDifference > 1f && yawDifference < 359f) // Account for wraparound
            {
                Debug.LogWarning($"[SidebarButtonHandler] Y rotation mismatch! Expected: {expectedYaw}°, Actual: {currentYaw}°. Re-setting rotation.");
                mainCamera.transform.rotation = targetRotation;
            }
        }
        
        // Re-enable controllers after ensuring rotation is correct
        if (cameraModeController != null)
        {
            // Only re-enable DroneController if we're still in Drone mode
            if (cameraModeController.droneController != null && cameraModeController.currentMode == TourMode.Drone)
            {
                cameraModeController.droneController.enabled = true; // Re-enable for joystick control
                Debug.Log($"[SidebarButtonHandler] Re-enabled DroneController after movement. Camera Y rotation: {mainCamera.transform.rotation.eulerAngles.y}°");
            }
        }
    }
    
    /// <summary>
    /// Fallback: Direct camera movement if CameraMovementController is not available
    /// Uses smooth movement with proper intermediate height
    /// </summary>
    IEnumerator MoveCameraDirectly(Vector3 targetPosition)
    {
        Camera mainCamera = Camera.main;
        if (mainCamera == null) yield break;
        
        Vector3 startPos = mainCamera.transform.position;
        Vector3 targetPos = targetPosition;
        Quaternion startRotation = mainCamera.transform.rotation;
        
        // Calculate target rotation - use absolute Y-axis rotation
        Vector3 lookTarget;
        
        if (collectionHighlighters != null && collectionHighlighters.Length > 0)
        {
            // Use center of collection highlighters
            Vector3 center = Vector3.zero;
            int count = 0;
            foreach (GameObject h in collectionHighlighters)
            {
                if (h != null)
                {
                    center += h.transform.position;
                    count++;
                }
            }
            lookTarget = count > 0 ? center / count : (targetHighlighter != null ? targetHighlighter.transform.position : Vector3.zero);
        }
        else if (targetHighlighter != null)
        {
            lookTarget = useCenterOfChildren && targetHighlighter.transform.childCount > 0
                ? GetCenterOfChildren()
                : targetHighlighter.transform.position;
        }
        else
        {
            // No highlighter assigned - use zero (shouldn't happen due to validation)
            lookTarget = Vector3.zero;
        }
        
        // Get the current X rotation (pitch) from the camera's current rotation
        // Keep it fixed throughout the movement (already at 30 degrees in drone mode)
        float startPitch = startRotation.eulerAngles.x;
        // Normalize to -180 to 180 range
        if (startPitch > 180f) startPitch -= 360f;
        
        // Keep pitch fixed at current value (should be 30 degrees in drone mode)
        float targetPitch = startPitch;
        
        // Calculate target Y rotation
        float targetYaw;
        // When using absolute position, always use cameraYRotation (even if 0, as 0 is a valid angle)
        if (useAbsolutePosition && bestCameraPosition != Vector3.zero)
        {
            // Always use the specified absolute Y rotation
            targetYaw = cameraYRotation;
            Debug.Log($"[SidebarButtonHandler] Using absolute Y rotation: {targetYaw} degrees");
        }
        else if (cameraYRotation != 0f)
        {
            // Use absolute Y rotation if specified
            targetYaw = cameraYRotation;
            Debug.Log($"[SidebarButtonHandler] Using specified Y rotation: {targetYaw} degrees");
        }
        else
        {
            // If no Y rotation specified, calculate from direction to highlighter
            Vector3 directionToTarget = (lookTarget - targetPos).normalized;
            targetYaw = Mathf.Atan2(directionToTarget.x, directionToTarget.z) * Mathf.Rad2Deg;
            Debug.Log($"[SidebarButtonHandler] Calculated Y rotation from highlighter: {targetYaw} degrees");
        }
        
        Quaternion targetRotation = Quaternion.Euler(targetPitch, targetYaw, 0f);
        
        // Single smooth movement from current position/rotation to target
        // All changes (position, X rotation, Y rotation) happen together smoothly
        float totalDistance = Vector3.Distance(startPos, targetPos);
        float totalDuration = totalDistance / movementSpeed;
        
        Debug.Log($"[SidebarButtonHandler] MoveCameraDirectly: Start pos: {startPos}, Target pos: {targetPos}, Distance: {totalDistance}, Duration: {totalDuration}, Speed: {movementSpeed}");
        Debug.Log($"[SidebarButtonHandler] Start rotation: {startRotation.eulerAngles}, Target rotation: ({targetPitch}, {targetYaw}, 0)");
        
        float elapsed = 0f;
        int frameCount = 0;
        float startYaw = startRotation.eulerAngles.y;
        
        Debug.Log($"[SidebarButtonHandler] Rotation interpolation: Start Yaw: {startYaw}°, Target Yaw: {targetYaw}°");
        
        // Ensure ALL controllers stay disabled during movement
        if (cameraModeController != null)
        {
            if (cameraModeController.droneController != null)
            {
                cameraModeController.droneController.enabled = false;
            }
            if (cameraModeController.walkController != null)
            {
                cameraModeController.walkController.enabled = false;
            }
            if (cameraModeController.camera2DController != null)
            {
                cameraModeController.camera2DController.enabled = false;
            }
        }
        
        while (elapsed < totalDuration)
        {
            // Check if GameObject is still active
            if (!gameObject.activeInHierarchy || mainCamera == null)
            {
                Debug.LogWarning($"[SidebarButtonHandler] Movement interrupted - GameObject inactive or camera null. Elapsed: {elapsed}/{totalDuration}, Frame: {frameCount}");
                break;
            }
            
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / totalDuration);
            
            // Use smooth step for very smooth ease in/out movement
            t = Mathf.SmoothStep(0f, 1f, t);
            
            // Smoothly interpolate position from current to target
            mainCamera.transform.position = Vector3.Lerp(startPos, targetPos, t);
            
            // Smoothly interpolate both X rotation (pitch) and Y rotation (yaw) together
            // X rotation stays fixed at current value (already at 30 degrees in drone mode)
            // Use LerpAngle for both to handle angle wraparound correctly
            float interpolatedPitch = Mathf.LerpAngle(startPitch, targetPitch, t);
            float interpolatedYaw = Mathf.LerpAngle(startYaw, targetYaw, t);
            mainCamera.transform.rotation = Quaternion.Euler(interpolatedPitch, interpolatedYaw, 0f);
            
            // Debug every 30 frames to track rotation progress
            if (frameCount % 30 == 0)
            {
                Debug.Log($"[SidebarButtonHandler] Movement progress: t={t:F2}, Pitch: {interpolatedPitch:F1}° (from {startPitch:F1} to {targetPitch:F1}), Yaw: {interpolatedYaw:F1}° (from {startYaw:F1} to {targetYaw:F1})");
            }
            
            frameCount++;
            
            // Keep ALL controllers disabled during movement (in case something re-enables them)
            if (cameraModeController != null)
            {
                if (cameraModeController.droneController != null)
                {
                    cameraModeController.droneController.enabled = false;
                }
                if (cameraModeController.walkController != null)
                {
                    cameraModeController.walkController.enabled = false;
                }
                if (cameraModeController.camera2DController != null)
                {
                    cameraModeController.camera2DController.enabled = false;
                }
            }
            
            yield return null;
        }
        
        // Ensure final position and rotation are exact (only if camera still exists)
        if (mainCamera != null)
        {
            mainCamera.transform.position = targetPos;
            mainCamera.transform.rotation = targetRotation;
            Debug.Log($"[SidebarButtonHandler] MoveCameraDirectly completed. Final position: {mainCamera.transform.position}, Final rotation: {mainCamera.transform.rotation.eulerAngles}");
            Debug.Log($"[SidebarButtonHandler] Expected target rotation: ({targetPitch}, {targetYaw}, 0), Actual: {mainCamera.transform.rotation.eulerAngles}");
        }
        else
        {
            Debug.LogWarning("[SidebarButtonHandler] MoveCameraDirectly completed but camera is null!");
        }
        
        // Wait a few frames to ensure rotation is set before re-enabling controllers
        yield return null;
        yield return null;
        yield return null;
        
        // Verify rotation is still correct before re-enabling
        if (mainCamera != null)
        {
            Vector3 currentEuler = mainCamera.transform.rotation.eulerAngles;
            float currentYaw = currentEuler.y;
            float expectedYaw = targetYaw;
            
            // Normalize angles to 0-360 range for comparison
            while (currentYaw < 0) currentYaw += 360;
            while (currentYaw >= 360) currentYaw -= 360;
            while (expectedYaw < 0) expectedYaw += 360;
            while (expectedYaw >= 360) expectedYaw -= 360;
            
            // Check if Y rotation is correct (within 1 degree tolerance)
            float yawDifference = Mathf.Abs(currentYaw - expectedYaw);
            if (yawDifference > 1f && yawDifference < 359f) // Account for wraparound
            {
                Debug.LogWarning($"[SidebarButtonHandler] Y rotation mismatch! Expected: {expectedYaw}°, Actual: {currentYaw}°. Re-setting rotation.");
                mainCamera.transform.rotation = targetRotation;
            }
        }
        
        // Re-enable controllers after ensuring rotation is correct
        if (cameraModeController != null)
        {
            // Only re-enable DroneController if we're still in Drone mode
            if (cameraModeController.droneController != null && cameraModeController.currentMode == TourMode.Drone)
            {
                cameraModeController.droneController.enabled = true; // Re-enable for joystick control
                Debug.Log($"[SidebarButtonHandler] Re-enabled DroneController after movement. Camera Y rotation: {mainCamera.transform.rotation.eulerAngles.y}°");
            }
        }
    }
    
    /// <summary>
    /// Helper to get center of children
    /// </summary>
    Vector3 GetCenterOfChildren()
    {
        if (targetHighlighter == null) return Vector3.zero;
        
        Vector3 center = Vector3.zero;
        int count = 0;
        foreach (Transform child in targetHighlighter.transform)
        {
            if (child != null)
            {
                center += child.position;
                count++;
            }
        }
        return count > 0 ? center / count : targetHighlighter.transform.position;
    }
    
    /// <summary>
    /// Public method to set target highlighter programmatically
    /// </summary>
    public void SetTargetHighlighter(GameObject highlighter)
    {
        targetHighlighter = highlighter;
    }
}

