// SimpleJoystick.cs
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SimpleJoystick : MonoBehaviour, IDragHandler, IPointerUpHandler, IPointerDownHandler
{
    public RectTransform handle;
    public float handleRange = 80f; // pixels
    Vector2 input = Vector2.zero;

    void Reset()
    {
        // try auto-assign handle if child exists
        if (transform.childCount > 0)
            handle = transform.GetChild(0) as RectTransform;
    }
    
    void Start()
    {
        // Validation checks
        if (handle == null)
        {
            Debug.LogWarning($"[SimpleJoystick] Handle not assigned on {gameObject.name}! Joystick will not move visually.");
        }
        
        // Check if this GameObject has a Graphic component (needed for raycasting)
        Graphic graphic = GetComponent<Graphic>();
        if (graphic == null)
        {
            // Try to add an Image component if none exists
            Image img = GetComponent<Image>();
            if (img == null)
            {
                Debug.LogWarning($"[SimpleJoystick] {gameObject.name} has no Graphic component (Image, Text, etc.). Adding Image component for touch detection.");
                img = gameObject.AddComponent<Image>();
                img.color = new Color(1, 1, 1, 0); // Make it transparent so it's invisible but still receives events
            }
        }
        
        // Check for EventSystem
        if (EventSystem.current == null)
        {
            Debug.LogError("[SimpleJoystick] No EventSystem found in scene! Joystick will not work. Add UI → Event System to the scene.");
        }
    }

    public void OnPointerDown(PointerEventData eventData) 
    {
        Debug.Log($"[SimpleJoystick] OnPointerDown - {gameObject.name}");
        OnDrag(eventData);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        Debug.Log($"[SimpleJoystick] OnPointerUp - {gameObject.name}");
        input = Vector2.zero;
        if (handle) handle.anchoredPosition = Vector2.zero;
    }

    public void OnDrag(PointerEventData eventData)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(transform as RectTransform, eventData.position, eventData.pressEventCamera, out Vector2 pos);
        pos = Vector2.ClampMagnitude(pos, handleRange);
        if (handle) handle.anchoredPosition = pos;
        input = pos / handleRange;
        
        // Debug logging
        if (input.magnitude > 0.1f)
        {
            Debug.Log($"[SimpleJoystick] Drag input: {input} - {gameObject.name}");
        }
    }

    // values in range [-1,1]
    public Vector2 GetInput() => input;
}
