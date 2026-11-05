using System.Collections.Generic;
using UnityEngine;

// Shared graph and A* implementation for the Hunter and every Wolf. No NavMesh.
public class NavigationGraph : MonoBehaviour
{
    public static NavigationGraph Instance { get; private set; }
    [SerializeField] private float _connectionRadius = 7f;
    [SerializeField] private LayerMask _wallMask;
    [SerializeField] private bool _autoConnect = true;
    [SerializeField] private float _rayHeight = 0.6f;
    private GraphNode[] _nodes;
    public GraphNode[] Nodes => _nodes;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        if (_autoConnect) RebuildConnections();
        else _nodes = GetComponentsInChildren<GraphNode>(true);
        if (_wallMask.value == 0)
            Debug.LogWarning("NavigationGraph: assign the Walls layer to Wall Mask to prevent routes through walls.", this);
    }

    [ContextMenu("Rebuild Graph Connections")]
    public void RebuildConnections()
    {
        _nodes = GetComponentsInChildren<GraphNode>(true);
        foreach (GraphNode node in _nodes) node.ClearNeighbors();
        for (int i = 0; i < _nodes.Length; i++)
        for (int j = i + 1; j < _nodes.Length; j++)
        {
            if (Vector3.Distance(_nodes[i].Position, _nodes[j].Position) > _connectionRadius) continue;
            if (!ClearLine(_nodes[i].Position, _nodes[j].Position)) continue;
            _nodes[i].AddNeighbor(_nodes[j]);
            _nodes[j].AddNeighbor(_nodes[i]);
        }
    }

    public bool ClearLine(Vector3 a, Vector3 b)
    {
        return !Physics.Linecast(a + Vector3.up * _rayHeight, b + Vector3.up * _rayHeight,
                                 _wallMask, QueryTriggerInteraction.Ignore);
    }

    // Physical safety guard: follow graph edges without clipping through wall colliders.
    public bool ClearMovement(Vector3 from, Vector3 to, float radius = 0.4f)
    {
        Vector3 delta = to - from;
        delta.y = 0f;
        if (delta.sqrMagnitude < 0.000001f) return true;
        return !Physics.SphereCast(from + Vector3.up * _rayHeight, radius,
                                  delta.normalized, out _, delta.magnitude,
                                  _wallMask, QueryTriggerInteraction.Ignore);
    }

    public GraphNode NearestNode(Vector3 position, bool allowSafe)
    {
        if (_nodes == null) return null;
        GraphNode best = null;
        float min = float.PositiveInfinity;
        foreach (GraphNode node in _nodes)
        {
            if (!CanUse(node, allowSafe)) continue;
            float d = (position - node.Position).sqrMagnitude;
            // Prefer a reachable node, avoiding an initial route across a wall.
            if (d < min && ClearLine(position, node.Position)) { min = d; best = node; }
        }
        return best;
    }

    private bool CanUse(GraphNode node, bool allowSafe)
    {
        return node != null && (allowSafe || WorldZones.Instance == null ||
                               !WorldZones.Instance.IsSafe(node.Position));
    }

    public List<GraphNode> FindPath(Vector3 from, Vector3 to, bool allowSafe)
    {
        GraphNode start = NearestNode(from, allowSafe);
        GraphNode goal = NearestNode(to, allowSafe);
        List<GraphNode> empty = new List<GraphNode>();
        if (start == null || goal == null) return empty;

        List<GraphNode> open = new List<GraphNode> { start };
        HashSet<GraphNode> closed = new HashSet<GraphNode>();
        Dictionary<GraphNode, GraphNode> parent = new Dictionary<GraphNode, GraphNode>();
        Dictionary<GraphNode, float> g = new Dictionary<GraphNode, float> { [start] = 0f };
        Dictionary<GraphNode, float> f = new Dictionary<GraphNode, float>
            { [start] = Vector3.Distance(start.Position, goal.Position) };

        while (open.Count > 0)
        {
            GraphNode current = open[0];
            foreach (GraphNode candidate in open)
                if (f[candidate] < f[current]) current = candidate;
            if (current == goal)
            {
                List<GraphNode> path = new List<GraphNode> { current };
                while (parent.TryGetValue(current, out GraphNode prev))
                {
                    current = prev;
                    path.Add(current);
                }
                path.Reverse();
                return path;
            }
            open.Remove(current);
            closed.Add(current);
            foreach (GraphNode next in current.Neighbors)
            {
                if (!CanUse(next, allowSafe) || closed.Contains(next)) continue;
                float tentative = g[current] + Vector3.Distance(current.Position, next.Position);
                if (!g.ContainsKey(next) || tentative < g[next])
                {
                    parent[next] = current;
                    g[next] = tentative;
                    f[next] = tentative + Vector3.Distance(next.Position, goal.Position);
                    if (!open.Contains(next)) open.Add(next);
                }
            }
        }
        return empty;
    }

    public float PathLength(Vector3 from, Vector3 to, bool allowSafe)
    {
        List<GraphNode> path = FindPath(from, to, allowSafe);
        if (path.Count == 0) return float.PositiveInfinity;
        float length = Vector3.Distance(from, path[0].Position);
        for (int i = 1; i < path.Count; i++)
            length += Vector3.Distance(path[i - 1].Position, path[i].Position);
        return length + Vector3.Distance(path[path.Count - 1].Position, to);
    }

    public GraphNode RandomHuntingNode()
    {
        if (_nodes == null || Bounds.Instance == null) return null;
        List<GraphNode> candidates = new List<GraphNode>();
        foreach (GraphNode node in _nodes)
            if (Bounds.Instance.Contains(node.Position)) candidates.Add(node);
        return candidates.Count > 0 ? candidates[Random.Range(0, candidates.Count)] : null;
    }
}
