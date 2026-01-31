// JoystickManager.cs
using UnityEngine;

public enum UIControlMode { Mode2D, ModeDrone, ModeWalk }

public class JoystickManager : MonoBehaviour
{
    [Header("Assign Joysticks")]
    public SimpleJoystick leftJoystick;
    public SimpleJoystick rightJoystick;

    [Header("Keyboard Controls")]
    public bool enableKeyboardControls = true;
    public bool keyboardOverridesJoystick = true;

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
        Vector2 raw = Vector2.zero;
        if (leftJoystick == null)
        {
            Debug.LogWarning("[JoystickManager] Left joystick is null!");
        }
        else
        {
            raw = leftJoystick.GetInput();
        }

        Vector2 keyboard = Vector2.zero;
        if (enableKeyboardControls)
        {
            float x = 0f;
            float y = 0f;

            if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightBracket)) x += 1f;
            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftBracket)) x -= 1f;
            if (Input.GetKey(KeyCode.W)) y += 1f;
            if (Input.GetKey(KeyCode.S)) y -= 1f;

            keyboard = new Vector2(Mathf.Clamp(x, -1f, 1f), Mathf.Clamp(y, -1f, 1f));
        }

        Vector2 combined = raw;
        if (enableKeyboardControls)
        {
            if (keyboardOverridesJoystick)
            {
                combined = keyboard.sqrMagnitude > 0f ? keyboard : raw;
            }
            else
            {
                combined = Vector2.ClampMagnitude(raw + keyboard, 1f);
            }
        }
        
        // Debug logging
        if (combined.magnitude > 0.1f)
        {
            Debug.Log($"[JoystickManager] Left joystick input: {combined}, Mode: {current}");
        }
        
        if (current == UIControlMode.ModeWalk && leftJoystickWalkRestrict)
        {
            // Only use X for strafe/turn. If Y > threshold, treat as jump flag via returned vector.y
            float jump = combined.y > 0.65f ? 1f : 0f;
            return new Vector2(combined.x, jump);
        }
        return combined; // Drone/full
    }

    // Right joystick for camera/look: return raw vector2 for both modes
    public Vector2 GetRightInput()
    {
        Vector2 raw = Vector2.zero;
        if (rightJoystick == null)
        {
            Debug.LogWarning("[JoystickManager] Right joystick is null!");
        }
        else
        {
            raw = rightJoystick.GetInput();
        }

        Vector2 keyboard = Vector2.zero;
        if (enableKeyboardControls)
        {
            float x = 0f;
            float y = 0f;

            if (Input.GetKey(KeyCode.RightArrow)) x += 1f;
            if (Input.GetKey(KeyCode.LeftArrow)) x -= 1f;
            if (Input.GetKey(KeyCode.UpArrow)) y += 1f;
            if (Input.GetKey(KeyCode.DownArrow)) y -= 1f;

            keyboard = new Vector2(Mathf.Clamp(x, -1f, 1f), Mathf.Clamp(y, -1f, 1f));
        }

        Vector2 combined = raw;
        if (enableKeyboardControls)
        {
            if (keyboardOverridesJoystick)
            {
                combined = keyboard.sqrMagnitude > 0f ? keyboard : raw;
            }
            else
            {
                combined = Vector2.ClampMagnitude(raw + keyboard, 1f);
            }
        }
        
        // Debug logging
        if (combined.magnitude > 0.1f)
        {
            Debug.Log($"[JoystickManager] Right joystick input: {combined}, Mode: {current}");
        }
        
        return combined;
    }
}
