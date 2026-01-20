using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

/// <summary>
/// Manages displaying text labels above highlighter objects
/// Labels can be toggled on/off via button
/// Labels are visible by default in 2D, Drone, and 3D modes
/// </summary>
public class HighlighterLabelManager : MonoBehaviour
{
    [Header("Highlighter References")]
    [Tooltip("Parent GameObject containing all highlighter objects. If not assigned, will search for 'Highlighters' in scene.")]
    public GameObject allHighlightersParent;
    
    [Header("Camera Mode Reference")]
    [Tooltip("Reference to CameraModeController to get current mode. If not assigned, will search for it.")]
    public CameraModeController cameraModeController;
    
    [Header("Label Settings")]
    [Tooltip("Prefab for label (should have Text component). If null, will create default labels.")]
    public GameObject labelPrefab;
    
    [Tooltip("Offset above highlighter (Y axis)")]
    public float labelHeightOffset = 7f; // Changed to 7 as requested
    
    [Tooltip("Label text color")]
    public Color labelColor = Color.white;
    
    [Tooltip("Label text size")]
    public float labelFontSize = 30f;
    
    // Force font size - using larger size for better quality with legacy Text
    // Canvas will be scaled down to compensate, giving us crisp rendering
    private const float FORCED_FONT_SIZE = 600f; // Large size for quality
    private const float CANVAS_SCALE = 0.01f; // Scale canvas down to compensate
    
    [Tooltip("Whether labels are visible by default")]
    public bool labelsVisibleByDefault = true;
    
    private Dictionary<GameObject, GameObject> highlighterLabels = new Dictionary<GameObject, GameObject>();
    private Dictionary<GameObject, float> cachedTopFaceHeights = new Dictionary<GameObject, float>(); // Cache top face heights to avoid expensive recalculations
    private List<GameObject> allHighlighters = new List<GameObject>();
    private bool labelsVisible = true;
    private int updateIndex = 0; // Track which label to update next (for spreading updates across frames)
    private bool isInitializing = true; // Flag to prevent updates during initialization
    private TourMode lastMode = TourMode.Mode2D; // Track last mode to update rotations when mode changes
    
    void Start()
    {
        // Find all highlighters parent if not assigned
        if (allHighlightersParent == null)
        {
            GameObject highlightersObj = GameObject.Find("Highlighters");
            if (highlightersObj != null)
            {
                allHighlightersParent = highlightersObj;
            }
        }
        
        // Find CameraModeController if not assigned
        if (cameraModeController == null)
        {
            cameraModeController = FindFirstObjectByType<CameraModeController>();
        }
        
        // Collect all highlighters (must be done first, but quickly)
        CollectAllHighlighters();
        
        // Spread all expensive operations across frames to prevent freeze
        StartCoroutine(InitializeLabelsCoroutine());
    }
    
    /// <summary>
    /// Initializes labels across multiple frames to prevent freeze
    /// </summary>
    System.Collections.IEnumerator InitializeLabelsCoroutine()
    {
        isInitializing = true;
        
        // Create labels gradually (spread across frames)
        yield return StartCoroutine(CreateLabelsCoroutine());
        
        // Cache top face heights gradually (spread across frames)
        yield return StartCoroutine(CacheTopFaceHeightsCoroutine());
        
        // Set initial visibility
        labelsVisible = labelsVisibleByDefault;
        SetLabelsVisible(labelsVisible);
        
        // Mark initialization as complete first
        isInitializing = false;
        
        // Wait one frame to ensure CameraModeController has initialized
        yield return null;
        
        // Set initial rotations based on current mode (after initialization)
        if (cameraModeController != null)
        {
            lastMode = cameraModeController.currentMode;
            UpdateLabelRotations(lastMode);
        }
        else
        {
            // Default to 2D mode rotation if controller not found
            lastMode = TourMode.Mode2D;
            UpdateLabelRotations(TourMode.Mode2D);
        }
        
        Debug.Log($"[HighlighterLabelManager] Initialization complete - labels ready, mode: {lastMode}");
    }
    
