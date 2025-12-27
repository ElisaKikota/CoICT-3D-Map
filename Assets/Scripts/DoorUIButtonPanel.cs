using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// UI Panel that contains Enter/Exit buttons for doors
/// Only one button is visible at a time based on context
/// </summary>
public class DoorUIButtonPanel : MonoBehaviour
{
    [Header("Button References")]
    public Button enterButton;
    public Button exitButton;
    
    [Header("Button Labels")]
    public TextMeshProUGUI enterButtonText;
    public TextMeshProUGUI exitButtonText;
    
    private DoorRaycastSystem doorRaycastSystem;
    
    void Start()
    {
        // Find door raycast system if not assigned
        if (doorRaycastSystem == null)
        {
            doorRaycastSystem = FindFirstObjectByType<DoorRaycastSystem>();
        }
        
        // Register with door raycast system
        if (doorRaycastSystem != null)
        {
            doorRaycastSystem.SetButtonPanel(this);
        }
        
        // Initially hide both buttons and panel
        HideButtons();
        HidePanel();
    }
    
    /// <summary>
    /// Shows the enter button with optional custom text
    /// </summary>
    public void ShowEnterButton(string buttonText = "Enter")
    {
        HideButtons();
        
        if (enterButton != null)
        {
            enterButton.gameObject.SetActive(true);
        }
        
        if (enterButtonText != null)
        {
            enterButtonText.text = buttonText;
        }
    }
    
    /// <summary>
    /// Shows the exit button with optional custom text
    /// </summary>
    public void ShowExitButton(string buttonText = "Exit")
    {
        HideButtons();
        
        if (exitButton != null)
        {
            exitButton.gameObject.SetActive(true);
        }
        
        if (exitButtonText != null)
        {
            exitButtonText.text = buttonText;
        }
    }
    
    /// <summary>
    /// Hides both buttons
    /// </summary>
    public void HideButtons()
    {
        if (enterButton != null)
        {
            enterButton.gameObject.SetActive(false);
        }
        
        if (exitButton != null)
        {
            exitButton.gameObject.SetActive(false);
        }
    }
    
    /// <summary>
    /// Shows the panel (makes the panel GameObject active)
    /// </summary>
    public void ShowPanel()
    {
        gameObject.SetActive(true);
    }
    
    /// <summary>
    /// Hides the panel (makes the panel GameObject inactive)
    /// </summary>
    public void HidePanel()
    {
        gameObject.SetActive(false);
    }
    
    /// <summary>
    /// Sets up button click listeners
    /// </summary>
    public void SetupButtonListeners(System.Action onEnterClick, System.Action onExitClick)
    {
        if (enterButton != null)
        {
            enterButton.onClick.RemoveAllListeners();
            enterButton.onClick.AddListener(() => onEnterClick?.Invoke());
        }
        
        if (exitButton != null)
        {
            exitButton.onClick.RemoveAllListeners();
            exitButton.onClick.AddListener(() => onExitClick?.Invoke());
        }
    }
}

