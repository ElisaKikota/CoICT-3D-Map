using UnityEngine;

/// <summary>
/// Camera controller for 3D mode with touch-based controls
/// Similar to Drone mode but uses screen controls instead of joysticks
/// - Pinch zoom = moves camera forward/backward and up/down (since camera is at angle)
/// - No camera rotation - maintains fixed rotation: x=30, y=225, z=0
/// </summary>
public class Camera3DController : MonoBehaviour
{
    [Header("Movement Settings")]
    [Tooltip("Speed of camera movement (forward/backward, left/right)")]
    public float moveSpeed = 30f;
    
    [Tooltip("Speed of camera zoom (pinch)")]
    public float zoomSpeed = 0.5f;
    
    [Header("Height Settings")]
    [Tooltip("Minimum height for camera")]
    public float minHeight = 10f;
    
    [Tooltip("Maximum height for camera")]
    public float maxHeight = 200f;
    
    [Header("Boundary Settings")]
    [Tooltip("Reference to BoundaryController for movement limits. If null, will search for it.")]
    public BoundaryController boundaryController;
    
    [Header("Ground Reference")]
    [Tooltip("Reference to ground GameObject/Transform. If null, will auto-search for 'Ground' GameObject. Used for accurate screen-to-world conversion.")]
    public Transform groundReference;
    [Tooltip("Ground Y position if groundReference is not found. Default is 0.")]
    public float groundYPosition = 0f;
    
    [Header("Camera Rotation")]
    [Tooltip("Fixed X rotation (pitch)")]
    public float fixedXRotation = 30f;
    
    [Tooltip("Fixed Y rotation (yaw)")]
    public float fixedYRotation = 225f;
    
    [Tooltip("Fixed Z rotation (roll)")]
    public float fixedZRotation = 0f;
    
    private Camera mainCamera;
    private float lastPinchDistance = 0f;
    private Vector2 lastTouchPosition;
    private bool isDragging = false;
    private float groundLevel;
    
    void Start()
    {
        // Get camera component
        mainCamera = GetComponent<Camera>();
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
            Debug.LogWarning("[Camera3DController] No Camera component found on this GameObject, using Camera.main");
        }
        
        // Find boundary controller if not assigned
        if (boundaryController == null)
        {
            boundaryController = FindFirstObjectByType<BoundaryController>();
        }
        
        // Auto-find ground reference if not assigned
        if (groundReference == null)
        {
            GameObject groundObj = GameObject.Find("Ground");
            if (groundObj != null)
            {
                groundReference = groundObj.transform;
                Debug.Log($"[Camera3DController] Auto-found Ground GameObject: {groundObj.name}");
            }
        }
        
        // Determine ground level
        if (groundReference != null)
        {
            groundLevel = groundReference.position.y;
            Debug.Log($"[Camera3DController] Ground level set from groundReference: {groundLevel}");
        }
        else
        {
            groundLevel = groundYPosition;
            Debug.Log($"[Camera3DController] Ground level set to default: {groundLevel}");
        }
        
