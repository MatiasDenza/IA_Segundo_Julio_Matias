using UnityEngine;
public class WolfPatrolState : State
{
    private readonly Wolf _wolf;
    public WolfPatrolState(Wolf wolf) { _wolf = wolf; }
    public override void Enter()
    {
        _wolf.Navigator.Clear();
        _wolf.SetStatus("PATROL");
    }
    public override void Update()
    {
        Transform[] points = _wolf.Waypoints;
        if (points == null || points.Length == 0) { _wolf.SetVelocity(Vector3.zero); return; }
        Transform target = points[_wolf.PatrolIndex % points.Length];
        if (target == null || WorldZones.Instance.IsSafe(target.position))
        {
            _wolf.PatrolIndex = (_wolf.PatrolIndex + 1) % points.Length;
            return;
        }
        if (Vector3.Distance(_wolf.transform.position, target.position) < 0.85f)
        {
            _wolf.PatrolIndex = (_wolf.PatrolIndex + 1) % points.Length;
            _wolf.Navigator.Clear();
            return;
        }
        if (NavigationGraph.Instance.ClearLine(_wolf.transform.position, target.position))
        {
            _wolf.Navigator.Clear();
            _wolf.SetSteering(Steering.Arrive(_wolf, target, _wolf.MaxSpeed, 2f));
        }
        else if (_wolf.Navigator.GoTo(target.position, false))
            _wolf.SetSteering(_wolf.Navigator.Steering(_wolf, _wolf.MaxSpeed));
        else _wolf.SetVelocity(Vector3.zero);
    }
}
