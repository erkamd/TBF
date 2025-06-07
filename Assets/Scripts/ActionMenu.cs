using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ActionMenu : MonoBehaviour
{
    public Text menuText;
    private AgentController agent;
    private Coroutine moveCoroutine;
    private bool passMode = false; // Track if we're in pass mode
    private bool hardPass = false;

    public void Open(AgentController selected)
    {
        agent = selected;
        gameObject.SetActive(true);
        passMode = false;
        hardPass = false;
        UpdateText();
    }

    public void Close()
    {
        gameObject.SetActive(false);
        agent = null;
        passMode = false;
        hardPass = false;
    }

    private void UpdateText()
    {
        if (agent == null) return;

        if (passMode)
        {
            menuText.text = $"AP: {agent.actionPoints}\nRight-click a cell to pass.";
            return;
        }

        menuText.text = $"AP: {agent.actionPoints}\n" +
                        "Click a cell to move.\n" +
                        (agent.hasBall ? "1) Soft Pass\n2) Hard Pass\n" : "") +
                        (CanControlBall() ? "5) Control Ball\n" : "") +
                        "3) Wait\n4) End Agent";
    }

    // Returns true if the agent can control the ball
    private bool CanControlBall()
    {
        return Ball.Instance != null &&
               !agent.hasBall &&
               agent.gridPosition == Ball.Instance.gridPosition;
    }

    // Call this to control the ball
    private void ControlBall()
    {
        if (CanControlBall())
        {
            agent.hasBall = true;
            Debug.Log($"Agent {agent.jerseyNumber} now controls the ball.");
            UpdateText();
        }
    }

    public void MoveOrder(Vector2Int targetCell)
    {
        if (moveCoroutine != null)
            StopCoroutine(moveCoroutine);

        moveCoroutine = StartCoroutine(MoveAgentStepByStep(targetCell));
    }

    public void PassOrder(Vector2Int targetCell)
    {
        passMode = false;
        UpdateText();

        // Only allow passing if agent has the ball and enough AP
        if (agent.hasBall && agent.SpendActionPoints(1))
        {
            agent.hasBall = false;
            if (Ball.Instance != null)
            {
                Ball.Instance.PassTo(targetCell, hardPass);
                Ball.Instance.AdvanceWithVelocity();
            }
            Debug.Log($"Passed ball to {targetCell}");
            if (agent.actionPoints == 0)
            {
                GameManager.Instance.EndAgentTurn();
            }
        }
        else
        {
            Debug.Log("Cannot pass: no ball or not enough AP.");
        }
    }

    public bool IsPassMode() => passMode;

    private void WaitOneAP()
    {
        if (agent.SpendActionPoints(1))
        {
            Ball.Instance.AdvanceWithVelocity();
            UpdateText();

            if (agent.actionPoints == 0)
            {
                GameManager.Instance.EndAgentTurn();
            }
        }
    }

    private IEnumerator MoveAgentStepByStep(Vector2Int targetCell)
    {
        while (agent.actionPoints > 0 && agent.gridPosition != targetCell)
        {
            Vector2Int nextStep = GetNextStep(agent.gridPosition, targetCell);
            var occupant = GameManager.Instance.GetAgentAtCell(nextStep);
            if (occupant != null && occupant != agent)
                break;

            if (nextStep == agent.gridPosition) // No progress possible
                break;

            if (agent.SpendActionPoints(1))
            {
                agent.MoveTo(nextStep);
                Ball.Instance.AdvanceWithVelocity();
                UpdateText();
            }
            else
            {
                break;
            }
            yield return new WaitForSeconds(0.2f); // Adjust speed as needed
        }

        if (agent.actionPoints == 0)
        {
            GameManager.Instance.EndAgentTurn();
        }
    }

    private Vector2Int GetNextStep(Vector2Int current, Vector2Int target)
    {
        int dx = target.x - current.x;
        int dy = target.y - current.y;
        int stepX = dx == 0 ? 0 : (dx > 0 ? 1 : -1);
        int stepY = dy == 0 ? 0 : (dy > 0 ? 1 : -1);

        // Move diagonally if possible
        if (dx != 0 && dy != 0)
            return new Vector2Int(current.x + stepX, current.y + stepY);
        if (dx != 0)
            return new Vector2Int(current.x + stepX, current.y);
        if (dy != 0)
            return new Vector2Int(current.x, current.y + stepY);
        return current;
    }

    private void Update()
    {
        if (!gameObject.activeSelf || agent == null) return;

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            if (agent.hasBall)
            {
                passMode = true;
                hardPass = false;
                UpdateText();
            }
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            if (agent.hasBall)
            {
                passMode = true;
                hardPass = true;
                UpdateText();
            }
        }

        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            WaitOneAP();
        }

        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            agent.actionPoints = 0;
            passMode = false;
            hardPass = false;
            UpdateText();
            GameManager.Instance.EndAgentTurn();
        }

        if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            ControlBall();
        }
    }
}
