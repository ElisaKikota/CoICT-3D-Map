using UnityEngine;

public class Camera2DController : MonoBehaviour
{
    public float panSpeed = 0.5f;
    public float zoomSpeed = 5f;
    public float minZoom = 10f;
    public float maxZoom = 60f;

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
