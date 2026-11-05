
using System.Collections.Generic;
using UnityEngine;

public class AgentStatusLabel : MonoBehaviour
{
    private static readonly List<AgentStatusLabel> labels = new();

    public static IReadOnlyList<AgentStatusLabel> Labels => labels;
    public static bool ShowDebugHUD = true;

    public string Status { get; private set; } = "";

    private void OnEnable()
    {
        labels.Add(this);
    }

    private void OnDisable()
    {
        labels.Remove(this);
    }

    public void Initialize(Transform parent, float height, Color color)
    {
    }

    public void SetText(string message)
    {
        Status = message;
    }

    public void SetColor(Color color) { }

    private void OnGUI()
    {
        if (!ShowDebugHUD || labels.Count == 0 || labels[0] != this)
            return;

        int count = 0;

        foreach (AgentStatusLabel label in labels)
        {
            if (label.GetComponent<Hunter>() != null ||
                label.GetComponent<Wolf>() != null)
                count++;
        }

        GUI.Box(new Rect(10, 10, 350, 45 + count * 26), "ESTADO DE AGENTES");

        int y = 45;

        foreach (AgentStatusLabel label in labels)
        {
            if (label.GetComponent<Hunter>() == null &&
                label.GetComponent<Wolf>() == null)
                continue;

            GUI.Label(
                new Rect(25, y, 320, 25),
                label.gameObject.name + ": " + label.Status
            );

            y += 26;
        }
    }
}
