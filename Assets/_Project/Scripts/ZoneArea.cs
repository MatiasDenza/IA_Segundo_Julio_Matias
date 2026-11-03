using UnityEngine;

// Give each safe zone an independent trigger BoxCollider.
[RequireComponent(typeof(BoxCollider))]
public class ZoneArea : MonoBehaviour
{
    [SerializeField] private Color _gizmoColor = new Color(0.2f, 0.8f, 0.5f, 0.35f);
    private BoxCollider _box;
    public Vector3 Center
    {
        get
        {
            if (_box == null) _box = GetComponent<BoxCollider>();
            return transform.TransformPoint(_box.center);
        }
    }
    private void Awake() { _box = GetComponent<BoxCollider>(); _box.isTrigger = true; }
    public bool Contains(Vector3 position)
    {
        if (_box == null) _box = GetComponent<BoxCollider>();
        // Check XZ only; a character standing on the floor is also inside the zone.
        Vector3 p = transform.InverseTransformPoint(position) - _box.center;
        Vector3 s = _box.size * 0.5f;
        return Mathf.Abs(p.x) <= s.x && Mathf.Abs(p.z) <= s.z;
    }
    private void OnDrawGizmos()
    {
        BoxCollider box = GetComponent<BoxCollider>();
        if (box == null) return;
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.color = _gizmoColor;
        Gizmos.DrawCube(box.center, box.size);
        Gizmos.DrawWireCube(box.center, box.size);
        Gizmos.matrix = Matrix4x4.identity;
    }
}
