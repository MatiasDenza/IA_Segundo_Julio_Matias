using UnityEngine;

// Tiny world-space feedback that works without TextMeshPro packages or scene UI.
public class AgentStatusLabel : MonoBehaviour
{
    private TextMesh _text;
    public void Initialize(Transform parent, float height, Color color)
    {
        GameObject label = new GameObject("Status Label");
        label.transform.SetParent(parent, false);
        label.transform.localPosition = Vector3.up * height;
        _text = label.AddComponent<TextMesh>();
        _text.fontSize = 35;
        _text.characterSize = 0.08f;
        _text.anchor = TextAnchor.MiddleCenter;
        _text.alignment = TextAlignment.Center;
        _text.color = color;
        _text.text = "";
    }
    public void SetText(string message) { if (_text != null) _text.text = message; }
    public void SetColor(Color color) { if (_text != null) _text.color = color; }
    private void LateUpdate()
    {
        if (_text != null && Camera.main != null)
            _text.transform.rotation = Camera.main.transform.rotation;
    }
}
