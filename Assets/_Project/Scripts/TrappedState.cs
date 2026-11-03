using UnityEngine;
public class TrappedState : State
{
    private readonly Boid _boid;
    private readonly StateMachine _fsm;
    private float _hitTimer;
    public TrappedState(Boid boid, StateMachine fsm) { _boid = boid; _fsm = fsm; }
    public override void Enter() { _hitTimer = 0f; _boid.SetStatus("DESTRUYENDO TRAMPA"); }
    public override void Update()
    {
        _boid.SetVelocity(Vector3.zero);
        if (_boid.Perception.DetectHunter() != null)
        {
            _fsm.ChangeState(BoidStateType.Evading);
            return;
        }
        if (_boid.TargetTrap == null || !_boid.TargetTrap.IsActive)
        {
            _boid.ClearTargetTrap();
            _fsm.ChangeState(BoidStateType.Flocking);
            return;
        }
        _hitTimer += Time.deltaTime;
        if (_hitTimer < _boid.TrapDamageInterval) return;
        _hitTimer = 0f;
        _boid.TargetTrap.TakeDamage(_boid.TrapDamage);
    }
    public override void Exit()
    {
        if (_boid.TargetTrap != null) _boid.TargetTrap.Release();
    }
}
