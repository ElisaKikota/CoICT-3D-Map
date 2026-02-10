using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
#if UNITY_TMP
using TMPro;
#endif

/// <summary>
/// Simplified door raycast system - single door list works for both entry and exit
/// </summary>
public class DoorRaycastSystem : MonoBehaviour
{
    [Header("References")]
    public Camera mainCamera;
    public Canvas uiCanvas;
    
    [Header("Door Data")]
    [Tooltip("List of all doors (works for both entry and exit)")]
    public DoorData[] doors;
    
    [Header("Door Configuration")]
    [Tooltip("Mapping of doors to their secondary interiors (nested interiors)")]
    public DoorSecondaryInteriorMapping[] doorsWithSecondaryInterior;
    
    [System.Serializable]
    public class DoorSecondaryInteriorMapping
    {
        public DoorData door;
        public RoomData secondaryInterior;
    }
    
    [Header("UI References")]
    [Tooltip("Door UI Button Panel (EnterExitButtonPanel with Enter/Exit buttons)")]
    public DoorUIButtonPanel doorButtonPanel;
    
    [Tooltip("Persistent Exit Button (shown in interior mode)")]
    public Button persistentExitButton;
    
    [Tooltip("Top Bar GameObject (will be hidden in interior mode)")]
    public GameObject topBar;

    [Header("Top Bar - Interior Mode Visibility")]
    [Tooltip("Top bar building icon button GameObject (hidden in interior mode)")]
    public GameObject buildingIconButton;

    [Tooltip("Top bar mode icons container GameObject (hidden in interior mode)")]
    public GameObject modeIconsContainer;

    [Tooltip("Top bar floor name GameObject (shown in interior mode)")]
    public GameObject floorName;

    [Tooltip("Top bar go back button GameObject (shown in interior mode)")]
    public Button goBackButton;

    [Tooltip("Top bar go home button GameObject (shown in all modes)")]
    public Button goHomeButton;

    [Tooltip("Minimum spacing between mode icons (when using a HorizontalLayoutGroup)")]
    public float modeIconsMinSpacing = 0f;

    [Tooltip("If true, hide the top bar while in interior mode")]
    public bool hideTopBarInInteriorMode = false;
    
    [Tooltip("Parent GameObject containing all door GameObjects (e.g., 'Doors')")]
    public GameObject doorsParent;
    
    [Header("Raycast Settings")]
    [Tooltip("Maximum raycast distance")]
    public float maxRaycastDistance = 50f;
    
    [Tooltip("Layers that can block raycast")]
    public LayerMask blockingLayers = -1;
    
    [Header("UI Settings")]
    [Tooltip("Distance threshold for showing door UI (closer = shown)")]
    public float showUIDistance = 15f;
    
    private DoorData currentHoveredDoor;
    private DoorData entryDoor; // Door used to enter interior
    private InteriorExteriorManager interiorManager;
    private CameraModeController cameraModeController;
    private HorizontalLayoutGroup modeIconsLayoutGroup;
    private Dictionary<DoorData, DoorLabel> doorLabels = new Dictionary<DoorData, DoorLabel>();
    
