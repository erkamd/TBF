using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RedTeamAI : MonoBehaviour
{
    public IEnumerator TakeTurn(AgentController agent)
    {
        // small delay before starting actions
        yield return new WaitForSeconds(0.3f);

        while (agent.actionPoints > 0)
        {
            if (agent.hasBall)
            {
                // If close to the opponent goal, shoot (hard pass to goal cell)
                if (TryShoot(agent))
                {
                    Ball.Instance.AdvanceWithVelocity();
                    yield return new WaitForSeconds(0.3f);
                    continue;
                }

                // Try to pass forward to a teammate
                if (TryPassForward(agent))
                {
                    Ball.Instance.AdvanceWithVelocity();
                    yield return new WaitForSeconds(0.3f);
                    continue;
                }

                // Otherwise move towards the opponent goal
                Vector2Int target = agent.gridPosition + Vector2Int.left;
                if (!GameManager.Instance.IsCellOccupied(target))
                {
                    if (agent.SpendActionPoints(1))
                    {
                        agent.MoveTo(target);
                        Ball.Instance.AdvanceWithVelocity();
                    }
                }
                else
                {
                    agent.SpendActionPoints(1); // wait if blocked
                }
            }
            else
            {
                // If on the ball's cell, take control
                if (Ball.Instance != null && Ball.Instance.gridPosition == agent.gridPosition && !Ball.Instance.IsTravelling())
                {
                    agent.hasBall = true;
                }
                else
                {
                    // Move towards the ball
                    Vector2Int next = StepTowards(agent.gridPosition, Ball.Instance.gridPosition);
                    if (!GameManager.Instance.IsCellOccupied(next))
                    {
                        if (agent.SpendActionPoints(1))
                        {
                            agent.MoveTo(next);
                            Ball.Instance.AdvanceWithVelocity();
                        }
                    }
                    else
                    {
                        agent.SpendActionPoints(1); // wait if blocked
                    }
                }
            }

            yield return new WaitForSeconds(0.3f);
        }

        GameManager.Instance.EndAgentTurn();
    }

    public IEnumerator HandleImmediateAction(AgentController agent)
    {
        // small delay to mimic thinking time
        yield return new WaitForSeconds(0.2f);

        if (TryShoot(agent))
        {
            Ball.Instance.AdvanceWithVelocity();
            yield break;
        }

        if (TryPassForward(agent))
        {
            Ball.Instance.AdvanceWithVelocity();
            yield break;
        }

        agent.hasBall = true;
    }

    private Vector2Int StepTowards(Vector2Int from, Vector2Int to)
    {
        Vector2Int step = from;
        if (from.x != to.x)
            step.x += from.x < to.x ? 1 : -1;
        else if (from.y != to.y)
            step.y += from.y < to.y ? 1 : -1;
        return step;
    }

    private bool TryShoot(AgentController agent)
    {
        if (agent.gridPosition.x <= 1)
        {
            Vector2Int target = new Vector2Int(-1, agent.gridPosition.y);
            if (agent.SpendActionPoints(1))
            {
                agent.hasBall = false;
                Ball.Instance.PassTo(target, true);
                return true;
            }
        }
        return false;
    }

    private bool TryPassForward(AgentController agent)
    {
        AgentController best = null;
        int bestX = agent.gridPosition.x;
        foreach (var a in GameManager.Instance.AIAgents)
        {
            if (a == agent) continue;
            if (a.gridPosition.x < bestX)
            {
                best = a;
                bestX = a.gridPosition.x;
            }
        }
        if (best != null && agent.SpendActionPoints(1))
        {
            agent.hasBall = false;
            Ball.Instance.PassTo(best.gridPosition, false);
            return true;
        }
        return false;
    }
}
