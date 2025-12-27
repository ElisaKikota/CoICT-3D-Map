using UnityEngine;
using TMPro;

/// <summary>
/// Manages world-space text label on a door that faces the camera
/// </summary>
public class DoorLabel : MonoBehaviour
{
    [Header("Label Settings")]
    [Tooltip("Text to display on the door")]
    public string labelText = "";
    
    [Tooltip("Offset from door center (local space)")]
    public Vector3 labelOffset = new Vector3(0, 1, 0);
    
    [Tooltip("Distance offset toward camera (world space)")]
    public float cameraOffsetDistance = 0.25f;
    
    [Tooltip("Font size")]
    public float fontSize = 2f;
    
    [Tooltip("Text color")]
    public Color textColor = Color.white;
    
    private TextMeshPro labelTextMesh;
    private Camera mainCamera;
    private bool isVisible = false;
    
    void Start()
    {
        // Find main camera
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            mainCamera = FindFirstObjectByType<Camera>();
        }
        
        // Create or find TextMeshPro component
        labelTextMesh = GetComponent<TextMeshPro>();
        if (labelTextMesh == null)
        {
            // Try to find in children
            labelTextMesh = GetComponentInChildren<TextMeshPro>();
            
            if (labelTextMesh == null)
            {
                // Create new GameObject for label
                GameObject labelObj = new GameObject("DoorLabel");
                // Don't parent it - we'll position it in world space
                labelTextMesh = labelObj.AddComponent<TextMeshPro>();
            }
        }
        
        // Setup text properties
        if (labelTextMesh != null)
        {
            labelTextMesh.text = labelText;
            labelTextMesh.fontSize = fontSize;
            labelTextMesh.color = textColor;
            labelTextMesh.alignment = TextAlignmentOptions.Center;
            labelTextMesh.textWrappingMode = TextWrappingModes.NoWrap;
            
            // Make text face camera initially
            if (mainCamera != null)
            {
                labelTextMesh.transform.LookAt(labelTextMesh.transform.position + mainCamera.transform.rotation * Vector3.forward,
                    mainCamera.transform.rotation * Vector3.up);
            }
        }
        
        // Hide label initially
        SetVisible(false);
    }
    
    void LateUpdate()
    {
        // Make text always face camera and offset toward camera
        if (labelTextMesh != null && mainCamera != null)
        {
            Vector3 doorPosition = transform.position + transform.TransformDirection(labelOffset);
            Vector3 cameraPosition = mainCamera.transform.position;
            Vector3 directionToCamera = cameraPosition - doorPosition;
            
            if (directionToCamera.sqrMagnitude > 0.01f)
            {
                // Normalize direction
                Vector3 normalizedDirection = directionToCamera.normalized;
                
                // Position label offset toward camera from door position
                Vector3 labelPosition = doorPosition + normalizedDirection * cameraOffsetDistance;
                labelTextMesh.transform.position = labelPosition;
                
                // Make text face camera
                if (isVisible)
                {
                    labelTextMesh.transform.rotation = Quaternion.LookRotation(-directionToCamera, mainCamera.transform.up);
                }
            }
        }
    }
    
    /// <summary>
    /// Sets the label text
    /// </summary>
    public void SetText(string text)
    {
        labelText = text;
        if (labelTextMesh != null)
        {
            labelTextMesh.text = text;
        }
    }
    
    /// <summary>
    /// Sets the label text color
    /// </summary>
    public void SetColor(Color color)
    {
        textColor = color;
        if (labelTextMesh != null)
        {
            labelTextMesh.color = color;
        }
    }
    
    /// <summary>
    /// Shows or hides the label
    /// </summary>
    public void SetVisible(bool visible)
    {
        isVisible = visible;
        if (labelTextMesh != null)
        {
            labelTextMesh.gameObject.SetActive(visible);
        }
    }
    
    /// <summary>
    /// Gets whether the label is currently visible
    /// </summary>
    public bool IsVisible()
    {
        return isVisible;
    }
}

