using UnityEngine;

/// <summary>
/// Defines an entry point to a room
/// </summary>
[System.Serializable]
public class RoomEntryPoint
{
    [Tooltip("Collider trigger that detects entry")]
    public Collider triggerCollider;
    
    [Tooltip("Room data for this entry point")]
    public RoomData roomData;
    
    [Tooltip("Optional: Specific door object to highlight")]
    public GameObject doorObject;
}

