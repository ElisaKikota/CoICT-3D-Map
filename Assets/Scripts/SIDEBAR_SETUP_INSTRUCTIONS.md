# Sidebar Setup Instructions

## Overview
The sidebar toggle functionality is already implemented in the `CameraModeController` script. You just need to assign the GameObjects in the Unity Inspector.

## Step-by-Step Setup

### 1. Locate the UIManager GameObject
1. In Unity, open your scene (e.g., `SampleScene`)
2. In the Hierarchy window, find the **UIManager** GameObject
3. Select it

### 2. Access the CameraModeController Component
1. In the Inspector window, you should see the **Camera Mode Controller** component
2. If you don't see it, click **Add Component** and search for "CameraModeController"

### 3. Assign the Sidebar GameObject
1. In the **Sidebar** section of the CameraModeController component, find the **Sidebar** field
2. In the Hierarchy, find the **Sidebar** GameObject (the one you renamed from AutocompleteDropdown)
3. Drag the **Sidebar** GameObject from Hierarchy into the **Sidebar** field
   - OR click the circle icon next to the field and search for "Sidebar"

### 4. Assign the Building Icon Button
1. In the **Sidebar** section, find the **Building Icon Button** field
2. In the Hierarchy, navigate to your top bar and find the building icon button
   - It's likely under Canvas > TopBar > BuildingIconButton (or similar path)
3. Drag the button GameObject from Hierarchy into the **Building Icon Button** field
   - OR click the circle icon and search for your building icon button

### 5. (Optional) Assign Sidebar Rect Transform
- The **Sidebar Rect** field is optional
- If you leave it empty, the script will automatically find the RectTransform component on the Sidebar GameObject
- If you want to assign it manually:
  1. Select the Sidebar GameObject
  2. Drag its RectTransform component into the **Sidebar Rect** field

### 6. Assign Mode Buttons (2D, Drone, Walk)
1. In the **Buttons** section, find:
   - **Btn 2D**
   - **Btn Drone**
   - **Btn Walk**
2. Drag each button from your Hierarchy into its respective field
   - These are the mode buttons on your top bar (2D mode, Drone mode, Walk mode buttons)

### 7. Verify Other Required Fields
Make sure these are also assigned (they may already be set):
- **Main Camera** - Your main camera
- **Top Down Pivot** - Transform for 2D view
- **Drone Pivot** - Transform for drone view
- **Walk Pivot** - Transform for walk view
- **Joystick Container** - Your joystick container GameObject
- **Joystick Manager** - Your JoystickManager component
- **Zoom Button Manager** - Your ZoomButtonManager component

## How It Works

Once assigned:
- **Building Icon Button**: Clicking this button will slide the Sidebar from left to right (and back when clicked again)
- **2D/Drone/Walk Buttons**: These switch between camera modes and the 2D mode is active on startup

## Testing

1. Click Play in Unity
2. Click the building icon button - the sidebar should slide in from the left
3. Click it again - the sidebar should slide out to the left
4. The 2D mode should be active by default (2D button should appear dimmed/active)
5. Click Drone or Walk buttons to switch modes

## Troubleshooting

- **Sidebar doesn't slide**: Make sure the Sidebar GameObject is assigned
- **Building button doesn't work**: Make sure Building Icon Button is assigned
- **Sidebar slides incorrectly**: Check that the Sidebar GameObject has a RectTransform component
- **Console warnings**: Check the Unity Console for specific error messages

## Animation Settings

You can adjust the sidebar animation in the **Sidebar Animation** section:
- **Sidebar Slide Duration**: How long the slide animation takes (default: 0.3 seconds)
- **Sidebar Slide Curve**: The animation curve for smoothness (default: EaseInOut)






