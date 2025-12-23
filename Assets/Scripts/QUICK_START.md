# Quick Start Guide - New Features Implementation

## ⚠️ IMPORTANT: Before You Start

**If you get "Script Class Can't Be Found" errors:**

1. **Wait for Unity to finish compiling** - Check bottom-right corner for compilation status
2. **Refresh Assets** - Press `Ctrl+R` or go to **Assets → Refresh**
3. **Check Console** - Open **Window → General → Console** for any red errors
4. **See Troubleshooting Section** below for detailed fixes

All scripts must compile successfully before you can add them as components!

---

## Overview
This document provides a quick reference for implementing all the new features. For detailed instructions, see `Complete_Setup_Guide.md`.

## New Scripts Created

### Core Systems
1. **MaterialAutoAssigner.cs** - Auto-assigns materials to grass/ground meshes
2. **InteriorExteriorManager.cs** - Manages interior/exterior mode switching
3. **RoomData.cs** - Data structure for room information
4. **RoomEntryPoint.cs** - Defines room entry points
5. **RoomTrigger.cs** - Detects when entering a room

### Camera & Movement
6. **CameraMovementController.cs** - Smooth camera movement to buildings (two-phase in drone mode)
7. **DoorGlowEffect.cs** - Creates glowing effect on doors
8. **BoundaryController.cs** - Limits movement within boundaries
9. **BoundaryTrigger.cs** - Helper for boundary detection

### UI
10. **ResponsiveUIHelper.cs** - Makes UI responsive across screen sizes

## Quick Setup Steps

### 1. Import Blender Assets (5 minutes)

**Step-by-step process:**

1. **Export from Blender:**
   - Open your Blender file (`COICT Whole Map New - Baked.blend` or similar)
   - Select all objects you want to export (or press `A` to select all)
   - Go to **File → Export → FBX (.fbx)**
   - In export settings:
     - Check ✅ **Selected Objects** (if you selected specific objects)
     - Check ✅ **Apply Transform**
     - Set **Forward** to `-Z Forward`
     - Set **Up** to `Y Up`
     - Set **Scale** to `1.0`
     - Check ✅ **Apply Modifiers**
     - Set **Smoothing** to `Face` or `Edge`
   - Click **Export FBX** and save to a location you can find

2. **Import to Unity:**
   - In Unity, navigate to `Assets/Models/` folder in Project window
   - Drag your exported FBX file into this folder
   - Unity will automatically import it (watch the progress bar at bottom)
   - Select the imported FBX in Project window

