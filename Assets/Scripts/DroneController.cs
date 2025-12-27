using UnityEngine;

public class DroneController : MonoBehaviour
{
    public JoystickManager joystickManager;
    public float moveSpeed = 30f;
    public float rotateSpeed = 90f;
    public float heightSpeed = 10f;
    
    [Header("Boundary Settings")]
    [Tooltip("Reference to BoundaryController for movement limits. If null, will search for it.")]
    public BoundaryController boundaryController;
    
    [Tooltip("Minimum height for drone camera")]
    public float minHeight = 10f;
    
    [Tooltip("Maximum height for drone camera")]
    public float maxHeight = 200f;
    
    [Header("Collision Detection")]
    [Tooltip("Radius of the collision sphere around the camera (prevents going through walls)")]
    public float collisionRadius = 0.5f;
    
    [Tooltip("Layers to check for collisions (walls, obstacles, etc.)")]
    public LayerMask collisionLayers = -1; // All layers by default
    
    [Tooltip("Enable collision detection to prevent going through walls")]
    public bool enableCollisionDetection = true;
    
    [Tooltip("Only use collision detection when inside a room (interior mode). If false, collision detection works in both interior and exterior modes")]
    public bool onlyInInteriorMode = false;
    
    [Header("Exterior Colliders")]
    [Tooltip("Parent GameObject containing exterior building colliders (will be kept enabled for collision detection)")]
    public GameObject exteriorBuildingsParent;
    
    [Tooltip("Automatically find and ensure exterior building colliders are enabled")]
    public bool autoManageExteriorColliders = true;
    
    [Header("Debug")]
    [Tooltip("Show debug logs when collisions are detected")]
    public bool debugCollisions = false;
    
