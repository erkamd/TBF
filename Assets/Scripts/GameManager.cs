using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public GameObject agentPrefab;

    private readonly List<AgentController> teamA = new();
    private readonly List<AgentController> teamB = new();
    private readonly List<AgentController> turnOrder = new();
    private int currentTurn;

    private void Start()
    {
        if (agentPrefab == null)
        {
            agentPrefab = Resources.Load<GameObject>("Prefabs/Agent");
        }

        SpawnTeams();
        DetermineTurnOrder();
    }

    private void SpawnTeams()
    {
        int centerRow = GridManager.Instance.rows / 2;
        for (int i = 0; i < 3; i++)
        {
            var a = Instantiate(agentPrefab);
            var ac = a.GetComponent<AgentController>();
            ac.Initialize(new Vector2Int(centerRow - 1 + i, 3));
            teamA.Add(ac);

            var b = Instantiate(agentPrefab);
            var bc = b.GetComponent<AgentController>();
            bc.Initialize(new Vector2Int(centerRow - 1 + i, GridManager.Instance.columns - 4));
            teamB.Add(bc);
        }

        teamA[0].hasBall = true;
    }

    private void DetermineTurnOrder()
    {
        turnOrder.Clear();
        turnOrder.AddRange(teamA);
        turnOrder.AddRange(teamB);
        turnOrder.Sort((a, b) => (b.stats.awareness + Dice.Roll(20)).CompareTo(a.stats.awareness + Dice.Roll(20)));
        currentTurn = 0;
    }

    public AgentController GetCurrentAgent()
    {
        if (turnOrder.Count == 0)
            return null;
        return turnOrder[currentTurn % turnOrder.Count];
    }

    public void EndTurn()
    {
        currentTurn++;
    }
}
