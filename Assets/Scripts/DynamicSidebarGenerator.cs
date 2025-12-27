using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// Dynamically generates sidebar content from CampusDataManager
/// Creates expandable sections and buttons for all locations
/// </summary>
public class DynamicSidebarGenerator : MonoBehaviour
{
    [Header("References")]
    public CampusDataManager campusDataManager;
    public GameObject sidebarContentParent; // Parent transform where items will be generated
    public SidebarLocationHandler locationHandler; // Handler for location selection
    
    [Header("Prefabs")]
    public GameObject sectionHeaderPrefab; // Prefab for category headers (expandable)
    public GameObject locationButtonPrefab; // Prefab for location buttons
    public GameObject nestedLocationButtonPrefab; // Prefab for nested locations (indented)
    
    [Header("Settings")]
    public float itemSpacing = 5f;
    public float sectionHeaderHeight = 100f; // Doubled from 50f
    public float locationButtonHeight = 100f; // Doubled from 50f
    public Color sectionHeaderColor = new Color(0.2f, 0.4f, 0.8f, 1f);
    public Color locationButtonColor = new Color(0.3f, 0.3f, 0.3f, 1f);
    
    private Dictionary<LocationCategory, GameObject> sectionHeaders = new Dictionary<LocationCategory, GameObject>();
    private Dictionary<LocationCategory, GameObject> sectionContentContainers = new Dictionary<LocationCategory, GameObject>();
    private Dictionary<LocationCategory, bool> sectionExpandedStates = new Dictionary<LocationCategory, bool>();
    
    void Start()
    {
        // DISABLED: Dynamic sidebar generation - using manual buttons instead
        // Uncomment below to re-enable dynamic generation
        
        /*
        if (campusDataManager == null)
        {
            Debug.LogError("[DynamicSidebarGenerator] CampusDataManager not assigned!");
            return;
        }
        
        if (sidebarContentParent == null)
        {
            Debug.LogError("[DynamicSidebarGenerator] Sidebar Content Parent not assigned!");
            return;
        }
        
        // Initialize all sections as expanded by default
        foreach (LocationCategory category in System.Enum.GetValues(typeof(LocationCategory)))
        {
            sectionExpandedStates[category] = true;
        }
        
        GenerateSidebar();
        */
        
        Debug.Log("[DynamicSidebarGenerator] Dynamic generation disabled - using manual buttons with SidebarButtonHandler");
    }
    
    /// <summary>
    /// Generate the entire sidebar structure from CampusDataManager
    /// </summary>
    public void GenerateSidebar()
    {
        // Clear existing content
        ClearSidebar();
        
        // Generate sections for each category
        foreach (LocationCategory category in System.Enum.GetValues(typeof(LocationCategory)))
        {
            List<LocationData> locations = campusDataManager.GetLocationsByCategory(category);
            if (locations.Count > 0)
            {
                CreateCategorySection(category, locations);
            }
        }
        
        Debug.Log("[DynamicSidebarGenerator] Sidebar generated successfully");
    }
    
    void ClearSidebar()
    {
        if (sidebarContentParent == null) return;
        
        // Destroy all children
        for (int i = sidebarContentParent.transform.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(sidebarContentParent.transform.GetChild(i).gameObject);
        }
        
        sectionHeaders.Clear();
        sectionContentContainers.Clear();
    }
    
    void CreateCategorySection(LocationCategory category, List<LocationData> locations)
    {
        // Create section header (expandable)
        GameObject headerObj = CreateSectionHeader(category);
        headerObj.transform.SetParent(sidebarContentParent.transform, false);
        
        // Create content container for this section
        GameObject contentContainer = CreateSectionContentContainer(category);
        contentContainer.transform.SetParent(sidebarContentParent.transform, false);
        contentContainer.SetActive(sectionExpandedStates[category]);
        
        // Add locations to the section
        foreach (LocationData location in locations)
        {
            CreateLocationItem(location, contentContainer.transform, 0);
        }
    }
    
