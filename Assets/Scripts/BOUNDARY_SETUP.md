# Camera Boundary Setup Guide

This guide explains how to set up boundaries to limit camera movement within the desired map area.

## Overview

The boundary system uses a `BoxCollider` to define the area where the camera can move. The `BoundaryController` script manages boundary enforcement across all camera modes (2D, Drone, and Walk).

## Setup Instructions

### Step 1: Create Boundary GameObject

1. In Unity, create an empty GameObject and name it **"MapBoundary"** (or any name you prefer)
2. Position it at the center of your map area
3. Add a **BoxCollider** component to this GameObject

### Step 2: Configure BoxCollider

1. Select the **MapBoundary** GameObject
2. In the BoxCollider component:
   - Set **Size** to match your map dimensions:
     - **X**: Width of your map
     - **Y**: Maximum height (for drone mode, e.g., 200)
     - **Z**: Depth of your map
   - Adjust **Center** if needed to center the collider on your map
   - **Note**: The collider should be large enough to encompass your entire map area

### Step 3: Assign Boundary to BoundaryController

1. Find the **BoundaryController** component in your scene (it should be on a GameObject, possibly the same one as CameraModeController)
2. Assign the **MapBoundary** GameObject's BoxCollider to the **Exterior Boundary** field
3. Make sure **Enforce Exterior Boundary** is checked

### Step 4: Verify Boundary Settings

In the **BoundaryController** component:
- **Exterior Boundary**: Should reference your MapBoundary BoxCollider
- **Enforce Exterior Boundary**: Should be checked (enabled)
- **Boundary Padding**: Adjust if you want some margin from the edges (default: 1 unit)

## How It Works

### Drone Mode
- Camera movement is clamped within the boundary bounds
- X, Y (height), and Z positions are all constrained
- Minimum height: 10 units (configurable in DroneController)
- Maximum height: 200 units (configurable in DroneController)

### Walk Mode
- Camera movement is clamped within the boundary bounds
- Only X and Z positions are constrained (Y stays at ground level)
- Horizontal movement only

### 2D Mode
- Camera movement is clamped within the boundary bounds
- Only X and Z positions are constrained (Y stays fixed at 180)
- Top-down view movement only

## Testing

1. Enter **Drone Mode** and try to move the camera beyond the map edges - it should stop at the boundary
2. Enter **Walk Mode** and try to walk beyond the map edges - movement should be constrained
3. Enter **2D Mode** and try to pan beyond the map edges - panning should be limited

## Troubleshooting

**Camera can still move outside boundaries:**
- Check that the BoundaryController's `exteriorBoundary` field is assigned
- Verify that `enforceExteriorBoundary` is enabled
- Make sure the BoxCollider size is correct

**Boundary too small/large:**
- Adjust the BoxCollider's Size values in the Inspector
- The boundary should encompass your entire playable map area

**Drone mode height issues:**
- Adjust `minHeight` and `maxHeight` in the DroneController component
- Make sure the boundary's Y size covers the desired height range




