    private InteriorExteriorManager interiorManager;

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
        // Ensure exterior colliders are enabled when drone mode is active
        if (autoManageExteriorColliders)
        {
            EnsureExteriorCollidersEnabled();
        }
    }

    void Update()
    {
        if (joystickManager == null) 
        {
            Debug.LogWarning("[DroneController] JoystickManager is null!");
            return;
        }

        Vector2 left = joystickManager.GetLeftInput();
        Vector2 right = joystickManager.GetRightInput();

        // Debug logging
        if (left.magnitude > 0.1f || right.magnitude > 0.1f)
        {
            Debug.Log($"[DroneController] Input - Left: {left}, Right: {right}");
            Debug.Log($"[DroneController] Moving object: {gameObject.name} at position: {transform.position}");
        }

        // Rotate around Y (left joystick horizontal)
        transform.Rotate(0f, left.x * rotateSpeed * Time.deltaTime, 0f);

        // Change height (invert Y for correct direction) with height limits
        Vector3 newPosition = transform.position;
        newPosition += Vector3.up * left.y * heightSpeed * Time.deltaTime;
        
        // Enforce height limits
        newPosition.y = Mathf.Clamp(newPosition.y, minHeight, maxHeight);

        // Move on XZ plane only (project movement to avoid Y changes)
        Vector3 forward = new Vector3(transform.forward.x, 0, transform.forward.z).normalized * right.y * moveSpeed * Time.deltaTime;
        Vector3 strafe = new Vector3(transform.right.x, 0, transform.right.z).normalized * right.x * moveSpeed * Time.deltaTime;
        Vector3 horizontalMovement = forward + strafe;
        Vector3 currentPosition = transform.position;
        Vector3 desiredPosition = newPosition + horizontalMovement;
        
        // Check for collisions if collision detection is enabled
        bool shouldCheckCollision = enableCollisionDetection && horizontalMovement.magnitude > 0.001f;
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
            // Check collision for horizontal movement (XZ plane)
            Vector3 adjustedHorizontalPos = CheckCollisionAndAdjustPosition(currentPosition, desiredPosition);
            // Preserve Y position from height adjustment
            newPosition = new Vector3(adjustedHorizontalPos.x, newPosition.y, adjustedHorizontalPos.z);
        }
        else
        {
            newPosition = desiredPosition;
        }
        
        // Clamp position within boundary if boundary controller is available
        if (boundaryController != null && boundaryController.exteriorBoundary != null)
        {
            newPosition = boundaryController.ClampToExteriorBoundary(newPosition);
        }
        
        transform.position = newPosition;
        
        // Constrain rotations: Z always 0, X stays at 30 degrees
        Vector3 currentRotation = transform.eulerAngles;
        transform.rotation = Quaternion.Euler(30f, currentRotation.y, 0f);
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
                Debug.Log($"[DroneController] Ensured {colliders.Length} exterior colliders are enabled");
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
                    Debug.Log($"[DroneController] Auto-found and enabled {colliders.Length} exterior colliders from {buildingsObj.name}");
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
        
        // First, check if the desired position itself is inside a collider
        Collider[] overlappingAtDestination = Physics.OverlapSphere(desiredPos, collisionRadius, collisionLayers, QueryTriggerInteraction.Ignore);
        
        if (overlappingAtDestination.Length > 0)
        {
            // We're trying to move into a collider - don't allow it
            if (debugCollisions)
            {
                Debug.Log($"[DroneController] Destination position overlaps with collider(s): {string.Join(", ", System.Array.ConvertAll(overlappingAtDestination, c => c.name))}");
            }
            // Try to find a safe position by moving back along the direction
            return FindSafePosition(currentPos, desiredPos, direction, distance);
        }
        
        // Use a sphere cast to check for collisions along the path
        RaycastHit hit;
        bool hasCollision = Physics.SphereCast(currentPos, collisionRadius, direction, out hit, distance + 0.1f, collisionLayers, QueryTriggerInteraction.Ignore);
        
        if (debugCollisions)
        {
            if (hasCollision)
            {
                Debug.Log($"[DroneController] Collision detected with {hit.collider.name} (Layer: {hit.collider.gameObject.layer}, IsTrigger: {hit.collider.isTrigger}, Enabled: {hit.collider.enabled}) at distance {hit.distance}, normal: {hit.normal}");
            }
            else
            {
                // Debug: Check if there are any colliders in the scene
                Collider[] nearbyColliders = Physics.OverlapSphere(currentPos, collisionRadius + distance, collisionLayers, QueryTriggerInteraction.Ignore);
                if (nearbyColliders.Length > 0)
                {
                    Debug.Log($"[DroneController] No collision detected, but found {nearbyColliders.Length} nearby colliders: {string.Join(", ", System.Array.ConvertAll(nearbyColliders, c => $"{c.name} (Layer:{c.gameObject.layer}, Trigger:{c.isTrigger}, Enabled:{c.enabled})"))}");
                }
                else
                {
                    // Check all colliders regardless of layer to see what's available
                    Collider[] allNearbyColliders = Physics.OverlapSphere(currentPos, collisionRadius + distance, -1, QueryTriggerInteraction.Ignore);
                    if (allNearbyColliders.Length > 0)
                    {
                        Debug.LogWarning($"[DroneController] Found {allNearbyColliders.Length} colliders nearby, but NONE match collision layers mask! Collision Layers: {collisionLayers.value}. Colliders: {string.Join(", ", System.Array.ConvertAll(allNearbyColliders, c => $"{c.name} (Layer:{c.gameObject.layer})"))}");
                    }
                    else
                    {
                        Debug.LogWarning($"[DroneController] No colliders found nearby at all! Position: {currentPos}, Search radius: {collisionRadius + distance}");
                    }
                }
            }
        }
        
        if (hasCollision)
        {
            // There's a collision - adjust position to stop just before the wall
            float safeDistance = Mathf.Max(0f, hit.distance - collisionRadius);
            Vector3 adjustedPosition = currentPos + direction * safeDistance;
            
            // Try to slide along the wall if possible
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
                        // Can slide - move along the wall
                        adjustedPosition += slideDirection * Mathf.Min(remainingDistance, remainingMovement.magnitude);
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
        // Try multiple positions along the path, getting closer to current position
        int steps = 10;
        for (int i = steps; i > 0; i--)
        {
            float t = (float)i / steps;
            Vector3 testPos = Vector3.Lerp(currentPos, desiredPos, t);
            
            Collider[] overlapping = Physics.OverlapSphere(testPos, collisionRadius, collisionLayers, QueryTriggerInteraction.Ignore);
            if (overlapping.Length == 0)
            {
                // Found a safe position
                if (debugCollisions)
                {
                    Debug.Log($"[DroneController] Found safe position at {testPos} (step {i}/{steps})");
                }
                return testPos;
            }
        }
        
        // Couldn't find a safe position - stay where we are
        if (debugCollisions)
        {
            Debug.Log($"[DroneController] Could not find safe position, staying at current position");
        }
        return currentPos;
    }
}
