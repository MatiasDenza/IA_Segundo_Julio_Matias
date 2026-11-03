using UnityEngine;
public class SearchState : State
{
    private readonly Hunter _hunter;
    private readonly StateMachine _fsm;
    private Vector3 _wanderPoint;
    private bool _hasWanderPoint;
    public SearchState(Hunter hunter, StateMachine fsm) { _hunter = hunter; _fsm = fsm; }
    public override void Enter()
    {
        _hasWanderPoint = false;
        _hunter.Navigator.Clear();
        _hunter.RefreshLabel("SEARCH");
    }
    public override void Update()
    {
        // In Search, collection always has precedence over a live target.
        Boid dead = _hunter.Perception.DetectDeadBoid();
        if (dead != null)
        {
            _hunter.SetTarget(dead);
            _fsm.ChangeState(HunterStateType.Gather);
            return;
        }
        Boid alive = _hunter.Perception.DetectBoid();
        if (alive != null && _hunter.CanAttack)
        {
            _hunter.SetTarget(alive);
            _fsm.ChangeState(HunterStateType.Attack);
            return;
        }
        bool inHuntingGround = Bounds.Instance.Contains(_hunter.transform.position);
        if (!inHuntingGround)
        {
            // Always use A* when moving between map sectors.
            MoveGraph(Bounds.Instance.Center);
            return;
        }
        _hunter.UpdateTrapTimer();
        _hunter.TryGenerateTrap();

        // Keep following an injured boid during cooldown, without staying in Attack.
        // This respects Parcial 2's successful-attack -> Search transition.
        Boid remembered = _hunter.LastHuntedBoid;
        if (!_hunter.CanAttack && remembered != null && remembered.IsAlive &&
            _hunter.Perception.CanSee(remembered.transform.position))
        {
            _hunter.SetSteering(Steering.Pursuit(_hunter, remembered, _hunter.MaxSpeed));
            _hunter.RefreshLabel("SEARCH / RASTREANDO");
            return;
        }
        if (!_hasWanderPoint || HorizontalDistance(_hunter.transform.position, _wanderPoint) < 1.2f)
        {
            GraphNode point = NavigationGraph.Instance.RandomHuntingNode();
            if (point == null) { _hunter.SetVelocity(Vector3.zero); return; }
            _wanderPoint = point.Position;
            _hasWanderPoint = true;
            _hunter.Navigator.Clear();
        }
        _hunter.RefreshLabel("SEARCH");
        MoveGraph(_wanderPoint);
    }
    private void MoveGraph(Vector3 p)
    {
        if (!_hunter.Navigator.GoTo(p)) { _hunter.SetVelocity(Vector3.zero); return; }
        _hunter.SetSteering(_hunter.Navigator.Steering(_hunter, _hunter.MaxSpeed));
    }
    private static float HorizontalDistance(Vector3 a, Vector3 b)
    {
        a.y = b.y = 0f;
        return Vector3.Distance(a, b);
    }
}
