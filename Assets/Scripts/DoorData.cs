using UnityEngine;

/// <summary>
/// Simplified data structure for doors - works for both entry and exit
/// </summary>
[System.Serializable]
public class DoorData
{
    [Header("Door Information")]
    [Tooltip("Name of the door (displayed when raycast hits)")]
    public string doorName;
    
    [Header("Door Object")]
    [Tooltip("The GameObject representing this door (for raycast detection)")]
    public GameObject doorObject;
    
    [Tooltip("Collider on the door object (for raycast detection)")]
    public Collider doorCollider;
    
    [Header("Allow Entering")]
    [Tooltip("If checked, this door can be entered (entry/exit positions will be used). If unchecked, raycast will only show door name")]
    public bool allowEntering = false;
    
    [Header("Camera Positions - Entry (from exterior)")]
    [Tooltip("Position where camera should be when entering interior through this door")]
    public Vector3 entryPosition;
    
    [Tooltip("Rotation camera should have when entering interior through this door")]
    public Vector3 entryRotation;
    
    [Header("Camera Positions - Exit (back to exterior)")]
    [Tooltip("Position where camera should be when exiting interior back through this door")]
    public Vector3 exitPosition;
    
    [Tooltip("Rotation camera should have when exiting interior back through this door")]
    public Vector3 exitRotation;
    
    /// <summary>
    /// Gets the world position of the door (center of collider or transform)
    /// </summary>
    public Vector3 GetDoorPosition()
    {
        if (doorCollider != null)
        {
            return doorCollider.bounds.center;
        }
        else if (doorObject != null)
        {
            return doorObject.transform.position;
        }
        return Vector3.zero;
    }
}