3. **Configure Import Settings:**
   - In the Inspector, you'll see import settings:
     - **Model Tab:**
       - Set **Scale Factor** to `1` (or adjust if model is wrong size)
       - Check ✅ **Import Blender Materials** (if you have materials in Blender)
       - Check ✅ **Generate Colliders** (optional, we'll add custom ones)
     - **Materials Tab:**
       - Set **Material Creation Mode** to `Standard` or `Import Standard Materials`
       - Set **Location** to `Use External Materials (Legacy)` or `By Material Name`

4. **Add MaterialAutoAssigner:**
   - Drag your imported model from Project window into Scene hierarchy
   - Select the model GameObject in hierarchy
   - In Inspector, click **Add Component**
   - Search for `MaterialAutoAssigner` and add it
   - Right-click the component → **Create Procedural Grass Material**
   - Right-click the component → **Create Procedural Ground Material**
   - The materials will be created in `Assets/Materials/` folder

5. **Auto-Assign Materials:**
   - In MaterialAutoAssigner component:
     - Assign the created **Grass Material** to `Grass Material` field
     - Assign the created **Ground Material** to `Ground Material` field
     - Optionally create/assign a **Building Material** for buildings
   - Right-click component → **List All Meshes** (to see all meshes in console)
   - Right-click component → **Auto-Assign Materials** (automatically assigns based on mesh names)
   - **For unnamed meshes:** Use the **Manual Assignments** list:
     - Click `+` to add entries
     - Drag mesh GameObjects from hierarchy to `Mesh Object` field
     - Assign appropriate material to `Material` field

**Troubleshooting:**
- If materials are pink: Make sure materials are assigned and shaders are compatible
- If meshes aren't found: Expand the model in hierarchy to see child meshes
- If auto-assign doesn't work: Use manual assignment for each mesh

---

### 2. Setup Interior/Exterior Mode (10 minutes)

**This system allows players to enter buildings and explore interior rooms.**

**Step-by-step process:**

1. **Create Fade Image (for smooth transitions):**
   - In Hierarchy, right-click → **UI → Canvas** (if you don't have one)
   - Right-click Canvas → **UI → Image**
   - Name it `FadeImage`
   - Select FadeImage, in Inspector:
     - **RectTransform:** Click anchor preset (top-left corner) → Hold `Shift+Alt` → Click **Stretch-Stretch** (fills entire screen)
     - **Image Component:**
       - **Color:** Set to Black (R:0, G:0, B:0, A:0) - fully transparent
       - **Image Type:** Simple
   - In Canvas component, set **Sort Order** to `100` (ensures it's on top)

2. **Create Exit Button:**
   - Right-click Canvas → **UI → Panel**
   - Name it `ExitButtonContainer`
   - Right-click ExitButtonContainer → **UI → Button**
   - Name it `ExitInteriorButton`
   - Select ExitInteriorButton:
     - Position it (e.g., top-right corner)
     - In **RectTransform**, set anchors to top-right
     - Set **Text** child to say "Exit" or "← Exit"
   - Select ExitButtonContainer, set it to **Inactive** (uncheck checkbox at top)

3. **Add InteriorExteriorManager Component:**
   - Create empty GameObject: Right-click Hierarchy → **Create Empty**
   - Name it `InteriorExteriorManager`
   - Select it, in Inspector click **Add Component**
   - Search for `InteriorExteriorManager` and add it
   - **If you get "script class can't be found" error:**
     - Wait for Unity to finish compiling (check bottom-right corner)
     - Go to **Assets → Refresh** (or press `Ctrl+R`)
     - If still not found, check Console for compilation errors
     - Make sure all scripts are in `Assets/Scripts/` folder

4. **Assign References in InteriorExteriorManager:**
   - **Main Camera:** Drag `MainCamera` from hierarchy
   - **Camera Mode Controller:** Drag your `CameraModeController` GameObject
   - **Drone Controller:** Drag your `DroneController` GameObject  
   - **Walk Controller:** Drag your `WalkController` GameObject
   - **Exit Interior Button:** Drag `ExitInteriorButton` from hierarchy
   - **Exit Button Container:** Drag `ExitButtonContainer` from hierarchy
   - **Fade Image:** Drag `FadeImage` Image component (not GameObject)
   - **Fade Duration:** Set to `1.0` (seconds)
   - **Fade Color:** Black (0, 0, 0, 255)
   - **Interior Light Intensity:** `2.0` (makes interior lights brighter)

5. **Create Room Entry Points:**
   For each room/doorway in your buildings:
   
   - Create empty GameObject at door location: Right-click → **Create Empty**
   - Name it `Room1_Entry` (or descriptive name like `BlockA_Room1_Entry`)
   - Position it at the doorway (use Scene view to place accurately)
   - Add Collider: **Add Component → Box Collider** (or appropriate collider)
   - In Box Collider:
     - Check ✅ **Is Trigger**
     - Adjust **Size** to cover doorway area (e.g., Width: 2, Height: 3, Depth: 1)
     - Adjust **Center** to position collider correctly

6. **Create Room Data:**
   - In InteriorExteriorManager Inspector, find **Room Entry Points** array
   - Set **Size** to number of rooms you have (e.g., `3` for 3 rooms)
   - For each room entry:
     - **Trigger Collider:** Drag the Box Collider from your Room1_Entry GameObject
     - **Room Data:** Click the circle icon next to it → **Create New RoomData**
     - In RoomData:
       - **Room Name:** "Room 1" or descriptive name
       - **Room Description:** Optional description
       - **Entry Position:** Position where camera should be when entering (use Scene view to find good position)
       - **Entry Rotation:** Rotation camera should have (use Inspector rotation values)
       - **Boundary Colliders:** Array of colliders for room walls
         - Set size to number of walls
         - Drag wall colliders from scene (these prevent going through walls)
       - **Room Lights:** Array of lights in this room
         - Set size to number of lights
         - Drag Light components from scene
       - **Room Objects:** Optional - objects visible only in this room

**How it works:**
- When player walks through door trigger → automatically enters interior mode
- Camera fades to black → moves to room entry position → fades in
- Exit button appears → clicking it returns to exterior at saved position
- Room boundaries prevent going through walls

**Testing:**
- Enter Play mode
- Walk to a door (in Walk or Drone mode)
- Should fade and enter room
- Exit button should appear
- Click exit → should return to exterior

---

### 3. Setup Building Highlight & Camera Movement (5 minutes)

**This enhances the search feature with smooth camera movement and glowing doors.**

**Step-by-step process:**

1. **Add CameraMovementController:**
   - Create empty GameObject: Right-click Hierarchy → **Create Empty**
   - Name it `CameraMovementController`
   - Select it, **Add Component → CameraMovementController**
   - Assign references:
     - **Main Camera:** Drag `MainCamera`
     - **Camera Mode Controller:** Drag `CameraModeController` GameObject
   - Configure settings:
     - **Intermediate Height:** `80` (height camera reaches before moving horizontally in drone mode)
     - **Vertical Speed:** `10` (speed of vertical movement)
     - **Horizontal Speed:** `8` (speed of horizontal movement)
     - **Rotation Speed:** `5` (speed of rotation)

2. **Update Building Data:**
   - Find your `SearchManager` GameObject in hierarchy
   - In Inspector, expand **All Buildings** list
   - For each building:
     - Expand the building entry
     - **Best View Position:** Set optimal camera position for viewing this building
       - Enter Play mode, position camera at best viewing angle
       - Copy position values from Transform component
       - Paste into Best View Position field
     - **Best View Rotation:** Set optimal camera rotation (Euler angles)
       - Copy rotation values from Transform
       - Paste into Best View Rotation field
     - **Door Objects:** Array of door GameObjects
       - Set size to number of doors
       - Drag door GameObjects from hierarchy

3. **Update BottomSheetManager:**
   - Find `BottomSheetManager` GameObject
   - In Inspector, assign:
     - **Camera Movement Controller:** Drag `CameraMovementController` GameObject
     - **Camera Mode Controller:** Drag `CameraModeController` GameObject

4. **Setup Door Glow Effect:**
   You have two options:
   
   **Option A - Manual (Recommended):**
   - For each door GameObject:
     - Select door in hierarchy
     - **Add Component → DoorGlowEffect**
     - Configure:
       - **Glow Color:** Yellow/Orange (R:255, G:204, B:0)
       - **Glow Intensity:** `2.0`
       - **Pulse Speed:** `2.0`
       - **Door Renderer:** Auto-assigned (or drag manually)
   
   **Option B - Auto-Detect:**
   - BuildingHighlightManager will automatically find objects with "door" in name
   - It will add DoorGlowEffect component automatically
   - Make sure **Enable Door Glow** is checked in BuildingHighlightManager

**How it works:**
- When building is highlighted → doors glow with pulsing effect
- When "Go" button clicked → camera smoothly moves to building
- In drone mode → camera moves up first, then horizontally (two-phase movement)
- In other modes → direct movement to building

**Testing:**
- Search for a building
- Click building in dropdown
- Click "Highlight" → building highlights, doors glow
- Click "Go" → camera smoothly moves to building

---

### 4. Setup Boundaries (5 minutes)

**This prevents camera/drone from going outside the map or through walls.**

**Step-by-step process:**

1. **Create Exterior Boundary:**
   - Create empty GameObject: Right-click → **Create Empty**
   - Name it `ExteriorBoundary`
   - Position it at center of your map
   - **Add Component → Box Collider**
   - In Box Collider:
     - Check ✅ **Is Trigger** (important!)
     - Adjust **Size** to cover entire map area
       - X: Map width
       - Y: High enough (e.g., 200)
       - Z: Map depth
   - Adjust **Center** if needed

2. **Add BoundaryController:**
   - Create empty GameObject: Right-click → **Create Empty**
   - Name it `BoundaryController`
   - Select it, **Add Component → BoundaryController**
   - Assign references:
     - **Exterior Boundary:** Drag `ExteriorBoundary` Box Collider component
     - **Interior Exterior Manager:** Drag `InteriorExteriorManager` GameObject
     - **Main Camera:** Drag `MainCamera`
     - **Drone Controller:** Drag `DroneController` GameObject
     - **Walk Controller:** Drag `WalkController` GameObject
   - Configure:
     - **Enforce Exterior Boundary:** ✅ Checked
     - **Enforce Interior Boundary:** ✅ Checked
     - **Boundary Padding:** `1.0` (distance from boundary edge)

3. **Interior Boundaries:**
   - Already set up in Step 2 (Room Setup)
   - Room boundary colliders are assigned in RoomData
   - These should NOT be triggers (solid colliders)
   - BoundaryController automatically uses them when in interior mode

**How it works:**
- Exterior mode: Prevents going outside ExteriorBoundary box
- Interior mode: Prevents going through room walls (boundary colliders)
- Camera position is clamped to valid area in Update()

**Testing:**
- Enter Play mode
- Try to move outside map → should be blocked
- Enter interior mode
- Try to move through walls → should be blocked

---

### 5. Make UI Responsive (5 minutes)

**This ensures UI scales properly on different screen sizes and devices.**

**Step-by-step process:**

1. **Setup Canvas Scaler:**
   - Select your Canvas GameObject
   - In Inspector, find **Canvas Scaler** component
   - If not present: **Add Component → Canvas Scaler**
   - Configure:
     - **UI Scale Mode:** `Scale With Screen Size`
     - **Reference Resolution:** 
       - X: `1920`
       - Y: `1080`
     - **Match:** `0.5` (or `0` for width-based, `1` for height-based)

2. **Anchor UI Elements:**
   For each UI element (buttons, panels, text):
   
   - Select the UI element
   - In **RectTransform**, use anchor presets:
     - **Top-left buttons:** Click anchor preset → Hold `Shift+Alt` → Click top-left preset
     - **Top-right buttons:** Same but top-right preset
     - **Bottom panels:** Same but bottom preset
     - **Center elements:** Same but center preset
   
   - **Manual anchoring:**
     - Click anchor square in RectTransform
     - Drag to set custom anchors
     - Hold `Shift` to also set position
     - Hold `Alt` to also set size

3. **Add ResponsiveUIHelper (Optional):**
   - Create empty GameObject: Right-click → **Create Empty**
   - Name it `ResponsiveUIHelper`
   - **Add Component → ResponsiveUIHelper**
   - Configure:
     - **Reference Width:** `1920`
     - **Reference Height:** `1080`
     - **Min Scale:** `0.5`
     - **Max Scale:** `2.0`
   - Assign arrays:
     - **Scalable Buttons:** Drag buttons that should scale
     - **Scalable Texts:** Drag Text components that should scale
     - **Scalable Panels:** Drag panels that should adjust

**How it works:**
- Canvas Scaler automatically scales UI based on screen size
- ResponsiveUIHelper provides additional fine-tuning
- Anchors ensure elements stay in correct positions

**Testing:**
- Enter Play mode
- Resize Game window (drag edges)
- UI should scale proportionally
- Elements should stay anchored correctly

---

### 6. Update Icons (as needed)

**Replace or add new icons for buttons.**

**Step-by-step process:**

1. **Prepare Icon Images:**
   - Create/obtain icon images (PNG format recommended)
   - Transparent background preferred
   - Recommended sizes:
     - Small icons: 64x64 or 128x128 pixels
     - Medium icons: 256x256 pixels
     - Large icons: 512x512 pixels

2. **Import to Unity:**
   - Create folder: `Assets/Images/Icons/` (if doesn't exist)
   - Drag icon PNG files into this folder
   - Select each icon in Project window

3. **Configure Import Settings:**
   - In Inspector:
     - **Texture Type:** `Sprite (2D and UI)`
     - **Pixels Per Unit:** `100`
     - **Filter Mode:** `Bilinear` (or `Point` for pixel art)
     - **Compression:** `None` (for crisp icons) or adjust as needed
   - Click **Apply**

4. **Apply to Buttons:**
   - Select button GameObject in hierarchy
   - In Inspector, find **Image** component
   - Drag icon sprite to **Source Image** field
   - Configure:
     - **Image Type:** `Simple`
     - **Preserve Aspect:** ✅ Checked (maintains icon proportions)
   - Adjust button size if needed

5. **Update Button Text (if needed):**
   - If button has text, adjust:
     - Font size
     - Text content
     - Position relative to icon

**Tips:**
- Use consistent icon style throughout app
- Test icons on different screen sizes
- Consider using icon fonts for scalability
- Keep file sizes small for performance

## Key Inspector Settings

### InteriorExteriorManager
- Fade Duration: 1.0
- Interior Light Intensity: 2.0
- Assign all controllers and UI references

### CameraMovementController
- Intermediate Height: 80
- Vertical Speed: 10
- Horizontal Speed: 8

### BoundaryController
- Boundary Padding: 1.0
- Enforce Exterior Boundary: ✅
- Enforce Interior Boundary: ✅

### BuildingData (in SearchManager)
- Set bestViewPosition for each building
- Set bestViewRotation for each building
- Assign doorObjects array

## Testing Order

1. ✅ Import assets and verify materials
2. ✅ Test exterior mode movement
3. ✅ Enter interior mode (walk through door)
4. ✅ Test interior movement and exit button
5. ✅ Search for building and highlight
6. ✅ Click "Go" button - verify smooth camera movement
7. ✅ Verify door glow effect
8. ✅ Test boundaries (try to go outside map)
9. ✅ Test UI responsiveness (resize game window)

## Common Issues & Fixes

### "Script Class Can't Be Found" Error

**This is the most common issue when adding new components. Here's how to fix it:**

1. **Wait for Unity to Compile:**
   - Check bottom-right corner of Unity Editor for compilation progress
   - Wait until it says "Compiling..." then finishes
   - Scripts won't be available until compilation completes

2. **Force Refresh:**
   - Go to **Assets → Refresh** (or press `Ctrl+R`)
   - This forces Unity to re-scan and compile scripts

3. **Check Console for Errors:**
   - Open **Window → General → Console**
   - Look for red error messages
   - Fix any compilation errors first
   - Common errors:
     - Missing `using` statements
     - Typos in class names
     - Missing dependencies

4. **Verify Script Files:**
   - Make sure script file name matches class name exactly
   - `InteriorExteriorManager.cs` must contain `public class InteriorExteriorManager`
   - Check file is in `Assets/Scripts/` folder (not subfolders unless namespace matches)

5. **Check Script Dependencies:**
   - InteriorExteriorManager requires:
     - `RoomData.cs` (must exist and compile)
     - `RoomEntryPoint.cs` (must exist and compile)
     - `RoomTrigger.cs` (must exist and compile)
   - If any dependency has errors, fix those first

6. **Restart Unity (Last Resort):**
   - Close Unity completely
   - Reopen your project
   - Wait for full compilation
   - Try adding component again

7. **Manual Script Check:**
   - In Project window, find `Assets/Scripts/InteriorExteriorManager.cs`
   - Double-click to open in code editor
   - Check for syntax errors (red underlines)
   - Save file if you made changes
   - Return to Unity and wait for recompilation

**If still not working:**
- Check that all scripts are saved (no unsaved changes in code editor)
- Verify Unity version compatibility
- Try creating a new script with same code to test

---

### Other Common Issues

**Pink Materials**: 
- Run MaterialAutoAssigner → Auto-Assign Materials
- Check materials are assigned in Inspector
- Verify shaders are compatible (use Standard shader)

**Camera doesn't move**: 
- Check CameraMovementController is assigned to BottomSheetManager
- Verify BuildingData has bestViewPosition set
- Check if in drone mode (two-phase movement)

**Doors don't glow**: 
- Check BuildingHighlightManager → Enable Door Glow = true
- Verify DoorGlowEffect component is on door GameObject
- Check door name contains "door", "entrance", or "entry"

**Boundaries don't work**: 
- Verify colliders are set correctly:
  - Exterior: Is Trigger = true
  - Interior walls: Is Trigger = false (solid)
- Check BoundaryController has correct references
- Verify boundary padding is appropriate

**UI not responsive**: 
- Check Canvas Scaler settings (Scale With Screen Size)
- Verify anchors are set correctly on UI elements
- Test on different screen sizes/aspect ratios

**Interior mode doesn't trigger**:
- Check RoomEntryPoint trigger collider is set as Trigger
- Verify RoomTrigger component is on trigger collider
- Check InteriorExteriorManager has roomEntryPoints assigned
- Make sure player/camera has a Collider component

## Next Steps After Setup

1. Fine-tune animation speeds and curves
2. Adjust boundary sizes
3. Add more rooms
4. Customize materials and colors
5. Optimize performance
6. Test on target devices

---

**Total Setup Time**: ~30-40 minutes
**Difficulty**: Medium
**Requires**: Basic Unity knowledge

For detailed instructions, see `Complete_Setup_Guide.md`