    GameObject CreateSectionHeader(LocationCategory category)
    {
        GameObject headerObj;
        
        if (sectionHeaderPrefab != null)
        {
            headerObj = Instantiate(sectionHeaderPrefab);
        }
        else
        {
            // Create default header if no prefab
            headerObj = new GameObject($"Section_{category}");
            headerObj.AddComponent<RectTransform>();
            
            // Add background
            Image bg = headerObj.AddComponent<Image>();
            bg.color = sectionHeaderColor;
            
            // Add button component
            Button button = headerObj.AddComponent<Button>();
            
            // Add text
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(headerObj.transform, false);
            TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
            
            RectTransform textRect = textObj.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;
            textRect.anchoredPosition = Vector2.zero;
            textRect.offsetMin = new Vector2(10, 0);
            textRect.offsetMax = new Vector2(-30, 0);
            
            // Configure text settings
            text.text = GetCategoryDisplayName(category);
            text.color = Color.white;
            text.fontSize = 40; // Doubled from 20
            text.alignment = TextAlignmentOptions.MidlineLeft;
            text.textWrappingMode = TextWrappingModes.NoWrap; // Disable word wrapping
            text.overflowMode = TextOverflowModes.Ellipsis;
        }
        
        // Setup button to toggle section
        Button headerButton = headerObj.GetComponent<Button>();
        if (headerButton != null)
        {
            LocationCategory capturedCategory = category; // Capture for lambda
            headerButton.onClick.AddListener(() => ToggleSection(capturedCategory));
        }
        
        // Set header size
        RectTransform headerRect = headerObj.GetComponent<RectTransform>();
        headerRect.sizeDelta = new Vector2(0, sectionHeaderHeight);
        
        // Add LayoutElement to ensure proper sizing
        LayoutElement headerLayout = headerObj.GetComponent<LayoutElement>();
        if (headerLayout == null)
        {
            headerLayout = headerObj.AddComponent<LayoutElement>();
        }
        headerLayout.flexibleWidth = 1f;
        headerLayout.minHeight = sectionHeaderHeight;
        headerLayout.preferredHeight = sectionHeaderHeight;
        
        sectionHeaders[category] = headerObj;
        return headerObj;
    }
    
    GameObject CreateSectionContentContainer(LocationCategory category)
    {
        GameObject container = new GameObject($"Content_{category}");
        RectTransform rect = container.AddComponent<RectTransform>();
        
        // Ensure container stretches to fill parent width
        rect.anchorMin = new Vector2(0, 0);
        rect.anchorMax = new Vector2(1, 0);
        rect.pivot = new Vector2(0.5f, 0);
        
        // Setup as vertical layout
        VerticalLayoutGroup layout = container.AddComponent<VerticalLayoutGroup>();
        layout.spacing = itemSpacing;
        layout.childControlHeight = false;
        layout.childControlWidth = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;
        
        ContentSizeFitter sizeFitter = container.AddComponent<ContentSizeFitter>();
        sizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        sizeFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        
        sectionContentContainers[category] = container;
        return container;
    }
    
