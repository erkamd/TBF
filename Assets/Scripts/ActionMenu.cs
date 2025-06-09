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
                        (CanTackle() ? "6) Tackle\n" : "") +
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

    private bool CanTackle()
    {
        Vector2Int[] dirs = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
        foreach (var d in dirs)
        {
            var other = GameManager.Instance.GetAgentAtCell(agent.gridPosition + d);
            if (other != null && other != agent && other.hasBall)
            {
                // Only allow tackling opponents
                bool sameTeam = GameManager.Instance.PlayerAgents.Contains(agent) ?
                                  GameManager.Instance.PlayerAgents.Contains(other) :
                                  GameManager.Instance.AIAgents.Contains(other);
                if (!sameTeam)
                    return true;
            }
        }
        return false;
    }

    private void TryTackle()
    {
        if (agent.actionPoints <= 0) return;

        Vector2Int[] dirs = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
        foreach (var d in dirs)
        {
            var other = GameManager.Instance.GetAgentAtCell(agent.gridPosition + d);
            if (other != null && other.hasBall)
            {
                bool sameTeam = GameManager.Instance.PlayerAgents.Contains(agent) ?
                                  GameManager.Instance.PlayerAgents.Contains(other) :
                                  GameManager.Instance.AIAgents.Contains(other);
                if (sameTeam) continue;

                if (!agent.SpendActionPoints(1))
                    return;

                int attackRoll = Dice.Roll(20) + agent.stats.defending;
                int defenseRoll = Dice.Roll(20) + other.stats.ballControl;

                if (attackRoll >= defenseRoll)
                {
                    other.hasBall = false;
                    agent.hasBall = true;
                    Debug.Log($"Tackle success by {agent.jerseyNumber} on {other.jerseyNumber}");
                }
                else
                {
                    Debug.Log($"Tackle failed by {agent.jerseyNumber} on {other.jerseyNumber}");
                }

                if (agent.actionPoints == 0)
                    GameManager.Instance.EndAgentTurn();

                UpdateText();
                return;
            }
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
        if (agent == null) return;
        passMode = false;
        UpdateText();

        // Only allow passing if agent has the ball and enough AP
        if (agent.hasBall && agent.SpendActionPoints(1))
        {
            Debug.Log($"Passed ball to {targetCell}");
            if (agent.actionPoints == 0)
            {
                GameManager.Instance.EndAgentTurn();
            }
            agent.hasBall = false;

            if (Ball.Instance != null)
            {
                Ball.Instance.PassTo(targetCell, hardPass);
                Ball.Instance.AdvanceWithVelocity();
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

        if (Input.GetKeyDown(KeyCode.Alpha6))
        {
            TryTackle();
        }
    }
}
