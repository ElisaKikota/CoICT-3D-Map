// JoystickManager.cs
using UnityEngine;

public enum UIControlMode { Mode2D, ModeDrone, ModeWalk }

public class JoystickManager : MonoBehaviour
{
    [Header("Assign Joysticks")]
    public SimpleJoystick leftJoystick;
    public SimpleJoystick rightJoystick;

    [Header("Left joystick walk constraint")]
    public bool leftJoystickWalkRestrict = false; // left/right + jump, no forward/back

    public UIControlMode current = UIControlMode.Mode2D;

    void Awake()
    {
        // ensure container starts disabled if in 2D
        gameObject.SetActive(false);
    }

    // Called by CameraModeController when mode changes
    public void SetMode(UIControlMode mode)
    {
        current = mode;
        // Show joysticks only for Drone and Walk
        bool show = (mode == UIControlMode.ModeDrone || mode == UIControlMode.ModeWalk);
        gameObject.SetActive(show);
    }

    // Returns processed left input depending on mode:
    // - Drone: full x/y (x = left/right, y = forward/back)
    // - Walk: x = left/right, y = jump (interpret > 0.6 as jump input)
    public Vector2 GetLeftInput()
    {
        if (leftJoystick == null) 
        {
            Debug.LogWarning("[JoystickManager] Left joystick is null!");
            return Vector2.zero;
        }
        Vector2 raw = leftJoystick.GetInput();
        
        // Debug logging
        if (raw.magnitude > 0.1f)
        {
            Debug.Log($"[JoystickManager] Left joystick input: {raw}, Mode: {current}");
        }
        
        if (current == UIControlMode.ModeWalk && leftJoystickWalkRestrict)
        {
            // Only use X for strafe/turn. If Y > threshold, treat as jump flag via returned vector.y
            float jump = raw.y > 0.65f ? 1f : 0f;
            return new Vector2(raw.x, jump);
        }
        return raw; // Drone/full
    }

    // Right joystick for camera/look: return raw vector2 for both modes
    public Vector2 GetRightInput()
    {
        if (rightJoystick == null) 
        {
            Debug.LogWarning("[JoystickManager] Right joystick is null!");
            return Vector2.zero;
        }
        Vector2 raw = rightJoystick.GetInput();
        
        // Debug logging
        if (raw.magnitude > 0.1f)
        {
            Debug.Log($"[JoystickManager] Right joystick input: {raw}, Mode: {current}");
        }
        
        return raw;
    }
}
