using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;

public class SearchManager : MonoBehaviour
{
    [Header("UI References")]
    public InputField searchInputField;
    public GameObject autocompleteDropdown;
    public Transform dropdownContent;
    public GameObject dropdownItemPrefab;
    public Button clearSearchButton;
    
    [Header("Bottom Sheet")]
    public BottomSheetManager bottomSheetManager;
    
    [Header("Building Data")]
    public List<BuildingData> allBuildings = new List<BuildingData>();
    
    [Header("Search Settings")]
    public int minSearchCharacters = 2;
    public int maxDropdownItems = 10;
    
    private List<GameObject> dropdownItems = new List<GameObject>();
    private List<BuildingData> searchResults = new List<BuildingData>();
    private bool isDropdownVisible = false;
    
    void Start()
    {
        InitializeSearch();
        SetupUI();
    }
    
    void InitializeSearch()
    {
        // Initialize with some sample buildings for testing
        if (allBuildings.Count == 0)
        {
            CreateSampleBuildings();
        }
        
        // Hide dropdown initially
        if (autocompleteDropdown != null)
        {
            autocompleteDropdown.SetActive(false);
        }
    }
    
    void SetupUI()
    {
        // Setup search input field
        if (searchInputField != null)
        {
            searchInputField.onValueChanged.AddListener(OnSearchInputChanged);
            searchInputField.placeholder.GetComponent<Text>().text = "Search buildings...";
        }
        
        // Setup clear button
        if (clearSearchButton != null)
        {
            clearSearchButton.onClick.AddListener(ClearSearch);
        }
    }
    
    void CreateSampleBuildings()
    {
        // Create sample building data for testing
        allBuildings.Add(new BuildingData("Block_A", "Main Administration Block", new Vector3(0, 0, 0), new Vector3(0, 10, -15), null));
        allBuildings.Add(new BuildingData("Block_B", "Lecture rooms and Hubs", new Vector3(20, 0, 0), new Vector3(20, 10, -15), null));
        allBuildings.Add(new BuildingData("Block_C", "Computer Labs and Offices", new Vector3(-20, 0, 0), new Vector3(-20, 10, -15), null));
        allBuildings.Add(new BuildingData("Block_D", "Electronics and Telecommunications Labs", new Vector3(-30, 0, 0), new Vector3(-30, 10, -15), null));
        allBuildings.Add(new BuildingData("Hostel", "Main hostel for CoICT and SJMC Students", new Vector3(0, 0, 20), new Vector3(0, 10, 5), null));
        // allBuildings.Add(new BuildingData("Cafeteria", "Student Cafeteria", new Vector3(10, 0, 10), new Vector3(10, 10, -5), null));
        // allBuildings.Add(new BuildingData("Computer Lab", "Computer Laboratory", new Vector3(15, 0, 5), new Vector3(15, 10, -10), null));
        // allBuildings.Add(new BuildingData("Electronics Lab", "Electronics Laboratory", new Vector3(-15, 0, 5), new Vector3(-15, 10, -10), null));
        // allBuildings.Add(new BuildingData("Workshop", "Engineering Workshop", new Vector3(0, 0, -20), new Vector3(0, 10, -35), null));
        // allBuildings.Add(new BuildingData("Auditorium", "Main Auditorium", new Vector3(30, 0, 0), new Vector3(30, 10, -15), null));
        
        Debug.Log($"[SearchManager] Created {allBuildings.Count} sample buildings");
    }
    
    void OnSearchInputChanged(string searchText)
    {
        if (string.IsNullOrEmpty(searchText))
        {
            HideDropdown();
            return;
        }
        
        if (searchText.Length < minSearchCharacters)
        {
            HideDropdown();
            return;
        }
        
        PerformSearch(searchText);
        ShowDropdown();
    }
    
    void PerformSearch(string searchText)
    {
        searchResults.Clear();
        
        // Case-insensitive search
        string lowerSearchText = searchText.ToLower();
        
        // Find matching buildings
        foreach (var building in allBuildings)
        {
            if (building.name.ToLower().Contains(lowerSearchText))
            {
                searchResults.Add(building);
            }
        }
        
        // Sort results by relevance (exact matches first, then partial matches)
        searchResults = searchResults.OrderBy(b => b.name.ToLower().IndexOf(lowerSearchText)).ToList();
        
        // Limit results
        if (searchResults.Count > maxDropdownItems)
        {
            searchResults = searchResults.Take(maxDropdownItems).ToList();
        }
        
        Debug.Log($"[SearchManager] Found {searchResults.Count} matching buildings for '{searchText}'");
    }
    
