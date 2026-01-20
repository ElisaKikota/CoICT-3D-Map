using UnityEngine;
using UnityEngine.UI;

public enum TourMode { Mode2D, Drone, Walk, Mode3D }

public class CameraModeController : MonoBehaviour
{
    [Header("References")]
    public Camera mainCamera;                 // Assign MainCamera in Inspector
    public Transform topDownPivot;            // Assign pivot for 2D view
    public Transform dronePivot;              // Assign pivot for Drone view
    public Transform walkPivot;               // Assign pivot for Walk view

    [Header("UI")]
    public GameObject joystickContainer;      // Canvas/JoystickContainer
    public JoystickManager joystickManager;   // Assign JoystickManager
    public ZoomButtonManager zoomButtonManager; // Assign ZoomButtonManager
    public NorthIndicatorManager northIndicatorManager; // Assign NorthIndicatorManager
    public HighlighterLabelManager highlighterLabelManager; // Assign HighlighterLabelManager (for label display)
    
    [Header("Sidebar")]
    public GameObject sidebar;                // Sidebar GameObject (renamed from AutocompleteDropdown)
    public RectTransform sidebarRect;         // Sidebar RectTransform (optional, auto-detected if not assigned)
    public Button buildingIconButton;         // Building icon button in top bar
    
    [Header("Eye Button Settings")]
    private bool joysticksVisible = true;     // Track joystick visibility state

    [Header("Controllers")]
    public Camera2DController camera2DController; // MainCamera Camera2DController (optional)
    public DroneController droneController;       // Assign DroneController
    public WalkController walkController;         // Assign WalkController
    public Camera3DController camera3DController; // Assign Camera3DController (for 3D mode)

    [Header("Buttons (assign Button components)")]
    public Button btn2D;       // assign Btn2D Button component
    public Button btnDrone;    // assign BtnDrone Button component
    public Button btnWalk;     // assign BtnWalk Button component
    public Button btn3D;       // assign Btn3D Button component
    public Button eyeButton;   // assign EyeButton Button component
    public Button labelButton; // assign LabelButton Button component (for toggling highlighter labels)
    
    [Header("Button Visual States")]
    public Color activeButtonColor = new Color(0.5f, 0.5f, 0.5f, 1f);  // Dimmed color for active button
    public Color normalButtonColor = new Color(1f, 1f, 1f, 1f);         // Normal color for inactive buttons

    [Header("Settings")]
    public float transitionSpeed = 5f;
    
    [Header("Sidebar Animation")]
    public float sidebarSlideDuration = 0.3f;
    public AnimationCurve sidebarSlideCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    [Header("Fog Settings")]
    public bool enableFog = true;
    public float fogStartDistance = 50f;
    public float fogEndDistance = 100f;
    public Color fogColor = new Color(0.5f, 0.5f, 0.5f, 1f);
    public FogMode fogMode = FogMode.Linear;

    [HideInInspector]
    public TourMode currentMode;
    
    // First-time initialization flags
    private bool firstTime2D = true;
    private bool firstTime3D = true;
    private bool isFirstTime2DInitializing = false; // Flag to prevent movement during first-time setup
    
    // Sidebar state
    private bool isSidebarOpen = false;
    private Vector2 sidebarHiddenPosition;
    private Vector2 sidebarVisiblePosition;
    private Coroutine sidebarSlideCoroutine;

    void Awake()
    {
        // Basic validation
        if (mainCamera == null) Debug.LogWarning("[CameraModeController] mainCamera not assigned.");
        if (topDownPivot == null) Debug.LogWarning("[CameraModeController] topDownPivot not assigned.");
        if (dronePivot == null) Debug.LogWarning("[CameraModeController] dronePivot not assigned.");
        if (walkPivot == null) Debug.LogWarning("[CameraModeController] walkPivot not assigned.");
        if (joystickContainer == null) Debug.LogWarning("[CameraModeController] joystickContainer not assigned.");
        if (joystickManager == null) Debug.LogWarning("[CameraModeController] joystickManager not assigned.");
        if (droneController == null) Debug.LogWarning("[CameraModeController] droneController not assigned.");
        if (walkController == null) Debug.LogWarning("[CameraModeController] walkController not assigned.");
    }

