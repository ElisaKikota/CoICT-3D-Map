using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Central data manager for all campus locations (buildings, offices, departments, etc.)
/// This acts as the single source of truth for all location data
/// </summary>
public class CampusDataManager : MonoBehaviour
{
    [Header("Location Data")]
    [Tooltip("All campus locations organized by category")]
    public List<LocationData> allLocations = new List<LocationData>();
    
    // Internal dictionary for quick lookup by ID/name
    private Dictionary<string, LocationData> locationLookup = new Dictionary<string, LocationData>();
    private Dictionary<LocationCategory, List<LocationData>> categoryLookup = new Dictionary<LocationCategory, List<LocationData>>();
    
    void Awake()
    {
        InitializeData();
        BuildLookupTables();
    }
    
    /// <summary>
    /// Initialize all campus location data
    /// You can populate this from Inspector or modify this method
    /// </summary>
    void InitializeData()
    {
        // Only initialize if empty (allows Inspector override)
        if (allLocations.Count > 0)
        {
            Debug.Log($"[CampusDataManager] Using {allLocations.Count} locations from Inspector");
            return;
        }
        
        // Initialize Main Offices
        InitializeMainOffices();
        
        // Initialize Coordination Units
        InitializeCoordinationUnits();
        
        // Initialize Functional Units
        InitializeFunctionalUnits();
        
        // Initialize Main Blocks
        InitializeMainBlocks();
        
        // Initialize Other Features
        InitializeOtherFeatures();
        
        Debug.Log($"[CampusDataManager] Initialized {allLocations.Count} locations");
    }
    
    void InitializeMainOffices()
    {
        // Principal's Office with sub-offices
        LocationData principalsOffice = new LocationData("Principal's Office", "Main administrative office", LocationCategory.MainOffices, LocationType.Office);
        principalsOffice.subLocations.Add(new LocationData("Accounting Office", "Financial and accounting services", LocationCategory.MainOffices, LocationType.Office));
        principalsOffice.subLocations.Add(new LocationData("Administrative Office", "General administration", LocationCategory.MainOffices, LocationType.Office));
        principalsOffice.subLocations.Add(new LocationData("Procurement Office", "Procurement and purchasing", LocationCategory.MainOffices, LocationType.Office));
        principalsOffice.subLocations.Add(new LocationData("Registry Office", "Student and staff registry", LocationCategory.MainOffices, LocationType.Office));
        principalsOffice.subLocations.Add(new LocationData("Driver's Office", "Transportation services", LocationCategory.MainOffices, LocationType.Office));
        allLocations.Add(principalsOffice);
        
        // Departments
        allLocations.Add(new LocationData("Department of Electronics & Telecommunications Engineering", 
            "Engineering department focusing on electronics and telecommunications", LocationCategory.MainOffices, LocationType.Department));
        allLocations.Add(new LocationData("Department of Computer Science & Engineering", 
            "Computer science and engineering programs", LocationCategory.MainOffices, LocationType.Department));
        allLocations.Add(new LocationData("Center for Virtual Learning", 
            "Virtual learning and e-learning services", LocationCategory.MainOffices, LocationType.Facility));
    }
    
    void InitializeCoordinationUnits()
    {
        string[] coordinationUnits = new string[]
        {
            "Undergraduate Studies",
            "Postgraduate Studies",
            "Research & Knowledge Exchange",
            "Innovation, Entrepreneurship, and Commercialization",
            "PDP & Continuing Education",
            "ICT Infrastructure & Maintenance",
            "System Administration",
            "Webmaster",
            "Risk Management & Resource Mobilization",
            "Internationalization",
            "Links & Networks",
            "Gender"
        };
        
        foreach (string unit in coordinationUnits)
        {
            allLocations.Add(new LocationData(unit, $"Coordination unit: {unit}", LocationCategory.CoordinationUnits, LocationType.Unit));
        }
    }
    
    void InitializeFunctionalUnits()
    {
        allLocations.Add(new LocationData("University of Dar es Salaam ICT Innovation Hub (UDICTI)", 
            "ICT innovation hub", LocationCategory.FunctionalUnits, LocationType.Facility));
        allLocations.Add(new LocationData("CoICT Consultancy Bureau (CCB)", 
            "Consultancy services", LocationCategory.FunctionalUnits, LocationType.Facility));
        allLocations.Add(new LocationData("Journal of ICT Systems (JICTS)", 
            "Academic journal", LocationCategory.FunctionalUnits, LocationType.Facility));
    }
    
