
using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonCursor : MonoBehaviour,
    IPointerEnterHandler, IPointerExitHandler
{
    public Texture2D handCursor;
    [Range(0.1f, 2f)]
    public float cursorScale = 0.3f;

    private Texture2D scaledCursor;

    void Start()
    {
        if (handCursor != null)
        {
            int width = Mathf.Max(1,
                Mathf.RoundToInt(handCursor.width * cursorScale));
            int height = Mathf.Max(1,
                Mathf.RoundToInt(handCursor.height * cursorScale));

            scaledCursor = new Texture2D(width, height);
            scaledCursor.filterMode = FilterMode.Point;

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    scaledCursor.SetPixel(x, y,
                        handCursor.GetPixelBilinear(
                            (float)x / width, (float)y / height));
                }
            }

            scaledCursor.Apply();
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (scaledCursor != null)
            Cursor.SetCursor(scaledCursor, Vector2.zero, CursorMode.Auto);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
    }

    void OnDestroy()
    {
        if (scaledCursor != null)
            Destroy(scaledCursor);
    }
}
