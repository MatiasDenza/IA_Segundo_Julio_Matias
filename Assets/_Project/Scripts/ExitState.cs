using UnityEngine;
public class ExitState : State
{
    private readonly Hunter _hunter;
    private readonly StateMachine _fsm;
    private bool _finished;
    private float _hideTimer;
    public ExitState(Hunter hunter, StateMachine fsm) { _hunter = hunter; _fsm = fsm; }
    public override void Enter()
    {
        _finished = false;
        _hideTimer = 0f;
        _hunter.Navigator.Clear();
        _hunter.RefreshLabel("EXIT");
        _hunter.Navigator.GoTo(WorldZones.Instance.ExitPoint.position);
    }
    public override void Update()
    {
        if (_finished)
        {
            _hideTimer += Time.deltaTime;
            if (_hideTimer < 0.3f) return;
            _hunter.ResetProgress();
            _hunter.transform.position = WorldZones.Instance.StartPoint.position;
            _hunter.Hide(false);
            _fsm.ChangeState(HunterStateType.Search);
            return;
        }
        if (Vector3.Distance(_hunter.transform.position, WorldZones.Instance.ExitPoint.position) < 1f)
        {
            _finished = true;
            _hunter.Hide(true);
            WolfAlert.SendHunterUnavailable();
            Debug.Log("Hunter: llego a la salida, reinicia ciclo", _hunter);
            return;
        }
        if (_hunter.Navigator.GoTo(WorldZones.Instance.ExitPoint.position))
            _hunter.SetSteering(_hunter.Navigator.Steering(_hunter, _hunter.MaxSpeed));
        else _hunter.SetVelocity(Vector3.zero);
    }
}
