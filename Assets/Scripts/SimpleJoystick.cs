// SimpleJoystick.cs
using UnityEngine;
using UnityEngine.EventSystems;

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
