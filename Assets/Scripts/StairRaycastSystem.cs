using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
#if UNITY_TMP
using TMPro;
#endif

/// <summary>
/// Handles raycast detection for stairs, floor switching, and UI management
/// Works only in interior mode
/// </summary>
public class StairRaycastSystem : MonoBehaviour
{
    [Header("References")]
    public Camera mainCamera;
    public Canvas uiCanvas;
    
    [Header("Stair Data")]
    [Tooltip("List of all stairs in the building")]
    public StairData[] stairs;
    
    [Tooltip("Parent GameObject containing all stair GameObjects (e.g., 'Stairs')")]
    public GameObject stairsParent;
    
    [Header("UI References")]
    [Tooltip("Stair UI Button Panel with Up/Down buttons")]
    public GameObject stairsButtonPanel;
    
    [Tooltip("Button for going up")]
    public Button upButton;
    
    [Tooltip("Button for going down")]
    public Button downButton;
    
    [Header("Floor Display UI")]
    [Tooltip("Text component to display current floor (e.g., 'Floor: Ground Floor')")]
    public GameObject floorDisplayUI;
    
#if UNITY_TMP
    [Tooltip("TextMeshPro component for floor display (if using TextMeshPro)")]
    public TMPro.TextMeshProUGUI floorDisplayText;
#else
    [Tooltip("Text component for floor display (if using Unity UI Text)")]
    public Text floorDisplayText;
#endif
    
    [Header("Raycast Settings")]
    [Tooltip("Maximum raycast distance")]
    public float maxRaycastDistance = 50f;
    
    [Tooltip("Layers that can block raycast")]
    public LayerMask blockingLayers = -1;
    
    [Header("UI Settings")]
    [Tooltip("Distance threshold for showing stairs UI (closer = shown)")]
    public float showUIDistance = 15f;
    
    [Header("Fade Settings")]
    [Tooltip("Reference to InteriorExteriorManager for fade transitions")]
    public InteriorExteriorManager interiorManager;
    
    [Tooltip("Speed for camera movement between floors")]
    public float floorMovementSpeed = 5f;
    
    [Header("Controller References")]
    [Tooltip("Reference to WalkController (auto-found if not assigned)")]
    public WalkController walkController;
    
    private StairData currentHoveredStair;
    private StairData currentFloorStair; // The stair we're currently on (tracks current floor)
    private InteriorExteriorManager interiorExteriorManager;
    private bool isTransitioning = false;
    
    void Start()
    {
        // Auto-find main camera if not assigned
        if (mainCamera == null)
        {
            mainCamera = GetComponent<Camera>();
            if (mainCamera == null)
            {
                mainCamera = Camera.main;
            }
        }
        
        // Find interior manager
        if (interiorManager == null)
        {
            interiorExteriorManager = FindFirstObjectByType<InteriorExteriorManager>();
            if (interiorExteriorManager != null)
            {
                interiorManager = interiorExteriorManager;
            }
        }
        else
        {
            interiorExteriorManager = interiorManager;
        }
        
        // Find WalkController if not assigned
        if (walkController == null)
        {
            walkController = FindFirstObjectByType<WalkController>();
            if (walkController == null)
            {
                // Try to find via CameraModeController
                CameraModeController cameraModeController = FindFirstObjectByType<CameraModeController>();
                if (cameraModeController != null && cameraModeController.walkController != null)
                {
                    walkController = cameraModeController.walkController;
                }
            }
        }
        
        // Find stairs parent if not assigned
        if (stairsParent == null)
        {
            GameObject stairsObj = GameObject.Find("Stairs");
            if (stairsObj != null)
            {
                stairsParent = stairsObj;
            }
        }
        
        // Find stairs button panel if not assigned
        if (stairsButtonPanel == null)
        {
            stairsButtonPanel = GameObject.Find("StairsButtonPanel");
        }
        
        // Auto-find buttons if not assigned
        if (stairsButtonPanel != null)
        {
            if (upButton == null)
            {
                // Try to find up button by name
                Transform upButtonTransform = stairsButtonPanel.transform.Find("UpButton");
                if (upButtonTransform == null)
                {
                    upButtonTransform = stairsButtonPanel.transform.Find("Up");
                }
                if (upButtonTransform != null)
                {
                    upButton = upButtonTransform.GetComponent<Button>();
                }
            }
            
            if (downButton == null)
            {
                // Try to find down button by name
                Transform downButtonTransform = stairsButtonPanel.transform.Find("DownButton");
                if (downButtonTransform == null)
                {
                    downButtonTransform = stairsButtonPanel.transform.Find("Down");
                }
                if (downButtonTransform != null)
                {
                    downButton = downButtonTransform.GetComponent<Button>();
                }
            }
        }
        
        // Hide stairs panel initially
        if (stairsButtonPanel != null)
        {
            stairsButtonPanel.SetActive(false);
        }
        
        // Setup button listeners
        if (upButton != null)
        {
            upButton.onClick.RemoveAllListeners();
            upButton.onClick.AddListener(OnUpButtonClicked);
            Debug.Log("[StairRaycastSystem] Up button found and listeners set up");
        }
        else
        {
            Debug.LogWarning("[StairRaycastSystem] Up button not found! Make sure it's assigned or exists as a child of StairsButtonPanel.");
        }
        
        if (downButton != null)
        {
            downButton.onClick.RemoveAllListeners();
            downButton.onClick.AddListener(OnDownButtonClicked);
            Debug.Log("[StairRaycastSystem] Down button found and listeners set up");
        }
        else
        {
            Debug.LogWarning("[StairRaycastSystem] Down button not found! Make sure it's assigned or exists as a child of StairsButtonPanel.");
        }
        
        // Hide floor display initially (only shown in interior mode)
        if (floorDisplayUI != null)
        {
            floorDisplayUI.SetActive(false);
        }
        
        // Auto-populate stairs from parent if not manually assigned
        if (stairs == null || stairs.Length == 0)
        {
            AutoPopulateStairs();
        }
        
        // Link stairs by name
        LinkStairsByName();
        
        // Initialize floor display
        UpdateFloorDisplay();
    }
    