    void Start()
    {
        // Auto-find main camera if not assigned
        if (mainCamera == null)
        {
            mainCamera = GetComponent<Camera>();
            if (mainCamera == null)
            {
                mainCamera = Camera.main;
            }
        }
        
        if (uiCanvas == null)
        {
            uiCanvas = FindFirstObjectByType<Canvas>();
        }
        
        interiorManager = FindFirstObjectByType<InteriorExteriorManager>();
        cameraModeController = FindFirstObjectByType<CameraModeController>();
        
        // Find doors parent if not assigned
        if (doorsParent == null)
        {
            GameObject doorsObj = GameObject.Find("Doors");
            if (doorsObj != null)
            {
                doorsParent = doorsObj;
            }
        }
        
        // Find door button panel if not assigned
        if (doorButtonPanel == null)
        {
            doorButtonPanel = FindFirstObjectByType<DoorUIButtonPanel>();
        }
        
        // Auto-populate doors from Doors parent if not manually assigned
        if (doors == null || doors.Length == 0)
        {
            AutoPopulateDoors();
        }
        
        // Setup door labels for all doors
        SetupDoorLabels();
        
        // Setup door labels for doors with secondary interiors (if not already in main doors array)
        SetupSecondaryInteriorDoorLabels();
        
        // Setup persistent exit button
        if (persistentExitButton != null)
        {
            persistentExitButton.onClick.RemoveAllListeners();
            persistentExitButton.onClick.AddListener(OnPersistentExitClicked);
            persistentExitButton.gameObject.SetActive(false);
        }
        
        // Initialize top bar visibility (shown by default since we start in exterior mode)
        if (topBar != null)
        {
            bool isInteriorMode = interiorManager != null && interiorManager.IsInteriorMode();
            topBar.SetActive(!isInteriorMode || !hideTopBarInInteriorMode);
        }

        // Auto-find top bar children by name if not assigned
        if (topBar != null)
        {
            if (buildingIconButton == null)
            {
                Transform t = topBar.transform.Find("BuildingIconButton");
                if (t != null) buildingIconButton = t.gameObject;
            }

            if (modeIconsContainer == null)
            {
                Transform t = topBar.transform.Find("ModeIconsContainer");
                if (t != null) modeIconsContainer = t.gameObject;
            }

            if (modeIconsLayoutGroup == null && modeIconsContainer != null)
            {
                modeIconsLayoutGroup = modeIconsContainer.GetComponent<HorizontalLayoutGroup>();
            }

            if (floorName == null)
            {
                Transform t = topBar.transform.Find("FloorName");
                if (t != null) floorName = t.gameObject;
            }

            if (goBackButton == null)
            {
                Transform t = null;
                if (modeIconsContainer != null)
                {
                    t = modeIconsContainer.transform.Find("GoBackButton");
                }
                if (t == null)
                {
                    t = topBar.transform.Find("GoBackButton");
                }
                if (t != null) goBackButton = t.GetComponent<Button>();
            }

            if (goHomeButton == null)
            {
                Transform t = null;
                if (modeIconsContainer != null)
                {
                    t = modeIconsContainer.transform.Find("GoHomeButton");
                }
                if (t == null)
                {
                    t = topBar.transform.Find("GoHomeButton");
                }
                if (t != null) goHomeButton = t.GetComponent<Button>();
            }
        }

        if (goBackButton != null && interiorManager != null)
        {
            goBackButton.onClick.RemoveAllListeners();
            goBackButton.onClick.AddListener(interiorManager.HandleGoBackButton);
        }

        if (goHomeButton != null && cameraModeController != null)
        {
            goHomeButton.onClick.RemoveAllListeners();
            goHomeButton.onClick.AddListener(cameraModeController.GoHome);
        }
    }
    
