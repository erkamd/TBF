using UnityEngine;
using UnityEngine.UI;

public class ImmediateActionMenu : MonoBehaviour
{
    public Text menuText;
    private AgentController agent;
    private bool passMode = false;

    public void Open(AgentController a)
    {
        agent = a;
        gameObject.SetActive(true);
        passMode = false;
        UpdateText();
    }

    public void Close()
    {
        gameObject.SetActive(false);
        agent = null;
        passMode = false;
    }

    private void UpdateText()
    {
        if (agent == null) return;

        if (passMode)
        {
            menuText.text = "Click a cell for one-touch pass.";
            return;
        }

        menuText.text = "Immediate Action:\n" +
                        "1) Control Ball\n" +
                        "2) One-Touch Pass\n" +
                        "3) Do Nothing";
    }

    public bool IsPassMode() => passMode;

    public void PassOrder(Vector2Int cell)
    {
        if (agent == null) return;
        passMode = false;
        Ball.Instance.PassTo(cell, true);
        Ball.Instance.AdvanceWithVelocity();
        Close();
    }

    private void Update()
    {
        if (!gameObject.activeSelf || agent == null) return;

        if (passMode) return;

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            agent.hasBall = true;
            Close();
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            passMode = true;
            UpdateText();
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            Close();
        }
    }
}
