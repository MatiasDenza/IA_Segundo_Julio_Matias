using UnityEngine;
public class EvadingState : State
{
    private readonly Boid _boid;
    private readonly StateMachine _fsm;
    public EvadingState(Boid boid, StateMachine fsm) { _boid = boid; _fsm = fsm; }
    public override void Enter() { _boid.SetStatus("EVADE"); }
    public override void Update()
    {
        Hunter hunter = _boid.Perception.DetectHunter();
        if (hunter == null) { _fsm.ChangeState(BoidStateType.Flocking); return; }
        _boid.SetSteering(Steering.Evade(_boid, hunter, _boid.MaxSpeed));
    }
}