    void Start()
    {
        // Auto-wire buttons (safe: will not duplicate listeners if run multiple times)
        if (btn2D != null)
        {
            btn2D.onClick.RemoveAllListeners();
            btn2D.onClick.AddListener(() => On2DButton());
        }
        if (btnDrone != null)
        {
            btnDrone.onClick.RemoveAllListeners();
            btnDrone.onClick.AddListener(() => OnDroneButton());
        }
        if (btnWalk != null)
        {
            btnWalk.onClick.RemoveAllListeners();
            btnWalk.onClick.AddListener(() => OnWalkButton());
        }
        if (eyeButton != null)
        {
            eyeButton.onClick.RemoveAllListeners();
            eyeButton.onClick.AddListener(() => OnEyeButton());
        }
        if (btn3D != null)
        {
            btn3D.onClick.RemoveAllListeners();
            btn3D.onClick.AddListener(() => On3DButton());
        }
        if (labelButton != null)
        {
            labelButton.onClick.RemoveAllListeners();
            labelButton.onClick.AddListener(() => OnLabelButton());
        }
        
        // Setup building icon button for sidebar toggle
        if (buildingIconButton != null)
        {
            buildingIconButton.onClick.RemoveAllListeners();
            buildingIconButton.onClick.AddListener(() => OnBuildingIconButton());
        }

        // Ensure controllers start disabled
        if (camera2DController != null) camera2DController.enabled = false;
        if (droneController != null) droneController.enabled = false;
        if (walkController != null) walkController.enabled = false;
        if (camera3DController != null) camera3DController.enabled = false;

        // Ensure joystick container starts hidden if set
        if (joystickContainer != null) joystickContainer.SetActive(false);

        // Initialize fog settings
        SetupFog();
        
        // Initialize sidebar positions
        InitializeSidebar();

        // Force initial mode to 2D (will set initial position in SwitchMode)
        SwitchMode(TourMode.Mode2D);

        Debug.Log("[CameraModeController] Initialized. CurrentMode = " + currentMode);
    }

    void Update()
    {
        if (mainCamera == null) return;

        // Don't interfere with camera movement in any mode
        // Let the Camera2DController handle 2D mode movement
        // Let DroneController and WalkController handle their respective modes
    }

