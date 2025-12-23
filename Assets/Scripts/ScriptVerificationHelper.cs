using UnityEngine;

/// <summary>
/// Helper script to verify all required scripts are compiled and available.
/// Add this to any GameObject to check script availability.
/// </summary>
public class ScriptVerificationHelper : MonoBehaviour
{
    [ContextMenu("Verify All Scripts")]
    public void VerifyAllScripts()
    {
        Debug.Log("=== Script Verification ===");
        
        // Check core scripts
        CheckScript<MaterialAutoAssigner>("MaterialAutoAssigner");
        CheckScript<InteriorExteriorManager>("InteriorExteriorManager");
        CheckScript<RoomData>("RoomData");
        CheckScript<RoomEntryPoint>("RoomEntryPoint");
        CheckScript<RoomTrigger>("RoomTrigger");
        
        // Check camera scripts
        CheckScript<CameraMovementController>("CameraMovementController");
        CheckScript<DoorGlowEffect>("DoorGlowEffect");
        CheckScript<BoundaryController>("BoundaryController");
        CheckScript<BoundaryTrigger>("BoundaryTrigger");
        
        // Check UI scripts
        CheckScript<ResponsiveUIHelper>("ResponsiveUIHelper");
        
        // Check existing scripts
        CheckScript<CameraModeController>("CameraModeController");
        CheckScript<DroneController>("DroneController");
        CheckScript<WalkController>("WalkController");
        CheckScript<BuildingHighlightManager>("BuildingHighlightManager");
        CheckScript<BottomSheetManager>("BottomSheetManager");
        CheckScript<SearchManager>("SearchManager");
        
        Debug.Log("=== Verification Complete ===");
    }
    
    void CheckScript<T>(string scriptName) where T : class
    {
        try
        {
            // Try to find the type
            System.Type type = typeof(T);
            if (type != null)
            {
                Debug.Log($"✅ {scriptName} - Available (Type: {type.Name})");
            }
            else
            {
                Debug.LogError($"❌ {scriptName} - NOT FOUND");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"❌ {scriptName} - ERROR: {e.Message}");
        }
    }
    
    [ContextMenu("Check RoomData and RoomEntryPoint")]
    public void CheckDataClasses()
    {
        Debug.Log("=== Checking Data Classes ===");
        
        try
        {
            System.Type roomDataType = typeof(RoomData);
            Debug.Log($"✅ RoomData - Available (Type: {roomDataType.Name})");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"❌ RoomData - ERROR: {e.Message}");
        }
        
        try
        {
            System.Type roomEntryPointType = typeof(RoomEntryPoint);
            Debug.Log($"✅ RoomEntryPoint - Available (Type: {roomEntryPointType.Name})");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"❌ RoomEntryPoint - ERROR: {e.Message}");
        }
        
        Debug.Log("=== Data Classes Check Complete ===");
    }
    
    void Start()
    {
        // Auto-verify on start
        VerifyAllScripts();
    }
}

