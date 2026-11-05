using System.Text;
using TMPro;
using UnityEngine;
using System.Linq;

public class SimulationHUD : MonoBehaviour
{
    [SerializeField] private TMP_Text statusText;

    private void OnEnable()
    {
        AgentStatusLabel.ShowDebugHUD = false;
    }
    private void OnDisable()
    {
        AgentStatusLabel.ShowDebugHUD = true;
    }
    private void Update()
    {
        if (statusText == null) return;

        StringBuilder text = new StringBuilder();
        text.AppendLine("HUNTER");

        foreach (AgentStatusLabel label in AgentStatusLabel.Labels.OrderBy(label => label.gameObject.name))
        {
            if (label.GetComponent<Hunter>() != null)
                text.AppendLine(label.Status);
        }

        text.AppendLine();
        text.AppendLine("WOLVES");

        foreach (AgentStatusLabel label in AgentStatusLabel.Labels.OrderBy(label => label.gameObject.name))
        {   
            if (label.GetComponent<Wolf>() != null)
                text.AppendLine(label.gameObject.name + ": " + label.Status);
        }

        statusText.text = text.ToString();
    }
}