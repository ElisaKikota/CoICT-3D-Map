using UnityEngine;
using System.Collections;

/// <summary>
/// Component that animates the alpha value of a material using a smooth sine wave
/// Works with transparent materials by modifying the color alpha
/// </summary>
[RequireComponent(typeof(Renderer))]
public class AlphaWaveAnimator : MonoBehaviour
{
    [Header("Animation Settings")]
    [Tooltip("Speed of the alpha wave animation")]
    [SerializeField] private float waveSpeed = 2f;
    
    [Tooltip("Minimum alpha value (0 = fully transparent)")]
    [SerializeField] private float minAlpha = 0.3f;
    
    [Tooltip("Maximum alpha value (1 = fully opaque)")]
    [SerializeField] private float maxAlpha = 1f;
    
    [Tooltip("Enable/disable the alpha wave animation")]
    [SerializeField] private bool enableAnimation = true;
    
    [Header("Material Settings")]
    [Tooltip("Property name to use for color (usually '_Color' or '_BaseColor')")]
    [SerializeField] private string colorPropertyName = "_Color";
    
    private Renderer targetRenderer;
    private MaterialPropertyBlock propertyBlock;
    private Color originalColor;
    private bool isAnimating = false;
    private Coroutine animationCoroutine;
    
    void Start()
    {
        targetRenderer = GetComponent<Renderer>();
        if (targetRenderer == null)
        {
            Debug.LogWarning($"[AlphaWaveAnimator] No Renderer found on {gameObject.name}!");
            enabled = false;
            return;
        }
        
        // Initialize property block
        propertyBlock = new MaterialPropertyBlock();
        
        // Get original color from material
        if (targetRenderer.materials != null && targetRenderer.materials.Length > 0)
        {
            Material mat = targetRenderer.materials[0];
            if (mat != null)
            {
                if (mat.HasProperty(colorPropertyName))
                {
                    originalColor = mat.GetColor(colorPropertyName);
                }
                else if (mat.HasProperty("_Color"))
                {
                    originalColor = mat.GetColor("_Color");
                    colorPropertyName = "_Color";
                }
                else if (mat.HasProperty("_BaseColor"))
                {
                    originalColor = mat.GetColor("_BaseColor");
                    colorPropertyName = "_BaseColor";
                }
                else
                {
                    originalColor = mat.color;
                }
                
                Debug.Log($"[AlphaWaveAnimator] Found material on {gameObject.name}, shader: {mat.shader.name}, color property: {colorPropertyName}, original alpha: {originalColor.a}");
            }
        }
        
        // Start animation if enabled
        if (enableAnimation)
        {
            StartAnimation();
        }
    }
    
    void OnEnable()
    {
        // Initialize property block if not already done (in case OnEnable runs before Start)
        if (propertyBlock == null)
        {
            propertyBlock = new MaterialPropertyBlock();
        }
        
        // Ensure targetRenderer is set
        if (targetRenderer == null)
        {
            targetRenderer = GetComponent<Renderer>();
        }
        
        if (enableAnimation && !isAnimating)
        {
            StartAnimation();
        }
    }
    
    void OnDisable()
    {
        StopAnimation();
    }
    
