using UnityEngine;
public class LuredState : State
{
    private readonly Boid _boid;
    private readonly StateMachine _fsm;
    public LuredState(Boid boid, StateMachine fsm) { _boid = boid; _fsm = fsm; }
    public override void Enter() { _boid.SetStatus("ARRIVE TRAMPA"); }
    public override void Update()
    {
        if (_boid.Perception.DetectHunter() != null)
        {
            _boid.ClearTargetTrap();
            _fsm.ChangeState(BoidStateType.Evading);
            return;
        }
        Trap trap = _boid.TargetTrap;
        if (trap == null || !trap.IsActive || trap.IsOccupied)
        {
            _boid.ClearTargetTrap();
            _fsm.ChangeState(BoidStateType.Flocking);
            return;
        }
        if (Vector3.Distance(_boid.transform.position, trap.transform.position) <= 1f)
        {
            _boid.SetVelocity(Vector3.zero);
            trap.Occupy();
            _fsm.ChangeState(BoidStateType.Trapped);
            return;
        }
        _boid.SetSteering(Steering.Arrive(_boid, trap.transform, _boid.MaxSpeed, 2f));
    }
}
