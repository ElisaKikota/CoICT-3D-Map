using UnityEngine;

public class Camera2DController : MonoBehaviour
{
    [Tooltip("Pan sensitivity multiplier (1.0 = 1:1 screen to world movement). Lower values = less sensitive.")]
    public float panSensitivity = 1.0f;
    [Tooltip("Zoom speed when using buttons or scroll wheel")]
    public float zoomSpeed = 5f;
    [Tooltip("Pinch zoom sensitivity")]
    public float pinchZoomSpeed = 0.1f;

    [Header("Two-Finger Rotation")]
    public bool enableTwoFingerRotation = true;
    public float twoFingerRotationSpeed = 1f;
    [Tooltip("Minimum camera height (zoomed in)")]
    public float minHeight = 50f;
    [Tooltip("Maximum camera height (zoomed out)")]
    public float maxHeight = 350f;
    
    [Header("Boundary Settings")]
    [Tooltip("Reference to BoundaryController for movement limits. If null, will search for it.")]
    public BoundaryController boundaryController;
    
    [Header("Ground Reference")]
    [Tooltip("Reference to ground GameObject/Transform. If null, will auto-search for 'Ground' GameObject. Used for accurate screen-to-world conversion.")]
    public Transform groundReference;
    [Tooltip("Ground Y position if groundReference is not found. Default is 0.")]
    public float groundYPosition = 0f;

    private Vector3 lastMousePos;
    private Vector2 lastTouchPos;
    private bool isTouchPanning;
    private float lastPinchDistance = 0f;
    private Camera cam;
    private float groundLevel;
    private Canvas uiCanvas; // For getting canvas dimensions if needed

    void Start()
    {
        InitializeCamera();
    }

    void OnEnable()
    {
        // Re-initialize camera when controller is enabled (in case it was disabled)
        InitializeCamera();
    }

    void InitializeCamera()
    {
        // Get camera component (more reliable than Camera.main)
        if (cam == null)
        {
            cam = GetComponent<Camera>();
            if (cam == null)
            {
                cam = Camera.main;
                Debug.LogWarning("[Camera2DController] No Camera component found on this GameObject, using Camera.main");
            }
        }
        
        // Ensure camera is in perspective mode (like drone/walk modes)
        if (cam != null)
        {
            cam.orthographic = false;
            Debug.Log($"[Camera2DController] Camera set to perspective mode. Current height: {transform.position.y}");
        }
        else
        {
            Debug.LogError("[Camera2DController] No camera found! Zoom will not work.");
        }
        
        // Auto-find ground reference if not assigned
        if (groundReference == null)
        {
            GameObject groundObj = GameObject.Find("Ground");
            if (groundObj != null)
            {
                groundReference = groundObj.transform;
                Debug.Log($"[Camera2DController] Auto-found Ground GameObject: {groundObj.name}");
            }
        }
        
        // Determine ground level
        if (groundReference != null)
        {
            groundLevel = groundReference.position.y;
            Debug.Log($"[Camera2DController] Ground level set from groundReference: {groundLevel} (from {groundReference.name})");
        }
        else
        {
            groundLevel = groundYPosition;
            Debug.Log($"[Camera2DController] Ground level set to default: {groundLevel}");
        }
        
        // Find UI Canvas for reference (optional, for future use)
        uiCanvas = FindFirstObjectByType<Canvas>();
        if (uiCanvas != null)
        {
            Debug.Log($"[Camera2DController] Found UI Canvas: {uiCanvas.name}, Scale: {uiCanvas.scaleFactor}");
        }
        
        // Find boundary controller if not assigned
        if (boundaryController == null)
        {
            boundaryController = FindFirstObjectByType<BoundaryController>();
        }
    }

