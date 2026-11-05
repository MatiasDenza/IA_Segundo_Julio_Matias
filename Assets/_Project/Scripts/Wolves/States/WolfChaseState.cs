using UnityEngine;

public class WolfChaseState : State
{
    private readonly Wolf _wolf;

    public WolfChaseState(Wolf wolf)
    {
        _wolf = wolf;
    }

    public override void Enter()
    {
        _wolf.Navigator.Clear();
        _wolf.SetStatus("CHASE / DETECTADO");
        Debug.Log($"{_wolf.name}: detecto al Cazador", _wolf);
    }

    public override void Update()
    {
        Hunter hunter = _wolf.Perception.DetectHunter();

        if (hunter == null)
        {
            _wolf.SetVelocity(Vector3.zero);
            return;
        }

        Vector3 pursuit = Steering.Pursuit(_wolf, hunter, _wolf.MaxSpeed);

        Vector3 predictedMovement = pursuit + _wolf.Velocity;

        if (
            NavigationGraph.Instance.ClearMovement(
                _wolf.transform.position,
                _wolf.transform.position + predictedMovement.normalized * 1.5f
            )
        )
        {
            _wolf.Navigator.Clear();
            _wolf.SetSteering(pursuit);
        }
        else if (_wolf.Navigator.GoTo(hunter.transform.position, false))
        {
            _wolf.SetSteering(_wolf.Navigator.Steering(_wolf, _wolf.MaxSpeed));
        }
        else
        {
            _wolf.SetVelocity(Vector3.zero);
        }
    }
}
