using UnityEngine;
using System.Collections;

public class Highlighter : MonoBehaviour
{
    [Header("Highlight Objects")]
    [SerializeField] private GameObject[] highlightObjects;
    
    [Header("Camera Settings")]
    [Tooltip("Camera to move when highlighting objects")]
    [SerializeField] private Camera targetCamera;
    
    [Tooltip("Speed of camera movement")]
    [SerializeField] private float cameraMoveSpeed = 5f;
    
    [Tooltip("Speed of camera rotation")]
    [SerializeField] private float cameraRotationSpeed = 5f;
    
    [Tooltip("Offset from the object position for camera")]
    [SerializeField] private Vector3 cameraPositionOffset = new Vector3(0f, 5f, -10f);
    
    [Tooltip("Elevation height for camera movement (Y position)")]
    [SerializeField] private float cameraElevationHeight = 20f;
    
    [Tooltip("Whether to use smooth camera movement")]
    [SerializeField] private bool smoothCameraMovement = true;
    
    [Header("Alpha Wave Settings")]
    [Tooltip("Speed of the alpha wave animation")]
    [SerializeField] private float waveSpeed = 2f;
    
    [Tooltip("Minimum alpha value (0 = fully transparent)")]
    [SerializeField] private float minAlpha = 0.3f;
    
    [Tooltip("Maximum alpha value (1 = fully opaque)")]
    [SerializeField] private float maxAlpha = 1f;
    
    [Tooltip("Enable smooth alpha wave animation")]
    [SerializeField] private bool enableAlphaWave = true;
    
    private GameObject currentActiveHighlighter;
    private Coroutine blinkCoroutine;
    private Coroutine cameraMoveCoroutine;
    private Renderer currentRenderer;
    private Renderer[] allRenderers; // All renderers on the highlighter object and children
    private MaterialPropertyBlock propertyBlock;
    private Material[] originalMaterials; // Store original materials to preserve color
    private Color baseColor; // Store the base color of the material
    private bool isSpriteRenderer = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Set all highlight objects to inactive
        foreach (GameObject obj in highlightObjects)
        {
            if (obj != null)
            {
                obj.SetActive(false);
            }
        }
        
