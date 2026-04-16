using UnityEngine;

public class CameraController : MonoBehaviour
{

    [Header("Settings")]
    public float scrollMin = -2f;
    public float scrollMax = 20f;
    [SerializeField] private float smoothSpeed = 12f;
    [SerializeField] private float dragSensitivity = 1f;

    [Header("State")]
    [Tooltip("When true the player is dragging to throw — camera scrolling is disabled.")]
    public bool IsLockedForThrow = false;

    private float _targetX;
    private bool _isDragging;
    private Vector3 _lastMouseWorld;
    [SerializeField] private PlayerController _pc;
    private void Start()
    {
        _targetX = transform.position.x;
    }

    private void Update()
    {
        if (IsLockedForThrow || _pc.IsDragging) return;

        HandleMouseInput();
        HandleTouchInput();
    }

    private void LateUpdate()
    {
        float clampedX = Mathf.Clamp(_targetX, scrollMin, scrollMax);
        float smoothX = Mathf.Lerp(transform.position.x, clampedX, Time.deltaTime * smoothSpeed);

        transform.position = new Vector3(smoothX, transform.position.y, transform.position.z);
    }
    private void HandleMouseInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            _isDragging = true;
            _lastMouseWorld = ScreenToWorld(Input.mousePosition);
        }

        if (Input.GetMouseButton(0) && _isDragging)
        {
            Vector3 current = ScreenToWorld(Input.mousePosition);
            float delta = _lastMouseWorld.x - current.x;
            _targetX += delta * dragSensitivity;
            _lastMouseWorld = current;
        }

        if (Input.GetMouseButtonUp(0))
            _isDragging = false;
    }

    private void HandleTouchInput()
    {
        if (Input.touchCount != 1) return;

        Touch touch = Input.GetTouch(0);

        if (touch.phase == TouchPhase.Began)
        {
            _isDragging = true;
            _lastMouseWorld = ScreenToWorld(touch.position);
        }
        else if (touch.phase == TouchPhase.Moved && _isDragging)
        {
            Vector3 current = ScreenToWorld(touch.position);
            float delta = _lastMouseWorld.x - current.x;
            _targetX += delta * dragSensitivity;
            _lastMouseWorld = current;
        }
        else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
        {
            _isDragging = false;
        }
    }

    private Vector3 ScreenToWorld(Vector3 screenPos)
    {
        screenPos.z = -transform.position.z;
        return Camera.main.ScreenToWorldPoint(screenPos);
    }
    public void SnapToX(float worldX)
    {
        _targetX = Mathf.Clamp(worldX, scrollMin, scrollMax);
        Vector3 pos = transform.position;
        pos.x = _targetX;
        transform.position = pos;
    }
    public void PanToX(float worldX)
    {
        _targetX = Mathf.Clamp(worldX, scrollMin, scrollMax);
    }

#if UNITY_EDITOR
    // Draw scroll limits as a gizmo line in Scene view
    private void OnDrawGizmosSelected()
    {
        float y = transform.position.y;
        float z = transform.position.z;

        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(new Vector3(scrollMin, y - 5f, z), new Vector3(scrollMin, y + 5f, z));
        Gizmos.DrawLine(new Vector3(scrollMax, y - 5f, z), new Vector3(scrollMax, y + 5f, z));

        Gizmos.color = new Color(0f, 1f, 1f, 0.15f);
        Gizmos.DrawCube(
            new Vector3((scrollMin + scrollMax) * 0.5f, y, z),
            new Vector3(scrollMax - scrollMin, 10f, 0.1f)
        );
    }
#endif
}