using UnityEngine;

[RequireComponent(typeof(GraphNavigator))]
public class Hunter : Agent
{
    [Header("Movimiento y combate")]
    [SerializeField] private float _maxSpeed = 5f;
    [SerializeField] private float _maxSteering = 12f;
    [SerializeField] private float _meleeDamage = 100f;
    [SerializeField] private float _rangeDamage = 50f;
    [SerializeField] private float _TBA = 3f;
    [SerializeField] private float _rangeAttackRadius = 8f;
    [SerializeField] private float _meleeAttackRadius = 2f;
    [Header("Parcial 2")]
    [SerializeField] private int _requiredGathers = 3;
    [SerializeField] private float _respawnTime = 5f;
    [SerializeField] private float _safeZoneWaitTime = 4f;
    [SerializeField] private float _gatherDuration = 2f;
    [SerializeField] private HunterPerception _perception;
    [Header("Objetos de interes")]
    [SerializeField] private Trap _trapPrefab;
    [SerializeField] private float _trapInterval = 5f;
    [SerializeField] private float _trapVariation = 2f;

    private float _tbaTimer;
    private float _trapTimer;
    private int _activeTrapCount;
    private int _gathers;
    private Boid _target;
    private bool _isDead;
    private bool _isHidden;
    private HunterStateMachine _machine;
    private GraphNavigator _navigator;
    private Renderer[] _renderers;
    private Collider[] _colliders;
    private AgentStatusLabel _label;
    private Boid _lastHuntedBoid;

    public float MaxSpeed => _maxSpeed;
    public float RangeAttackRadius => _rangeAttackRadius;
    public float MeleeAttackRadius => _meleeAttackRadius;
    public int RequiredGathers => _requiredGathers;
    public int Gathers => _gathers;
    public float RespawnTime => _respawnTime;
    public float SafeZoneWaitTime => _safeZoneWaitTime;
    public float GatherDuration => _gatherDuration;
    public bool CanAttack => _tbaTimer >= _TBA;
    public bool IsDead => _isDead;
    public bool IsHidden => _isHidden;
    public HunterPerception Perception => _perception;
    public GraphNavigator Navigator => _navigator;
    public Boid Target => _target;
    public Boid LastHuntedBoid => _lastHuntedBoid;
    public HunterStateType CurrentState => _machine.CurrentState;

    private void Awake()
    {
        if (_perception == null) _perception = GetComponentInChildren<HunterPerception>();
        _navigator = GetComponent<GraphNavigator>();
        _renderers = GetComponentsInChildren<Renderer>(true);
        _colliders = GetComponentsInChildren<Collider>(true);
        _label = gameObject.AddComponent<AgentStatusLabel>();
        _label.Initialize(transform, 2.7f, Color.yellow);
        _tbaTimer = _TBA;
        _trapTimer = _trapInterval;
    }
    private void Start()
    {
        if (_perception == null || _navigator == null || WorldZones.Instance == null ||
            !WorldZones.Instance.HasRequiredReferences || NavigationGraph.Instance == null)
        {
            Debug.LogError("Hunter: configure WorldZones, Bounds, NavigationGraph, GraphNavigator and HunterPerception.", this);
            enabled = false;
            return;
        }
        transform.position = WorldZones.Instance.StartPoint.position;
        _machine = new HunterStateMachine(this);
    }
    private void Update()
    {
        if (_machine == null) return;
        _tbaTimer += Time.deltaTime;
        _machine.Update(); // DeadState controls its own clock.
        if (_isHidden) return;
        _velocity.y = 0f;
        Vector3 nextPosition = transform.position + _velocity * Time.deltaTime;
        // IMPORTANT: no Bounds wrap. Keep Hunter outside physical walls instead.
        if (NavigationGraph.Instance.ClearMovement(transform.position, nextPosition))
            transform.position = nextPosition;
        else _velocity = Vector3.zero;
        if (_velocity.sqrMagnitude > 0.001f) transform.forward = _velocity.normalized;
    }
    public void SetSteering(Vector3 steering)
    {
        _velocity = Vector3.ClampMagnitude(_velocity +
            Vector3.ClampMagnitude(steering, _maxSteering * Time.deltaTime), _maxSpeed);
    }
    public void SetVelocity(Vector3 velocity) { _velocity = velocity; }
    public void SetTarget(Boid boid) { _target = boid; }
    public void RememberHunt(Boid boid) { _lastHuntedBoid = boid; }
    public void ClearHunt() { _lastHuntedBoid = null; }
    public bool MeleeAttack()
    {
        if (!CanAttack || _target == null || !_target.IsAlive) return false;
        _velocity = Vector3.zero;
        _target.TakeDamage(_meleeDamage);
        _tbaTimer = 0f;
        Debug.Log($"Hunter: ataque MELEE a {_target.name}", this);
        return true;
    }
    public bool RangedAttack()
    {
        if (!CanAttack || _target == null || !_target.IsAlive) return false;
        _velocity = Vector3.zero;
        _target.TakeDamage(_rangeDamage);
        _tbaTimer = 0f;
        Debug.Log($"Hunter: ataque RANGED a {_target.name}", this);
        return true;
    }
    public void UpdateTrapTimer() { _trapTimer -= Time.deltaTime; }
    public void TryGenerateTrap()
    {
        if (Bounds.Instance == null || !Bounds.Instance.Contains(transform.position) ||
            _trapTimer > 0f || _activeTrapCount >= 5 || _trapPrefab == null) return;
        Vector3 pos = transform.position;
        Trap trap = Instantiate(_trapPrefab, pos, Quaternion.identity);
        trap.Activate(pos, this);
        _activeTrapCount++;
        ResetTrapTimer();
        Debug.Log("Hunter: objeto de interes generado", this);
    }
    public void RemoveTrap(Trap trap)
    {
        // The trap calls this once as it is destroyed (no recursion).
        _activeTrapCount = Mathf.Max(0, _activeTrapCount - 1);
    }
    public void ResetTrapTimer()
    {
        _trapTimer = Mathf.Max(0.2f, _trapInterval + Random.Range(-_trapVariation, _trapVariation));
    }
    public void CountGather()
    {
        _gathers++;
        ClearHunt();
        Debug.Log($"Hunter: recolectados {_gathers}/{_requiredGathers}", this);
    }
    public void ResetProgress()
    {
        _gathers = 0;
        _target = null;
        _lastHuntedBoid = null;
        _tbaTimer = _TBA;
        _navigator.Clear();
    }
    public void CaughtByWolf(Wolf wolf)
    {
        if (!_isDead && !_isHidden && WorldZones.Instance != null &&
            !WorldZones.Instance.IsSafe(transform.position))
            _machine.Captured(); // All FSM transitions happen in HunterStateMachine.
    }
    public void SetDead(bool value) { _isDead = value; }
    public void Hide(bool hidden)
    {
        _isHidden = hidden;
        _velocity = Vector3.zero;
        foreach (Renderer r in _renderers) if (r != null) r.enabled = !hidden;
        foreach (Collider c in _colliders) if (c != null) c.enabled = !hidden;
        _label.SetText(hidden ? "" : $"{_machine.CurrentState}  {_gathers}/{_requiredGathers}");
    }
    public void RefreshLabel(string message)
    {
        _label.SetText($"{message}  {_gathers}/{_requiredGathers}");
    }
    public void ShowSafeChoice(ZoneArea safe)
    {
        Debug.Log($"Hunter detecto lobo; huye a {safe.name}", this);
        RefreshLabel("FLEE -> " + safe.name);
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, _meleeAttackRadius);
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, _rangeAttackRadius);
    }
}
