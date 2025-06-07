using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public GameObject agentPrefab;
    public GameObject ballPrefab;

    private readonly List<AgentController> teamA = new();
    private readonly List<AgentController> teamB = new();
    private readonly List<AgentController> allAgents = new();
    private readonly List<AgentController> turnOrder = new();
    private int currentAgentIndex = 0;
    private Ball ball;

    public static GameManager Instance { get; private set; }

    public AgentController CurrentAgent => turnOrder.Count > 0 ? turnOrder[currentAgentIndex] : null;
    public List<AgentController> PlayerAgents => teamA;
    public List<AgentController> AllAgents => allAgents;

    public PlayerController playerController;
    public TextMeshProUGUI orderText;

    [Header("Team Spawn Settings")]
    public int agentGap = 1; // Set this in the Inspector

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

        SpawnTeams(agentGap);
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

        SetupUI();
        StartCycle();

        UpdateTurnOrderDisplay();
    }

    private void SpawnTeams(int gap)
    {
        int numAgents = 3;
        int centerColumn = GridManager.Instance.columns / 2;

        int startOffset = -((numAgents - 1) / 2) * gap;

        int jersey = 1;
        for (int i = 0; i < numAgents; i++)
        {
            int col = centerColumn + startOffset + i * gap;

            var a = Instantiate(agentPrefab);
            var ac = a.GetComponent<AgentController>();
            ac.agentColor = Color.blue;
            ac.jerseyNumber = jersey++;
            ac.Initialize(new Vector2Int(3, col));
            teamA.Add(ac);
            allAgents.Add(ac);

            var b = Instantiate(agentPrefab);
            var bc = b.GetComponent<AgentController>();
            bc.agentColor = Color.red;
            bc.jerseyNumber = jersey++;
            bc.Initialize(new Vector2Int(GridManager.Instance.rows - 4, col));
            teamB.Add(bc);
            allAgents.Add(bc);
        }

        teamA[0].hasBall = true;
    }

    private void SpawnBall()
    {
        var obj = Instantiate(ballPrefab);
        ball = obj.GetComponent<Ball>();
        ball.MoveTo(teamA[0].gridPosition);
    }

    private void StartCycle()
    {
        foreach (var a in allAgents)
        {
            a.ResetActionPoints();
        }

        turnOrder.Clear();
        var copy = new List<AgentController>(allAgents);
        while (copy.Count > 0)
        {
            int index = Random.Range(0, copy.Count);
            turnOrder.Add(copy[index]);
            copy.RemoveAt(index);
        }

        currentAgentIndex = 0;
        StartAgentTurn();
    }

    private void StartAgentTurn()
    {
        if (CurrentAgent == null)
            return;
    }

    private void UpdateTurnOrderDisplay()
    {
        if (orderText == null) return;
        var nums = new List<string>();
        for (int i = 0; i < turnOrder.Count; i++)
        {
            var color = (i == currentAgentIndex) ? "#FFFF00" : "#FFFFFF"; // Yellow for current, white for others
            nums.Add($"<color={color}>{turnOrder[i].jerseyNumber}</color>");
        }
        orderText.text = "Order: " + string.Join(" ", nums);
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
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.alignment = TextAnchor.UpperLeft;
        var rt = text.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0, 1);
        rt.anchorMax = new Vector2(0, 1);
        rt.pivot = new Vector2(0, 1);
        rt.anchoredPosition = new Vector2(10, -30);

        var menu = canvasObj.AddComponent<ActionMenu>();
        menu.menuText = text;

        playerController.actionMenu = menu;
    }

    public void EndAgentTurn()
    {
        if (CurrentAgent != null)
        {
            playerController.actionMenu.Close();
            playerController.selected = null;
        }

        ball.AdvanceWithVelocity();

        currentAgentIndex++;
        if (currentAgentIndex >= turnOrder.Count)
        {
            StartCycle();
        }
        else
        {
            StartAgentTurn();
        }

        UpdateTurnOrderDisplay();
    }

    public bool IsCellOccupied(Vector2Int cell)
    {
        return GetAgentAtCell(cell) != null;
    }

    public AgentController GetAgentAtCell(Vector2Int cell)
    {
        foreach (var a in allAgents)
        {
            if (a.gridPosition == cell)
                return a;
        }
        return null;
    }

    public void OnGridCellClicked(Vector2Int clickedPosition)
    {
        if (playerController.selected)
        {
            if (playerController.actionMenu.IsPassMode())
                playerController.actionMenu.PassOrder(clickedPosition);
            else
                playerController.actionMenu.MoveOrder(clickedPosition);
        }
    }
}
