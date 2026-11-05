using UnityEngine;
public class FleeState : State
{
    private readonly Hunter _hunter;
    private readonly StateMachine _fsm;
    private ZoneArea _safe;
    private float _wait;
    private bool _entered;
    public FleeState(Hunter hunter, StateMachine fsm) { _hunter = hunter; _fsm = fsm; }
    public override void Enter()
    {
        _entered = false;
        _wait = 0f;
        _hunter.Navigator.Clear();
        float shortestPath = float.PositiveInfinity;
        _safe = null;
        foreach (ZoneArea candidate in WorldZones.Instance.SafeZones)
        {
            if (candidate == null) continue;
            float length = NavigationGraph.Instance.PathLength(
                _hunter.transform.position, candidate.Center, true);
            if (length < shortestPath) { shortestPath = length; _safe = candidate; }
        }
        if (_safe != null)
        {
            _hunter.ShowSafeChoice(_safe);
            _hunter.Navigator.GoTo(_safe.Center, true);
        }
        else Debug.LogError("Hunter: no reachable safe zone via A*.", _hunter);
    }
    public override void Update()
    {
        if (_safe == null) { _hunter.SetVelocity(Vector3.zero); return; }
        if (WorldZones.Instance.IsSafe(_hunter.transform.position))
        {
            if (!_entered)
            {
                _entered = true;
                _hunter.SetVelocity(Vector3.zero);
                WolfAlert.SendHunterUnavailable(); // Wolves drop their target immediately.
                Debug.Log("Hunter: en zona segura; los lobos abandonan la persecucion", _hunter);
            }
            _wait += Time.deltaTime;
            _hunter.SetVelocity(Vector3.zero);
            _hunter.RefreshLabel($"SAFE {_wait:F1}/{_hunter.SafeZoneWaitTime:F1}");
            if (_wait >= _hunter.SafeZoneWaitTime)
                _fsm.ChangeState(_hunter.Gathers >= _hunter.RequiredGathers ?
                    HunterStateType.Exit : HunterStateType.Search);
            return;
        }
        if (_hunter.Navigator.GoTo(_safe.Center, true))
            _hunter.SetSteering(_hunter.Navigator.Steering(_hunter, _hunter.MaxSpeed));
        else _hunter.SetVelocity(Vector3.zero);
    }
}
