using UnityEngine;

/// <summary>
/// Trigger component that detects when camera leaves the boundary
/// </summary>
public class BoundaryTrigger : MonoBehaviour
{
    public BoundaryController boundaryController;
    
    void OnTriggerExit(Collider other)
    {
        // This is called when something leaves the boundary
        // The BoundaryController will handle clamping in Update()
    }
}

