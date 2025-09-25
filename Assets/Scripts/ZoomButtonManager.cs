using UnityEngine;

public class ZoomButtonManager : MonoBehaviour
{
    [Tooltip("Optional container holding the zoom UI")]
    public GameObject container;

    public void SetVisible(bool visible)
    {
        var target = container != null ? container : gameObject;
        if (target != null)
        {
            target.SetActive(visible);
        }
    }
}



