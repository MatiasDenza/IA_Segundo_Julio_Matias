using UnityEngine;

public class Trap : MonoBehaviour
{
    [SerializeField] private float _maxLife = 40f;
    public bool IsActive { get; private set; } = true;
    public bool IsOccupied { get; private set; }
    private float _life;
    private Hunter _owner;
    private void Awake() { _life = _maxLife; }

    public void Activate(Vector3 position, Hunter owner = null)
    {
        transform.position = position;
        _owner = owner;
        _life = _maxLife;
        IsActive = true;
        IsOccupied = false;
        gameObject.SetActive(true);
    }
    public void Occupy() { if (IsActive) IsOccupied = true; }
    public void Release() { IsOccupied = false; }
    public void TakeDamage(float damage)
    {
        if (!IsActive) return;
        _life -= damage;
        if (_life <= 0f) Deactivate();
    }
    public void Deactivate()
    {
        if (!IsActive) return;
        IsActive = false;
        IsOccupied = false;
        if (_owner != null) _owner.RemoveTrap(this);
        Destroy(gameObject);
    }
}
