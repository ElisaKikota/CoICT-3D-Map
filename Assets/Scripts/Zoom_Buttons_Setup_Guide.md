# Zoom Buttons Setup Guide

## Overview
This guide will help you set up the + and - zoom buttons for the 2D mode. These buttons will replace the pinch sensing feature and will only appear in 2D mode.

## 1. Create Zoom Button Panel

### 1.1 Create Main Panel
- Create UI > Panel as child of Canvas
- Name it "ZoomButtonPanel"
- Set width to 150px
- Set height to 120px
- Position it in the top-right corner of the screen
- Set anchor to top-right
- Add semi-transparent background color (e.g., RGBA: 0, 0, 0, 0.3)

### 1.2 Create Button Container
- Create empty GameObject as child of ZoomButtonPanel
- Name it "ButtonContainer"
- Add VerticalLayoutGroup component
- Set Child Force Expand to false
- Set Child Control Size to true
- Set Spacing to 10
- Set Child Alignment to Middle Center
- Set Padding: Top=10, Bottom=10, Left=10, Right=10

## 2. Create Zoom Buttons

### 2.1 Zoom In Button (+)
- Create UI > Button as child of ButtonContainer
- Name it "ZoomInButton"
- Set size to 60x60px
- Add TextMeshPro - Text (UI) as child
- Set text to "+"
- Set font size to 30
- Set font style to Bold
- Set color to white

### 2.2 Zoom Out Button (-)
- Create UI > Button as child of ButtonContainer
- Name it "ZoomOutButton"
- Set size to 60x60px
- Add TextMeshPro - Text (UI) as child
- Set text to "-"
- Set font size to 30
- Set font style to Bold
- Set color to white

## 3. Setup ZoomButtonManager

### 3.1 Add Script
- Create empty GameObject in scene
- Name it "ZoomButtonManager"
- Add ZoomButtonManager script to it

### 3.2 Assign References
- Assign ZoomButtonPanel to the ZoomButtonManager script
- Assign ZoomInButton to zoomInButton field
- Assign ZoomOutButton to zoomOutButton field
- Assign MainCamera's Camera2DController to camera2DController field

## 4. Update CameraModeController

### 4.1 Assign ZoomButtonManager
- Select the UIManager GameObject (which has CameraModeController script)
- In the CameraModeController component, assign the ZoomButtonManager to the zoomButtonManager field

## 5. Test the Setup

### 5.1 Verify Functionality
- Switch to 2D mode - zoom buttons should appear
- Switch to Drone mode - zoom buttons should disappear
- Switch to Walk mode - zoom buttons should disappear
- Click + button - should zoom in
- Click - button - should zoom out

### 5.2 Debug Logs
- Check console for debug messages when buttons are clicked
- Verify that zoom buttons are shown/hidden correctly when switching modes

## 6. Troubleshooting

### 6.1 If Zoom Buttons Don't Work
**Problem**: Console shows button clicks but zoom doesn't work
**Solution**: 
1. Check that ZoomButtonManager is assigned to CameraModeController
2. Check that Camera2DController is assigned to ZoomButtonManager
3. Verify the camera has the Camera2DController component
4. Check console for error messages about null references

### 6.2 Common Issues
- **"Camera2DController is null"**: Assign the Camera2DController reference in ZoomButtonManager
- **"ZoomButtonManager is null"**: Assign the ZoomButtonManager reference in CameraModeController
- **Buttons not visible**: Check that ZoomButtonPanel is assigned to ZoomButtonManager
- **Buttons always visible**: Check that CameraModeController has ZoomButtonManager assigned

## 7. Zoom Settings

### 7.1 Default Zoom Configuration
- **Min Height**: 50 (closest to ground - highest zoom level)
- **Max Height**: 300 (furthest from ground - lowest zoom level)
- **Zoom Speed**: 10 (how much to change height per click)

### 7.2 How Zoom Works
- **Zoom In (+)**: Lowers camera height (moves camera closer to ground)
- **Zoom Out (-)**: Raises camera height (moves camera further from ground)
- **Mouse Scroll**: Also changes camera height for smooth zooming

### 7.3 Adjusting Zoom Settings
You can modify these values in the Camera2DController component:
- **minHeight**: Lower values = can get closer to the ground
- **maxHeight**: Higher values = can see more of the map from above
- **zoomSpeed**: Higher values = bigger height changes per click

## 8. Eye Button Functionality

### 8.1 What the Eye Button Does
- **Toggle Joystick Visibility**: Hides/shows the joystick container
- **Only Active in Drone/Walk Modes**: Eye button only works when joysticks are visible
- **Reset on Mode Switch**: When switching to 2D mode, joystick visibility resets to visible

### 8.2 How It Works
- Click eye button → Joysticks disappear
- Click eye button again → Joysticks reappear
- Switch to 2D mode → Joysticks automatically hidden (eye button not functional)
- Switch back to Drone/Walk → Joysticks visible again (eye button functional)

## Notes

- The zoom buttons will only be visible in 2D mode
- The pinch sensing feature has been removed from Camera2DController
- Mouse scroll wheel zoom still works in 2D mode (changes camera height)
- The eye button toggles joystick visibility in drone and walk modes
- Zoom works by changing the camera's Y position (height) rather than orthographic size
- If zoom buttons don't seem to work, check the console - you might already be at the min/max height
