using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class HunterPerception : MonoBehaviour
{
    [SerializeField] private float _perceptionRadius = 10f;
    [SerializeField] private bool _drawGizmos = true;
    private readonly HashSet<Boid> _boids = new HashSet<Boid>();
    private readonly HashSet<Wolf> _wolves = new HashSet<Wolf>();
    public float PerceptionRadius => _perceptionRadius;

    private void Awake() { ConfigureSensor(); }
    private void OnValidate() { ConfigureSensor(); }
    private void ConfigureSensor()
    {
        SphereCollider sensor = GetComponent<SphereCollider>();
        if (sensor != null) { sensor.isTrigger = true; sensor.radius = _perceptionRadius; }
    }
    public bool InRange(Vector3 p) => (p - transform.position).sqrMagnitude <=
                                      _perceptionRadius * _perceptionRadius;
    public bool CanSee(Vector3 position)
    {
        return InRange(position) && (NavigationGraph.Instance == null ||
               NavigationGraph.Instance.ClearLine(transform.position, position));
    }
    private void OnTriggerEnter(Collider other)
    {
        Boid boid = other.GetComponentInParent<Boid>();
        if (boid != null) _boids.Add(boid);
        Wolf wolf = other.GetComponentInParent<Wolf>();
        if (wolf != null) _wolves.Add(wolf);
    }
    private void OnTriggerExit(Collider other)
    {
        Boid boid = other.GetComponentInParent<Boid>();
        if (boid != null) _boids.Remove(boid);
        Wolf wolf = other.GetComponentInParent<Wolf>();
        if (wolf != null) _wolves.Remove(wolf);
    }
    public Boid DetectBoid()
    {
        foreach (Boid boid in _boids)
            if (boid != null && boid.IsAlive && !boid.IsRespawning && CanSee(boid.transform.position)) return boid;
        return null;
    }
    public Boid DetectDeadBoid()
    {
        foreach (Boid boid in _boids)
            if (boid != null && boid.IsDead && !boid.IsRespawning && CanSee(boid.transform.position)) return boid;
        return null;
    }
    public Wolf DetectWolf()
    {
        foreach (Wolf wolf in _wolves)
            if (wolf != null && CanSee(wolf.transform.position)) return wolf;
        return null;
    }
    private void OnDrawGizmos()
    {
        if (!_drawGizmos) return;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, _perceptionRadius);
    }
}
