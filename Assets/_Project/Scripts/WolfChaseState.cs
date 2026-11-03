using UnityEngine;
public class WolfChaseState : State
{
    private readonly Wolf _wolf;
    public WolfChaseState(Wolf wolf) { _wolf = wolf; }
    public override void Enter()
    {
        _wolf.Navigator.Clear();
        _wolf.SetStatus("CHASE / DETECTADO");
        Debug.Log($"{_wolf.name}: detecto visualmente al Cazador", _wolf);
    }
    public override void Update()
    {
        Hunter hunter = _wolf.Perception.DetectHunter();
        if (hunter == null) { _wolf.SetVelocity(Vector3.zero); return; }
        _wolf.SetSteering(Steering.Pursuit(_wolf, hunter, _wolf.MaxSpeed));
    }
}
