# Sidebar Dynamic System Architecture

## Overview
Create a scalable, hierarchical sidebar system that can display all buildings, offices, departments, and facilities with dynamic UI generation.

## Suggested Architecture

### 1. Data Structure

#### Option A: Enhanced BuildingData (Recommended)
Extend the existing `BuildingData` class to support hierarchical structures:

```csharp
[System.Serializable]
public class LocationData : BuildingData
{
    public LocationType type; // Building, Office, Department, Facility, etc.
    public LocationCategory category; // Main Office, Coordination Unit, Block, etc.
    public List<LocationData> subLocations; // For nested structures (e.g., Principal's Office → Accounting Office)
    public string parentLocationName; // Reference to parent if nested
    public int hierarchyLevel; // 0 = top level, 1 = nested, etc.
}
```

**Advantages:**
- Works with existing `BottomSheetManager` and `CameraMovementController`
- Reuses existing infrastructure
- Simple to implement

#### Option B: Hierarchical Structure
Create a new hierarchical class that references BuildingData:

```csharp
[System.Serializable]
public class LocationItem
{
    public string id;
    public string displayName;
    public LocationType type;
    public LocationCategory category;
    public BuildingData buildingData; // Optional - only for physical locations
    public List<LocationItem> children;
    public bool hasCameraView; // Does this item have a 3D position?
}
```

### 2. UI System Architecture

#### Dynamic Sidebar Generation
Instead of manually creating buttons in Unity Editor, generate them programmatically:

**Structure:**
```
Sidebar (ScrollView)
  ├─ Main Offices (Expandable Section)
  │   ├─ Principal's Office (Expandable Item)
  │   │   ├─ Accounting Office (Button)
  │   │   ├─ Administrative Office (Button)
  │   │   └─ ...
  │   ├─ Department of Electronics (Button)
  │   └─ ...
  ├─ Coordination Units (Expandable Section)
  ├─ Functional Units (Expandable Section)
  ├─ Main Blocks (Expandable Section)
  │   ├─ Block A (Button) [Shows details + moves camera]
  │   ├─ Block B (Expandable Item - shows contents)
  │   └─ ...
  └─ Other Features (Expandable Section)
```

### 3. Suggested Implementation Flow

#### Phase 1: Data Structure
1. Create `LocationCategory` enum
2. Extend `BuildingData` or create `LocationData`
3. Create a `CampusDataManager` script that:
   - Holds all location data
   - Can be populated from Inspector or JSON/CSV
   - Provides methods to get locations by category/type

#### Phase 2: Dynamic UI Generation
1. Create `SidebarItemPrefab` (Button prefab with expand/collapse icon)
2. Create `SidebarSectionPrefab` (Header with expand/collapse)
3. Create `SidebarContentGenerator` script that:
   - Reads data from `CampusDataManager`
   - Generates UI hierarchy dynamically
   - Handles expand/collapse logic
   - Creates buttons for each location

#### Phase 3: Integration
1. When location button clicked:
   - Close sidebar
   - If location has `BuildingData`:
     - Show BottomSheet with details
     - Move camera to `bestViewPosition` using `CameraMovementController`
   - If location doesn't have 3D position:
     - Show BottomSheet with info only (no camera movement)

### 4. Recommended Structure

#### Scripts to Create:
1. **CampusDataManager.cs** - Central data repository
   - Serializable list of all locations
   - Organized by categories
   - Methods to query/filter locations

2. **SidebarContentGenerator.cs** - Dynamic UI generator
   - Generates sidebar UI from CampusDataManager data
   - Handles expand/collapse
   - Creates clickable buttons

3. **LocationButtonHandler.cs** - Button click handler
   - Handles location selection
   - Calls BottomSheetManager
   - Calls CameraMovementController

### 5. Location Categories (Based on Your Data)

```csharp
public enum LocationCategory
{
    MainOffices,
    CoordinationUnits,
    FunctionalUnits,
    MainBlocks,
    OtherFeatures
}

public enum LocationType
{
    Building,        // Blocks A-E, Hostel, etc.
    Office,          // Principal's Office, Accounting Office, etc.
    Department,      // Dept of Electronics, etc.
    Unit,            // Coordination Units
    Facility,        // Labs, Studios, etc.
    Feature          // Playgrounds, Car Parks, etc.
}
```

### 6. Example Data Structure

```csharp
// Example: Block A
LocationData blockA = new LocationData
{
    name = "Block A (Administration Block)",
    description = "Contains: Principal's Office, Coordination Units",
    type = LocationType.Building,
    category = LocationCategory.MainBlocks,
    position = new Vector3(0, 0, 0),
    bestViewPosition = new Vector3(0, 50, -30),
    subLocations = new List<LocationData>
    {
        // Principal's Office (nested)
        new LocationData { name = "Principal's Office", type = LocationType.Office, ... },
        // Coordination Units (nested)
        new LocationData { name = "Undergraduate Studies", type = LocationType.Unit, ... }
    }
};
```

## Implementation Recommendation

### Step 1: Start Simple (Recommended)
1. Keep existing `BuildingData` structure
2. Create `CampusDataManager` to hold all locations
3. Manually create UI structure in Unity Editor first (Main Offices, Blocks, etc.)
4. Wire buttons to query `CampusDataManager` for data

### Step 2: Add Dynamic Generation (Later)
1. Create prefabs for sidebar items
2. Implement `SidebarContentGenerator`
3. Generate UI dynamically from data

### Step 3: Add Expandable Sections
1. Implement expand/collapse functionality
2. Support nested items (Principal's Office → Sub-offices)

## Your Idea Enhancement

**Your idea:** "When building is pressed → show details + move camera"

**Enhanced suggestion:**
1. **Click location in sidebar**
2. **If location has 3D position:**
   - Switch to 2D mode (best for viewing buildings)
   - Move camera to `bestViewPosition` smoothly
   - Show BottomSheet with:
     - Location name
     - Description/contents
     - "View Inside" button (if applicable)
     - "Highlight on Map" button
3. **If location is nested/office:**
   - Show BottomSheet with info
   - Option to "View Parent Building" (e.g., Principal's Office → Block A)

## Next Steps

Would you like me to:
1. Create the `CampusDataManager` script with all your locations?
2. Create a simple dynamic sidebar generator?
3. Start with manual UI structure and wire it up?
4. Something else?

Let me know which approach you prefer!
























