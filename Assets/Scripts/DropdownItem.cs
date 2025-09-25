using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DropdownItem : MonoBehaviour
{
    [Header("UI References")]
    public Text itemText;
    public TextMeshProUGUI itemTmpText;
    public Button itemButton;
    public Image backgroundImage;
    
    [Header("Visual Settings")]
    public Color normalColor = Color.white;
    public Color highlightedColor = new Color(0.8f, 0.8f, 1f, 1f);
    public Color pressedColor = new Color(0.6f, 0.6f, 1f, 1f);
    
    private bool isHighlighted = false;
    
    void Start()
    {
        SetupButton();
    }
    
    void SetupButton()
    {
        if (itemButton != null)
        {
            // Setup button colors
            ColorBlock colors = itemButton.colors;
            colors.normalColor = normalColor;
            colors.highlightedColor = highlightedColor;
            colors.pressedColor = pressedColor;
            itemButton.colors = colors;
        }
    }
    
    public void SetText(string text)
    {
        // Try regular Unity Text first
        if (itemText != null)
        {
            itemText.text = text;
        }
        // Try TextMeshPro Text
        else if (itemTmpText != null)
        {
            itemTmpText.text = text;
        }
        // Auto-detect if neither is assigned
        else
        {
            Text autoDetectedText = GetComponentInChildren<Text>();
            if (autoDetectedText != null)
            {
                autoDetectedText.text = text;
            }
            else
            {
                TextMeshProUGUI autoDetectedTmp = GetComponentInChildren<TextMeshProUGUI>();
                if (autoDetectedTmp != null)
                {
                    autoDetectedTmp.text = text;
                }
            }
        }
    }
    
    public void SetHighlighted(bool highlighted)
    {
        isHighlighted = highlighted;
        
        if (backgroundImage != null)
        {
            backgroundImage.color = highlighted ? highlightedColor : normalColor;
        }
    }
    
    public void OnPointerEnter()
    {
        SetHighlighted(true);
    }
    
    public void OnPointerExit()
    {
        SetHighlighted(false);
    }
}
