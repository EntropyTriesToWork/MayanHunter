using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class MouseFollowingSprite : MonoBehaviour
{
    [Header("Follow Mode")]
    [SerializeField] private bool useLerp = true;
    [SerializeField] private float smoothSpeed = 5f;

    [Header("Offset")]
    [SerializeField] private Vector3 offset = Vector3.zero;

    private Camera mainCamera;
    private float targetZ = 0f;

    private void Awake()
    {
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("MouseFollowingSprite requires a Camera component.");
        }
    }

    private void LateUpdate()
    {
        if (mainCamera == null) return;

        Vector3 mouseScreenPos = Input.mousePosition;
        Vector3 desiredPosition = mainCamera.ScreenToWorldPoint(mouseScreenPos);
        desiredPosition.z = targetZ;
        desiredPosition += offset;

        if (useLerp)
        {
            Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
            transform.position = smoothedPosition;
        }
        else
        {
            transform.position = desiredPosition;
        }
    }

    public void SetUseLerp(bool lerp)
    {
        useLerp = lerp;
    }

    public void SetSmoothSpeed(float speed)
    {
        smoothSpeed = speed;
    }
}