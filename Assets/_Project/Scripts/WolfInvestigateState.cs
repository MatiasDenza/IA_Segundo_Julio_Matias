using UnityEngine;
public class WolfInvestigateState : State
{
    private readonly Wolf _wolf;
    private readonly StateMachine _fsm;
    private float _searchTimer;
    private bool _reachedLastKnown;
    private Vector3 _currentSearchPoint;
    public WolfInvestigateState(Wolf wolf, StateMachine fsm) { _wolf = wolf; _fsm = fsm; }
    public override void Enter()
    {
        _searchTimer = 0f;
        _reachedLastKnown = false;
        _wolf.Navigator.Clear();
        _currentSearchPoint = _wolf.LastKnownPosition;
        _wolf.SetStatus("ALERTA / BUSCANDO");
    }
    public void NewAlert() { Enter(); }
    public override void Update()
    {
        if (!_wolf.HasLastKnownPosition)
        {
            _fsm.ChangeState(WolfStateType.Patrol); return;
        }
        if (!_reachedLastKnown && Vector3.Distance(_wolf.transform.position, _wolf.LastKnownPosition) < 1.25f)
        {
            _reachedLastKnown = true;
            SelectNearbyNode();
        }
        if (_reachedLastKnown)
        {
            _searchTimer += Time.deltaTime;
            if (_searchTimer > _wolf.SearchDuration)
            {
                _wolf.ForgetPosition();
                _fsm.ChangeState(WolfStateType.Patrol); return;
            }
            if (Vector3.Distance(_wolf.transform.position, _currentSearchPoint) < 1.1f)
                SelectNearbyNode();
        }
        Vector3 target = _reachedLastKnown ? _currentSearchPoint : _wolf.LastKnownPosition;
        if (_wolf.Navigator.GoTo(target, false))
            _wolf.SetSteering(_wolf.Navigator.Steering(_wolf, _wolf.MaxSpeed));
        else
        {
            // Unreachable last-seen point: search reachable nodes nearby instead.
            if (!_reachedLastKnown) { _reachedLastKnown = true; SelectNearbyNode(); }
            _wolf.SetVelocity(Vector3.zero);
        }
    }
    private void SelectNearbyNode()
    {
        GraphNode[] nodes = NavigationGraph.Instance.Nodes;
        GraphNode chosen = null;
        int seen = 0;
        if (nodes != null)
            foreach (GraphNode n in nodes)
            {
                if (n == null || WorldZones.Instance.IsSafe(n.Position) ||
                    Vector3.Distance(n.Position, _wolf.LastKnownPosition) > 8f) continue;
                if (float.IsInfinity(NavigationGraph.Instance.PathLength(
                    _wolf.transform.position, n.Position, false))) continue;
                seen++;
                if (Random.Range(0, seen) == 0) chosen = n;
            }
        _currentSearchPoint = chosen != null ? chosen.Position : _wolf.LastKnownPosition;
        _wolf.Navigator.Clear();
    }
}
