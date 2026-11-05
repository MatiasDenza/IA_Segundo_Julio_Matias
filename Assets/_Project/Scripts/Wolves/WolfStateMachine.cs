public enum WolfStateType { Patrol, Chase, Investigate }

public class WolfStateMachine
{
    private readonly StateMachine _fsm = new StateMachine();
    private readonly Wolf _wolf;
    public WolfStateMachine(Wolf wolf)
    {
        _wolf = wolf;
        _fsm.RegisterState(WolfStateType.Patrol, new WolfPatrolState(wolf));
        _fsm.RegisterState(WolfStateType.Chase, new WolfChaseState(wolf));
        _fsm.RegisterState(WolfStateType.Investigate, new WolfInvestigateState(wolf, _fsm));
        _fsm.ChangeState(WolfStateType.Patrol);
    }
    public void Update()
    {
        Hunter seen = _wolf.Perception.DetectHunter();
        if (seen != null)
        {
            _wolf.SetLastKnownPosition(seen.transform.position);
            if (!(_fsm.CurrentState is WolfChaseState))
                _fsm.ChangeState(WolfStateType.Chase);
            _wolf.EmitHowl(seen.transform.position);
        }
        else if (_fsm.CurrentState is WolfChaseState)
            _fsm.ChangeState(WolfStateType.Investigate);
        _fsm.Update();
    }
    public void HeardAlert()
    {
        if (_fsm.CurrentState is WolfInvestigateState investigating)
            investigating.NewAlert();
        else if (!(_fsm.CurrentState is WolfChaseState))
            _fsm.ChangeState(WolfStateType.Investigate);
    }
    public void LostHunter()
    {
        _wolf.Navigator.Clear();
        _wolf.SetVelocity(UnityEngine.Vector3.zero);
        _fsm.ChangeState(WolfStateType.Patrol);
    }
}
