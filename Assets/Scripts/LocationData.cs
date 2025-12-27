using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Extended location data that supports hierarchical structures (buildings, offices, departments, etc.)
/// Inherits from BuildingData to maintain compatibility with existing systems
/// </summary>
[System.Serializable]
public class LocationData : BuildingData
{
    [Header("Location Metadata")]
    public LocationCategory category;
    public LocationType type;
    
    [Header("Hierarchy")]
    public List<LocationData> subLocations = new List<LocationData>();
    public string parentLocationId; // Reference to parent location if nested
    
    [Header("UI Display")]
    public string displayName; // Optional custom display name
    public string detailedDescription; // Extended description with contents
    
    public LocationData() : base("", "", Vector3.zero, Vector3.zero, null)
    {
        // Default constructor for serialization
        category = LocationCategory.OtherFeatures;
        type = LocationType.Feature;
    }
    
    public LocationData(string locationName, string description, LocationCategory category, LocationType type)
        : base(locationName, description, Vector3.zero, Vector3.zero, null)
    {
        this.category = category;
        this.type = type;
        this.displayName = locationName;
        this.detailedDescription = description;
    }
    
    public LocationData(string locationName, string description, LocationCategory category, LocationType type, Vector3 position, Vector3 viewPosition)
        : base(locationName, description, position, viewPosition, null)
    {
        this.category = category;
        this.type = type;
        this.displayName = locationName;
        this.detailedDescription = description;
        this.bestViewPosition = viewPosition;
    }
    
    // Check if this location has a 3D position (can be viewed in 3D)
    public bool Has3DPosition()
    {
        return position != Vector3.zero || bestViewPosition != Vector3.zero;
    }
    
    // Check if this location has sub-locations
    public bool HasSubLocations()
    {
        return subLocations != null && subLocations.Count > 0;
    }
}

/// <summary>
/// Categories for organizing locations in the sidebar
/// </summary>
public enum LocationCategory
{
    MainOffices,
    CoordinationUnits,
    FunctionalUnits,
    MainBlocks,
    OtherFeatures
}

/// <summary>
/// Types of locations
/// </summary>
public enum LocationType
{
    Building,       // Blocks A-E, Hostel, etc.
    Office,         // Principal's Office, Accounting Office, etc.
    Department,     // Dept of Electronics, etc.
    Unit,           // Coordination Units
    Facility,       // Labs, Studios, etc.
    Feature         // Playgrounds, Car Parks, etc.
}

