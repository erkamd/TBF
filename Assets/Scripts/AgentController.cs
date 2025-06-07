using UnityEngine;

public class AgentController : MonoBehaviour
{
    public AgentStats stats = new AgentStats();
    public Vector2Int gridPosition;
    public bool hasBall;
    public int number;

    private void Start()
    {
    }

    public void Initialize(Vector2Int cell)
    {
        gridPosition = cell;
        transform.position = GridManager.Instance.CellToWorld(cell);
    }

    public void MoveTo(Vector2Int cell)
    {
        if (GameManager.Instance.IsCellOccupied(cell))
            return;

        GameManager.Instance.UpdateAgentCell(this, cell);
        gridPosition = cell;
        transform.position = GridManager.Instance.CellToWorld(cell);
    }
}
