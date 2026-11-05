using UnityEngine;

[RequireComponent(typeof(GraphNavigator))]
public class Wolf : Agent
{
    [Header("Patrulla")]
    [SerializeField] private Transform[] _waypoints;
    [SerializeField] private float _maxSpeed = 3.2f;
    [SerializeField] private float _maxSteering = 10f;
    [Header("Deteccion")]
    [SerializeField] private WolfPerception _perception;
    [SerializeField] private float _captureRadius = 0.9f;
    [SerializeField] private float _searchDuration = 5f;
    [SerializeField] private float _howlCooldown = 3f;

    private WolfStateMachine _machine;
    private GraphNavigator _navigator;
    private Hunter _hunter;
    private int _patrolIndex;
    private Vector3 _lastKnownPosition;
    private bool _hasLastKnown;
    private AgentStatusLabel _label;
    private float _nextHowl;
    public float MaxSpeed => _hunter != null ? Mathf.Min(_maxSpeed, _hunter.MaxSpeed * 0.8f) : _maxSpeed;
    public float SearchDuration => _searchDuration;
    public float CaptureRadius => _captureRadius;
    public Transform[] Waypoints => _waypoints;
    public int PatrolIndex { get => _patrolIndex; set => _patrolIndex = value; }
    public WolfPerception Perception => _perception;
    public GraphNavigator Navigator => _navigator;
    public Vector3 LastKnownPosition => _lastKnownPosition;
    public bool HasLastKnownPosition => _hasLastKnown;
    public Hunter Hunter => _hunter;

    private void Awake()
    {
        _navigator = GetComponent<GraphNavigator>();
        if (_perception == null) _perception = GetComponentInChildren<WolfPerception>();
        _label = gameObject.AddComponent<AgentStatusLabel>();
        _label.Initialize(transform, 2.4f, Color.red);
    }
    private void OnEnable()
    {
        WolfAlert.Howled += OnHowl;
        WolfAlert.HunterUnavailable += OnHunterUnavailable;
    }
    private void OnDisable()
    {
        WolfAlert.Howled -= OnHowl;
        WolfAlert.HunterUnavailable -= OnHunterUnavailable;
    }
    private void Start()
    {
        _hunter = FindFirstObjectByType<Hunter>();
        if (_hunter == null || _perception == null || _navigator == null ||
            NavigationGraph.Instance == null || WorldZones.Instance == null)
        {
            Debug.LogError("Wolf requires Hunter, WolfPerception, GraphNavigator, NavigationGraph and WorldZones.", this);
            enabled = false; return;
        }
        _machine = new WolfStateMachine(this);
    }
    private void Update()
    {
        if (_machine == null) return;
        _machine.Update();
        if (_hunter != null && !_hunter.IsDead && !_hunter.IsHidden &&
            !WorldZones.Instance.IsSafe(_hunter.transform.position) &&
            Vector3.Distance(transform.position, _hunter.transform.position) <= _captureRadius)
            _hunter.CaughtByWolf(this);
        _velocity.y = 0f;
        Vector3 nextPosition = transform.position + _velocity * Time.deltaTime;
        // Even while chasing, never step into a safe zone.
        if (WorldZones.Instance.IsSafe(nextPosition) ||
            !NavigationGraph.Instance.ClearMovement(transform.position, nextPosition))
            _velocity = Vector3.zero;
        else transform.position = nextPosition;
        if (_velocity.sqrMagnitude > 0.001f) transform.forward = _velocity.normalized;
    }
    public void SetSteering(Vector3 steering)
    {
        _velocity = Vector3.ClampMagnitude(_velocity +
            Vector3.ClampMagnitude(steering, _maxSteering * Time.deltaTime), MaxSpeed);
    }
    public void SetVelocity(Vector3 v) { _velocity = v; }
    public void SetLastKnownPosition(Vector3 position)
    {
        _lastKnownPosition = position;
        _hasLastKnown = true;
    }
    public void ForgetPosition() { _hasLastKnown = false; }
    public void SetStatus(string status) { _label.SetText(status); }
    public void EmitHowl(Vector3 position)
    {
        if (Time.time < _nextHowl) return;
        _nextHowl = Time.time + _howlCooldown;
        WolfAlert.Howl(this, position);
    }
    private void OnHowl(Wolf source, Vector3 position)
    {
        if (_machine == null || source == this) return;
        SetLastKnownPosition(position);
        Debug.Log($"{name}: recibio alerta de {source.name}; ultima posicion {position}", this);
        _machine.HeardAlert();
    }
    private void OnHunterUnavailable()
    {
        ForgetPosition();
        if (_machine != null) _machine.LostHunter();
    }
    private void OnDrawGizmos()
    {
        if (!_hasLastKnown) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(_lastKnownPosition, 0.45f);
        Gizmos.DrawLine(transform.position, _lastKnownPosition);
    }
}
