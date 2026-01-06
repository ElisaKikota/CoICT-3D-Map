# Interior/Exterior System Setup Guide

This guide explains how to set up the interior/exterior entry system with doors and raycast detection.

## Overview

The system allows users to:
1. Select a building from the sidebar
2. See building details in bottom sheet with "Enter Inside" button
3. Switch to 2D mode and move to door position
4. Use raycast to detect doors and show "Enter" button
5. Enter interior mode and walk inside buildings
6. Navigate between rooms using interior doors

## Components

### 1. DoorData
- Defines door objects, their positions, and target interiors
- Located in `Assets/Scripts/DoorData.cs`

### 2. DoorRaycastSystem
- Handles raycast detection for doors
- Shows UI popup when door is detected
- Located in `Assets/Scripts/DoorRaycastSystem.cs`

### 3. BuildingEntrySystem
- Manages building entry flow: bottom sheet -> 2D mode -> door position -> interior
- Located in `Assets/Scripts/BuildingEntrySystem.cs`

### 4. InteriorExteriorManager (Existing)
- Manages interior/exterior mode switching
- Handles fade transitions and room boundaries

## Setup Steps
✅✅✅✅
### Step 1: Create Door GameObjects

1. For each building entry door, create a GameObject (e.g., "BlockA_MainDoor")
2. Position it at the door location
3. Add a **Collider** component (BoxCollider recommended for simplicity):
   - Make sure it's sized appropriately to cover the door area
   - The collider will be used for raycast detection

### Step 2: Create RoomData
❌❌❌❌
1. For each interior space, create a ScriptableObject or assign RoomData in a manager:
   - Set `roomName` and `roomDescription`
   - Set `entryPosition` (where camera spawns when entering)
   - Set `entryRotation` (camera rotation when entering)
❌❌❌❌
2. **For Boundaries:**
   - **Option A - Multiple BoxColliders (Recommended for L-shapes):**
     - Create multiple BoxCollider GameObjects as children of room boundary parent
     - Assign all colliders to `boundaryColliders` array in RoomData
     - Supports complex shapes like L-shaped corridors
   
   - **Option B - MeshColliders:**
     - Set `useMeshColliders = true`
     - Assign the parent GameObject containing room geometry to `roomMeshParent`
     - System will use all MeshColliders in children
     - Good for complex room shapes that match the geometry exactly
❌❌❌❌
3. **For Interior Doors:**
   - Add DoorData entries to `interiorDoors` array for doors inside this room
   - These allow navigation to other rooms or exits

### Step 3: Create DoorData Entries

1. In your scene manager or DoorRaycastSystem:
   - Create DoorData instances for each exterior door
   - Assign the door GameObject and Collider
   - Set `doorViewPosition` (where camera should be in 2D mode to view the door)
   - Set `doorViewRotation` (camera rotation, typically 90° for top-down 2D view)
   - Assign the `targetInterior` RoomData

### Step 4: Setup BuildingEntrySystem

1. Create a GameObject with `BuildingEntrySystem` component
2. Assign references:
   - `bottomSheetManager`
   - `cameraModeController`
   - `interiorManager`
   - `mainCamera`

3. **Create Building-Door Mappings:**
   - In the `buildingDoorMappings` array, add entries:
     - `buildingName`: Must match BuildingData.name (e.g., "Block_A")
     - `entryDoor`: The DoorData for this building's entry door

### Step 5: Setup DoorRaycastSystem

1. Create a GameObject with `DoorRaycastSystem` component
2. Assign references:
   - `mainCamera`
   - `uiCanvas`
   - `exteriorDoors`: Array of all exterior DoorData entries
   - `doorButtonPrefab`: UI prefab for door button (should have Button and TextMeshProUGUI)

3. **Create Door Button UI Prefab:**
   - Create a UI Button with TextMeshProUGUI child
   - Position will be set dynamically by the system
   - Assign this prefab to `doorButtonPrefab`

### Step 6: Update BottomSheetManager

1. Add an "Enter Inside" button to your bottom sheet UI
2. Assign it to `enterInsideButton` field in BottomSheetManager
3. The button will automatically show/hide based on whether building has interior

## Boundary Setup Options

### L-Shaped Corridors

For L-shaped corridors or complex room shapes:

1. **Multiple BoxColliders (Recommended):**
   - Create separate BoxColliders for each section of the L-shape
   - Position and rotate them to cover each corridor section
   - Add all to `boundaryColliders` array in RoomData
   - System will clamp position to be within at least one collider

2. **MeshColliders:**
   - Set `useMeshColliders = true`
   - Assign room geometry parent to `roomMeshParent`
   - System will use all MeshColliders found in children
   - More accurate but potentially more expensive

## Workflow Example

1. User selects "Block A" from sidebar
2. Bottom sheet shows with "Enter Inside" button
3. User clicks "Enter Inside"
4. Camera switches to 2D mode and moves to door view position
5. User can see the door and look at it with camera
6. DoorRaycastSystem detects door when camera looks at it
7. "Enter" button appears near door
8. User clicks "Enter" button
9. Camera fades and enters interior mode
10. User can now walk inside using WalkController
11. Inside, doors are detected via raycast showing labels/buttons
12. User can navigate to other rooms or exit

## Troubleshooting

**Doors not detected:**
- Check that door GameObject has a Collider component
- Verify collider is not set to "Is Trigger"
- Check raycast distance settings
- Ensure door is in correct layer (not blocked by blockingLayers)

**"Enter Inside" button not showing:**
- Verify building name in BuildingDoorMapping matches BuildingData.name exactly
- Check that door has a targetInterior assigned

**Camera doesn't move to door:**
- Verify doorViewPosition is set correctly
- Check BuildingEntrySystem has all required references

**Boundaries not working:**
- Ensure boundary colliders are assigned to RoomData
- Check that colliders are enabled when entering interior
- For mesh colliders, verify useMeshColliders is true and roomMeshParent is assigned





















