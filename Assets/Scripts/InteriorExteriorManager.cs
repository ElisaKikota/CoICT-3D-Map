using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Manages switching between exterior and interior modes.
/// Handles fade transitions, location saving, and mode-specific features.
/// </summary>
public class InteriorExteriorManager : MonoBehaviour
{
    [Header("Mode State")]
    public bool isInteriorMode = false;
    public Vector3 savedExteriorPosition;
    public Quaternion savedExteriorRotation;
    
    [Header("References")]
    public Camera mainCamera;
    public CameraModeController cameraModeController;
    public DroneController droneController;
    public WalkController walkController;
    
    [Header("Interior Settings")]
    [Tooltip("Light intensity multiplier for interior mode")]
    public float interiorLightIntensity = 2f;
    
    [Tooltip("List of room entry points (doors/triggers)")]
    public RoomEntryPoint[] roomEntryPoints;
    
    [Header("UI")]
    public Button exitInteriorButton;
    public GameObject exitButtonContainer;
    
    [Header("Fade Settings")]
    public Image fadeImage; // Full-screen image for fading
    public float fadeDuration = 1f;
    public Color fadeColor = Color.black;
    
    [Header("Current Room")]
    public RoomData currentRoom;
    
    private bool isTransitioning = false;
    
    void Start()
    {
        // Setup exit button
        if (exitInteriorButton != null)
        {
            exitInteriorButton.onClick.RemoveAllListeners();
            exitInteriorButton.onClick.AddListener(ExitInteriorMode);
        }
        
        // Hide exit button initially
        if (exitButtonContainer != null)
        {
            exitButtonContainer.SetActive(false);
        }
        
        // Setup fade image
        if (fadeImage != null)
        {
            fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, 0f);
            fadeImage.gameObject.SetActive(false);
        }
        
        // Initialize all room entry points
        foreach (var entryPoint in roomEntryPoints)
        {
            if (entryPoint != null && entryPoint.triggerCollider != null)
            {
                SetupRoomTrigger(entryPoint);
            }
        }
    }
    
    void SetupRoomTrigger(RoomEntryPoint entryPoint)
    {
        // Ensure trigger is set up correctly
        if (entryPoint.triggerCollider != null)
        {
            entryPoint.triggerCollider.isTrigger = true;
            
            // Add RoomTrigger component if not present
            RoomTrigger trigger = entryPoint.triggerCollider.GetComponent<RoomTrigger>();
            if (trigger == null)
            {
                trigger = entryPoint.triggerCollider.gameObject.AddComponent<RoomTrigger>();
            }
            trigger.manager = this;
            trigger.roomData = entryPoint.roomData;
        }
    }
    
    public void EnterInteriorMode(RoomData room)
    {
        if (isTransitioning || isInteriorMode) return;
        
        StartCoroutine(EnterInteriorCoroutine(room));
    }
    
    IEnumerator EnterInteriorCoroutine(RoomData room)
    {
        isTransitioning = true;
        currentRoom = room;
        
        // Save exterior position
        if (mainCamera != null)
        {
            savedExteriorPosition = mainCamera.transform.position;
            savedExteriorRotation = mainCamera.transform.rotation;
        }
        
        // Fade out
        yield return StartCoroutine(FadeOut());
        
        // Switch to interior mode
        isInteriorMode = true;
        
        // Move camera to room entry position
        if (mainCamera != null && room != null)
        {
            mainCamera.transform.position = room.entryPosition;
            mainCamera.transform.rotation = room.entryRotation;
        }
        
        // Enable interior lighting
        SetInteriorLighting(true);
        
        // Setup room boundaries
        SetupRoomBoundaries(room);
        
        // Show exit button
        if (exitButtonContainer != null)
        {
            exitButtonContainer.SetActive(true);
        }
        
        // Fade in
        yield return StartCoroutine(FadeIn());
        
        isTransitioning = false;
        Debug.Log($"[InteriorExteriorManager] Entered interior mode: {room.roomName}");
    }
    
    public void ExitInteriorMode()
    {
        if (isTransitioning || !isInteriorMode) return;
        
        StartCoroutine(ExitInteriorCoroutine());
    }
    
    IEnumerator ExitInteriorCoroutine()
    {
        isTransitioning = true;
        
        // Fade out
        yield return StartCoroutine(FadeOut());
        
        // Restore exterior position
        if (mainCamera != null)
        {
            mainCamera.transform.position = savedExteriorPosition;
            mainCamera.transform.rotation = savedExteriorRotation;
        }
        
        // Disable interior lighting
        SetInteriorLighting(false);
        
        // Remove room boundaries
        if (currentRoom != null)
        {
            RemoveRoomBoundaries(currentRoom);
        }
        
        // Hide exit button
        if (exitButtonContainer != null)
        {
            exitButtonContainer.SetActive(false);
        }
        
        // Switch to exterior mode
        isInteriorMode = false;
        currentRoom = null;
        
        // Fade in
        yield return StartCoroutine(FadeIn());
        
        isTransitioning = false;
        Debug.Log("[InteriorExteriorManager] Exited interior mode");
    }
    
    IEnumerator FadeOut()
    {
        if (fadeImage == null) yield break;
        
        fadeImage.gameObject.SetActive(true);
        float elapsed = 0f;
        
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Clamp01(elapsed / fadeDuration);
            fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, alpha);
            yield return null;
        }
        
        fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, 1f);
    }
    
    IEnumerator FadeIn()
    {
        if (fadeImage == null) yield break;
        
        float elapsed = 0f;
        
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Clamp01(1f - (elapsed / fadeDuration));
            fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, alpha);
            yield return null;
        }
        
        fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, 0f);
        fadeImage.gameObject.SetActive(false);
    }
    
    void SetInteriorLighting(bool enable)
    {
        // Find all lights in the scene
        Light[] allLights = FindObjectsOfType<Light>();
        
        foreach (Light light in allLights)
        {
            // Only affect lights that are part of the current room
            if (currentRoom != null && currentRoom.roomLights != null)
            {
                foreach (Light roomLight in currentRoom.roomLights)
                {
                    if (light == roomLight)
                    {
                        if (enable)
                        {
                            light.intensity *= interiorLightIntensity;
                        }
                        else
                        {
                            light.intensity /= interiorLightIntensity;
                        }
                    }
                }
            }
        }
        
        // Also adjust ambient light
        if (enable)
        {
            RenderSettings.ambientIntensity *= interiorLightIntensity;
        }
        else
        {
            RenderSettings.ambientIntensity /= interiorLightIntensity;
        }
    }
    
    void SetupRoomBoundaries(RoomData room)
    {
        if (room == null || room.boundaryColliders == null) return;
        
        // Enable boundary colliders
        foreach (Collider col in room.boundaryColliders)
        {
            if (col != null)
            {
                col.enabled = true;
            }
        }
    }
    
    void RemoveRoomBoundaries(RoomData room)
    {
        if (room == null || room.boundaryColliders == null) return;
        
        // Disable boundary colliders
        foreach (Collider col in room.boundaryColliders)
        {
            if (col != null)
            {
                col.enabled = false;
            }
        }
    }
    
    public bool IsInteriorMode()
    {
        return isInteriorMode;
    }
}

