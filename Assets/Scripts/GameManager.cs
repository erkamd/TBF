using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public GameObject agentPrefab;

    private readonly List<AgentController> teamA = new();
    private readonly List<AgentController> teamB = new();
    private readonly List<AgentController> turnOrder = new();
    private int currentTurn;

    private readonly Dictionary<Vector2Int, AgentController> board = new();
    public BallController ball;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        if (agentPrefab == null)
        {
            agentPrefab = Resources.Load<GameObject>("Prefabs/Agent");
        }

        SpawnTeams();
        SpawnBall();
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
            ac.number = i + 1;
            teamA.Add(ac);
            board[ac.gridPosition] = ac;

            var b = Instantiate(agentPrefab);
            var bc = b.GetComponent<AgentController>();
            bc.Initialize(new Vector2Int(centerRow - 1 + i, GridManager.Instance.columns - 4));
            bc.number = i + 4;
            teamB.Add(bc);
            board[bc.gridPosition] = bc;
        }

        teamA[0].hasBall = true;
    }

    private void SpawnBall()
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        ball = go.AddComponent<BallController>();
        ball.possessor = teamA[0];
        ball.transform.position = teamA[0].transform.position;
    }

    private void DetermineTurnOrder()
    {
        turnOrder.Clear();
        turnOrder.AddRange(teamA);
        turnOrder.AddRange(teamB);
        for (int i = 0; i < turnOrder.Count; i++)
        {
            int j = Random.Range(i, turnOrder.Count);
            var temp = turnOrder[i];
            turnOrder[i] = turnOrder[j];
            turnOrder[j] = temp;
        }
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
        if (ball != null)
        {
            ball.MoveBall();
        }

        currentTurn++;

        if (currentTurn >= turnOrder.Count)
        {
            DetermineTurnOrder();
        }
    }

    public bool IsCellOccupied(Vector2Int cell)
    {
        return board.ContainsKey(cell);
    }

    public void UpdateAgentCell(AgentController agent, Vector2Int newCell)
    {
        board.Remove(agent.gridPosition);
        board[newCell] = agent;
    }

    private void OnGUI()
    {
        GUILayout.BeginHorizontal();
        foreach (var agent in turnOrder)
        {
            string label = agent.number.ToString();
            if (agent == GetCurrentAgent())
                label = $">{label}<";
            GUILayout.Label(label, GUILayout.Width(30));
        }
        GUILayout.EndHorizontal();
    }
}
