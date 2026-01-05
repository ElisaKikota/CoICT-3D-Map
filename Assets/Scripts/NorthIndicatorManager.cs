using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class NorthIndicatorManager : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("Container GameObject holding the North indicator UI (will be shown/hidden)")]
    public GameObject container;
    
    [Tooltip("Text component displaying 'N' (TextMeshPro or regular Text)")]
    public TextMeshProUGUI northTextTMP;
    public Text northText; // Fallback for regular Unity Text
    
    [Header("Settings")]
    [Tooltip("Color of the North indicator")]
    public Color indicatorColor = Color.white;
    
    [Tooltip("Font size for the North indicator")]
    public int fontSize = 48;

    void Start()
    {
        // Auto-find text component if not assigned
        if (northTextTMP == null && northText == null && container != null)
        {
            northTextTMP = container.GetComponentInChildren<TextMeshProUGUI>();
            if (northTextTMP == null)
            {
                northText = container.GetComponentInChildren<Text>();
            }
        }
        
        // Set initial text and styling
        if (northTextTMP != null)
        {
            northTextTMP.text = "N";
            northTextTMP.color = indicatorColor;
            northTextTMP.fontSize = fontSize;
            northTextTMP.fontStyle = FontStyles.Bold;
            northTextTMP.alignment = TextAlignmentOptions.Center;
        }
        else if (northText != null)
        {
            northText.text = "N";
            northText.color = indicatorColor;
            northText.fontSize = fontSize;
            northText.fontStyle = FontStyle.Bold;
            northText.alignment = TextAnchor.MiddleCenter;
        }
        
        // Start hidden by default
        SetVisible(false);
    }

    public void SetVisible(bool visible)
    {
        var target = container != null ? container : gameObject;
        if (target != null)
        {
            target.SetActive(visible);
            Debug.Log($"[NorthIndicatorManager] Set visibility to: {visible}");
        }
    }
}