    void Update()
    {
        bool isInteriorMode = interiorManager != null && interiorManager.IsInteriorMode();
        
        // Update persistent exit button visibility
        if (persistentExitButton != null)
        {
            persistentExitButton.gameObject.SetActive(isInteriorMode);
        }
        
        // Hide/show top bar based on interior mode
        if (topBar != null)
        {
            bool topBarShouldBeActive = !isInteriorMode || !hideTopBarInInteriorMode;
            topBar.SetActive(topBarShouldBeActive);

            if (topBarShouldBeActive)
            {
                if (buildingIconButton != null)
                {
                    buildingIconButton.SetActive(!isInteriorMode);
                }

                if (modeIconsContainer != null)
                {
                    modeIconsContainer.SetActive(true);

                    if (modeIconsLayoutGroup == null)
                    {
                        modeIconsLayoutGroup = modeIconsContainer.GetComponent<HorizontalLayoutGroup>();
                    }

                    if (modeIconsLayoutGroup != null)
                    {
                        modeIconsLayoutGroup.childAlignment = TextAnchor.MiddleRight;
                        modeIconsLayoutGroup.reverseArrangement = false;
                        modeIconsLayoutGroup.spacing = modeIconsMinSpacing;
                        modeIconsLayoutGroup.childForceExpandWidth = false;
                        modeIconsLayoutGroup.childForceExpandHeight = false;
                    }

                    // In interior mode, only show GoBack and GoHome buttons within the container
                    int childCount = modeIconsContainer.transform.childCount;
                    for (int i = 0; i < childCount; i++)
                    {
                        Transform child = modeIconsContainer.transform.GetChild(i);
                        if (child == null) continue;

                        bool shouldShow = !isInteriorMode;
                        if (isInteriorMode)
                        {
                            shouldShow = child.name == "GoBackButton" || child.name == "GoHomeButton";
                        }

                        child.gameObject.SetActive(shouldShow);
                    }

                    // Ensure layout recalculates after enabling/disabling children.
                    Canvas.ForceUpdateCanvases();
                    RectTransform modeIconsRect = modeIconsContainer.GetComponent<RectTransform>();
                    if (modeIconsRect != null)
                    {
                        LayoutRebuilder.ForceRebuildLayoutImmediate(modeIconsRect);
                    }
                }

                if (floorName != null)
                {
                    floorName.SetActive(isInteriorMode);
                }

                if (goBackButton != null)
                {
                    goBackButton.gameObject.SetActive(isInteriorMode);
                }

                if (goHomeButton != null)
                {
                    goHomeButton.gameObject.SetActive(true);
                }
            }
        }
        
        // Check doors for raycast
        if (doorButtonPanel == null) return;
        
        // Perform raycast from camera center
        Ray ray = new Ray(mainCamera.transform.position, mainCamera.transform.forward);
        RaycastHit hit;
        
        DoorData detectedDoor = null;
        float closestDistance = float.MaxValue;
        
        // Check each door in main doors array
        if (doors != null && doors.Length > 0)
        {
            foreach (DoorData door in doors)
        {
            if (door == null || door.doorObject == null) continue;
            
            // Get door collider
            Collider doorCol = door.doorCollider;
            if (doorCol == null && door.doorObject != null)
            {
                doorCol = door.doorObject.GetComponent<Collider>();
                if (doorCol != null)
                {
                    door.doorCollider = doorCol;
                }
            }
            
            if (doorCol == null && door.doorObject != null)
            {
                doorCol = door.doorObject.GetComponentInChildren<Collider>();
                if (doorCol != null)
                {
                    door.doorCollider = doorCol;
                }
            }
            
            if (doorCol == null || !doorCol.enabled) continue;
            
            // Check if raycast hits this door
            if (doorCol.Raycast(ray, out hit, maxRaycastDistance))
            {
                float distance = Vector3.Distance(mainCamera.transform.position, hit.point);
                
                // Check if anything blocks the raycast between camera and door
                RaycastHit[] hits = Physics.RaycastAll(ray, distance, blockingLayers);
                bool isBlocked = false;
                foreach (RaycastHit blockHit in hits)
                {
                    if (blockHit.collider != doorCol && blockHit.collider.transform != door.doorObject.transform)
                    {
                        isBlocked = true;
                        break;
                    }
                }
                
                if (!isBlocked && distance < closestDistance)
                {
                    closestDistance = distance;
                    detectedDoor = door;
                }
            }
        }
        }
        
        // Also check doors with secondary interiors (they may not be in main doors array)
        if (doorsWithSecondaryInterior != null)
        {
            foreach (var mapping in doorsWithSecondaryInterior)
            {
                DoorData door = mapping.door;
                if (door == null || door.doorObject == null) continue;
                
                // Get door collider
                Collider doorCol = door.doorCollider;
                if (doorCol == null && door.doorObject != null)
                {
                    doorCol = door.doorObject.GetComponent<Collider>();
                    if (doorCol != null)
                    {
                        door.doorCollider = doorCol;
                    }
                }
                
                if (doorCol == null && door.doorObject != null)
                {
                    doorCol = door.doorObject.GetComponentInChildren<Collider>();
                    if (doorCol != null)
                    {
                        door.doorCollider = doorCol;
                    }
                }
                
                if (doorCol == null || !doorCol.enabled) continue;
                
                // Check if raycast hits this door
                if (doorCol.Raycast(ray, out hit, maxRaycastDistance))
                {
                    float distance = Vector3.Distance(mainCamera.transform.position, hit.point);
                    
                    // Check if anything blocks the raycast between camera and door
                    RaycastHit[] hits = Physics.RaycastAll(ray, distance, blockingLayers);
                    bool isBlocked = false;
                    foreach (RaycastHit blockHit in hits)
                    {
                        if (blockHit.collider != doorCol && blockHit.collider.transform != door.doorObject.transform)
                        {
                            isBlocked = true;
                            break;
                        }
                    }
                    
                    if (!isBlocked && distance < closestDistance)
                    {
                        closestDistance = distance;
                        detectedDoor = door;
                    }
                }
            }
        }
        
        // Update UI based on detected door
        if (detectedDoor != null && closestDistance <= showUIDistance)
        {
            if (currentHoveredDoor != detectedDoor)
            {
                ShowDoorUI(detectedDoor, isInteriorMode);
            }
            else if (currentHoveredDoor == detectedDoor)
            {
                ShowDoorUI(detectedDoor, isInteriorMode);
            }
        }
        else
        {
            if (currentHoveredDoor != null)
            {
                HideDoorUI();
            }
        }
        
        // Update persistent exit button text based on hovered door
        if (isInteriorMode && persistentExitButton != null)
        {
            UpdatePersistentExitButton(detectedDoor);
        }
    }
    
