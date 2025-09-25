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
    public TextMeshProUGUI buildingLocationText;
    
    [Header("Action Buttons")]
    public Button closeButton;
    public Button highlightButton;
    public Button goButton;
    
    [Header("Highlight System")]
    public BuildingHighlightManager highlightManager;
    
    [Header("Animation Settings")]
    public float slideUpDuration = 0.3f;
    public float slideDownDuration = 0.2f;
    public AnimationCurve slideUpCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    public AnimationCurve slideDownCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    [Header("Visual Settings")]
    public Color normalButtonColor = Color.white;
    public Color highlightButtonColor = new Color(0.2f, 0.8f, 1f, 1f);
    public Color goButtonColor = new Color(0.2f, 1f, 0.2f, 1f);
    
    private BuildingData currentBuilding;
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
        
        // Calculate positions
        hiddenPosition = new Vector2(0, -bottomSheetRect.rect.height);
        visiblePosition = Vector2.zero;
        
        // Start hidden
        bottomSheetRect.anchoredPosition = hiddenPosition;
        
        if (bottomSheetPanel != null)
        {
            bottomSheetPanel.SetActive(false);
        }
        
        Debug.Log("[BottomSheetManager] Bottom sheet initialized and hidden");
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
    }
    
    public void ShowBuildingDetails(BuildingData building)
    {
        if (building == null)
        {
            Debug.LogWarning("[BottomSheetManager] Cannot show details for null building");
            return;
        }
        
        currentBuilding = building;
        
        // Update UI content
        UpdateBuildingInfo(building);
        
        // Show bottom sheet with animation
        StartCoroutine(SlideUp());
        
        Debug.Log($"[BottomSheetManager] Showing details for: {building.name}");
    }
    
    void UpdateBuildingInfo(BuildingData building)
    {
        // Update building name
        if (buildingNameText != null)
        {
            buildingNameText.text = building.name;
        }
        
        // Update building description
        if (buildingDescriptionText != null)
        {
            buildingDescriptionText.text = building.description;
        }
        
        // Update building location
        if (buildingLocationText != null)
        {
            buildingLocationText.text = $"Position: {building.position.x:F1}, {building.position.y:F1}, {building.position.z:F1}";
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
    
    IEnumerator SlideUp()
    {
        if (bottomSheetPanel != null)
        {
            bottomSheetPanel.SetActive(true);
        }
        
        isVisible = true;
        
        Vector2 startPos = hiddenPosition;
        Vector2 endPos = visiblePosition;
        
        float elapsedTime = 0f;
        
        while (elapsedTime < slideUpDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / slideUpDuration;
            float curveValue = slideUpCurve.Evaluate(progress);
            
            bottomSheetRect.anchoredPosition = Vector2.Lerp(startPos, endPos, curveValue);
            
            yield return null;
        }
        
        bottomSheetRect.anchoredPosition = endPos;
        Debug.Log("[BottomSheetManager] Bottom sheet slide up completed");
    }
    
    IEnumerator SlideDown()
    {
        Vector2 startPos = bottomSheetRect.anchoredPosition;
        Vector2 endPos = hiddenPosition;
        
        float elapsedTime = 0f;
        
        while (elapsedTime < slideDownDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / slideDownDuration;
            float curveValue = slideDownCurve.Evaluate(progress);
            
            bottomSheetRect.anchoredPosition = Vector2.Lerp(startPos, endPos, curveValue);
            
            yield return null;
        }
        
        bottomSheetRect.anchoredPosition = endPos;
        
        if (bottomSheetPanel != null)
        {
            bottomSheetPanel.SetActive(false);
        }
        
        isVisible = false;
        Debug.Log("[BottomSheetManager] Bottom sheet slide down completed");
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
            
            StartCoroutine(SlideDown());
            Debug.Log("[BottomSheetManager] Bottom sheet closed by user");
        }
    }
    
    void OnHighlightButtonClicked()
    {
        if (currentBuilding != null)
        {
            Debug.Log($"[BottomSheetManager] Highlight button clicked for: {currentBuilding.name}");
            
            if (highlightManager != null)
            {
                // Find the building GameObject
                GameObject buildingObject = FindBuildingObject(currentBuilding);
                if (buildingObject != null)
                {
                    highlightManager.HighlightBuilding(buildingObject);
                    Debug.Log($"[BottomSheetManager] Highlighting building object: {buildingObject.name}");
                }
                else
                {
                    Debug.LogWarning($"[BottomSheetManager] Could not find GameObject for building: {currentBuilding.name}");
                }
            }
            else
            {
                Debug.LogWarning("[BottomSheetManager] BuildingHighlightManager not assigned!");
            }
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
        foreach (GameObject obj in GameObject.FindObjectsOfType<GameObject>())
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
            Debug.Log($"[BottomSheetManager] Moving camera to: {currentBuilding.cameraViewPosition}");
            // TODO: Implement camera movement functionality in Phase 4
            // This will move the camera to the building's viewing position
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
