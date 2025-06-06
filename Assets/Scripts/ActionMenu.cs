using UnityEngine;
using UnityEngine.UI;

public class ActionMenu : MonoBehaviour
{
    public Text menuText;
    private AgentController agent;

    public void Open(AgentController selected)
    {
        agent = selected;
        gameObject.SetActive(true);
        UpdateText();
    }

    public void Close()
    {
        gameObject.SetActive(false);
        agent = null;
    }

    private void UpdateText()
    {
        if (agent == null) return;
        menuText.text = $"AP: {agent.actionPoints}\n" +
                        "1) Move (1)\n" +
                        "2) End Agent";
    }

    private void Update()
    {
        if (!gameObject.activeSelf || agent == null) return;

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            if (agent.SpendActionPoints(1))
            {
                agent.MoveTo(agent.gridPosition + Vector2Int.right);
            }
            UpdateText();
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            agent.actionPoints = 0;
            UpdateText();
        }
    }
}
