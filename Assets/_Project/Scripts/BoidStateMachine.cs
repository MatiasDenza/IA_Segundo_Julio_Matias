public class BoidStateMachine
{
    private readonly StateMachine _stateMachine = new StateMachine();
    public BoidStateMachine(Boid boid)
    {
        _stateMachine.RegisterState(BoidStateType.Flocking, new FlockingState(boid, _stateMachine));
        _stateMachine.RegisterState(BoidStateType.Evading, new EvadingState(boid, _stateMachine));
        _stateMachine.RegisterState(BoidStateType.Lured, new LuredState(boid, _stateMachine));
        _stateMachine.RegisterState(BoidStateType.Trapped, new TrappedState(boid, _stateMachine));
        _stateMachine.RegisterState(BoidStateType.Hunted, new HuntedState(boid));
        _stateMachine.ChangeState(BoidStateType.Flocking);
    }
    public void Update() { _stateMachine.Update(); }
    public void ChangeState(BoidStateType state) { _stateMachine.ChangeState(state); }
}
public enum BoidStateType { Flocking, Evading, Lured, Trapped, Hunted }
