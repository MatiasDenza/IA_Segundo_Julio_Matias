using UnityEngine;
public class GatherState : State
{
    private readonly Hunter _hunter;
    private readonly StateMachine _fsm;
    private float _gatherTimer;
    public GatherState(Hunter hunter, StateMachine fsm) { _hunter = hunter; _fsm = fsm; }
    public override void Enter()
    {
        _gatherTimer = 0f;
        _hunter.Navigator.Clear();
        _hunter.RefreshLabel("GATHER");
    }
    public override void Update()
    {
        Boid target = _hunter.Target;
        if (target == null || !target.IsDead || target.IsRespawning)
        {
            _fsm.ChangeState(HunterStateType.Search); return;
        }
        float distance = Vector3.Distance(_hunter.transform.position, target.transform.position);
        if (distance <= 1.1f)
        {
            _hunter.SetVelocity(Vector3.zero);
            target.SetGathering(true);
            _gatherTimer += Time.deltaTime;
            _hunter.RefreshLabel($"GATHER {_gatherTimer:F1}s");
            if (_gatherTimer < _hunter.GatherDuration) return;
            Trap trap = target.TargetTrap;
            target.StartRespawn();
            target.ClearTargetTrap();
            if (trap != null) trap.Deactivate();
            _hunter.CountGather();
            _hunter.SetTarget(null);
            _fsm.ChangeState(_hunter.Gathers >= _hunter.RequiredGathers ?
                HunterStateType.Exit : HunterStateType.Search);
            return;
        }
        target.SetGathering(false);
        _gatherTimer = 0f;
        if (_hunter.Perception.CanSee(target.transform.position))
            _hunter.SetSteering(Steering.Arrive(_hunter, target.transform, _hunter.MaxSpeed, 2f));
        else if (_hunter.Navigator.GoTo(target.transform.position))
            _hunter.SetSteering(_hunter.Navigator.Steering(_hunter, _hunter.MaxSpeed));
        else _hunter.SetVelocity(Vector3.zero);
    }
    public override void Exit()
    {
        if (_hunter.Target != null) _hunter.Target.SetGathering(false);
        _gatherTimer = 0f; // Interrupted gathering never counts.
    }
}
