using UnityEngine;

public class Camera2DController : MonoBehaviour
{
    public float panSpeed = 0.5f;
    public float zoomSpeed = 5f;
    public float minZoom = 10f;
    public float maxZoom = 60f;
    
    [Header("Boundary Settings")]
    [Tooltip("Reference to BoundaryController for movement limits. If null, will search for it.")]
    public BoundaryController boundaryController;

    private Vector3 lastMousePos;

    void Start()
    {
        // Find boundary controller if not assigned
        if (boundaryController == null)
        {
            boundaryController = FindFirstObjectByType<BoundaryController>();
        }
    }

    void Update()
    {
        // Pan
        if (Input.GetMouseButtonDown(0)) lastMousePos = Input.mousePosition;
        if (Input.GetMouseButton(0))
        {
            Vector3 delta = Input.mousePosition - lastMousePos;
            delta *= panSpeed * Time.deltaTime;
            Vector3 newPosition = transform.position;
            newPosition += new Vector3(-delta.x, 0, -delta.y);
            
            // Clamp position within boundary if boundary controller is available
            // Only clamp X and Z for 2D mode (Y stays fixed at 180)
            if (boundaryController != null && boundaryController.exteriorBoundary != null)
            {
                Vector3 clamped = boundaryController.ClampToExteriorBoundary(newPosition);
                // Preserve Y position (180 for 2D mode) when clamping
                newPosition = new Vector3(clamped.x, newPosition.y, clamped.z);
            }
            
            transform.position = newPosition;
            lastMousePos = Input.mousePosition;
        }

        // Zoom
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        Camera.main.orthographicSize -= scroll * zoomSpeed;
        Camera.main.orthographicSize = Mathf.Clamp(Camera.main.orthographicSize, minZoom, maxZoom);

        // Touch pinch
        if (Input.touchCount == 2)
        {
            Touch t0 = Input.GetTouch(0);
            Touch t1 = Input.GetTouch(1);
            Vector2 prevDist = (t0.position - t0.deltaPosition) - (t1.position - t1.deltaPosition);
            Vector2 curDist = t0.position - t1.position;
            float delta = prevDist.magnitude - curDist.magnitude;
            Camera.main.orthographicSize += delta * 0.02f;
            Camera.main.orthographicSize = Mathf.Clamp(Camera.main.orthographicSize, minZoom, maxZoom);
        }
    }

    public void ZoomIn()
    {
        Camera.main.orthographicSize -= zoomSpeed;
        Camera.main.orthographicSize = Mathf.Clamp(Camera.main.orthographicSize, minZoom, maxZoom);
    }

    public void ZoomOut()
    {
        Camera.main.orthographicSize += zoomSpeed;
        Camera.main.orthographicSize = Mathf.Clamp(Camera.main.orthographicSize, minZoom, maxZoom);
    }
}