    void InitializeMainBlocks()
    {
        // Block A
        LocationData blockA = new LocationData("Block A", 
            "Administration Block - Contains: Principal's Office, Coordination Units", 
            LocationCategory.MainBlocks, LocationType.Building,
            new Vector3(0, 0, 0), new Vector3(0, 50, -30));
        blockA.detailedDescription = "Main Administration Block\n\nContains:\n- Principal's Office\n- Coordination Units";
        allLocations.Add(blockA);
        
        // Block B
        LocationData blockB = new LocationData("Block B", 
            "Innovation and Learning Block", 
            LocationCategory.MainBlocks, LocationType.Building,
            new Vector3(20, 0, 0), new Vector3(20, 50, -30));
        blockB.detailedDescription = "Block B\n\nContains:\n- Innovation Spaces (UDICTI, Y4C, FinHub)\n- DHIS2 Lab\n- SmartGrid Lab\n- Blue Economy Research Lab\n- Computer Labs\n- Classes\n- Washrooms\n- UDSM ICT Students' Society\n- CCB\n- Staff Offices\n- Mini Library\n- Registry Repository\n- Faculty Lounge";
        allLocations.Add(blockB);
        
        // Block C
        LocationData blockC = new LocationData("Block C", 
            "Research and Technology Block", 
            LocationCategory.MainBlocks, LocationType.Building,
            new Vector3(-20, 0, 0), new Vector3(-20, 50, -30));
        blockC.detailedDescription = "Block C\n\nContains:\n- Geoinformatics Lab\n- Networking Lab\n- DHIS2 Lab\n- Server Room\n- Staff Offices\n- Washrooms";
        allLocations.Add(blockC);
        
        // Block D
        LocationData blockD = new LocationData("Block D", 
            "Lecture and Laboratory Block", 
            LocationCategory.MainBlocks, LocationType.Building,
            new Vector3(-30, 0, 0), new Vector3(-30, 50, -30));
        blockD.detailedDescription = "Block D\n\nContains:\n- Lecture Theater (Luhanga, D01)\n- Multimedia Studio\n- Telecommunications Lab\n- Electronics Lab\n- Networking Lab\n- 3D Solutions & Robotics Lab\n- E-Learning Research Lab\n- dLab\n- Postgraduate Study Room\n- Staff Offices\n- Washrooms";
        allLocations.Add(blockD);
        
        // Block E
        LocationData blockE = new LocationData("Block E", 
            "Backup Powerhouse", 
            LocationCategory.MainBlocks, LocationType.Building,
            new Vector3(40, 0, 0), new Vector3(40, 50, -30));
        blockE.detailedDescription = "Block E\n\nContains:\n- Backup Powerhouse";
        allLocations.Add(blockE);
    }
    
    void InitializeOtherFeatures()
    {
        allLocations.Add(new LocationData("Hostel", 
            "Main hostel for CoICT and SJMC Students", 
            LocationCategory.OtherFeatures, LocationType.Building,
            new Vector3(0, 0, 20), new Vector3(0, 50, 5)));
        
        allLocations.Add(new LocationData("Playgrounds", 
            "Sports facilities: football, netball, volleyball, tennis", 
            LocationCategory.OtherFeatures, LocationType.Feature));
        
        allLocations.Add(new LocationData("iGrid Demo Site", 
            "iGrid demonstration site", 
            LocationCategory.OtherFeatures, LocationType.Feature));
        
        allLocations.Add(new LocationData("HEET Building", 
            "HEET Building", 
            LocationCategory.OtherFeatures, LocationType.Building));
        
        allLocations.Add(new LocationData("UDSM-Huawei ICT Practice Center", 
            "Huawei ICT Practice Center", 
            LocationCategory.OtherFeatures, LocationType.Facility));
        
        allLocations.Add(new LocationData("Study Areas", 
            "Study and learning areas", 
            LocationCategory.OtherFeatures, LocationType.Feature));
        
        allLocations.Add(new LocationData("Startups Incubation Center", 
            "Startup incubation and support center", 
            LocationCategory.OtherFeatures, LocationType.Facility));
        
        allLocations.Add(new LocationData("Car Parks", 
            "Parking facilities", 
            LocationCategory.OtherFeatures, LocationType.Feature));
    }
    
    void BuildLookupTables()
    {
        locationLookup.Clear();
        categoryLookup.Clear();
        
        foreach (LocationData location in allLocations)
        {
            // Add to name lookup
            if (!string.IsNullOrEmpty(location.name))
            {
                locationLookup[location.name] = location;
            }
            
            // Add to category lookup
            if (!categoryLookup.ContainsKey(location.category))
            {
                categoryLookup[location.category] = new List<LocationData>();
            }
            categoryLookup[location.category].Add(location);
            
            // Recursively add sub-locations
            if (location.HasSubLocations())
            {
                AddSubLocationsToLookup(location);
            }
        }
    }
    
    void AddSubLocationsToLookup(LocationData parent)
    {
        foreach (LocationData subLocation in parent.subLocations)
        {
            if (!string.IsNullOrEmpty(subLocation.name))
            {
                locationLookup[subLocation.name] = subLocation;
            }
        }
    }
    
    /// <summary>
    /// Get a location by name/ID
    /// </summary>
    public LocationData GetLocation(string locationName)
    {
        if (locationLookup.TryGetValue(locationName, out LocationData location))
        {
            return location;
        }
        return null;
    }
    
    /// <summary>
    /// Get all locations in a specific category
    /// </summary>
    public List<LocationData> GetLocationsByCategory(LocationCategory category)
    {
        if (categoryLookup.TryGetValue(category, out List<LocationData> locations))
        {
            return locations;
        }
        return new List<LocationData>();
    }
    
    /// <summary>
    /// Get all top-level locations (no sub-locations included)
    /// </summary>
    public List<LocationData> GetAllTopLevelLocations()
    {
        return allLocations;
    }
    
    /// <summary>
    /// Get all locations flattened (including sub-locations)
    /// </summary>
    public List<LocationData> GetAllLocationsFlattened()
    {
        List<LocationData> result = new List<LocationData>();
        
        foreach (LocationData location in allLocations)
        {
            result.Add(location);
            if (location.HasSubLocations())
            {
                result.AddRange(location.subLocations);
            }
        }
        
        return result;
    }
    
    /// <summary>
    /// Find locations by type
    /// </summary>
    public List<LocationData> GetLocationsByType(LocationType type)
    {
        return GetAllLocationsFlattened().Where(loc => loc.type == type).ToList();
    }
}
