    public void SwitchMode(TourMode mode)
    {
        // If an earlier error prevented Start from completing, make sure we don't crash
        currentMode = mode;
        Debug.Log("[CameraModeController] Switching to " + mode);

        // Enable/disable controllers based on mode
        if (camera2DController != null)
        {
            // Set camera position and use perspective mode when entering 2D mode
            if (mode == TourMode.Mode2D && mainCamera != null)
            {
                // First time entering 2D mode - set specific position at height 350 (don't move, just set it)
                if (firstTime2D || isFirstTime2DInitializing)
                {
                    Vector3 initial2DPosition = new Vector3(8.36f, 350f, -61.98f);
                    // Stop any coroutines that might be moving the camera
                    StopAllCoroutines();
                    // Set flag to prevent any movement during initialization
                    isFirstTime2DInitializing = true;
                    // Set position directly - no smooth movement on first time
                    mainCamera.transform.position = initial2DPosition;
                    mainCamera.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
                    // Ensure camera is in perspective mode
                    mainCamera.orthographic = false;
                    // Mark as no longer first time
                    firstTime2D = false;
                    // Enable controller AFTER setting position to prevent any interference
                    camera2DController.enabled = true;
                    Debug.Log($"[CameraModeController] First time 2D mode - Set initial position at height 350: {initial2DPosition}. Controller enabled after position set.");
                    // Clear the initialization flag after a frame to allow normal operation
                    StartCoroutine(ClearFirstTime2DFlag());
                }
                else
                {
                    // Enable controller first for normal behavior
                    camera2DController.enabled = true;
                    // Ensure camera is in perspective mode (same as drone/walk modes)
                    mainCamera.orthographic = false;
                    
                    Vector3 currentPosition = mainCamera.transform.position;
                    Vector3 currentRotation = mainCamera.transform.eulerAngles;
                    
                    // Only move to 180 if we're not at the first-time position (350)
                    // This prevents movement if camera is already at the correct starting position
                    if (Mathf.Abs(currentPosition.y - 350f) > 0.1f)
                    {
                        // Normal behavior - smooth movement to height 180
                        StartCoroutine(SmoothMoveTo(new Vector3(currentPosition.x, 180f, currentPosition.z)));
                        // Rotate to 90 degrees X (looking down), 0 degrees Y (reset rotation), 0 degrees Z
                        StartCoroutine(SmoothRotateTo(90f, 0f, 0f));
                        Debug.Log($"[CameraModeController] Smoothly moving camera to height 180 and rotating to (90, 0, 0) for 2D mode. Current Y rotation: {currentRotation.y}, Target Y rotation: 0");
                    }
                    else
                    {
                        Debug.Log($"[CameraModeController] Camera already at first-time position (350), skipping movement to 180");
                    }
                }
            }
            else
            {
                // Not 2D mode - disable controller
                camera2DController.enabled = false;
            }
        }
        
        // CRITICAL: Disable WalkController in 2D mode to prevent rotation conflicts
        if (walkController != null)
        {
            if (mode == TourMode.Mode2D)
            {
                walkController.enabled = false;
                Debug.Log($"[CameraModeController] FORCING WalkController to DISABLED in 2D mode");
            }
            else if (mode == TourMode.Walk)
            {
                walkController.enabled = true;
                Debug.Log($"[CameraModeController] WalkController ENABLED for Walk mode");
            }
            else
            {
                walkController.enabled = false;
                Debug.Log($"[CameraModeController] WalkController DISABLED for Drone mode");
            }
        }
        
        if (droneController != null)
        {
            // Check if DroneController was disabled BEFORE enabling it (means sidebar is handling movement)
            bool wasDisabled = !droneController.enabled;
            
            droneController.enabled = (mode == TourMode.Drone);
            Debug.Log($"[CameraModeController] DroneController enabled: {droneController.enabled} (was disabled: {wasDisabled})");
            
            // Set camera to 30-degree X rotation and height 50 when entering drone mode
            // BUT: Skip this if DroneController was disabled before we enabled it (means sidebar button is handling movement)
            if (mode == TourMode.Drone && mainCamera != null && !wasDisabled)
            {
                Vector3 currentPosition = mainCamera.transform.position;
                Vector3 currentRotation = mainCamera.transform.eulerAngles;
                StartCoroutine(SmoothMoveTo(new Vector3(currentPosition.x, 50f, currentPosition.z)));
                StartCoroutine(SmoothRotateTo(30f, currentRotation.y, 0f));
                Debug.Log($"[CameraModeController] Smoothly moving camera to height 50 and rotating to 30 degrees for drone mode");
            }
            else if (mode == TourMode.Drone && wasDisabled)
            {
                Debug.Log($"[CameraModeController] Skipping automatic camera movement - DroneController was disabled before switch (sidebar movement in progress)");
            }
        }
        
        // Walk mode logic - only set height and rotation when entering walk mode
        if (walkController != null && mode == TourMode.Walk && mainCamera != null)
        {
            Vector3 currentPosition = mainCamera.transform.position;
            Vector3 currentRotation = mainCamera.transform.eulerAngles;
            StartCoroutine(SmoothMoveTo(new Vector3(currentPosition.x, -1f, currentPosition.z)));
            StartCoroutine(SmoothRotateTo(0f, currentRotation.y, 0f));
            Debug.Log($"[CameraModeController] Smoothly moving camera to height -1 and rotating to 0 degrees for walk mode");
        }
        
        // 3D mode logic - set camera rotation and enable controller
        if (camera3DController != null)
        {
            camera3DController.enabled = (mode == TourMode.Mode3D);
            Debug.Log($"[CameraModeController] Camera3DController enabled: {camera3DController.enabled}");
            
            if (mode == TourMode.Mode3D && mainCamera != null)
            {
                Vector3 currentPosition = mainCamera.transform.position;
                
                // First time entering 3D mode - smoothly move to specific position
                if (firstTime3D)
                {
                    Vector3 initial3DPosition = new Vector3(112f, 50f, -135f);
                    StartCoroutine(SmoothMoveTo(initial3DPosition));
                    StartCoroutine(SmoothRotateTo(30f, 225f, 0f));
                    firstTime3D = false;
                    Debug.Log($"[CameraModeController] First time 3D mode - Smoothly moving to initial position: {initial3DPosition}");
                }
                else
                {
                    // Normal behavior - set Y to 50, keep X/Z, and rotate to x=30, y=225, z=0 for 3D mode
                    StartCoroutine(SmoothMoveTo(new Vector3(currentPosition.x, 50f, currentPosition.z)));
                    StartCoroutine(SmoothRotateTo(30f, 225f, 0f));
                    Debug.Log($"[CameraModeController] Smoothly moving camera to height 50 and rotating to (30, 225, 0) for 3D mode");
                }
            }
        }
        else if (mode == TourMode.Mode3D)
        {
            Debug.LogWarning("[CameraModeController] Camera3DController not assigned but Mode3D selected!");
        }

        // Reset joystick visibility state when switching to 2D mode
        if (mode == TourMode.Mode2D)
        {
            joysticksVisible = true; // Reset to visible for when switching back to drone/walk
        }
        
        // Joysticks visible only in Drone/Walk (NOT in 3D mode), and only if joysticksVisible is true
        if (joystickContainer != null)
        {
            bool showJoysticks = (mode == TourMode.Drone || mode == TourMode.Walk) && joysticksVisible;
            joystickContainer.SetActive(showJoysticks);
            Debug.Log($"[CameraModeController] Joystick container active: {showJoysticks} (mode: {mode}, joysticksVisible: {joysticksVisible})");
        }
        else
        {
            Debug.LogWarning("[CameraModeController] Joystick container is null!");
        }

        // Zoom buttons visible only in 2D mode
        if (zoomButtonManager != null)
        {
            bool showZoomButtons = (mode == TourMode.Mode2D);
            zoomButtonManager.SetVisible(showZoomButtons);
            Debug.Log($"[CameraModeController] Zoom buttons active: {showZoomButtons}");
        }
        else
        {
            Debug.LogWarning("[CameraModeController] ZoomButtonManager is null!");
        }

        // North indicator visible only in 2D mode
        if (northIndicatorManager != null)
        {
            bool showNorthIndicator = (mode == TourMode.Mode2D);
            northIndicatorManager.SetVisible(showNorthIndicator);
            Debug.Log($"[CameraModeController] North indicator active: {showNorthIndicator}");
        }
        else
        {
            Debug.LogWarning("[CameraModeController] NorthIndicatorManager is null!");
        }

        // Notify JoystickManager of mode change
        if (joystickManager != null)
        {
            UIControlMode uiMode = UIControlMode.Mode2D;
            if (mode == TourMode.Drone) uiMode = UIControlMode.ModeDrone;
            else if (mode == TourMode.Walk) uiMode = UIControlMode.ModeWalk;
            
            joystickManager.SetMode(uiMode);
            Debug.Log($"[CameraModeController] Set JoystickManager mode to: {uiMode}");
        }
        else
        {
            Debug.LogWarning("[CameraModeController] JoystickManager is null!");
        }

        // Eye button visible only in Drone/Walk (NOT in 3D mode)
        if (eyeButton != null)
            eyeButton.gameObject.SetActive(mode == TourMode.Drone || mode == TourMode.Walk);
        
        // Label button visible only in 2D and 3D modes (NOT in Drone/Walk)
        if (labelButton != null)
            labelButton.gameObject.SetActive(mode == TourMode.Mode2D || mode == TourMode.Mode3D);
        
        // Labels visible only in 2D and 3D modes (NOT in Drone/Walk)
        if (highlighterLabelManager != null)
        {
            bool labelsShouldBeVisible = (mode == TourMode.Mode2D || mode == TourMode.Mode3D);
            highlighterLabelManager.SetLabelsVisible(labelsShouldBeVisible);
            // Update label rotations when mode changes (only if labels are visible)
            if (labelsShouldBeVisible)
            {
                highlighterLabelManager.UpdateLabelRotations(mode);
            }
            Debug.Log($"[CameraModeController] Highlighter labels visible: {labelsShouldBeVisible} (mode: {mode})");
        }
            
        // Update button visual states
        UpdateButtonVisuals(mode);
        
        // Update fog based on mode
        UpdateFogForMode(mode);
    }

