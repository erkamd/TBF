using UnityEngine;

[RequireComponent(typeof(AgentController))]
public class GoalkeeperAI : MonoBehaviour
{
    private AgentController agent;
    private int side; // -1 for left, 1 for right
    private int minY;
    private int maxY;
    private int keeperX;
    private bool canMove = false;

    private void Start()
    {
        agent = GetComponent<AgentController>();
        var gm = GridManager.Instance;
        side = (agent.gridPosition.x < gm.width / 2) ? -1 : 1;
        keeperX = side == -1 ? 0 : gm.width - 1;
        minY = Mathf.Max(0, gm.GoalStartY - 1);
        maxY = Mathf.Min(gm.height - 1, gm.GoalEndY + 1);
    }

    private Vector2Int DetermineTarget()
    {
        var gm = GridManager.Instance;
        Ball ball = Ball.Instance;
        if (ball == null)
            return agent.gridPosition;

        int goalX = side == -1 ? -1 : gm.width;
        int centerY = (gm.GoalStartY + gm.GoalEndY) / 2;

        // Check if ball is possessed
        var occupant = GameManager.Instance.GetAgentAtCell(ball.gridPosition);
        if (occupant != null && occupant.hasBall)
        {
            Vector2Int pos = occupant.gridPosition;
            float t = (keeperX - pos.x) / (float)(goalX - pos.x);
            float py = pos.y + (centerY - pos.y) * t;
            int y = Mathf.Clamp(Mathf.RoundToInt(py), minY, maxY);
            return new Vector2Int(keeperX, y);
        }
        else if (ball.IsTravelling() && Mathf.Sign(ball.Velocity.x) == side)
        {
            Vector2 posW = ball.transform.position;
            Vector2 vel = ball.Velocity;
            float t = (goalX * gm.cellSize - posW.x) / vel.x;
            float pyWorld = posW.y + vel.y * t;
            int y = Mathf.Clamp(Mathf.RoundToInt(pyWorld / gm.cellSize), minY, maxY);
            return new Vector2Int(keeperX, y);
        }

        return new Vector2Int(keeperX, Mathf.Clamp(centerY, minY, maxY));
    }

    private void MoveTowards(Vector2Int target)
    {
        if (agent.gridPosition == target)
            return;

        Vector2Int next = agent.gridPosition;
        if (target.y > agent.gridPosition.y) next.y += 1;
        else if (target.y < agent.gridPosition.y) next.y -= 1;
        next.x = keeperX;
        next.y = Mathf.Clamp(next.y, minY, maxY);

        if (!GameManager.Instance.IsCellOccupied(next))
        {
            if (agent.SpendActionPoints(1))
            {
                agent.MoveTo(next);
            }
        }
    }

    public void OnActionPointSpent()
    {
        canMove = true;
        TryMove();
    }

    private void TryMove()
    {
        if (!canMove) return;

        Vector2Int target = DetermineTarget();
        if (agent.gridPosition == target)
            return;

        Vector2Int next = agent.gridPosition;
        if (target.y > agent.gridPosition.y) next.y += 1;
        else if (target.y < agent.gridPosition.y) next.y -= 1;
        next.x = keeperX;
        next.y = Mathf.Clamp(next.y, minY, maxY);

        if (!GameManager.Instance.IsCellOccupied(next))
        {
            agent.MoveTo(next);
            canMove = false; // Only move once per AP spent
        }
    }
}