    void CreateLocationItem(LocationData location, Transform parent, int indentLevel)
    {
        GameObject buttonObj;
        
        if (indentLevel == 0 && locationButtonPrefab != null)
        {
            buttonObj = Instantiate(locationButtonPrefab);
        }
        else if (indentLevel > 0 && nestedLocationButtonPrefab != null)
        {
            buttonObj = Instantiate(nestedLocationButtonPrefab);
        }
        else
        {
            // Create default button if no prefab
            buttonObj = new GameObject($"Button_{location.name}");
            buttonObj.AddComponent<RectTransform>();
            
            Image bg = buttonObj.AddComponent<Image>();
            bg.color = locationButtonColor;
            
            // Add button component (will get reference later)
            buttonObj.AddComponent<Button>();
        }
        
        buttonObj.transform.SetParent(parent, false);
        
        // Set button size and layout FIRST (before text creation for proper width calculation)
        RectTransform buttonRect = buttonObj.GetComponent<RectTransform>();
        buttonRect.anchorMin = new Vector2(0, 0);
        buttonRect.anchorMax = new Vector2(1, 0);
        buttonRect.pivot = new Vector2(0.5f, 0);
        buttonRect.sizeDelta = new Vector2(0, locationButtonHeight); // Width 0 means stretch to fill parent
        
        // Add LayoutElement to ensure proper sizing in layout groups
        LayoutElement layoutElement = buttonObj.GetComponent<LayoutElement>();
        if (layoutElement == null)
        {
            layoutElement = buttonObj.AddComponent<LayoutElement>();
        }
        layoutElement.flexibleWidth = 1f;
        layoutElement.minHeight = locationButtonHeight;
        layoutElement.preferredHeight = locationButtonHeight;
        layoutElement.preferredWidth = -1; // Use flexible width
        
        // Setup button click handler
        Button locationButton = buttonObj.GetComponent<Button>();
        if (locationButton != null)
        {
            LocationData capturedLocation = location; // Capture for lambda
            locationButton.onClick.AddListener(() => OnLocationClicked(capturedLocation));
        }
        
        // Set button text (button RectTransform is now properly sized)
        TextMeshProUGUI text = buttonObj.GetComponentInChildren<TextMeshProUGUI>();
        if (text == null)
        {
            // Try to find Text component
            Text textComponent = buttonObj.GetComponentInChildren<Text>();
            if (textComponent != null)
            {
                // Create TMP text to replace
                GameObject textObj = new GameObject("Text");
                textObj.transform.SetParent(buttonObj.transform, false);
                text = textObj.AddComponent<TextMeshProUGUI>();
                
                RectTransform textRect = textObj.GetComponent<RectTransform>();
                // Set anchors to stretch horizontally and fill button
                textRect.anchorMin = new Vector2(0, 0);
                textRect.anchorMax = new Vector2(1, 1);
                textRect.pivot = new Vector2(0, 0.5f);
                textRect.anchoredPosition = Vector2.zero;
                textRect.sizeDelta = Vector2.zero;
                textRect.offsetMin = new Vector2(10 + (indentLevel * 20), 2);
                textRect.offsetMax = new Vector2(-5, -2);
                
                // Configure text settings BEFORE setting text (critical for proper rendering)
                text.textWrappingMode = TextWrappingModes.NoWrap; // CRITICAL: Disable word wrapping
                text.overflowMode = TextOverflowModes.Ellipsis;
                text.alignment = TextAlignmentOptions.MidlineLeft;
                text.color = Color.white;
                text.fontSize = 36; // Doubled from 18
                text.text = location.displayName ?? location.name;
                
                Destroy(textComponent.gameObject);
            }
            else
            {
                // No text component found, create new one
                GameObject textObj = new GameObject("Text");
                textObj.transform.SetParent(buttonObj.transform, false);
                text = textObj.AddComponent<TextMeshProUGUI>();
                
                RectTransform textRect = textObj.GetComponent<RectTransform>();
                // Set anchors to stretch horizontally and fill button
                textRect.anchorMin = new Vector2(0, 0);
                textRect.anchorMax = new Vector2(1, 1);
                textRect.pivot = new Vector2(0, 0.5f);
                textRect.anchoredPosition = Vector2.zero;
                textRect.sizeDelta = Vector2.zero;
                textRect.offsetMin = new Vector2(10 + (indentLevel * 20), 2);
                textRect.offsetMax = new Vector2(-5, -2);
                
                // Configure text settings BEFORE setting text
                text.textWrappingMode = TextWrappingModes.NoWrap; // CRITICAL: Disable word wrapping
                text.overflowMode = TextOverflowModes.Ellipsis;
                text.alignment = TextAlignmentOptions.MidlineLeft;
                text.color = Color.white;
                text.fontSize = 36; // Doubled from 18
                text.text = location.displayName ?? location.name;
            }
        }
        else
        {
            // Text component already exists, just update settings
            text.text = location.displayName ?? location.name;
            text.textWrappingMode = TextWrappingModes.NoWrap; // Ensure wrapping is disabled
            text.alignment = TextAlignmentOptions.MidlineLeft;
            text.overflowMode = TextOverflowModes.Ellipsis;
            text.fontSize = 36; // Doubled from 18
            
            // Ensure RectTransform is set up correctly
            RectTransform existingTextRect = text.GetComponent<RectTransform>();
            if (existingTextRect != null)
            {
                existingTextRect.anchorMin = new Vector2(0, 0);
                existingTextRect.anchorMax = new Vector2(1, 1);
                existingTextRect.sizeDelta = Vector2.zero;
                existingTextRect.offsetMin = new Vector2(10 + (indentLevel * 20), 0);
                existingTextRect.offsetMax = new Vector2(-5, 0);
                existingTextRect.pivot = new Vector2(0, 0.5f);
            }
        }
        
        // Create nested sub-locations if any
        if (location.HasSubLocations())
        {
            foreach (LocationData subLocation in location.subLocations)
            {
                CreateLocationItem(subLocation, parent, indentLevel + 1);
            }
        }
    }
    
    void ToggleSection(LocationCategory category)
    {
        if (sectionContentContainers.ContainsKey(category))
        {
            bool newState = !sectionExpandedStates[category];
            sectionExpandedStates[category] = newState;
            sectionContentContainers[category].SetActive(newState);
            
            Debug.Log($"[DynamicSidebarGenerator] {category} section {(newState ? "expanded" : "collapsed")}");
        }
    }
    
    void OnLocationClicked(LocationData location)
    {
        Debug.Log($"[DynamicSidebarGenerator] Location clicked: {location.name}");
        
        // Notify location handler
        if (locationHandler != null)
        {
            locationHandler.OnLocationSelected(location);
        }
        else
        {
            Debug.LogWarning("[DynamicSidebarGenerator] LocationHandler not assigned! Cannot handle location selection.");
        }
    }
    
    string GetCategoryDisplayName(LocationCategory category)
    {
        switch (category)
        {
            case LocationCategory.MainOffices:
                return "Main Offices";
            case LocationCategory.CoordinationUnits:
                return "Coordination Units";
            case LocationCategory.FunctionalUnits:
                return "Functional Units";
            case LocationCategory.MainBlocks:
                return "Main Blocks";
            case LocationCategory.OtherFeatures:
                return "Other Features";
            default:
                return category.ToString();
        }
    }
    
    /// <summary>
    /// Public method to refresh sidebar (useful if data changes)
    /// </summary>
    public void RefreshSidebar()
    {
        GenerateSidebar();
    }
}

