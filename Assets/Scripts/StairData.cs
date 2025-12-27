using UnityEngine;

/// <summary>
/// Data structure for stairs connecting floors
/// </summary>
[System.Serializable]
public class StairData
{
    [Header("Stair Information")]
    [Tooltip("Name of the stair (for debugging/identification)")]
    public string stairName;
    
    [Header("Stair Object")]
    [Tooltip("The GameObject representing this stair (for raycast detection)")]
    public GameObject stairObject;
    
    [Tooltip("Collider on the stair object (for raycast detection)")]
    public Collider stairCollider;
    
    [Header("Floor Information")]
    [Tooltip("Floor name/number to display (e.g., 'Ground Floor', '1st Floor', '2nd Floor')")]
    public string floorName = "Ground Floor";
    
    [Header("Stair Type")]
    [Tooltip("Type of stair - determines which buttons are shown")]
    public StairType stairType = StairType.MiddleFloor;
    
    [Header("Linked Stairs")]
    [Tooltip("Name of the stair above this one (for going up) - Set this to link stairs by name")]
    public string upStairName;
    
    [Tooltip("Name of the stair below this one (for going down) - Set this to link stairs by name")]
    public string downStairName;
    
    [Tooltip("The stair above this one (auto-linked by name at runtime)")]
    [System.NonSerialized]
    public StairData upStair;
    
    [Tooltip("The stair below this one (auto-linked by name at runtime)")]
    [System.NonSerialized]
    public StairData downStair;
    
    [Header("Camera Positions - Going Up")]
    [Tooltip("Position where camera should appear when going up from this stair")]
    public Vector3 upPosition;
    
    [Tooltip("Rotation camera should have when going up from this stair")]
    public Vector3 upRotation;
    
    [Header("Camera Positions - Going Down")]
    [Tooltip("Position where camera should appear when going down from this stair")]
    public Vector3 downPosition;
    
    [Tooltip("Rotation camera should have when going down from this stair")]
    public Vector3 downRotation;
    
    /// <summary>
    /// Gets the world position of the stair (center of collider or transform)
    /// </summary>
    public Vector3 GetStairPosition()
    {
        if (stairCollider != null)
        {
            return stairCollider.bounds.center;
        }
        else if (stairObject != null)
        {
            return stairObject.transform.position;
        }
        return Vector3.zero;
    }
    
    /// <summary>
    /// Checks if this stair can go up
    /// </summary>
    public bool CanGoUp()
    {
        // Can go up if we have a linked stair above
        // Position can be determined from the linked stair if not explicitly set
        return upStair != null;
    }
    
    /// <summary>
    /// Checks if this stair can go down
    /// </summary>
    public bool CanGoDown()
    {
        // Can go down if we have a linked stair below
        // Position can be determined from the linked stair if not explicitly set
        return downStair != null;
    }
    
    /// <summary>
    /// Gets the position where camera should be when going up
    /// Uses upPosition if set, otherwise uses the downPosition of the upStair
    /// </summary>
    public Vector3 GetUpPosition()
    {
        if (upPosition != Vector3.zero)
        {
            return upPosition;
        }
        else if (upStair != null && upStair.downPosition != Vector3.zero)
        {
            return upStair.downPosition;
        }
        else if (upStair != null)
        {
            return upStair.GetStairPosition();
        }
        return Vector3.zero;
    }
    
    /// <summary>
    /// Gets the rotation where camera should be when going up
    /// Uses upRotation if set, otherwise uses the downRotation of the upStair
    /// </summary>
    public Vector3 GetUpRotation()
    {
        if (upRotation != Vector3.zero)
        {
            return upRotation;
        }
        else if (upStair != null && upStair.downRotation != Vector3.zero)
        {
            return upStair.downRotation;
        }
        return Vector3.zero;
    }
    
    /// <summary>
    /// Gets the position where camera should be when going down
    /// Uses downPosition if set, otherwise uses the upPosition of the downStair
    /// </summary>
    public Vector3 GetDownPosition()
    {
        if (downPosition != Vector3.zero)
        {
            return downPosition;
        }
        else if (downStair != null && downStair.upPosition != Vector3.zero)
        {
            return downStair.upPosition;
        }
        else if (downStair != null)
        {
            return downStair.GetStairPosition();
        }
        return Vector3.zero;
    }
    
    /// <summary>
    /// Gets the rotation where camera should be when going down
    /// Uses downRotation if set, otherwise uses the upRotation of the downStair
    /// </summary>
    public Vector3 GetDownRotation()
    {
        if (downRotation != Vector3.zero)
        {
            return downRotation;
        }
        else if (downStair != null && downStair.upRotation != Vector3.zero)
        {
            return downStair.upRotation;
        }
        return Vector3.zero;
    }
}

/// <summary>
/// Type of stair - determines which navigation buttons are shown
/// </summary>
public enum StairType
{
    GroundFloor,  // Only shows up arrow
    MiddleFloor,  // Shows both up and down arrows
    UpperFloor    // Only shows down arrow
}


