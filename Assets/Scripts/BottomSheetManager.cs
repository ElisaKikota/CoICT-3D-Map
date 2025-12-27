using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class BottomSheetManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject bottomSheetPanel;
    public RectTransform bottomSheetRect;
    public TextMeshProUGUI buildingNameText;
    public TextMeshProUGUI buildingDescriptionText;
    public TextMeshProUGUI buildingFeaturesText;
    
    [Header("Action Buttons")]
    public Button closeButton;
    public Button highlightButton;
    public Button goButton; // Keep for future use, but hide it for now
    public Button enterInsideButton; // New button for entering building interiors
    
    [Header("Highlight System")]
    public BuildingHighlightManager highlightManager;
    
    [Header("Navigation UI")]
    public NavigationUIManager navigationUIManager;
    
    [Header("Camera Movement")]
    public CameraMovementController cameraMovementController;
    public CameraModeController cameraModeController;
    
    [Header("Building Entry System")]
    public BuildingEntrySystem buildingEntrySystem;
    
    [Header("Sidebar Button Handler")]
    [Tooltip("Reference to SidebarButtonHandler for glow functionality")]
    public SidebarButtonHandler sidebarButtonHandler;
    
    [Header("Animation Settings")]
    public float slideDuration = 0.3f;
    public AnimationCurve slideCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    [Header("Visual Settings")]
    public Color normalButtonColor = Color.white;
    public Color highlightButtonColor = new Color(0.2f, 0.8f, 1f, 1f);
    public Color goButtonColor = new Color(0.2f, 1f, 0.2f, 1f);
    
    private BuildingData currentBuilding;
    private SidebarButtonHandler currentSidebarButtonHandler;
    private bool isVisible = false;
    private Vector2 hiddenPosition;
    private Vector2 visiblePosition;
    
    void Start()
    {
        InitializeBottomSheet();
        SetupButtons();
    }
    
    void InitializeBottomSheet()
    {
        if (bottomSheetRect == null)
        {
            Debug.LogError("[BottomSheetManager] Bottom sheet RectTransform not assigned!");
            return;
        }
        
        // IMPORTANT: Capture visible position BEFORE moving it (preserve Y position)
        // The bottom sheet should be positioned correctly in the editor where you want it visible
        visiblePosition = bottomSheetRect.anchoredPosition;
        
        // Calculate hidden position - off-screen to the left, but preserve Y position
        hiddenPosition = new Vector2(-bottomSheetRect.rect.width, visiblePosition.y);
        
        // Start hidden (move off-screen to the left)
        bottomSheetRect.anchoredPosition = hiddenPosition;
        
        if (bottomSheetPanel != null)
        {
            bottomSheetPanel.SetActive(false);
        }
        
        Debug.Log($"[BottomSheetManager] Bottom sheet initialized. Visible position: {visiblePosition}, Hidden position: {hiddenPosition}");
    }
    
    void SetupButtons()
    {
        // Setup close button
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(CloseBottomSheet);
        }
        
        // Setup highlight button
        if (highlightButton != null)
        {
            highlightButton.onClick.AddListener(OnHighlightButtonClicked);
        }
        
        // Setup go button
        if (goButton != null)
        {
            goButton.onClick.AddListener(OnGoButtonClicked);
        }
        
        // Setup enter inside button
        if (enterInsideButton != null)
        {
            enterInsideButton.onClick.AddListener(OnEnterInsideButtonClicked);
        }
        
        // Find building entry system if not assigned
        if (buildingEntrySystem == null)
        {
            buildingEntrySystem = FindFirstObjectByType<BuildingEntrySystem>();
        }
    }
    
    public void ShowBuildingDetails(BuildingData building, SidebarButtonHandler handler = null)
    {
        if (building == null)
        {
            Debug.LogWarning("[BottomSheetManager] Cannot show details for null building");
            return;
        }
        
        Debug.Log($"[BottomSheetManager] ShowBuildingDetails called for: {building.name}, Description: '{building.description}', Features: '{building.features}'");
        
        currentBuilding = building;
        currentSidebarButtonHandler = handler;
        
        // Update UI content
        UpdateBuildingInfo(building);
        
        // Show bottom sheet with animation (slide from left)
        StartCoroutine(SlideIn());
        
        Debug.Log($"[BottomSheetManager] Bottom sheet slide animation started for: {building.name}");
    }
    
    void UpdateBuildingInfo(BuildingData building)
    {
        Debug.Log($"[BottomSheetManager] Updating UI - Name: '{building.name}', Description: '{building.description}', Features: '{building.features}'");
        
        // Update building name
        if (buildingNameText != null)
        {
            buildingNameText.text = building.name;
            Debug.Log($"[BottomSheetManager] Set buildingNameText to: '{building.name}'");
        }
        else
        {
            Debug.LogWarning("[BottomSheetManager] buildingNameText is null!");
        }
        
        // Update building description
        if (buildingDescriptionText != null)
        {
            buildingDescriptionText.text = building.description;
            Debug.Log($"[BottomSheetManager] Set buildingDescriptionText to: '{building.description}'");
        }
        else
        {
            Debug.LogWarning("[BottomSheetManager] buildingDescriptionText is null!");
        }
        
        // Update building features
        if (buildingFeaturesText != null)
        {
            buildingFeaturesText.text = !string.IsNullOrEmpty(building.features) ? building.features : "";
            Debug.Log($"[BottomSheetManager] Set buildingFeaturesText to: '{buildingFeaturesText.text}'");
        }
        else
        {
            Debug.LogWarning("[BottomSheetManager] buildingFeaturesText is null!");
        }
        
        // Hide go button, only show highlight button
        if (goButton != null)
        {
            goButton.gameObject.SetActive(false);
        }
        
        if (highlightButton != null)
        {
            highlightButton.gameObject.SetActive(true);
        }
        
        // Show "Enter Inside" button if building has door position set OR has interior
        // This allows the button to show even if interior isn't set up yet (just moves camera to door)
        if (enterInsideButton != null)
        {
            // Check if the handler wants to show the enter building button
            bool handlerWantsToShow = currentSidebarButtonHandler != null && currentSidebarButtonHandler.showEnterBuilding;
            
            // Also check if building has door position or interior (for backwards compatibility when handler is null)
            bool hasDoorPosition = building.enterBuildingCameraPosition != Vector3.zero;
            bool hasInterior = buildingEntrySystem != null && buildingEntrySystem.BuildingHasInterior(building.name);
            
            // Show button if handler explicitly wants it, OR if no handler is assigned and building has door/interior (legacy support)
            bool shouldShow = handlerWantsToShow || (currentSidebarButtonHandler == null && (hasDoorPosition || hasInterior));
            enterInsideButton.gameObject.SetActive(shouldShow);
        }
        
        // Update button colors
        UpdateButtonColors();
    }
    
    void UpdateButtonColors()
    {
        // Update highlight button color
        if (highlightButton != null)
        {
            ColorBlock highlightColors = highlightButton.colors;
            highlightColors.normalColor = highlightButtonColor;
            highlightButton.colors = highlightColors;
        }
        
        // Update go button color
        if (goButton != null)
        {
            ColorBlock goColors = goButton.colors;
            goColors.normalColor = goButtonColor;
            goButton.colors = goColors;
        }
    }
    
    IEnumerator SlideIn()
    {
        if (bottomSheetPanel != null)
        {
            bottomSheetPanel.SetActive(true);
        }
        
        isVisible = true;
        
        Vector2 startPos = hiddenPosition;
        Vector2 endPos = visiblePosition;
        
        float elapsedTime = 0f;
        
        while (elapsedTime < slideDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / slideDuration;
            float curveValue = slideCurve.Evaluate(progress);
            
            bottomSheetRect.anchoredPosition = Vector2.Lerp(startPos, endPos, curveValue);
            
            yield return null;
        }
        
        bottomSheetRect.anchoredPosition = endPos;
        Debug.Log("[BottomSheetManager] Bottom sheet slide in from left completed");
    }
    
    IEnumerator SlideOut()
    {
        Vector2 startPos = bottomSheetRect.anchoredPosition;
        Vector2 endPos = hiddenPosition;
        
        float elapsedTime = 0f;
        
        while (elapsedTime < slideDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / slideDuration;
            float curveValue = slideCurve.Evaluate(progress);
            
            bottomSheetRect.anchoredPosition = Vector2.Lerp(startPos, endPos, curveValue);
            
            yield return null;
        }
        
        bottomSheetRect.anchoredPosition = endPos;
        
        if (bottomSheetPanel != null)
        {
            bottomSheetPanel.SetActive(false);
        }
        
        isVisible = false;
        Debug.Log("[BottomSheetManager] Bottom sheet slide out to left completed");
    }
    
    public void CloseBottomSheet()
    {
        if (isVisible)
        {
            // Reset highlight if active
            if (highlightManager != null && highlightManager.IsHighlightMode())
            {
                highlightManager.ResetAllBuildings();
                Debug.Log("[BottomSheetManager] Reset highlight when closing bottom sheet");
            }
            
            StartCoroutine(SlideOut());
            Debug.Log("[BottomSheetManager] Bottom sheet closed by user");
            
            // Notify NavigationUIManager to show joysticks
            if (navigationUIManager != null)
            {
                navigationUIManager.OnBottomSheetClosed();
            }
        }
    }
    
    void OnHighlightButtonClicked()
    {
        // Use the current sidebar button handler (set when showing building details)
        SidebarButtonHandler handlerToUse = currentSidebarButtonHandler != null 
            ? currentSidebarButtonHandler 
            : sidebarButtonHandler;
            
        if (handlerToUse != null)
        {
            Debug.Log($"[BottomSheetManager] Highlight button clicked for: {currentBuilding?.name}");
            
            // Ensure the handler's GameObject is active before calling (coroutines need active GameObjects)
            if (handlerToUse.gameObject.activeInHierarchy)
            {
                handlerToUse.StartHighlightGlow();
            }
            else
            {
                Debug.LogWarning($"[BottomSheetManager] Cannot start glow - SidebarButtonHandler GameObject '{handlerToUse.gameObject.name}' is inactive!");
            }
        }
        else
        {
            Debug.LogWarning("[BottomSheetManager] Cannot highlight - SidebarButtonHandler not assigned!");
        }
    }
    
    GameObject FindBuildingObject(BuildingData building)
    {
        // Try to find building by name
        GameObject foundBuilding = GameObject.Find(building.name);
        if (foundBuilding != null)
        {
            return foundBuilding;
        }
        
        // If not found by name, try to find by position (with tolerance)
        float tolerance = 1f;
        foreach (GameObject obj in GameObject.FindObjectsByType<GameObject>(FindObjectsSortMode.None))
        {
            if (Vector3.Distance(obj.transform.position, building.position) < tolerance)
            {
                return obj;
            }
        }
        
        return null;
    }
    
    void OnGoButtonClicked()
    {
        if (currentBuilding != null)
        {
            Debug.Log($"[BottomSheetManager] Go button clicked for: {currentBuilding.name}");
            
            // Use camera movement controller if available
            if (cameraMovementController != null)
            {
                // Check if we're in drone mode for two-phase movement
                bool useDronePath = false;
                if (cameraModeController != null && cameraModeController.currentMode == TourMode.Drone)
                {
                    useDronePath = true;
                }
                
                cameraMovementController.MoveToBuilding(currentBuilding, useDronePath);
                Debug.Log($"[BottomSheetManager] Moving camera to: {currentBuilding.cameraViewPosition}");
            }
            else
            {
                Debug.LogWarning("[BottomSheetManager] CameraMovementController not assigned!");
            }
        }
    }
    
    void OnEnterInsideButtonClicked()
    {
        if (currentBuilding != null && buildingEntrySystem != null)
        {
            Debug.Log($"[BottomSheetManager] Enter Inside button clicked for: {currentBuilding.name}");
            buildingEntrySystem.OnEnterInsideButtonClicked(currentBuilding);
        }
        else
        {
            Debug.LogWarning("[BottomSheetManager] Cannot enter building - building data or entry system is null!");
        }
    }
    
    // Public methods for external control
    public bool IsVisible()
    {
        return isVisible;
    }
    
    public BuildingData GetCurrentBuilding()
    {
        return currentBuilding;
    }
    
    // Method to be called from SearchManager
    public void OnBuildingSelected(BuildingData building)
    {
        ShowBuildingDetails(building);
    }
    
    // Emergency method to fix pink materials
    public void EmergencyResetMaterials()
    {
        if (highlightManager != null)
        {
            highlightManager.EmergencyResetAllBuildings();
            Debug.Log("[BottomSheetManager] Emergency material reset called");
        }
        else
        {
            Debug.LogWarning("[BottomSheetManager] BuildingHighlightManager not assigned for emergency reset!");
        }
    }
}