    /// <summary>
    /// Updates persistent exit button - changes to "Enter" if looking at secondary door
    /// </summary>
    void UpdatePersistentExitButton(DoorData hoveredDoor)
    {
        if (persistentExitButton == null) return;
        
        // Check if hovered door has secondary interior
        RoomData secondaryInterior = GetSecondaryInteriorForDoor(hoveredDoor);
        
        // If looking at a door with secondary interior, change button to "Enter"
        if (hoveredDoor != null && secondaryInterior != null)
        {
            var buttonText = persistentExitButton.GetComponentInChildren<UnityEngine.UI.Text>();
            if (buttonText == null)
            {
#if UNITY_TMP
                buttonText = persistentExitButton.GetComponentInChildren<TextMeshProUGUI>();
#endif
            }
            if (buttonText != null)
            {
                buttonText.text = "Enter";
            }
        }
        else
        {
            // Normal exit button
            var buttonText = persistentExitButton.GetComponentInChildren<UnityEngine.UI.Text>();
            if (buttonText == null)
            {
#if UNITY_TMP
                buttonText = persistentExitButton.GetComponentInChildren<TextMeshProUGUI>();
#endif
            }
            if (buttonText != null)
            {
                buttonText.text = "Exit";
            }
        }
    }
    
    /// <summary>
    /// Shows UI for the specified door
    /// </summary>
    void ShowDoorUI(DoorData door, bool isInteriorMode)
    {
        if (door == null) return;
        
        if (doorButtonPanel == null)
        {
            doorButtonPanel = FindFirstObjectByType<DoorUIButtonPanel>();
            if (doorButtonPanel == null) return;
        }
        
        currentHoveredDoor = door;
        
        // Ensure door label is set up and show it
        if (!doorLabels.ContainsKey(door) || doorLabels[door] == null)
        {
            SetupDoorLabel(door);
        }
        ShowDoorLabel(door);
        
        // In interior mode, show exit button if door allows entering
        if (isInteriorMode)
        {
            if (door.allowEntering)
            {
                // Show exit button for this door
                doorButtonPanel.ShowPanel();
                doorButtonPanel.ShowExitButton("Exit");
                doorButtonPanel.SetupButtonListeners(
                    onEnterClick: () => OnEnterButtonClicked(door),
                    onExitClick: () => OnInteriorExitButtonClicked(door)
                );
            }
            else
            {
                // Door doesn't allow entering - just show name, no button
                doorButtonPanel.HidePanel();
            }
            return;
        }
        
        // In exterior mode - show Enter button only if door allows entering
        if (door.allowEntering)
        {
            doorButtonPanel.ShowPanel();
            doorButtonPanel.ShowEnterButton("Enter");
            doorButtonPanel.SetupButtonListeners(
                onEnterClick: () => OnEnterButtonClicked(door),
                onExitClick: () => OnExitButtonClicked(door)
            );
        }
        else
        {
            // Door doesn't allow entering - just show name, no button
            doorButtonPanel.HidePanel();
        }
    }
    
    /// <summary>
    /// Hides the current door UI
    /// </summary>
    void HideDoorUI()
    {
        HideAllDoorLabels();
        if (doorButtonPanel != null)
        {
            doorButtonPanel.HidePanel();
        }
        currentHoveredDoor = null;
    }
    
