using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class BuildingHighlightManager : MonoBehaviour
{
    [Header("Highlight Settings")]
    public Color highlightColor = Color.cyan;
    public Color normalColor = Color.white;
    public float transparencyAmount = 0.3f; // How transparent other buildings become
    public float highlightIntensity = 2f; // How bright the highlighted building becomes
    
    [Header("Building References")]
    public Transform campusRoot; // Reference to CampusRoot containing all buildings
    public List<GameObject> allBuildingObjects = new List<GameObject>();
    
    [Header("Materials")]
    public Material highlightMaterial;
    public Material transparentMaterial;
    
    private Dictionary<GameObject, Material[]> originalMaterials = new Dictionary<GameObject, Material[]>();
    private Dictionary<GameObject, Renderer[]> buildingRenderers = new Dictionary<GameObject, Renderer[]>();
    private GameObject currentlyHighlighted = null;
    private bool isHighlightMode = false;
    
    void Start()
    {
        InitializeBuildingReferences();
        CreateMaterials();
    }
    
    void InitializeBuildingReferences()
    {
        // Find all building objects under CampusRoot
        if (campusRoot != null)
        {
            allBuildingObjects.Clear();
            
            // Get all child objects (buildings) from CampusRoot
            for (int i = 0; i < campusRoot.childCount; i++)
            {
                GameObject building = campusRoot.GetChild(i).gameObject;
                allBuildingObjects.Add(building);
                
                // Store original materials and renderers
                StoreOriginalMaterials(building);
                
                Debug.Log($"[BuildingHighlightManager] Found building: {building.name}");
            }
            
            Debug.Log($"[BuildingHighlightManager] Initialized {allBuildingObjects.Count} buildings");
        }
        else
        {
            Debug.LogWarning("[BuildingHighlightManager] CampusRoot not assigned! Please assign the CampusRoot transform.");
        }
    }
    
    void StoreOriginalMaterials(GameObject building)
    {
        Renderer[] renderers = building.GetComponentsInChildren<Renderer>();
        buildingRenderers[building] = renderers;
        
        // Store original materials for each renderer
        Material[] originalMats = new Material[renderers.Length];
        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i].materials.Length > 0)
            {
                originalMats[i] = renderers[i].materials[0]; // Store first material
            }
        }
        originalMaterials[building] = originalMats;
    }
    
    void CreateMaterials()
    {
        // Create highlight material with fallback shaders
        if (highlightMaterial == null)
        {
            Shader highlightShader = Shader.Find("Standard");
            if (highlightShader == null)
            {
                highlightShader = Shader.Find("Legacy Shaders/Diffuse");
            }
            if (highlightShader == null)
            {
                highlightShader = Shader.Find("Unlit/Color");
            }
            
            if (highlightShader != null)
            {
                highlightMaterial = new Material(highlightShader);
                highlightMaterial.color = highlightColor;
                highlightMaterial.SetFloat("_Metallic", 0f);
                highlightMaterial.SetFloat("_Smoothness", 0.8f);
                highlightMaterial.EnableKeyword("_EMISSION");
                highlightMaterial.SetColor("_EmissionColor", highlightColor * highlightIntensity);
                Debug.Log("[BuildingHighlightManager] Created highlight material with shader: " + highlightShader.name);
            }
            else
            {
                Debug.LogError("[BuildingHighlightManager] Could not find any suitable shader for highlight material!");
            }
        }
        
        // Create transparent material with fallback shaders
        if (transparentMaterial == null)
        {
            Shader transparentShader = Shader.Find("Standard");
            if (transparentShader == null)
            {
                transparentShader = Shader.Find("Legacy Shaders/Transparent/Diffuse");
            }
            if (transparentShader == null)
            {
                transparentShader = Shader.Find("Unlit/Transparent");
            }
            
            if (transparentShader != null)
            {
                transparentMaterial = new Material(transparentShader);
                transparentMaterial.color = new Color(normalColor.r, normalColor.g, normalColor.b, transparencyAmount);
                transparentMaterial.SetFloat("_Mode", 3); // Transparent mode
                transparentMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                transparentMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                transparentMaterial.SetInt("_ZWrite", 0);
                transparentMaterial.DisableKeyword("_ALPHATEST_ON");
                transparentMaterial.EnableKeyword("_ALPHABLEND_ON");
                transparentMaterial.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                transparentMaterial.renderQueue = 3000;
                Debug.Log("[BuildingHighlightManager] Created transparent material with shader: " + transparentShader.name);
            }
            else
            {
                Debug.LogError("[BuildingHighlightManager] Could not find any suitable shader for transparent material!");
            }
        }
    }
    
    public void HighlightBuilding(string buildingName)
    {
        GameObject building = FindBuildingByName(buildingName);
        if (building != null)
        {
            HighlightBuilding(building);
        }
        else
        {
            Debug.LogWarning($"[BuildingHighlightManager] Building '{buildingName}' not found!");
        }
    }
    
    public void HighlightBuilding(GameObject building)
    {
        if (building == null)
        {
            Debug.LogWarning("[BuildingHighlightManager] Cannot highlight null building");
            return;
        }
        
        // Reset any previous highlight
        ResetAllBuildings();
        
        // Set highlight mode
        isHighlightMode = true;
        currentlyHighlighted = building;
        
        // Apply highlight to selected building
        ApplyHighlightToBuilding(building);
        
        // Apply transparency to other buildings
        ApplyTransparencyToOtherBuildings(building);
        
        Debug.Log($"[BuildingHighlightManager] Highlighted building: {building.name}");
    }
    
    void ApplyHighlightToBuilding(GameObject building)
    {
        if (buildingRenderers.ContainsKey(building))
        {
            Renderer[] renderers = buildingRenderers[building];
            foreach (Renderer renderer in renderers)
            {
                if (renderer != null)
                {
                    // Apply highlight material
                    Material[] newMaterials = new Material[renderer.materials.Length];
                    for (int i = 0; i < newMaterials.Length; i++)
                    {
                        newMaterials[i] = highlightMaterial;
                    }
                    renderer.materials = newMaterials;
                }
            }
        }
    }
    
    void ApplyTransparencyToOtherBuildings(GameObject highlightedBuilding)
    {
        foreach (GameObject building in allBuildingObjects)
        {
            if (building != highlightedBuilding && buildingRenderers.ContainsKey(building))
            {
                Renderer[] renderers = buildingRenderers[building];
                foreach (Renderer renderer in renderers)
                {
                    if (renderer != null)
                    {
                        // Apply transparent material
                        Material[] newMaterials = new Material[renderer.materials.Length];
                        for (int i = 0; i < newMaterials.Length; i++)
                        {
                            newMaterials[i] = transparentMaterial;
                        }
                        renderer.materials = newMaterials;
                    }
                }
            }
        }
    }
    
    public void ResetAllBuildings()
    {
        // Restore original materials to all buildings
        foreach (GameObject building in allBuildingObjects)
        {
            if (buildingRenderers.ContainsKey(building) && originalMaterials.ContainsKey(building))
            {
                Renderer[] renderers = buildingRenderers[building];
                Material[] originalMats = originalMaterials[building];
                
                for (int i = 0; i < renderers.Length && i < originalMats.Length; i++)
                {
                    if (renderers[i] != null && originalMats[i] != null)
                    {
                        renderers[i].material = originalMats[i];
                    }
                }
            }
        }
        
        isHighlightMode = false;
        currentlyHighlighted = null;
        
        Debug.Log("[BuildingHighlightManager] Reset all buildings to original materials");
    }
    
    // Emergency reset method to fix pink materials
    public void EmergencyResetAllBuildings()
    {
        Debug.Log("[BuildingHighlightManager] Emergency reset - restoring all buildings to default materials");
        
        foreach (GameObject building in allBuildingObjects)
        {
            if (buildingRenderers.ContainsKey(building))
            {
                Renderer[] renderers = buildingRenderers[building];
                foreach (Renderer renderer in renderers)
                {
                    if (renderer != null)
                    {
                        // Create a simple default material
                        Material defaultMat = new Material(Shader.Find("Legacy Shaders/Diffuse"));
                        if (defaultMat.shader != null)
                        {
                            defaultMat.color = Color.white;
                            renderer.material = defaultMat;
                        }
                    }
                }
            }
        }
        
        isHighlightMode = false;
        currentlyHighlighted = null;
        
        Debug.Log("[BuildingHighlightManager] Emergency reset completed");
    }
    
    GameObject FindBuildingByName(string buildingName)
    {
        // Try exact name match first
        GameObject found = allBuildingObjects.FirstOrDefault(b => b.name == buildingName);
        if (found != null) return found;
        
        // Try partial name match
        found = allBuildingObjects.FirstOrDefault(b => b.name.Contains(buildingName));
        if (found != null) return found;
        
        // Try case-insensitive match
        found = allBuildingObjects.FirstOrDefault(b => b.name.ToLower().Contains(buildingName.ToLower()));
        return found;
    }
    
    // Public getters
    public bool IsHighlightMode()
    {
        return isHighlightMode;
    }
    
    public GameObject GetCurrentlyHighlighted()
    {
        return currentlyHighlighted;
    }
    
    public List<GameObject> GetAllBuildings()
    {
        return allBuildingObjects;
    }
    
    // Method to refresh building list (useful if buildings are added dynamically)
    public void RefreshBuildingList()
    {
        InitializeBuildingReferences();
    }
}
