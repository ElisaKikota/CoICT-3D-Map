using UnityEngine;

public class DroneController : MonoBehaviour
{
    public JoystickManager joystickManager;
    public float moveSpeed = 5f;
    public float rotateSpeed = 90f;
    public float heightSpeed = 3f;

    void Update()
    {
        if (joystickManager == null) 
        {
            Debug.LogWarning("[DroneController] JoystickManager is null!");
            return;
        }

        Vector2 left = joystickManager.GetLeftInput();
        Vector2 right = joystickManager.GetRightInput();

        // Debug logging
        if (left.magnitude > 0.1f || right.magnitude > 0.1f)
        {
            Debug.Log($"[DroneController] Input - Left: {left}, Right: {right}");
            Debug.Log($"[DroneController] Moving object: {gameObject.name} at position: {transform.position}");
        }

        // Rotate around Y (left joystick horizontal)
        transform.Rotate(0f, left.x * rotateSpeed * Time.deltaTime, 0f);

        // Change height (invert Y for correct direction) with minimum height of 10
        transform.position += Vector3.up * left.y * heightSpeed * Time.deltaTime;
        
        // Enforce minimum height of 10
        if (transform.position.y < 10f)
        {
            Vector3 pos = transform.position;
            pos.y = 10f;
            transform.position = pos;
        }

        // Move on XZ plane only (project movement to avoid Y changes)
        Vector3 forward = new Vector3(transform.forward.x, 0, transform.forward.z).normalized * right.y * moveSpeed * Time.deltaTime;
        Vector3 strafe = new Vector3(transform.right.x, 0, transform.right.z).normalized * right.x * moveSpeed * Time.deltaTime;
        transform.position += forward + strafe;
        
        // Constrain rotations: Z always 0, X stays at 30 degrees
        Vector3 currentRotation = transform.eulerAngles;
        transform.rotation = Quaternion.Euler(30f, currentRotation.y, 0f);
    }
}
