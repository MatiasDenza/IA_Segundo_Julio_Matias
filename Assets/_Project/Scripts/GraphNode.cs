using System.Collections.Generic;
using UnityEngine;

public class GraphNode : MonoBehaviour
{
    // Assigned automatically by NavigationGraph, or add them through the Inspector.
    [SerializeField] private List<GraphNode> _neighbors = new List<GraphNode>();
    public List<GraphNode> Neighbors => _neighbors;
    public Vector3 Position => transform.position;

    public void ClearNeighbors() { _neighbors.Clear(); }
    public void AddNeighbor(GraphNode other)
    {
        if (other != null && other != this && !_neighbors.Contains(other)) _neighbors.Add(other);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawSphere(transform.position, 0.18f);
        Gizmos.color = new Color(0.15f, 0.8f, 0.95f, 0.45f);
        foreach (GraphNode neighbor in _neighbors)
            if (neighbor != null) Gizmos.DrawLine(transform.position, neighbor.transform.position);
    }
}
