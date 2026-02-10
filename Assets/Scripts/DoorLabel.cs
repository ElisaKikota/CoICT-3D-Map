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

    public bool useColliderBounds = true;
    public bool placeOnBoundsFaceTowardCamera = true;
    public float boundsFacePadding = 0.02f;
    public bool billboardToCamera = false;
    public bool offsetTowardCamera = false;

    public float billboardYawOffset = 0f;
    
    [Tooltip("Font size")]
    public float fontSize = 2f;
    
    [Tooltip("Text color")]
    public Color textColor = Color.white;
    
    private TextMeshPro labelTextMesh;
    private Camera mainCamera;
    private bool isVisible = false;
    private Collider targetCollider;

    private Vector3 lastFaceNormalWorld = Vector3.forward;
    
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
                labelObj.transform.SetParent(transform, worldPositionStays: false);
                labelTextMesh = labelObj.AddComponent<TextMeshPro>();
            }
        }

        if (targetCollider == null)
        {
            targetCollider = GetComponent<Collider>();
            if (targetCollider == null)
            {
                targetCollider = GetComponentInChildren<Collider>();
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

            if (labelTextMesh.transform.parent != transform)
            {
                labelTextMesh.transform.SetParent(transform, worldPositionStays: true);
            }
        }
        
        // Hide label initially
        SetVisible(false);
    }
    
    void LateUpdate()
    {
        if (labelTextMesh == null) return;

        Vector3 basePos;
        if (useColliderBounds && targetCollider != null)
        {
            // Prefer using collider local space for rotated doors (BoxCollider), otherwise fallback to world bounds.
            BoxCollider box = targetCollider as BoxCollider;
            if (placeOnBoundsFaceTowardCamera && mainCamera != null && box != null)
            {
                Transform ct = box.transform;
                Vector3 localCenter = box.center;
                Vector3 worldCenter = ct.TransformPoint(localCenter);
                Vector3 worldDir = mainCamera.transform.position - worldCenter;
                Vector3 localDir = ct.InverseTransformDirection(worldDir);

                Vector3 halfSize = box.size * 0.5f;
                Vector3 localPos = localCenter;

                float ax = Mathf.Abs(localDir.x);
                float ay = Mathf.Abs(localDir.y);
                float az = Mathf.Abs(localDir.z);

                if (ax >= ay && ax >= az)
                {
                    float sign = Mathf.Sign(localDir.x);
                    localPos.x += sign * (halfSize.x + boundsFacePadding);
                    lastFaceNormalWorld = ct.TransformDirection(new Vector3(sign, 0f, 0f));
                }
                else if (ay >= ax && ay >= az)
                {
                    float sign = Mathf.Sign(localDir.y);
                    localPos.y += sign * (halfSize.y + boundsFacePadding);
                    lastFaceNormalWorld = ct.TransformDirection(new Vector3(0f, sign, 0f));
                }
                else
                {
                    float sign = Mathf.Sign(localDir.z);
                    localPos.z += sign * (halfSize.z + boundsFacePadding);
                    lastFaceNormalWorld = ct.TransformDirection(new Vector3(0f, 0f, sign));
                }

                basePos = ct.TransformPoint(localPos);
            }
            else
            {
                Bounds b = targetCollider.bounds;
                basePos = b.center;

                if (placeOnBoundsFaceTowardCamera && mainCamera != null)
                {
                    Vector3 dir = mainCamera.transform.position - b.center;
                    if (dir.sqrMagnitude > 0.0001f)
                    {
                        float ax = Mathf.Abs(dir.x);
                        float ay = Mathf.Abs(dir.y);
                        float az = Mathf.Abs(dir.z);

                        if (ax >= ay && ax >= az)
                        {
                            float sign = Mathf.Sign(dir.x);
                            basePos.x += sign * (b.extents.x + boundsFacePadding);
                            lastFaceNormalWorld = new Vector3(sign, 0f, 0f);
                        }
                        else if (ay >= ax && ay >= az)
                        {
                            float sign = Mathf.Sign(dir.y);
                            basePos.y += sign * (b.extents.y + boundsFacePadding);
                            lastFaceNormalWorld = new Vector3(0f, sign, 0f);
                        }
                        else
                        {
                            float sign = Mathf.Sign(dir.z);
                            basePos.z += sign * (b.extents.z + boundsFacePadding);
                            lastFaceNormalWorld = new Vector3(0f, 0f, sign);
                        }
                    }
                }
            }
        }
        else
        {
            basePos = transform.position;
        }

        basePos += transform.TransformDirection(labelOffset);

        if (billboardToCamera && mainCamera != null)
        {
            Vector3 cameraPosition = mainCamera.transform.position;
            Vector3 directionToCamera = cameraPosition - basePos;
            if (directionToCamera.sqrMagnitude > 0.01f)
            {
                Vector3 normalizedDirection = directionToCamera.normalized;

                Vector3 labelPosition = basePos;
                if (offsetTowardCamera)
                {
                    labelPosition = basePos + normalizedDirection * cameraOffsetDistance;
                }

                labelTextMesh.transform.position = labelPosition;
                if (isVisible)
                {
                    Quaternion look = Quaternion.LookRotation(directionToCamera, mainCamera.transform.up);
                    labelTextMesh.transform.rotation = look * Quaternion.Euler(0f, billboardYawOffset, 0f);
                }
            }
        }
        else
        {
            labelTextMesh.transform.position = basePos;
            if (isVisible)
            {
                // Fixed orientation on the selected face (no camera billboarding).
                Vector3 n = lastFaceNormalWorld.sqrMagnitude > 0.0001f ? lastFaceNormalWorld.normalized : transform.forward;
                Quaternion faceRot = Quaternion.LookRotation(n, transform.up);
                labelTextMesh.transform.rotation = faceRot * Quaternion.Euler(0f, billboardYawOffset, 0f);
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