    /// <summary>
    /// Collects all highlighter GameObjects from the scene
    /// Optimized to avoid expensive FindObjectsByType when possible
    /// </summary>
    void CollectAllHighlighters()
    {
        allHighlighters.Clear();
        
        if (allHighlightersParent != null)
        {
            // Get all children of the highlighters parent (fast - no search needed)
            foreach (Transform child in allHighlightersParent.transform)
            {
                allHighlighters.Add(child.gameObject);
            }
        }
        else
        {
            // Only search if parent not assigned - but this is expensive, so try to avoid
            // Try to find parent first before doing expensive search
            GameObject highlightersObj = GameObject.Find("Highlighters");
            if (highlightersObj != null)
            {
                allHighlightersParent = highlightersObj;
                foreach (Transform child in highlightersObj.transform)
                {
                    allHighlighters.Add(child.gameObject);
                }
            }
            else
            {
                // Last resort: search all objects (VERY EXPENSIVE - avoid if possible)
                Debug.LogWarning("[HighlighterLabelManager] No 'Highlighters' parent found, searching all objects (slow!)");
                GameObject[] allObjects = FindObjectsByType<GameObject>(FindObjectsSortMode.None);
                foreach (GameObject obj in allObjects)
                {
                    if (obj.name.Contains("Highlighter") || obj.name.Contains("highlighter"))
                    {
                        allHighlighters.Add(obj);
                    }
                }
            }
        }
        
        Debug.Log($"[HighlighterLabelManager] Found {allHighlighters.Count} highlighter objects");
    }
    
    /// <summary>
    /// Creates labels for all highlighters (spread across frames to prevent freeze)
    /// </summary>
    System.Collections.IEnumerator CreateLabelsCoroutine()
    {
        highlighterLabels.Clear();
        
        int processedThisFrame = 0;
        const int maxPerFrame = 3; // Create max 3 labels per frame to avoid freeze
        
        foreach (GameObject highlighter in allHighlighters)
        {
            if (highlighter != null)
            {
                // Check if highlighter has children
                if (highlighter.transform.childCount > 0)
                {
                    // Create a label for each child
                    foreach (Transform child in highlighter.transform)
                    {
                        CreateLabelForHighlighter(child.gameObject, highlighter);
                        processedThisFrame++;
                        
                        // Yield every few labels to prevent freeze
                        if (processedThisFrame >= maxPerFrame)
                        {
                            processedThisFrame = 0;
                            yield return null; // Wait one frame
                        }
                    }
                }
                else
                {
                    // No children, create label for the parent itself
                    CreateLabelForHighlighter(highlighter, highlighter);
                    processedThisFrame++;
                    
                    // Yield every few labels to prevent freeze
                    if (processedThisFrame >= maxPerFrame)
                    {
                        processedThisFrame = 0;
                        yield return null; // Wait one frame
                    }
                }
            }
        }
        
        Debug.Log($"[HighlighterLabelManager] Created {highlighterLabels.Count} labels");
    }
    
