using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class BuildingVisibilityManager : MonoBehaviour
{
    Dictionary<string, GameObject> buildings = new Dictionary<string, GameObject>();

    void Awake()
    {
        CacheBuildings();
    }

    public void CacheBuildings()
    {
        buildings.Clear();
        foreach (Transform t in transform)
        {
            if (!buildings.ContainsKey(t.name))
                buildings.Add(t.name, t.gameObject);
        }
    }

    public void ToggleBuilding(string buildingName)
    {
        if (buildings.TryGetValue(buildingName, out GameObject go))
            go.SetActive(!go.activeSelf);
        else Debug.LogWarning($"BuildingVisibilityManager: no building named '{buildingName}'");
    }

    public void SetBuildingActive(string buildingName, bool active)
    {
        if (buildings.TryGetValue(buildingName, out GameObject go))
            go.SetActive(active);
        else Debug.LogWarning($"BuildingVisibilityManager: no building named '{buildingName}'");
    }

    public void SetAllBuildingsActive(bool active)
    {
        foreach (var kv in buildings) kv.Value.SetActive(active);
    }

    public void SetFogBoxesActive(bool active)
    {
        foreach (var kv in buildings)
        {
            Transform box = kv.Value.transform.Find("LOD_Box");
            if (box) box.gameObject.SetActive(active);
        }
    }
}
