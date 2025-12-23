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
        
        // Check exterior boundary if in exterior mode
        if (!interiorExteriorManager.IsInteriorMode() && enforceExteriorBoundary)
        {
            isValidPosition = IsWithinExteriorBoundary(currentPos);
        }
        
        // Check interior boundary if in interior mode
        if (interiorExteriorManager.IsInteriorMode() && enforceInteriorBoundary)
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
        if (interiorExteriorManager.IsInteriorMode())
        {
            return ClampToInteriorBoundary(position);
        }
        else
        {
            return ClampToExteriorBoundary(position);
        }
    }
    
    Vector3 ClampToExteriorBoundary(Vector3 position)
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
        if (room.boundaryColliders == null || room.boundaryColliders.Length == 0)
        {
            return position;
        }
        
        // Use the first boundary collider to clamp position
        Collider firstCollider = room.boundaryColliders[0];
        if (firstCollider != null && firstCollider.enabled)
        {
            if (firstCollider is BoxCollider)
            {
                Bounds bounds = firstCollider.bounds;
                bounds.Expand(-boundaryPadding * 2);
                return new Vector3(
                    Mathf.Clamp(position.x, bounds.min.x, bounds.max.x),
                    Mathf.Clamp(position.y, bounds.min.y, bounds.max.y),
                    Mathf.Clamp(position.z, bounds.min.z, bounds.max.z)
                );
            }
            else
            {
                // For other collider types, move towards closest point
                Vector3 closestPoint = firstCollider.ClosestPoint(position);
                Vector3 direction = (closestPoint - position).normalized;
                return closestPoint - direction * boundaryPadding;
            }
        }
        
        return position;
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

