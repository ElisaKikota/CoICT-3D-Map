# Navigation UI System Setup Guide

This guide will help you set up the new navigation UI system that replaces the search bar.

## Overview

The new navigation system includes:
- **Top Bar**: Building icon (left) + 2D/Drone/Walk icons (right)
- **Sidebar**: Slides from right when building icon is pressed
  - **Blocks Section** (expanded by default): Block A, Block B, Block C, Block D, Cafe, Hostel
  - **Facilities Section** (collapsed by default): Administration, Departments, Lecture Theatres, Hostels, Cafe, Washrooms, Studying Areas
- **Bottom Sheet**: Shows building details with highlight button (joysticks hide when shown)

## Step 1: Create Top Bar UI

1. **Create Top Bar Panel**
   - Create UI > Panel as child of Canvas
   - Name it "TopBar"
   - Set Anchor: Top-Stretch
   - Set Height: 80px
   - Position at top of screen

2. **Create Building Icon Button (Left)**
   - Create UI > Button as child of TopBar
   - Name it "BuildingIconButton"
   - Set Anchor: Left-Middle
   - Position: Left side of TopBar
   - Set Size: 60x60px
   - Add your building icon image to the Image component

3. **Create Mode Icons Container (Right)**
   - Create empty GameObject as child of TopBar
   - Name it "ModeIconsContainer"
   - Add Horizontal Layout Group component
   - Set Anchor: Right-Middle
   - Position: Right side of TopBar
   - Set Spacing: 10px

4. **Create 2D Icon Button**
   - Create UI > Button as child of ModeIconsContainer
   - Name it "Icon2DButton"
   - Set Size: 50x50px
   - Add your 2D icon image

5. **Create Drone Icon Button**
   - Create UI > Button as child of ModeIconsContainer
   - Name it "IconDroneButton"
   - Set Size: 50x50px
   - Add your drone icon image

6. **Create Walk Icon Button**
   - Create UI > Button as child of ModeIconsContainer
   - Name it "IconWalkButton"
   - Set Size: 50x50px
   - Add your walk icon image

## Step 2: Create Sidebar UI

1. **Create Sidebar Panel**
   - Create UI > Panel as child of Canvas
   - Name it "SidebarPanel"
   - Set Anchor: Right-Stretch
   - Set Width: 300px
   - Set Height: Full screen height
   - Position off-screen to the right (will slide in)

2. **Add Sidebar Background**
   - Add Image component if not present
   - Set color to semi-transparent dark (e.g., RGBA: 0, 0, 0, 200)

3. **Create Close Button**
   - Create UI > Button as child of SidebarPanel
   - Name it "CloseSidebarButton"
   - Set Anchor: Top-Right
   - Position: Top-right corner
   - Set Size: 40x40px
   - Add "X" text or close icon

4. **Create Blocks Section**
   - Create UI > Button as child of SidebarPanel
   - Name it "BlocksHeaderButton"
   - Set Anchor: Top-Stretch
   - Position: Top of sidebar
   - Set Height: 50px
   - Add TextMeshPro - Text (UI) child: "Blocks ▼" (or "Blocks ▲" when expanded)
   
   - Create empty GameObject as child of SidebarPanel
   - Name it "BlocksContent"
   - Add Vertical Layout Group component
   - Set Spacing: 5px
   - Set Child Force Expand: Width = true, Height = false
   - Position below BlocksHeaderButton

5. **Create Block Buttons**
   - Create UI > Button as child of BlocksContent for each:
     - "BlockAButton" - Text: "Block A"
     - "BlockBButton" - Text: "Block B"
     - "BlockCButton" - Text: "Block C"
     - "BlockDButton" - Text: "Block D"
     - "CafeButton" - Text: "Cafe"
     - "HostelButton" - Text: "Hostel"
   - Set Height: 40px for each button

6. **Create Facilities Section**
   - Create UI > Button as child of SidebarPanel
   - Name it "FacilitiesHeaderButton"
   - Set Anchor: Top-Stretch
   - Position: Below BlocksContent
   - Set Height: 50px
   - Add TextMeshPro - Text (UI) child: "Facilities ▶" (or "Facilities ▼" when expanded)
   
   - Create empty GameObject as child of SidebarPanel
   - Name it "FacilitiesContent"
   - Add Vertical Layout Group component
   - Set Spacing: 5px
   - Set Child Force Expand: Width = true, Height = false
   - Position below FacilitiesHeaderButton
   - **Set Active to false** (collapsed by default)

