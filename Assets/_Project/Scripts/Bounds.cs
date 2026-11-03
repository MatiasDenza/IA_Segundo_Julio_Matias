using UnityEngine;

// The hunting ground is a rectangle on the XZ plane (not the entire level).
public class Bounds : MonoBehaviour
{
    public static Bounds Instance { get; private set; }

    [SerializeField] private float _height = 30f;
    [SerializeField] private float _width = 60f;
    [SerializeField] private bool _drawGizmos = true;
    public Vector3 Center => transform.position;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public bool Contains(Vector3 position)
    {
        Vector3 p = position - Center;
        return Mathf.Abs(p.x) <= _width * 0.5f && Mathf.Abs(p.z) <= _height * 0.5f;
    }

    // Kept for compatibility with the boid behavior of Parcial 1.
    public Vector3 OutOfBounds(Vector3 position)
    {
        Vector3 p = position - Center;
        if (p.x > _width * 0.5f) p.x = -_width * 0.5f + 0.15f;
        if (p.x < -_width * 0.5f) p.x = _width * 0.5f - 0.15f;
        if (p.z > _height * 0.5f) p.z = -_height * 0.5f + 0.15f;
        if (p.z < -_height * 0.5f) p.z = _height * 0.5f - 0.15f;
        return new Vector3(Center.x + p.x, position.y, Center.z + p.z);
    }

    public Vector3 RandomPosition()
    {
        return new Vector3(Center.x + Random.Range(-_width * 0.45f, _width * 0.45f),
                           1f, Center.z + Random.Range(-_height * 0.45f, _height * 0.45f));
    }

    private void OnDrawGizmos()
    {
        if (!_drawGizmos) return;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position, new Vector3(_width, 0.1f, _height));
    }
}
