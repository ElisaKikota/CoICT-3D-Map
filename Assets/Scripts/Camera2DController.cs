using UnityEngine;

public class Camera2DController : MonoBehaviour
{
    public float panSpeed = 0.5f;
    public float zoomSpeed = 10f;
    public float minHeight = 50f;   // Closest to ground (highest zoom)
    public float maxHeight = 300f;  // Furthest from ground (lowest zoom)

    private Vector3 lastMousePos;

    void Update()
    {
        // Pan
        if (Input.GetMouseButtonDown(0)) lastMousePos = Input.mousePosition;
        if (Input.GetMouseButton(0))
        {
            Vector3 delta = Input.mousePosition - lastMousePos;
            delta *= panSpeed * Time.deltaTime;
            transform.Translate(-delta.x, 0, -delta.y, Space.World);
            lastMousePos = Input.mousePosition;
        }

        // Zoom with mouse scroll wheel
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0)
        {
            Vector3 currentPos = transform.position;
            float newHeight = currentPos.y - (scroll * zoomSpeed * 10f); // Multiply by 10 for more responsive scrolling
            newHeight = Mathf.Clamp(newHeight, minHeight, maxHeight);
            transform.position = new Vector3(currentPos.x, newHeight, currentPos.z);
        }

        // Touch pinch removed - using buttons instead
    }

    // Public methods for zoom buttons
    public void ZoomIn()
    {
        Vector3 currentPos = transform.position;
        float newHeight = currentPos.y - zoomSpeed;
        newHeight = Mathf.Clamp(newHeight, minHeight, maxHeight);
        
        transform.position = new Vector3(currentPos.x, newHeight, currentPos.z);
        
        if (currentPos.y == newHeight)
        {
            Debug.Log($"[Camera2DController] ZoomIn - Already at minimum height ({minHeight}) - closest zoom");
        }
        else
        {
            Debug.Log($"[Camera2DController] ZoomIn - Height: {currentPos.y} -> {newHeight} (closer to ground)");
        }
    }

    public void ZoomOut()
    {
        Vector3 currentPos = transform.position;
        float newHeight = currentPos.y + zoomSpeed;
        newHeight = Mathf.Clamp(newHeight, minHeight, maxHeight);
        
        transform.position = new Vector3(currentPos.x, newHeight, currentPos.z);
        
        if (currentPos.y == newHeight)
        {
            Debug.Log($"[Camera2DController] ZoomOut - Already at maximum height ({maxHeight}) - furthest zoom");
        }
        else
        {
            Debug.Log($"[Camera2DController] ZoomOut - Height: {currentPos.y} -> {newHeight} (further from ground)");
        }
    }

    // Get current zoom percentage (0 = min height/closest zoom, 1 = max height/furthest zoom)
    public float GetZoomPercentage()
    {
        return (transform.position.y - minHeight) / (maxHeight - minHeight);
    }
}
