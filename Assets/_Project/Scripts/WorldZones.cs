using UnityEngine;

public class WorldZones : MonoBehaviour
{
    public static WorldZones Instance { get; private set; }
    [SerializeField] private Transform _startPoint;
    [SerializeField] private Transform _exitPoint;
    [SerializeField] private ZoneArea[] _safeZones;

    public Transform StartPoint => _startPoint;
    public Transform ExitPoint => _exitPoint;
    public ZoneArea[] SafeZones => _safeZones;
    public Vector3 HuntingCenter => Bounds.Instance != null ? Bounds.Instance.Center : transform.position;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public bool IsSafe(Vector3 position)
    {
        if (_safeZones == null) return false;
        foreach (ZoneArea zone in _safeZones)
            if (zone != null && zone.Contains(position)) return true;
        return false;
    }

    public bool HasRequiredReferences => _startPoint != null && _exitPoint != null
        && _safeZones != null && _safeZones.Length >= 2 && Bounds.Instance != null;
}
