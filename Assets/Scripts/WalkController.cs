using UnityEngine;

public class WalkController : MonoBehaviour
{
    [Header("References")]
    public JoystickManager joystickManager;
    
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float rotateSpeed = 90f;  // Horizontal rotation speed
    
    [Header("Camera Height")]
    [Tooltip("Camera height (Y position) in walk mode")]
    public float cameraHeight = -1f;
    
    [Tooltip("Allow external systems to temporarily override camera height (used for floor transitions)")]
    private bool heightOverrideActive = false;
    private float heightOverride = -1f;
    
    [Header("Boundary Settings")]
    [Tooltip("Reference to BoundaryController for movement limits. If null, will search for it.")]
    public BoundaryController boundaryController;
    
    [Header("Collision Detection")]
    [Tooltip("Radius of the collision sphere around the camera (prevents going through walls)")]
    public float collisionRadius = 0.5f;
    
    [Tooltip("Layers to check for collisions (walls, obstacles, etc.)")]
    public LayerMask collisionLayers = -1; // All layers by default
    
    [Tooltip("Enable collision detection to prevent going through walls")]
    public bool enableCollisionDetection = true;
    
    [Tooltip("Only use collision detection when inside a room (interior mode). If false, collision detection works in both interior and exterior modes")]
    public bool onlyInInteriorMode = false;
    
    [Header("Debug")]
    [Tooltip("Show debug logs when collisions are detected")]
    public bool debugCollisions = false;
    
    private InteriorExteriorManager interiorManager;
    
    [Header("Exterior Colliders")]
    [Tooltip("Parent GameObject containing exterior building colliders (will be kept enabled for collision detection)")]
    public GameObject exteriorBuildingsParent;
    
    [Tooltip("Automatically find and ensure exterior building colliders are enabled")]
    public bool autoManageExteriorColliders = true;

    void Start()
    {
        // Find boundary controller if not assigned
        if (boundaryController == null)
        {
            boundaryController = FindFirstObjectByType<BoundaryController>();
        }
        
        // Find interior manager for checking if we're in interior mode
        if (interiorManager == null)
        {
            interiorManager = FindFirstObjectByType<InteriorExteriorManager>();
        }
        
        // Ensure exterior building colliders are enabled
        if (autoManageExteriorColliders)
        {
            EnsureExteriorCollidersEnabled();
        }
    }
    
    void OnEnable()
    {
        // Set camera height when walk mode is enabled (will use override if active)
        SetCameraHeight();
        
        if (heightOverrideActive)
        {
            Debug.Log($"[WalkController] OnEnable - Height override is active: {heightOverride}, setting camera to this height");
        }
        else
        {
            Debug.Log($"[WalkController] OnEnable - No height override, using default: {cameraHeight}");
        }
        
        // Ensure exterior colliders are enabled when walk mode is active
        if (autoManageExteriorColliders)
        {
            EnsureExteriorCollidersEnabled();
        }
    }

