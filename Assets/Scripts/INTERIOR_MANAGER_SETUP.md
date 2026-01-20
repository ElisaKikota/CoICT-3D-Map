# InteriorExteriorManager Setup Guide

## Quick Setup Steps

### Step 1: Create the GameObject
1. In Unity Hierarchy, right-click → **Create Empty**
2. Name it **"InteriorExteriorManager"**

### Step 2: Add the Component
1. Select the **InteriorExteriorManager** GameObject
2. In Inspector, click **Add Component**
3. Search for **"InteriorExteriorManager"** and add it

### Step 3: Assign References (Minimum Required)
You only need to assign these for basic functionality:

**Required:**
- **Main Camera**: Drag your `Main Camera` from Hierarchy
- **Camera Mode Controller**: Drag your `UIManager` GameObject (the one with CameraModeController)
- **Drone Controller**: Drag your `DroneController` GameObject
- **Walk Controller**: Drag your `WalkController` GameObject

**Optional (can leave empty for now):**
- **Exit Interior Button**: Leave empty if you don't have an exit button yet
- **Exit Button Container**: Leave empty
- **Fade Image**: Leave empty if you don't have fade transitions yet
- **Door Raycast System**: Will auto-find if not assigned
- **Room Entry Points**: Leave empty array for now

### Step 4: Assign to BuildingEntrySystem
1. Find your **BuildingEntrySystem** GameObject (or create one if it doesn't exist)
2. In the **Interior Manager** field, drag the **InteriorExteriorManager** GameObject you just created

## Notes:
- The script will auto-find InteriorExteriorManager if not assigned, but it's better to assign it explicitly
- You can add fade transitions and exit buttons later - the basic functionality will work without them
- The InteriorExteriorManager is only needed when you actually want to enter building interiors
