    // Update button visual states based on current mode
    private void UpdateButtonVisuals(TourMode mode)
    {
        // Reset all buttons to normal color first
        if (btn2D != null)
        {
            var colors = btn2D.colors;
            colors.normalColor = normalButtonColor;
            btn2D.colors = colors;
        }
        
        if (btnDrone != null)
        {
            var colors = btnDrone.colors;
            colors.normalColor = normalButtonColor;
            btnDrone.colors = colors;
        }
        
        if (btnWalk != null)
        {
            var colors = btnWalk.colors;
            colors.normalColor = normalButtonColor;
            btnWalk.colors = colors;
        }
        
        if (btn3D != null)
        {
            var colors = btn3D.colors;
            colors.normalColor = normalButtonColor;
            btn3D.colors = colors;
        }
        
        // Dim the active button
        switch (mode)
        {
            case TourMode.Mode2D:
                if (btn2D != null)
                {
                    var colors = btn2D.colors;
                    colors.normalColor = activeButtonColor;
                    btn2D.colors = colors;
                }
                break;
                
            case TourMode.Drone:
                if (btnDrone != null)
                {
                    var colors = btnDrone.colors;
                    colors.normalColor = activeButtonColor;
                    btnDrone.colors = colors;
                }
                break;
                
            case TourMode.Walk:
                if (btnWalk != null)
                {
                    var colors = btnWalk.colors;
                    colors.normalColor = activeButtonColor;
                    btnWalk.colors = colors;
                }
                break;
                
            case TourMode.Mode3D:
                if (btn3D != null)
                {
                    var colors = btn3D.colors;
                    colors.normalColor = activeButtonColor;
                    btn3D.colors = colors;
                }
                break;
        }
    }

