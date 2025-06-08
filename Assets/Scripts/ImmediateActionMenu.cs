using UnityEngine;
using UnityEngine.UI;

public class ImmediateActionMenu : MonoBehaviour
{
    public Text menuText;
    public AgentController agent { get; private set; }
    private bool passMode = false;

    public bool IsPassMode() => passMode;
    public bool IsOpen() => gameObject.activeSelf;

    public void Open(AgentController a)
    {
        agent = a;
        gameObject.SetActive(true);
        passMode = agent.isGoalkeeper; // GK goes directly to pass mode
        UpdateText();
    }

    public void Close()
    {
        gameObject.SetActive(false);
        agent = null;
        passMode = false;
    }

    private void CloseAndResume()
    {
        Close();
        GameManager.Instance.FinishImmediateAction();
    }

    private void UpdateText()
    {
        if (agent == null) return;

        if (passMode)
        {
            menuText.text = "Click a cell for one-touch pass.";
            return;
        }

        if (agent.isGoalkeeper)
        {
            menuText.text = "Click a cell for one-touch pass.";
            passMode = true;
        }
        else
        {
            menuText.text = "Immediate Action:\n" +
                            "1) Control Ball\n" +
                            "2) One-Touch Pass\n" +
                            "3) Do Nothing";
        }
    }

    public void PassOrder(Vector2Int cell)
    {
        if (agent == null) return;
        passMode = false;
        Ball.Instance.PassTo(cell, true);
        Ball.Instance.AdvanceWithVelocity();
        CloseAndResume();
    }

    private void Update()
    {
        if (!gameObject.activeSelf || agent == null) return;

        if (agent.isGoalkeeper || passMode) return;

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            agent.hasBall = true;

            CloseAndResume();
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            passMode = true;
            UpdateText();
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            CloseAndResume();
        }
    }
}