    void Update()
    {
        // Only work in interior mode
        if (interiorExteriorManager == null || !interiorExteriorManager.IsInteriorMode())
        {
            HideStairsUI();
            if (floorDisplayUI != null)
            {
                floorDisplayUI.SetActive(false);
            }
            return;
        }
        
        // Show floor display in interior mode
        if (floorDisplayUI != null)
        {
            floorDisplayUI.SetActive(true);
        }
        
        if (stairs == null || stairs.Length == 0) return;
        if (stairsButtonPanel == null) return;
        if (isTransitioning) return;
        
        // Perform raycast from camera center
        Ray ray = new Ray(mainCamera.transform.position, mainCamera.transform.forward);
        RaycastHit hit;
        
        StairData detectedStair = null;
        float closestDistance = float.MaxValue;
        
        // Check each stair
        foreach (StairData stair in stairs)
        {
            if (stair == null || stair.stairObject == null) continue;
            
            Collider stairCol = stair.stairCollider;
            if (stairCol == null)
            {
                // Try to get collider from stair object
                stairCol = stair.stairObject.GetComponent<Collider>();
                if (stairCol == null)
                {
                    stairCol = stair.stairObject.GetComponentInChildren<Collider>();
                }
                stair.stairCollider = stairCol;
            }
            
            if (stairCol == null || !stairCol.enabled) continue;
            
            // Check if raycast hits this stair
            if (stairCol.Raycast(ray, out hit, maxRaycastDistance))
            {
                float distance = Vector3.Distance(mainCamera.transform.position, hit.point);
                
                // Check if anything blocks the raycast between camera and stair (excluding stair itself)
                RaycastHit[] hits = Physics.RaycastAll(ray, distance, blockingLayers);
                bool isBlocked = false;
                foreach (RaycastHit blockHit in hits)
                {
                    // Don't count the stair itself as blocking
                    if (blockHit.collider != stairCol && blockHit.collider.transform != stair.stairObject.transform)
                    {
                        isBlocked = true;
                        break;
                    }
                }
                
                if (!isBlocked)
                {
                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        detectedStair = stair;
                    }
                }
            }
        }
        
