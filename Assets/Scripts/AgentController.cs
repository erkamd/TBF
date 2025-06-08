using TMPro;
using UnityEngine;

public class AgentController : MonoBehaviour
{
    public AgentStats stats = new AgentStats();
    public Vector2Int gridPosition;
    public bool hasBall;
    public bool isGoalkeeper = false;
    public int actionPoints;
    public int jerseyNumber;
    public Color agentColor = Color.white; // Default to white

    public TextMeshPro jerseyText;

    private GameObject selectionCube;

    private void Start()
    {
        GameObject agentCube = null;
        if (GetComponent<MeshRenderer>() == null)
        {
            agentCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            agentCube.transform.SetParent(transform, false);

            // Assign color to material
            MeshRenderer meshRenderer = agentCube.GetComponent<MeshRenderer>();
            if (meshRenderer != null)
                meshRenderer.material.color = agentColor;
        }

        // Create selection cube (slightly bigger, yellow, semi-transparent)
        selectionCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        selectionCube.transform.SetParent(transform, false);
        selectionCube.transform.localPosition = new Vector3(0, 0, 1);
        selectionCube.transform.localScale = Vector3.one * 1.3f; // 30% bigger
        var selRenderer = selectionCube.GetComponent<MeshRenderer>();
        if (selRenderer != null)
        {
            selRenderer.material.color = new Color(1f, 1f, 0f, 0.3f); // yellow, semi-transparent
            selRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            selRenderer.receiveShadows = false;
        }
        selectionCube.GetComponent<Collider>().enabled = false;
        selectionCube.SetActive(false);

        jerseyText.text = jerseyNumber.ToString();

        ResetActionPoints();
    }

    public void SetSelected(bool selected)
    {
        if (selectionCube != null)
            selectionCube.SetActive(selected);
    }

    public void Initialize(Vector2Int cell)
    {
        gridPosition = cell;
        transform.position = GridManager.Instance.CellToWorld(cell);
    }

    public void MoveTo(Vector2Int cell)
    {
        gridPosition = cell;
        transform.position = GridManager.Instance.CellToWorld(cell);
        if (hasBall)
        {
            Ball.Instance.MoveTo(cell);
        }
    }

    public void ResetActionPoints()
    {
        actionPoints = stats.speed;
    }

    public bool SpendActionPoints(int amount)
    {
        if (actionPoints < amount)
            return false;
        actionPoints -= amount;
        GameManager.Instance.NotifyGoalkeepersOnActionPointSpent();
        return true;
    }
}
