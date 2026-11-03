using UnityEngine;

public class Boid : Agent
{
    [Header("Movimiento")]
    [SerializeField] private float _maxSpeed = 4f;
    [SerializeField] private float _maxSteering = 10f;
    [SerializeField] private float _separationRadius = 2f;
    [Header("Flocking (Inspector)")]
    [SerializeField] private float _separationWeight = 9f;
    [SerializeField] private float _alignmentWeight = 15f;
    [SerializeField] private float _cohesionWeight = 5f;
    [Header("Percepcion")]
    [SerializeField] private BoidPerception _perception;
    [Header("Vida y trampas")]
    [SerializeField] private float _maxLife = 100f;
    [SerializeField] private float _respawnDelay = 3f;
    [SerializeField] private float _trapDamage = 10f;
    [SerializeField] private float _trapDamageInterval = 1f;

    private float _life;
    private float _respawnAt;
    private bool _respawning;
    private BoidStateMachine _stateMachine;
    private Trap _targetTrap;
    private Renderer[] _renderers;
    private Collider[] _colliders;
    private Renderer _body;
    private Color _originalColor;
    private AgentStatusLabel _label;
    public float MaxSpeed => _maxSpeed;
    public float SeparationRadius => _separationRadius;
    public float TrapDamage => _trapDamage;
    public float TrapDamageInterval => _trapDamageInterval;
    public BoidPerception Perception => _perception;
    public Trap TargetTrap => _targetTrap;
    public bool IsAlive => _life > 0f && !_respawning;
    public bool IsDead => _life <= 0f && !_respawning;
    public bool IsRespawning => _respawning;

    private void Awake()
    {
        if (_perception == null) _perception = GetComponentInChildren<BoidPerception>();
        _renderers = GetComponentsInChildren<Renderer>(true);
        _colliders = GetComponentsInChildren<Collider>(true);
        _body = GetComponentInChildren<MeshRenderer>();
        if (_body != null) _originalColor = _body.material.color;
        _life = _maxLife;
        _velocity = Random.insideUnitSphere.normalized * _maxSpeed;
        _velocity.y = 0f;
        _label = gameObject.AddComponent<AgentStatusLabel>();
        _label.Initialize(transform, 2f, Color.white);
        _stateMachine = new BoidStateMachine(this);
    }
    private void Start()
    {
        if (_perception == null || Bounds.Instance == null)
        {
            Debug.LogError("Boid requires a child BoidPerception sensor and Bounds in the scene.", this);
            enabled = false;
        }
    }
    private void Update()
    {
        if (_respawning)
        {
            if (Time.time >= _respawnAt) Respawn();
            return;
        }
        _stateMachine.Update();
        _velocity.y = 0f;
        transform.position += _velocity * Time.deltaTime;
        transform.position = Bounds.Instance.OutOfBounds(transform.position);
        if (_velocity.sqrMagnitude > 0.001f) transform.forward = _velocity.normalized;
    }
    public void SetSteering(Vector3 steering)
    {
        steering = Vector3.ClampMagnitude(steering, _maxSteering * Time.deltaTime);
        _velocity = Vector3.ClampMagnitude(_velocity + steering, _maxSpeed);
    }
    public void SetVelocity(Vector3 velocity) { _velocity = velocity; }
    private Vector3 Separation()
    {
        Vector3 desired = Vector3.zero;
        int count = 0;
        foreach (Boid other in _perception.Neighbors)
        {
            if (other == null || !other.IsAlive) continue;
            Vector3 delta = transform.position - other.transform.position;
            float dist = delta.magnitude;
            if (dist > 0.001f && dist < _separationRadius)
            {
                desired += delta / dist; count++;
            }
        }
        return count == 0 ? Vector3.zero : desired.normalized * _maxSpeed - _velocity;
    }
    private Vector3 Alignment()
    {
        Vector3 velocity = Vector3.zero;
        int count = 0;
        foreach (Boid other in _perception.Neighbors)
        {
            if (other == null || !other.IsAlive || !_perception.InRange(other.transform.position)) continue;
            velocity += other.Velocity; count++;
        }
        return count == 0 ? Vector3.zero : velocity.normalized * _maxSpeed - _velocity;
    }
    private Vector3 Cohesion()
    {
        Vector3 center = Vector3.zero;
        int count = 0;
        foreach (Boid other in _perception.Neighbors)
        {
            if (other == null || !other.IsAlive || !_perception.InRange(other.transform.position)) continue;
            center += other.transform.position; count++;
        }
        return count == 0 ? Vector3.zero : ((center / count) - transform.position).normalized * _maxSpeed - _velocity;
    }
    public Vector3 CalculateFlocking()
    {
        return Separation() * _separationWeight + Alignment() * _alignmentWeight + Cohesion() * _cohesionWeight;
    }
    public void TakeDamage(float damage)
    {
        if (!IsAlive) return;
        _life = Mathf.Max(0f, _life - damage);
        if (_life == 0f)
        {
            _velocity = Vector3.zero;
            if (_body != null) _body.material.color = new Color(0.25f, 0.05f, 0.05f);
            _label.SetText("MUERTO");
            _stateMachine.ChangeState(BoidStateType.Hunted);
            Debug.Log($"{name}: boid eliminado", this);
        }
        else
        {
            if (_body != null) _body.material.color = Color.red;
            CancelInvoke(nameof(ResetColor));
            Invoke(nameof(ResetColor), 0.18f);
        }
    }
    private void ResetColor() { if (IsAlive && _body != null) _body.material.color = _originalColor; }
    public void SetTargetTrap(Trap trap) { _targetTrap = trap; }
    public void ClearTargetTrap() { _targetTrap = null; }
    public void SetGathering(bool gathering)
    {
        if (IsDead) _label.SetText(gathering ? "RECOLECTANDO" : "MUERTO");
    }
    public void StartRespawn()
    {
        if (_respawning) return;
        _respawning = true;
        _velocity = Vector3.zero;
        _respawnAt = Time.time + _respawnDelay;
        foreach (Renderer r in _renderers) if (r != null) r.enabled = false;
        foreach (Collider c in _colliders) if (c != null) c.enabled = false;
        _label.SetText("");
        Debug.Log($"{name}: recolectado; respawn en {_respawnDelay:F1}s", this);
    }
    private void Respawn()
    {
        _life = _maxLife;
        _respawning = false;
        _targetTrap = null;
        transform.position = Bounds.Instance.RandomPosition();
        Vector3 dir = Random.insideUnitSphere; dir.y = 0f;
        _velocity = dir.normalized * _maxSpeed;
        if (_body != null) _body.material.color = _originalColor;
        foreach (Renderer r in _renderers) if (r != null) r.enabled = true;
        foreach (Collider c in _colliders) if (c != null) c.enabled = true;
        _stateMachine.ChangeState(BoidStateType.Flocking);
        Debug.Log($"{name}: reaparecio en el coto", this);
    }
    public void SetStatus(string text) { _label.SetText(text); }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, _separationRadius);
    }
}
