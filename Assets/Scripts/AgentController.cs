using UnityEngine;

public class AgentController : MonoBehaviour
{
    public AgentStats stats = new AgentStats();
    public Vector2Int gridPosition;
    public bool hasBall;
    public int actionPoints;
    public Color agentColor = Color.white; // Default to white

    private void Start()
    {
        if (GetComponent<MeshRenderer>() == null)
        {
            var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.transform.SetParent(transform, false);
        }
        // Assign color to material
        MeshRenderer meshRenderer = GetComponentInChildren<MeshRenderer>();
        if (meshRenderer != null)
            meshRenderer.material.color = agentColor;

        ResetActionPoints();
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
        if (hasBall && Ball.Instance != null)
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
        return true;
    }
}