    void Update()
    {
        if (!enabled)
        {
            return; // Don't process if this component is disabled
        }
        
        if (joystickManager == null) 
        {
            Debug.LogWarning("[WalkController] JoystickManager is null!");
            return;
        }

        Vector2 left = joystickManager.GetLeftInput();
        Vector2 right = joystickManager.GetRightInput();

        // Check if we're in Walk mode - if not, don't process input
        if (joystickManager.current != UIControlMode.ModeWalk)
        {
            return; // Exit early if not in Walk mode
        }

        // Jump feature removed - no jumping in walk mode
        
        // Rotate camera/player Y axis (left joystick horizontal) - horizontal rotation
        transform.Rotate(0, left.x * rotateSpeed * Time.deltaTime, 0);

        // Move XZ plane only (project movement to avoid Y changes)
        // Use horizontal forward/right vectors to prevent vertical movement when camera is pitched
        Vector3 currentPosition = transform.position;
        Vector3 forward = new Vector3(transform.forward.x, 0, transform.forward.z).normalized * right.y * moveSpeed * Time.deltaTime;
        Vector3 strafe = new Vector3(transform.right.x, 0, transform.right.z).normalized * right.x * moveSpeed * Time.deltaTime;
        Vector3 movement = forward + strafe;
        
        // Calculate desired new position
        Vector3 newPosition = currentPosition + movement;
        
        // Maintain camera height (use override if active, otherwise use default)
        float currentHeight = heightOverrideActive ? heightOverride : cameraHeight;
        newPosition.y = currentHeight;
        
        // Debug: Log if camera Y position is different from expected height (every 60 frames = ~1 second)
        if (Time.frameCount % 60 == 0)
        {
            float actualY = transform.position.y;
            if (Mathf.Abs(actualY - currentHeight) > 0.1f)
            {
                Debug.LogWarning($"[WalkController] Camera Y ({actualY}) doesn't match expected height ({currentHeight}). Override active: {heightOverrideActive}, Override value: {heightOverride}, Default: {cameraHeight}");
            }
        }
        
        // Check for collisions if collision detection is enabled
        // Only check if we're in interior mode (if onlyInInteriorMode is true)
        bool shouldCheckCollision = enableCollisionDetection && movement.magnitude > 0.001f;
        if (onlyInInteriorMode && interiorManager != null)
        {
            shouldCheckCollision = shouldCheckCollision && interiorManager.IsInteriorMode();
        }
        
        // Ensure exterior colliders are enabled before checking collisions
        if (shouldCheckCollision && autoManageExteriorColliders && (interiorManager == null || !interiorManager.IsInteriorMode()))
        {
            EnsureExteriorCollidersEnabled();
        }
        
        if (shouldCheckCollision)
        {
            Vector3 originalPosition = newPosition;
            newPosition = CheckCollisionAndAdjustPosition(currentPosition, newPosition);
            
            if (debugCollisions && Vector3.Distance(newPosition, originalPosition) > 0.01f)
            {
                Debug.Log($"[WalkController] Position adjusted due to collision. Original: {originalPosition}, Adjusted: {newPosition}");
            }
        }
        
        // Clamp position within boundary if boundary controller is available
        // Only clamp X and Z for walk mode (Y stays fixed at current height)
        if (boundaryController != null && boundaryController.exteriorBoundary != null)
        {
            Vector3 clamped = boundaryController.ClampToExteriorBoundary(newPosition);
            // Preserve Y position (current height) when clamping - reuse the currentHeight variable already declared
            newPosition = new Vector3(clamped.x, currentHeight, clamped.z);
        }
        
        transform.position = newPosition;
        
        // Constrain rotations: Z always 0, X stays at 0 degrees
        Vector3 currentRotation = transform.eulerAngles;
        transform.rotation = Quaternion.Euler(0f, currentRotation.y, 0f);
    }
    
    /// <summary>
    /// Sets the camera height to the specified value
    /// </summary>
    void SetCameraHeight()
    {
        float currentHeight = heightOverrideActive ? heightOverride : cameraHeight;
        Vector3 pos = transform.position;
        pos.y = currentHeight;
        transform.position = pos;
    }
    
    /// <summary>
    /// Sets the camera height override (used by stair system for floor transitions)
    /// </summary>
    public void SetCameraHeightOverride(float height)
    {
        heightOverride = height;
        heightOverrideActive = true;
        SetCameraHeight(); // Apply immediately
        Debug.Log($"[WalkController] Camera height override set to: {height}");
    }
    
    /// <summary>
    /// Clears the camera height override and returns to default
    /// </summary>
    public void ClearCameraHeightOverride()
    {
        heightOverrideActive = false;
        heightOverride = -1f;
        SetCameraHeight(); // Apply immediately
        Debug.Log($"[WalkController] Camera height override cleared, returning to default: {cameraHeight}");
    }
    
    /// <summary>
    /// Gets the current effective camera height (override or default)
    /// </summary>
    public float GetCurrentCameraHeight()
    {
        return heightOverrideActive ? heightOverride : cameraHeight;
    }
    
    /// <summary>
    /// Checks if height override is currently active
    /// </summary>
    public bool IsHeightOverrideActive()
    {
        return heightOverrideActive;
    }
    
