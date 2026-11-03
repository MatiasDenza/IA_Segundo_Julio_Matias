using System.Collections.Generic;
using UnityEngine;

// Attach to a CHILD Sensor object containing a trigger SphereCollider.
[RequireComponent(typeof(SphereCollider))]
public class BoidPerception : MonoBehaviour
{
    [SerializeField] private float _perceptionRadius = 5f;
    [SerializeField] private bool _drawGizmos = true;
    private readonly HashSet<Boid> _neighbors = new HashSet<Boid>();
    private readonly HashSet<Hunter> _hunters = new HashSet<Hunter>();
    private readonly HashSet<Trap> _traps = new HashSet<Trap>();
    public float PerceptionRadius => _perceptionRadius;
    public IEnumerable<Boid> Neighbors => _neighbors;

    private void Awake() { ConfigureSensor(); }
    private void OnValidate() { ConfigureSensor(); }
    private void ConfigureSensor()
    {
        SphereCollider sensor = GetComponent<SphereCollider>();
        if (sensor != null) { sensor.isTrigger = true; sensor.radius = _perceptionRadius; }
    }
    public bool InRange(Vector3 p) => (p - transform.position).sqrMagnitude <=
                                      _perceptionRadius * _perceptionRadius;

    private void OnTriggerEnter(Collider other)
    {
        Boid boid = other.GetComponentInParent<Boid>();
        if (boid != null && boid != GetComponentInParent<Boid>()) _neighbors.Add(boid);
        Hunter hunter = other.GetComponentInParent<Hunter>();
        if (hunter != null) _hunters.Add(hunter);
        Trap trap = other.GetComponentInParent<Trap>();
        if (trap != null) _traps.Add(trap);
    }
    private void OnTriggerExit(Collider other)
    {
        Boid boid = other.GetComponentInParent<Boid>();
        if (boid != null) _neighbors.Remove(boid);
        Hunter hunter = other.GetComponentInParent<Hunter>();
        if (hunter != null) _hunters.Remove(hunter);
        Trap trap = other.GetComponentInParent<Trap>();
        if (trap != null) _traps.Remove(trap);
    }
    public Hunter DetectHunter()
    {
        foreach (Hunter hunter in _hunters)
            if (hunter != null && !hunter.IsDead && InRange(hunter.transform.position) &&
                (NavigationGraph.Instance == null ||
                 NavigationGraph.Instance.ClearLine(transform.position, hunter.transform.position))) return hunter;
        return null;
    }
    public Trap DetectTrap()
    {
        foreach (Trap trap in _traps)
            if (trap != null && trap.IsActive && !trap.IsOccupied && InRange(trap.transform.position)) return trap;
        return null;
    }
    private void OnDrawGizmos()
    {
        if (!_drawGizmos) return;
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, _perceptionRadius);
    }
}
