using UnityEngine;

[System.Serializable]
public class BuildingData
{
    public string name;
    public string description;
    public Vector3 position;
    public Vector3 cameraViewPosition; // Good viewing angle for camera
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
    }
}