    /// <summary>
    /// Called when enter button is clicked (exterior mode)
    /// </summary>
    void OnEnterButtonClicked(DoorData door)
    {
        if (door == null || interiorManager == null) return;
        
        Debug.Log($"[DoorRaycastSystem] Enter button clicked for door: {door.doorObject.name}");
        
        // Store entry door for exit later
        entryDoor = door;
        
        // Enter interior mode using door's entry position/rotation
        interiorManager.EnterInteriorMode(door.entryPosition, Quaternion.Euler(door.entryRotation), door);
        
        HideDoorUI();
    }
    
    /// <summary>
    /// Called when persistent exit button is clicked
    /// </summary>
    void OnPersistentExitClicked()
    {
        bool isInteriorMode = interiorManager != null && interiorManager.IsInteriorMode();
        
        if (!isInteriorMode) return;
        
        // Check if we're looking at a door with secondary interior
        RoomData secondaryInterior = GetSecondaryInteriorForDoor(currentHoveredDoor);
        if (currentHoveredDoor != null && secondaryInterior != null)
        {
            // Enter secondary interior
            Debug.Log($"[DoorRaycastSystem] Entering secondary interior through door: {currentHoveredDoor.doorObject.name}");
            interiorManager.EnterSecondaryInterior(secondaryInterior, currentHoveredDoor);
        }
        else
        {
            // Exit to exterior
            Debug.Log("[DoorRaycastSystem] Exiting interior");
            interiorManager.ExitInteriorMode();
        }
    }
    
    /// <summary>
    /// Called when exit button is clicked in interior mode
    /// </summary>
    void OnInteriorExitButtonClicked(DoorData door)
    {
        if (door == null || interiorManager == null) return;
        
        Debug.Log($"[DoorRaycastSystem] Exit button clicked in interior mode for door: {door.doorObject.name}");
        
        // Exit to exterior using door's exit position/rotation
        if (door.exitPosition != Vector3.zero && door.exitRotation != Vector3.zero)
        {
            // Use door's exit position/rotation
            interiorManager.ExitInteriorModeToPosition(door.exitPosition, Quaternion.Euler(door.exitRotation));
        }
        else
        {
            // Fall back to normal exit
            interiorManager.ExitInteriorMode();
        }
        
        HideDoorUI();
    }
    
    /// <summary>
    /// Called when exit button is clicked (legacy - not used in new system)
    /// </summary>
    void OnExitButtonClicked(DoorData door)
    {
        // Not used in simplified system
    }
    
    /// <summary>
    /// Gets the entry door (door used to enter interior)
    /// </summary>
    public DoorData GetEntryDoor()
    {
        return entryDoor;
    }
    
    /// <summary>
    /// Sets the entry door (called by InteriorExteriorManager)
    /// </summary>
    public void SetEntryDoor(DoorData door)
    {
        entryDoor = door;
    }
    
    /// <summary>
    /// Automatically populates door data from door GameObjects in the scene
    /// </summary>
    void AutoPopulateDoors()
    {
        if (doorsParent == null) return;
        
        List<DoorData> foundDoors = new List<DoorData>();
        
        foreach (Transform child in doorsParent.transform)
        {
            Collider doorCollider = child.GetComponent<Collider>();
            if (doorCollider != null)
            {
                DoorData doorData = new DoorData();
                doorData.doorObject = child.gameObject;
                doorData.doorCollider = doorCollider;
                
                foundDoors.Add(doorData);
                Debug.Log($"[DoorRaycastSystem] Found door: {child.name}");
            }
        }
        
        if (foundDoors.Count > 0)
        {
            doors = foundDoors.ToArray();
            Debug.Log($"[DoorRaycastSystem] Auto-populated {doors.Length} doors from scene");
            
            // Setup labels for newly found doors
            foreach (DoorData door in doors)
            {
                if (door != null && door.doorObject != null)
                {
                    // Set door name from GameObject name if not set
                    if (string.IsNullOrEmpty(door.doorName))
                    {
                        door.doorName = door.doorObject.name;
                    }
                    SetupDoorLabel(door);
                }
            }
        }
    }
    
