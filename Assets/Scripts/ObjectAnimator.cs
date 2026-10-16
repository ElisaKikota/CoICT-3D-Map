
using UnityEngine;

public class ObjectAnimator : MonoBehaviour
{
    [Header("Scale / Blink")]
    [SerializeField] private Vector3 minScale = new Vector3(0.8f, 0.8f, 0.8f);
    [SerializeField] private Vector3 maxScale = new Vector3(1.2f, 1.2f, 1.2f);
    [SerializeField, Min(0f)] private float scaleSpeed = 2f;

    [Header("World Y Rotation")]
    [SerializeField] private float rotationSpeed = 60f;

    [Header("Local Up and Down Movement")]
    [SerializeField] private float moveHeight = 0.5f;
    [SerializeField, Min(0f)] private float moveSpeed = 2f;

    private Vector3 initialLocalPosition;
    private float scaleTime;
    private float moveTime;

    private void Start()
    {
        initialLocalPosition = transform.localPosition;
    }

    private void Update()
    {
        float dt = Time.deltaTime;

        // 1. Smooth, continuous scaling (blink effect)
        scaleTime += dt * scaleSpeed;
        float scaleT = (Mathf.Sin(scaleTime) + 1f) * 0.5f;

        transform.localScale = Vector3.Lerp(
            minScale,
            maxScale,
            scaleT
        );

        // 2. Continuous rotation around the WORLD Y axis
        transform.Rotate(
            Vector3.up,
            rotationSpeed * dt,
            Space.World
        );

        // 3. Smooth up-and-down movement in LOCAL space
        moveTime += dt * moveSpeed;

        Vector3 localPos = initialLocalPosition;
        localPos.y += Mathf.Sin(moveTime) * moveHeight;

        transform.localPosition = localPos;
    }
}
