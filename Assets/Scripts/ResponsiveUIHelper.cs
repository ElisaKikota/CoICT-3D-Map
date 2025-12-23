using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Helps make UI elements responsive across different screen sizes and aspect ratios
/// </summary>
public class ResponsiveUIHelper : MonoBehaviour
{
    [Header("Screen Size Settings")]
    [Tooltip("Reference width for scaling calculations")]
    public float referenceWidth = 1920f;
    
    [Tooltip("Reference height for scaling calculations")]
    public float referenceHeight = 1080f;
    
    [Header("UI Elements to Scale")]
    [Tooltip("Buttons that should scale with screen size")]
    public Button[] scalableButtons;
    
    [Tooltip("Text elements that should scale with screen size")]
    public Text[] scalableTexts;
    
    [Tooltip("Panels that should adjust padding")]
    public RectTransform[] scalablePanels;
    
    [Header("Scaling Settings")]
    [Tooltip("Minimum scale factor")]
    public float minScale = 0.5f;
    
    [Tooltip("Maximum scale factor")]
    public float maxScale = 2f;
    
    [Tooltip("Scale buttons based on screen size")]
    public bool scaleButtons = true;
    
    [Tooltip("Scale text based on screen size")]
    public bool scaleText = true;
    
    private float currentScaleFactor = 1f;
    private Vector2 lastScreenSize;
    
    void Start()
    {
        lastScreenSize = new Vector2(Screen.width, Screen.height);
        UpdateUI();
    }
    
    void Update()
    {
        // Check if screen size changed
        Vector2 currentScreenSize = new Vector2(Screen.width, Screen.height);
        if (currentScreenSize != lastScreenSize)
        {
            UpdateUI();
            lastScreenSize = currentScreenSize;
        }
    }
    
    void UpdateUI()
    {
        // Calculate scale factor based on screen size
        float widthRatio = Screen.width / referenceWidth;
        float heightRatio = Screen.height / referenceHeight;
        currentScaleFactor = Mathf.Min(widthRatio, heightRatio);
        currentScaleFactor = Mathf.Clamp(currentScaleFactor, minScale, maxScale);
        
        // Scale buttons
        if (scaleButtons && scalableButtons != null)
        {
            foreach (Button button in scalableButtons)
            {
                if (button != null)
                {
                    ScaleRectTransform(button.GetComponent<RectTransform>());
                }
            }
        }
        
        // Scale text
        if (scaleText && scalableTexts != null)
        {
            foreach (Text text in scalableTexts)
            {
                if (text != null)
                {
                    text.fontSize = Mathf.RoundToInt(text.fontSize * currentScaleFactor);
                }
            }
        }
        
        // Scale panels
        if (scalablePanels != null)
        {
            foreach (RectTransform panel in scalablePanels)
            {
                if (panel != null)
                {
                    ScaleRectTransform(panel);
                }
            }
        }
        
        Debug.Log($"[ResponsiveUIHelper] Updated UI scale factor: {currentScaleFactor} (Screen: {Screen.width}x{Screen.height})");
    }
    
    void ScaleRectTransform(RectTransform rect)
    {
        if (rect == null) return;
        
        // Scale size delta
        rect.sizeDelta = rect.sizeDelta * currentScaleFactor;
        
        // Scale anchored position
        rect.anchoredPosition = rect.anchoredPosition * currentScaleFactor;
    }
    
    [ContextMenu("Update UI Now")]
    public void ForceUpdateUI()
    {
        UpdateUI();
    }
    
    public float GetCurrentScaleFactor()
    {
        return currentScaleFactor;
    }
}

