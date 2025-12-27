using UnityEngine;
using System.Collections;

/// <summary>
/// Handles building entry flow: shows bottom sheet -> "Enter Inside" button -> 2D mode -> door position -> enter interior
/// </summary>
public class BuildingEntrySystem : MonoBehaviour
{
    [Header("References")]
    public BottomSheetManager bottomSheetManager;
    public CameraModeController cameraModeController;
    public InteriorExteriorManager interiorManager;
    public Camera mainCamera;
    
    [Header("Building to Door Mapping")]
    [Tooltip("Mapping of building names to their entry doors")]
    public BuildingDoorMapping[] buildingDoorMappings;
    
    [Header("Camera Movement")]
    [Tooltip("Speed for moving camera to door position in 2D mode")]
    public float doorMovementSpeed = 10f;
    
    private BuildingData currentBuilding;
    
    void Start()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }
        
        if (bottomSheetManager == null)
        {
            bottomSheetManager = FindFirstObjectByType<BottomSheetManager>();
        }
        
        if (cameraModeController == null)
        {
            cameraModeController = FindFirstObjectByType<CameraModeController>();
        }
        
        if (interiorManager == null)
        {
            interiorManager = FindFirstObjectByType<InteriorExteriorManager>();
        }
    }
    
    /// <summary>
    /// Called when "Enter Inside" button is pressed from bottom sheet
    /// </summary>
    public void OnEnterInsideButtonClicked(BuildingData building)
    {
        if (building == null)
        {
            Debug.LogWarning("[BuildingEntrySystem] Cannot enter building - building data is null");
            return;
        }
        
        currentBuilding = building;
        StartCoroutine(EnterBuildingCoroutine(building));
    }
    
    /// <summary>
    /// Coroutine that handles the building entry flow
    /// </summary>
    IEnumerator EnterBuildingCoroutine(BuildingData building)
    {
        Debug.Log($"[BuildingEntrySystem] Starting entry flow for building: {building.name}");
        
        // Close bottom sheet
        if (bottomSheetManager != null)
        {
            bottomSheetManager.CloseBottomSheet();
        }
        
        // Switch to Walk mode
        if (cameraModeController != null)
        {
            cameraModeController.SwitchMode(TourMode.Walk);
            yield return new WaitForSeconds(0.2f); // Wait for mode transition
        }
        
        // Move camera to door view position (using building's enter building camera position/rotation)
        Vector3 doorViewPos;
        Vector3 doorViewRot;
        
        if (building.enterBuildingCameraPosition != Vector3.zero)
        {
            // Use building's manually set door position from SidebarButtonHandler
            doorViewPos = building.enterBuildingCameraPosition;
            doorViewRot = building.enterBuildingCameraRotation;
            Debug.Log($"[BuildingEntrySystem] Using SidebarButtonHandler enter position: {doorViewPos}, rotation: {doorViewRot}");
        }
        else
        {
            // Fallback: Find the door for this building
            DoorData entryDoor = GetDoorForBuilding(building.name);
            if (entryDoor != null)
            {
                doorViewPos = entryDoor.entryPosition != Vector3.zero 
                    ? entryDoor.entryPosition 
                    : entryDoor.GetDoorPosition();
                doorViewRot = entryDoor.entryRotation != Vector3.zero
                    ? entryDoor.entryRotation
                    : Vector3.zero; // Default to 0 rotation for walk mode
                Debug.Log($"[BuildingEntrySystem] Using door position: {doorViewPos}, rotation: {doorViewRot}");
            }
            else
            {
                Debug.LogWarning($"[BuildingEntrySystem] No enter building position set and no door found for building: {building.name}");
                currentBuilding = null;
                yield break;
            }
        }
        
        // Smoothly interpolate camera to door position and rotation
        yield return StartCoroutine(MoveCameraToDoorPosition(doorViewPos, doorViewRot));
        
        // Don't enter interior automatically - let the user press Enter button from raycast
        // The raycast system will detect the door and show the Enter button
        
        currentBuilding = null;
    }
    
    /// <summary>
    /// Smoothly interpolates camera to door position in Walk mode
    /// </summary>
    IEnumerator MoveCameraToDoorPosition(Vector3 targetPosition, Vector3 targetRotation)
    {
        if (mainCamera == null)
        {
            Debug.LogWarning("[BuildingEntrySystem] Main camera is null, cannot move to door position");
            yield break;
        }
        
        // Disable controllers during movement to prevent interference
        if (cameraModeController != null)
        {
            if (cameraModeController.walkController != null)
            {
                cameraModeController.walkController.enabled = false;
            }
            if (cameraModeController.droneController != null)
            {
                cameraModeController.droneController.enabled = false;
            }
            if (cameraModeController.camera2DController != null)
            {
                cameraModeController.camera2DController.enabled = false;
            }
        }
        
        Vector3 startPos = mainCamera.transform.position;
        Quaternion startRot = mainCamera.transform.rotation;
        Quaternion targetRot = Quaternion.Euler(targetRotation);
        
        float distance = Vector3.Distance(startPos, targetPosition);
        float duration = distance / doorMovementSpeed;
        float elapsed = 0f;
        
        Debug.Log($"[BuildingEntrySystem] Moving camera from {startPos} to {targetPosition}, rotation from {startRot.eulerAngles} to {targetRotation}, duration: {duration}s");
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            t = Mathf.SmoothStep(0f, 1f, t); // Smooth ease in/out
            
            // Linearly interpolate position
            mainCamera.transform.position = Vector3.Lerp(startPos, targetPosition, t);
            
            // Spherically interpolate rotation for smooth rotation
            mainCamera.transform.rotation = Quaternion.Slerp(startRot, targetRot, t);
            
            yield return null;
        }
        
        // Ensure exact final position and rotation
        mainCamera.transform.position = targetPosition;
        mainCamera.transform.rotation = targetRot;
        
        Debug.Log($"[BuildingEntrySystem] Camera movement completed. Final position: {mainCamera.transform.position}, Final rotation: {mainCamera.transform.rotation.eulerAngles}");
        
        // Re-enable WalkController after movement completes
        if (cameraModeController != null && cameraModeController.walkController != null && cameraModeController.currentMode == TourMode.Walk)
        {
            cameraModeController.walkController.enabled = true;
            Debug.Log("[BuildingEntrySystem] Re-enabled WalkController after movement");
        }
    }
    
    /// <summary>
    /// Gets the door data for a building name
    /// </summary>
    DoorData GetDoorForBuilding(string buildingName)
    {
        foreach (var mapping in buildingDoorMappings)
        {
            if (mapping.buildingName == buildingName)
            {
                return mapping.entryDoor;
            }
        }
        
        Debug.LogWarning($"[BuildingEntrySystem] No door mapping found for building: {buildingName}");
        return null;
    }
    
    /// <summary>
    /// Checks if a building has an entry door (can be entered)
    /// </summary>
    public bool BuildingHasInterior(string buildingName)
    {
        DoorData door = GetDoorForBuilding(buildingName);
        // A door can be entered if it has entry position set (simplified system)
        return door != null && door.entryPosition != Vector3.zero;
    }
    
    /// <summary>
    /// Gets the building name for a given door (reverse lookup)
    /// </summary>
    public string GetBuildingNameForDoor(DoorData door)
    {
        if (door == null)
        {
            Debug.LogWarning("[BuildingEntrySystem] GetBuildingNameForDoor called with null door");
            return null;
        }
        
        if (buildingDoorMappings == null || buildingDoorMappings.Length == 0)
        {
            Debug.LogWarning("[BuildingEntrySystem] buildingDoorMappings is null or empty");
            return null;
        }
        
        // Try reference comparison first (fastest)
        foreach (var mapping in buildingDoorMappings)
        {
            if (mapping.entryDoor == door)
            {
                Debug.Log($"[BuildingEntrySystem] Found building name via reference: {mapping.buildingName} for door: {door.doorObject?.name ?? "NULL"}");
                return mapping.buildingName;
            }
        }
        
        // If reference comparison fails, try comparing by door object
        foreach (var mapping in buildingDoorMappings)
        {
            if (mapping.entryDoor != null && door.doorObject != null && mapping.entryDoor.doorObject == door.doorObject)
            {
                Debug.Log($"[BuildingEntrySystem] Found building name via door object: {mapping.buildingName} for door: {door.doorObject.name}");
                return mapping.buildingName;
            }
        }
        
        // If that fails, try comparing by door name
        if (!string.IsNullOrEmpty(door.doorName))
        {
            foreach (var mapping in buildingDoorMappings)
            {
                if (mapping.entryDoor != null && mapping.entryDoor.doorName == door.doorName)
                {
                    Debug.Log($"[BuildingEntrySystem] Found building name via door name: {mapping.buildingName} for door name: {door.doorName}");
                    return mapping.buildingName;
                }
            }
        }
        
        Debug.LogWarning($"[BuildingEntrySystem] No building mapping found for door: {(door.doorObject != null ? door.doorObject.name : "NULL")}, doorName: {door.doorName}. Total mappings: {buildingDoorMappings.Length}");
        return null;
    }
}

/// <summary>
/// Maps a building name to its entry door
/// </summary>
[System.Serializable]
public class BuildingDoorMapping
{
    [Tooltip("Name of the building (must match BuildingData.name)")]
    public string buildingName;
    
    [Tooltip("The door that serves as entry point for this building")]
    public DoorData entryDoor;
}