        // Ensure camera is in perspective mode
        if (mainCamera != null)
        {
            mainCamera.orthographic = false;
            
            // Initialize rotation to fixed values (x=30, y=225, z=0)
            mainCamera.transform.rotation = Quaternion.Euler(fixedXRotation, fixedYRotation, fixedZRotation);
        }
    }
    
    void Update()
    {
        if (mainCamera == null || !enabled) return;
        
        // Handle touch input
        if (Input.touchCount == 2)
        {
            // Pinch zoom - affects height and horizontal movement
            HandlePinchZoom();
            isDragging = false;
        }
        else if (Input.touchCount == 1)
        {
            // Single touch drag - camera movement (like right joystick: up/down/left/right)
            HandleTouchMovement();
        }
        else
        {
            // No touches - reset states
            isDragging = false;
            lastPinchDistance = 0f;
        }
        
        // Maintain fixed rotation (x=30, y=225, z=0) - no rotation changes
        mainCamera.transform.rotation = Quaternion.Euler(fixedXRotation, fixedYRotation, fixedZRotation);
    }
    
    /// <summary>
    /// Handles single touch drag for camera movement (like right joystick: up/down/left/right)
    /// Slide up = move forward, slide down = move backward
    /// Slide left = strafe left, slide right = strafe right
    /// </summary>
    void HandleTouchMovement()
    {
        Touch touch = Input.GetTouch(0);
        
        if (touch.phase == TouchPhase.Began)
        {
            lastTouchPosition = touch.position;
            isDragging = true;
        }
        else if (touch.phase == TouchPhase.Moved && isDragging)
        {
            Vector2 deltaPosition = touch.position - lastTouchPosition;
            
            // Convert screen delta to world delta at ground level (accounts for camera height and angle)
            Vector3 worldDelta = ConvertScreenDeltaToWorldDelta(deltaPosition, groundLevel);
            
            // Calculate movement direction based on camera's orientation
            Vector3 cameraForward = mainCamera.transform.forward;
            Vector3 cameraRight = mainCamera.transform.right;
            
            // Project to horizontal plane (XZ) - same as DroneController
            Vector3 horizontalForward = new Vector3(cameraForward.x, 0, cameraForward.z).normalized;
            Vector3 horizontalRight = new Vector3(cameraRight.x, 0, cameraRight.z).normalized;
            
            // Convert world delta to movement along camera's forward/right axes
            // World delta Z is forward/backward in world space (already correct direction)
            // World delta X is left/right in world space
            // Need to invert X so screen right = camera move right (not left)
            float forwardInput = worldDelta.z;
            float strafeInput = -worldDelta.x; // Invert X - screen right = camera move right
            
            // Calculate movement - use camera's forward/right vectors
            Vector3 forwardMovement = horizontalForward * forwardInput;
            Vector3 strafeMovement = horizontalRight * strafeInput;
            Vector3 totalMovement = forwardMovement + strafeMovement;
            
            // Apply movement
            Vector3 newPosition = mainCamera.transform.position + totalMovement;
            
            // Clamp position within boundary if boundary controller is available
            if (boundaryController != null && boundaryController.exteriorBoundary != null)
            {
                newPosition = boundaryController.ClampToExteriorBoundary(newPosition);
            }
            
            mainCamera.transform.position = newPosition;
            
            lastTouchPosition = touch.position;
        }
        else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
        {
            isDragging = false;
        }
    }
    
    /// <summary>
    /// Handles pinch zoom - moves camera forward/backward and adjusts height
    /// Since camera is at an angle, zoom affects both height and horizontal position
    /// </summary>
    void HandlePinchZoom()
    {
        Touch touch0 = Input.GetTouch(0);
        Touch touch1 = Input.GetTouch(1);
        
        // Calculate current distance between touches
        float currentDistance = Vector2.Distance(touch0.position, touch1.position);
        
        // If this is the first frame of pinch, store the initial distance
        if (lastPinchDistance == 0f || touch0.phase == TouchPhase.Began || touch1.phase == TouchPhase.Began)
        {
            lastPinchDistance = currentDistance;
            return;
        }
        
        // Calculate the difference in distance from last frame
        float deltaDistance = currentDistance - lastPinchDistance;
        
        // Calculate movement direction based on camera's forward direction
        Vector3 cameraForward = mainCamera.transform.forward;
        Vector3 cameraRight = mainCamera.transform.right;
        
        // Project forward direction to horizontal plane (XZ)
        Vector3 horizontalForward = new Vector3(cameraForward.x, 0, cameraForward.z).normalized;
        
        // Zoom in (pinch closer) = move forward and down
        // Zoom out (pinch apart) = move backward and up
        float zoomAmount = deltaDistance * zoomSpeed;
        
        // Move forward/backward in camera's forward direction
        Vector3 forwardMovement = horizontalForward * zoomAmount;
        
        // Adjust height (pinch in = lower, pinch out = higher)
        float heightAdjustment = -zoomAmount * 0.5f; // Scale height adjustment
        
        Vector3 newPosition = mainCamera.transform.position;
        newPosition += forwardMovement;
        newPosition.y += heightAdjustment;
        
        // Clamp height
        newPosition.y = Mathf.Clamp(newPosition.y, minHeight, maxHeight);
        
        // Clamp position within boundary if boundary controller is available
        if (boundaryController != null && boundaryController.exteriorBoundary != null)
        {
            newPosition = boundaryController.ClampToExteriorBoundary(newPosition);
        }
        
        mainCamera.transform.position = newPosition;
        
        // Update last distance for next frame
        lastPinchDistance = currentDistance;
    }
    
    /// <summary>
    /// Converts screen space delta (in pixels) to world space delta at the specified ground level.
    /// Accounts for camera angle (30 degrees) and height to ensure accurate movement scaling.
    /// </summary>
    private Vector3 ConvertScreenDeltaToWorldDelta(Vector2 screenDelta, float groundY)
    {
        if (mainCamera == null) return Vector3.zero;
        
        float cameraHeight = mainCamera.transform.position.y - groundY;
        
        if (cameraHeight <= 0.1f)
        {
            Debug.LogWarning($"[Camera3DController] Camera height too low ({cameraHeight}), cannot calculate world delta");
            return Vector3.zero;
        }
        
        // For a camera at 30 degrees pitch, calculate visible area at ground level
        // The camera's view frustum intersects the ground plane at an angle
        float fovRad = mainCamera.fieldOfView * Mathf.Deg2Rad;
        
        // Calculate the distance from camera to ground intersection point (along view direction)
        // Camera pitch is 30 degrees, so the view direction is 30 degrees down from horizontal
        float pitchRad = fixedXRotation * Mathf.Deg2Rad;
        float distanceToGround = cameraHeight / Mathf.Sin(pitchRad); // Distance along view direction
        
        // Calculate the visible area at ground level
        // At the ground plane, the frustum forms a trapezoid
        // We calculate the size at the center of the visible area
        float halfFovHeight = distanceToGround * Mathf.Tan(fovRad * 0.5f);
        float halfFovWidth = halfFovHeight * mainCamera.aspect;
        
        // Total visible world dimensions at ground level
        float visibleWorldWidth = halfFovWidth * 2f;
        float visibleWorldHeight = halfFovHeight * 2f;
        
        // Convert screen pixels to world units
        float worldUnitsPerPixelX = visibleWorldWidth / Screen.width;
        float worldUnitsPerPixelY = visibleWorldHeight / Screen.height;
        
        // Apply the conversion (screen Y is inverted in Unity)
        Vector3 worldDelta = new Vector3(
            screenDelta.x * worldUnitsPerPixelX,
            0f,
            -screenDelta.y * worldUnitsPerPixelY  // Invert Y because screen coordinates are top-to-bottom
        );
        
        return worldDelta;
    }
}