    void Update()
    {
        // Handle touch input for pinch zoom (check this first to prevent panning during pinch)
        if (Input.touchCount == 2)
        {
            Touch touch0 = Input.GetTouch(0);
            Touch touch1 = Input.GetTouch(1);

            // --- Two finger rotation (twist) ---
            // We compute angle change between the touch vector in the previous frame and current frame.
            if (enableTwoFingerRotation)
            {
                Vector2 prevPos0 = touch0.position - touch0.deltaPosition;
                Vector2 prevPos1 = touch1.position - touch1.deltaPosition;

                Vector2 prevVector = prevPos1 - prevPos0;
                Vector2 currentVector = touch1.position - touch0.position;

                if (prevVector.sqrMagnitude > 0.01f && currentVector.sqrMagnitude > 0.01f)
                {
                    float deltaAngle = Vector2.SignedAngle(prevVector, currentVector);

                    // Positive twist should feel natural; invert if needed by adjusting the sign here.
                    transform.Rotate(0f, deltaAngle * twoFingerRotationSpeed, 0f, Space.World);
                }
            }
            
            // Calculate current distance between touches
            float currentDistance = Vector2.Distance(touch0.position, touch1.position);
            
            // If this is the first frame of pinch, store the initial distance
            if (lastPinchDistance == 0f || touch0.phase == TouchPhase.Began || touch1.phase == TouchPhase.Began)
            {
                lastPinchDistance = currentDistance;
            }
            else
            {
                // Calculate the difference in distance from last frame (inverted for correct zoom direction)
                float deltaDistance = currentDistance - lastPinchDistance;
                
                // Apply zoom by changing camera height (pinch in = zoom in = lower height, pinch out = zoom out = higher height)
                // When fingers move closer (pinch in): currentDistance < lastPinchDistance, so deltaDistance is negative
                // To zoom in, we decrease height, so we subtract negative deltaDistance (which adds to decrease height)
                // When fingers move apart (pinch out): currentDistance > lastPinchDistance, so deltaDistance is positive
                // To zoom out, we increase height, so we subtract positive deltaDistance (which subtracts, increasing height)
                if (cam != null)
                {
                    Vector3 newPosition = transform.position;
                    newPosition.y -= deltaDistance * pinchZoomSpeed;
                    newPosition.y = Mathf.Clamp(newPosition.y, minHeight, maxHeight);
                    transform.position = newPosition;
                }
                
                // Update last distance for next frame
                lastPinchDistance = currentDistance;
            }
        }
        else
        {
            // Reset pinch distance when not pinching
            lastPinchDistance = 0f;

            // One-finger touch pan (mobile)
            if (Input.touchCount == 1 && cam != null)
            {
                Touch t = Input.GetTouch(0);

                if (t.phase == TouchPhase.Began)
                {
                    isTouchPanning = true;
                    lastTouchPos = t.position;
                }
                else if ((t.phase == TouchPhase.Moved || t.phase == TouchPhase.Stationary) && isTouchPanning)
                {
                    Vector2 deltaScreen2 = t.position - lastTouchPos;
                    Vector3 worldDelta = ConvertScreenDeltaToWorldDelta(new Vector3(deltaScreen2.x, deltaScreen2.y, 0f), groundLevel);
                    worldDelta *= panSensitivity;

                    float yaw = transform.eulerAngles.y;
                    Quaternion yawRot = Quaternion.Euler(0f, yaw, 0f);
                    Vector3 rightYaw = yawRot * Vector3.right;
                    Vector3 forwardYaw = yawRot * Vector3.forward;

                    Vector3 newPosition = transform.position;
                    newPosition += (-rightYaw * worldDelta.x) + (forwardYaw * worldDelta.z);

                    if (boundaryController != null && boundaryController.exteriorBoundary != null)
                    {
                        Vector3 clamped = boundaryController.ClampToExteriorBoundary(newPosition);
                        newPosition = new Vector3(clamped.x, newPosition.y, clamped.z);
                    }

                    transform.position = newPosition;
                    lastTouchPos = t.position;
                }
                else if (t.phase == TouchPhase.Ended || t.phase == TouchPhase.Canceled)
                {
                    isTouchPanning = false;
                }
            }
            
            // Pan (editor / desktop) - using screen-to-world conversion for accurate movement
            if (Input.touchCount == 0)
            {
                if (Input.GetMouseButtonDown(0)) lastMousePos = Input.mousePosition;
                if (Input.GetMouseButton(0) && cam != null)
                {
                    Vector3 currentScreenPos = Input.mousePosition;
                    Vector3 deltaScreen = currentScreenPos - lastMousePos;

                    Vector3 worldDelta = ConvertScreenDeltaToWorldDelta(deltaScreen, groundLevel);
                    worldDelta *= panSensitivity;

                    float yaw = transform.eulerAngles.y;
                    Quaternion yawRot = Quaternion.Euler(0f, yaw, 0f);
                    Vector3 rightYaw = yawRot * Vector3.right;
                    Vector3 forwardYaw = yawRot * Vector3.forward;

                    Vector3 newPosition = transform.position;
                    newPosition += (-rightYaw * worldDelta.x) + (forwardYaw * worldDelta.z);

                    if (boundaryController != null && boundaryController.exteriorBoundary != null)
                    {
                        Vector3 clamped = boundaryController.ClampToExteriorBoundary(newPosition);
                        newPosition = new Vector3(clamped.x, newPosition.y, clamped.z);
                    }

                    transform.position = newPosition;
                    lastMousePos = currentScreenPos;
                }
            }
        }

        // Zoom with mouse scroll wheel (controls camera height)
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0f && cam != null)
        {
            Vector3 newPosition = transform.position;
            // Scroll up = zoom in = lower height, scroll down = zoom out = higher height
            newPosition.y -= scroll * zoomSpeed;
            newPosition.y = Mathf.Clamp(newPosition.y, minHeight, maxHeight);
            transform.position = newPosition;
        }
    }

    public void ZoomIn()
    {
        if (cam != null)
        {
            Vector3 newPosition = transform.position;
            // Zoom in = lower height
            newPosition.y -= zoomSpeed;
            newPosition.y = Mathf.Clamp(newPosition.y, minHeight, maxHeight);
            transform.position = newPosition;
            Debug.Log($"[Camera2DController] Zoom In - New height: {newPosition.y}");
        }
        else
        {
            Debug.LogWarning("[Camera2DController] Cannot zoom in - camera is null!");
        }
    }

    public void ZoomOut()
    {
        if (cam != null)
        {
            Vector3 newPosition = transform.position;
            // Zoom out = higher height
            newPosition.y += zoomSpeed;
            newPosition.y = Mathf.Clamp(newPosition.y, minHeight, maxHeight);
            transform.position = newPosition;
            Debug.Log($"[Camera2DController] Zoom Out - New height: {newPosition.y}");
        }
        else
        {
            Debug.LogWarning("[Camera2DController] Cannot zoom out - camera is null!");
        }
    }
    
    /// <summary>
    /// Converts screen space delta (in pixels) to world space delta at the specified ground level.
    /// This ensures that finger movement on screen translates to proportional movement on the ground.
    /// Uses accurate raycasting for top-down perspective camera.
    /// </summary>
    private Vector3 ConvertScreenDeltaToWorldDelta(Vector3 screenDelta, float groundY)
    {
        if (cam == null) return Vector3.zero;
        
        // For top-down camera (90 degrees), use direct calculation based on camera height and FOV
        // This is more accurate and performant than raycasting
        float cameraHeight = transform.position.y - groundY;
        
        if (cameraHeight <= 0.1f)
        {
            Debug.LogWarning($"[Camera2DController] Camera height too low ({cameraHeight}), cannot calculate world delta");
            return Vector3.zero;
        }
        
        // Calculate the visible world area at ground level using camera FOV and height
        // For a perspective camera looking straight down (90 degrees):
        // - The visible area forms a frustum
        // - At ground level, we calculate the width and height of the visible rectangle
        
        float fovRad = cam.fieldOfView * Mathf.Deg2Rad;
        
        // Calculate half-angles for the frustum
        float halfHeight = cameraHeight * Mathf.Tan(fovRad * 0.5f);
        float halfWidth = halfHeight * cam.aspect;
        
        // Total visible world dimensions at ground level
        float visibleWorldWidth = halfWidth * 2f;
        float visibleWorldHeight = halfHeight * 2f;
        
        // Convert screen pixels to world units
        // This gives us the exact world movement per pixel on screen
        float worldUnitsPerPixelX = visibleWorldWidth / Screen.width;
        float worldUnitsPerPixelY = visibleWorldHeight / Screen.height;
        
        // Apply the conversion (note: screen Y is inverted in Unity, so we invert it back)
        Vector3 worldDelta = new Vector3(
            screenDelta.x * worldUnitsPerPixelX,
            0f,
            -screenDelta.y * worldUnitsPerPixelY  // Invert Y because screen coordinates are top-to-bottom
        );
        
        // Debug logging (can be removed in production)
        // Debug.Log($"[Camera2DController] Screen delta: {screenDelta}, World delta: {worldDelta}, Height: {cameraHeight}, UnitsPerPixel: ({worldUnitsPerPixelX}, {worldUnitsPerPixelY})");
        
        return worldDelta;
    }
}
