using UnityEngine;
using UnityEngine.UI;

public class ZoomButtonManager : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("Optional container holding the zoom UI")]
    public GameObject container;
    
    [Tooltip("Assign the + zoom button")]
    public Button zoomInButton;
    
    [Tooltip("Assign the - zoom button")]
    public Button zoomOutButton;
    
    [Header("Camera Controller")]
    [Tooltip("Assign the Camera2DController")]
    public Camera2DController camera2DController;

    void Start()
    {
        // Auto-wire zoom buttons (safe: will not duplicate listeners if run multiple times)
        if (zoomInButton != null)
        {
            zoomInButton.onClick.RemoveAllListeners();
            zoomInButton.onClick.AddListener(OnZoomInClicked);
        }
        else
        {
            Debug.LogWarning("[ZoomButtonManager] zoomInButton is not assigned!");
        }
        
        if (zoomOutButton != null)
        {
            zoomOutButton.onClick.RemoveAllListeners();
            zoomOutButton.onClick.AddListener(OnZoomOutClicked);
        }
        else
        {
            Debug.LogWarning("[ZoomButtonManager] zoomOutButton is not assigned!");
        }
        
        if (camera2DController == null)
        {
            Debug.LogWarning("[ZoomButtonManager] camera2DController is not assigned!");
        }
    }

    public void SetVisible(bool visible)
    {
        var target = container != null ? container : gameObject;
        if (target != null)
        {
            target.SetActive(visible);
            Debug.Log($"[ZoomButtonManager] Set visibility to: {visible}");
        }
    }
    
    private void OnZoomInClicked()
    {
        Debug.Log("[ZoomButtonManager] Zoom In button clicked");
        if (camera2DController != null)
        {
            camera2DController.ZoomIn();
        }
        else
        {
            Debug.LogError("[ZoomButtonManager] Cannot zoom in - camera2DController is null!");
        }
    }
    
    private void OnZoomOutClicked()
    {
        Debug.Log("[ZoomButtonManager] Zoom Out button clicked");
        if (camera2DController != null)
        {
            camera2DController.ZoomOut();
        }
        else
        {
            Debug.LogError("[ZoomButtonManager] Cannot zoom out - camera2DController is null!");
        }
    }
}



