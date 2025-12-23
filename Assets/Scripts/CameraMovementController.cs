using UnityEngine;
using System.Collections;

/// <summary>
/// Handles smooth camera movement to building positions with two-phase interpolation:
/// 1. Move up vertically
/// 2. Move horizontally to best viewing angle
/// </summary>
public class CameraMovementController : MonoBehaviour
{
    [Header("References")]
    public Camera mainCamera;
    public CameraModeController cameraModeController;
    
    [Header("Movement Settings")]
    [Tooltip("Height to reach before horizontal movement (in drone mode)")]
    public float intermediateHeight = 80f;
    
    [Tooltip("Speed of vertical movement")]
    public float verticalSpeed = 10f;
    
    [Tooltip("Speed of horizontal movement")]
    public float horizontalSpeed = 8f;
    
    [Tooltip("Rotation speed when moving to building")]
    public float rotationSpeed = 5f;
    
    [Tooltip("Minimum distance to consider 'arrived'")]
    public float arrivalThreshold = 0.5f;
    
    [Header("Animation Curves")]
    public AnimationCurve verticalCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    public AnimationCurve horizontalCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    private bool isMoving = false;
    private Coroutine currentMovementCoroutine;
    
    void Start()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }
    }
    
    /// <summary>
    /// Move camera to building's best viewing position with smooth interpolation
    /// </summary>
    public void MoveToBuilding(BuildingData building, bool useDroneModePath = false)
    {
        if (building == null || mainCamera == null)
        {
            Debug.LogWarning("[CameraMovementController] Cannot move - building or camera is null");
            return;
        }
        
        // Stop any existing movement
        if (currentMovementCoroutine != null)
        {
            StopCoroutine(currentMovementCoroutine);
        }
        
        // Check if we're in drone mode and should use two-phase movement
        if (useDroneModePath && cameraModeController != null && cameraModeController.currentMode == TourMode.Drone)
        {
            currentMovementCoroutine = StartCoroutine(MoveToBuildingTwoPhase(building));
        }
        else
        {
            currentMovementCoroutine = StartCoroutine(MoveToBuildingDirect(building));
        }
    }
    
    /// <summary>
    /// Two-phase movement: up first, then horizontal (for drone mode)
    /// </summary>
    IEnumerator MoveToBuildingTwoPhase(BuildingData building)
    {
        isMoving = true;
        Vector3 startPos = mainCamera.transform.position;
        Vector3 startRot = mainCamera.transform.eulerAngles;
        
        Vector3 intermediatePos = new Vector3(startPos.x, intermediateHeight, startPos.z);
        Vector3 targetPos = building.bestViewPosition != Vector3.zero ? building.bestViewPosition : building.cameraViewPosition;
        
        // Calculate target rotation to look at building
        Vector3 buildingPos = building.position;
        Vector3 directionToBuilding = (buildingPos - targetPos).normalized;
        Quaternion targetRotation;
        
        // Use best view rotation if specified, otherwise calculate from direction
        if (building.bestViewRotation != Vector3.zero)
        {
            targetRotation = Quaternion.Euler(building.bestViewRotation);
        }
        else
        {
            targetRotation = Quaternion.LookRotation(directionToBuilding);
        }
        
        Debug.Log($"[CameraMovementController] Starting two-phase movement to {building.name}");
        Debug.Log($"  Phase 1: {startPos} -> {intermediatePos} (vertical)");
        Debug.Log($"  Phase 2: {intermediatePos} -> {targetPos} (horizontal)");
        
        // Phase 1: Move up vertically
        float phase1Duration = Vector3.Distance(startPos, intermediatePos) / verticalSpeed;
        float elapsed = 0f;
        
        while (elapsed < phase1Duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / phase1Duration);
            float curveValue = verticalCurve.Evaluate(t);
            
            mainCamera.transform.position = Vector3.Lerp(startPos, intermediatePos, curveValue);
            yield return null;
        }
        
        mainCamera.transform.position = intermediatePos;
        Debug.Log("[CameraMovementController] Phase 1 complete - reached intermediate height");
        
        // Phase 2: Move horizontally to target position
        float phase2Duration = Vector3.Distance(intermediatePos, targetPos) / horizontalSpeed;
        elapsed = 0f;
        
        while (elapsed < phase2Duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / phase2Duration);
            float curveValue = horizontalCurve.Evaluate(t);
            
            mainCamera.transform.position = Vector3.Lerp(intermediatePos, targetPos, curveValue);
            
            // Smoothly rotate to look at building
            Quaternion currentRot = mainCamera.transform.rotation;
            mainCamera.transform.rotation = Quaternion.Slerp(currentRot, targetRotation, Time.deltaTime * rotationSpeed);
            
            yield return null;
        }
        
        mainCamera.transform.position = targetPos;
        mainCamera.transform.rotation = targetRotation;
        
        isMoving = false;
        Debug.Log($"[CameraMovementController] Arrived at {building.name}");
    }
    
    /// <summary>
    /// Direct movement to target (for non-drone modes)
    /// </summary>
    IEnumerator MoveToBuildingDirect(BuildingData building)
    {
        isMoving = true;
        Vector3 startPos = mainCamera.transform.position;
        Vector3 targetPos = building.bestViewPosition != Vector3.zero ? building.bestViewPosition : building.cameraViewPosition;
        
        // Calculate target rotation to look at building
        Vector3 buildingPos = building.position;
        Vector3 directionToBuilding = (buildingPos - targetPos).normalized;
        Quaternion targetRotation;
        
        // Use best view rotation if specified, otherwise calculate from direction
        if (building.bestViewRotation != Vector3.zero)
        {
            targetRotation = Quaternion.Euler(building.bestViewRotation);
        }
        else
        {
            targetRotation = Quaternion.LookRotation(directionToBuilding);
        }
        
        float duration = Vector3.Distance(startPos, targetPos) / horizontalSpeed;
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float curveValue = horizontalCurve.Evaluate(t);
            
            mainCamera.transform.position = Vector3.Lerp(startPos, targetPos, curveValue);
            
            // Smoothly rotate
            Quaternion currentRot = mainCamera.transform.rotation;
            mainCamera.transform.rotation = Quaternion.Slerp(currentRot, targetRotation, Time.deltaTime * rotationSpeed);
            
            yield return null;
        }
        
        mainCamera.transform.position = targetPos;
        mainCamera.transform.rotation = targetRotation;
        
        isMoving = false;
        Debug.Log($"[CameraMovementController] Arrived at {building.name} (direct path)");
    }
    
    public bool IsMoving()
    {
        return isMoving;
    }
    
    public void StopMovement()
    {
        if (currentMovementCoroutine != null)
        {
            StopCoroutine(currentMovementCoroutine);
            currentMovementCoroutine = null;
        }
        isMoving = false;
    }
}

