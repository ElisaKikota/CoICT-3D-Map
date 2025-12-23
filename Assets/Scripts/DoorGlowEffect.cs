using UnityEngine;
using System.Collections;

/// <summary>
/// Creates a glowing effect on doors when a building is highlighted
/// </summary>
public class DoorGlowEffect : MonoBehaviour
{
    [Header("Glow Settings")]
    [Tooltip("Color of the glow effect")]
    public Color glowColor = new Color(1f, 0.8f, 0f, 1f); // Yellow/orange glow
    
    [Tooltip("Intensity of the glow")]
    public float glowIntensity = 2f;
    
    [Tooltip("Speed of the pulsing animation")]
    public float pulseSpeed = 2f;
    
    [Tooltip("Minimum glow intensity (for pulsing)")]
    public float minGlowIntensity = 1f;
    
    [Tooltip("Maximum glow intensity (for pulsing)")]
    public float maxGlowIntensity = 3f;
    
    [Header("References")]
    [Tooltip("Renderer component of the door")]
    public Renderer doorRenderer;
    
    [Tooltip("Optional: Additional renderers for door parts")]
    public Renderer[] additionalRenderers;
    
    private Material glowMaterial;
    private Material[] originalMaterials;
    private Material[] originalAdditionalMaterials;
    private bool isGlowing = false;
    private Coroutine glowCoroutine;
    
    void Start()
    {
        if (doorRenderer == null)
        {
            doorRenderer = GetComponent<Renderer>();
        }
        
        if (doorRenderer != null)
        {
            // Store original materials
            originalMaterials = new Material[doorRenderer.materials.Length];
            for (int i = 0; i < originalMaterials.Length; i++)
            {
                originalMaterials[i] = doorRenderer.materials[i];
            }
            
            // Create glow material
            CreateGlowMaterial();
        }
        
        // Store original materials for additional renderers
        if (additionalRenderers != null && additionalRenderers.Length > 0)
        {
            originalAdditionalMaterials = new Material[additionalRenderers.Length][];
            for (int i = 0; i < additionalRenderers.Length; i++)
            {
                if (additionalRenderers[i] != null)
                {
                    originalAdditionalMaterials[i] = new Material[additionalRenderers[i].materials.Length];
                    for (int j = 0; j < originalAdditionalMaterials[i].Length; j++)
                    {
                        originalAdditionalMaterials[i][j] = additionalRenderers[i].materials[j];
                    }
                }
            }
        }
    }
    
    void CreateGlowMaterial()
    {
        glowMaterial = new Material(Shader.Find("Standard"));
        glowMaterial.name = "DoorGlowMaterial";
        glowMaterial.SetColor("_Color", glowColor);
        glowMaterial.SetColor("_EmissionColor", glowColor * glowIntensity);
        glowMaterial.EnableKeyword("_EMISSION");
        glowMaterial.SetFloat("_Glossiness", 0.5f);
        glowMaterial.SetFloat("_Metallic", 0f);
    }
    
    /// <summary>
    /// Start glowing effect
    /// </summary>
    public void StartGlow()
    {
        if (isGlowing) return;
        
        isGlowing = true;
        
        // Apply glow material to main renderer
        if (doorRenderer != null && glowMaterial != null)
        {
            Material[] glowMaterials = new Material[doorRenderer.materials.Length];
            for (int i = 0; i < glowMaterials.Length; i++)
            {
                glowMaterials[i] = glowMaterial;
            }
            doorRenderer.materials = glowMaterials;
        }
        
        // Apply glow material to additional renderers
        if (additionalRenderers != null)
        {
            foreach (Renderer renderer in additionalRenderers)
            {
                if (renderer != null && glowMaterial != null)
                {
                    Material[] glowMaterials = new Material[renderer.materials.Length];
                    for (int i = 0; i < glowMaterials.Length; i++)
                    {
                        glowMaterials[i] = glowMaterial;
                    }
                    renderer.materials = glowMaterials;
                }
            }
        }
        
        // Start pulsing animation
        if (glowCoroutine != null)
        {
            StopCoroutine(glowCoroutine);
        }
        glowCoroutine = StartCoroutine(PulseGlow());
        
        Debug.Log($"[DoorGlowEffect] Started glow on {gameObject.name}");
    }
    
    /// <summary>
    /// Stop glowing effect
    /// </summary>
    public void StopGlow()
    {
        if (!isGlowing) return;
        
        isGlowing = false;
        
        // Restore original materials
        if (doorRenderer != null && originalMaterials != null)
        {
            doorRenderer.materials = originalMaterials;
        }
        
        // Restore original materials for additional renderers
        if (additionalRenderers != null && originalAdditionalMaterials != null)
        {
            for (int i = 0; i < additionalRenderers.Length; i++)
            {
                if (additionalRenderers[i] != null && originalAdditionalMaterials[i] != null)
                {
                    additionalRenderers[i].materials = originalAdditionalMaterials[i];
                }
            }
        }
        
        // Stop pulsing animation
        if (glowCoroutine != null)
        {
            StopCoroutine(glowCoroutine);
            glowCoroutine = null;
        }
        
        Debug.Log($"[DoorGlowEffect] Stopped glow on {gameObject.name}");
    }
    
    /// <summary>
    /// Pulsing animation coroutine
    /// </summary>
    IEnumerator PulseGlow()
    {
        while (isGlowing)
        {
            float pulse = Mathf.PingPong(Time.time * pulseSpeed, 1f);
            float currentIntensity = Mathf.Lerp(minGlowIntensity, maxGlowIntensity, pulse);
            
            if (glowMaterial != null)
            {
                glowMaterial.SetColor("_EmissionColor", glowColor * currentIntensity);
            }
            
            yield return null;
        }
    }
    
    void OnDestroy()
    {
        StopGlow();
        
        if (glowMaterial != null)
        {
            Destroy(glowMaterial);
        }
    }
}

