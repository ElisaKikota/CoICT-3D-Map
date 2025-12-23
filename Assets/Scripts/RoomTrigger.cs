using UnityEngine;

/// <summary>
/// Trigger component that detects when player enters a room
/// </summary>
public class RoomTrigger : MonoBehaviour
{
    public InteriorExteriorManager manager;
    public RoomData roomData;
    
    void OnTriggerEnter(Collider other)
    {
        // Check if it's the player/camera
        if (other.CompareTag("Player") || other.CompareTag("MainCamera") || other.GetComponent<Camera>() != null)
        {
            if (manager != null && roomData != null && !manager.IsInteriorMode())
            {
                manager.EnterInteriorMode(roomData);
            }
        }
    }
}

