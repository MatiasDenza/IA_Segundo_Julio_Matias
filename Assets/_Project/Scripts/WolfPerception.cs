using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class WolfPerception : MonoBehaviour
{
    [SerializeField] private float _visionRadius = 11f;
    [Range(1f, 360f)] [SerializeField] private float _visionAngle = 110f;
    private readonly HashSet<Hunter> _hunters = new HashSet<Hunter>();

    private void Awake() { Configure(); }
    private void OnValidate() { Configure(); }
    private void Configure()
    {
        SphereCollider sphere = GetComponent<SphereCollider>();
        if (sphere != null) { sphere.isTrigger = true; sphere.radius = _visionRadius; }
    }
    private void OnTriggerEnter(Collider other)
    {
        Hunter hunter = other.GetComponentInParent<Hunter>();
        if (hunter != null) _hunters.Add(hunter);
    }
    private void OnTriggerExit(Collider other)
    {
        Hunter hunter = other.GetComponentInParent<Hunter>();
        if (hunter != null) _hunters.Remove(hunter);
    }

    public Hunter DetectHunter()
    {
        foreach (Hunter hunter in _hunters)
            if (hunter != null && CanSee(hunter)) return hunter;
        return null;
    }
    public bool CanSee(Hunter hunter)
    {
        if (hunter == null || hunter.IsDead || WorldZones.Instance == null ||
            WorldZones.Instance.IsSafe(hunter.transform.position)) return false;
        Vector3 direction = hunter.transform.position - transform.position;
        direction.y = 0f;
        if (direction.sqrMagnitude > _visionRadius * _visionRadius) return false;
        Vector3 forward = transform.parent != null ? transform.parent.forward : transform.forward;
        forward.y = 0f;
        if (Vector3.Angle(forward, direction) > _visionAngle * 0.5f) return false;
        return NavigationGraph.Instance == null ||
               NavigationGraph.Instance.ClearLine(transform.position, hunter.transform.position);
    }
    private void OnDrawGizmos()
    {
        Vector3 forward = transform.parent != null ? transform.parent.forward : transform.forward;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, _visionRadius);
        Vector3 left = Quaternion.AngleAxis(-_visionAngle * 0.5f, Vector3.up) * forward;
        Vector3 right = Quaternion.AngleAxis(_visionAngle * 0.5f, Vector3.up) * forward;
        Gizmos.DrawRay(transform.position, left * _visionRadius);
        Gizmos.DrawRay(transform.position, right * _visionRadius);
    }
}
