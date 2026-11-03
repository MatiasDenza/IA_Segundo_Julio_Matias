using UnityEngine;
public class DeadState : State
{
    private readonly Hunter _hunter;
    private readonly StateMachine _fsm;
    private float _timer;
    public DeadState(Hunter hunter, StateMachine fsm) { _hunter = hunter; _fsm = fsm; }
    public override void Enter()
    {
        _timer = 0f;
        _hunter.SetDead(true);
        _hunter.Hide(true);
        _hunter.Navigator.Clear();
        WolfAlert.SendHunterUnavailable();
        Debug.Log("Hunter: capturado por un lobo; DEAD", _hunter);
    }
    public override void Update()
    {
        _timer += Time.deltaTime;
        if (_timer < _hunter.RespawnTime) return;
        _hunter.ResetProgress();
        _hunter.transform.position = WorldZones.Instance.StartPoint.position;
        _hunter.SetDead(false);
        _hunter.Hide(false);
        _fsm.ChangeState(HunterStateType.Search);
        Debug.Log("Hunter: reaparecio en zona inicial", _hunter);
    }
}
