# Phase 2: Bottom Sheet Setup Guide

## Overview
This guide will help you set up the bottom sheet panel that slides up when a building is selected from the search dropdown.

## 1. Create Bottom Sheet Panel

### 1.1 Create Main Panel
- Create UI > Panel as child of Canvas
- Name it "BottomSheetPanel"
- Set width to 100% of screen width
- Set height to 40% of screen height (e.g., 400px on 1080p screen)
- Position at bottom of screen (Y = -200 for 1080p screen)
- Set anchor to bottom-center
- Add semi-transparent background color (e.g., RGBA: 0, 0, 0, 0.8)

### 1.2 Create Content Area
- Create empty GameObject as child of BottomSheetPanel
- Name it "Content"
- Add VerticalLayoutGroup component
- Set Child Force Expand to true
- Set Child Control Size to true
- Set Spacing to 10
- Set Padding: Top=20, Bottom=20, Left=20, Right=20

## 2. Create Building Information Display

### 2.1 Building Name
- Create TextMeshPro - Text (UI) as child of Content
- Name it "BuildingNameText"
- Set font size to 24
- Set font style to Bold
- Set color to white
- Set text to "Building Name"

### 2.2 Building Description
- Create TextMeshPro - Text (UI) as child of Content
- Name it "BuildingDescriptionText"
- Set font size to 16
- Set color to light gray
- Set text to "Building description will appear here"
- Set preferred width to 90% of parent

### 2.3 Building Location
- Create TextMeshPro - Text (UI) as child of Content
- Name it "BuildingLocationText"
- Set font size to 14
- Set color to light blue
- Set text to "Position: X, Y, Z"

## 3. Create Action Buttons

### 3.1 Button Container
- Create empty GameObject as child of Content
- Name it "ButtonContainer"
- Add HorizontalLayoutGroup component
- Set Child Force Expand to true
- Set Child Control Size to true
- Set Spacing to 20
- Set Child Alignment to Middle Center

### 3.2 Close Button (Top Right)
- Create UI > Button as child of BottomSheetPanel (NOT in Content)
- Name it "CloseButton"
- Set size to 40x40px
- Position at top-right corner (X=180, Y=180 for 1080p screen)
- Set anchor to top-right
- Add TextMeshPro - Text (UI) as child
- Set text to "×"
- Set font size to 24
- Set color to white

### 3.3 Highlight Button
- Create UI > Button as child of ButtonContainer
- Name it "HighlightButton"
- Set width to 120px, height to 50px
- Add TextMeshPro - Text (UI) as child
- Set text to "Highlight"
- Set font size to 16
- Set color to white
- Set button color to cyan (0.2, 0.8, 1.0)

### 3.4 Go Button
- Create UI > Button as child of ButtonContainer
- Name it "GoButton"
- Set width to 120px, height to 50px
- Add TextMeshPro - Text (UI) as child
- Set text to "Go"
- Set font size to 16
- Set color to white
- Set button color to green (0.2, 1.0, 0.2)

## 4. Setup BottomSheetManager

### 4.1 Create Manager Object
- Create empty GameObject in scene
- Name it "BottomSheetManager"
- Add BottomSheetManager script

### 4.2 Assign References
- **Bottom Sheet Panel**: BottomSheetPanel
- **Bottom Sheet Rect**: BottomSheetPanel's RectTransform
- **Building Name Text**: BuildingNameText
- **Building Description Text**: BuildingDescriptionText
- **Building Location Text**: BuildingLocationText
- **Close Button**: CloseButton
- **Highlight Button**: HighlightButton
- **Go Button**: GoButton

### 4.3 Configure Animation
- **Slide Up Duration**: 0.3 seconds
- **Slide Down Duration**: 0.2 seconds
- **Slide Up Curve**: EaseInOut
- **Slide Down Curve**: EaseInOut

## 5. Connect SearchManager to BottomSheetManager

### 5.1 Update SearchManager
- Select the SearchManager object
- In the SearchManager component, find "Bottom Sheet" section
- Assign the BottomSheetManager to the "Bottom Sheet Manager" field

## 6. Test the Bottom Sheet

### 6.1 Test Steps
1. Play the scene
2. Type in the search field (e.g., "block")
3. Select a building from the dropdown
4. Verify bottom sheet slides up
5. Check that building information is displayed
6. Test close button
7. Test highlight and go buttons (they will log to console for now)

### 6.2 Expected Behavior
- Bottom sheet slides up smoothly when building is selected
- Building name, description, and position are displayed
- Close button (×) closes the bottom sheet
- Highlight and Go buttons are clickable (functionality in Phase 3 & 4)

## 7. Troubleshooting

### Common Issues:

1. **Bottom sheet doesn't slide up:**
   - Check that BottomSheetManager is assigned in SearchManager
   - Verify all UI references are assigned in BottomSheetManager
   - Check Console for error messages

2. **Building information not displayed:**
   - Verify TextMeshPro components are assigned
   - Check that building data is being passed correctly
   - Look for debug messages in Console

3. **Animation not smooth:**
   - Check animation curve settings
   - Verify duration values are reasonable
   - Test on different devices for performance

4. **Buttons not working:**
   - Verify button references are assigned
   - Check that onClick events are properly set up
   - Look for debug messages when clicking buttons

## 8. Next Steps
- Phase 3: Visual effects and highlighting
- Phase 4: Camera movement system
- Integration with existing camera modes

## 9. Visual Design Tips
- Use consistent colors and fonts
- Ensure good contrast for readability
- Test on different screen sizes
- Consider mobile touch targets (minimum 44px)
- Use smooth animations for better UX