    // Public wrappers (safe for inspector OnClick too)
    public void On2DButton() => SwitchMode(TourMode.Mode2D);
    public void OnDroneButton() => SwitchMode(TourMode.Drone);
    public void OnWalkButton() => SwitchMode(TourMode.Walk);
    public void On3DButton() => SwitchMode(TourMode.Mode3D);
    
    // Building icon button functionality - toggles sidebar
    public void OnBuildingIconButton()
    {
        ToggleSidebar();
    }
    
    // Eye button functionality - toggles joystick visibility
    public void OnEyeButton()
    {
        Debug.Log("[CameraModeController] Eye button clicked - toggling joystick visibility");
        
        // Toggle joystick visibility
        joysticksVisible = !joysticksVisible;
        
        // Only affect joysticks if we're in Drone or Walk mode (where joysticks should be visible)
        if (currentMode == TourMode.Drone || currentMode == TourMode.Walk)
        {
            if (joystickContainer != null)
            {
                joystickContainer.SetActive(joysticksVisible);
                Debug.Log($"[CameraModeController] Joysticks now {(joysticksVisible ? "visible" : "hidden")}");
            }
        }
    }
    
    // Label button functionality - toggles highlighter label visibility
    public void OnLabelButton()
    {
        Debug.Log("[CameraModeController] Label button clicked - toggling highlighter label visibility");
        
        if (highlighterLabelManager != null)
        {
            highlighterLabelManager.ToggleLabels();
        }
        else
        {
            Debug.LogWarning("[CameraModeController] HighlighterLabelManager not assigned!");
        }
    }

