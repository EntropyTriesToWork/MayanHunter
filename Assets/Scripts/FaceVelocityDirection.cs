using UnityEngine;

public class FaceVelocityDirection : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private bool rotateOnStart = true;
    [SerializeField] private bool stopRotatingOnCollision = true;

    private Rigidbody2D rb;
    private bool shouldRotate = true;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
            Debug.LogError("FaceVelocityDirection requires a Rigidbody2D component.");
    }

    private void Start()
    {
        shouldRotate = rotateOnStart;
    }

    private void Update()
    {
        if (!shouldRotate) return;
        if (rb == null) return;

        Vector2 velocity = rb.linearVelocity;
        if (velocity.sqrMagnitude < 0.01f) return;

        float angle = Mathf.Atan2(velocity.y, velocity.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (stopRotatingOnCollision)
            shouldRotate = false;
    }
    public void StartRotating()
    {
        shouldRotate = true;
    }

    public void StopRotating()
    {
        shouldRotate = false;
    }

    public void ResetAndRotate()
    {
        shouldRotate = true;
    }
}