    /// <summary>
    /// Sets the door button panel (called by DoorUIButtonPanel on Start)
    /// </summary>
    public void SetButtonPanel(DoorUIButtonPanel panel)
    {
        doorButtonPanel = panel;
    }
    
    /// <summary>
    /// Creates or finds DoorLabel components for all doors
    /// </summary>
    void SetupDoorLabels()
    {
        if (doors != null)
        {
            foreach (DoorData door in doors)
            {
                if (door != null && door.doorObject != null)
                {
                    SetupDoorLabel(door);
                }
            }
        }
    }
    
    /// <summary>
    /// Creates or finds DoorLabel component for a door
    /// </summary>
    void SetupDoorLabel(DoorData door, bool isSecondaryInteriorDoor = false)
    {
        if (door == null || door.doorObject == null) return;
        
        DoorLabel label = door.doorObject.GetComponent<DoorLabel>();
        if (label == null)
        {
            label = door.doorObject.GetComponentInChildren<DoorLabel>();
        }
        
        if (label == null)
        {
            label = door.doorObject.AddComponent<DoorLabel>();
        }
        
        // Use door name if set, otherwise use GameObject name
        string displayText = !string.IsNullOrEmpty(door.doorName) ? door.doorName : door.doorObject.name;
        label.SetText(displayText);

        label.useColliderBounds = true;
        label.placeOnBoundsFaceTowardCamera = true;
        label.boundsFacePadding = 0.02f;
        label.billboardToCamera = false;
        label.offsetTowardCamera = false;
        label.billboardYawOffset = 180f;
        
        // Set text color to black for doors with secondary interiors
        if (isSecondaryInteriorDoor)
        {
            label.textColor = Color.black;
            label.SetColor(Color.black);
        }
        
        label.SetVisible(false);
        
        doorLabels[door] = label;
    }
    
    /// <summary>
    /// Shows door label for the specified door
    /// </summary>
    void ShowDoorLabel(DoorData door)
    {
        if (door == null) return;
        
        HideAllDoorLabels();
        
        // Check if this is a secondary interior door
        bool isSecondaryInteriorDoor = GetSecondaryInteriorForDoor(door) != null;
        
        if (doorLabels.ContainsKey(door) && doorLabels[door] != null)
        {
            // Ensure distance is always 0.25
            doorLabels[door].cameraOffsetDistance = 0.25f;
            // Update text to use doorName field (in case it was changed)
            string displayText = !string.IsNullOrEmpty(door.doorName) ? door.doorName : door.doorObject.name;
            doorLabels[door].SetText(displayText);
            // Set color to black for secondary interior doors
            if (isSecondaryInteriorDoor)
            {
                doorLabels[door].SetColor(Color.black);
            }
            doorLabels[door].SetVisible(true);
        }
        else
        {
            SetupDoorLabel(door, isSecondaryInteriorDoor);
            if (doorLabels.ContainsKey(door) && doorLabels[door] != null)
            {
                doorLabels[door].SetVisible(true);
            }
        }
    }
    
    /// <summary>
    /// Hides all door labels
    /// </summary>
    void HideAllDoorLabels()
    {
        foreach (var kvp in doorLabels)
        {
            if (kvp.Value != null)
            {
                kvp.Value.SetVisible(false);
            }
        }
    }
    
    /// <summary>
    /// Gets the secondary interior for a door (if any)
    /// </summary>
    RoomData GetSecondaryInteriorForDoor(DoorData door)
    {
        if (door == null || doorsWithSecondaryInterior == null) return null;
        
        foreach (var mapping in doorsWithSecondaryInterior)
        {
            if (mapping.door == door && mapping.secondaryInterior != null)
            {
                return mapping.secondaryInterior;
            }
        }
        
        return null;
    }
    
    /// <summary>
    /// Sets up door labels for doors with secondary interiors
    /// </summary>
    void SetupSecondaryInteriorDoorLabels()
    {
        if (doorsWithSecondaryInterior == null) return;
        
        foreach (var mapping in doorsWithSecondaryInterior)
        {
            if (mapping.door != null && mapping.door.doorObject != null)
            {
                SetupDoorLabel(mapping.door, true); // true = is secondary interior door
            }
        }
    }
}
