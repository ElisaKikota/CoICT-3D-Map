# Phase 3: Highlight System Setup Guide

## Overview
This guide will help you set up the building highlight system that works with your existing Blender FBX models.

## 1. Create BuildingHighlightManager

### 1.1 Create Manager Object
- Create empty GameObject in scene
- Name it "BuildingHighlightManager"
- Add BuildingHighlightManager script

### 1.2 Assign CampusRoot Reference
- In the BuildingHighlightManager component, find "Building References" section
- Assign your CampusRoot transform to the "Campus Root" field
- This should be the parent object containing all your building models (Block_A, Block_B, etc.)

## 2. Configure Highlight Settings

### 2.1 Highlight Colors
- **Highlight Color**: Cyan (0, 1, 1) - Color for the highlighted building
- **Normal Color**: White (1, 1, 1) - Original building color
- **Transparency Amount**: 0.3 - How transparent other buildings become
- **Highlight Intensity**: 2.0 - How bright the highlighted building becomes

### 2.2 Materials (Auto-Generated)
- The system will automatically create highlight and transparent materials
- **Highlight Material**: Cyan with emission for glowing effect
- **Transparent Material**: Semi-transparent for other buildings

## 3. Connect BottomSheetManager to BuildingHighlightManager

### 3.1 Update BottomSheetManager
- Select the BottomSheetManager object
- In the BottomSheetManager component, find "Highlight System" section
- Assign the BuildingHighlightManager to the "Highlight Manager" field

## 4. Update Building Data with Correct Names

### 4.1 Update SearchManager Building Names
Make sure your building names in SearchManager match your actual GameObject names:

```csharp
// In SearchManager.cs, update the building names to match your FBX models:
allBuildings.Add(new BuildingData("Block_A", "Main Administration Block", new Vector3(0, 0, 0), new Vector3(0, 10, -15), null));
allBuildings.Add(new BuildingData("Block_B", "Computer Science Department", new Vector3(20, 0, 0), new Vector3(20, 10, -15), null));
allBuildings.Add(new BuildingData("Block_C", "Electronics Department", new Vector3(-20, 0, 0), new Vector3(-20, 10, -15), null));
allBuildings.Add(new BuildingData("Block_D", "College Administration", new Vector3(-30, 0, 0), new Vector3(-30, 10, -15), null));
allBuildings.Add(new BuildingData("Hostel", "Student Hostel", new Vector3(0, 0, 30), new Vector3(0, 10, 15), null));
// Add more buildings as needed
```

### 4.2 Verify Building Names
- Check your Hierarchy to see the exact names of your building GameObjects
- Update the SearchManager building names to match exactly
- Names are case-sensitive!

## 5. Test the Highlight System

### 5.1 Test Steps
1. Play the scene
2. Type in search field (e.g., "block")
3. Select a building from dropdown
4. Click the "Highlight" button in the bottom sheet
5. Verify the selected building becomes cyan and glows
6. Verify other buildings become semi-transparent
7. Test the close button to reset all buildings

### 5.2 Expected Behavior
- **Highlighted Building**: Cyan color with emission glow
- **Other Buildings**: Semi-transparent (30% opacity)
- **Reset**: All buildings return to original materials when closed

## 6. Troubleshooting

### Common Issues:

1. **Buildings don't highlight:**
   - Check that CampusRoot is assigned in BuildingHighlightManager
   - Verify building names match between SearchManager and actual GameObjects
   - Check Console for error messages
   - Ensure buildings have Renderer components

2. **Materials not changing:**
   - Check that buildings have Renderer components
   - Verify materials are being created (check Console)
   - Ensure shaders are compatible (Standard shader recommended)

3. **Performance issues:**
   - Reduce highlight intensity if too bright
   - Check material count on complex buildings
   - Consider LOD (Level of Detail) for distant buildings

4. **Buildings not found:**
   - Verify building names are exact matches
   - Check that buildings are children of CampusRoot
   - Use debug logging to see which buildings are found

### Debug Steps:
1. Check Console for building discovery messages
2. Verify CampusRoot assignment
3. Test with simple building names first
4. Check material assignments in Inspector

## 7. Advanced Configuration

### 7.1 Custom Highlight Colors
```csharp
// In BuildingHighlightManager, you can customize:
public Color highlightColor = Color.cyan; // Change highlight color
public float highlightIntensity = 2f; // Adjust glow intensity
public float transparencyAmount = 0.3f; // Adjust transparency
```

### 7.2 Building-Specific Settings
- You can modify the highlight system to have different colors for different building types
- Add building categories (Academic, Residential, Administrative)
- Implement different highlight effects per category

## 8. Integration with Existing Systems

### 8.1 Camera Modes
- Highlight system works with all camera modes (2D, Drone, Walk)
- Highlighted buildings remain visible in all modes
- Reset highlight when switching camera modes

### 8.2 Search Integration
- Highlight automatically activates when building is selected
- Reset highlight when searching for new building
- Maintain highlight state during camera movement

## 9. Performance Optimization

### 9.1 Material Management
- Materials are created once and reused
- Original materials are stored for quick restoration
- Consider material pooling for many buildings

### 9.2 Rendering Optimization
- Use LOD groups for distant buildings
- Consider culling for buildings outside camera view
- Optimize shader complexity for mobile devices

## 10. Next Steps
- Phase 4: Camera movement system
- Integration with existing camera controllers
- Smooth transitions between highlight and camera movement







