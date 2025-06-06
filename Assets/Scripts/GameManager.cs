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

        // Place camera at the geometric center of the pitch using the four corners
        var rows = GridManager.Instance.rows;
        var columns = GridManager.Instance.columns;

        var topLeft = GridManager.Instance.CellToWorld(new Vector2Int(0, 0));
        var bottomRight = GridManager.Instance.CellToWorld(new Vector2Int(rows - 1, columns - 1));

        // Calculate the average position
        var centerWorld = (topLeft + bottomRight) * 0.5f;

        var camera = Camera.main;
        if (camera != null)
        {
            camera.transform.position = new Vector3(centerWorld.x, centerWorld.y, centerWorld.z - 10f); // Adjust height and distance as needed
            //camera.transform.LookAt(new Vector3(centerWorld.x, 0f, centerWorld.z));
        }

        StartTeamTurn(0);
        SetupUI();
    }

    private void SpawnTeams()
    {
        int centerColumn = GridManager.Instance.columns / 2;
        for (int i = 0; i < 3; i++)
        {
            var a = Instantiate(agentPrefab);
            var ac = a.GetComponent<AgentController>();
            ac.Initialize(new Vector2Int(3, centerColumn - 1 + i));
            teamA.Add(ac);

            var b = Instantiate(agentPrefab);
            var bc = b.GetComponent<AgentController>();
            bc.Initialize(new Vector2Int(GridManager.Instance.rows - 4, centerColumn - 1 + i));
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
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); // Changed here
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