    void ShowDropdown()
    {
        if (autocompleteDropdown == null || searchResults.Count == 0)
        {
            HideDropdown();
            return;
        }
        
        // Clear existing dropdown items
        ClearDropdownItems();
        
        // Create new dropdown items
        for (int i = 0; i < searchResults.Count; i++)
        {
            CreateDropdownItem(searchResults[i], i);
        }
        
        autocompleteDropdown.SetActive(true);
        isDropdownVisible = true;
    }
    
    void HideDropdown()
    {
        if (autocompleteDropdown != null)
        {
            autocompleteDropdown.SetActive(false);
        }
        isDropdownVisible = false;
    }
    
    void CreateDropdownItem(BuildingData building, int index)
    {
        if (dropdownItemPrefab == null || dropdownContent == null)
        {
            Debug.LogWarning("[SearchManager] Dropdown item prefab or content not assigned!");
            return;
        }
        
        GameObject item = Instantiate(dropdownItemPrefab, dropdownContent);
        dropdownItems.Add(item);
        
        // Setup item text - try multiple approaches
        // First try regular Unity Text component
        Text itemText = item.GetComponentInChildren<Text>();
        if (itemText != null)
        {
            itemText.text = building.name;
            Debug.Log($"[SearchManager] Set dropdown item text (Unity Text) to: {building.name}");
        }
        else
        {
            // Try TextMeshPro Text component
            TextMeshProUGUI tmpText = item.GetComponentInChildren<TextMeshProUGUI>();
            if (tmpText != null)
            {
                tmpText.text = building.name;
                Debug.Log($"[SearchManager] Set dropdown item text (TMP) to: {building.name}");
            }
            else
            {
                Debug.LogWarning($"[SearchManager] Could not find Text or TMP Text component in dropdown item for {building.name}");
            }
        }
        
        // Alternative: Use DropdownItem script if available
        DropdownItem dropdownItemScript = item.GetComponent<DropdownItem>();
        if (dropdownItemScript != null)
        {
            dropdownItemScript.SetText(building.name);
            Debug.Log($"[SearchManager] Set text via DropdownItem script: {building.name}");
        }
        
        // Setup button click
        Button itemButton = item.GetComponent<Button>();
        if (itemButton != null)
        {
            int capturedIndex = index; // Capture index for closure
            itemButton.onClick.AddListener(() => OnBuildingSelected(capturedIndex));
        }
    }
    
    void ClearDropdownItems()
    {
        foreach (GameObject item in dropdownItems)
        {
            if (item != null)
            {
                Destroy(item);
            }
        }
        dropdownItems.Clear();
    }
    
    void OnBuildingSelected(int index)
    {
        if (index >= 0 && index < searchResults.Count)
        {
            BuildingData selectedBuilding = searchResults[index];
            Debug.Log($"[SearchManager] Building selected: {selectedBuilding.name}");
            
            // Hide dropdown
            HideDropdown();
            
            // Clear search field
            if (searchInputField != null)
            {
                searchInputField.text = "";
            }
            
            // Show bottom sheet with building details
            if (bottomSheetManager != null)
            {
                bottomSheetManager.OnBuildingSelected(selectedBuilding);
                Debug.Log($"[SearchManager] Showing bottom sheet for: {selectedBuilding.name}");
            }
            else
            {
                Debug.LogWarning("[SearchManager] BottomSheetManager not assigned!");
            }
        }
    }
    
    void ClearSearch()
    {
        if (searchInputField != null)
        {
            searchInputField.text = "";
        }
        HideDropdown();
        Debug.Log("[SearchManager] Search cleared");
    }
    
    void Update()
    {
        // Hide dropdown when clicking outside
        if (isDropdownVisible && Input.GetMouseButtonDown(0))
        {
            // Check if click is outside the dropdown
            if (!RectTransformUtility.RectangleContainsScreenPoint(
                autocompleteDropdown.GetComponent<RectTransform>(), 
                Input.mousePosition, 
                null))
            {
                HideDropdown();
            }
        }
    }
    
    // Public method to add buildings dynamically
    public void AddBuilding(BuildingData building)
    {
        if (building != null && !allBuildings.Contains(building))
        {
            allBuildings.Add(building);
            Debug.Log($"[SearchManager] Added building: {building.name}");
        }
    }
    
    // Public method to get all buildings
    public List<BuildingData> GetAllBuildings()
    {
        return allBuildings;
    }
}
