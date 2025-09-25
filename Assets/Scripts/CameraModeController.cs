using UnityEngine;
using UnityEngine.UI;

public enum TourMode { Mode2D, Drone, Walk }

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
    
    [Header("Eye Button Settings")]
    private bool joysticksVisible = true;     // Track joystick visibility state

    [Header("Controllers")]
    public Camera2DController camera2DController; // MainCamera Camera2DController (optional)
    public DroneController droneController;       // Assign DroneController
    public WalkController walkController;         // Assign WalkController

    [Header("Buttons (assign Button components)")]
    public Button btn2D;       // assign Btn2D Button component
    public Button btnDrone;    // assign BtnDrone Button component
    public Button btnWalk;     // assign BtnWalk Button component
    public Button eyeButton;   // assign EyeButton Button component
    
    [Header("Button Visual States")]
    public Color activeButtonColor = new Color(0.5f, 0.5f, 0.5f, 1f);  // Dimmed color for active button
    public Color normalButtonColor = new Color(1f, 1f, 1f, 1f);         // Normal color for inactive buttons

    [Header("Settings")]
    public float transitionSpeed = 5f;
    
    [Header("Fog Settings")]
    public bool enableFog = true;
    public float fogStartDistance = 50f;
    public float fogEndDistance = 100f;
    public Color fogColor = new Color(0.5f, 0.5f, 0.5f, 1f);
    public FogMode fogMode = FogMode.Linear;

    [HideInInspector]
    public TourMode currentMode;

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

        // Ensure controllers start disabled
        if (camera2DController != null) camera2DController.enabled = false;
        if (droneController != null) droneController.enabled = false;
        if (walkController != null) walkController.enabled = false;

        // Ensure joystick container starts hidden if set
        if (joystickContainer != null) joystickContainer.SetActive(false);

        // Set initial 2D camera rotation and height
        if (mainCamera != null)
        {
            Vector3 currentPosition = mainCamera.transform.position;
            mainCamera.transform.position = new Vector3(currentPosition.x, 180f, currentPosition.z);
            mainCamera.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
            Debug.Log("[CameraModeController] Set initial 2D camera height to 180 and rotation to 90 degrees down");
        }

        // Initialize fog settings
        SetupFog();

        // Force initial mode to 2D
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
            camera2DController.enabled = (mode == TourMode.Mode2D);
            
            // Set camera height to 180 when entering 2D mode
            if (mode == TourMode.Mode2D && mainCamera != null)
            {
                Vector3 currentPosition = mainCamera.transform.position;
                Vector3 currentRotation = mainCamera.transform.eulerAngles;
                StartCoroutine(SmoothMoveTo(new Vector3(currentPosition.x, 180f, currentPosition.z)));
                StartCoroutine(SmoothRotateTo(90f, currentRotation.y, 0f));
                Debug.Log($"[CameraModeController] Smoothly moving camera to height 180 and rotating to (90, Y, 0) for 2D mode");
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
            droneController.enabled = (mode == TourMode.Drone);
            Debug.Log($"[CameraModeController] DroneController enabled: {droneController.enabled}");
            
            // Set camera to 30-degree X rotation and height 50 when entering drone mode
            if (mode == TourMode.Drone && mainCamera != null)
            {
                Vector3 currentPosition = mainCamera.transform.position;
                Vector3 currentRotation = mainCamera.transform.eulerAngles;
                StartCoroutine(SmoothMoveTo(new Vector3(currentPosition.x, 50f, currentPosition.z)));
                StartCoroutine(SmoothRotateTo(30f, currentRotation.y, 0f));
                Debug.Log($"[CameraModeController] Smoothly moving camera to height 50 and rotating to 30 degrees for drone mode");
            }
        }
        
        // Walk mode logic - only set height and rotation when entering walk mode
        if (walkController != null && mode == TourMode.Walk && mainCamera != null)
        {
            Vector3 currentPosition = mainCamera.transform.position;
            Vector3 currentRotation = mainCamera.transform.eulerAngles;
            StartCoroutine(SmoothMoveTo(new Vector3(currentPosition.x, 2f, currentPosition.z)));
            StartCoroutine(SmoothRotateTo(0f, currentRotation.y, 0f));
            Debug.Log($"[CameraModeController] Smoothly moving camera to height 2 and rotating to 0 degrees for walk mode");
        }

        // Reset joystick visibility state when switching to 2D mode
        if (mode == TourMode.Mode2D)
        {
            joysticksVisible = true; // Reset to visible for when switching back to drone/walk
        }
        
        // Joysticks visible only in Drone/Walk, and only if joysticksVisible is true
        if (joystickContainer != null)
        {
            bool showJoysticks = (mode != TourMode.Mode2D) && joysticksVisible;
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

        // Eye button visible only in Drone/Walk
        if (eyeButton != null)
            eyeButton.gameObject.SetActive(mode != TourMode.Mode2D);
            
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
        }
    }

    // Public wrappers (safe for inspector OnClick too)
    public void On2DButton() => SwitchMode(TourMode.Mode2D);
    public void OnDroneButton() => SwitchMode(TourMode.Drone);
    public void OnWalkButton() => SwitchMode(TourMode.Walk);
    
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
        if (!enableFog) return;
        
        switch (mode)
        {
            case TourMode.Mode2D:
                // 2D mode: Longer visibility for overview
                RenderSettings.fogStartDistance = fogStartDistance * 2f;
                RenderSettings.fogEndDistance = fogEndDistance * 2f;
                Debug.Log("[CameraModeController] 2D mode fog - Extended visibility");
                break;
                
            case TourMode.Drone:
                // Drone mode: Medium visibility
                RenderSettings.fogStartDistance = fogStartDistance;
                RenderSettings.fogEndDistance = fogEndDistance;
                Debug.Log("[CameraModeController] Drone mode fog - Standard visibility");
                break;
                
            case TourMode.Walk:
                // Walk mode: Shorter visibility for ground-level view
                RenderSettings.fogStartDistance = fogStartDistance * 0.5f;
                RenderSettings.fogEndDistance = fogEndDistance * 0.5f;
                Debug.Log("[CameraModeController] Walk mode fog - Reduced visibility");
                break;
        }

        // Ensure fog remains enabled and keywords are set after mode switches
        RenderSettings.fog = true;
        ForceFogKeywords();
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
}
