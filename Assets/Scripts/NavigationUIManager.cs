using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class NavigationUIManager : MonoBehaviour
{
    [Header("Top Bar")]
    [SerializeField] private Button buildingIconButton;
    [SerializeField] private Button icon2DButton;
    [SerializeField] private Button iconDroneButton;
    [SerializeField] private Button iconWalkButton;
    
    [Header("Sidebar (Dropdown)")]
    [SerializeField] private GameObject sidebar;
    [SerializeField] private RectTransform sidebarRect;
    
    [Header("Old Sidebar (Legacy - not used)")]
    [SerializeField] private GameObject oldSidebarPanel;
    [SerializeField] private RectTransform oldSidebarRect;
    [SerializeField] private Button closeSidebarButton;
    
    [Header("Blocks Section")]
    [SerializeField] private Button blocksHeaderButton;
    [SerializeField] private GameObject blocksContent;
    [SerializeField] private Button blockAButton;
    [SerializeField] private Button blockBButton;
    [SerializeField] private Button blockCButton;
    [SerializeField] private Button blockDButton;
    [SerializeField] private Button cafeButton;
    [SerializeField] private Button hostelButton;
    
    [Header("Facilities Section")]
    [SerializeField] private Button facilitiesHeaderButton;
    [SerializeField] private GameObject facilitiesContent;
    [SerializeField] private Button administrationButton;
    [SerializeField] private Button departmentsButton;
    [SerializeField] private Button lectureTheatresButton;
    [SerializeField] private Button hostelsButton;
    [SerializeField] private Button cafeFacilityButton;
    [SerializeField] private Button washroomsButton;
    [SerializeField] private Button studyingAreasButton;
    
    [Header("Bottom Sheet")]
    [SerializeField] private BottomSheetManager bottomSheetManager;
    
    [Header("Joystick Controls")]
    [SerializeField] private GameObject joystickContainer;
    
    [Header("Camera Mode Controller")]
    [SerializeField] private CameraModeController cameraModeController;
    
    [Header("Animation Settings")]
    [SerializeField] private float sidebarPanelSlideDuration = 0.3f;
    [SerializeField] private float sidebarSlideDuration = 0.3f;
    [SerializeField] private float joystickHideDuration = 0.2f;
    [SerializeField] private AnimationCurve slideCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    [Header("Building Data")]
    [SerializeField] private List<BuildingData> buildingDatabase = new List<BuildingData>();
    
    private bool isSidebarPanelOpen = false;
    private bool isSidebarOpen = false;
    private bool isBlocksExpanded = true; // Default open
    private bool isFacilitiesExpanded = false; // Default closed
    private Vector2 sidebarPanelHiddenPosition;
    private Vector2 sidebarPanelVisiblePosition;
    private Vector2 sidebarHiddenPosition; // Position when hidden (off-screen left)
    private Vector2 sidebarVisiblePosition; // Position when visible (current position)
    private Coroutine sidebarPanelCoroutine;
    private Coroutine sidebarSlideCoroutine;
    private Coroutine joystickHideCoroutine;
    
    // Building data mapping
    private Dictionary<string, BuildingData> buildingDataMap = new Dictionary<string, BuildingData>();
    
    void Start()
    {
        InitializeNavigation();
        SetupButtons();
        InitializeBuildingData();
    }
    
    void InitializeNavigation()
    {
        // Initialize old sidebar positions (legacy - may not be used)
        if (oldSidebarRect != null)
        {
            float oldSidebarWidth = oldSidebarRect.rect.width;
            sidebarPanelHiddenPosition = new Vector2(oldSidebarWidth, 0);
            sidebarPanelVisiblePosition = Vector2.zero;
            
            // Start with old sidebar hidden
            oldSidebarRect.anchoredPosition = sidebarPanelHiddenPosition;
            if (oldSidebarPanel != null)
            {
                oldSidebarPanel.SetActive(false);
            }
        }
        
        // NOTE: Sidebar initialization is handled by CameraModeController
        // NavigationUIManager should NOT initialize sidebar to avoid conflicts
        // Only initialize if CameraModeController is not present
        if (cameraModeController == null)
        {
            // Initialize sidebar (renamed from AutocompleteDropdown) positions
            // Try to get RectTransform if not directly assigned
            if (sidebarRect == null && sidebar != null)
            {
                sidebarRect = sidebar.GetComponent<RectTransform>();
            }
            
            if (sidebarRect != null)
            {
                // Store the current position as the visible position (assumes sidebar is positioned correctly in editor)
                sidebarVisiblePosition = sidebarRect.anchoredPosition;
                
                // Calculate hidden position (off-screen to the left)
                Canvas canvas = sidebarRect.GetComponentInParent<Canvas>();
                float screenWidth = canvas != null && canvas.renderMode == RenderMode.ScreenSpaceOverlay 
                    ? Screen.width 
                    : sidebarRect.rect.width * 2;
                
                sidebarHiddenPosition = new Vector2(-screenWidth, sidebarVisiblePosition.y);
                
                // Start with sidebar hidden (move off-screen left and deactivate)
                sidebarRect.anchoredPosition = sidebarHiddenPosition;
                if (sidebar != null)
                {
                    sidebar.SetActive(false);
                }
                
                Debug.Log($"[NavigationUIManager] Sidebar initialized - Visible: {sidebarVisiblePosition}, Hidden: {sidebarHiddenPosition}, Screen Width: {screenWidth}");
            }
            else
            {
                Debug.LogWarning("[NavigationUIManager] Sidebar RectTransform not found! Building button toggle will not work.");
            }
        }
        else
        {
            Debug.Log("[NavigationUIManager] CameraModeController found - skipping sidebar initialization to avoid conflicts");
        }
        
        // Set initial states
        if (blocksContent != null)
        {
            blocksContent.SetActive(isBlocksExpanded);
        }
        
        if (facilitiesContent != null)
        {
            facilitiesContent.SetActive(isFacilitiesExpanded);
        }
        
        // Ensure 2D mode is active on startup
        if (cameraModeController != null)
        {
            cameraModeController.SwitchMode(TourMode.Mode2D);
            Debug.Log("[NavigationUIManager] Set initial mode to 2D");
        }
        
        Debug.Log("[NavigationUIManager] Navigation system initialized");
    }
    
    void InitializeBuildingData()
    {
        // Create building data for blocks
        buildingDataMap["Block_A"] = new BuildingData("Block_A", "Main Administration Block", new Vector3(0, 0, 0), new Vector3(0, 10, -15), null);
        buildingDataMap["Block_B"] = new BuildingData("Block_B", "Lecture rooms and Hubs", new Vector3(20, 0, 0), new Vector3(20, 10, -15), null);
        buildingDataMap["Block_C"] = new BuildingData("Block_C", "Computer Labs and Offices", new Vector3(-20, 0, 0), new Vector3(-20, 10, -15), null);
        buildingDataMap["Block_D"] = new BuildingData("Block_D", "Electronics and Telecommunications Labs", new Vector3(-30, 0, 0), new Vector3(-30, 10, -15), null);
        buildingDataMap["Cafe"] = new BuildingData("Cafe", "Student Cafeteria", new Vector3(10, 0, 10), new Vector3(10, 10, -5), null);
        buildingDataMap["Hostel"] = new BuildingData("Hostel", "Main hostel for CoICT and SJMC Students", new Vector3(0, 0, 20), new Vector3(0, 10, 5), null);
        
        // Add to database
        buildingDatabase.AddRange(buildingDataMap.Values);
        
        Debug.Log($"[NavigationUIManager] Initialized {buildingDataMap.Count} buildings");
    }
    
    void SetupButtons()
    {
        // Top bar buttons
        // Only add sidebar toggle listener if CameraModeController doesn't exist
        // (CameraModeController handles sidebar management when present)
        if (buildingIconButton != null && cameraModeController == null)
        {
            buildingIconButton.onClick.AddListener(ToggleSidebar);
            Debug.Log("[NavigationUIManager] Added sidebar toggle listener to building icon button (CameraModeController not found)");
        }
        else if (buildingIconButton != null && cameraModeController != null)
        {
            Debug.Log("[NavigationUIManager] Skipping sidebar toggle listener - CameraModeController will handle it");
        }
        
        if (icon2DButton != null)
        {
            icon2DButton.onClick.AddListener(() => OnModeButtonClicked(TourMode.Mode2D));
        }
        
        if (iconDroneButton != null)
        {
            iconDroneButton.onClick.AddListener(() => OnModeButtonClicked(TourMode.Drone));
        }
        
        if (iconWalkButton != null)
        {
            iconWalkButton.onClick.AddListener(() => OnModeButtonClicked(TourMode.Walk));
        }
        
        // Old sidebar close button (legacy)
        if (closeSidebarButton != null)
        {
            closeSidebarButton.onClick.AddListener(CloseOldSidebar);
        }
        
        // Blocks section
        if (blocksHeaderButton != null)
        {
            blocksHeaderButton.onClick.AddListener(ToggleBlocksSection);
        }
        
        // Facilities section
        if (facilitiesHeaderButton != null)
        {
            facilitiesHeaderButton.onClick.AddListener(ToggleFacilitiesSection);
        }
        
        // Block buttons - check if they have SidebarButtonHandler before adding listener
        if (blockAButton != null)
        {
            // Only add listener if button doesn't have SidebarButtonHandler (let handler manage everything)
            if (blockAButton.GetComponent<SidebarButtonHandler>() == null)
            {
                blockAButton.onClick.AddListener(() => OnBuildingSelected("Block_A"));
            }
        }
        
        if (blockBButton != null)
        {
            if (blockBButton.GetComponent<SidebarButtonHandler>() == null)
            {
                blockBButton.onClick.AddListener(() => OnBuildingSelected("Block_B"));
            }
        }
        
        if (blockCButton != null)
        {
            if (blockCButton.GetComponent<SidebarButtonHandler>() == null)
            {
                blockCButton.onClick.AddListener(() => OnBuildingSelected("Block_C"));
            }
        }
        
        if (blockDButton != null)
        {
            if (blockDButton.GetComponent<SidebarButtonHandler>() == null)
            {
                blockDButton.onClick.AddListener(() => OnBuildingSelected("Block_D"));
            }
        }
        
        if (cafeButton != null)
        {
            if (cafeButton.GetComponent<SidebarButtonHandler>() == null)
            {
                cafeButton.onClick.AddListener(() => OnBuildingSelected("Cafe"));
            }
        }
        
        if (hostelButton != null)
        {
            if (hostelButton.GetComponent<SidebarButtonHandler>() == null)
            {
                hostelButton.onClick.AddListener(() => OnBuildingSelected("Hostel"));
            }
        }
        
        // Facilities buttons (for future use)
        if (administrationButton != null)
        {
            administrationButton.onClick.AddListener(() => OnFacilitySelected("Administration"));
        }
        
        if (departmentsButton != null)
        {
            departmentsButton.onClick.AddListener(() => OnFacilitySelected("Departments"));
        }
        
        if (lectureTheatresButton != null)
        {
            lectureTheatresButton.onClick.AddListener(() => OnFacilitySelected("Lecture Theatres"));
        }
        
        if (hostelsButton != null)
        {
            hostelsButton.onClick.AddListener(() => OnFacilitySelected("Hostels"));
        }
        
        if (cafeFacilityButton != null)
        {
            cafeFacilityButton.onClick.AddListener(() => OnFacilitySelected("Cafe"));
        }
        
        if (washroomsButton != null)
        {
            washroomsButton.onClick.AddListener(() => OnFacilitySelected("Washrooms"));
        }
        
        if (studyingAreasButton != null)
        {
            studyingAreasButton.onClick.AddListener(() => OnFacilitySelected("Studying Areas"));
        }
    }
    
    void ToggleSidebar()
    {
        // Don't toggle in interior mode
        InteriorExteriorManager interiorManager = FindFirstObjectByType<InteriorExteriorManager>();
        if (interiorManager != null && interiorManager.IsInteriorMode())
        {
            Debug.Log("[NavigationUIManager] Sidebar toggle blocked - in interior mode");
            return;
        }
        
        if (isSidebarOpen)
        {
            CloseSidebar();
        }
        else
        {
            OpenSidebar();
        }
    }
    
    void OpenSidebar()
    {
        // Don't show sidebar in interior mode
        InteriorExteriorManager interiorManager = FindFirstObjectByType<InteriorExteriorManager>();
        if (interiorManager != null && interiorManager.IsInteriorMode())
        {
            Debug.Log($"[NavigationUIManager] Sidebar not shown - in interior mode (isInteriorMode: {interiorManager.IsInteriorMode()})");
            return;
        }
        
        if (sidebar == null || sidebarRect == null) 
        {
            Debug.LogWarning("[NavigationUIManager] Sidebar or RectTransform not assigned!");
            return;
        }
        
        // If already open, don't do anything
        if (isSidebarOpen)
        {
            Debug.Log("[NavigationUIManager] Sidebar already open, ignoring open request");
            return;
        }
        
        if (sidebarSlideCoroutine != null)
        {
            StopCoroutine(sidebarSlideCoroutine);
            sidebarSlideCoroutine = null;
        }
        
        // Set the visible position to the specified coordinates
        // With top-left anchor: X positive = right, Y positive = down
        // User wants x=17 (right), y=-257 (up from top, so negative)
        sidebarVisiblePosition = new Vector2(17f, -257f);
        sidebarHiddenPosition = new Vector2(-2000f, sidebarVisiblePosition.y);
        
        sidebar.SetActive(true);
        sidebarSlideCoroutine = StartCoroutine(SlideSidebar(sidebarHiddenPosition, sidebarVisiblePosition));
        isSidebarOpen = true;
        
        Debug.Log($"[NavigationUIManager] Sidebar opened - Target position: {sidebarVisiblePosition}");
    }
    
    void CloseSidebar()
    {
        if (sidebarRect == null) 
        {
            Debug.LogWarning("[NavigationUIManager] Sidebar RectTransform not assigned!");
            return;
        }
        
        if (sidebarSlideCoroutine != null)
        {
            StopCoroutine(sidebarSlideCoroutine);
        }
        
        sidebarSlideCoroutine = StartCoroutine(SlideSidebar(sidebarVisiblePosition, sidebarHiddenPosition));
        isSidebarOpen = false;
        
        Debug.Log("[NavigationUIManager] Sidebar closed");
    }
    
    IEnumerator SlideSidebar(Vector2 startPos, Vector2 endPos)
    {
        float elapsedTime = 0f;
        
        while (elapsedTime < sidebarSlideDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / sidebarSlideDuration;
            float curveValue = slideCurve.Evaluate(progress);
            
            sidebarRect.anchoredPosition = Vector2.Lerp(startPos, endPos, curveValue);
            
            yield return null;
        }
        
        sidebarRect.anchoredPosition = endPos;
        
        // Hide sidebar GameObject when fully closed
        if (!isSidebarOpen && sidebar != null)
        {
            sidebar.SetActive(false);
        }
        
        sidebarSlideCoroutine = null;
    }
    
    void ToggleOldSidebar()
    {
        if (isSidebarPanelOpen)
        {
            CloseOldSidebar();
        }
        else
        {
            OpenOldSidebar();
        }
    }
    
    void OpenOldSidebar()
    {
        if (oldSidebarPanel == null || oldSidebarRect == null) return;
        
        if (sidebarPanelCoroutine != null)
        {
            StopCoroutine(sidebarPanelCoroutine);
        }
        
        oldSidebarPanel.SetActive(true);
        sidebarPanelCoroutine = StartCoroutine(SlideOldSidebar(sidebarPanelHiddenPosition, sidebarPanelVisiblePosition));
        isSidebarPanelOpen = true;
        
        Debug.Log("[NavigationUIManager] Old sidebar opened");
    }
    
    void CloseOldSidebar()
    {
        if (oldSidebarRect == null) return;
        
        if (sidebarPanelCoroutine != null)
        {
            StopCoroutine(sidebarPanelCoroutine);
        }
        
        sidebarPanelCoroutine = StartCoroutine(SlideOldSidebar(sidebarPanelVisiblePosition, sidebarPanelHiddenPosition));
        isSidebarPanelOpen = false;
        
        Debug.Log("[NavigationUIManager] Old sidebar closed");
    }
    
    IEnumerator SlideOldSidebar(Vector2 startPos, Vector2 endPos)
    {
        float elapsedTime = 0f;
        
        while (elapsedTime < sidebarPanelSlideDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / sidebarPanelSlideDuration;
            float curveValue = slideCurve.Evaluate(progress);
            
            oldSidebarRect.anchoredPosition = Vector2.Lerp(startPos, endPos, curveValue);
            
            yield return null;
        }
        
        oldSidebarRect.anchoredPosition = endPos;
        
        // Hide panel when fully closed
        if (!isSidebarPanelOpen && oldSidebarPanel != null)
        {
            oldSidebarPanel.SetActive(false);
        }
        
        sidebarPanelCoroutine = null;
    }
    
    void ToggleBlocksSection()
    {
        isBlocksExpanded = !isBlocksExpanded;
        if (blocksContent != null)
        {
            blocksContent.SetActive(isBlocksExpanded);
        }
        
        Debug.Log($"[NavigationUIManager] Blocks section {(isBlocksExpanded ? "expanded" : "collapsed")}");
    }
    
    void ToggleFacilitiesSection()
    {
        isFacilitiesExpanded = !isFacilitiesExpanded;
        if (facilitiesContent != null)
        {
            facilitiesContent.SetActive(isFacilitiesExpanded);
        }
        
        Debug.Log($"[NavigationUIManager] Facilities section {(isFacilitiesExpanded ? "expanded" : "collapsed")}");
    }
    
    void OnBuildingSelected(string buildingName)
    {
        Debug.Log($"[NavigationUIManager] Building selected: {buildingName}");
        
        // Close sidebar
        CloseSidebar();
        
        // Find building data
        if (buildingDataMap.ContainsKey(buildingName))
        {
            BuildingData building = buildingDataMap[buildingName];
            
            // Show bottom sheet with building details
            if (bottomSheetManager != null)
            {
                bottomSheetManager.OnBuildingSelected(building);
                
                // Hide joystick controls smoothly
                HideJoystickControls();
            }
            else
            {
                Debug.LogWarning("[NavigationUIManager] BottomSheetManager not assigned!");
            }
        }
        else
        {
            Debug.LogWarning($"[NavigationUIManager] Building '{buildingName}' not found in database!");
        }
    }
    
    void OnFacilitySelected(string facilityName)
    {
        Debug.Log($"[NavigationUIManager] Facility selected: {facilityName}");
        // TODO: Implement facility selection logic (for future levels)
    }
    
    void OnModeButtonClicked(TourMode mode)
    {
        Debug.Log($"[NavigationUIManager] Mode button clicked: {mode}");
        
        // Use CameraModeController to switch mode
        if (cameraModeController != null)
        {
            switch (mode)
            {
                case TourMode.Mode2D:
                    cameraModeController.On2DButton();
                    break;
                case TourMode.Drone:
                    cameraModeController.OnDroneButton();
                    break;
                case TourMode.Walk:
                    cameraModeController.OnWalkButton();
                    break;
            }
            Debug.Log($"[NavigationUIManager] Switching to {mode} mode");
        }
        else
        {
            Debug.LogWarning("[NavigationUIManager] CameraModeController not assigned!");
        }
    }
    
    void HideJoystickControls()
    {
        if (joystickContainer == null) return;
        
        if (joystickHideCoroutine != null)
        {
            StopCoroutine(joystickHideCoroutine);
        }
        
        joystickHideCoroutine = StartCoroutine(FadeOutJoysticks());
    }
    
    void ShowJoystickControls()
    {
        if (joystickContainer == null) return;
        
        if (joystickHideCoroutine != null)
        {
            StopCoroutine(joystickHideCoroutine);
        }
        
        joystickHideCoroutine = StartCoroutine(FadeInJoysticks());
    }
    
    IEnumerator FadeOutJoysticks()
    {
        CanvasGroup canvasGroup = joystickContainer.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = joystickContainer.AddComponent<CanvasGroup>();
        }
        
        float elapsedTime = 0f;
        float startAlpha = canvasGroup.alpha;
        
        while (elapsedTime < joystickHideDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / joystickHideDuration;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, progress);
            yield return null;
        }
        
        canvasGroup.alpha = 0f;
        joystickContainer.SetActive(false);
        joystickHideCoroutine = null;
    }
    
    IEnumerator FadeInJoysticks()
    {
        if (joystickContainer == null) yield break;
        
        joystickContainer.SetActive(true);
        
        CanvasGroup canvasGroup = joystickContainer.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = joystickContainer.AddComponent<CanvasGroup>();
        }
        
        float elapsedTime = 0f;
        float startAlpha = canvasGroup.alpha;
        
        while (elapsedTime < joystickHideDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / joystickHideDuration;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, 1f, progress);
            yield return null;
        }
        
        canvasGroup.alpha = 1f;
        joystickHideCoroutine = null;
    }
    
    // Public method to show joysticks when bottom sheet closes
    public void OnBottomSheetClosed()
    {
        // Only show joysticks if we're in Drone or Walk mode
        if (cameraModeController != null)
        {
            TourMode currentMode = cameraModeController.currentMode;
            if (currentMode == TourMode.Drone || currentMode == TourMode.Walk)
            {
                ShowJoystickControls();
            }
        }
    }
    
    // Method to get building data
    public BuildingData GetBuildingData(string buildingName)
    {
        if (buildingDataMap.ContainsKey(buildingName))
        {
            return buildingDataMap[buildingName];
        }
        return null;
    }
}