    /// <summary>
    /// Creates a label for a specific highlighter object
    /// </summary>
    /// <param name="targetObject">The actual object to create label for (child or parent)</param>
    /// <param name="parentHighlighter">The parent highlighter object (for reference)</param>
    void CreateLabelForHighlighter(GameObject targetObject, GameObject parentHighlighter)
    {
        if (targetObject == null) return;
        
        // Get the target object's name for the label text
        string labelText = targetObject.name;
        
        // Remove "Highlighter" suffix if present
        if (labelText.EndsWith("Highlighter") || labelText.EndsWith("highlighter"))
        {
            labelText = labelText.Substring(0, labelText.Length - "Highlighter".Length).Trim();
        }
        
        // Calculate label position: Y position + 7 (as requested)
        Vector3 labelPosition = new Vector3(targetObject.transform.position.x, targetObject.transform.position.y + 7f, targetObject.transform.position.z);
        
        GameObject labelObject;
        
        if (labelPrefab != null)
        {
            // Use prefab if available
            labelObject = Instantiate(labelPrefab, labelPosition, Quaternion.identity);
            
            // Scale canvas transform down for higher quality rendering
            labelObject.transform.localScale = new Vector3(CANVAS_SCALE, CANVAS_SCALE, CANVAS_SCALE);
            
            // Update Text component if it exists in prefab
            Text textComponent = labelObject.GetComponent<Text>();
            if (textComponent == null)
            {
                textComponent = labelObject.GetComponentInChildren<Text>();
            }
            if (textComponent != null)
            {
                textComponent.text = labelText;
                textComponent.fontSize = (int)FORCED_FONT_SIZE;
                textComponent.color = labelColor;
                textComponent.alignment = TextAnchor.MiddleCenter;
                textComponent.horizontalOverflow = HorizontalWrapMode.Overflow;
                textComponent.verticalOverflow = VerticalWrapMode.Overflow;
            }
        }
        else
        {
            // Create default label with Canvas and Text component
            labelObject = new GameObject($"Label_{targetObject.name}");
            labelObject.transform.position = labelPosition;
            
            // Add Canvas component for UI rendering
            Canvas canvas = labelObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.worldCamera = Camera.main;
            
            // Add CanvasScaler for proper scaling
            CanvasScaler scaler = labelObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;
            scaler.scaleFactor = 1f;
            
            // Scale canvas transform down for higher quality rendering
            // This makes the canvas render at higher resolution while maintaining same visual size
            labelObject.transform.localScale = new Vector3(CANVAS_SCALE, CANVAS_SCALE, CANVAS_SCALE);
            
            // Create child GameObject for the text
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(labelObject.transform);
            textObj.transform.localPosition = Vector3.zero;
            textObj.transform.localScale = Vector3.one;
            
            // Add Text component
            Text textComponent = textObj.AddComponent<Text>();
            textComponent.text = labelText;
            textComponent.fontSize = (int)FORCED_FONT_SIZE; // Always use 30
            textComponent.color = labelColor;
            textComponent.alignment = TextAnchor.MiddleCenter;
            textComponent.horizontalOverflow = HorizontalWrapMode.Overflow;
            textComponent.verticalOverflow = VerticalWrapMode.Overflow;
            
            // Set default font (LegacyRuntime)
            textComponent.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        }
        
        // Keep label independent of highlighter's active state
        // Parent to target object's parent if available, otherwise keep as child of target object
        // (Labels will still work even when highlighter is inactive because we update position in Update)
        if (targetObject.transform.parent != null)
        {
            labelObject.transform.SetParent(targetObject.transform.parent, true);
        }
        else
        {
            // If no parent, keep as child of target object but it will still be positioned correctly in Update
            labelObject.transform.SetParent(targetObject.transform, true);
        }
        
        // Set initial rotation based on current mode (if available)
        TourMode currentMode = TourMode.Mode2D; // Default to 2D
        if (cameraModeController != null)
        {
            currentMode = cameraModeController.currentMode;
        }
        SetLabelRotation(labelObject, currentMode);
        
        // Store reference: key is the target object (child or parent), value is the label
        highlighterLabels[targetObject] = labelObject;
        
        // Store the top face height for this highlighter
        // We'll update label position in Update if needed
    }
    
    /// <summary>
    /// Sets rotation for a single label based on camera mode
    /// </summary>
    void SetLabelRotation(GameObject labelObject, TourMode mode)
    {
        if (labelObject == null) return;
        
        Vector3 rotation;
        
        if (mode == TourMode.Mode2D)
        {
            // 2D mode: Camera looks down, labels should look up (X = 90)
            rotation = new Vector3(90f, 0f, 0f);
        }
        else if (mode == TourMode.Mode3D)
        {
            // 3D mode: X = 0, Y = 225 to match camera rotation
            rotation = new Vector3(0f, 225f, 0f);
        }
        else
        {
            // Drone/Walk modes: Use 3D rotation as default
            rotation = new Vector3(0f, 225f, 0f);
        }
        
        labelObject.transform.rotation = Quaternion.Euler(rotation);
    }
    
    /// <summary>
    /// Caches top face heights for all highlighters using a coroutine to spread work across frames
    /// This avoids expensive activation/deactivation cycles causing freezes
    /// </summary>
    System.Collections.IEnumerator CacheTopFaceHeightsCoroutine()
    {
        cachedTopFaceHeights.Clear();
        
        int processedThisFrame = 0;
        const int maxPerFrame = 5; // Process max 5 objects per frame to avoid freeze
        
        foreach (GameObject highlighter in allHighlighters)
        {
            if (highlighter != null)
            {
                // Check if highlighter has children
                if (highlighter.transform.childCount > 0)
                {
                    // Cache height for each child
                    foreach (Transform child in highlighter.transform)
                    {
                        if (child.gameObject != null)
                        {
                            float height = CalculateTopFaceHeight(child.gameObject);
                            cachedTopFaceHeights[child.gameObject] = height;
                            processedThisFrame++;
                            
                            // Yield every few objects to prevent freeze
                            if (processedThisFrame >= maxPerFrame)
                            {
                                processedThisFrame = 0;
                                yield return null; // Wait one frame
                            }
                        }
                    }
                }
                else
                {
                    // Cache height for parent
                    float height = CalculateTopFaceHeight(highlighter);
                    cachedTopFaceHeights[highlighter] = height;
                    processedThisFrame++;
                    
                    // Yield every few objects to prevent freeze
                    if (processedThisFrame >= maxPerFrame)
                    {
                        processedThisFrame = 0;
                        yield return null; // Wait one frame
                    }
                }
            }
        }
        
        Debug.Log($"[HighlighterLabelManager] Cached top face heights for {cachedTopFaceHeights.Count} objects");
    }
    
