using UnityEngine;

public class WalkController : MonoBehaviour
{
    public JoystickManager joystickManager;
    public float moveSpeed = 5f;
    public float rotateSpeed = 90f;
    // Jump feature removed for simplicity

    void Update()
    {
        if (joystickManager == null) 
        {
            Debug.LogWarning("[WalkController] JoystickManager is null!");
            return;
        }

        Vector2 left = joystickManager.GetLeftInput();
        Vector2 right = joystickManager.GetRightInput();

        // Check if we're in Walk mode - if not, don't process input
        if (joystickManager.current != UIControlMode.ModeWalk)
        {
            return; // Exit early if not in Walk mode
        }

        // Debug logging - show ALL input, not just when magnitude > 0.1f
        Debug.Log($"[WalkController] Input - Left: {left}, Right: {right}, Object: {gameObject.name}");

        // Debug logging
        if (left.magnitude > 0.1f || right.magnitude > 0.1f)
        {
            Debug.Log($"[WalkController] Significant input detected - Left: {left}, Right: {right}");
            Debug.Log($"[WalkController] Moving object: {gameObject.name} at position: {transform.position}");
        }

        // Jump feature removed - no jumping in walk mode
        
        // Rotate camera/player Y axis (left joystick horizontal)
        transform.Rotate(0, left.x * rotateSpeed * Time.deltaTime, 0);

        // Move XZ
        Vector3 forward = transform.forward * right.y * moveSpeed * Time.deltaTime;
        Vector3 strafe = transform.right * right.x * moveSpeed * Time.deltaTime;
        transform.position += forward + strafe;
        
        // Constrain rotations: Z always 0, X stays at 0 degrees
        Vector3 currentRotation = transform.eulerAngles;
        transform.rotation = Quaternion.Euler(0f, currentRotation.y, 0f);
    }
}
