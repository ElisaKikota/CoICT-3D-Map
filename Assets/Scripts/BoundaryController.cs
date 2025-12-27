using UnityEngine;

/// <summary>
/// Controls movement boundaries for exterior and interior modes.
/// Limits camera/drone movement within defined areas.
/// </summary>
public class BoundaryController : MonoBehaviour
{
    [Header("Exterior Boundaries")]
    [Tooltip("Box collider that defines the exterior map boundary")]
    public BoxCollider exteriorBoundary;
    
    [Tooltip("Enable exterior boundary enforcement")]
    public bool enforceExteriorBoundary = true;
    
    [Header("Interior Boundaries")]
    [Tooltip("Enable interior boundary enforcement (room walls)")]
    public bool enforceInteriorBoundary = true;
    
    [Header("References")]
    public InteriorExteriorManager interiorExteriorManager;
    public Camera mainCamera;
    public DroneController droneController;
    public WalkController walkController;
    
    [Header("Boundary Settings")]
    [Tooltip("Padding from boundary edges")]
    public float boundaryPadding = 1f;
    
    private Vector3 lastValidPosition;
    
    void Start()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }
        
        if (mainCamera != null)
        {
            lastValidPosition = mainCamera.transform.position;
        }
        
        // Setup exterior boundary collider
        if (exteriorBoundary != null)
        {
            exteriorBoundary.isTrigger = true;
            // Add a component to detect when camera leaves boundary
            BoundaryTrigger trigger = exteriorBoundary.GetComponent<BoundaryTrigger>();
            if (trigger == null)
            {
                trigger = exteriorBoundary.gameObject.AddComponent<BoundaryTrigger>();
            }
            trigger.boundaryController = this;
        }
    }
    
    void Update()
    {
        if (mainCamera == null) return;
        
        Vector3 currentPos = mainCamera.transform.position;
        bool isValidPosition = true;
        
        // Check exterior boundary if in exterior mode (or if interior manager is null)
        bool isInteriorMode = interiorExteriorManager != null && interiorExteriorManager.IsInteriorMode();
        
        if (!isInteriorMode && enforceExteriorBoundary)
        {
            isValidPosition = IsWithinExteriorBoundary(currentPos);
        }
        
        // Check interior boundary if in interior mode
        if (isInteriorMode && enforceInteriorBoundary)
        {
            isValidPosition = IsWithinInteriorBoundary(currentPos);
        }
        
        if (isValidPosition)
        {
            lastValidPosition = currentPos;
        }
        else
        {
            // Clamp position to valid area
            Vector3 clampedPos = ClampToBoundary(currentPos);
            mainCamera.transform.position = clampedPos;
            lastValidPosition = clampedPos;
        }
    }
    
    bool IsWithinExteriorBoundary(Vector3 position)
    {
        if (exteriorBoundary == null) return true;
        
        Bounds bounds = exteriorBoundary.bounds;
        bounds.Expand(-boundaryPadding * 2);
        
        return bounds.Contains(position);
    }
    
    bool IsWithinInteriorBoundary(Vector3 position)
    {
        if (interiorExteriorManager == null || interiorExteriorManager.currentRoom == null)
        {
            return true;
        }
        
        RoomData room = interiorExteriorManager.currentRoom;
        if (room.boundaryColliders == null || room.boundaryColliders.Length == 0)
        {
            return true;
        }
        
        // Check if position is within any of the room boundary colliders
        foreach (Collider col in room.boundaryColliders)
        {
            if (col != null && col.enabled)
            {
                // For box colliders, check bounds
                if (col is BoxCollider)
                {
                    Bounds bounds = col.bounds;
                    bounds.Expand(-boundaryPadding * 2);
                    if (!bounds.Contains(position))
                    {
                        return false;
                    }
                }
                // For other collider types, use ClosestPoint
                else
                {
                    Vector3 closestPoint = col.ClosestPoint(position);
                    if (Vector3.Distance(position, closestPoint) > boundaryPadding)
                    {
                        return false;
                    }
                }
            }
        }
        
        return true;
    }
    
    Vector3 ClampToBoundary(Vector3 position)
    {
        bool isInteriorMode = interiorExteriorManager != null && interiorExteriorManager.IsInteriorMode();
        
        if (isInteriorMode)
        {
            return ClampToInteriorBoundary(position);
        }
        else
        {
            return ClampToExteriorBoundary(position);
        }
    }
    
    public Vector3 ClampToExteriorBoundary(Vector3 position)
    {
        if (exteriorBoundary == null) return position;
        
        Bounds bounds = exteriorBoundary.bounds;
        bounds.Expand(-boundaryPadding * 2);
        
        return new Vector3(
            Mathf.Clamp(position.x, bounds.min.x, bounds.max.x),
            Mathf.Clamp(position.y, bounds.min.y, bounds.max.y),
            Mathf.Clamp(position.z, bounds.min.z, bounds.max.z)
        );
    }
    
    Vector3 ClampToInteriorBoundary(Vector3 position)
    {
        if (interiorExteriorManager == null || interiorExteriorManager.currentRoom == null)
        {
            return position;
        }
        
        RoomData room = interiorExteriorManager.currentRoom;
        
        // Check if using mesh colliders
        if (room.useMeshColliders && room.roomMeshParent != null)
        {
            // Use mesh colliders from room geometry
            Collider[] meshColliders = room.roomMeshParent.GetComponentsInChildren<Collider>();
            if (meshColliders != null && meshColliders.Length > 0)
            {
                return ClampToMultipleColliders(position, meshColliders);
            }
        }
        
        // Use boundary colliders (supports multiple for L-shapes, etc.)
        if (room.boundaryColliders != null && room.boundaryColliders.Length > 0)
        {
            return ClampToMultipleColliders(position, room.boundaryColliders);
        }
        
        return position;
    }
    
    /// <summary>
    /// Clamps position to be within at least one of the provided colliders (for L-shapes, etc.)
    /// </summary>
    Vector3 ClampToMultipleColliders(Vector3 position, Collider[] colliders)
    {
        if (colliders == null || colliders.Length == 0) return position;
        
        // Check if position is already within any collider
        foreach (Collider col in colliders)
        {
            if (col != null && col.enabled)
            {
                if (col.bounds.Contains(position))
                {
                    // Position is within bounds, but check if it's actually inside the collider geometry
                    Vector3 closestPoint = col.ClosestPoint(position);
                    if (Vector3.Distance(position, closestPoint) < 0.1f)
                    {
                        // Position is inside this collider - clamp within its bounds
                        if (col is BoxCollider boxCol)
                        {
                            Bounds bounds = boxCol.bounds;
                            bounds.Expand(-boundaryPadding * 2);
                            return new Vector3(
                                Mathf.Clamp(position.x, bounds.min.x, bounds.max.x),
                                Mathf.Clamp(position.y, bounds.min.y, bounds.max.y),
                                Mathf.Clamp(position.z, bounds.min.z, bounds.max.z)
                            );
                        }
                        else if (col is MeshCollider meshCol)
                        {
                            // For mesh colliders, use closest point with padding
                            closestPoint = meshCol.ClosestPoint(position);
                            Vector3 direction = (position - closestPoint).normalized;
                            return closestPoint + direction * boundaryPadding;
                        }
                    }
                }
            }
        }
        
        // Position is not inside any collider - find the closest valid point
        Vector3 bestPosition = position;
        float closestDistance = float.MaxValue;
        
        foreach (Collider col in colliders)
        {
            if (col != null && col.enabled)
            {
                Vector3 closestPoint = col.ClosestPoint(position);
                float distance = Vector3.Distance(position, closestPoint);
                
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    // Move slightly inside the collider
                    if (col is BoxCollider)
                    {
                        Bounds bounds = col.bounds;
                        bounds.Expand(-boundaryPadding * 2);
                        Vector3 clamped = new Vector3(
                            Mathf.Clamp(position.x, bounds.min.x, bounds.max.x),
                            Mathf.Clamp(position.y, bounds.min.y, bounds.max.y),
                            Mathf.Clamp(position.z, bounds.min.z, bounds.max.z)
                        );
                        // Use the point that's closest to original position but inside bounds
                        if (bounds.Contains(clamped))
                        {
                            bestPosition = clamped;
                        }
                    }
                    else
                    {
                        // For mesh colliders, use closest point with padding inward
                        Vector3 direction = (closestPoint - position).normalized;
                        if (direction == Vector3.zero) direction = Vector3.up;
                        bestPosition = closestPoint - direction * boundaryPadding;
                    }
                }
            }
        }
        
        return bestPosition;
    }
    
    /// <summary>
    /// Manually set the exterior boundary using a box collider
    /// </summary>
    public void SetExteriorBoundary(BoxCollider boundary)
    {
        exteriorBoundary = boundary;
        if (boundary != null)
        {
            boundary.isTrigger = true;
        }
    }
}



