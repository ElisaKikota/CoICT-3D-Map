# Manual Sidebar Setup Guide

This guide explains how to set up manually placed buttons in the sidebar that move the camera to highlighter GameObjects.

## Overview

The dynamic sidebar generation has been disabled. Instead, you'll manually create buttons and attach the `SidebarButtonHandler` script to each button.

## Setup Instructions

### 1. Disable Dynamic Sidebar Generator

The `DynamicSidebarGenerator` script is already disabled in code. If you have it attached to a GameObject, you can:
- Remove the component, OR
- Leave it (it won't generate anything)

### 2. Create Buttons Manually

1. In your Sidebar GameObject, create Button GameObjects for each location
2. Name them appropriately (e.g., "BlockAButton", "BlockBButton", etc.)
3. Set up their text/labels as needed

### 3. Add SidebarButtonHandler to Each Button

1. Select a button GameObject
2. Add Component → `SidebarButtonHandler`
3. In the Inspector, assign:
   - **Target Highlighter**: Drag the corresponding highlighter GameObject from your scene
   - **Camera Movement Controller**: (Optional) Drag the CameraMovementController GameObject, or leave empty to auto-find
   - **Camera Mode Controller**: (Optional) Drag the CameraModeController GameObject, or leave empty to auto-find
   - **Camera Offset**: Adjust the camera position offset (default: 0, 10, -15)
   - **Use Center Of Children**: Check this if the highlighter has multiple children (like "Studying Areas")

### 4. Handling Multiple Highlighters (e.g., "Studying Areas")

If you have a parent GameObject with multiple highlighter children (like "Studying Areas" with 3 highlighters inside):

1. **Option A: Use Parent GameObject (Recommended)**
   - Assign the **parent GameObject** (e.g., "Studying Areas") as the Target Highlighter
   - Check **"Use Center Of Children"** in the SidebarButtonHandler
   - The camera will move to the center position of all children

2. **Option B: Use Individual Highlighters**
   - Create separate buttons for each highlighter
   - Assign each button to a different child highlighter
   - Uncheck "Use Center Of Children"

### 5. Example Setup

**Single Highlighter (Block A):**
```
BlockAButton
  └─ SidebarButtonHandler
      └─ Target Highlighter: [BlockAHighlighter]
      └─ Use Center Of Children: [Unchecked]
```

**Multiple Highlighters (Studying Areas):**
```
StudyingAreasButton
  └─ SidebarButtonHandler
      └─ Target Highlighter: [StudyingAreas] (parent GameObject)
      └─ Use Center Of Children: [Checked]
```

**Studying Areas GameObject Structure:**
```
StudyingAreas (Empty GameObject)
  ├─ Highlighter1
  ├─ Highlighter2
  └─ Highlighter3
```

## How It Works

1. When a button is clicked, `SidebarButtonHandler`:
   - Switches to Drone mode (if not already)
   - Calculates target position:
     - **Single highlighter**: Uses highlighter position + offset
     - **Multiple highlighters**: Calculates center of all children + offset
   - Moves camera using the existing `CameraMovementController` system
   - Uses two-phase movement (up, then horizontal) for smooth transition

## Customization

You can adjust these settings per button:
- **Camera Offset**: Change the viewing angle/distance
- **Intermediate Height**: Height reached before horizontal movement
- **Movement Speed**: Speed of camera movement

## Troubleshooting

- **Camera doesn't move**: Check that `CameraMovementController` is in the scene
- **Wrong position**: Adjust the `Camera Offset` value
- **Multiple highlighters not working**: Ensure "Use Center Of Children" is checked and the parent GameObject is assigned (not a child)






