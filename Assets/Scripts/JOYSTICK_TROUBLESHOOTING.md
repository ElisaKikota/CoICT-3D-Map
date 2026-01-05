# Right Joystick Troubleshooting Guide

## Common Issues and Solutions

### Issue: Right Joystick Not Working

The right joystick controls forward/backward movement and strafe left/right in both Drone and Walk modes.

## Step-by-Step Diagnosis

### 1. Check JoystickManager Assignment
1. In Unity, find the **JoystickContainer** GameObject in Hierarchy
2. Select it
3. In Inspector, find the **Joystick Manager** component
4. Check if **Right Joystick** field is assigned:
   - If it says "None (Simple Joystick)", you need to assign it
   - Drag the RightJoystick GameObject from Hierarchy into this field

### 2. Verify Right Joystick GameObject Has Components
Select the **RightJoystick** GameObject and verify it has:

**Required Components:**
- ✅ **Rect Transform** (should be there automatically)
- ✅ **Simple Joystick** script component
- ✅ **Image** component (or other Graphic component) - needed to receive touch events
- ✅ **Graphic Raycaster** on the Canvas (check the Canvas component)

**Check Simple Joystick Component:**
- Look for the **Handle** field
- It should be assigned to a child GameObject (the joystick handle/knob)
- If not assigned, the joystick won't move visually

### 3. Check Handle GameObject
1. Find the handle child object under RightJoystick
2. It should have:
   - **Rect Transform** component
   - **Image** component (visible circle/knob)

### 4. Verify Canvas Setup
1. Check that the Canvas (parent of JoystickContainer) has:
   - **Graphic Raycaster** component
   - **Canvas Scaler** component
2. Check Canvas settings:
   - Render Mode should be "Screen Space - Overlay" or "Screen Space - Camera"

### 5. Check Event System
1. In Hierarchy, look for an **EventSystem** GameObject
2. If it doesn't exist, create one:
   - Right-click in Hierarchy → UI → Event System
3. It should have:
   - **Event System** component
   - **Standalone Input Module** component (or **Input System UI Input Module**)

### 6. Verify JoystickContainer is Active
1. Make sure **JoystickContainer** is active in Hierarchy
2. Note: It should be inactive in 2D mode, but active in Drone/Walk modes

### 7. Check Console for Errors
1. Open Unity Console (Window → General → Console)
2. Look for warnings like:
   - "[JoystickManager] Right joystick is null!"
   - Any errors related to SimpleJoystick

## Quick Fix Checklist

- [ ] Right Joystick GameObject has SimpleJoystick component
- [ ] Right Joystick is assigned in JoystickManager
- [ ] Handle is assigned in SimpleJoystick component
- [ ] Right Joystick has Image component (or other Graphic)
- [ ] Canvas has GraphicRaycaster component
- [ ] EventSystem exists in scene
- [ ] You're in Drone or Walk mode (joysticks are hidden in 2D mode)

## Testing Steps

1. Enter Play mode
2. Switch to **Drone** or **Walk** mode (joysticks only appear in these modes)
3. Try dragging on the right joystick area
4. Check Console for debug messages from SimpleJoystick
5. The handle should move when you drag
6. Check if movement occurs when dragging the joystick

## Still Not Working?

If after all checks it still doesn't work:
1. Check if the left joystick works (to verify the system is functional)
2. Compare the RightJoystick setup with LeftJoystick setup
3. Make sure both joysticks have identical component setups
4. Check if the RightJoystick GameObject is actually visible and not hidden by another UI element














