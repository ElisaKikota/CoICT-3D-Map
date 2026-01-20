# Dynamic Sidebar Setup Guide

## Overview
This guide explains how to set up the dynamic sidebar system that generates UI from CampusDataManager data.

## Components Created

1. **LocationData.cs** - Extended BuildingData with hierarchy support
2. **CampusDataManager.cs** - Central data repository for all campus locations
3. **DynamicSidebarGenerator.cs** - Generates sidebar UI dynamically
4. **SidebarLocationHandler.cs** - Handles location selection and coordinates with BottomSheet/Camera

## Setup Steps

### Step 1: Create CampusDataManager GameObject

1. Create an empty GameObject in your scene
2. Name it "CampusDataManager"
3. Add the `CampusDataManager` component
4. The component will auto-populate with all locations on Start
5. (Optional) You can edit locations in the Inspector if needed

### Step 2: Set Up Sidebar Structure in Unity

1. Find your **Sidebar** GameObject (the one that slides in from left)
2. Inside Sidebar, ensure you have:
   - A **ScrollView** or **ScrollRect** component
   - A **Content** GameObject (child of ScrollView)
   - This Content GameObject will be the parent for generated items

### Step 3: Create Sidebar Generator

1. Create an empty GameObject as child of Sidebar (or wherever appropriate)
2. Name it "SidebarGenerator"
3. Add the `DynamicSidebarGenerator` component
4. In Inspector, assign:
   - **Campus Data Manager**: Drag CampusDataManager GameObject
   - **Sidebar Content Parent**: Drag the Content GameObject from Step 2
   - **Section Header Prefab**: (Optional) Create a prefab for section headers
   - **Location Button Prefab**: (Optional) Create a prefab for location buttons
   - **Nested Location Button Prefab**: (Optional) Create a prefab for nested items

### Step 4: Create Sidebar Location Handler

1. Create an empty GameObject (can be same as SidebarGenerator)
2. Add the `SidebarLocationHandler` component
3. In Inspector, assign:
   - **Bottom Sheet Manager**: Your existing BottomSheetManager
   - **Camera Movement Controller**: Your existing CameraMovementController
   - **Camera Mode Controller**: Your existing CameraModeController
   - **Sidebar Generator**: The DynamicSidebarGenerator from Step 3

### Step 5: Connect Generator to Handler

1. Select the GameObject with `DynamicSidebarGenerator`
2. In Inspector, find the **Location Handler** field
3. Drag the `SidebarLocationHandler` component into it

### Step 6: (Optional) Create Prefabs for Better UI

#### Section Header Prefab:
1. Create UI > Button
2. Add TextMeshPro - Text (UI) as child
3. Style as needed (header appearance)
4. Save as prefab: "SidebarSectionHeaderPrefab"

#### Location Button Prefab:
1. Create UI > Button
2. Add TextMeshPro - Text (UI) as child
3. Style as needed
4. Save as prefab: "SidebarLocationButtonPrefab"

#### Nested Location Button Prefab:
1. Same as Location Button, but with left padding/indentation
2. Save as prefab: "SidebarNestedLocationButtonPrefab"

**Note:** If you don't create prefabs, the system will create default buttons automatically.

## How It Works

1. **On Start**: `DynamicSidebarGenerator` reads data from `CampusDataManager`
2. **Generation**: Creates UI elements for each category and location
3. **User Clicks Location**: `SidebarLocationHandler.OnLocationSelected()` is called
4. **Actions**:
   - Sidebar closes (if enabled)
   - Switches to **Drone mode** (as requested - stays in 3D)
   - Moves camera to location's `bestViewPosition` (if location has 3D position)
   - Shows BottomSheet with location details

## Customization

### Adding/Editing Locations

**Option 1: Edit in Inspector**
- Select CampusDataManager GameObject
- Expand "All Locations" list
- Add/remove/edit locations directly

**Option 2: Edit in Code**
- Open `CampusDataManager.cs`
- Modify the `Initialize...()` methods
- Add your locations with proper categories and positions

### Changing Default Behavior

Edit `SidebarLocationHandler.cs`:
- `switchToDroneMode`: Set to false if you want different behavior
- `closeSidebarOnSelect`: Set to false if you want sidebar to stay open

### Styling

Edit `DynamicSidebarGenerator.cs`:
- `sectionHeaderColor`: Color of category headers
- `locationButtonColor`: Color of location buttons
- `itemSpacing`: Spacing between items

## Testing

1. Enter Play mode
2. Click the building icon button to open sidebar
3. Verify all categories and locations appear
4. Click a location (e.g., "Block A")
5. Verify:
   - Sidebar closes
   - Camera switches to Drone mode
   - Camera moves to Block A's position
   - BottomSheet shows Block A details

## Troubleshooting

- **Sidebar is empty**: Check that CampusDataManager is assigned and has data
- **Buttons don't respond**: Check that SidebarLocationHandler is assigned to generator
- **Camera doesn't move**: Check that location has `bestViewPosition` set (not Vector3.zero)
- **BottomSheet doesn't show**: Check that BottomSheetManager is assigned to handler

## Next Steps

1. Set proper 3D positions for buildings in CampusDataManager
2. Customize BottomSheet content for different location types
3. Add icons/images for locations
4. Enhance nested location display (Principal's Office → Sub-offices)
























