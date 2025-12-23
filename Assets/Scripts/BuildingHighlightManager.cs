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
    
    [Header("Door Glow")]
    [Tooltip("Enable door glow effect when highlighting buildings")]
    public bool enableDoorGlow = true;
    
    [Header("Materials")]
    public Material highlightMaterial;
    public Material transparentMaterial;
    
    private Dictionary<GameObject, Material[]> originalMaterials = new Dictionary<GameObject, Material[]>();
    private Dictionary<GameObject, Renderer[]> buildingRenderers = new Dictionary<GameObject, Renderer[]>();
    private GameObject currentlyHighlighted = null;
    private bool isHighlightMode = false;
    private DoorGlowEffect[] currentDoorGlows = null;
    
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
        
        // Enable door glow effect
        if (enableDoorGlow)
        {
            EnableDoorGlow(building);
        }
        
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
        
        // Disable door glow
        DisableDoorGlow();
        
        isHighlightMode = false;
        currentlyHighlighted = null;
        
        Debug.Log("[BuildingHighlightManager] Reset all buildings to original materials");
    }
    
    void EnableDoorGlow(GameObject building)
    {
        // Disable any existing door glows first
        DisableDoorGlow();
        
        // Find door objects in the building
        DoorGlowEffect[] doorGlows = building.GetComponentsInChildren<DoorGlowEffect>(true);
        
        if (doorGlows != null && doorGlows.Length > 0)
        {
            currentDoorGlows = doorGlows;
            foreach (DoorGlowEffect glow in doorGlows)
            {
                if (glow != null)
                {
                    glow.StartGlow();
                }
            }
            Debug.Log($"[BuildingHighlightManager] Enabled door glow on {doorGlows.Length} doors");
        }
        else
        {
            // Try to find doors by name or tag
            Transform[] allChildren = building.GetComponentsInChildren<Transform>(true);
            List<DoorGlowEffect> foundGlows = new List<DoorGlowEffect>();
            
            foreach (Transform child in allChildren)
            {
                string childName = child.name.ToLower();
                if (childName.Contains("door") || childName.Contains("entrance") || childName.Contains("entry"))
                {
                    DoorGlowEffect glow = child.GetComponent<DoorGlowEffect>();
                    if (glow == null)
                    {
                        glow = child.gameObject.AddComponent<DoorGlowEffect>();
                    }
                    glow.StartGlow();
                    foundGlows.Add(glow);
                }
            }
            
            if (foundGlows.Count > 0)
            {
                currentDoorGlows = foundGlows.ToArray();
                Debug.Log($"[BuildingHighlightManager] Created and enabled door glow on {foundGlows.Count} doors");
            }
        }
    }
    
    void DisableDoorGlow()
    {
        if (currentDoorGlows != null)
        {
            foreach (DoorGlowEffect glow in currentDoorGlows)
            {
                if (glow != null)
                {
                    glow.StopGlow();
                }
            }
            currentDoorGlows = null;
        }
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
