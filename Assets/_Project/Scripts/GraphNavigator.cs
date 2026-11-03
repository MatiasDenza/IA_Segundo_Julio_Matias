using System.Collections.Generic;
using UnityEngine;

// Route planner. Agent movement still uses the project's steering and velocity.
public class GraphNavigator : MonoBehaviour
{
    [SerializeField] private float _arrivalRadius = 0.65f;
    [SerializeField] private bool _showPath = true;
    private List<GraphNode> _path = new List<GraphNode>();
    private int _next;
    private Vector3 _destination;
    private bool _allowSafe;
    public bool HasPath => _path.Count > 0;
    public Vector3 Destination => _destination;

    public bool GoTo(Vector3 target, bool allowSafe = true)
    {
        if (NavigationGraph.Instance == null) return false;
        if (_path.Count > 0 && _allowSafe == allowSafe &&
            (_destination - target).sqrMagnitude < 1f) return true;
        _destination = target;
        _allowSafe = allowSafe;
        _path = NavigationGraph.Instance.FindPath(transform.position, target, allowSafe);
        _next = 0;
        return _path.Count > 0;
    }

    public void Clear() { _path.Clear(); _next = 0; }

    public bool Arrived(float radius = 0.9f)
    {
        Vector3 diff = _destination - transform.position;
        diff.y = 0f;
        return diff.sqrMagnitude <= radius * radius;
    }

    public Vector3 Steering(Agent agent, float maxSpeed)
    {
        if (_path.Count == 0) return -agent.Velocity;
        Vector3 position = transform.position;
        while (_next < _path.Count && HorizontalDistance(position, _path[_next].Position) < _arrivalRadius)
            _next++;
        Vector3 target = _next < _path.Count ? _path[_next].Position : _destination;
        Vector3 delta = target - position;
        delta.y = 0;
        if (_next == _path.Count && delta.sqrMagnitude < _arrivalRadius * _arrivalRadius)
            return -agent.Velocity;
        // Slow down near EVERY node to avoid overshooting corners and hitting walls.
        float speed = maxSpeed * Mathf.Clamp01(delta.magnitude / 2.0f);
        return delta.normalized * speed - agent.Velocity;
    }

    private static float HorizontalDistance(Vector3 a, Vector3 b)
    {
        a.y = b.y = 0f;
        return Vector3.Distance(a, b);
    }

    private void OnDrawGizmos()
    {
        if (!_showPath || _path == null || _path.Count == 0) return;
        Gizmos.color = Color.magenta;
        Vector3 previous = transform.position;
        for (int i = _next; i < _path.Count; i++)
        {
            if (_path[i] == null) continue;
            Gizmos.DrawLine(previous, _path[i].Position);
            previous = _path[i].Position;
        }
        Gizmos.DrawLine(previous, _destination);
        Gizmos.DrawWireSphere(_destination, 0.25f);
    }
}
