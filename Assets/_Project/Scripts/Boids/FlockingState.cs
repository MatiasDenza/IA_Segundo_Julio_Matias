public class FlockingState : State
{
    private readonly Boid _boid;
    private readonly StateMachine _fsm;
    public FlockingState(Boid boid, StateMachine fsm) { _boid = boid; _fsm = fsm; }
    public override void Enter() { _boid.SetStatus("FLOCKING"); }
    public override void Update()
    {
        if (_boid.Perception.DetectHunter() != null) { _fsm.ChangeState(BoidStateType.Evading); return; }
        Trap trap = _boid.Perception.DetectTrap();
        if (trap != null)
        {
            _boid.SetTargetTrap(trap);
            _fsm.ChangeState(BoidStateType.Lured);
            return;
        }
        _boid.SetSteering(_boid.CalculateFlocking());
    }
}