    // Clear first-time 2D flag after initialization
    private System.Collections.IEnumerator ClearFirstTime2DFlag()
    {
        // Wait a few frames to ensure initialization is complete
        yield return new WaitForSeconds(0.1f);
        isFirstTime2DInitializing = false;
        Debug.Log("[CameraModeController] First-time 2D initialization complete, flag cleared");
    }
    
    // Smooth transition coroutines
    private System.Collections.IEnumerator SmoothRotateTo(float x, float y, float z)
    {
        Quaternion startRotation = mainCamera.transform.rotation;
        Quaternion targetRotation = Quaternion.Euler(x, y, z);
        float elapsed = 0f;
        
        while (elapsed < 1f)
        {
            elapsed += Time.deltaTime * transitionSpeed;
            mainCamera.transform.rotation = Quaternion.Lerp(startRotation, targetRotation, elapsed);
            yield return null;
        }
        
        mainCamera.transform.rotation = targetRotation;
    }

    private System.Collections.IEnumerator SmoothMoveTo(Vector3 targetPosition)
    {
        Vector3 startPosition = mainCamera.transform.position;
        float elapsed = 0f;
        
        while (elapsed < 1f)
        {
            elapsed += Time.deltaTime * transitionSpeed;
            mainCamera.transform.position = Vector3.Lerp(startPosition, targetPosition, elapsed);
            yield return null;
        }
        
        mainCamera.transform.position = targetPosition;
    }

    // Setup fog settings
    private void SetupFog()
    {
        if (!enableFog) return;
        
        RenderSettings.fog = true;
        RenderSettings.fogMode = fogMode;
        RenderSettings.fogColor = fogColor;
        RenderSettings.fogStartDistance = fogStartDistance;
        RenderSettings.fogEndDistance = fogEndDistance;
        ForceFogKeywords();
        
        Debug.Log($"[CameraModeController] Fog enabled - Start: {fogStartDistance}, End: {fogEndDistance}, Color: {fogColor}");
    }

    // Update fog settings based on camera mode
    private void UpdateFogForMode(TourMode mode)
    {
        switch (mode)
        {
            case TourMode.Mode2D:
                // 2D mode: Disable fog completely
                RenderSettings.fog = false;
                Debug.Log("[CameraModeController] 2D mode - Fog disabled");
                break;
                
            case TourMode.Drone:
                // Drone mode: Enable fog with medium visibility
                if (enableFog)
                {
                    RenderSettings.fog = true;
                    RenderSettings.fogStartDistance = fogStartDistance;
                    RenderSettings.fogEndDistance = fogEndDistance;
                    ForceFogKeywords();
                    Debug.Log("[CameraModeController] Drone mode fog - Standard visibility");
                }
                else
                {
                    RenderSettings.fog = false;
                }
                break;
                
            case TourMode.Walk:
                // Walk mode: Enable fog with shorter visibility for ground-level view
                if (enableFog)
                {
                    RenderSettings.fog = true;
                    RenderSettings.fogStartDistance = fogStartDistance * 0.5f;
                    RenderSettings.fogEndDistance = fogEndDistance * 0.5f;
                    ForceFogKeywords();
                    Debug.Log("[CameraModeController] Walk mode fog - Reduced visibility");
                }
                else
                {
                    RenderSettings.fog = false;
                }
                break;
                
            case TourMode.Mode3D:
                // 3D mode: Enable fog with medium visibility (similar to Drone mode)
                if (enableFog)
                {
                    RenderSettings.fog = true;
                    RenderSettings.fogStartDistance = fogStartDistance;
                    RenderSettings.fogEndDistance = fogEndDistance;
                    ForceFogKeywords();
                    Debug.Log("[CameraModeController] 3D mode fog - Standard visibility");
                }
                else
                {
                    RenderSettings.fog = false;
                }
                break;
        }
    }

