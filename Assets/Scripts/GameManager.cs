using System.Collections;
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

    [Header("AI Settings")]
    public bool enableRedAI = true;
    private RedTeamAI aiController;
    private Coroutine aiRoutine;

    private AgentController savedSelection;
    private bool savedMenuVisible;

    public static GameManager Instance { get; private set; }

    public AgentController CurrentAgent => turnOrder.Count > 0 ? turnOrder[currentAgentIndex] : null;
    public List<AgentController> PlayerAgents => teamA;
    public List<AgentController> AllAgents => allAgents;
    public List<AgentController> AIAgents => teamB;

    public PlayerController playerController;
    public TextMeshProUGUI orderText;

    [Header("Team Spawn Settings")]
    public int agentGap = 1; // Set this in the Inspector
    private readonly List<AgentController> goalkeepers = new();

    private void Awake()
    {
        Instance = this;
        aiController = GetComponent<RedTeamAI>();
        if (aiController == null)
            aiController = gameObject.AddComponent<RedTeamAI>();
    }

    private IEnumerator Start()
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
        var width = GridManager.Instance.width;
        var height = GridManager.Instance.height;

        var topLeft = GridManager.Instance.CellToWorld(new Vector2Int(0, 0));
        var bottomRight = GridManager.Instance.CellToWorld(new Vector2Int(width - 1, height - 1));

        // Calculate the average position
        var centerWorld = (topLeft + bottomRight) * 0.5f;

        var camera = Camera.main;
        if (camera != null)
        {
            camera.transform.position = new Vector3(centerWorld.x, centerWorld.y, centerWorld.z - 10f); // Adjust height and distance as needed
            //camera.transform.LookAt(new Vector3(centerWorld.x, 0f, centerWorld.z));
        }

        yield return null;

        SetupUI();
        StartCycle();

        UpdateTurnOrderDisplay();
    }

    private void SpawnTeams(int gap)
    {
        int numAgents = 3;
        int centerColumn = GridManager.Instance.height / 2;

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
            bc.Initialize(new Vector2Int(GridManager.Instance.width - 4, col));
            teamB.Add(bc);
            allAgents.Add(bc);
        }

        int gkY = (GridManager.Instance.GoalStartY + GridManager.Instance.GoalEndY) / 2;

        var gkaObj = Instantiate(agentPrefab);
        var gka = gkaObj.GetComponent<AgentController>();
        gka.agentColor = Color.blue;
        gka.jerseyNumber = jersey++;
        gka.isGoalkeeper = true;
        gka.Initialize(new Vector2Int(0, gkY));
        gkaObj.AddComponent<GoalkeeperAI>();
        teamA.Add(gka);
        allAgents.Add(gka);

        var gkbObj = Instantiate(agentPrefab);
        var gkb = gkbObj.GetComponent<AgentController>();
        gkb.agentColor = Color.red;
        gkb.jerseyNumber = jersey++;
        gkb.isGoalkeeper = true;
        gkb.Initialize(new Vector2Int(GridManager.Instance.width - 1, gkY));
        gkbObj.AddComponent<GoalkeeperAI>();
        teamB.Add(gkb);
        allAgents.Add(gkb);

        goalkeepers.Add(gka);
        goalkeepers.Add(gkb);

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
        copy.RemoveAll(a => a.isGoalkeeper);
        while (copy.Count > 0)
        {
            int index = 0; // Random.Range(0, copy.Count);
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
        if (enableRedAI && teamB.Contains(CurrentAgent))
        {
            if (aiRoutine != null)
                StopCoroutine(aiRoutine);
            aiRoutine = StartCoroutine(aiController.TakeTurn(CurrentAgent));
        }
        else if (playerController != null)
        {
            // Auto-select the agent that has the turn for the player
            playerController.SelectAgent(CurrentAgent);
        }
    }

    private void UpdateTurnOrderDisplay()
    {
        if (orderText == null) return;
        var nums = new List<string>();

        // Determine if immediate action is open and for which agent
        bool immediateActionActive = playerController.immediateMenu != null && playerController.immediateMenu.IsOpen();
        AgentController immediateAgent = immediateActionActive ? playerController.immediateMenu.agent : null;

        for (int i = 0; i < turnOrder.Count; i++)
        {
            string color = "#FFFFFF"; // Default: white

            if (immediateActionActive && turnOrder[i] == immediateAgent)
                color = "#FF0000"; // Red if immediate action for this agent

            if (i == currentAgentIndex)
            {
                color = "#FFFF00"; // Yellow for current agent
            }

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

        var menu = textObj.AddComponent<ActionMenu>();
        menu.menuText = text;

        playerController.actionMenu = menu;

        var immObj = new GameObject("ImmediateText");
        immObj.transform.SetParent(canvasObj.transform);
        var immText = immObj.AddComponent<UnityEngine.UI.Text>();
        immText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        immText.alignment = TextAnchor.UpperLeft;
        var rt2 = immText.GetComponent<RectTransform>();
        rt2.anchorMin = new Vector2(0, 1);
        rt2.anchorMax = new Vector2(0, 1);
        rt2.pivot = new Vector2(0, 1);
        rt2.anchoredPosition = new Vector2(10, -30);

        var immMenu = immObj.AddComponent<ImmediateActionMenu>();
        immMenu.menuText = immText;
        immMenu.Close();

        playerController.immediateMenu = immMenu;
    }

    public void EndAgentTurn()
    {
        if (CurrentAgent != null)
        {
            playerController.ResetSelection();
        }

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

    public void TriggerImmediateAction(AgentController agent)
    {
        savedSelection = playerController.selected;
        savedMenuVisible = playerController.actionMenu.gameObject.activeSelf;
        playerController.actionMenu.Close();
        playerController.selectionLocked = true;
        playerController.immediateMenu.Open(agent);
        UpdateTurnOrderDisplay();
    }

    public void FinishImmediateAction()
    {
        playerController.immediateMenu.Close();
        playerController.selectionLocked = false;

        if (savedMenuVisible && savedSelection != null)
        {
            playerController.selected = savedSelection;
            playerController.actionMenu.Open(savedSelection);
        }
        savedSelection = null;
        savedMenuVisible = false;
    }
    
    public void GoalScored(int side)
    {
        Debug.Log($"Goal scored on {(side < 0 ? "left" : "right")} side!");
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
        if (playerController.immediateMenu != null && playerController.immediateMenu.IsOpen())
        {
            if (playerController.immediateMenu.IsPassMode())
                playerController.immediateMenu.PassOrder(clickedPosition);
            return;
        }

        if (playerController.selected)
        {
            if (playerController.actionMenu.IsPassMode())
                playerController.actionMenu.PassOrder(clickedPosition);
            else
                playerController.actionMenu.MoveOrder(clickedPosition);
        }
    }
    public void NotifyGoalkeepersOnActionPointSpent()
    {
        foreach (var gk in goalkeepers)
        {
            var ai = gk.GetComponent<GoalkeeperAI>();
            if (ai != null)
                ai.OnActionPointSpent();
        }
    }
}
