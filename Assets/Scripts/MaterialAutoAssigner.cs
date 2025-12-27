using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Automatically assigns materials to meshes based on naming patterns or manual assignment.
/// Useful when importing FBX files with unnamed meshes.
/// </summary>
public class MaterialAutoAssigner : MonoBehaviour
{
    [Header("Materials")]
    [Tooltip("Material to apply to grass meshes")]
    public Material grassMaterial;
    
    [Tooltip("Material to apply to ground/floor meshes")]
    public Material groundMaterial;
    
    [Tooltip("Material to apply to building meshes")]
    public Material buildingMaterial;
    
    [Header("Auto-Detection Settings")]
    [Tooltip("Keywords in mesh names that indicate grass (case-insensitive)")]
    public string[] grassKeywords = new string[] { "grass", "lawn", "terrain", "ground" };
    
    [Tooltip("Keywords in mesh names that indicate ground/floor (case-insensitive)")]
    public string[] groundKeywords = new string[] { "floor", "ground", "pavement", "road", "path" };
    
    [Header("Manual Assignment")]
    [Tooltip("Manually assign meshes to materials")]
    public List<MeshAssignment> manualAssignments = new List<MeshAssignment>();
    
    [System.Serializable]
    public class MeshAssignment
    {
        public GameObject meshObject;
        public Material material;
    }
    
    [ContextMenu("Auto-Assign Materials")]
    public void AutoAssignMaterials()
    {
        if (grassMaterial == null && groundMaterial == null && buildingMaterial == null)
        {
            Debug.LogWarning("[MaterialAutoAssigner] No materials assigned! Please assign materials in the Inspector.");
            return;
        }
        
        // Process manual assignments first
        foreach (var assignment in manualAssignments)
        {
            if (assignment.meshObject != null && assignment.material != null)
            {
                ApplyMaterialToMesh(assignment.meshObject, assignment.material);
            }
        }
        
        // Auto-detect and assign based on names
        Renderer[] allRenderers = GetComponentsInChildren<Renderer>(true);
        int grassCount = 0;
        int groundCount = 0;
        int buildingCount = 0;
        
        foreach (Renderer renderer in allRenderers)
        {
            if (renderer == null) continue;
            
            string meshName = renderer.gameObject.name.ToLower();
            bool assigned = false;
            
            // Check for grass
            if (grassMaterial != null)
            {
                foreach (string keyword in grassKeywords)
                {
                    if (meshName.Contains(keyword.ToLower()))
                    {
                        ApplyMaterialToMesh(renderer.gameObject, grassMaterial);
                        grassCount++;
                        assigned = true;
                        break;
                    }
                }
            }
            
            // Check for ground (if not already assigned as grass)
            if (!assigned && groundMaterial != null)
            {
                foreach (string keyword in groundKeywords)
                {
                    if (meshName.Contains(keyword.ToLower()))
                    {
                        ApplyMaterialToMesh(renderer.gameObject, groundMaterial);
                        groundCount++;
                        assigned = true;
                        break;
                    }
                }
            }
            
            // Default to building material if nothing matched
            if (!assigned && buildingMaterial != null)
            {
                ApplyMaterialToMesh(renderer.gameObject, buildingMaterial);
                buildingCount++;
            }
        }
        
        Debug.Log($"[MaterialAutoAssigner] Assigned materials - Grass: {grassCount}, Ground: {groundCount}, Building: {buildingCount}");
    }
    
    void ApplyMaterialToMesh(GameObject meshObject, Material material)
    {
        Renderer renderer = meshObject.GetComponent<Renderer>();
        if (renderer != null)
        {
            Material[] materials = new Material[renderer.sharedMaterials.Length];
            for (int i = 0; i < materials.Length; i++)
            {
                materials[i] = material;
            }
            renderer.sharedMaterials = materials;
            Debug.Log($"[MaterialAutoAssigner] Applied {material.name} to {meshObject.name}");
        }
    }
    
    [ContextMenu("List All Meshes")]
    public void ListAllMeshes()
    {
        Renderer[] allRenderers = GetComponentsInChildren<Renderer>(true);
        Debug.Log($"[MaterialAutoAssigner] Found {allRenderers.Length} renderers:");
        
        foreach (Renderer renderer in allRenderers)
        {
            if (renderer != null)
            {
                Debug.Log($"  - {renderer.gameObject.name} (Materials: {renderer.sharedMaterials.Length})");
            }
        }
    }
    
    [ContextMenu("Create Procedural Grass Material")]
    public void CreateProceduralGrassMaterial()
    {
        Material mat = new Material(Shader.Find("Standard"));
        mat.name = "ProceduralGrass";
        mat.color = new Color(0.2f, 0.6f, 0.2f); // Green color
        mat.SetFloat("_Glossiness", 0.3f);
        mat.SetFloat("_Metallic", 0f);
        
        // Save as asset
        #if UNITY_EDITOR
        string path = "Assets/Materials/ProceduralGrass.mat";
        UnityEditor.AssetDatabase.CreateAsset(mat, path);
        UnityEditor.AssetDatabase.SaveAssets();
        Debug.Log($"[MaterialAutoAssigner] Created grass material at {path}");
        #endif
        
        grassMaterial = mat;
    }
    
    [ContextMenu("Create Procedural Ground Material")]
    public void CreateProceduralGroundMaterial()
    {
        Material mat = new Material(Shader.Find("Standard"));
        mat.name = "ProceduralGround";
        mat.color = new Color(0.5f, 0.5f, 0.5f); // Gray color
        mat.SetFloat("_Glossiness", 0.1f);
        mat.SetFloat("_Metallic", 0f);
        
        // Save as asset
        #if UNITY_EDITOR
        string path = "Assets/Materials/ProceduralGround.mat";
        UnityEditor.AssetDatabase.CreateAsset(mat, path);
        UnityEditor.AssetDatabase.SaveAssets();
        Debug.Log($"[MaterialAutoAssigner] Created ground material at {path}");
        #endif
        
        groundMaterial = mat;
    }
}



