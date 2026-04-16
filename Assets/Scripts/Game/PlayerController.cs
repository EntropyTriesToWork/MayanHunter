using System.Collections;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Rigidbody2D projectilePrefab;
    [SerializeField] private Transform launchAnchor;
    [SerializeField] private LineRenderer trajectoryLine;

    [Header("Launch Settings")]
    [SerializeField] private float maxDragDistance = 2.5f;
    [SerializeField] private float launchForce = 8f;
    [SerializeField] private int trajectoryPointCount = 30;
    [SerializeField] private float trajectoryTimeStep = 0.05f;

    [Header("Drag Detection")]
    [Tooltip("Screen-space radius (pixels) within which a press registers on the anchor.")]
    [SerializeField] private float dragActivationRadius = 60f;

    [Tooltip("Minimum drag power (0-1) required to launch. Below this the throw is cancelled.")]
    [SerializeField][Range(0f, 1f)] private float minLaunchPower = 0.15f;

    private Rigidbody2D _currentProjectile;
    private bool _isDragging;
    private bool _canThrow = true;
    private Vector2 _dragWorldPos;

    private float _currentPower;
    private Vector2 _currentLaunchDir;

    public bool IsDragging => _isDragging;

    public System.Action OnProjectileLaunched;

    public bool CanThrow
    {
        get => _canThrow;
        set => _canThrow = value;
    }

    private void Start()
    {
        SpawnProjectile();
        if (trajectoryLine != null)
            trajectoryLine.enabled = false;
    }

    private void Update()
    {
        if (!_canThrow) return;

        HandleInput();

        if (_isDragging)
        {
            UpdateAim();
            DrawTrajectory();
        }
    }
    private void HandleInput()
    {
        if (GetInputDown(out Vector2 pressScreen))
        {
            Vector2 anchorScreen = Camera.main.WorldToScreenPoint(launchAnchor.position);
            if (Vector2.Distance(pressScreen, anchorScreen) <= dragActivationRadius)
            {
                _isDragging = true;
            }
        }

        if (_isDragging && GetInputHeld(out Vector2 holdScreen))
        {
            _dragWorldPos = Camera.main.ScreenToWorldPoint(holdScreen);
        }
        if (_isDragging && GetInputUp())
        {
            _isDragging = false;
            Launch();
        }
    }

    private bool GetInputDown(out Vector2 screenPos)
    {
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        { screenPos = Input.GetTouch(0).position; return true; }
        if (Input.GetMouseButtonDown(0))
        { screenPos = Input.mousePosition; return true; }
        screenPos = default; return false;
    }

    private bool GetInputHeld(out Vector2 screenPos)
    {
        if (Input.touchCount > 0)
        { screenPos = Input.GetTouch(0).position; return true; }
        if (Input.GetMouseButton(0))
        { screenPos = Input.mousePosition; return true; }
        screenPos = default; return false;
    }

    private bool GetInputUp()
    {
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Ended) return true;
        if (Input.GetMouseButtonUp(0)) return true;
        return false;
    }
    private void SpawnProjectile()
    {
        if (_currentProjectile != null)
            Destroy(_currentProjectile.gameObject);

        _currentProjectile = Instantiate(projectilePrefab, launchAnchor.position, Quaternion.identity);
        _currentProjectile.bodyType = RigidbodyType2D.Kinematic;
        _currentProjectile.linearVelocity = Vector2.zero;

        _currentPower = 0f;
        _currentLaunchDir = Vector2.up;
    }

    // Calculates aim direction and power from drag position; rotates projectile to face launch dir.
    private void UpdateAim()
    {
        if (_currentProjectile == null) return;

        Vector2 anchor = launchAnchor.position;
        Vector2 offset = _dragWorldPos - anchor;

        // Clamp to max distance for power calculation but don't move the projectile
        float clampedMag = Mathf.Min(offset.magnitude, maxDragDistance);
        _currentPower = Mathf.Clamp01(clampedMag / maxDragDistance);
        _currentLaunchDir = offset.magnitude > 0.001f ? (-offset).normalized : Vector2.up;

        // Rotate projectile to face the launch direction
        float angle = Mathf.Atan2(_currentLaunchDir.y, _currentLaunchDir.x) * Mathf.Rad2Deg;
        _currentProjectile.transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }
    private void Launch()
    {
        if (_currentProjectile == null) return;

        // Cancel if drag power is below the minimum threshold
        if (_currentPower < minLaunchPower)
        {
            CancelThrow();
            return;
        }

        // Hide trajectory
        if (trajectoryLine != null)
            trajectoryLine.enabled = false;

        _currentProjectile.bodyType = RigidbodyType2D.Dynamic;
        _currentProjectile.AddForce(_currentLaunchDir * (_currentPower * launchForce), ForceMode2D.Impulse);

        _canThrow = false;
        OnProjectileLaunched?.Invoke();

        StartCoroutine(WatchProjectile(_currentProjectile));
    }

    private void CancelThrow()
    {
        // Reset rotation and hide the trajectory line to signal cancellation
        if (_currentProjectile != null)
            _currentProjectile.transform.rotation = Quaternion.identity;

        if (trajectoryLine != null)
            trajectoryLine.enabled = false;

        _isDragging = false;
    }
    public System.Action OnProjectileSettled;

    [Tooltip("Speed below which the projectile is considered 'settled'.")]
    [SerializeField] private float settleSpeedThreshold = 0.2f;

    [Tooltip("Seconds the projectile must be below settle speed before 'settled' fires.")]
    [SerializeField] private float settleGracePeriod = 0.5f;

    [Tooltip("Maximum seconds to wait for settle before forcing it.")]
    [SerializeField] private float settleTimeout = 5f;

    private IEnumerator WatchProjectile(Rigidbody2D proj)
    {
        float slowTime = 0f;
        float elapsed = 0f;

        while (proj != null && elapsed < settleTimeout)
        {
            elapsed += Time.deltaTime;

            if (proj.linearVelocity.magnitude < settleSpeedThreshold)
                slowTime += Time.deltaTime;
            else
                slowTime = 0f;

            if (slowTime >= settleGracePeriod) break;

            yield return null;
        }

        OnProjectileSettled?.Invoke();
    }
    public void PrepareNextThrow()
    {
        SpawnProjectile();
        _canThrow = true;

        if (trajectoryLine != null)
            trajectoryLine.enabled = false;
    }
    private void DrawTrajectory()
    {
        if (trajectoryLine == null || _currentProjectile == null) return;

        // Hide line if below minimum power (cancel zone feedback)
        if (_currentPower < minLaunchPower)
        {
            trajectoryLine.enabled = false;
            return;
        }

        Vector2 velocity = _currentLaunchDir * (_currentPower * launchForce) / _currentProjectile.mass;

        trajectoryLine.enabled = true;
        trajectoryLine.positionCount = trajectoryPointCount;

        for (int i = 0; i < trajectoryPointCount; i++)
        {
            float t = i * trajectoryTimeStep;
            Vector2 pos = (Vector2)launchAnchor.position
                          + velocity * t
                          + 0.5f * Physics2D.gravity * (t * t);
            trajectoryLine.SetPosition(i, pos);
        }
    }
}