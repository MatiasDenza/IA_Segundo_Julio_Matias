using UnityEngine;
public class AttackState : State
{
    private readonly Hunter _hunter;
    private readonly StateMachine _fsm;
    public AttackState(Hunter hunter, StateMachine fsm) { _hunter = hunter; _fsm = fsm; }
    public override void Enter()
    {
        _hunter.Navigator.Clear();
        _hunter.RefreshLabel("ATTACK");
    }
    public override void Update()
    {
        Boid boid = _hunter.Target;
        if (boid == null) { _fsm.ChangeState(HunterStateType.Search); return; }
        if (boid.IsDead)
        {
            _fsm.ChangeState(_hunter.Perception.CanSee(boid.transform.position) ?
                HunterStateType.Gather : HunterStateType.Search);
            return;
        }
        if (!boid.IsAlive || !_hunter.Perception.CanSee(boid.transform.position))
        {
            _fsm.ChangeState(HunterStateType.Search); return;
        }
        float d = Vector3.Distance(boid.transform.position, _hunter.transform.position);
        bool success = false;
        if (d <= _hunter.MeleeAttackRadius) success = _hunter.MeleeAttack();
        else if (d <= _hunter.RangeAttackRadius) success = _hunter.RangedAttack();
        else
        {
            // Only steer directly to a Boid when it is in direct line of sight.
            _hunter.SetSteering(Steering.Pursuit(_hunter, boid, _hunter.MaxSpeed));
            return;
        }
        if (success)
        {
            if (boid.IsAlive) _hunter.RememberHunt(boid);
            else _hunter.ClearHunt();
            _fsm.ChangeState(HunterStateType.Search); // Explicit Parcial 2 requirement.
        }
    }
}
