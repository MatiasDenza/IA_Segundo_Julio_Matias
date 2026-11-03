using UnityEngine;
public class HuntedState : State
{
    private readonly Boid _boid;
    public HuntedState(Boid boid) { _boid = boid; }
    public override void Enter() { _boid.SetVelocity(Vector3.zero); }
    public override void Update() { _boid.SetVelocity(Vector3.zero); }
}