    /// <summary>
    /// Gets the cached top face height for an object, or calculates it if not cached
    /// </summary>
    float GetTopFaceHeight(GameObject highlighter)
    {
        if (highlighter == null) return 0f;
        
        // Check cache first
        if (cachedTopFaceHeights.TryGetValue(highlighter, out float cachedHeight))
        {
            return cachedHeight;
        }
        
        // If not cached, calculate and cache it
        float height = CalculateTopFaceHeight(highlighter);
        cachedTopFaceHeights[highlighter] = height;
        return height;
    }
    
    /// <summary>
    /// Calculates the top face height (Y position) of a highlighter using renderer bounds
    /// Works even when the highlighter is inactive
    /// This is the expensive operation that should be cached
    /// Optimized to avoid SetActive calls when possible
    /// </summary>
    float CalculateTopFaceHeight(GameObject highlighter)
    {
        if (highlighter == null) return highlighter.transform.position.y;
        
        float maxY = float.MinValue;
        
        // Try to get renderers without activating (works if parent is active)
        Renderer[] renderers = highlighter.GetComponentsInChildren<Renderer>(true); // Include inactive
        
        if (renderers != null && renderers.Length > 0)
        {
            foreach (Renderer renderer in renderers)
            {
                if (renderer != null)
                {
                    // Try to get bounds without activating
                    // Bounds might be invalid for inactive renderers, so we check
                    if (renderer.bounds.size.y > 0.001f) // Small threshold to avoid zero-size bounds
                    {
                        float topY = renderer.bounds.max.y;
                        if (topY > maxY)
                        {
                            maxY = topY;
                        }
                    }
                }
            }
        }
        
        // If we didn't get valid bounds (renderer was inactive), try activating temporarily
        if (maxY == float.MinValue)
        {
            bool wasActive = highlighter.activeSelf;
            if (!wasActive)
            {
                highlighter.SetActive(true);
                // Wait one frame for renderer to update (but we can't wait in a non-coroutine)
                // So we'll just get what we can
                renderers = highlighter.GetComponentsInChildren<Renderer>();
                if (renderers != null && renderers.Length > 0)
                {
                    foreach (Renderer renderer in renderers)
                    {
                        if (renderer != null && renderer.bounds.size.y > 0.001f)
                        {
                            float topY = renderer.bounds.max.y;
                            if (topY > maxY)
                            {
                                maxY = topY;
                            }
                        }
                    }
                }
                highlighter.SetActive(false);
            }
        }
        
        // If we found a valid top Y, use it; otherwise use transform position
        if (maxY != float.MinValue)
        {
            return maxY;
        }
        else
        {
            return highlighter.transform.position.y;
        }
    }
    
    /// <summary>
    /// Sets labels visibility
    /// </summary>
    public void SetLabelsVisible(bool visible)
    {
        labelsVisible = visible;
        
        foreach (var kvp in highlighterLabels)
        {
            if (kvp.Value != null)
            {
                kvp.Value.SetActive(visible);
            }
        }
        
        Debug.Log($"[HighlighterLabelManager] Labels {(visible ? "shown" : "hidden")}");
    }
    
    /// <summary>
    /// Toggles labels visibility
    /// </summary>
    public void ToggleLabels()
    {
        SetLabelsVisible(!labelsVisible);
    }
    