    /// <summary>
    /// Ensures all exterior building colliders are enabled for collision detection
    /// </summary>
    void EnsureExteriorCollidersEnabled()
    {
        // Only manage exterior colliders when not in interior mode
        if (interiorManager != null && interiorManager.IsInteriorMode())
        {
            return; // Don't manage colliders in interior mode
        }
        
        // If exteriorBuildingsParent is assigned, enable all colliders in it
        if (exteriorBuildingsParent != null)
        {
            Collider[] colliders = exteriorBuildingsParent.GetComponentsInChildren<Collider>();
            foreach (Collider col in colliders)
            {
                if (col != null && !col.isTrigger)
                {
                    col.enabled = true;
                }
            }
            
            if (debugCollisions && colliders.Length > 0)
            {
                Debug.Log($"[WalkController] Ensured {colliders.Length} exterior colliders are enabled");
            }
        }
        else
        {
            // Try to find buildings automatically
            GameObject buildingsObj = GameObject.Find("Buildings");
            if (buildingsObj == null)
            {
                buildingsObj = GameObject.Find("ExteriorBuildings");
            }
            
            if (buildingsObj != null)
            {
                Collider[] colliders = buildingsObj.GetComponentsInChildren<Collider>();
                foreach (Collider col in colliders)
                {
                    if (col != null && !col.isTrigger)
                    {
                        col.enabled = true;
                    }
                }
                
                if (debugCollisions && colliders.Length > 0)
                {
                    Debug.Log($"[WalkController] Auto-found and enabled {colliders.Length} exterior colliders from {buildingsObj.name}");
                }
            }
        }
    }
    