    // Some Android builds can strip fog variants or disable keywords. Nudge shader keywords at runtime.
    private void ForceFogKeywords()
    {
        // Built-in pipeline uses these keywords. Harmless if not present in URP.
        if (fogMode == FogMode.Linear)
        {
            Shader.EnableKeyword("FOG_LINEAR");
            Shader.DisableKeyword("FOG_EXP");
            Shader.DisableKeyword("FOG_EXP2");
        }
        else if (fogMode == FogMode.Exponential)
        {
            Shader.DisableKeyword("FOG_LINEAR");
            Shader.EnableKeyword("FOG_EXP");
            Shader.DisableKeyword("FOG_EXP2");
        }
        else if (fogMode == FogMode.ExponentialSquared)
        {
            Shader.DisableKeyword("FOG_LINEAR");
            Shader.DisableKeyword("FOG_EXP");
            Shader.EnableKeyword("FOG_EXP2");
        }

        // Re-apply color in case color space differences on device washed it out
        RenderSettings.fogColor = fogColor;
    }

    void OnEnable()
    {
        if (enableFog)
        {
            RenderSettings.fog = true;
            ForceFogKeywords();
        }
    }
    
    // Sidebar initialization and control methods
    void InitializeSidebar()
    {
        // Try to get RectTransform if not directly assigned
        if (sidebarRect == null && sidebar != null)
        {
            sidebarRect = sidebar.GetComponent<RectTransform>();
        }
        
        if (sidebarRect != null)
        {
            // Set the visible position to the specified coordinates
            // With top-left anchor: X positive = right, Y positive = down
            sidebarVisiblePosition = new Vector2(17f, -257f);
            
            // Calculate hidden position (off-screen to the left)
            // Use screen width to ensure it's completely off-screen
            Canvas canvas = sidebarRect.GetComponentInParent<Canvas>();
            float screenWidth = 2000f; // Default large value
            
            if (canvas != null)
            {
                if (canvas.renderMode == RenderMode.ScreenSpaceOverlay)
                {
                    screenWidth = Screen.width;
                }
                else if (canvas.renderMode == RenderMode.ScreenSpaceCamera || canvas.renderMode == RenderMode.WorldSpace)
                {
                    // For non-overlay canvases, use canvas scaler or a large fixed value
                    UnityEngine.UI.CanvasScaler scaler = canvas.GetComponent<UnityEngine.UI.CanvasScaler>();
                    if (scaler != null)
                    {
                        screenWidth = scaler.referenceResolution.x;
                    }
                    else
                    {
                        screenWidth = 2000f; // Fallback large value
                    }
                }
            }
            
            // Hidden position is far to the left (negative screen width), preserving Y position
            sidebarHiddenPosition = new Vector2(-screenWidth, sidebarVisiblePosition.y);
            
            // Start with sidebar hidden (move off-screen left and deactivate)
            sidebarRect.anchoredPosition = sidebarHiddenPosition;
            if (sidebar != null)
            {
                sidebar.SetActive(false);
            }
            
            Debug.Log($"[CameraModeController] Sidebar initialized - Visible: {sidebarVisiblePosition}, Hidden: {sidebarHiddenPosition}, Screen Width: {screenWidth}");
        }
        else if (sidebar != null)
        {
            Debug.LogWarning("[CameraModeController] Sidebar RectTransform not found! Building button toggle will not work.");
        }
    }
    
