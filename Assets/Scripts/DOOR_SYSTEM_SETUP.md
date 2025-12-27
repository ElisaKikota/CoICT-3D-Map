# Door System Setup Guide

## Where to Attach DoorRaycastSystem

**Recommended:** Attach `DoorRaycastSystem` to a **separate manager GameObject** (not on the camera itself).

### Option 1: Separate Manager GameObject (Recommended)

1. Create an empty GameObject in your scene (e.g., "DoorSystem" or "DoorManager")
2. Attach the `DoorRaycastSystem` component to this GameObject
3. Assign references:
   - **Main Camera**: Your Main Camera GameObject from the scene
   - **UI Canvas**: Your Canvas GameObject
   - **Door Button Panel**: Your EnterExitButtonPanel GameObject
   - **Doors Parent**: Your "Doors" parent GameObject

**Why this approach?**
- Keeps camera GameObject clean
- Better organization (separates systems)
- Easier to find and manage
- Follows Unity best practices

### Option 2: Attach to Main Camera (Alternative)

If you prefer to attach it to the camera:
1. Select your Main Camera GameObject
2. Add `DoorRaycastSystem` component to it
3. Assign references as above (mainCamera can be the camera itself or left to auto-detect)

**Note:** The script will auto-find the main camera if not assigned, so you can also leave it empty if attaching to the camera.

## Main Camera Reference

The `mainCamera` field in DoorRaycastSystem should:
- Point to your **Main Camera GameObject** (the one with the Camera component)
- Can be left empty - the script will auto-find it on Start
- Used for raycasting from the camera's position and forward direction

## How Raycasting Works

The DoorRaycastSystem performs raycasts from:
- **Position**: `mainCamera.transform.position` (camera's current position)
- **Direction**: `mainCamera.transform.forward` (where the camera is looking)

So the raycast goes straight forward from wherever the camera is positioned and facing.

## Setup Checklist

- [ ] Create manager GameObject OR attach to camera
- [ ] Add DoorRaycastSystem component
- [ ] Assign Main Camera reference (or leave empty for auto-detection)
- [ ] Assign UI Canvas
- [ ] Assign Door Button Panel (EnterExitButtonPanel with DoorUIButtonPanel component)
- [ ] Assign Doors Parent ("Doors" GameObject)
- [ ] Verify doors are auto-detected (check console logs)