    /// <summary>
    /// Starts the alpha wave animation
    /// </summary>
    public void StartAnimation()
    {
        if (isAnimating) return;
        
        // Initialize property block if not already done
        if (propertyBlock == null)
        {
            propertyBlock = new MaterialPropertyBlock();
        }
        
        if (targetRenderer == null)
        {
            targetRenderer = GetComponent<Renderer>();
            if (targetRenderer == null) return;
        }
        
        // Ensure original color is set if not already (in case Start hasn't run)
        if (originalColor == Color.clear && targetRenderer.materials != null && targetRenderer.materials.Length > 0)
        {
            Material mat = targetRenderer.materials[0];
            if (mat != null)
            {
                if (mat.HasProperty(colorPropertyName))
                {
                    originalColor = mat.GetColor(colorPropertyName);
                }
                else if (mat.HasProperty("_Color"))
                {
                    originalColor = mat.GetColor("_Color");
                    colorPropertyName = "_Color";
                }
                else if (mat.HasProperty("_BaseColor"))
                {
                    originalColor = mat.GetColor("_BaseColor");
                    colorPropertyName = "_BaseColor";
                }
                else
                {
                    originalColor = mat.color;
                }
            }
        }
        
        isAnimating = true;
        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
        }
        animationCoroutine = StartCoroutine(AnimateAlpha());
    }
    
    /// <summary>
    /// Stops the alpha wave animation
    /// </summary>
    public void StopAnimation()
    {
        isAnimating = false;
        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
            animationCoroutine = null;
        }
        
        // Initialize property block if null
        if (propertyBlock == null)
        {
            propertyBlock = new MaterialPropertyBlock();
        }
        
        // Restore original alpha
        if (targetRenderer != null)
        {
            RestoreOriginalAlpha();
        }
    }
    
    /// <summary>
    /// Restores the original alpha value
    /// </summary>
    void RestoreOriginalAlpha()
    {
        if (targetRenderer == null) return;
        
        // Initialize property block if null
        if (propertyBlock == null)
        {
            propertyBlock = new MaterialPropertyBlock();
        }
        
        targetRenderer.GetPropertyBlock(propertyBlock);
        propertyBlock.SetColor(colorPropertyName, originalColor);
        targetRenderer.SetPropertyBlock(propertyBlock);
        
        // Also update material directly as fallback
        if (targetRenderer.materials != null)
        {
            foreach (Material mat in targetRenderer.materials)
            {
                if (mat != null)
                {
                    if (mat.HasProperty(colorPropertyName))
                    {
                        mat.SetColor(colorPropertyName, originalColor);
                    }
                    else if (mat.HasProperty("_Color"))
                    {
                        mat.SetColor("_Color", originalColor);
                    }
                    else if (mat.HasProperty("_BaseColor"))
                    {
                        mat.SetColor("_BaseColor", originalColor);
                    }
                    mat.color = originalColor;
                }
            }
        }
    }
    
    /// <summary>
    /// Coroutine that animates the alpha value using a sine wave
    /// </summary>
    IEnumerator AnimateAlpha()
    {
        // Ensure property block is initialized
        if (propertyBlock == null)
        {
            propertyBlock = new MaterialPropertyBlock();
        }
        
        // Ensure targetRenderer is set
        if (targetRenderer == null)
        {
            targetRenderer = GetComponent<Renderer>();
        }
        
        while (isAnimating && targetRenderer != null)
        {
            // Calculate alpha using sine wave for smooth pulsing
            float normalizedWave = (Mathf.Sin(Time.time * waveSpeed) + 1f) / 2f;
            float alpha = Mathf.Lerp(minAlpha, maxAlpha, normalizedWave);
            
            // Create color with updated alpha
            Color animatedColor = originalColor;
            animatedColor.a = alpha;
            
            // Apply using MaterialPropertyBlock (preferred method)
            // Ensure propertyBlock is not null before using
            if (propertyBlock == null)
            {
                propertyBlock = new MaterialPropertyBlock();
            }
            
            targetRenderer.GetPropertyBlock(propertyBlock);
            propertyBlock.SetColor(colorPropertyName, animatedColor);
            
            // Also try alternative property names
            propertyBlock.SetColor("_Color", animatedColor);
            propertyBlock.SetColor("_BaseColor", animatedColor);
            propertyBlock.SetFloat("_Alpha", alpha);
            
            targetRenderer.SetPropertyBlock(propertyBlock);
            
            // Fallback: Direct material modification
            if (targetRenderer.materials != null)
            {
                foreach (Material mat in targetRenderer.materials)
                {
                    if (mat != null)
                    {
                        // Try the specified property name
                        if (mat.HasProperty(colorPropertyName))
                        {
                            mat.SetColor(colorPropertyName, animatedColor);
                        }
                        
                        // Try common property names
                        if (mat.HasProperty("_Color"))
                        {
                            mat.SetColor("_Color", animatedColor);
                        }
                        
                        if (mat.HasProperty("_BaseColor"))
                        {
                            mat.SetColor("_BaseColor", animatedColor);
                        }
                        
                        if (mat.HasProperty("_Alpha"))
                        {
                            mat.SetFloat("_Alpha", alpha);
                        }
                        
                        // Also set color directly
                        mat.color = animatedColor;
                    }
                }
            }
            
            yield return null;
        }
    }
    
    /// <summary>
    /// Sets the animation parameters
    /// </summary>
    public void SetAnimationParameters(float speed, float min, float max)
    {
        waveSpeed = speed;
        minAlpha = min;
        maxAlpha = max;
    }
    
    /// <summary>
    /// Enables or disables the animation
    /// </summary>
    public void SetEnabled(bool enabled)
    {
        enableAnimation = enabled;
        if (enabled)
        {
            StartAnimation();
        }
        else
        {
            StopAnimation();
        }
    }
}