    void ToggleSidebar()
    {
        // Don't toggle in interior mode
        InteriorExteriorManager interiorManager = FindFirstObjectByType<InteriorExteriorManager>();
        if (interiorManager != null && interiorManager.IsInteriorMode())
        {
            Debug.Log($"[CameraModeController] Sidebar toggle blocked - in interior mode (isInteriorMode: {interiorManager.IsInteriorMode()})");
            return;
        }
        
        if (isSidebarOpen)
        {
            CloseSidebar();
        }
        else
        {
            OpenSidebar();
        }
    }
    
    void OpenSidebar()
    {
        // Don't show sidebar in interior mode
        InteriorExteriorManager interiorManager = FindFirstObjectByType<InteriorExteriorManager>();
        if (interiorManager != null && interiorManager.IsInteriorMode())
        {
            Debug.Log($"[CameraModeController] Sidebar not shown - in interior mode (isInteriorMode: {interiorManager.IsInteriorMode()})");
            return;
        }
        
        if (sidebar == null || sidebarRect == null) 
        {
            Debug.LogWarning("[CameraModeController] Sidebar or RectTransform not assigned!");
            return;
        }
        
        // If already open, don't do anything
        if (isSidebarOpen)
        {
            Debug.Log("[CameraModeController] Sidebar already open, ignoring open request");
            return;
        }
        
        if (sidebarSlideCoroutine != null)
        {
            StopCoroutine(sidebarSlideCoroutine);
            sidebarSlideCoroutine = null;
        }
        
        // Ensure visible position is set correctly before opening
        sidebarVisiblePosition = new Vector2(17f, -257f);
        sidebarHiddenPosition = new Vector2(-2000f, sidebarVisiblePosition.y);
        
        sidebar.SetActive(true);
        sidebarSlideCoroutine = StartCoroutine(SlideSidebar(sidebarHiddenPosition, sidebarVisiblePosition));
        isSidebarOpen = true;
        
        Debug.Log($"[CameraModeController] Sidebar opened - Target position: {sidebarVisiblePosition}, isInteriorMode: {(interiorManager != null ? interiorManager.IsInteriorMode().ToString() : "N/A")}");
    }
    
    void CloseSidebar()
    {
        CloseSidebarPublic();
    }
    
    // Public method to close sidebar (called from other scripts)
    public void CloseSidebarPublic()
    {
        if (sidebarRect == null) 
        {
            Debug.LogWarning("[CameraModeController] Sidebar RectTransform not assigned!");
            return;
        }
        
        // Only close if actually open
        if (!isSidebarOpen)
        {
            return; // Already closed, don't do anything
        }
        
        if (sidebarSlideCoroutine != null)
        {
            StopCoroutine(sidebarSlideCoroutine);
        }
        
        sidebarSlideCoroutine = StartCoroutine(SlideSidebar(sidebarVisiblePosition, sidebarHiddenPosition));
        isSidebarOpen = false;
        
        Debug.Log("[CameraModeController] Sidebar closed");
    }
    
    System.Collections.IEnumerator SlideSidebar(Vector2 startPos, Vector2 endPos)
    {
        float elapsedTime = 0f;
        
        while (elapsedTime < sidebarSlideDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / sidebarSlideDuration;
            float curveValue = sidebarSlideCurve.Evaluate(progress);
            
            sidebarRect.anchoredPosition = Vector2.Lerp(startPos, endPos, curveValue);
            
            yield return null;
        }
        
        sidebarRect.anchoredPosition = endPos;
        
        // Hide sidebar GameObject when fully closed
        if (!isSidebarOpen && sidebar != null)
        {
            sidebar.SetActive(false);
        }
        
        sidebarSlideCoroutine = null;
    }
}
