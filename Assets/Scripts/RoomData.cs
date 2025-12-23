using UnityEngine;

/// <summary>
/// Data structure for room information
/// </summary>
[System.Serializable]
public class RoomData
{
    [Header("Room Info")]
    public string roomName;
    public string roomDescription;
    
    [Header("Entry Point")]
    [Tooltip("Position where camera should be when entering room")]
    public Vector3 entryPosition;
    
    [Tooltip("Rotation camera should have when entering room")]
    public Quaternion entryRotation;
    
    [Header("Boundaries")]
    [Tooltip("Colliders that define room boundaries (walls, etc.)")]
    public Collider[] boundaryColliders;
    
    [Header("Lighting")]
    [Tooltip("Lights that should be enabled/strengthened in this room")]
    public Light[] roomLights;
    
    [Header("Room Objects")]
    [Tooltip("GameObjects that should be visible only in this room")]
    public GameObject[] roomObjects;
    
    public RoomData(string name, Vector3 entryPos, Quaternion entryRot)
    {
        roomName = name;
        entryPosition = entryPos;
        entryRotation = entryRot;
    }
}

