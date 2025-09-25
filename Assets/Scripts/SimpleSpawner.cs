using UnityEngine;
using System.Collections.Generic;

// Optional: assign in inspector prefabs + target transforms to spawn at start
public class SimpleSpawner : MonoBehaviour
{
    [System.Serializable]
    public class SpawnEntry {
        public string name;
        public GameObject prefab;
        public Transform spawnTransform; // position/rotation of target
    }

    public List<SpawnEntry> entries = new List<SpawnEntry>();

    void Start()
    {
        foreach (var e in entries)
        {
            if (e.prefab == null) continue;
            Vector3 pos = e.spawnTransform != null ? e.spawnTransform.position : e.prefab.transform.position;
            Quaternion rot = e.spawnTransform != null ? e.spawnTransform.rotation : e.prefab.transform.rotation;
            Instantiate(e.prefab, pos, rot, transform);
        }
    }
}
