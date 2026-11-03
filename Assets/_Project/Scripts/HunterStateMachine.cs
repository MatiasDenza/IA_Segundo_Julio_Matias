public enum HunterStateType { Search, Attack, Gather, Flee, Exit, Dead }

public class HunterStateMachine
{
    private readonly StateMachine _fsm = new StateMachine();
    private readonly Hunter _hunter;
    public HunterStateType CurrentState
    {
        get
        {
            State state = _fsm.CurrentState;
            if (state is AttackState) return HunterStateType.Attack;
            if (state is GatherState) return HunterStateType.Gather;
            if (state is FleeState) return HunterStateType.Flee;
            if (state is ExitState) return HunterStateType.Exit;
            if (state is DeadState) return HunterStateType.Dead;
            return HunterStateType.Search;
        }
    }
    public HunterStateMachine(Hunter hunter)
    {
        _hunter = hunter;
        _fsm.RegisterState(HunterStateType.Search, new SearchState(hunter, _fsm));
        _fsm.RegisterState(HunterStateType.Attack, new AttackState(hunter, _fsm));
        _fsm.RegisterState(HunterStateType.Gather, new GatherState(hunter, _fsm));
        _fsm.RegisterState(HunterStateType.Flee, new FleeState(hunter, _fsm));
        _fsm.RegisterState(HunterStateType.Exit, new ExitState(hunter, _fsm));
        _fsm.RegisterState(HunterStateType.Dead, new DeadState(hunter, _fsm));
        _fsm.ChangeState(HunterStateType.Search);
    }
    public void Update()
    {
        // Global priorities are checked INSIDE the Hunter FSM: Dead > Flee > Exit > ...
        if (CurrentState == HunterStateType.Dead || CurrentState == HunterStateType.Flee)
        {
            _fsm.Update(); return;
        }
        if (_hunter.Perception.DetectWolf() != null)
        {
            _fsm.ChangeState(HunterStateType.Flee);
            _fsm.Update(); return;
        }
        if (_hunter.Gathers >= _hunter.RequiredGathers && CurrentState != HunterStateType.Exit)
            _fsm.ChangeState(HunterStateType.Exit);
        _fsm.Update();
    }
    public void Captured()
    {
        _fsm.ChangeState(HunterStateType.Dead);
    }
}