        propertyBlock = new MaterialPropertyBlock();
    }

    /// <summary>
    /// Activates a highlighter object by name and starts blinking effect
    /// </summary>
    /// <param name="objectName">The name of the highlighter object to activate</param>
    public void HighlightObject(string objectName)
    {
        // Stop any existing blink coroutine
        if (blinkCoroutine != null)
        {
            StopCoroutine(blinkCoroutine);
            blinkCoroutine = null;
        }
        
        // Deactivate current highlighter if any
        if (currentActiveHighlighter != null)
        {
            currentActiveHighlighter.SetActive(false);
        }
        
        // Find the object by name
        GameObject targetObject = null;
        foreach (GameObject obj in highlightObjects)
        {
            if (obj != null && obj.name == objectName)
            {
                targetObject = obj;
                break;
            }
        }
        
        if (targetObject == null)
        {
            Debug.LogWarning($"Highlighter object with name '{objectName}' not found!");
            return;
        }
        
        // Activate the found object
        currentActiveHighlighter = targetObject;
        targetObject.SetActive(true);
        
        // Get all renderer components (including children)
        allRenderers = targetObject.GetComponentsInChildren<Renderer>(true);
        if (allRenderers == null || allRenderers.Length == 0)
        {
            Debug.LogWarning($"No Renderer found on '{objectName}' or its children!");
            return;
        }
        
        // Use the first renderer as the main one (or find SpriteRenderer if available)
        currentRenderer = null;
        foreach (Renderer r in allRenderers)
        {
            if (r is SpriteRenderer)
            {
                currentRenderer = r;
                break;
            }
        }
        if (currentRenderer == null)
        {
            currentRenderer = allRenderers[0];
        }
        
        Debug.Log($"[Highlighter] Found {allRenderers.Length} renderer(s) on '{objectName}', will animate all of them");
        
        // Store original materials and base color
        if (currentRenderer is SpriteRenderer spriteRenderer)
        {
            isSpriteRenderer = true;
            baseColor = spriteRenderer.color;
            Debug.Log($"[Highlighter] SpriteRenderer found, base color: {baseColor}, alpha: {baseColor.a}");
        }
        else
        {
            isSpriteRenderer = false;
            // Store original materials
            if (currentRenderer.materials != null && currentRenderer.materials.Length > 0)
            {
                originalMaterials = new Material[currentRenderer.materials.Length];
                for (int i = 0; i < currentRenderer.materials.Length; i++)
                {
                    if (currentRenderer.materials[i] != null)
                    {
                        originalMaterials[i] = currentRenderer.materials[i];
                        // Get base color from material
                        if (currentRenderer.materials[i].HasProperty("_Color"))
                        {
                            baseColor = currentRenderer.materials[i].GetColor("_Color");
                            Debug.Log($"[Highlighter] Material has _Color property, base color: {baseColor}, alpha: {baseColor.a}");
                        }
                        else if (currentRenderer.materials[i].HasProperty("_BaseColor"))
                        {
                            baseColor = currentRenderer.materials[i].GetColor("_BaseColor");
                            Debug.Log($"[Highlighter] Material has _BaseColor property, base color: {baseColor}, alpha: {baseColor.a}");
                        }
                        else
                        {
                            baseColor = currentRenderer.materials[i].color;
                            Debug.Log($"[Highlighter] Material color: {baseColor}, alpha: {baseColor.a}");
                        }
                        
                        // Log shader name for debugging
                        Debug.Log($"[Highlighter] Material shader: {currentRenderer.materials[i].shader.name}");
                    }
                }
            }
            
            // Initialize property block
            if (propertyBlock == null)
            {
                propertyBlock = new MaterialPropertyBlock();
            }
        }
        
        // Move camera to target object
        MoveCameraToObject(targetObject);
        
        // Start alpha wave effect
        if (enableAlphaWave)
        {
            Debug.Log($"[Highlighter] Starting alpha wave animation (Speed: {waveSpeed}, Min: {minAlpha}, Max: {maxAlpha})");
            blinkCoroutine = StartCoroutine(AlphaWaveAnimation());
        }
        else
        {
            Debug.LogWarning("[Highlighter] Alpha wave is disabled! Enable it in the inspector.");
        }
    }
    
    /// <summary>
    /// Moves the camera to focus on the target object
    /// </summary>
    private void MoveCameraToObject(GameObject targetObject)
    {
        if (targetCamera == null)
        {
            // Try to find main camera if not assigned
            targetCamera = Camera.main;
            if (targetCamera == null)
            {
                Debug.LogWarning("No camera assigned and no main camera found!");
                return;
            }
        }
        
        // Calculate target position and rotation
        Vector3 targetPosition = targetObject.transform.position + cameraPositionOffset;
        Quaternion targetRotation = Quaternion.LookRotation(targetObject.transform.position - targetPosition);
        
        // Stop any existing camera movement
        if (cameraMoveCoroutine != null)
        {
            StopCoroutine(cameraMoveCoroutine);
        }
        
        if (smoothCameraMovement)
        {
            // Smooth camera movement - pass target object position for rotation calculation
            Vector3 targetObjectPosition = targetObject.transform.position;
            cameraMoveCoroutine = StartCoroutine(MoveCameraSmoothly(targetPosition, targetRotation, targetObjectPosition));
        }
        else
        {
            // Instant camera movement
            targetCamera.transform.position = targetPosition;
            targetCamera.transform.rotation = targetRotation;
        }
    }
    
    /// <summary>
    /// Coroutine for smooth camera movement in 3 stages: elevate, move horizontally, descend
    /// </summary>
    private IEnumerator MoveCameraSmoothly(Vector3 targetPosition, Quaternion targetRotation, Vector3 targetObjectPosition)
    {
        Vector3 startPosition = targetCamera.transform.position;
        Quaternion startRotation = targetCamera.transform.rotation;
        
        // Stage 1: Elevate to specified height (keep X and Z, change Y)
        Vector3 elevatedPosition = new Vector3(startPosition.x, cameraElevationHeight, startPosition.z);
        yield return StartCoroutine(MoveCameraToPosition(startPosition, elevatedPosition, startRotation, 0.33f));
        
        // Stage 2: Move horizontally to above target (keep Y at elevation, change X and Z)
        Vector3 aboveTargetPosition = new Vector3(targetPosition.x, cameraElevationHeight, targetPosition.z);
        // Calculate rotation to look at target object from above
        Vector3 lookDirection = targetObjectPosition - aboveTargetPosition;
        Quaternion horizontalRotation = Quaternion.LookRotation(lookDirection);
        yield return StartCoroutine(MoveCameraToPosition(elevatedPosition, aboveTargetPosition, horizontalRotation, 0.33f));
        
        // Stage 3: Descend to final target position (change Y to target Y, keep X and Z)
        yield return StartCoroutine(MoveCameraToPosition(aboveTargetPosition, targetPosition, targetRotation, 0.34f));
        
        // Ensure final position and rotation are exact
        targetCamera.transform.position = targetPosition;
        targetCamera.transform.rotation = targetRotation;
        
        cameraMoveCoroutine = null;
    }
    
    /// <summary>
    /// Helper coroutine to move camera between two positions
    /// </summary>
    private IEnumerator MoveCameraToPosition(Vector3 startPos, Vector3 endPos, Quaternion endRotation, float duration)
    {
        Quaternion startRotation = targetCamera.transform.rotation;
        float elapsedTime = 0f;
        
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime * cameraMoveSpeed;
            float t = Mathf.Clamp01(elapsedTime / duration);
            t = Mathf.SmoothStep(0f, 1f, t);
            
            targetCamera.transform.position = Vector3.Lerp(startPos, endPos, t);
            targetCamera.transform.rotation = Quaternion.Slerp(startRotation, endRotation, t * cameraRotationSpeed);
            
            yield return null;
        }
        
        // Ensure we reach the exact position
        targetCamera.transform.position = endPos;
        targetCamera.transform.rotation = endRotation;
    }
    
    /// <summary>
    /// Deactivates the current highlighter and stops blinking
    /// </summary>
    public void StopHighlight()
    {
        if (blinkCoroutine != null)
        {
            StopCoroutine(blinkCoroutine);
            blinkCoroutine = null;
        }
        
        if (cameraMoveCoroutine != null)
        {
            StopCoroutine(cameraMoveCoroutine);
            cameraMoveCoroutine = null;
        }
        
        // Restore original alpha before deactivating
        if (currentRenderer != null)
        {
            if (currentRenderer is SpriteRenderer spriteRenderer)
            {
                // Restore original color with original alpha
                spriteRenderer.color = baseColor;
            }
            else if (originalMaterials != null && currentRenderer.materials != null)
            {
                // Restore original materials
                for (int i = 0; i < Mathf.Min(originalMaterials.Length, currentRenderer.materials.Length); i++)
                {
                    if (originalMaterials[i] != null)
                    {
                        // Restore original color
                        Color originalColor = baseColor;
                        currentRenderer.materials[i].color = originalColor;
                        if (currentRenderer.materials[i].HasProperty("_Color"))
                        {
                            currentRenderer.materials[i].SetColor("_Color", originalColor);
                        }
                    }
                }
            }
        }
        
        if (currentActiveHighlighter != null)
        {
            currentActiveHighlighter.SetActive(false);
            currentActiveHighlighter = null;
        }
        
        currentRenderer = null;
        originalMaterials = null;
    }
    
    /// <summary>
    /// Coroutine that creates a smooth alpha wave/pulse effect using sine wave
    /// </summary>
    private IEnumerator AlphaWaveAnimation()
    {
        int frameCount = 0;
        
        while (currentActiveHighlighter != null && currentActiveHighlighter.activeSelf && currentRenderer != null)
        {
            // Calculate alpha using sine wave for smooth pulsing
            // Sin wave oscillates between -1 and 1, we normalize it to 0-1, then lerp between min and max
            float normalizedWave = (Mathf.Sin(Time.time * waveSpeed) + 1f) / 2f;
            float alpha = Mathf.Lerp(minAlpha, maxAlpha, normalizedWave);
            
            // Debug log every 60 frames (approximately once per second at 60fps)
            if (frameCount % 60 == 0)
            {
                Debug.Log($"[Highlighter] Alpha wave - Normalized: {normalizedWave:F2}, Alpha: {alpha:F2}, Time: {Time.time:F2}");
            }
            frameCount++;
            
            // Apply alpha to ALL renderers (including children)
            if (allRenderers != null)
            {
                foreach (Renderer renderer in allRenderers)
                {
                    if (renderer == null) continue;
                    
                    if (renderer is SpriteRenderer spriteRenderer)
                    {
                        // For SpriteRenderer, modify the color alpha directly
                        Color color = spriteRenderer.color;
                        color.a = alpha;
                        spriteRenderer.color = color;
                    }
                    else
                    {
                        // For MeshRenderer, use MaterialPropertyBlock to modify properties
                        if (propertyBlock == null)
                        {
                            propertyBlock = new MaterialPropertyBlock();
                        }
                        
                        renderer.GetPropertyBlock(propertyBlock);
                        
                        // Create color with updated alpha
                        Color matColor = baseColor;
                        matColor.a = alpha;
                        
                        // Try setting _Color property (most common for transparent shaders)
                        propertyBlock.SetColor("_Color", matColor);
                        
                        // Also try _BaseColor for URP/HDRP shaders
                        propertyBlock.SetColor("_BaseColor", matColor);
                        
                        // For transparent materials, also set the alpha directly
                        propertyBlock.SetFloat("_Alpha", alpha);
                        
                        // Apply the property block
                        renderer.SetPropertyBlock(propertyBlock);
                        
                        // Fallback: Direct material modification if property block doesn't work
                        if (renderer.materials != null)
                        {
                            foreach (Material mat in renderer.materials)
                            {
                                if (mat != null)
                                {
                                    // Try _Color property
                                    if (mat.HasProperty("_Color"))
                                    {
                                        mat.SetColor("_Color", matColor);
                                    }
                                    
                                    // Try _BaseColor property (URP/HDRP)
                                    if (mat.HasProperty("_BaseColor"))
                                    {
                                        mat.SetColor("_BaseColor", matColor);
                                    }
                                    
                                    // Try _Alpha property
                                    if (mat.HasProperty("_Alpha"))
                                    {
                                        mat.SetFloat("_Alpha", alpha);
                                    }
                                    
                                    // Also set the color property directly
                                    mat.color = matColor;
                                }
                            }
                        }
                    }
                }
            }
            
            yield return null;
        }
        
        Debug.Log("[Highlighter] Alpha wave animation stopped");
    }
}
