using UnityEngine;

[System.Serializable]
public class BuildingData
{
    public string name;
    public string description;
    public Vector3 position;
    public Vector3 cameraViewPosition; // Good viewing angle for camera
    
    [Header("Best Viewing Angle")]
    [Tooltip("Best position for camera to view this building (for drone mode)")]
    public Vector3 bestViewPosition;
    
    [Tooltip("Best rotation for camera to view this building (for drone mode)")]
    public Vector3 bestViewRotation;
    
    [Header("Door References")]
    [Tooltip("Door objects that should glow when building is highlighted")]
    public GameObject[] doorObjects;
    
    public GameObject buildingObject;
    public bool isHighlighted;
    
    public BuildingData(string buildingName, string buildingDescription, Vector3 buildingPosition, Vector3 viewPosition, GameObject buildingObj)
    {
        name = buildingName;
        description = buildingDescription;
        position = buildingPosition;
        cameraViewPosition = viewPosition;
        buildingObject = buildingObj;
        isHighlighted = false;
        
        // Default best view position to camera view position
        bestViewPosition = viewPosition;
        bestViewRotation = Vector3.zero;
    }
    
    public BuildingData(string buildingName, string buildingDescription, Vector3 buildingPosition, Vector3 viewPosition, Vector3 bestViewPos, Vector3 bestViewRot, GameObject buildingObj)
    {
        name = buildingName;
        description = buildingDescription;
        position = buildingPosition;
        cameraViewPosition = viewPosition;
        bestViewPosition = bestViewPos;
        bestViewRotation = bestViewRot;
        buildingObject = buildingObj;
        isHighlighted = false;
    }
}








