using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class LevelObject : MonoBehaviour
{

    [Header("Stats")]
    [Tooltip("Starting health of this object.")]
    [SerializeField] private int maxHealth = 10;

    [Tooltip("Score awarded to the player when this object is destroyed.")]
    [SerializeField] private int scoreValue = 100;

    [Tooltip("Multiplier applied to the raw damage calculation.")]
    [SerializeField] private float damageMultiplier = 1f;

    [Tooltip("Tag that identifies enemy objects (used by LevelController for victory check).")]
    [SerializeField] private bool isTarget = false;

    [Header("Damage Thresholds")]
    [Tooltip("Minimum collision impulse magnitude before damage is applied. Filters out micro-bumps.")]
    [SerializeField] private float minImpactVelocity = 0.5f;

    private int _currentHealth;
    private Rigidbody2D _rb;
    private bool _isDead;

    private float _lastRollDamageTime = -999f;
    private const float RollDamageCooldown = 1f;
    private const float RollVelocityThreshold = 0.3f;
    private const float StillVelocityThreshold = 0.05f; 
    public int CurrentHealth  => _currentHealth;
    public int MaxHealth      => maxHealth;
    public int ScoreValue     => scoreValue;
    public bool IsTarget      => isTarget;
    public bool IsDead        => _isDead;

    private bool _addedToGameManager = false;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _currentHealth = maxHealth;
    }
    private void Start()
    {
        if (isTarget && !_addedToGameManager) { GameManager.Instance.AddTarget(this); _addedToGameManager = true; }
    }

    private void Update()
    {
        if (_isDead) return;
        HandleRollingDamage();
    }
    private void OnCollisionEnter2D(Collision2D col)
    {
        if (_isDead) return;

        float impactSpeed = col.relativeVelocity.magnitude;
        if (impactSpeed < minImpactVelocity) return;

        float mass = _rb.mass;
        int damage = Mathf.Max(1, Mathf.FloorToInt(impactSpeed * mass * damageMultiplier));

        TakeDamage(damage);
    }

    private void HandleRollingDamage()
    {
        float speed = _rb.linearVelocity.magnitude;

        bool isRolling = speed >= RollVelocityThreshold && speed < minImpactVelocity;
        if (!isRolling) return;

        if (Time.time - _lastRollDamageTime >= RollDamageCooldown)
        {
            _lastRollDamageTime = Time.time;
            TakeDamage(1);
        }
    }
    public void TakeDamage(int amount)
    {
        if (_isDead) return;

        amount = Mathf.Max(1, amount);
        _currentHealth -= amount;
        if (IsTarget) { Debug.Log("Target took " + amount + " damage."); }
        GameEvents.LevelObjectDamaged(this, amount);

        if (_currentHealth <= 0)
            Die();
    }

    private void Die()
    {
        if (_isDead) return;
        _isDead = true;

        GameEvents.LevelObjectDestroyed(this);
        StartCoroutine(DisableNextFrame());
    }

    private IEnumerator DisableNextFrame()
    {
        yield return null;
        gameObject.SetActive(false);
    }
}