    /// <summary>
    /// Updates label positions and rotations based on camera mode
    /// Uses fixed rotations for better performance:
    /// - 2D mode: X rotation = 90 (looking up, since camera looks down)
    /// - 3D mode: X rotation = 0, Y rotation = 225 (to match camera)
    /// Uses LateUpdate to ensure camera position is finalized
    /// Optimized to spread updates across frames to prevent freeze
    /// </summary>
    void LateUpdate()
    {
        // Don't update during initialization to prevent freeze
        if (isInitializing) return;
        
        if (!labelsVisible) return;
        
        Camera mainCam = Camera.main;
        if (mainCam == null) return;
        
        if (highlighterLabels.Count == 0) return;
        
        // Get current camera mode
        TourMode currentMode = TourMode.Mode2D; // Default to 2D
        if (cameraModeController != null)
        {
            currentMode = cameraModeController.currentMode;
        }
        
        // Update rotations if mode changed (fixed rotations based on mode)
        if (currentMode != lastMode)
        {
            UpdateLabelRotations(currentMode);
            lastMode = currentMode;
        }
        
        // Only update positions for a few labels per frame to avoid freeze
        const int labelsPerFrame = 10; // Update max 10 labels per frame
        int processed = 0;
        
        // Convert dictionary to list for indexed access (more efficient)
        var labelList = new List<KeyValuePair<GameObject, GameObject>>(highlighterLabels);
        int startIndex = updateIndex % labelList.Count;
        
        for (int i = 0; i < labelList.Count && processed < labelsPerFrame; i++)
        {
            int index = (startIndex + i) % labelList.Count;
            var kvp = labelList[index];
            
            GameObject targetObject = kvp.Key;
            GameObject labelObject = kvp.Value;
            
            if (targetObject == null || labelObject == null || !labelObject.activeSelf)
                continue;
            
            // Update label position: Y position + 7 (as requested)
            Vector3 targetPos = targetObject.transform.position;
            labelObject.transform.position = new Vector3(targetPos.x, targetPos.y + 7f, targetPos.z);
            
            // Update Canvas worldCamera reference
            Canvas canvas = labelObject.GetComponent<Canvas>();
            if (canvas != null)
            {
                canvas.worldCamera = mainCam;
            }
            
            // Always update font size to ensure it matches the forced value
            Text textComponent = labelObject.GetComponent<Text>();
            if (textComponent == null)
            {
                textComponent = labelObject.GetComponentInChildren<Text>();
            }
            if (textComponent != null)
            {
                textComponent.fontSize = (int)FORCED_FONT_SIZE;
            }
            
            processed++;
        }
        
        // Wrap around to start for next frame
        updateIndex = (updateIndex + processed) % Mathf.Max(1, labelList.Count);
    }
    
    /// <summary>
    /// Updates all label rotations based on camera mode
    /// 2D mode: X rotation = 90 (looking up)
    /// 3D mode: X rotation = 0, Y rotation = 225
    /// Can be called externally when mode changes
    /// </summary>
    public void UpdateLabelRotations(TourMode mode)
    {
        Vector3 rotation;
        
        if (mode == TourMode.Mode2D)
        {
            // 2D mode: Camera looks down, labels should look up (X = 90)
            rotation = new Vector3(90f, 0f, 0f);
        }
        else if (mode == TourMode.Mode3D)
        {
            // 3D mode: X = 0, Y = 225 to match camera rotation
            rotation = new Vector3(0f, 225f, 0f);
        }
        else
        {
            // Drone/Walk modes: Use 3D rotation as default
            rotation = new Vector3(0f, 225f, 0f);
        }
        
        // Apply rotation to all labels
        foreach (var kvp in highlighterLabels)
        {
            GameObject labelObject = kvp.Value;
            
            if (labelObject == null || !labelObject.activeSelf)
                continue;
            
            labelObject.transform.rotation = Quaternion.Euler(rotation);
        }
        
        Debug.Log($"[HighlighterLabelManager] Updated label rotations for mode {mode}: {rotation}");
    }
    
    /// <summary>
    /// Called when values change in inspector - updates all existing labels
    /// </summary>
    void OnValidate()
    {
        // Update all existing labels when font size changes in inspector
        if (Application.isPlaying && highlighterLabels != null)
        {
            foreach (var kvp in highlighterLabels)
            {
                if (kvp.Value != null)
                {
                    Text textComponent = kvp.Value.GetComponent<Text>();
                    if (textComponent == null)
                    {
                        textComponent = kvp.Value.GetComponentInChildren<Text>();
                    }
                    if (textComponent != null)
                    {
                        textComponent.fontSize = (int)FORCED_FONT_SIZE; // Always use 30
                    }
                }
            }
        }
    }
}

