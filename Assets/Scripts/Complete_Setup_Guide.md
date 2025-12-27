# Complete Setup Guide - New Features

This guide will walk you through setting up all the new features for your 3D CoICT Map Game.

## Table of Contents
1. [Importing Blender Assets](#1-importing-blender-assets)
2. [Interior/Exterior Mode Setup](#2-interiorexterior-mode-setup)
3. [Building Highlight & Camera Movement](#3-building-highlight--camera-movement)
4. [Boundary Colliders](#4-boundary-colliders)
5. [Responsive UI](#5-responsive-ui)
6. [Icons](#6-icons)

---

## 1. Importing Blender Assets

### Step 1: Export from Blender
1. Open your Blender file (`WholeCoICT - Anchoring.blend`)
2. Select all objects you want to export
3. **File → Export → FBX (.fbx)**
4. Settings:
   - ✅ Selected Objects
   - ✅ Apply Transform
   - Forward: -Z Forward
   - Up: Y Up
   - Scale: 1.0
   - ✅ Apply Modifiers

### Step 2: Import to Unity
1. Drag FBX file into `Assets/Models/` folder
2. Select the imported FBX in Project window
3. In Inspector:
   - **Model Tab**: Scale Factor = 1, ✅ Generate Colliders
   - **Materials Tab**: Material Creation Mode = Standard

### Step 3: Apply Materials to Grass/Ground
1. Select your imported model in the scene hierarchy
2. Add `MaterialAutoAssigner` component to the root GameObject
3. In Inspector:
   - Right-click component → **Create Procedural Grass Material**
   - Right-click component → **Create Procedural Ground Material**
   - Assign materials to the respective fields
   - Right-click component → **List All Meshes** (to see all meshes)
   - Right-click component → **Auto-Assign Materials** (to auto-assign based on names)
4. For unnamed meshes:
   - Use **Manual Assignment** list in Inspector
   - Drag mesh GameObjects and assign materials manually

---

## 2. Interior/Exterior Mode Setup

### Step 1: Create Fade Image
1. Create a new Canvas (if not exists): **GameObject → UI → Canvas**
2. Create full-screen Image: **GameObject → UI → Image** (child of Canvas)
3. Name it "FadeImage"
4. Set:
   - Anchor: Stretch-Stretch (all corners)
   - Color: Black (0, 0, 0, 0) - transparent initially
   - Order in Layer: 100 (on top)

### Step 2: Create Exit Button
1. Create Button: **GameObject → UI → Button** (child of Canvas)
2. Name it "ExitInteriorButton"
3. Position it (e.g., top-right corner)
4. Create container GameObject: **GameObject → UI → Panel**
5. Name it "ExitButtonContainer"
6. Make ExitInteriorButton a child of ExitButtonContainer
7. Set ExitButtonContainer inactive initially

### Step 3: Setup InteriorExteriorManager
1. Create empty GameObject: **GameObject → Create Empty**
2. Name it "InteriorExteriorManager"
3. Add `InteriorExteriorManager` component
4. In Inspector, assign:
   - **Main Camera**: Drag MainCamera
   - **Camera Mode Controller**: Drag CameraModeController GameObject
   - **Drone Controller**: Drag DroneController GameObject
   - **Walk Controller**: Drag WalkController GameObject
   - **Exit Interior Button**: Drag ExitInteriorButton
   - **Exit Button Container**: Drag ExitButtonContainer
   - **Fade Image**: Drag FadeImage Image component
   - **Fade Duration**: 1.0
   - **Fade Color**: Black

### Step 4: Setup Rooms
For each room in your building:

1. **Create Room Entry Point**:
   - Create empty GameObject at door location
   - Name it "Room1_Entry" (or similar)
   - Add Box Collider (or appropriate collider)
   - Set collider as **Trigger** (Is Trigger = true)
   - Size collider to cover doorway area

2. **Create Room Data**:
   - In InteriorExteriorManager Inspector, expand **Room Entry Points** array
   - Set array size to number of rooms
   - For each entry:
     - **Trigger Collider**: Drag the entry point collider
     - **Room Data**: Create new RoomData:
       - **Room Name**: "Room 1" (or descriptive name)
       - **Entry Position**: Position where camera should be when entering
       - **Entry Rotation**: Rotation camera should have
       - **Boundary Colliders**: Drag all wall colliders for this room
       - **Room Lights**: Drag lights that should be brighter in interior
       - **Room Objects**: Drag objects visible only in this room

3. **Setup Room Boundaries**:
   - For each room wall, ensure there's a Collider
   - Add these colliders to RoomData's **Boundary Colliders** array
   - These will prevent camera from going through walls

---

## 3. Building Highlight & Camera Movement

### Step 1: Setup Camera Movement Controller
1. Create empty GameObject: **GameObject → Create Empty**
2. Name it "CameraMovementController"
3. Add `CameraMovementController` component
4. Assign:
   - **Main Camera**: Drag MainCamera
   - **Camera Mode Controller**: Drag CameraModeController
   - **Intermediate Height**: 80 (height before horizontal movement)
   - **Vertical Speed**: 10
   - **Horizontal Speed**: 8

### Step 2: Update Building Data
1. Select SearchManager GameObject
2. For each building in **All Buildings** list:
   - **Best View Position**: Set optimal camera position for viewing this building (in drone mode)
   - **Best View Rotation**: Set optimal camera rotation (Euler angles)
   - **Door Objects**: Drag door GameObjects that should glow

### Step 3: Update BottomSheetManager
1. Select BottomSheetManager GameObject
2. In Inspector, assign:
   - **Camera Movement Controller**: Drag CameraMovementController GameObject
   - **Camera Mode Controller**: Drag CameraModeController GameObject

### Step 4: Setup Door Glow
For each door that should glow:

1. Select the door GameObject
2. Add `DoorGlowEffect` component
3. Configure:
   - **Glow Color**: Yellow/Orange (1, 0.8, 0)
   - **Glow Intensity**: 2
   - **Pulse Speed**: 2
   - **Door Renderer**: Auto-assigned (or drag manually)

**OR** let BuildingHighlightManager auto-detect doors:
- BuildingHighlightManager will automatically find objects with "door" in name
- It will add DoorGlowEffect component automatically

---

## 4. Boundary Colliders

### Step 1: Create Exterior Boundary
1. Create empty GameObject: **GameObject → Create Empty**
2. Name it "ExteriorBoundary"
3. Add Box Collider component
4. Position and scale to cover entire map area
5. Set **Is Trigger** = true

### Step 2: Setup Boundary Controller
1. Create empty GameObject: **GameObject → Create Empty**
2. Name it "BoundaryController"
3. Add `BoundaryController` component
4. Assign:
   - **Exterior Boundary**: Drag ExteriorBoundary BoxCollider
   - **Interior Exterior Manager**: Drag InteriorExteriorManager GameObject
   - **Main Camera**: Drag MainCamera
   - **Drone Controller**: Drag DroneController GameObject
   - **Walk Controller**: Drag WalkController GameObject
   - **Boundary Padding**: 1.0 (distance from boundary edge)

### Step 3: Setup Interior Boundaries
- Already done in Step 2.4 (Room Setup)
- Room boundary colliders are assigned in RoomData
- BoundaryController will automatically use them when in interior mode

---

## 5. Responsive UI

### Step 1: Setup Canvas Scaler
1. Select your Canvas GameObject
2. In Inspector, find **Canvas Scaler** component
3. Set:
   - **UI Scale Mode**: Scale With Screen Size
   - **Reference Resolution**: 1920 x 1080 (or your target resolution)
   - **Match**: 0.5 (or adjust based on preference)

### Step 2: Anchor UI Elements Properly
For each UI element:

1. Select the UI element
2. In RectTransform, set anchors:
   - **Buttons**: Anchor to corners (e.g., Top-Left, Top-Right)
   - **Panels**: Anchor to edges or center
   - **Text**: Anchor appropriately for content

3. Use **Anchors Presets** (hold Shift+Alt for position + size):
   - Top-left: Shift+Alt+Click top-left preset
   - Top-right: Shift+Alt+Click top-right preset
   - Bottom: Shift+Alt+Click bottom preset

### Step 3: Create ResponsiveUIHelper Script
A helper script will be created to adjust UI elements based on screen size.

---

## 6. Icons

### Step 1: Prepare Icon Images
1. Create/obtain icon images (PNG format, transparent background)
2. Recommended sizes:
   - Small icons: 64x64 or 128x128
   - Medium icons: 256x256
   - Large icons: 512x512
3. Import to `Assets/Images/Icons/` folder

### Step 2: Import Settings
1. Select each icon image
2. In Inspector:
   - **Texture Type**: Sprite (2D and UI)
   - **Pixels Per Unit**: 100
   - **Filter Mode**: Bilinear
   - **Compression**: None (for crisp icons)

### Step 3: Apply Icons to Buttons
1. Select button GameObject
2. In Inspector, find **Image** component
3. Drag icon sprite to **Source Image** field
4. Set **Image Type**: Simple
5. Adjust **Preserve Aspect** if needed

### Step 4: Update Existing Icons
- Replace old icon images with new ones
- Update button Image components
- Test on different screen sizes

---

## Testing Checklist

- [ ] Blender assets imported and materials applied
- [ ] Can switch between interior/exterior modes
- [ ] Fade transitions work smoothly
- [ ] Exit button appears in interior mode
- [ ] Camera position saved when entering room
- [ ] Building highlight works
- [ ] Camera smoothly moves to buildings (two-phase in drone mode)
- [ ] Doors glow when building is highlighted
- [ ] Exterior boundary prevents going outside map
- [ ] Interior boundaries prevent going through walls
- [ ] UI scales properly on different screen sizes
- [ ] Icons display correctly

---

## Troubleshooting

### Materials are pink/missing
- Check MaterialAutoAssigner has materials assigned
- Run "Auto-Assign Materials" again
- Check shader compatibility (use Standard shader)

### Camera doesn't move smoothly
- Check CameraMovementController is assigned
- Verify BuildingData has bestViewPosition set
- Check if in drone mode (two-phase movement)

### Doors don't glow
- Ensure DoorGlowEffect component is on door GameObject
- Check BuildingHighlightManager has enableDoorGlow = true
- Verify door name contains "door", "entrance", or "entry"

### Boundaries don't work
- Ensure colliders are set as triggers (exterior) or solid (interior walls)
- Check BoundaryController has correct references
- Verify boundary padding is appropriate

### UI not responsive
- Check Canvas Scaler settings
- Verify anchors are set correctly
- Test on different aspect ratios

---

## Next Steps

1. Test all features thoroughly
2. Adjust values in Inspector for your specific needs
3. Fine-tune animation curves and speeds
4. Add more rooms as needed
5. Customize materials and colors
6. Optimize for performance if needed

Good luck with your project! 🚀