    /// <summary>
    /// Checks for collisions and adjusts position to prevent going through walls
    /// Uses sphere cast to detect collisions with walls
    /// </summary>
    Vector3 CheckCollisionAndAdjustPosition(Vector3 currentPos, Vector3 desiredPos)
    {
        Vector3 direction = desiredPos - currentPos;
        float distance = direction.magnitude;
        
        if (distance < 0.001f)
        {
            return currentPos; // No movement, return current position
        }
        
        direction.Normalize();
        
        // Get current camera height (use override if active, otherwise use default)
        float cameraHeightToUse = heightOverrideActive ? heightOverride : cameraHeight;
        
        // First, check if the desired position itself is inside a collider
        Vector3 desiredPosAtHeight = desiredPos;
        desiredPosAtHeight.y = cameraHeightToUse;
        Collider[] overlappingAtDestination = Physics.OverlapSphere(desiredPosAtHeight, collisionRadius, collisionLayers, QueryTriggerInteraction.Ignore);
        
        if (overlappingAtDestination.Length > 0)
        {
            // We're trying to move into a collider - don't allow it
            if (debugCollisions)
            {
                Debug.Log($"[WalkController] Destination position overlaps with collider(s): {string.Join(", ", System.Array.ConvertAll(overlappingAtDestination, c => c.name))}");
            }
            // Try to find a safe position by moving back along the direction
            return FindSafePosition(currentPos, desiredPos, direction, distance);
        }
        
        // Use a sphere cast to check for collisions along the path
        // The sphere represents the camera's collision radius
        RaycastHit hit;
        Vector3 sphereCenter = currentPos;
        sphereCenter.y = cameraHeightToUse; // Ensure we're checking at the correct height
        
        // Check if there's a collision in the movement direction
        // Use maxDistance slightly larger than the movement distance to catch collisions
        bool hasCollision = Physics.SphereCast(sphereCenter, collisionRadius, direction, out hit, distance + 0.1f, collisionLayers, QueryTriggerInteraction.Ignore);
        
        if (debugCollisions)
        {
            if (hasCollision)
            {
                Debug.Log($"[WalkController] Collision detected with {hit.collider.name} (Layer: {hit.collider.gameObject.layer}, IsTrigger: {hit.collider.isTrigger}, Enabled: {hit.collider.enabled}) at distance {hit.distance}, normal: {hit.normal}");
            }
            else
            {
                // Debug: Check if there are any colliders in the scene
                Collider[] nearbyColliders = Physics.OverlapSphere(sphereCenter, collisionRadius + distance, collisionLayers, QueryTriggerInteraction.Ignore);
                if (nearbyColliders.Length > 0)
                {
                    Debug.Log($"[WalkController] No collision detected, but found {nearbyColliders.Length} nearby colliders: {string.Join(", ", System.Array.ConvertAll(nearbyColliders, c => $"{c.name} (Layer:{c.gameObject.layer}, Trigger:{c.isTrigger}, Enabled:{c.enabled})"))}");
                }
                else
                {
                    // Check all colliders regardless of layer to see what's available
                    Collider[] allNearbyColliders = Physics.OverlapSphere(sphereCenter, collisionRadius + distance, -1, QueryTriggerInteraction.Ignore);
                    if (allNearbyColliders.Length > 0)
                    {
                        Debug.LogWarning($"[WalkController] Found {allNearbyColliders.Length} colliders nearby, but NONE match collision layers mask! Collision Layers: {collisionLayers.value}. Colliders: {string.Join(", ", System.Array.ConvertAll(allNearbyColliders, c => $"{c.name} (Layer:{c.gameObject.layer})"))}");
                    }
                    else
                    {
                        Debug.LogWarning($"[WalkController] No colliders found nearby at all! Position: {sphereCenter}, Search radius: {collisionRadius + distance}");
                    }
                }
            }
        }
        
        if (hasCollision)
        {
            // There's a collision - adjust position to stop just before the wall
            // Calculate the safe distance (hit distance minus collision radius)
            float safeDistance = Mathf.Max(0f, hit.distance - collisionRadius);
            
            // Move only the safe distance (reuse cameraHeightToUse already declared)
            Vector3 adjustedPosition = currentPos + direction * safeDistance;
            adjustedPosition.y = cameraHeightToUse; // Maintain camera height
            
            // Try to slide along the wall if possible
            // Calculate the remaining movement after hitting the wall
            Vector3 remainingMovement = desiredPos - adjustedPosition;
            float remainingDistance = remainingMovement.magnitude;
            
            if (remainingDistance > 0.001f)
            {
                // Project the remaining movement onto the wall plane (perpendicular to hit normal)
                Vector3 slideDirection = Vector3.ProjectOnPlane(remainingMovement, hit.normal).normalized;
                
                // Check if we can slide along the wall
                if (slideDirection.magnitude > 0.1f)
                {
                    // Check for collision in slide direction
                    RaycastHit slideHit;
                    if (!Physics.SphereCast(adjustedPosition, collisionRadius, slideDirection, out slideHit, remainingDistance, collisionLayers, QueryTriggerInteraction.Ignore))
                    {
                        // Can slide - move along the wall (reuse cameraHeightToUse already declared)
                        adjustedPosition += slideDirection * Mathf.Min(remainingDistance, remainingMovement.magnitude);
                        adjustedPosition.y = cameraHeightToUse;
                    }
                }
            }
            
            return adjustedPosition;
        }
        
        // No collision - safe to move to desired position
        return desiredPos;
    }
    
    /// <summary>
    /// Finds a safe position when destination overlaps with a collider
    /// </summary>
    Vector3 FindSafePosition(Vector3 currentPos, Vector3 desiredPos, Vector3 direction, float maxDistance)
    {
        // Get current camera height (use override if active, otherwise use default)
        float cameraHeightToUse = heightOverrideActive ? heightOverride : cameraHeight;
        
        // Try multiple positions along the path, getting closer to current position
        int steps = 10;
        for (int i = steps; i > 0; i--)
        {
            float t = (float)i / steps;
            Vector3 testPos = Vector3.Lerp(currentPos, desiredPos, t);
            testPos.y = cameraHeightToUse;
            
            Collider[] overlapping = Physics.OverlapSphere(testPos, collisionRadius, collisionLayers, QueryTriggerInteraction.Ignore);
            if (overlapping.Length == 0)
            {
                // Found a safe position
                if (debugCollisions)
                {
                    Debug.Log($"[WalkController] Found safe position at {testPos} (step {i}/{steps})");
                }
                return testPos;
            }
        }
        
        // Couldn't find a safe position - stay where we are
        if (debugCollisions)
        {
            Debug.Log($"[WalkController] Could not find safe position, staying at current position");
        }
        return currentPos;
    }
}