        // Update UI based on detected stair
        if (detectedStair != null && closestDistance <= showUIDistance)
        {
            if (currentHoveredStair != detectedStair)
            {
                ShowStairsUI(detectedStair);
                currentHoveredStair = detectedStair;
            }
        }
        else
        {
            HideStairsUI();
            currentHoveredStair = null;
        }
    }
    
    /// <summary>
    /// Shows the stairs UI with appropriate buttons based on stair type
    /// </summary>
    void ShowStairsUI(StairData stair)
    {
        if (stairsButtonPanel == null || stair == null)
        {
            Debug.LogWarning($"[StairRaycastSystem] Cannot show stairs UI - panel: {(stairsButtonPanel != null ? "OK" : "NULL")}, stair: {(stair != null ? "OK" : "NULL")}");
            return;
        }
        
        stairsButtonPanel.SetActive(true);
        
        // Show/hide buttons based on stair type and available connections
        bool showUp = false;
        bool showDown = false;
        
        switch (stair.stairType)
        {
            case StairType.GroundFloor:
                showUp = stair.CanGoUp();
                showDown = false;
                break;
                
            case StairType.MiddleFloor:
                showUp = stair.CanGoUp();
                showDown = stair.CanGoDown();
                break;
                
            case StairType.UpperFloor:
                showUp = false;
                showDown = stair.CanGoDown();
                break;
        }
        
        bool canGoUp = stair.CanGoUp();
        bool canGoDown = stair.CanGoDown();
        
        Debug.Log($"[StairRaycastSystem] Showing stairs UI for {stair.stairName} - Type: {stair.stairType}");
        Debug.Log($"[StairRaycastSystem] CanGoUp: {canGoUp} (upStair: {(stair.upStair != null ? stair.upStair.stairName : "NULL")}, upPosition: {stair.upPosition})");
        Debug.Log($"[StairRaycastSystem] CanGoDown: {canGoDown} (downStair: {(stair.downStair != null ? stair.downStair.stairName : "NULL")}, downPosition: {stair.downPosition})");
        
        if (upButton != null)
        {
            upButton.gameObject.SetActive(showUp);
            Debug.Log($"[StairRaycastSystem] Up button {(showUp ? "shown" : "hidden")}");
        }
        else
        {
            Debug.LogWarning("[StairRaycastSystem] Up button is null! Cannot show/hide it.");
        }
        
        if (downButton != null)
        {
            downButton.gameObject.SetActive(showDown);
            Debug.Log($"[StairRaycastSystem] Down button {(showDown ? "shown" : "hidden")}");
        }
        else
        {
            Debug.LogWarning("[StairRaycastSystem] Down button is null! Cannot show/hide it.");
        }
    }
    
    /// <summary>
    /// Hides the stairs UI
    /// </summary>
    void HideStairsUI()
    {
        if (stairsButtonPanel != null)
        {
            stairsButtonPanel.SetActive(false);
        }
    }
    
    /// <summary>
    /// Called when up button is clicked
    /// </summary>
    void OnUpButtonClicked()
    {
        if (currentHoveredStair == null || isTransitioning) return;
        
        if (currentHoveredStair.upStair != null && currentHoveredStair.CanGoUp())
        {
            StartCoroutine(SwitchFloorCoroutine(currentHoveredStair, currentHoveredStair.upStair, true));
        }
    }
    
    /// <summary>
    /// Called when down button is clicked
    /// </summary>
    void OnDownButtonClicked()
    {
        if (currentHoveredStair == null || isTransitioning) return;
        
        if (currentHoveredStair.downStair != null && currentHoveredStair.CanGoDown())
        {
            StartCoroutine(SwitchFloorCoroutine(currentHoveredStair, currentHoveredStair.downStair, false));
        }
    }
    
    /// <summary>
    /// Coroutine for switching floors with fade effect
    /// </summary>
    IEnumerator SwitchFloorCoroutine(StairData fromStair, StairData toStair, bool goingUp)
    {
        if (fromStair == null || toStair == null || mainCamera == null) yield break;
        
        isTransitioning = true;
        HideStairsUI();
        
        // Determine target position and rotation using CURRENT stair's position data (much simpler!)
        Vector3 targetPosition;
        Vector3 targetRotation;
        
        if (goingUp)
        {
            // Use the current stair's Up Position (where it takes you when going up)
            targetPosition = fromStair.upPosition;
            targetRotation = fromStair.upRotation;
            Debug.Log($"[StairRaycastSystem] Going UP from {fromStair.stairName}. Using current stair's Up Position: {targetPosition}");
        }
        else
        {
            // Use the current stair's Down Position (where it takes you when going down)
            targetPosition = fromStair.downPosition;
            targetRotation = fromStair.downRotation;
            Debug.Log($"[StairRaycastSystem] Going DOWN from {fromStair.stairName}. Using current stair's Down Position: {targetPosition}");
        }
        
        Debug.Log($"[StairRaycastSystem] Target position Y: {targetPosition.y}, Current camera Y: {mainCamera.transform.position.y}");
        
        if (targetPosition == Vector3.zero)
        {
            Debug.LogWarning($"[StairRaycastSystem] Target position is zero for stair: {fromStair.stairName}. Make sure you've set the {(goingUp ? "Up Position" : "Down Position")} for this stair in the inspector.");
            isTransitioning = false;
            yield break;
        }
        
        // Check if Y position is reasonable (not still at ground level)
        if (targetPosition.y <= -0.5f)
        {
            Debug.LogWarning($"[StairRaycastSystem] Target position Y is {targetPosition.y}, which seems to be at ground level. Did you set the Y coordinate correctly in the stair positions? (Ground: -1, 1st: 1.5, 2nd: 4.5, 3rd: 7.5)");
        }
        
        if (targetRotation == Vector3.zero)
        {
            // Use current camera rotation if rotation not set
            targetRotation = mainCamera.transform.rotation.eulerAngles;
        }
        
        // IMPORTANT: Check if target position Y is actually different from current position
        // If it's the same or at ground level, the positions probably aren't configured correctly
        float currentY = mainCamera.transform.position.y;
        if (Mathf.Abs(targetPosition.y - currentY) < 0.5f)
        {
            Debug.LogError($"[StairRaycastSystem] Target Y ({targetPosition.y}) is too close to current Y ({currentY})! " +
                          $"Make sure you've set the Y coordinate in the stair positions in the inspector. " +
                          $"For the stair '{fromStair.stairName}', check the {(goingUp ? "Up Position" : "Down Position")} field and set Y to: " +
                          $"{(goingUp ? "1.5 (1st floor), 4.5 (2nd floor), or 7.5 (3rd floor)" : "-1 (ground), 1.5 (1st floor), or 4.5 (2nd floor)")}");
        }
        
        // Set WalkController height override to the target floor height BEFORE moving camera
        if (walkController != null)
        {
            walkController.SetCameraHeightOverride(targetPosition.y);
            Debug.Log($"[StairRaycastSystem] Set camera height override to {targetPosition.y} for floor transition (target position: {targetPosition}, current Y: {currentY})");
        }
        else
        {
            Debug.LogError("[StairRaycastSystem] WalkController is null! Cannot set height override. Make sure WalkController is assigned in the inspector.");
        }
        
        // Fade out (use InteriorExteriorManager's fade if available)
        if (interiorExteriorManager != null)
        {
            yield return interiorExteriorManager.PerformFadeOut();
        }
        
        // Move camera to new floor position
        yield return StartCoroutine(MoveCameraToFloorPosition(targetPosition, Quaternion.Euler(targetRotation)));
        
        // Update current floor
        currentFloorStair = toStair;
        UpdateFloorDisplay();
        
        // Notify InteriorExteriorManager to update interior panel
        if (interiorExteriorManager != null)
        {
            interiorExteriorManager.UpdateInteriorPanel();
        }
        
        // Fade in
        if (interiorExteriorManager != null)
        {
            yield return interiorExteriorManager.PerformFadeIn();
        }
        
        // Note: We keep the height override active so the camera stays at the new floor height
        // It will be cleared when exiting interior mode or can be cleared manually if needed
        
        isTransitioning = false;
        Debug.Log($"[StairRaycastSystem] Switched to floor: {toStair.floorName} at height {targetPosition.y}");
    }
    
    /// <summary>
    /// Moves camera smoothly to the new floor position
    /// </summary>
    IEnumerator MoveCameraToFloorPosition(Vector3 targetPosition, Quaternion targetRotation)
    {
        if (mainCamera == null) yield break;
        
        // Disable WalkController during movement to prevent it from overriding the height
        bool wasWalkControllerEnabled = false;
        if (walkController != null)
        {
            wasWalkControllerEnabled = walkController.enabled;
            walkController.enabled = false;
            Debug.Log($"[StairRaycastSystem] Disabled WalkController during floor transition");
        }
        
        Vector3 startPosition = mainCamera.transform.position;
        Quaternion startRotation = mainCamera.transform.rotation;
        
        float distance = Vector3.Distance(startPosition, targetPosition);
        float duration = distance / floorMovementSpeed;
        float elapsed = 0f;
        
        Debug.Log($"[StairRaycastSystem] Moving camera from {startPosition} to {targetPosition}, height override active: {(walkController != null ? walkController.GetCurrentCameraHeight().ToString() : "N/A")}");
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            t = Mathf.SmoothStep(0f, 1f, t); // Smooth ease in/out
            
            // Lerp position but maintain target Y to respect height override
            Vector3 lerpedPosition = Vector3.Lerp(startPosition, targetPosition, t);
            mainCamera.transform.position = lerpedPosition;
            mainCamera.transform.rotation = Quaternion.Slerp(startRotation, targetRotation, t);
            
            yield return null;
        }
        
        // Ensure exact final position and rotation
        mainCamera.transform.position = targetPosition;
        mainCamera.transform.rotation = targetRotation;
        
        Debug.Log($"[StairRaycastSystem] Camera moved to {targetPosition}, height override: {(walkController != null ? walkController.GetCurrentCameraHeight().ToString() : "N/A")}");
        
        // Re-enable WalkController after movement (it will now use the height override)
        if (walkController != null && wasWalkControllerEnabled)
        {
            walkController.enabled = true;
            Debug.Log($"[StairRaycastSystem] Re-enabled WalkController after floor transition, height override: {walkController.GetCurrentCameraHeight()}");
        }
    }
    
    
    /// <summary>
    /// Updates the floor display UI with current floor name
    /// </summary>
    void UpdateFloorDisplay()
    {
        if (currentFloorStair == null) return;
        
        string floorText = $"Floor: {currentFloorStair.floorName}";
        
#if UNITY_TMP
        if (floorDisplayText != null)
        {
            floorDisplayText.text = floorText;
        }
#else
        if (floorDisplayText != null)
        {
            floorDisplayText.text = floorText;
        }
#endif
        
        Debug.Log($"[StairRaycastSystem] Floor display updated: {floorText}");
    }
    
    /// <summary>
    /// Sets the current floor (called when entering interior through a door)
    /// </summary>
    public void SetCurrentFloor(StairData stair)
    {
        currentFloorStair = stair;
        UpdateFloorDisplay();
    }
    
    /// <summary>
    /// Gets the current floor stair
    /// </summary>
    public StairData GetCurrentFloor()
    {
        return currentFloorStair;
    }
    
    /// <summary>
    /// Auto-populates stairs from stairs parent GameObject
    /// </summary>
    void AutoPopulateStairs()
    {
        if (stairsParent == null) return;
        
        List<StairData> foundStairs = new List<StairData>();
        
        foreach (Transform child in stairsParent.transform)
        {
            Collider col = child.GetComponent<Collider>();
            if (col == null)
            {
                col = child.GetComponentInChildren<Collider>();
            }
            
            if (col != null)
            {
                StairData stair = new StairData();
                stair.stairName = child.name;
                stair.stairObject = child.gameObject;
                stair.stairCollider = col;
                stair.floorName = child.name; // Default floor name to object name
                foundStairs.Add(stair);
                
                Debug.Log($"[StairRaycastSystem] Found stair: {child.name}");
            }
        }
        
        stairs = foundStairs.ToArray();
        Debug.Log($"[StairRaycastSystem] Auto-populated {stairs.Length} stairs from scene");
    }
    
    /// <summary>
    /// Links stairs together by matching names (upStairName/downStairName)
    /// This allows easy configuration in the inspector
    /// </summary>
    void LinkStairsByName()
    {
        if (stairs == null || stairs.Length == 0) return;
        
        // Build a dictionary of stairs by name for quick lookup
        Dictionary<string, StairData> stairsByName = new Dictionary<string, StairData>();
        foreach (StairData stair in stairs)
        {
            if (stair != null && !string.IsNullOrEmpty(stair.stairName))
            {
                stairsByName[stair.stairName] = stair;
            }
        }
        
        // Link stairs based on names
        int linkedCount = 0;
        foreach (StairData stair in stairs)
        {
            if (stair == null) continue;
            
            // Link up stair
            if (!string.IsNullOrEmpty(stair.upStairName))
            {
                if (stairsByName.TryGetValue(stair.upStairName, out StairData upStair))
                {
                    stair.upStair = upStair;
                    linkedCount++;
                    Debug.Log($"[StairRaycastSystem] Linked {stair.stairName}.upStair -> {upStair.stairName}");
                }
                else
                {
                    Debug.LogWarning($"[StairRaycastSystem] Could not find upStair named '{stair.upStairName}' for stair '{stair.stairName}'");
                }
            }
            
            // Link down stair
            if (!string.IsNullOrEmpty(stair.downStairName))
            {
                if (stairsByName.TryGetValue(stair.downStairName, out StairData downStair))
                {
                    stair.downStair = downStair;
                    linkedCount++;
                    Debug.Log($"[StairRaycastSystem] Linked {stair.stairName}.downStair -> {downStair.stairName}");
                }
                else
                {
                    Debug.LogWarning($"[StairRaycastSystem] Could not find downStair named '{stair.downStairName}' for stair '{stair.stairName}'");
                }
            }
        }
        
        Debug.Log($"[StairRaycastSystem] Linked {linkedCount} stair connections by name");
    }
}
