# Quick Door Setup Guide

This guide helps you quickly set up doors using your existing "Doors" parent GameObject structure.

## Your Current Setup

You have:
- ✅ **Doors** parent GameObject containing door GameObjects with BoxColliders
- ✅ **EnterExitButtonPanel** with Enter/Exit buttons that overlay
- ✅ Camera positions recorded for entry/exit at each door

## Setup Steps

### Step 1: Add DoorUIButtonPanel Component

1. Select your **EnterExitButtonPanel** GameObject in Canvas
2. Add `DoorUIButtonPanel` component to it
3. Assign references:
   - **Enter Button**: The Enter button in your panel
   - **Exit Button**: The Exit button in your panel
   - **Enter Button Text** (optional): TextMeshProUGUI component for Enter button text
   - **Exit Button Text** (optional): TextMeshProUGUI component for Exit button text

### Step 2: Setup DoorRaycastSystem

1. Create a GameObject (or use existing manager GameObject)
2. Add `DoorRaycastSystem` component
3. Assign references:
   - **Main Camera**: Your main camera
   - **UI Canvas**: Your Canvas GameObject
   - **Door Button Panel**: The EnterExitButtonPanel GameObject (with DoorUIButtonPanel component)
   - **Doors Parent**: Your "Doors" parent GameObject

4. The system will **auto-detect** all doors from the "Doors" parent on Start!

### Step 3: Configure Door Entry Positions

After doors are auto-detected, you need to manually set entry positions:

1. In **DoorRaycastSystem**, the `exteriorDoors` array will be auto-populated
2. For each door, set:
   - **Door View Position**: The camera position you recorded for viewing the door in 2D mode
   - **Door View Rotation**: The camera rotation (typically 90° for top-down 2D view)
   - **Target Interior**: Assign the RoomData this door leads to (if it has an interior)

### Step 4: Configure Building-Door Mapping (BuildingEntrySystem)

1. Find or create GameObject with `BuildingEntrySystem` component
2. Assign references:
   - **Bottom Sheet Manager**
   - **Camera Mode Controller**
   - **Interior Manager**
   - **Main Camera**

3. In **Building-Door Mappings**:
   - Add entry for each building (e.g., "Block_A", "Block_B", etc.)
   - Assign the corresponding DoorData from DoorRaycastSystem's exteriorDoors array

## How Enter/Exit Works

The raycast system automatically determines when to show Enter vs Exit:

- **In Exterior Mode:**
  - Shows "Enter" button when looking at doors with `targetInterior` assigned
  - Shows door name if door has `displayName` set

- **In Interior Mode:**
  - Shows "Exit" button when looking at doors with NO `targetInterior` (exit doors)
  - Shows door name when looking at doors that lead to other rooms/interiors

## Recording Door Positions

You mentioned you've recorded camera positions. To use them:

1. Select each door GameObject in "Doors" parent
2. Note the camera position/rotation you recorded for entry
3. In DoorRaycastSystem's `exteriorDoors` array, find the corresponding DoorData
4. Set `doorViewPosition` and `doorViewRotation` to your recorded values

## Next Steps

- Create RoomData for each interior space
- Assign RoomData to doors' `targetInterior` field
- Configure room boundaries (multiple BoxColliders for L-shapes, or mesh colliders)






