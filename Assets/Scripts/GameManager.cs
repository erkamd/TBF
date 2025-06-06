using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public GameObject agentPrefab;
    public GameObject ballPrefab;

    private readonly List<AgentController> teamA = new();
    private readonly List<AgentController> teamB = new();
    private Ball ball;
    private int currentTeam; // 0 = teamA, 1 = teamB

    public static GameManager Instance { get; private set; }

    public bool IsPlayerTurn => currentTeam == 0;
    public List<AgentController> PlayerAgents => teamA;

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
        if (ballPrefab == null)
        {
            ballPrefab = Resources.Load<GameObject>("Prefabs/Ball");
        }

        SpawnTeams();
        SpawnBall();
        StartTeamTurn(0);
        SetupUI();
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

    private void SpawnBall()
    {
        var obj = Instantiate(ballPrefab);
        ball = obj.GetComponent<Ball>();
        ball.MoveTo(teamA[0].gridPosition);
    }

    private void StartTeamTurn(int teamIndex)
    {
        currentTeam = teamIndex;
        var team = currentTeam == 0 ? teamA : teamB;
        foreach (var a in team)
        {
            a.ResetActionPoints();
        }

        if (currentTeam == 1)
        {
            // very naive AI that immediately ends its turn
            EndTeamTurn();
        }
    }

    private void SetupUI()
    {
        var canvasObj = new GameObject("UI");
        var canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
        canvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();

        var textObj = new GameObject("ActionText");
        textObj.transform.SetParent(canvasObj.transform);
        var text = textObj.AddComponent<UnityEngine.UI.Text>();
        text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        text.alignment = TextAnchor.UpperLeft;
        var rt = text.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0, 1);
        rt.anchorMax = new Vector2(0, 1);
        rt.pivot = new Vector2(0, 1);
        rt.anchoredPosition = new Vector2(10, -10);

        var menu = canvasObj.AddComponent<ActionMenu>();
        menu.menuText = text;

        var pc = gameObject.AddComponent<PlayerController>();
        pc.actionMenu = menu;
    }

    public void EndTeamTurn()
    {
        StartTeamTurn(1 - currentTeam);
    }
}
