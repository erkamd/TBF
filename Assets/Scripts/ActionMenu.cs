using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ActionMenu : MonoBehaviour
{
    public Text menuText;
    private AgentController agent;
    private Coroutine moveCoroutine;
    private bool passMode = false; // Track if we're in pass mode

    public void Open(AgentController selected)
    {
        agent = selected;
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
        menuText.text = $"AP: {agent.actionPoints}\n" +
                        (passMode ? "Right-click a cell to pass.\n"
                                  : "Click a cell to move.\n" +
                                    (agent.hasBall ? "1) Pass\n" : "")) +
                        "2) End Agent";
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
                Ball.Instance.PassTo(targetCell);
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
                UpdateText();
            }
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            agent.actionPoints = 0;
            passMode = false;
            UpdateText();
            GameManager.Instance.EndAgentTurn();
        }
    }
}
