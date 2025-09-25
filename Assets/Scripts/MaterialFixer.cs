using UnityEngine;

public class MaterialFixer : MonoBehaviour
{
    [Header("Emergency Fix")]
    public KeyCode resetKey = KeyCode.R;
    public BuildingHighlightManager highlightManager;
    public BottomSheetManager bottomSheetManager;
    
    void Update()
    {
        // Press R to emergency reset materials
        if (Input.GetKeyDown(resetKey))
        {
            EmergencyReset();
        }
    }
    
    public void EmergencyReset()
    {
        Debug.Log("[MaterialFixer] Emergency reset triggered!");
        
        if (highlightManager != null)
        {
            highlightManager.EmergencyResetAllBuildings();
        }
        
        if (bottomSheetManager != null)
        {
            bottomSheetManager.EmergencyResetMaterials();
        }
        
        // Also try to reset all renderers in the scene
        Renderer[] allRenderers = FindObjectsOfType<Renderer>();
        foreach (Renderer renderer in allRenderers)
        {
            if (renderer != null)
            {
                // Create a simple white material
                Material defaultMat = new Material(Shader.Find("Legacy Shaders/Diffuse"));
                if (defaultMat.shader != null)
                {
                    defaultMat.color = Color.white;
                    renderer.material = defaultMat;
                }
            }
        }
        
        Debug.Log("[MaterialFixer] Emergency reset completed!");
    }
}





