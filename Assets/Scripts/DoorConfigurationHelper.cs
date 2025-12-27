using UnityEngine;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Helper script to configure DoorData from door GameObjects in the scene
/// Attach this to a GameObject and use the button in inspector to auto-configure doors
/// </summary>
public class DoorConfigurationHelper : MonoBehaviour
{
    [Header("Door Parent")]
    [Tooltip("Parent GameObject containing all door GameObjects (e.g., 'Doors')")]
    public GameObject doorsParent;
    
    [Header("Generated Door Data")]
    [Tooltip("Auto-generated door data (read-only, for reference)")]
    public DoorData[] generatedDoors;
    
    [Header("Configuration")]
    [Tooltip("Default raycast distance for doors")]
    public float defaultRaycastDistance = 50f;
    
#if UNITY_EDITOR
    [ContextMenu("Auto-Configure Doors from Scene")]
    public void AutoConfigureDoors()
    {
        if (doorsParent == null)
        {
            GameObject doorsObj = GameObject.Find("Doors");
            if (doorsObj != null)
            {
                doorsParent = doorsObj;
            }
            else
            {
                Debug.LogError("[DoorConfigurationHelper] 'Doors' parent GameObject not found in scene!");
                return;
            }
        }
        
        List<DoorData> doors = new List<DoorData>();
        
        foreach (Transform child in doorsParent.transform)
        {
            Collider col = child.GetComponent<Collider>();
            if (col != null)
            {
                DoorData door = new DoorData();
                door.doorObject = child.gameObject;
                door.doorCollider = col;
                
                // Entry/exit positions and rotations need to be set manually after generation
                // (You can manually set these after generation)
                
                doors.Add(door);
                Debug.Log($"[DoorConfigurationHelper] Configured door: {child.name}");
            }
        }
        
        generatedDoors = doors.ToArray();
        Debug.Log($"[DoorConfigurationHelper] Successfully configured {generatedDoors.Length} doors!");
        
        // Mark scene as dirty so changes are saved
        EditorUtility.SetDirty(this);
    }
    
    string FormatDisplayName(string doorName)
    {
        // Convert "BlockA_MainDoor" -> "Block A Main Door"
        return doorName.Replace("_", " ").Replace("Block", "Block ");
    }
#endif
    
    /// <summary>
    /// Gets the generated door data (for use by DoorRaycastSystem)
    /// </summary>
    public DoorData[] GetGeneratedDoors()
    {
        return generatedDoors;
    }
}