7. **Create Facility Buttons**
   - Create UI > Button as child of FacilitiesContent for each:
     - "AdministrationButton" - Text: "Administration"
     - "DepartmentsButton" - Text: "Departments"
     - "LectureTheatresButton" - Text: "Lecture Theatres"
     - "HostelsButton" - Text: "Hostels"
     - "CafeFacilityButton" - Text: "Cafe"
     - "WashroomsButton" - Text: "Washrooms"
     - "StudyingAreasButton" - Text: "Studying Areas"
   - Set Height: 40px for each button

## Step 3: Update Bottom Sheet

1. **Hide Go Button**
   - Find your existing BottomSheetPanel
   - Find the "GoButton" in the bottom sheet
   - In the NavigationUIManager, the go button will be automatically hidden
   - Ensure only the Highlight button is visible

## Step 4: Setup NavigationUIManager Script

1. **Create NavigationUIManager GameObject**
   - Create empty GameObject in scene
   - Name it "NavigationUIManager"
   - Add NavigationUIManager component

2. **Assign Top Bar References**
   - Building Icon Button → BuildingIconButton
   - Icon 2D Button → Icon2DButton
   - Icon Drone Button → IconDroneButton
   - Icon Walk Button → IconWalkButton

3. **Assign Sidebar References**
   - Sidebar Panel → SidebarPanel
   - Sidebar Rect → SidebarPanel RectTransform
   - Close Sidebar Button → CloseSidebarButton

4. **Assign Blocks Section**
   - Blocks Header Button → BlocksHeaderButton
   - Blocks Content → BlocksContent
   - Block A Button → BlockAButton
   - Block B Button → BlockBButton
   - Block C Button → BlockCButton
   - Block D Button → BlockDButton
   - Cafe Button → CafeButton
   - Hostel Button → HostelButton

5. **Assign Facilities Section**
   - Facilities Header Button → FacilitiesHeaderButton
   - Facilities Content → FacilitiesContent
   - Assign all facility buttons

6. **Assign Other References**
   - Bottom Sheet Manager → Your existing BottomSheetManager
   - Joystick Container → Your JoystickContainer GameObject
   - Camera Mode Controller → Your CameraModeController

## Step 5: Update BottomSheetManager

1. **Add NavigationUIManager Reference**
   - In BottomSheetManager Inspector
   - Find "Navigation UI Manager" field
   - Assign NavigationUIManager GameObject

## Step 6: Remove Old Search UI (Optional)

1. **Disable SearchManager**
   - Find SearchManager GameObject
   - Disable it or remove it
   - Hide/remove search input field and dropdown UI

## Step 7: Test the System

1. **Test Top Bar**
   - Click building icon → Sidebar should slide in from right
   - Click 2D/Drone/Walk icons → Camera mode should switch

2. **Test Sidebar**
   - Click building icon → Sidebar opens
   - Click Blocks header → Should expand/collapse
   - Click Facilities header → Should expand/collapse
   - Click a block (e.g., Block A) → Sidebar closes, bottom sheet appears, joysticks hide

3. **Test Bottom Sheet**
   - When bottom sheet appears → Joysticks should fade out smoothly
   - Click highlight button → Building should highlight
   - Close bottom sheet → Joysticks should fade back in

## Animation Settings

You can adjust these in NavigationUIManager Inspector:
- **Sidebar Slide Duration**: How fast sidebar slides (default: 0.3s)
- **Joystick Hide Duration**: How fast joysticks fade (default: 0.2s)
- **Slide Curve**: Animation curve for sidebar movement

## Notes

- The sidebar starts hidden and slides in from the right
- Blocks section is expanded by default
- Facilities section is collapsed by default
- Joysticks automatically hide when bottom sheet appears
- Joysticks automatically show when bottom sheet closes
- Building data is hardcoded in NavigationUIManager (can be extended later)

## Future Enhancements

The system is designed to support 3 more inner levels for facilities (mentioned in requirements). The OnFacilitySelected method is ready for future implementation.


