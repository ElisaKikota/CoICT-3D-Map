using UnityEngine;

/// <summary>
/// Handles location selection from sidebar and coordinates with BottomSheet and CameraMovement
/// This connects the dynamic sidebar to the existing systems
/// </summary>
public class SidebarLocationHandler : MonoBehaviour
{
    [Header("References")]
    public BottomSheetManager bottomSheetManager;
    public CameraMovementController cameraMovementController;
    public CameraModeController cameraModeController;
    public DynamicSidebarGenerator sidebarGenerator;
    
    [Header("Settings")]
    public bool switchToDroneMode = true; // Keep in Drone mode for best view (as requested)
    public bool closeSidebarOnSelect = true;
    
    /// <summary>
    /// Called when a location is selected from the sidebar
    /// </summary>
    public void OnLocationSelected(LocationData location)
    {
        if (location == null)
        {
            Debug.LogWarning("[SidebarLocationHandler] Location is null!");
            return;
        }
        
        Debug.Log($"[SidebarLocationHandler] Location selected: {location.name}");
        
        // Close sidebar if requested
        if (closeSidebarOnSelect && cameraModeController != null)
        {
            cameraModeController.CloseSidebarPublic();
        }
        
        // Switch to Drone mode for best viewing (as requested)
        if (switchToDroneMode && cameraModeController != null)
        {
            cameraModeController.SwitchMode(TourMode.Drone);
        }
        
        // If location has 3D position, move camera to it
        if (location.Has3DPosition() && cameraMovementController != null)
        {
            // Convert LocationData to BuildingData for camera movement
            BuildingData buildingData = ConvertToBuildingData(location);
            cameraMovementController.MoveToBuilding(buildingData, useDroneModePath: true);
        }
        
        // Show bottom sheet with location details
        if (bottomSheetManager != null)
        {
            // Convert LocationData to BuildingData for bottom sheet
            BuildingData buildingData = ConvertToBuildingData(location);
            bottomSheetManager.OnBuildingSelected(buildingData);
        }
        else
        {
            Debug.LogWarning("[SidebarLocationHandler] BottomSheetManager not assigned!");
        }
    }
    
    /// <summary>
    /// Convert LocationData to BuildingData for compatibility with existing systems
    /// </summary>
    BuildingData ConvertToBuildingData(LocationData location)
    {
        BuildingData buildingData = new BuildingData(
            location.name,
            location.detailedDescription ?? location.description,
            location.position,
            location.bestViewPosition != Vector3.zero ? location.bestViewPosition : location.cameraViewPosition,
            location.bestViewPosition,
            location.bestViewRotation,
            location.buildingObject
        );
        
        return buildingData;
    }
}

